using MMORPG.Framework.Pooling;
using MMORPG.Game.Core;
using UnityEngine;

namespace MMORPG.Game.VFX
{
    public sealed class PooledBattleEffect : MonoBehaviour, IPoolable
    {
        private static ComponentObjectPool<PooledBattleEffect> pool;
        private static Transform poolRoot;
        [SerializeField] private ParticleSystem particles;
        private float remaining;
        private static readonly System.Random random = new System.Random();

        public static void Spawn(Vector3 position, Color color, float size, int count, float duration, bool smoke = false, Vector2 drift = default)
        {
            if (poolRoot == null)
            {
                poolRoot = new GameObject("BattleEffectPool").transform;
                pool = new ComponentObjectPool<PooledBattleEffect>(Create, poolRoot, 12);
            }
            var effect = pool.Get(position, Quaternion.identity, poolRoot);
            var assets = Resources.Load<MMORPG.Game.Bosses.GardenBossAssets>("Config/GardenBossAssets");
            effect.particles.GetComponent<ParticleSystemRenderer>().sharedMaterial = smoke || assets == null
                ? Resources.Load<PrototypeSpriteCatalog>("Config/PrototypeSpriteCatalog").particleMaterial : assets.impactMaterial;
            var main = effect.particles.main;
            main.useUnscaledTime = true;
            effect.remaining = duration;
            for (int i = 0; i < count; i++)
            {
                float angle = (float)random.NextDouble() * Mathf.PI * 2f;
                effect.particles.Emit(new ParticleSystem.EmitParams
                {
                    position = position,
                    velocity = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * (smoke ? 1.2f : 2.5f) + (Vector3)drift,
                    startColor = color,
                    startLifetime = duration * (0.6f + (float)random.NextDouble() * 0.4f),
                    startSize = size * (0.7f + (float)random.NextDouble() * 0.6f),
                    rotation = angle * Mathf.Rad2Deg
                }, 1);
            }
            effect.particles.Play();
        }

        private static PooledBattleEffect Create()
        {
            var catalog = Resources.Load<PrototypeSpriteCatalog>("Config/PrototypeSpriteCatalog");
            return catalog.battleParticlePrefab != null
                ? Instantiate(catalog.battleParticlePrefab).GetComponent<PooledBattleEffect>()
                : CreateTemplate();
        }

        public static PooledBattleEffect CreateTemplate()
        {
            GameObject go = new GameObject("BattleParticleBurst");
            go.SetActive(false);
            var effect = go.AddComponent<PooledBattleEffect>();
            effect.particles = go.AddComponent<ParticleSystem>();
            effect.particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = effect.particles.main;
            main.playOnAwake = false;
            main.loop = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 64;
            var emission = effect.particles.emission;
            emission.enabled = false;
            var shape = effect.particles.shape;
            shape.enabled = false;
            var size = effect.particles.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0.1f));
            var color = effect.particles.colorOverLifetime;
            color.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.65f), new GradientAlphaKey(0f, 1f) });
            color.color = gradient;
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = Resources.Load<PrototypeSpriteCatalog>("Config/PrototypeSpriteCatalog").particleMaterial;
            renderer.sortingOrder = 75;
            return effect;
        }

        private void Update()
        {
            if (MMORPG.Framework.Timing.BattleClock.Paused) { particles.Pause(); return; }
            if (particles.isPaused) particles.Play();
            remaining -= Time.unscaledDeltaTime;
            if (remaining <= 0f) pool.Release(this);
        }

        public void OnSpawnedFromPool() => particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        public void OnDespawnedToPool() => particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}
