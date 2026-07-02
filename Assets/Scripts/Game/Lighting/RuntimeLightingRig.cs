using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MMORPG.Game.Lighting
{
    public sealed class RuntimeLightingRig : MonoBehaviour
    {
        private const string RigName = "RuntimeLighting";
        private const string KeyLightName = "Key_Directional_Light";
        private const string ReflectionProbeName = "Battle_Reflection_Probe";
        private const string LightProbeName = "Battle_Light_Probes";

        /// <summary>
        /// 确保运行时基础光照存在。启动脚本会调用这里，让场景只保留一个 GameRoot 也能自动获得探针和主光。
        /// </summary>
        public static RuntimeLightingRig Ensure(Transform parent, Vector3 focusPosition)
        {
            Transform existing = parent != null ? parent.Find(RigName) : null;
            RuntimeLightingRig rig = existing != null ? existing.GetComponent<RuntimeLightingRig>() : null;
            if (rig == null)
            {
                GameObject rigObject = new(RigName);
                if (parent != null)
                {
                    rigObject.transform.SetParent(parent, false);
                }

                rig = rigObject.AddComponent<RuntimeLightingRig>();
            }

            rig.Configure(focusPosition);
            return rig;
        }

        /// <summary>
        /// 创建或刷新光照组件。这里不依赖烘焙流程，方便早期原型阶段频繁重建地图。
        /// </summary>
        private void Configure(Vector3 focusPosition)
        {
            ConfigureAmbient();
            ConfigureKeyLight();
            ConfigureReflectionProbe(focusPosition);
            ConfigureLightProbes(focusPosition);
        }

        private static void ConfigureAmbient()
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.52f, 0.68f, 0.95f);
            RenderSettings.ambientEquatorColor = new Color(0.45f, 0.36f, 0.58f);
            RenderSettings.ambientGroundColor = new Color(0.22f, 0.17f, 0.25f);
            RenderSettings.ambientIntensity = 1.35f;
            RenderSettings.reflectionIntensity = 1.15f;
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;
        }

        private void ConfigureKeyLight()
        {
            Transform lightTransform = transform.Find(KeyLightName);
            if (lightTransform == null)
            {
                lightTransform = new GameObject(KeyLightName).transform;
                lightTransform.SetParent(transform, false);
            }

            Light keyLight = GetOrAddComponent<Light>(lightTransform.gameObject);
            keyLight.type = LightType.Directional;
            keyLight.color = new Color(1f, 0.88f, 0.72f);
            keyLight.intensity = 3.4f;
            keyLight.shadowStrength = 0.72f;
            keyLight.shadows = LightShadows.Soft;
            lightTransform.localRotation = Quaternion.Euler(48f, -34f, 0f);
        }

        private void ConfigureReflectionProbe(Vector3 focusPosition)
        {
            Transform probeTransform = transform.Find(ReflectionProbeName);
            if (probeTransform == null)
            {
                probeTransform = new GameObject(ReflectionProbeName).transform;
                probeTransform.SetParent(transform, false);
            }

            probeTransform.position = focusPosition + Vector3.up * 18f;
            ReflectionProbe probe = GetOrAddComponent<ReflectionProbe>(probeTransform.gameObject);
            probe.mode = ReflectionProbeMode.Realtime;
            probe.refreshMode = ReflectionProbeRefreshMode.OnAwake;
            probe.timeSlicingMode = ReflectionProbeTimeSlicingMode.IndividualFaces;
            probe.intensity = 1.25f;
            probe.boxProjection = true;
            probe.size = new Vector3(320f, 140f, 320f);
            probe.center = Vector3.zero;
        }

        private void ConfigureLightProbes(Vector3 focusPosition)
        {
            Transform probeTransform = transform.Find(LightProbeName);
            if (probeTransform == null)
            {
                probeTransform = new GameObject(LightProbeName).transform;
                probeTransform.SetParent(transform, false);
            }

            probeTransform.position = focusPosition;
            LightProbeGroup probeGroup = GetOrAddComponent<LightProbeGroup>(probeTransform.gameObject);
#if UNITY_EDITOR
            ApplyProbePositions(probeGroup, BuildProbePositions());
#endif
        }

        private static Vector3[] BuildProbePositions()
        {
            List<Vector3> positions = new();
            float[] heights = { 1.5f, 6f, 14f, 28f };
            for (int x = -3; x <= 3; x++)
            {
                for (int z = -3; z <= 3; z++)
                {
                    for (int h = 0; h < heights.Length; h++)
                    {
                        positions.Add(new Vector3(x * 32f, heights[h], z * 32f));
                    }
                }
            }

            return positions.ToArray();
        }

        private static T GetOrAddComponent<T>(GameObject target) where T : Component
        {
            T component = target.GetComponent<T>();
            return component != null ? component : target.AddComponent<T>();
        }

#if UNITY_EDITOR
        private static void ApplyProbePositions(LightProbeGroup probeGroup, Vector3[] positions)
        {
            SerializedObject serializedObject = new(probeGroup);
            SerializedProperty probePositions = serializedObject.FindProperty("m_ProbePositions");
            if (probePositions == null)
            {
                return;
            }

            probePositions.arraySize = positions.Length;
            for (int i = 0; i < positions.Length; i++)
            {
                probePositions.GetArrayElementAtIndex(i).vector3Value = positions[i];
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
#endif
    }
}
