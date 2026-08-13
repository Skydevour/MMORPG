using MMORPG.Game.Config;
using UnityEngine;

namespace MMORPG.Game.VFX
{
    public sealed class EnergyPickupEffect : MonoBehaviour
    {
        private float timer;
        private float duration;

        public static void Spawn(Vector3 position)
        {
            GameObject effectObject = new GameObject("EnergyPickupBurst");
            effectObject.transform.position = position;
            ParticleSystem particleSystem = effectObject.AddComponent<ParticleSystem>();
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            float effectDuration = GameConfigService.Current.projectile.pinkPickupEffectDuration;
            Configure(particleSystem, effectDuration);
            ParticleSystemRenderer renderer = effectObject.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = CreateMaterial();
            renderer.sortingOrder = 86;

            EnergyPickupEffect effect = effectObject.AddComponent<EnergyPickupEffect>();
            effect.duration = effectDuration;
            particleSystem.Play();
            for (int index = 0; index < 10; index++)
            {
                float angle = index * Mathf.PI * 2f / 10f;
                ParticleSystem.EmitParams emit = new ParticleSystem.EmitParams
                {
                    position = Vector3.zero,
                    velocity = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * 1.4f,
                    startLifetime = effectDuration,
                    startSize = 0.12f,
                    startColor = new Color(1f, 0.25f, 0.85f, 1f)
                };
                particleSystem.Emit(emit, 1);
            }
        }

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer >= duration)
            {
                Destroy(gameObject);
            }
        }

        private static void Configure(ParticleSystem particleSystem, float effectDuration)
        {
            ParticleSystem.MainModule main = particleSystem.main;
            main.duration = effectDuration;
            main.loop = false;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.gravityModifier = 0f;

            ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.enabled = false;

            ParticleSystem.ColorOverLifetimeModule color = particleSystem.colorOverLifetime;
            color.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(1f, 0.2f, 0.8f), 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            color.color = new ParticleSystem.MinMaxGradient(gradient);
        }

        private static Material CreateMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
            return shader == null ? null : new Material(shader) { hideFlags = HideFlags.DontSave };
        }
    }
}
