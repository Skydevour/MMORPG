using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using MMORPG.Game.Player;
using MMORPG.Game.Player.Motor;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace MMORPG.Editor.Role
{
    public static class RoleRuntimeAssetBuilder
    {
        private static readonly RoleBuildDefinition[] RoleDefinitions =
        {
            new(
                "Maria Great Sword",
                "Assets/Res/Role/\u5927\u5251\u89d2\u8272",
                "Maria WProp J J Ong.fbx",
                "MariaController.controller",
                "MariaRuntime.prefab",
                "Great Sword"),
            new(
                "Paladin Sword Shield",
                "Assets/Res/Role/\u5251\u76fe\u89d2\u8272",
                "Paladin WProp J Nordstrom.fbx",
                "PaladinController.controller",
                "PaladinRuntime.prefab",
                "Sword And Shield")
        };

        [MenuItem("MMORPG/Role/Rebuild All Runtime Role Assets")]
        public static void RebuildAllRuntimeRoleAssets()
        {
            foreach (RoleBuildDefinition definition in RoleDefinitions)
            {
                RebuildRole(definition);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Runtime role assets rebuilt for Maria and Paladin.");
        }

        private static void RebuildRole(RoleBuildDefinition definition)
        {
            ConfigureAnimationImporters(definition);
            List<RoleAnimationEntry> animations = LoadAnimationEntries(definition);
            AnimatorController controller = RebuildAnimatorController(definition, animations);
            BuildRuntimePrefab(definition, controller, animations);
        }

        private static void ConfigureAnimationImporters(RoleBuildDefinition definition)
        {
            string[] fbxPaths = Directory.GetFiles(definition.FolderPath, "*.fbx", SearchOption.TopDirectoryOnly)
                .Select(ToUnityPath)
                .Where(path => !string.Equals(path, definition.ModelPath, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            foreach (string fbxPath in fbxPaths)
            {
                ModelImporter importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
                if (importer == null)
                {
                    continue;
                }

                ModelImporterClipAnimation[] clips = importer.clipAnimations;
                bool usingDefaultClips = clips == null || clips.Length == 0;
                if (clips == null || clips.Length == 0)
                {
                    clips = importer.defaultClipAnimations;
                }

                string stateName = ToStateName(fbxPath);
                bool shouldLoop = ShouldLoopAnimation(stateName);
                bool changed = usingDefaultClips;
                for (int i = 0; i < clips.Length; i++)
                {
                    string clipName = clips.Length == 1 ? stateName : $"{stateName} {i + 1}";
                    if (!string.Equals(clips[i].name, clipName, StringComparison.Ordinal))
                    {
                        clips[i].name = clipName;
                        changed = true;
                    }

                    if (clips[i].loopTime != shouldLoop)
                    {
                        clips[i].loopTime = shouldLoop;
                        changed = true;
                    }

                    if (clips[i].loopPose != shouldLoop)
                    {
                        clips[i].loopPose = shouldLoop;
                        changed = true;
                    }

                    WrapMode targetWrapMode = shouldLoop ? WrapMode.Loop : WrapMode.Default;
                    if (clips[i].wrapMode != targetWrapMode)
                    {
                        clips[i].wrapMode = targetWrapMode;
                        changed = true;
                    }
                }

                if (!changed)
                {
                    continue;
                }

                importer.clipAnimations = clips;
                importer.SaveAndReimport();
            }
        }

        private static List<RoleAnimationEntry> LoadAnimationEntries(RoleBuildDefinition definition)
        {
            string[] fbxPaths = Directory.GetFiles(definition.FolderPath, "*.fbx", SearchOption.TopDirectoryOnly)
                .Select(ToUnityPath)
                .Where(path => !string.Equals(path, definition.ModelPath, StringComparison.OrdinalIgnoreCase))
                .OrderBy(path => Path.GetFileNameWithoutExtension(path), StringComparer.OrdinalIgnoreCase)
                .ToArray();

            List<RoleAnimationEntry> entries = new();
            foreach (string fbxPath in fbxPaths)
            {
                AnimationClip clip = LoadPrimaryClip(fbxPath);
                if (clip == null)
                {
                    Debug.LogWarning($"Skipped animation without clip: {fbxPath}");
                    continue;
                }

                entries.Add(new RoleAnimationEntry(ToStateName(fbxPath), clip));
            }

            return entries;
        }

        private static AnimatorController RebuildAnimatorController(RoleBuildDefinition definition, IReadOnlyList<RoleAnimationEntry> animations)
        {
            if (File.Exists(definition.ControllerPath))
            {
                AssetDatabase.DeleteAsset(definition.ControllerPath);
            }

            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(definition.ControllerPath);
            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            stateMachine.states = Array.Empty<ChildAnimatorState>();

            RoleAnimationStateNames stateNames = ResolveStateNames(definition, animations);
            AnimatorState defaultState = null;
            for (int i = 0; i < animations.Count; i++)
            {
                RoleAnimationEntry entry = animations[i];
                AnimatorState state = stateMachine.AddState(entry.StateName, new Vector3(260 + i % 3 * 260, 80 + i / 3 * 70, 0f));
                state.motion = entry.Clip;
                state.writeDefaultValues = true;
                state.iKOnFeet = IsLocomotionState(entry.StateName);
                state.speed = 1f;

                if (entry.StateName == stateNames.Idle)
                {
                    defaultState = state;
                }
            }

            stateMachine.defaultState = defaultState ?? (stateMachine.states.Length > 0 ? stateMachine.states[0].state : null);
            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static void BuildRuntimePrefab(
            RoleBuildDefinition definition,
            RuntimeAnimatorController controller,
            IReadOnlyList<RoleAnimationEntry> animations)
        {
            GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(definition.ModelPath);
            if (modelPrefab == null)
            {
                Debug.LogError($"Role model was not found: {definition.ModelPath}");
                return;
            }

            GameObject root = new(definition.RuntimeObjectName);
            GameObject model = null;
            try
            {
                model = PrefabUtility.InstantiatePrefab(modelPrefab) as GameObject;
                if (model == null)
                {
                    model = UnityEngine.Object.Instantiate(modelPrefab);
                }

                model.name = "Model";
                model.transform.SetParent(root.transform, false);
                model.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

                Animator animator = model.GetComponentInChildren<Animator>();
                if (animator == null)
                {
                    animator = model.AddComponent<Animator>();
                }

                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

                CharacterController characterController = root.GetComponent<CharacterController>() ?? root.AddComponent<CharacterController>();
                CharacterControllerSetup.FitToModel(characterController, model);

                PlayerAnimationDriver animationDriver = root.GetComponent<PlayerAnimationDriver>() ?? root.AddComponent<PlayerAnimationDriver>();
                RoleAnimationStateNames stateNames = ResolveStateNames(definition, animations);
                MovementTuning movementTuning = ResolveMovementTuning(stateNames, animations);

                CharacterControllerMotor motor = root.GetComponent<CharacterControllerMotor>() ?? root.AddComponent<CharacterControllerMotor>();
                motor.ConfigureMovement(
                    movementTuning.WalkSpeed,
                    movementTuning.RunSpeed,
                    movementTuning.Acceleration,
                    movementTuning.Deceleration,
                    movementTuning.RotationSpeed);
                motor.ConfigureJump(4.2f, -32f);

                animationDriver.ConfigureStateNames(
                    stateNames.Idle,
                    stateNames.IdleVariants,
                    stateNames.Walk,
                    stateNames.Run,
                    stateNames.Jump,
                    stateNames.Attack,
                    stateNames.Turn);

                _ = root.GetComponent<PlayerController>() ?? root.AddComponent<PlayerController>();

                PrefabUtility.SaveAsPrefabAsset(root, definition.PrefabPath);
                Debug.Log($"Runtime role prefab rebuilt: {definition.PrefabPath}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static RoleAnimationStateNames ResolveStateNames(RoleBuildDefinition definition, IReadOnlyList<RoleAnimationEntry> animations)
        {
            string prefix = definition.StatePrefix;
            string[] stateNames = animations.Select(animation => animation.StateName).ToArray();
            string[] idleVariants = stateNames
                .Where(stateName => ContainsIgnoreCase(stateName, "Idle"))
                .OrderBy(stateName => stateName, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return new RoleAnimationStateNames(
                FindState(stateNames, $"{prefix} Idle") ?? idleVariants.FirstOrDefault() ?? stateNames.FirstOrDefault() ?? string.Empty,
                idleVariants,
                FindState(stateNames, $"{prefix} Walk") ?? FindContainingState(stateNames, "Walk") ?? string.Empty,
                FindState(stateNames, $"{prefix} Run (2)") ?? FindState(stateNames, $"{prefix} Run") ?? FindContainingState(stateNames, "Run") ?? string.Empty,
                FindState(stateNames, $"{prefix} Jump (2)") ?? FindState(stateNames, $"{prefix} Jump") ?? FindContainingState(stateNames, "Jump") ?? string.Empty,
                FindState(stateNames, $"{prefix} Slash") ?? FindState(stateNames, $"{prefix} Attack") ?? FindContainingState(stateNames, "Attack") ?? string.Empty,
                FindState(stateNames, $"{prefix} Turn") ?? FindContainingState(stateNames, "180 Turn") ?? FindContainingState(stateNames, "Turn") ?? string.Empty);
        }

        private static AnimationClip LoadPrimaryClip(string assetPath)
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);
            foreach (UnityEngine.Object asset in assets)
            {
                if (asset is AnimationClip clip && !clip.name.StartsWith("__preview__", StringComparison.Ordinal))
                {
                    return clip;
                }
            }

            return AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
        }

        private static string ToStateName(string assetPath)
        {
            string fileName = Path.GetFileNameWithoutExtension(assetPath);
            TextInfo textInfo = CultureInfo.InvariantCulture.TextInfo;
            return textInfo.ToTitleCase(fileName.ToLowerInvariant());
        }

        private static string ToUnityPath(string path)
        {
            return path.Replace('\\', '/');
        }

        private static string FindState(IEnumerable<string> stateNames, string expected)
        {
            return stateNames.FirstOrDefault(stateName => string.Equals(stateName, expected, StringComparison.OrdinalIgnoreCase));
        }

        private static string FindContainingState(IEnumerable<string> stateNames, string keyword)
        {
            return stateNames.FirstOrDefault(stateName => ContainsIgnoreCase(stateName, keyword));
        }

        private static bool IsLocomotionState(string stateName)
        {
            return ContainsIgnoreCase(stateName, "Idle")
                || ContainsIgnoreCase(stateName, "Walk")
                || ContainsIgnoreCase(stateName, "Run")
                || ContainsIgnoreCase(stateName, "Strafe")
                || ContainsIgnoreCase(stateName, "Turn")
                || ContainsIgnoreCase(stateName, "Block")
                || ContainsIgnoreCase(stateName, "Crouch");
        }

        private static MovementTuning ResolveMovementTuning(RoleAnimationStateNames stateNames, IReadOnlyList<RoleAnimationEntry> animations)
        {
            float walkSpeed = ResolvePlanarClipSpeed(stateNames.Walk, animations, 2.2f, 1.6f, 3.2f);
            float runFallback = Mathf.Max(7.4f, walkSpeed * 3.4f);
            float runSpeed = ResolvePlanarClipSpeed(stateNames.Run, animations, runFallback, 7.2f, 9f);

            if (runSpeed <= walkSpeed + 2f)
            {
                runSpeed = Mathf.Min(9f, Mathf.Max(7.4f, walkSpeed * 3.4f));
            }

            return new MovementTuning(walkSpeed, runSpeed, 34f, 42f, 20f);
        }

        private static float ResolvePlanarClipSpeed(
            string stateName,
            IReadOnlyList<RoleAnimationEntry> animations,
            float fallback,
            float min,
            float max)
        {
            if (string.IsNullOrWhiteSpace(stateName))
            {
                return fallback;
            }

            for (int i = 0; i < animations.Count; i++)
            {
                if (!string.Equals(animations[i].StateName, stateName, StringComparison.OrdinalIgnoreCase) || animations[i].Clip == null)
                {
                    continue;
                }

                Vector3 averageSpeed = animations[i].Clip.averageSpeed;
                float planarSpeed = new Vector2(averageSpeed.x, averageSpeed.z).magnitude;
                if (planarSpeed > 0.2f)
                {
                    return Mathf.Clamp(planarSpeed, min, max);
                }

                break;
            }

            return fallback;
        }

        private static bool ShouldLoopAnimation(string stateName)
        {
            if (ContainsIgnoreCase(stateName, "Attack")
                || ContainsIgnoreCase(stateName, "Slash")
                || ContainsIgnoreCase(stateName, "Jump")
                || ContainsIgnoreCase(stateName, "Impact")
                || ContainsIgnoreCase(stateName, "Death")
                || ContainsIgnoreCase(stateName, "Draw")
                || ContainsIgnoreCase(stateName, "Sheath")
                || ContainsIgnoreCase(stateName, "Kick")
                || ContainsIgnoreCase(stateName, "Casting")
                || ContainsIgnoreCase(stateName, "Cast")
                || ContainsIgnoreCase(stateName, "Power Up"))
            {
                return false;
            }

            return ContainsIgnoreCase(stateName, "Idle")
                || ContainsIgnoreCase(stateName, "Walk")
                || ContainsIgnoreCase(stateName, "Run")
                || ContainsIgnoreCase(stateName, "Strafe")
                || ContainsIgnoreCase(stateName, "Turn")
                || ContainsIgnoreCase(stateName, "Block")
                || ContainsIgnoreCase(stateName, "Blocking")
                || ContainsIgnoreCase(stateName, "Crouch")
                || ContainsIgnoreCase(stateName, "Crouching");
        }

        private static bool ContainsIgnoreCase(string value, string keyword)
        {
            return value != null && keyword != null && value.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private readonly struct RoleBuildDefinition
        {
            public RoleBuildDefinition(string label, string folderPath, string modelFileName, string controllerFileName, string prefabFileName, string statePrefix)
            {
                Label = label;
                FolderPath = folderPath;
                ModelPath = $"{folderPath}/{modelFileName}";
                ControllerPath = $"{folderPath}/{controllerFileName}";
                PrefabPath = $"{folderPath}/{prefabFileName}";
                RuntimeObjectName = Path.GetFileNameWithoutExtension(prefabFileName);
                StatePrefix = statePrefix;
            }

            public string Label { get; }
            public string FolderPath { get; }
            public string ModelPath { get; }
            public string ControllerPath { get; }
            public string PrefabPath { get; }
            public string RuntimeObjectName { get; }
            public string StatePrefix { get; }
        }

        private readonly struct RoleAnimationEntry
        {
            public RoleAnimationEntry(string stateName, AnimationClip clip)
            {
                StateName = stateName;
                Clip = clip;
            }

            public string StateName { get; }
            public AnimationClip Clip { get; }
        }

        private readonly struct RoleAnimationStateNames
        {
            public RoleAnimationStateNames(string idle, string[] idleVariants, string walk, string run, string jump, string attack, string turn)
            {
                Idle = idle;
                IdleVariants = idleVariants;
                Walk = walk;
                Run = run;
                Jump = jump;
                Attack = attack;
                Turn = turn;
            }

            public string Idle { get; }
            public string[] IdleVariants { get; }
            public string Walk { get; }
            public string Run { get; }
            public string Jump { get; }
            public string Attack { get; }
            public string Turn { get; }
        }

        private readonly struct MovementTuning
        {
            public MovementTuning(float walkSpeed, float runSpeed, float acceleration, float deceleration, float rotationSpeed)
            {
                WalkSpeed = walkSpeed;
                RunSpeed = runSpeed;
                Acceleration = acceleration;
                Deceleration = deceleration;
                RotationSpeed = rotationSpeed;
            }

            public float WalkSpeed { get; }
            public float RunSpeed { get; }
            public float Acceleration { get; }
            public float Deceleration { get; }
            public float RotationSpeed { get; }
        }
    }
}
