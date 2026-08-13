using UnityEngine;

namespace MMORPG.Game.VFX
{
    public sealed class DodgeSmokeEffect : MonoBehaviour
    {
        private const float EffectDuration = 0.52f;
        private const int ParticleCount = 18;

        private static Texture2D particleTexture;
        private static Material particleMaterial;

        private float lifeTimer;

        public static void Spawn(Vector3 position, int facingDirection)
        {
            GameObject effectObject = new GameObject("DodgeParticleBurst");
            effectObject.transform.position = position;

            ParticleSystem particleSystem = effectObject.AddComponent<ParticleSystem>();
            ConfigureParticleSystem(particleSystem);

            ParticleSystemRenderer renderer = effectObject.GetComponent<ParticleSystemRenderer>();
            Material material = GetParticleMaterial();
            if (material != null)
            {
                renderer.sharedMaterial = material;
            }

            renderer.sortingOrder = 70;

            DodgeSmokeEffect effect = effectObject.AddComponent<DodgeSmokeEffect>();
            effect.EmitBurst(particleSystem, facingDirection);
        }

        private void Update()
        {
            lifeTimer += Time.deltaTime;
            if (lifeTimer >= EffectDuration)
            {
                Destroy(gameObject);
            }
        }

        private void EmitBurst(ParticleSystem particleSystem, int facingDirection)
        {
            particleSystem.Play();
            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
            for (int index = 0; index < ParticleCount; index++)
            {
                float angle = Random.Range(12f, 168f) * Mathf.Deg2Rad;
                float speed = Random.Range(1.6f, 3.9f);
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                emitParams.position = new Vector3(Random.Range(-0.12f, 0.12f), Random.Range(-0.04f, 0.1f), 0f);
                emitParams.velocity = new Vector3(
                    direction.x * speed + facingDirection * Random.Range(0.35f, 0.85f),
                    direction.y * speed,
                    0f);
                emitParams.startLifetime = Random.Range(0.28f, 0.5f);
                emitParams.startSize = Random.Range(0.12f, 0.28f);
                emitParams.startColor = Color.Lerp(
                    new Color(0.96f, 0.94f, 0.87f, 1f),
                    new Color(0.62f, 0.62f, 0.58f, 1f),
                    Random.value);
                particleSystem.Emit(emitParams, 1);
            }
        }

        private static void ConfigureParticleSystem(ParticleSystem particleSystem)
        {
            ParticleSystem.MainModule main = particleSystem.main;
            main.duration = EffectDuration;
            main.loop = false;
            main.playOnAwake = false;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            main.maxParticles = ParticleCount;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.28f, 0.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.28f);
            main.gravityModifier = 0.12f;

            ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.enabled = false;

            ParticleSystem.ShapeModule shape = particleSystem.shape;
            shape.enabled = false;

            ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particleSystem.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient colorGradient = new Gradient();
            colorGradient.SetKeys(
                new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(new Color(0.78f, 0.76f, 0.7f), 0.65f),
                    new GradientColorKey(new Color(0.55f, 0.54f, 0.5f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.82f, 0.2f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(colorGradient);

            ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particleSystem.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve(
                new Keyframe(0f, 0.45f),
                new Keyframe(0.35f, 1.1f),
                new Keyframe(1f, 1.55f));
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);
        }

        private static Material GetParticleMaterial()
        {
            if (particleMaterial != null)
            {
                return particleMaterial;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Particles/Standard Unlit");
            }

            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            if (shader == null)
            {
                return null;
            }

            particleMaterial = new Material(shader)
            {
                hideFlags = HideFlags.DontSave
            };
            particleTexture = CreateParticleTexture();
            if (particleMaterial.HasProperty("_BaseMap"))
            {
                particleMaterial.SetTexture("_BaseMap", particleTexture);
            }

            if (particleMaterial.HasProperty("_MainTex"))
            {
                particleMaterial.SetTexture("_MainTex", particleTexture);
            }

            return particleMaterial;
        }

        private static Texture2D CreateParticleTexture()
        {
            const int textureSize = 32;
            Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };

            float center = (textureSize - 1) * 0.5f;
            for (int y = 0; y < textureSize; y++)
            {
                for (int x = 0; x < textureSize; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center)) / center;
                    float alpha = 1f - Mathf.SmoothStep(0.35f, 1f, distance);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply();
            return texture;
        }
    }
}
