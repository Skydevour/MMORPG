using MMORPG.Game.Config;
using UnityEngine;

namespace MMORPG.Game.VFX
{
    public sealed class SuperAttackEffect : MonoBehaviour
    {
        private float timer;
        private float duration;

        public static void Spawn(Vector3 position, int facingDirection)
        {
            GameObject effectObject = new GameObject("PlayerSuperAttackBurst");
            effectObject.transform.position = position;
            ParticleSystem particleSystem = effectObject.AddComponent<ParticleSystem>();
            particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            float effectDuration = GameConfigService.Current.special.duration;
            Configure(particleSystem, effectDuration);
            ParticleSystemRenderer renderer = effectObject.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = CreateMaterial();
            renderer.sortingOrder = 88;

            SuperAttackEffect effect = effectObject.AddComponent<SuperAttackEffect>();
            effect.duration = effectDuration;
            particleSystem.Play();

            for (int index = 0; index < 30; index++)
            {
                float horizontal = facingDirection * Random.Range(0.5f, 2.2f);
                float vertical = Random.Range(-0.35f, 0.65f);
                ParticleSystem.EmitParams emit = new ParticleSystem.EmitParams
                {
                    position = new Vector3(Random.Range(-0.08f, 0.08f), Random.Range(-0.08f, 0.08f), 0f),
                    velocity = new Vector3(horizontal, vertical, 0f),
                    startLifetime = Random.Range(0.38f, 0.78f),
                    startSize = Random.Range(0.12f, 0.32f),
                    startColor = Color.Lerp(Color.white, new Color(1f, 0.8f, 0.45f), Random.Range(0f, 0.35f))
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
            main.maxParticles = 40;

            ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.enabled = false;

            ParticleSystem.ColorOverLifetimeModule color = particleSystem.colorOverLifetime;
            color.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(1f, 0.75f, 0.36f), 0.62f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.12f), new GradientAlphaKey(0f, 1f) });
            color.color = new ParticleSystem.MinMaxGradient(gradient);
        }

        private static Material CreateMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit") ?? Shader.Find("Particles/Standard Unlit") ?? Shader.Find("Sprites/Default");
            return shader == null ? null : new Material(shader) { hideFlags = HideFlags.DontSave };
        }
    }
}
