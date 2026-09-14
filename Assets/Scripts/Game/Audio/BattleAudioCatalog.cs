using System;
using UnityEngine;

namespace MMORPG.Game.Audio
{
    public sealed class BattleAudioCatalog : ScriptableObject
    {
        [Serializable] public sealed class Entry
        {
            public string id;
            public AudioClip[] clips;
            public int priority = 50, maxVoices = 3;
            public float gain = 0.6f, minInterval = 0.06f;
        }
        public Entry[] entries;
        public AudioClip[] music;
        public UnityEngine.Audio.AudioMixer mixer;
        public UnityEngine.Audio.AudioMixerGroup[] groups;
    }
}
