using System.Collections.Generic;
using MMORPG.Framework.Pooling;
using MMORPG.Game.Config;
using MMORPG.Game.Player;
using MMORPG.Game.VFX;
using UnityEngine;
using MMORPG.Game.Combat;

namespace MMORPG.Game.Projectiles
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public sealed class BossLobbedSeed : MonoBehaviour, IPoolable
    {
        private static ComponentObjectPool<BossLobbedSeed> pool;
        private static Transform poolRoot;
        private static Sprite seedSprite;
        private static readonly HashSet<BossLobbedSeed> ActiveSeeds = new HashSet<BossLobbedSeed>();

        private SpriteRenderer spriteRenderer;
        private Rigidbody2D body;
        private CircleCollider2D triggerCollider;
        private Vector2 velocity;
        private float lifeTimer;
        private bool landed;
        private float groundY;

        public static int ActiveCount => ActiveSeeds.Count;

        public static int TotalSpawned { get; private set; }

        public static void ResetCounters()
        {
            TotalSpawned = 0;
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
            ActiveSeeds.RemoveWhere(seed => seed == null || seed.gameObject == null);
            BossLobbedSeed[] active = new BossLobbedSeed[ActiveSeeds.Count];
            ActiveSeeds.CopyTo(active);
            foreach (BossLobbedSeed seed in active)
            {
                if (seed != null && seed.gameObject != null)
                {
                    seed.Despawn();
                }
            }
        }

        public static BossLobbedSeed Spawn(Vector3 position, Vector2 velocity, float targetGroundY)
        {
            EnsurePool();
            BossLobbedSeed seed = pool.Get(position, Quaternion.identity);
            seed.velocity = velocity;
            seed.groundY = targetGroundY;
            seed.lifeTimer = 0f;
            seed.landed = false;
            seed.transform.localScale = Vector3.one;
            seed.ApplyVisuals();
            seed.body.linearVelocity = velocity;
            ActiveSeeds.Add(seed);
            TotalSpawned++;
            return seed;
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            body = GetComponent<Rigidbody2D>();
            triggerCollider = GetComponent<CircleCollider2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 1.4f;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            triggerCollider.isTrigger = true;
        }

        private void Update()
        {
            lifeTimer += Time.deltaTime;
            if (!landed && body.position.y <= groundY + 0.05f)
            {
                Land();
            }

            if (landed && lifeTimer >= GameConfigService.Current.boss.seedObstacleLifetime)
            {
                Despawn();
            }
        }

        public bool TryCollectByPlayerAttack()
        {
            if (!landed || !gameObject.activeSelf)
            {
                return false;
            }

            CombatImpactEffect.SpawnHit(transform.position, new Color(0.6f, 0.4f, 0.2f), false);
            Debug.Log("玩家子弹摧毁了落地种子障碍。");
            Despawn();
            return true;
        }

        private void Land()
        {
            if (landed)
            {
                return;
            }

            landed = true;
            body.linearVelocity = Vector2.zero;
            body.gravityScale = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
            transform.position = new Vector3(transform.position.x, groundY + 0.3f, transform.position.z);
            transform.localScale = new Vector3(0.85f, 1.6f, 1f);
            Debug.Log($"抛物线种子落地成为竖直障碍，位置 X={transform.position.x:0.00}，持续 {GameConfigService.Current.boss.seedObstacleLifetime:0.0} 秒。");
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!landed)
            {
                return;
            }

            PlayerController2D player = other.GetComponentInParent<PlayerController2D>();
            if (player == null)
            {
                return;
            }

            player.ReceiveHit(new HitContext(GameConfigService.Current.projectile.normalDamage, other.ClosestPoint(transform.position)));
        }

        private void OnTriggerStay2D(Collider2D other) => OnTriggerEnter2D(other);

        public void OnSpawnedFromPool()
        {
            lifeTimer = 0f;
            landed = false;
            velocity = Vector2.zero;
            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
                body.gravityScale = 1.4f;
                body.bodyType = RigidbodyType2D.Dynamic;
            }
        }

        public void OnDespawnedToPool()
        {
            lifeTimer = 0f;
            landed = false;
            velocity = Vector2.zero;
            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
                body.gravityScale = 1.4f;
                body.bodyType = RigidbodyType2D.Dynamic;
            }
        }

        private void Despawn()
        {
            ActiveSeeds.Remove(this);
            if (pool != null && gameObject != null && gameObject.activeSelf)
            {
                pool.Release(this);
            }
        }

        private void ApplyVisuals()
        {
            spriteRenderer.sprite = GetSeedSprite();
            triggerCollider.radius = 0.22f;
            spriteRenderer.color = Color.white;
        }

        private static void EnsurePool()
        {
            if (pool != null && poolRoot != null)
            {
                return;
            }

            GameObject rootObject = new GameObject("BossLobbedSeedPool");
            poolRoot = rootObject.transform;
            pool = new ComponentObjectPool<BossLobbedSeed>(CreateSeed, poolRoot, 6);
        }

        private static BossLobbedSeed CreateSeed()
        {
            GameObject seedObject = new GameObject("BossLobbedSeed");
            seedObject.transform.SetParent(poolRoot, false);
            SpriteRenderer renderer = seedObject.AddComponent<SpriteRenderer>();
            renderer.sprite = GetSeedSprite();
            renderer.sortingOrder = 76;
            seedObject.AddComponent<Rigidbody2D>();
            CircleCollider2D collider = seedObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.22f;
            return seedObject.AddComponent<BossLobbedSeed>();
        }

        private static Sprite GetSeedSprite()
        {
            if (seedSprite != null)
            {
                return seedSprite;
            }

            const int width = 32;
            const int height = 32;
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };

            Color clear = new Color(0f, 0f, 0f, 0f);
            Color seedColor = new Color(0.62f, 0.4f, 0.18f, 1f);
            Color edgeColor = new Color(0.24f, 0.13f, 0.06f, 1f);
            Vector2 center = new Vector2((width - 1) * 0.5f, (height - 1) * 0.5f);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = (x - center.x) / (width * 0.24f);
                    float dy = (y - center.y) / (height * 0.32f);
                    float radius = dx * dx + dy * dy;
                    if (radius > 1f)
                    {
                        texture.SetPixel(x, y, clear);
                    }
                    else
                    {
                        texture.SetPixel(x, y, radius > 0.75f ? edgeColor : seedColor);
                    }
                }
            }

            texture.Apply();
            seedSprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f);
            seedSprite.name = "RuntimeBossLobbedSeed";
            return seedSprite;
        }
    }
}
