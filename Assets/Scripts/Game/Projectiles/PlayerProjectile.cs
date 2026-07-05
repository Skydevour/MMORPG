using MMORPG.Framework.Pooling;
using UnityEngine;

namespace MMORPG.Game.Projectiles
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PlayerProjectile : MonoBehaviour, IPoolable
    {
        [SerializeField] private float speed = 11f;
        [SerializeField] private float lifetime = 1.4f;

        private static Sprite cachedSprite;
        private static ComponentObjectPool<PlayerProjectile> pool;
        private static Transform poolRoot;

        private int direction = 1;
        private float lifeTimer;

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
            if (pool != null)
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

            PlayerProjectile projectile = projectileObject.AddComponent<PlayerProjectile>();
            CircleCollider2D collider = projectileObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.08f;
            return projectile;
        }

        private void Update()
        {
            transform.position += Vector3.right * (direction * speed * Time.deltaTime);
            lifeTimer += Time.deltaTime;
            if (lifeTimer >= lifetime)
            {
                Despawn();
            }
        }

        public void OnSpawnedFromPool()
        {
            lifeTimer = 0f;
        }

        public void OnDespawnedToPool()
        {
            lifeTimer = 0f;
        }

        private void Despawn()
        {
            if (pool != null)
            {
                pool.Release(this);
            }
            else
            {
                Destroy(gameObject);
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
                    if (radius <= 1f)
                    {
                        texture.SetPixel(x, y, radius > 0.74f ? outline : core);
                    }
                    else
                    {
                        texture.SetPixel(x, y, clear);
                    }
                }
            }

            texture.Apply();
            cachedSprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
            cachedSprite.name = "RuntimePlayerBullet";
            return cachedSprite;
        }
    }
}
