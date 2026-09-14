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
    public sealed class BossInsect : MonoBehaviour, IPoolable
    {
        private static ComponentObjectPool<BossInsect> pool;
        private static Transform poolRoot;
        private static Sprite insectSprite;
        private static readonly HashSet<BossInsect> ActiveInsects = new HashSet<BossInsect>();

        private SpriteRenderer spriteRenderer;
        private Rigidbody2D body;
        private CircleCollider2D triggerCollider;
        private Transform target;
        private float lifeTimer;
        private float flapTimer;

        public static int ActiveCount => ActiveInsects.Count;

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
            ActiveInsects.RemoveWhere(insect => insect == null || insect.gameObject == null);
            BossInsect[] active = new BossInsect[ActiveInsects.Count];
            ActiveInsects.CopyTo(active);
            foreach (BossInsect insect in active)
            {
                if (insect != null && insect.gameObject != null)
                {
                    insect.Despawn();
                }
            }
        }

        public static BossInsect Spawn(Vector3 position, Transform playerTarget)
        {
            EnsurePool();
            BossInsect insect = pool.Get(position, Quaternion.identity);
            insect.target = playerTarget;
            insect.lifeTimer = 0f;
            insect.flapTimer = 0f;
            ActiveInsects.Add(insect);
            TotalSpawned++;
            insect.ApplyVisuals();
            Debug.Log($"昆虫从土豆 Boss 身旁飞出，目标玩家，位置 X={position.x:0.00}。");
            return insect;
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            body = GetComponent<Rigidbody2D>();
            triggerCollider = GetComponent<CircleCollider2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            triggerCollider.isTrigger = true;
        }

        private void Update()
        {
            lifeTimer += Time.deltaTime;
            if (lifeTimer >= GameConfigService.Current.boss.insectLifetime)
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

            float speed = GameConfigService.Current.boss.insectSpeed;
            Vector2 direction = target != null
                ? (Vector2)(target.position - transform.position)
                : Vector2.left;
            if (direction.sqrMagnitude > 0.01f)
            {
                direction.Normalize();
            }

            Vector2 velocity = direction * speed;
            if (target != null)
            {
                velocity += Vector2.up * Mathf.Sin(flapTimer * 6f) * 0.9f;
            }

            flapTimer += Time.fixedDeltaTime;
            body.MovePosition(body.position + velocity * Time.fixedDeltaTime);
            transform.localScale = new Vector3(velocity.x >= 0f ? 1f : -1f, 1f, 1f);
        }

        public bool TryDestroyByPlayerAttack()
        {
            if (!gameObject.activeSelf)
            {
                return false;
            }

            CombatImpactEffect.SpawnHit(transform.position, new Color(0.78f, 0.95f, 0.5f), false);
            Debug.Log("玩家子弹消灭了一只昆虫。");
            Despawn();
            return true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            PlayerController2D player = other.GetComponentInParent<PlayerController2D>();
            if (player == null)
            {
                return;
            }

            if (player.ReceiveHit(new HitContext(GameConfigService.Current.projectile.normalDamage,
                other.ClosestPoint(transform.position))).Accepted()) Despawn();
        }

        public void OnSpawnedFromPool()
        {
            lifeTimer = 0f;
            flapTimer = 0f;
            target = null;
        }

        public void OnDespawnedToPool()
        {
            lifeTimer = 0f;
            flapTimer = 0f;
            target = null;
        }

        private void Despawn()
        {
            ActiveInsects.Remove(this);
            if (pool != null && gameObject != null && gameObject.activeSelf)
            {
                pool.Release(this);
            }
        }

        private void ApplyVisuals()
        {
            spriteRenderer.sprite = GetInsectSprite();
            triggerCollider.radius = 0.22f;
            transform.localScale = Vector3.one;
            spriteRenderer.color = Color.white;
        }

        private static void EnsurePool()
        {
            if (pool != null && poolRoot != null)
            {
                return;
            }

            GameObject rootObject = new GameObject("BossInsectPool");
            poolRoot = rootObject.transform;
            pool = new ComponentObjectPool<BossInsect>(CreateInsect, poolRoot, 8);
        }

        private static BossInsect CreateInsect()
        {
            GameObject insectObject = new GameObject("BossInsect");
            insectObject.transform.SetParent(poolRoot, false);
            SpriteRenderer renderer = insectObject.AddComponent<SpriteRenderer>();
            renderer.sprite = GetInsectSprite();
            renderer.sortingOrder = 74;
            insectObject.AddComponent<Rigidbody2D>();
            CircleCollider2D collider = insectObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.22f;
            return insectObject.AddComponent<BossInsect>();
        }

        private static Sprite GetInsectSprite()
        {
            if (insectSprite != null)
            {
                return insectSprite;
            }

            const int width = 40;
            const int height = 36;
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };

            Color clear = new Color(0f, 0f, 0f, 0f);
            Color bodyColor = new Color(0.45f, 0.78f, 0.3f, 1f);
            Color edgeColor = new Color(0.16f, 0.3f, 0.12f, 1f);
            Color wingColor = new Color(0.85f, 0.9f, 0.95f, 0.7f);
            Vector2 center = new Vector2(width * 0.5f, height * 0.5f);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color color = clear;
                    float dx = (x - center.x) / (width * 0.28f);
                    float dy = (y - center.y) / (height * 0.32f);
                    float bodyRadius = dx * dx + dy * dy;
                    if (bodyRadius <= 1f)
                    {
                        color = bodyRadius > 0.72f ? edgeColor : bodyColor;
                    }

                    float leftWing = ((x - (center.x - width * 0.32f)) / (width * 0.26f));
                    float rightWing = ((x - (center.x + width * 0.32f)) / (width * 0.26f));
                    if (y > center.y + height * 0.05f)
                    {
                        float wingY = (y - center.y) / (height * 0.22f);
                        if (leftWing * leftWing + wingY * wingY <= 1f && x < center.x)
                        {
                            color = Color.Lerp(color, wingColor, 0.8f);
                        }

                        if (rightWing * rightWing + wingY * wingY <= 1f && x > center.x)
                        {
                            color = Color.Lerp(color, wingColor, 0.8f);
                        }
                    }

                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            insectSprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f);
            insectSprite.name = "RuntimeBossInsect";
            return insectSprite;
        }
    }
}
