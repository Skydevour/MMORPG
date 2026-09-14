using System;
using MMORPG.Framework.Animation;
using MMORPG.Game.Characters;
using MMORPG.Game.Combat;
using MMORPG.Game.Config;
using MMORPG.Game.Player;
using MMORPG.Game.Audio;
using MMORPG.Game.VFX;
using UnityEngine;

namespace MMORPG.Game.Bosses
{
    public enum BossActionPhase { Idle, Tell, Release, Recover, Dead }
    public abstract class GardenBossActor : CharacterStateDriverBase, IBossActor
    {
        protected PlayerController2D target;
        protected EncounterConfig settings;
        protected bool locked = true;
        protected bool attacking;
        protected float timer;
        private BossFormConfig form;
        private float releaseRemaining;
        public BossActionPhase ActionPhase { get; private set; }
        public Transform Root => transform;
        public string DisplayName => form.displayName;
        public int CurrentHealth { get; private set; }
        public int MaxHealth => form.maxHealth;
        public bool IsDead => CurrentHealth <= 0;
        public event Action<int, int> HealthChanged;
        public event Action Died;

        public void Initialize(BossFormConfig profile, PlayerController2D player, FrameAnimator animator)
        {
            form = profile; target = player; settings = GameConfigService.Current.encounter;
            CurrentHealth = form.maxHealth;
            RegisterAnimationState("idle", animator); RegisterAnimationState("attack", animator); RegisterAnimationState("dead", animator);
            RegisterAnimationState("tell", animator); RegisterAnimationState("release", animator); RegisterAnimationState("recover", animator);
            SetInitialState("idle");
        }
        public void SetBattleLocked(bool value)
        {
            locked = value; attacking = false; timer = settings.safetyDuration;
            ActionPhase = BossActionPhase.Idle; releaseRemaining = 0f;
            if (value) CancelAttack();
        }
        protected override string ResolveStateName()
        {
            if (IsDead) return "dead";
            switch (ActionPhase)
            {
                case BossActionPhase.Tell: return "tell";
                case BossActionPhase.Release: return "release";
                case BossActionPhase.Recover: return "recover";
                default: return "idle";
            }
        }
        protected override void TickState(float deltaTime)
        {
            if (locked || IsDead || deltaTime <= 0f) return;
            if (releaseRemaining > 0f)
            {
                releaseRemaining -= deltaTime;
                if (releaseRemaining <= 0f) ActionPhase = BossActionPhase.Recover;
            }
            TickAttack(deltaTime);
        }
        protected void Present(BossActionPhase phase, float releaseDuration = 0f)
        {
            ActionPhase = phase; releaseRemaining = releaseDuration;
        }
        protected abstract void TickAttack(float deltaTime);
        protected virtual void CancelAttack() { }
        public void TakeDamage(int damage) => ReceiveHit(new HitContext(damage, transform.position + Vector3.up));
        public HitResult ReceiveHit(HitContext hit)
        {
            if (locked || IsDead || hit.Damage <= 0) return HitResult.Ignored;
            CurrentHealth = Mathf.Max(0, CurrentHealth - hit.Damage);
            HealthChanged?.Invoke(CurrentHealth, MaxHealth);
            HitFlashEffect.Play(gameObject, 0.05f, 1, Color.white);
            CombatImpactEffect.SpawnHit(hit.Point, new Color(1f, 0.8f, 0.3f), direction: hit.Direction);
            BattleAudio.Play("boss_hit", hit.Point);
            if (!IsDead) return HitResult.Applied;
            locked = true; CancelAttack();
            GetComponent<Collider2D>().enabled = false;
            BattleAudio.Play("boss_defeat", transform.position);
            Died?.Invoke();
            return HitResult.Killed;
        }
    }
}
