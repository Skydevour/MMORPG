using MMORPG.Framework.Pooling;
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

        private SpriteRenderer spriteRenderer;
        private Rigidbody2D body;
        private CircleCollider2D triggerCollider;
        private Vector2 velocity;
        private float lifeTimer;
        private BossProjectileType projectileType;
        private BossProjectileLane projectileLane;
        private bool collected;

        public static int TotalSpawned { get; private set; }
        public static int TotalNormalSpawned { get; private set; }
        public static int TotalPinkSpawned { get; private set; }
        public static int TotalTopSpawned { get; private set; }
        public static int TotalMiddleSpawned { get; private set; }
        public static int TotalBottomSpawned { get; private set; }
        public static Vector2 LastSpawnedVelocity { get; private set; }

        public BossProjectileType ProjectileType => projectileType;
        public BossProjectileLane ProjectileLane => projectileLane;

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
            projectile.ApplyVisuals();
            return projectile;
        }

        public bool TryCollectByPlayerAttack()
        {
            if (projectileType != BossProjectileType.Pink || collected)
            {
                return false;
            }

            collected = true;
            PlayerEnergyController energy = PlayerEnergyController.Active;
            if (energy != null)
            {
                int amount = GameConfigService.Current.projectile.pinkEnergyValue;
                energy.AddEnergy(amount);
                EnergyPickupEffect.Spawn(transform.position);
                Debug.Log($"玩家攻击命中紫色子弹，获得能量 {amount} 格。当前能量 {energy.CurrentEnergy}/{energy.MaxEnergy}。");
            }
            else
            {
                Debug.LogWarning("紫色子弹被命中，但没有找到玩家能量组件。");
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
            if (lifeTimer >= GameConfigService.Current.projectile.lifetime)
            {
                Despawn();
            }
        }

        private void FixedUpdate()
        {
            if (body == null || !gameObject.activeSelf)
            {
                return;
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

            player.TakeDamage(GameConfigService.Current.projectile.normalDamage);
            Despawn();
        }

        public void OnSpawnedFromPool()
        {
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
            triggerCollider.radius = projectileType == BossProjectileType.Pink ? 0.18f : 0.2f;
            transform.localScale = projectileType == BossProjectileType.Pink ? Vector3.one * 0.9f : Vector3.one;
            spriteRenderer.color = Color.white;
        }

        private void Despawn()
        {
            if (pool != null && gameObject.activeSelf)
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
