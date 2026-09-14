using MMORPG.Framework.Pooling;
using MMORPG.Framework.Timing;
using UnityEngine;

namespace MMORPG.Game.VFX
{
    public sealed class ParryFeedback : MonoBehaviour, IPoolable
    {
        private static ComponentObjectPool<ParryFeedback> pool;
        private static Transform root;
        private LineRenderer star, ring;
        private float age;
        public static int Played { get; private set; }
        public static void Spawn(Vector3 point)
        {
            if (root == null)
            {
                root = new GameObject("ParryFeedbackPool").transform;
                pool = new ComponentObjectPool<ParryFeedback>(Create, root, 4);
            }
            pool.Get(point, Quaternion.identity); Played++;
        }
        private static ParryFeedback Create()
        {
            var effect = new GameObject("ParryStarRing").AddComponent<ParryFeedback>();
            effect.star = MakeLine(effect.transform, "Star", 8);
            effect.ring = MakeLine(effect.transform, "Ring", 32);
            return effect;
        }
        private static LineRenderer MakeLine(Transform parent, string name, int count)
        {
            var line = new GameObject(name).AddComponent<LineRenderer>();
            line.transform.SetParent(parent, false); line.useWorldSpace = false; line.loop = true;
            line.positionCount = count; line.widthMultiplier = 0.06f; line.sortingOrder = 95;
            line.sharedMaterial = Resources.Load<MMORPG.Game.Bosses.GardenBossAssets>("Config/GardenBossAssets").lineMaterial;
            return line;
        }
        private void Update()
        {
            if (BattleClock.Paused) return;
            age += Time.unscaledDeltaTime;
            float t = age / 0.3f;
            if (t >= 1f) { pool.Release(this); return; }
            Draw(star, Mathf.Lerp(0.38f, 0.12f, t), true, t);
            Draw(ring, Mathf.Lerp(0.15f, 0.8f, t), false, t);
        }
        private static void Draw(LineRenderer line, float radius, bool pointed, float t)
        {
            for (int i = 0; i < line.positionCount; i++)
            {
                float angle = i * Mathf.PI * 2f / line.positionCount;
                float r = pointed && i % 2 != 0 ? radius * 0.22f : radius;
                line.SetPosition(i, new Vector3(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r));
            }
            Color color = new Color(1f, pointed ? 0.94f : 0.25f, 0.85f, 1f - t);
            line.startColor = line.endColor = color;
        }
        public void OnSpawnedFromPool() { age = 0f; }
        public void OnDespawnedToPool() { }
    }
}
