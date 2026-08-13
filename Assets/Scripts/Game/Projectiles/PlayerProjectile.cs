using MMORPG.Framework.Pooling;
using MMORPG.Game.Bosses.Potato;
using MMORPG.Game.Config;
using UnityEngine;

namespace MMORPG.Game.Projectiles
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(CircleCollider2D))]
    public sealed class PlayerProjectile : MonoBehaviour, IPoolable
    {
        private static Sprite cachedSprite;
        private static ComponentObjectPool<PlayerProjectile> pool;
        private static Transform poolRoot;

        private int direction = 1;
        private float lifeTimer;
        private Rigidbody2D body;

        public static PlayerProjectile Spawn(Vector3 position, int facingDirection)
        {
            EnsurePool();
            PlayerProjectile projectile = pool.Get(position, Quaternion.identity);
            projectile.direction = facingDirection >= 0 ? 1 : -1;
            projectile.lifeTimer = 0f;
            projectile.transform.localScale = new Vector3(projectile.direction, 1f, 1f);
            return projectile;
        }

        private static void EnsurePool()
        {
            if (pool != null && poolRoot != null)
            {
                return;
            }

            GameObject rootObject = new GameObject("PlayerProjectilePool");
            poolRoot = rootObject.transform;
            pool = new ComponentObjectPool<PlayerProjectile>(CreateProjectile, poolRoot, 12);
        }

        private static PlayerProjectile CreateProjectile()
        {
            GameObject projectileObject = new GameObject("PlayerBullet");
            SpriteRenderer renderer = projectileObject.AddComponent<SpriteRenderer>();
            renderer.sprite = GetBulletSprite();
            renderer.sortingOrder = 80;

            Rigidbody2D body = projectileObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            CircleCollider2D collider = projectileObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.08f;
            return projectileObject.AddComponent<PlayerProjectile>();
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        private void Update()
        {
            lifeTimer += Time.deltaTime;
            if (lifeTimer >= GameConfigService.Current.projectile.playerBulletLifetime)
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

            float speed = GameConfigService.Current.projectile.playerBulletSpeed;
            body.MovePosition(body.position + Vector2.right * (direction * speed * Time.fixedDeltaTime));
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            BossProjectile bossProjectile = other.GetComponent<BossProjectile>();
            if (bossProjectile != null && bossProjectile.TryCollectByPlayerAttack())
            {
                Despawn();
                return;
            }

            PotatoBossController boss = other.GetComponentInParent<PotatoBossController>();
            if (boss != null && !boss.IsDead)
            {
                boss.TakeDamage(1);
                Despawn();
            }
        }

        public void OnSpawnedFromPool()
        {
            lifeTimer = 0f;
            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
            }
        }

        public void OnDespawnedToPool()
        {
            lifeTimer = 0f;
        }

        private void Despawn()
        {
            if (pool != null && gameObject.activeSelf)
            {
                pool.Release(this);
            }
        }

        private static Sprite GetBulletSprite()
        {
            if (cachedSprite != null)
            {
                return cachedSprite;
            }

            const int width = 32;
            const int height = 16;
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color clear = new Color(0f, 0f, 0f, 0f);
            Color core = new Color(0.55f, 0.95f, 1f, 1f);
            Color outline = new Color(0.05f, 0.08f, 0.12f, 1f);

            Vector2 center = new Vector2((width - 1) * 0.5f, (height - 1) * 0.5f);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector2 normalized = new Vector2((x - center.x) / 14f, (y - center.y) / 6f);
                    float radius = normalized.sqrMagnitude;
                    texture.SetPixel(x, y, radius <= 1f ? (radius > 0.74f ? outline : core) : clear);
                }
            }

            texture.Apply();
            cachedSprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f);
            cachedSprite.name = "RuntimePlayerBullet";
            return cachedSprite;
        }
    }
}