using System;
using MMORPG.Game.Combat;
using MMORPG.Game.Config;
using MMORPG.Game.Player;
using MMORPG.Game.Projectiles;
using MMORPG.Game.VFX;
using UnityEngine;

namespace MMORPG.Game.Bosses.Potato
{
    public sealed class PotatoBossController : MonoBehaviour, IDamageable
    {
        private PlayerController2D target;
        private GameConfig config;
        private int currentHealth;
        private float attackTimer;
        private float nextAttackTimer;
        private float attackShotTimer;
        private int attackShotIndex;
        private bool battleLocked;

        public bool IsDead { get; private set; }

        public bool IsAttacking => !IsDead && !battleLocked && attackTimer > 0f;

        public int CurrentHealth => currentHealth;

        public int MaxHealth { get; private set; }

        public int CurrentPhase => 1;

        public event Action<int, int> HealthChanged;

        public event Action Died;

        public void Initialize(PlayerController2D playerTarget)
        {
            target = playerTarget;
            nextAttackTimer = UnityEngine.Random.Range(config.boss.attackCooldownMin * 0.6f, config.boss.attackCooldownMax);
        }

        private void Awake()
        {
            config = GameConfigService.Current;
            MaxHealth = config.boss.maxHealth;
            currentHealth = MaxHealth;
        }

        private void Update()
        {
            if (IsDead || battleLocked)
            {
                return;
            }

            if (attackTimer > 0f)
            {
                attackTimer -= Time.deltaTime;
                TickAttackShots();
                return;
            }

            nextAttackTimer -= Time.deltaTime;
            if (nextAttackTimer <= 0f)
            {
                BeginAttack();
            }
        }

        public void TakeDamage(int damage)
        {
            if (IsDead || battleLocked || damage <= 0)
            {
                return;
            }

            currentHealth = Mathf.Max(0, currentHealth - damage);
            HealthChanged?.Invoke(currentHealth, MaxHealth);
            HitFlashEffect.Play(gameObject, 0.14f, 3, Color.white);
            CombatImpactEffect.SpawnHit(transform.position + Vector3.up * 0.8f, new Color(1f, 0.68f, 0.22f));
            ScreenShakeEffect.Shake(0.07f, 0.045f);
            Debug.Log($"土豆 Boss 受到 {damage} 点伤害，剩余生命 {currentHealth}/{MaxHealth}。");

            if (currentHealth <= 0)
            {
                MarkDead();
            }
        }

        public void MarkDead()
        {
            if (IsDead)
            {
                return;
            }

            IsDead = true;
            attackTimer = 0f;
            attackShotIndex = config.boss.attackShotCount;
            foreach (Collider2D collider in GetComponents<Collider2D>())
            {
                collider.enabled = false;
            }

            CombatImpactEffect.SpawnDeath(transform.position + Vector3.up * 0.75f, new Color(0.95f, 0.62f, 0.2f));
            ScreenShakeEffect.Shake(0.25f, 0.12f);
            Died?.Invoke();
            Debug.Log("土豆 Boss 已被击败，胜利流程已触发。");
        }

        public void SetBattleLocked(bool locked)
        {
            battleLocked = locked;
            if (locked)
            {
                attackTimer = 0f;
            }
        }

        private void TickAttackShots()
        {
            attackShotTimer -= Time.deltaTime;
            while (attackShotIndex < config.boss.attackShotCount && attackShotTimer <= 0f)
            {
                FireAttackShot(attackShotIndex);
                attackShotIndex++;
                attackShotTimer += config.boss.attackShotInterval;
            }
        }

        private void BeginAttack()
        {
            attackTimer = config.boss.attackDuration;
            nextAttackTimer = UnityEngine.Random.Range(config.boss.attackCooldownMin, config.boss.attackCooldownMax);
            attackShotIndex = 0;
            attackShotTimer = 0f;

            FireAttackShot(attackShotIndex);
            attackShotIndex++;
            attackShotTimer = config.boss.attackShotInterval;
        }

        private void FireAttackShot(int shotIndex)
        {
            int direction = -1;
            if (target != null)
            {
                direction = target.transform.position.x < transform.position.x ? -1 : 1;
            }

            BossProjectileLane lane = (BossProjectileLane)(shotIndex % 3);
            float laneOffsetY = GetLaneOffset(lane);
            Vector3 spawnPosition = transform.position + new Vector3(
                direction * config.boss.projectileSpawnOffsetX,
                laneOffsetY,
                -0.1f);

            BossProjectileType type = UnityEngine.Random.value < config.projectile.pinkChance
                ? BossProjectileType.Pink
                : BossProjectileType.Normal;
            float speed = type == BossProjectileType.Pink
                ? config.projectile.pinkSpeed
                : config.projectile.normalSpeed;

            Vector2 horizontalVelocity = new Vector2(direction * speed, 0f);
            BossProjectile.Spawn(spawnPosition, horizontalVelocity, type, lane);
            string projectileName = type == BossProjectileType.Pink ? "紫色可采集子弹" : "普通土块子弹";
            Debug.Log($"土豆 Boss 发动吐射攻击，第 {shotIndex + 1} 发从{GetLaneName(lane)}位置生成{projectileName}，水平速度 {speed:0.0}。");
        }

        private float GetLaneOffset(BossProjectileLane lane)
        {
            switch (lane)
            {
                case BossProjectileLane.Top:
                    return config.boss.projectileSpawnOffsetYTop;
                case BossProjectileLane.Bottom:
                    return config.boss.projectileSpawnOffsetYBottom;
                default:
                    return config.boss.projectileSpawnOffsetYMiddle;
            }
        }

        private static string GetLaneName(BossProjectileLane lane)
        {
            switch (lane)
            {
                case BossProjectileLane.Top:
                    return "上方";
                case BossProjectileLane.Bottom:
                    return "下方";
                default:
                    return "中间";
            }
        }
    }
}