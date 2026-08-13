using UnityEngine;

namespace MMORPG.Game.VFX
{
    public sealed class CombatImpactEffect : MonoBehaviour
    {
        private const float HitDuration = 0.3f;
        private const float DeathDuration = 0.9f;

        private float timer;
        private float duration;

        public static void SpawnHit(Vector3 position, Color color, bool heavy = false)
        {
            Spawn(position, color, heavy ? 20 : 9, heavy ? 1.9f : 1.15f, HitDuration);
        }

        public static void SpawnDeath(Vector3 position, Color color)
        {
            Spawn(position, color, 34, 2.8f, DeathDuration);
        }

        private static void Spawn(Vector3 position, Color color, int count, float speed, float effectDuration)
        {
            GameObject effectObject = new GameObject("CombatImpactEffect");
            effectObject.transform.position = position;

            ParticleSystem particles = effectObject.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            Configure(particles, effectDuration, count);
            ParticleSystemRenderer renderer = effectObject.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = CreateMaterial();
            renderer.sortingOrder = 92;

            CombatImpactEffect effect = effectObject.AddComponent<CombatImpactEffect>();
            effect.duration = effectDuration;
            particles.Play();

            for (int index = 0; index < count; index++)
            {
                float angle = Random.Range(0f, Mathf.PI * 2f);
                float particleSpeed = Random.Range(speed * 0.55f, speed);
                ParticleSystem.EmitParams emit = new ParticleSystem.EmitParams
                {
                    position = Random.insideUnitCircle * 0.08f,
                    velocity = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * particleSpeed,
                    startLifetime = Random.Range(effectDuration * 0.45f, effectDuration),
                    startSize = Random.Range(0.06f, 0.16f),
                    startColor = Color.Lerp(Color.white, color, Random.Range(0.25f, 0.9f))
                };
                particles.Emit(emit, 1);
            }
        }

        private void Update()
        {
            timer += Time.unscaledDeltaTime;
            if (timer >= duration)
            {
                Destroy(gameObject);
            }
        }

        private static void Configure(ParticleSystem particles, float effectDuration, int count)
        {
            ParticleSystem.MainModule main = particles.main;
            main.duration = effectDuration;
            main.loop = false;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.maxParticles = count;
            main.gravityModifier = 0.08f;

            ParticleSystem.EmissionModule emission = particles.emission;
            emission.enabled = false;

            ParticleSystem.ColorOverLifetimeModule color = particles.colorOverLifetime;
            color.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            color.color = new ParticleSystem.MinMaxGradient(gradient);

            ParticleSystem.SizeOverLifetimeModule size = particles.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 1f),
                new Keyframe(0.25f, 1.25f),
                new Keyframe(1f, 0.1f)));
        }

        private static Material CreateMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                ?? Shader.Find("Particles/Standard Unlit")
                ?? Shader.Find("Sprites/Default");
            return shader == null ? null : new Material(shader) { hideFlags = HideFlags.DontSave };
        }
    }
}