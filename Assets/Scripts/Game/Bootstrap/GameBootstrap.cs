using MMORPG.Game.Camera;
using MMORPG.Game.Lighting;
using MMORPG.Game.Player;
using MMORPG.Game.Player.Motor;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MMORPG.Game.Bootstrap
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject mapPrefab;

        [Header("Editor Fallback Paths")]
        [SerializeField] private string mapPrefabPath = "Assets/Res/Map/Prefabs/DesertMapRoot.prefab";
#if UNITY_EDITOR
        [SerializeField] private string playerPrefabPath = "Assets/Res/Role/\u5927\u5251\u89d2\u8272/MariaRuntime.prefab";
        [SerializeField] private string playerModelPath = "Assets/Res/Role/\u5927\u5251\u89d2\u8272/Maria WProp J J Ong.fbx";
        [SerializeField] private string animatorControllerPath = "Assets/Res/Role/\u5927\u5251\u89d2\u8272/MariaController.controller";
#endif

        [Header("Spawn")]
        [SerializeField] private Vector3 playerSpawnPosition = new(0f, 4f, -18f);
        [SerializeField] private Vector3 playerSpawnEulerAngles = Vector3.zero;

        [Header("Runtime Roots")]
        [SerializeField] private string mapRootName = "RuntimeMap";
        [SerializeField] private string playerRootName = "RuntimePlayer";

        private GameObject spawnedMap;
        private GameObject spawnedPlayer;

        private void Start()
        {
            SpawnMap();
            SpawnPlayer();
            SetupLighting();
        }

        private void SpawnMap()
        {
            if (spawnedMap != null)
            {
                return;
            }

            Object prefab = ResolvePrefab(mapPrefab, mapPrefabPath, nameof(mapPrefab));
            if (prefab == null)
            {
                return;
            }

            spawnedMap = InstantiatePrefab(prefab, Vector3.zero, Quaternion.identity, transform, mapRootName);
            if (spawnedMap == null)
            {
                return;
            }

            spawnedMap.name = mapRootName;
        }

        private void SpawnPlayer()
        {
            UnityEngine.Camera mainCamera = EnsureMainCamera();

            if (spawnedPlayer != null)
            {
                return;
            }

            spawnedPlayer = CreatePlayerRoot();
            if (spawnedPlayer == null)
            {
                return;
            }

            GameObject model = ResolvePlayerModel(spawnedPlayer);
            if (model == null)
            {
                Destroy(spawnedPlayer);
                spawnedPlayer = null;
                return;
            }

            CharacterController characterController = EnsureCharacterController(spawnedPlayer, model);
            CharacterControllerMotor motor = spawnedPlayer.GetComponent<CharacterControllerMotor>() ?? spawnedPlayer.AddComponent<CharacterControllerMotor>();
            PlayerAnimationDriver animationDriver = spawnedPlayer.GetComponent<PlayerAnimationDriver>() ?? spawnedPlayer.AddComponent<PlayerAnimationDriver>();
            PlayerController playerController = spawnedPlayer.GetComponent<PlayerController>() ?? spawnedPlayer.AddComponent<PlayerController>();
            _ = motor;
            _ = animationDriver;

            ThirdPersonCameraFollow cameraFollow = mainCamera.GetComponent<ThirdPersonCameraFollow>() ?? mainCamera.gameObject.AddComponent<ThirdPersonCameraFollow>();
            cameraFollow.SetTarget(spawnedPlayer.transform);
            playerController.Initialize(mainCamera.transform);
            characterController.enabled = true;
        }

        private GameObject CreatePlayerRoot()
        {
            Quaternion spawnRotation = Quaternion.Euler(playerSpawnEulerAngles);
#if UNITY_EDITOR
            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(playerPrefabPath);
            if (playerPrefab != null)
            {
                GameObject prefabRoot = InstantiatePrefab(playerPrefab, playerSpawnPosition, spawnRotation, transform, playerRootName);
                if (prefabRoot == null)
                {
                    return null;
                }

                prefabRoot.name = playerRootName;
                EnsurePlayerAnimator(prefabRoot);
                return prefabRoot;
            }
#endif

            GameObject root = new(playerRootName);
            root.transform.SetParent(transform, false);
            root.transform.SetPositionAndRotation(playerSpawnPosition, spawnRotation);
            GameObject model = CreateFallbackPlayerModel(root.transform);
            if (model == null)
            {
                Destroy(root);
                return null;
            }

            return root;
        }

        private GameObject CreateFallbackPlayerModel(Transform playerRoot)
        {
#if UNITY_EDITOR
            GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(playerModelPath);
            if (modelPrefab == null)
            {
                Debug.LogError($"{nameof(GameBootstrap)} could not load player model at path: {playerModelPath}", this);
                return null;
            }

            Object modelInstance = Instantiate(modelPrefab, playerRoot);
            if (modelInstance is not GameObject model)
            {
                Debug.LogError($"{nameof(GameBootstrap)} loaded player asset, but instantiate result was {modelInstance?.GetType().Name ?? "null"} instead of GameObject.", this);
                if (modelInstance != null)
                {
                    Destroy(modelInstance);
                }

                return null;
            }

            model.name = "Model";
            model.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            EnsurePlayerAnimator(model);
            return model;
#else
            Debug.LogError($"{nameof(GameBootstrap)} runtime player model loading needs an addressable/resource prefab outside the editor.", this);
            return null;
#endif
        }

        private static GameObject ResolvePlayerModel(GameObject playerRoot)
        {
            if (playerRoot == null)
            {
                return null;
            }

            Transform model = playerRoot.transform.Find("Model");
            return model != null ? model.gameObject : playerRoot;
        }

        private void EnsurePlayerAnimator(GameObject playerOrModel)
        {
#if UNITY_EDITOR
            Animator animator = playerOrModel.GetComponentInChildren<Animator>();
            if (animator == null)
            {
                animator = playerOrModel.AddComponent<Animator>();
            }

            if (animator.runtimeAnimatorController == null)
            {
                RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(animatorControllerPath);
                if (controller == null)
                {
                    Debug.LogError($"{nameof(GameBootstrap)} could not load animator controller at path: {animatorControllerPath}", this);
                    return;
                }

                animator.runtimeAnimatorController = controller;
            }

            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.Rebind();
            animator.Update(0f);
#endif
        }

        private Object ResolvePrefab(Object prefab, string assetPath, string fieldName)
        {
            if (prefab != null)
            {
                return prefab;
            }

#if UNITY_EDITOR
            if (!string.IsNullOrWhiteSpace(assetPath))
            {
                Object loadedPrefab = AssetDatabase.LoadMainAssetAtPath(assetPath);
                if (loadedPrefab != null)
                {
                    return loadedPrefab;
                }
            }
#endif

            Debug.LogError($"{nameof(GameBootstrap)} could not resolve {fieldName}. Assign it on GameRoot or check fallback path: {assetPath}", this);
            return null;
        }

        private GameObject InstantiatePrefab(Object prefab, Vector3 position, Quaternion rotation, Transform parent, string label)
        {
            Object instance = Instantiate(prefab, position, rotation, parent);
            if (instance is GameObject gameObject)
            {
                return gameObject;
            }

            Debug.LogError($"{nameof(GameBootstrap)} instantiated {label}, but the result was {instance?.GetType().Name ?? "null"} instead of GameObject. Source asset: {prefab.name}", this);
            if (instance != null)
            {
                Destroy(instance);
            }

            return null;
        }

        private static CharacterController EnsureCharacterController(GameObject player, GameObject model)
        {
            CharacterController characterController = player.GetComponent<CharacterController>();
            if (characterController == null)
            {
                characterController = player.AddComponent<CharacterController>();
            }

            CharacterControllerSetup.FitToModel(characterController, model);
            return characterController;
        }

        private static UnityEngine.Camera EnsureMainCamera()
        {
            UnityEngine.Camera mainCamera = UnityEngine.Camera.main;
            if (mainCamera != null)
            {
                return mainCamera;
            }

            GameObject cameraObject = new("Main Camera");
            cameraObject.tag = "MainCamera";
            mainCamera = cameraObject.AddComponent<UnityEngine.Camera>();
            cameraObject.AddComponent<AudioListener>();
            return mainCamera;
        }

        /// <summary>
        /// 创建运行时光照、反射探针和 Light Probe。早期原型阶段先用代码统一铺光，避免每次重建场景都手动摆灯。
        /// </summary>
        private void SetupLighting()
        {
            Vector3 focusPosition = spawnedPlayer != null ? spawnedPlayer.transform.position : playerSpawnPosition;
            RuntimeLightingRig.Ensure(transform, focusPosition);
        }
    }
}
