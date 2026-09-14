using System;
using MMORPG.Framework.Animation;
using MMORPG.Game.Audio;
using MMORPG.Game.Bosses;
using MMORPG.Game.Bosses.Potato;
using MMORPG.Game.Bosses.Onion;
using MMORPG.Game.Bosses.Carrot;
using MMORPG.Game.Combat;
using MMORPG.Game.Config;
using MMORPG.Game.Player;
using MMORPG.Game.Projectiles;
using UnityEngine;

namespace MMORPG.Game.Core
{
    public enum EncounterState { Title, Intro, Fighting, Transition, Victory, Defeat }

    public sealed class EncounterDirector : MonoBehaviour
    {
        private PlayerController2D player;
        private Transform stage;
        private EncounterConfig config;
        private float timer;
        private bool bossDied, swapped;
        private float defeatedHealth;
        private BossEntrancePlayer entrance;
        public IBossActor CurrentBoss { get; private set; }
        public int FormIndex { get; private set; }
        public EncounterState State { get; private set; } = EncounterState.Title;
        public float Progress
        {
            get
            {
                float total = 0f; foreach (var f in config.forms) total += f.maxHealth;
                return Mathf.Clamp01((defeatedHealth + CurrentBoss.MaxHealth - CurrentBoss.CurrentHealth) / total);
            }
        }
        public event Action<IBossActor, int> BossChanged;
        public event Action<bool> Finished;
        public event Action Started;

        public void Initialize(PlayerController2D hero, PotatoBossController potato, Transform parent)
        {
            player = hero; stage = parent; config = GameConfigService.Current.encounter;
            potato.ConfigureEncounter(config.forms[0]);
            Bind(potato); player.SetBattleLocked(true);
        }
        private void Bind(IBossActor actor)
        {
            if (CurrentBoss != null) CurrentBoss.Died -= OnBossDied;
            CurrentBoss = actor; actor.Died += OnBossDied; actor.SetBattleLocked(true);
            var collider = actor.Root.GetComponent<Collider2D>();
            player.SetCombatRightEdge(collider.bounds.min.x - config.bossSeparation);
            BossChanged?.Invoke(actor, FormIndex);
        }
        public void Begin()
        {
            if (State != EncounterState.Title) return;
            State = EncounterState.Intro; timer = config.introDuration;
            Started?.Invoke();
            BattleStats.Begin(); BattleAudio.SetMusic(0); BattleAudio.Play("emerge", CurrentBoss.Root.position);
            BeginEntrance(config.introDuration);
        }
        private void BeginEntrance(float fallbackDuration)
        {
            var assets = Resources.Load<GardenBossAssets>("Config/GardenBossAssets");
            var clip = FormIndex == 0 ? assets.potatoEntrance : FormIndex == 1 ? assets.onionEntrance : assets.carrotEntrance;
            entrance = CurrentBoss.Root.GetComponent<BossEntrancePlayer>();
            if (entrance == null) entrance = CurrentBoss.Root.gameObject.AddComponent<BossEntrancePlayer>();
            entrance.Begin(CurrentBoss.Root.GetComponent<SpriteRenderer>(), clip, fallbackDuration);
        }
        private void OnBossDied() => bossDied = true;
        private void LateUpdate()
        {
            if (State == EncounterState.Title || State == EncounterState.Victory || State == EncounterState.Defeat) return;
            // 同一物理步内统一决策，玩家死亡优先于 Boss 死亡回调。
            if (player.IsDead) { Finish(false); return; }
            if (bossDied)
            {
                bossDied = false;
                if (FormIndex == config.forms.Length - 1) { Finish(true); return; }
                State = EncounterState.Transition; timer = config.transitionDuration; swapped = false;
                player.SetBattleLocked(true); ClearAttacks();
                BattleAudio.Play("boss_defeat", CurrentBoss.Root.position);
            }
            if (State != EncounterState.Intro && State != EncounterState.Transition) return;
            timer -= Time.deltaTime;
            if (State == EncounterState.Transition && !swapped && timer <= config.transitionDuration - config.exitDuration)
            {
                swapped = true;
                var old = CurrentBoss;
                defeatedHealth += old.MaxHealth; FormIndex++;
                Bind(SpawnForm(config.forms[FormIndex]));
                Destroy(old.Root.gameObject);
                var body = player.GetComponent<Rigidbody2D>();
                body.position = new Vector2(body.position.x, GameConfigService.Current.level.stageFloorY);
                BattleStats.Active?.RecordPhaseTransition();
                BattleAudio.SetMusic(FormIndex); BattleAudio.Play("emerge", CurrentBoss.Root.position);
                BeginEntrance(Mathf.Max(0f, config.transitionDuration - config.exitDuration));
                return;
            }
            if (State == EncounterState.Transition && !swapped) return;
            entrance.Tick(Time.deltaTime);
            if (!entrance.IsComplete) return;
            State = EncounterState.Fighting;
            CurrentBoss.SetBattleLocked(false); player.SetBattleLocked(false);
            Debug.Log($"遭遇第 {FormIndex + 1} 形态 {CurrentBoss.DisplayName} 开始。");
        }
        private void Finish(bool won)
        {
            if (entrance != null) entrance.Cancel();
            State = won ? EncounterState.Victory : EncounterState.Defeat;
            CurrentBoss.SetBattleLocked(true); player.SetBattleLocked(true); ClearAttacks();
            BattleStats.Active?.Finish(won); BattleAudio.StopBattle();
            BattleAudio.Play(won ? "ui_victory" : "ui_defeat"); Finished?.Invoke(won);
        }
        public static void ClearAttacks()
        {
            BossProjectile.DespawnAllActive(); BossRootHazard.DespawnAllActive();
            BossInsect.DespawnAllActive(); BossLobbedSeed.DespawnAllActive(); GardenHazard.ClearAll();
            PlayerProjectile.ClearAll();
            MMORPG.Game.VFX.SuperAttackEffect.ClearAll();
        }
        private IBossActor SpawnForm(BossFormConfig form)
        {
            var assets = Resources.Load<GardenBossAssets>("Config/GardenBossAssets");
            if (assets == null) throw new InvalidOperationException("缺少三形态资源目录，请先执行资源准备。");
            var obj = new GameObject(form.id); obj.transform.SetParent(stage);
            obj.transform.position = new Vector3(form.spawnX, GameConfigService.Current.level.stageFloorY - form.embedDepth, 0f);
            var renderer = obj.AddComponent<SpriteRenderer>(); renderer.sortingOrder = 45;
            Sprite[] idle = form.id == "onion" ? assets.onionIdle : assets.carrotIdle;
            Sprite[] attack = form.id == "onion" ? assets.onionAttack : assets.carrotAttack;
            renderer.sprite = idle[0];
            var collider = obj.AddComponent<BoxCollider2D>(); collider.isTrigger = true;
            (form.id == "onion" ? assets.onionGeometry : assets.carrotGeometry).Apply(obj.transform, collider, form);
            var animator = obj.AddComponent<FrameAnimator>();
            animator.SetClipDuration("idle", idle, 0.8f); animator.SetClipDuration("attack", attack, 0.6f);
            int first = Mathf.Max(1, attack.Length / 3);
            int second = Mathf.Max(first + 1, attack.Length * 2 / 3);
            if (attack.Length < 3) throw new InvalidOperationException($"Boss {form.id} 攻击序列至少需要预备、释放、恢复三张真实帧。");
            animator.SetClipDuration("tell", Slice(attack, 0, first), 0.3f, false);
            animator.SetClipDuration("release", Slice(attack, first, second), 0.12f, false);
            animator.SetClipDuration("recover", Slice(attack, second, attack.Length), 0.25f, false);
            animator.SetClipDuration("dead", idle, 0.4f, false);
            GardenBossActor actor = form.id == "onion" ? (GardenBossActor)obj.AddComponent<OnionBossController>() : obj.AddComponent<CarrotBossController>();
            actor.Initialize(form, player, animator); return actor;
        }
        private void OnDestroy() { if (CurrentBoss != null) CurrentBoss.Died -= OnBossDied; }
        private static Sprite[] Slice(Sprite[] source, int start, int end)
        {
            var frames = new Sprite[end - start];
            Array.Copy(source, start, frames, 0, frames.Length);
            return frames;
        }
    }
}
