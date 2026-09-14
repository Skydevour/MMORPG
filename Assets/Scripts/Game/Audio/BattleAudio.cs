using System.Collections.Generic;
using MMORPG.Framework.Timing;
using MMORPG.Game.Config;
using UnityEngine;

namespace MMORPG.Game.Audio
{
    public sealed class BattleAudio : MonoBehaviour
    {
        private sealed class Voice
        {
            public AudioSource source;
            public BattleAudioCatalog.Entry entry;
            public float started;
        }
        private static BattleAudio active;
        private readonly Dictionary<string, BattleAudioCatalog.Entry> entries = new Dictionary<string, BattleAudioCatalog.Entry>();
        private readonly Dictionary<string, float> lastPlayed = new Dictionary<string, float>();
        private Voice[] voices;
        private AudioSource music;
        private AudioSource outgoingMusic;
        private BattleAudioCatalog catalog;
        private bool wasPaused;
        private float duckRemaining;
        private float musicBlend = 1f, outgoingStartVolume;
        public static float Master { get; private set; } = 1f;
        public static float Music { get; private set; } = 0.316f;
        public static float Effects { get; private set; } = 0.5f;
        public static int ActiveVoices
        {
            get { int count = 0; if (active != null && active.voices != null) foreach (var voice in active.voices) if (voice.source.isPlaying) count++; return count; }
        }

        public static void Initialize()
        {
            if (active != null) return;
            active = new GameObject("BattleAudio").AddComponent<BattleAudio>();
        }

        private void Awake()
        {
            active = this;
            var config = GameConfigService.Current.audio;
            Master = PlayerPrefs.GetFloat("Audio.Master", config.master);
            Music = PlayerPrefs.GetFloat("Audio.Music", config.music);
            Effects = PlayerPrefs.GetFloat("Audio.Effects", config.effects);
            catalog = Resources.Load<BattleAudioCatalog>("Config/BattleAudioCatalog");
            if (catalog == null) { Debug.LogError("缺少音频目录 Config/BattleAudioCatalog，请执行资源准备。"); return; }
            foreach (var entry in catalog.entries)
            {
                var runtimeEntry = new BattleAudioCatalog.Entry { id = entry.id, clips = entry.clips, priority = entry.priority, maxVoices = entry.maxVoices, gain = entry.gain, minInterval = entry.minInterval };
                if (config.events != null) foreach (var rule in config.events)
                {
                    if (rule.eventId != entry.id) continue;
                    runtimeEntry.priority = rule.priority; runtimeEntry.maxVoices = Mathf.Clamp(rule.maxVoices, 1, config.maxVoices);
                    runtimeEntry.gain = Mathf.Clamp01(rule.gain); runtimeEntry.minInterval = Mathf.Max(0f, rule.minInterval); break;
                }
                entries[entry.id] = runtimeEntry;
            }
            voices = new Voice[config.maxVoices];
            for (int i = 0; i < voices.Length; i++)
            {
                var source = new GameObject("Voice" + i).AddComponent<AudioSource>();
                source.transform.SetParent(transform);
                source.playOnAwake = false;
                voices[i] = new Voice { source = source };
            }
            music = gameObject.AddComponent<AudioSource>();
            music.loop = true;
            music.playOnAwake = false;
            outgoingMusic = gameObject.AddComponent<AudioSource>(); outgoingMusic.loop = true; outgoingMusic.playOnAwake = false;
            if (catalog.groups != null && catalog.groups.Length > 0) music.outputAudioMixerGroup = outgoingMusic.outputAudioMixerGroup = catalog.groups[0];
        }

        public static void SetVolumes(float master, float musicVolume, float effects)
        {
            Master = Mathf.Clamp01(master); Music = Mathf.Clamp01(musicVolume); Effects = Mathf.Clamp01(effects);
            PlayerPrefs.SetFloat("Audio.Master", Master); PlayerPrefs.SetFloat("Audio.Music", Music); PlayerPrefs.SetFloat("Audio.Effects", Effects);
            PlayerPrefs.Save();
        }

        public static void Play(string id, Vector3 position = default)
        {
            if (active == null || active.voices == null || !active.entries.TryGetValue(id, out var entry)) return;
            bool ui = id.StartsWith("ui_");
            if (BattleClock.Paused && !ui) return;
            if (active.lastPlayed.TryGetValue(id, out float last) && Time.unscaledTime - last < entry.minInterval) return;
            Voice candidate = null;
            int same = 0;
            foreach (var voice in active.voices)
            {
                if (voice.source.isPlaying && voice.entry == entry) same++;
                if (!voice.source.isPlaying && !(active.wasPaused && voice.entry != null)) candidate = voice;
            }
            if (same >= entry.maxVoices) return;
            if (candidate == null)
                foreach (var voice in active.voices)
                    if (voice.entry != null && voice.entry.priority < entry.priority &&
                        (candidate == null || voice.started < candidate.started)) candidate = voice;
            if (candidate == null || entry.clips == null || entry.clips.Length == 0) return;
            candidate.source.Stop();
            candidate.source.loop = id == "super_loop";
            candidate.entry = entry; candidate.started = Time.unscaledTime;
            candidate.source.clip = entry.clips[Random.Range(0, entry.clips.Length)];
            candidate.source.volume = entry.gain * Master * Effects;
            candidate.source.panStereo = Mathf.Clamp(position.x / 20f, -0.35f, 0.35f);
            candidate.source.pitch = id == "shot" ? Random.Range(0.96f, 1.04f) : 1f;
            int group = ui ? 4 : id == "shot" || id == "jump" || id == "double_jump" || id.StartsWith("dash") ? 1 : id == "hurt" || id == "parry_success" || id == "boss_hit" || id == "pickup" ? 3 : 2;
            if (active.catalog.groups != null && active.catalog.groups.Length > group) candidate.source.outputAudioMixerGroup = active.catalog.groups[group];
            candidate.source.Play(); active.lastPlayed[id] = Time.unscaledTime;
            if (id == "hurt" || id == "parry_success") active.duckRemaining = GameConfigService.Current.audio.shotDuckDuration;
        }

        public static void SetMusic(int stage)
        {
            if (active == null || active.catalog == null || active.music == null) return;
            if (stage < 0 || stage >= active.catalog.music.Length) return;
            active.outgoingMusic.Stop();
            var previous = active.music; active.music = active.outgoingMusic; active.outgoingMusic = previous;
            active.outgoingStartVolume = active.outgoingMusic.volume;
            active.musicBlend = 0f;
            active.music.clip = active.catalog.music[stage]; active.music.volume = 0f; active.music.Play();
        }

        public static void StopBattle()
        {
            if (active == null || active.voices == null) return;
            foreach (var voice in active.voices) { voice.source.Stop(); voice.entry = null; }
            active.music.Stop(); active.outgoingMusic.Stop(); active.lastPlayed.Clear();
        }

        public static void StopLoop(string id)
        {
            if (active == null || active.voices == null) return;
            foreach (var voice in active.voices)
                if (voice.entry != null && voice.entry.id == id) { voice.source.Stop(); voice.entry = null; }
        }

        private void Update()
        {
            if (voices == null) return;
            if (!wasPaused)
                foreach (var voice in voices) if (!voice.source.isPlaying) voice.entry = null;
            if (wasPaused != BattleClock.Paused)
            {
                foreach (var voice in voices)
                {
                    if (voice.entry == null || voice.entry.id.StartsWith("ui_")) continue;
                    if (BattleClock.Paused) voice.source.Pause(); else voice.source.UnPause();
                }
                if (BattleClock.Paused) { music.Pause(); outgoingMusic.Pause(); }
                else { music.UnPause(); outgoingMusic.UnPause(); }
                wasPaused = BattleClock.Paused;
            }
            var config = GameConfigService.Current.audio;
            if (!wasPaused) musicBlend = Mathf.Clamp01(musicBlend + Time.unscaledDeltaTime / Mathf.Max(0.01f, config.musicCrossfadeDuration));
            music.volume = Music * Master * musicBlend;
            outgoingMusic.volume = outgoingStartVolume * (1f - musicBlend);
            if (outgoingMusic.volume <= 0f && !wasPaused) outgoingMusic.Stop();
            if (!wasPaused) duckRemaining = Mathf.Max(0f, duckRemaining - Time.unscaledDeltaTime);
            foreach (var voice in voices)
                if (voice.entry != null) voice.source.volume = voice.entry.gain * Master * Effects * (duckRemaining > 0f && voice.entry.id == "shot" ? config.shotDuckGain : 1f);
            if (!wasPaused)
                foreach (var voice in voices)
                    if (!voice.source.isPlaying) voice.entry = null;
        }
        private void OnDestroy() { if (active == this) active = null; }
        private void OnApplicationQuit() => StopBattle();
    }
}
