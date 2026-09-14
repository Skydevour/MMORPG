using System.Collections.Generic;
using MMORPG.Framework.Pooling;
using MMORPG.Game.Combat;
using MMORPG.Game.Config;
using MMORPG.Game.Player;
using MMORPG.Game.VFX;
using UnityEngine;

namespace MMORPG.Game.Projectiles
{
    public enum BossProjectileType
    {
        Normal,
        Pink
    }

    public enum BossProjectileLane
    {
        Top,
        Middle,
        Bottom
    }

    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public sealed class BossProjectile : MonoBehaviour, IPoolable
    {
        private static ComponentObjectPool<BossProjectile> pool;
        private static Transform poolRoot;
        private static Sprite normalSprite;
        private static Sprite pinkSprite;
        private static readonly HashSet<BossProjectile> ActiveProjectiles = new HashSet<BossProjectile>();

        private SpriteRenderer spriteRenderer;
        private Rigidbody2D body;
        private CircleCollider2D triggerCollider;
        private Vector2 velocity;
        private float lifeTimer;
        private BossProjectileType projectileType;
        private BossProjectileLane projectileLane;
        private bool collected;
        private Transform seekerTarget;
        private bool seeker, rain;
        public bool IsSeeker => seeker;
        public static float RainDiameter => Mathf.Max(GameConfigService.Current.projectile.normalRadius, GameConfigService.Current.projectile.pinkRadius) * GameConfigService.Current.projectile.rainScaleX * 2f;

        public static int TotalSpawned { get; private set; }
        public static int TotalNormalSpawned { get; private set; }
        public static int TotalPinkSpawned { get; private set; }
        public static int TotalTopSpawned { get; private set; }
        public static int TotalMiddleSpawned { get; private set; }
        public static int TotalBottomSpawned { get; private set; }
        public static Vector2 LastSpawnedVelocity { get; private set; }

        public BossProjectileType ProjectileType => projectileType;
        public BossProjectileLane ProjectileLane => projectileLane;
        public int Generation { get; private set; }
        public Vector2 Velocity => velocity;
        public float Age => lifeTimer;

        public static int ActiveCount => ActiveProjectiles.Count;

        public static void ResetCounters()
        {
            TotalSpawned = 0;
            TotalNormalSpawned = 0;
            TotalPinkSpawned = 0;
            TotalTopSpawned = 0;
            TotalMiddleSpawned = 0;
            TotalBottomSpawned = 0;
            LastSpawnedVelocity = Vector2.zero;
        }

        public static void ResetAll()
        {
            DespawnAllActive();
            ResetCounters();
            if (pool != null && poolRoot != null)
            {
                pool.Clear();
            }
        }

        public static void DespawnAllActive()
        {
            ActiveProjectiles.RemoveWhere(projectile => projectile == null || projectile.gameObject == null);
            BossProjectile[] active = new BossProjectile[ActiveProjectiles.Count];
            ActiveProjectiles.CopyTo(active);
            foreach (BossProjectile projectile in active)
            {
                if (projectile != null && projectile.gameObject != null)
                {
                    projectile.Despawn();
                }
            }
        }

        public static BossProjectile Spawn(Vector3 position, Vector2 velocity, BossProjectileType type, BossProjectileLane lane = BossProjectileLane.Middle)
        {
            EnsurePool();
            BossProjectile projectile = pool.Get(position, Quaternion.identity);
            projectile.projectileType = type;
            projectile.projectileLane = lane;
            projectile.velocity = velocity;
            LastSpawnedVelocity = velocity;
            TotalSpawned++;
            if (type == BossProjectileType.Pink)
            {
                TotalPinkSpawned++;
            }
            else
            {
                TotalNormalSpawned++;
            }

            switch (lane)
            {
                case BossProjectileLane.Top:
                    TotalTopSpawned++;
                    break;
                case BossProjectileLane.Bottom:
                    TotalBottomSpawned++;
                    break;
                default:
                    TotalMiddleSpawned++;
                    break;
            }

            projectile.lifeTimer = 0f;
            projectile.collected = false;
            ActiveProjectiles.Add(projectile);
            projectile.ApplyVisuals();
            return projectile;
        }

        public void SetRain()
        {
            rain = true;
            spriteRenderer.color = projectileType == BossProjectileType.Pink ? Color.white : new Color(0.3f, 0.9f, 1f);
            var config = GameConfigService.Current.projectile;
            transform.localScale = new Vector3(config.rainScaleX, config.rainScaleY, 1f);
        }

        public static BossProjectile SpawnSeeker(Vector3 position, Transform target, bool pink)
        {
            var settings = GameConfigService.Current.encounter;
            var p = Spawn(position, ((Vector2)(target.position + Vector3.up * 0.5f - position)).normalized * settings.seekerSpeed,
                pink ? BossProjectileType.Pink : BossProjectileType.Normal);
            p.seeker = true; p.seekerTarget = target;
            p.spriteRenderer.color = pink ? Color.white : new Color(1f, 0.85f, 0.3f);
            return p;
        }

        public bool TryDestroySeeker()
        {
            if (!seeker || !gameObject.activeInHierarchy || collected) return false;
            collected = true;
            CombatImpactEffect.SpawnHit(transform.position, Color.yellow);
            Despawn(); return true;
        }

        public bool TryCollectByPlayerAttack()
        {
            if (!gameObject.activeInHierarchy || projectileType != BossProjectileType.Pink || collected)
            {
                return false;
            }

            collected = true;
            PlayerEnergyController energy = PlayerEnergyController.Active;
            if (energy != null)
            {
                int amount = GameConfigService.Current.projectile.pinkEnergyValue;
                energy.AddEnergy(amount);
                BattleStats.Active?.RecordPinkCollected();
                EnergyPickupEffect.Spawn(transform.position);
                MMORPG.Game.Audio.BattleAudio.Play("pickup", transform.position);
                Debug.Log($"玩家攻击命中紫色子弹，获得能量 {amount} 格。当前能量 {energy.CurrentEnergy}/{energy.MaxEnergy}。");
            }
            else
            {
                Debug.LogWarning("紫色子弹被命中，但没有找到玩家能量组件。");
            }

            Despawn();
            return true;
        }

        public bool TryCollectByJump()
        {
            if (!gameObject.activeInHierarchy || projectileType != BossProjectileType.Pink || collected)
            {
                return false;
            }

            collected = true;
            PlayerEnergyController energy = PlayerEnergyController.Active;
            if (energy != null)
            {
                int amount = GameConfigService.Current.projectile.pinkEnergyValue;
                energy.AddEnergy(amount);
                BattleStats.Active?.RecordPinkCollected();
                EnergyPickupEffect.Spawn(transform.position);
                CombatImpactEffect.SpawnHit(transform.position, new Color(1f, 0.2f, 0.85f), true);
                ParryFeedback.Spawn(transform.position);
                ScreenShakeEffect.Shake(0.04f, 0.025f);
                Debug.Log($"玩家跳跃击碎粉色子弹，获得能量 {amount} 格，当前能量 {energy.CurrentEnergy}/{energy.MaxEnergy}。");
            }
            else
            {
                Debug.LogWarning("粉色子弹被玩家跳跃击碎，但没有找到玩家能量组件。");
            }

            Despawn();
            return true;
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            body = GetComponent<Rigidbody2D>();
            triggerCollider = GetComponent<CircleCollider2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.gravityScale = 0f;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            triggerCollider.isTrigger = true;
        }

        private void Update()
        {
            lifeTimer += Time.deltaTime;
            if (lifeTimer >= (seeker ? GameConfigService.Current.encounter.seekerLifetime : GameConfigService.Current.projectile.lifetime)
                || (!seeker && GameConfigService.Current.camera.Outside(transform.position, 1f)))
            {
                Despawn();
                return;
            }
            if (rain && transform.position.y <= GameConfigService.Current.level.stageFloorY)
            {
                CombatImpactEffect.SpawnHit(transform.position, new Color(0.3f, 0.9f, 1f));
                MMORPG.Game.Audio.BattleAudio.Play("tear_splash", transform.position);
                Despawn();
            }
        }

        private void FixedUpdate()
        {
            if (body == null || !gameObject.activeSelf)
            {
                return;
            }

            if (seeker && seekerTarget != null && lifeTimer >= GameConfigService.Current.encounter.seekerStraightTime)
            {
                Vector2 aim = (Vector2)(seekerTarget.position + Vector3.up * 0.5f) - body.position;
                float angle = Mathf.MoveTowardsAngle(Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg,
                    Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg, GameConfigService.Current.encounter.seekerTurnRate * Time.fixedDeltaTime) * Mathf.Deg2Rad;
                velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * GameConfigService.Current.encounter.seekerSpeed;
            }
            body.MovePosition(body.position + velocity * Time.fixedDeltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            PlayerController2D player = other.GetComponentInParent<PlayerController2D>();
            if (player == null)
            {
                return;
            }

            if (projectileType == BossProjectileType.Pink && player.TryBreakPinkProjectile(this))
            {
                return;
            }

            if (player.ReceiveHit(new HitContext(GameConfigService.Current.projectile.normalDamage,
                other.ClosestPoint(transform.position), velocity.normalized, Generation)).Accepted()) Despawn();
        }

        public void OnSpawnedFromPool()
        {
            Generation++;
            seeker = rain = false; seekerTarget = null;
            lifeTimer = 0f;
            collected = false;
            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
            }
        }

        public void OnDespawnedToPool()
        {
            lifeTimer = 0f;
            velocity = Vector2.zero;
            collected = false;
            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
            }
        }

        private void ApplyVisuals()
        {
            spriteRenderer.sprite = projectileType == BossProjectileType.Pink ? GetPinkSprite() : GetNormalSprite();
            triggerCollider.radius = projectileType == BossProjectileType.Pink ? GameConfigService.Current.projectile.pinkRadius : GameConfigService.Current.projectile.normalRadius;
            transform.localScale = projectileType == BossProjectileType.Pink ? Vector3.one * 1.1f : Vector3.one;
            spriteRenderer.color = Color.white;
        }

        private void Despawn()
        {
            ActiveProjectiles.Remove(this);
            if (pool != null && gameObject != null && gameObject.activeSelf)
            {
                pool.Release(this);
            }
        }

        private static void EnsurePool()
        {
            if (pool != null && poolRoot != null)
            {
                return;
            }

            GameObject rootObject = new GameObject("BossProjectilePool");
            poolRoot = rootObject.transform;
            pool = new ComponentObjectPool<BossProjectile>(CreateProjectile, poolRoot, 8);
        }

        private static BossProjectile CreateProjectile()
        {
            GameObject projectileObject = new GameObject("BossProjectile");
            projectileObject.transform.SetParent(poolRoot, false);
            SpriteRenderer renderer = projectileObject.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = 78;
            projectileObject.AddComponent<Rigidbody2D>();
            projectileObject.AddComponent<CircleCollider2D>();
            return projectileObject.AddComponent<BossProjectile>();
        }

        private static Sprite GetNormalSprite()
        {
            if (normalSprite == null)
            {
                normalSprite = CreateOrbSprite("RuntimePotatoProjectile", new Color(0.5f, 0.26f, 0.13f), new Color(0.16f, 0.08f, 0.04f));
            }

            return normalSprite;
        }

        private static Sprite GetPinkSprite()
        {
            if (pinkSprite == null)
            {
                pinkSprite = CreateOrbSprite("RuntimePinkProjectile", new Color(1f, 0.28f, 0.82f), new Color(0.22f, 0.03f, 0.24f));
            }

            return pinkSprite;
        }

        private static Sprite CreateOrbSprite(string spriteName, Color fill, Color outline)
        {
            const int textureSize = 32;
            Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };

            Vector2 center = new Vector2((textureSize - 1) * 0.5f, (textureSize - 1) * 0.5f);
            for (int y = 0; y < textureSize; y++)
            {
                for (int x = 0; x < textureSize; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center) / 15f;
                    if (distance > 1f)
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                    else
                    {
                        texture.SetPixel(x, y, distance > 0.78f ? outline : fill);
                    }
                }
            }

            texture.Apply();
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, textureSize, textureSize), new Vector2(0.5f, 0.5f), 64f);
            sprite.name = spriteName;
            return sprite;
        }
    }
}
