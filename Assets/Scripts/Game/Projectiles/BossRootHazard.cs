using System.Collections.Generic;
using MMORPG.Framework.Pooling;
using MMORPG.Game.Config;
using MMORPG.Game.Player;
using UnityEngine;
using MMORPG.Game.Combat;

namespace MMORPG.Game.Projectiles
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class BossRootHazard : MonoBehaviour, IPoolable
    {
        private static ComponentObjectPool<BossRootHazard> pool;
        private static Transform poolRoot;
        private static Sprite rootSprite;
        private static readonly HashSet<BossRootHazard> ActiveHazards = new HashSet<BossRootHazard>();

        private SpriteRenderer spriteRenderer;
        private Rigidbody2D body;
        private BoxCollider2D triggerCollider;
        private float stateTimer;
        private float lifeTimer;
        private float groundY;
        private bool erupted;

        public static int ActiveCount => ActiveHazards.Count;

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
            ActiveHazards.RemoveWhere(hazard => hazard == null || hazard.gameObject == null);
            BossRootHazard[] active = new BossRootHazard[ActiveHazards.Count];
            ActiveHazards.CopyTo(active);
            foreach (BossRootHazard hazard in active)
            {
                if (hazard != null && hazard.gameObject != null)
                {
                    hazard.Despawn();
                }
            }
        }

        public static BossRootHazard Spawn(Vector3 groundPosition)
        {
            EnsurePool();
            BossConfig boss = GameConfigService.Current.boss;
            BossRootHazard hazard = pool.Get(groundPosition, Quaternion.identity);
            hazard.groundY = groundPosition.y;
            hazard.stateTimer = boss.rootTellDuration;
            hazard.lifeTimer = 0f;
            hazard.erupted = false;
            ActiveHazards.Add(hazard);
            TotalSpawned++;
            hazard.ShowTell();
            return hazard;
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            body = GetComponent<Rigidbody2D>();
            triggerCollider = GetComponent<BoxCollider2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            triggerCollider.isTrigger = true;
        }

        private void Update()
        {
            BossConfig boss = GameConfigService.Current.boss;

            if (!erupted)
            {
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0f)
                {
                    Erupt();
                }

                return;
            }

            lifeTimer += Time.deltaTime;
            if (lifeTimer >= boss.rootHazardLifetime)
            {
                Despawn();
            }
        }

        public void OnSpawnedFromPool()
        {
            stateTimer = 0f;
            lifeTimer = 0f;
            erupted = false;
        }

        public void OnDespawnedToPool()
        {
            stateTimer = 0f;
            lifeTimer = 0f;
            erupted = false;
        }

        private void ShowTell()
        {
            transform.localScale = new Vector3(1f, 0.25f, 1f);
            spriteRenderer.sprite = GetRootSprite();
            spriteRenderer.color = new Color(0.72f, 0.55f, 0.3f, 0.9f);
        }

        private void Erupt()
        {
            erupted = true;
            transform.localScale = Vector3.one;
            spriteRenderer.color = Color.white;
            Debug.Log($"根须危险物在玩家附近破土，位置 X={transform.position.x:0.00}，Y={transform.position.y:0.00}。");

            Collider2D[] overlaps = Physics2D.OverlapBoxAll(
                triggerCollider.bounds.center,
                triggerCollider.bounds.size,
                0f);
            foreach (Collider2D other in overlaps)
            {
                PlayerController2D player = other.GetComponentInParent<PlayerController2D>();
                if (player == null)
                {
                    continue;
                }

                player.ReceiveHit(new HitContext(GameConfigService.Current.projectile.normalDamage, other.ClosestPoint(transform.position)));
                break;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!erupted)
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

        private void Despawn()
        {
            ActiveHazards.Remove(this);
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

            GameObject rootObject = new GameObject("BossRootHazardPool");
            poolRoot = rootObject.transform;
            pool = new ComponentObjectPool<BossRootHazard>(CreateHazard, poolRoot, 6);
        }

        private static BossRootHazard CreateHazard()
        {
            GameObject hazardObject = new GameObject("BossRootHazard");
            hazardObject.transform.SetParent(poolRoot, false);
            SpriteRenderer renderer = hazardObject.AddComponent<SpriteRenderer>();
            renderer.sprite = GetRootSprite();
            renderer.sortingOrder = 72;
            hazardObject.AddComponent<Rigidbody2D>();
            BoxCollider2D collider = hazardObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(0.5f, 0.9f);
            collider.offset = new Vector2(0f, 0.45f);
            return hazardObject.AddComponent<BossRootHazard>();
        }

        private static Sprite GetRootSprite()
        {
            if (rootSprite != null)
            {
                return rootSprite;
            }

            const int width = 48;
            const int height = 64;
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };

            Color clear = new Color(0f, 0f, 0f, 0f);
            Color soil = new Color(0.5f, 0.26f, 0.13f, 1f);
            Color rootCore = new Color(0.36f, 0.52f, 0.2f, 1f);
            Color rootEdge = new Color(0.18f, 0.3f, 0.1f, 1f);
            Vector2 center = new Vector2(width * 0.5f, height * 0.2f);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float radial = Mathf.Abs(x - center.x) / (center.x * 0.62f);
                    float heightRatio = y / (float)height;
                    if (heightRatio < 0.22f)
                    {
                        texture.SetPixel(x, y, soil);
                    }
                    else if (radial <= 1f && y > 0)
                    {
                        texture.SetPixel(x, y, radial > 0.72f ? rootEdge : rootCore);
                    }
                    else
                    {
                        texture.SetPixel(x, y, clear);
                    }
                }
            }

            texture.Apply();
            rootSprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0f), 64f);
            rootSprite.name = "RuntimeBossRootHazard";
            return rootSprite;
        }
    }
}
