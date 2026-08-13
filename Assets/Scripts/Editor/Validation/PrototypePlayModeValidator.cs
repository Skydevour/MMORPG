#if UNITY_EDITOR
using MMORPG.Game.Bosses.Potato;
using MMORPG.Game.Core;
using MMORPG.Game.Player;
using MMORPG.Game.Projectiles;
using MMORPG.Game.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MMORPG.EditorTools.Validation
{
    public static class PrototypePlayModeValidator
    {
        private const string MainScenePath = "Assets/Scenes/MainScene.unity";
        private static double startTime;
        private static bool validationFailed;
        private static bool requestedStop;

        [MenuItem("MMORPG/Validate Prototype Play Flow")]
        public static void ValidateMainSceneFlow()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("关卡流程验证已经在运行，忽略重复请求。");
                return;
            }

            EditorSceneManager.OpenScene(MainScenePath);
            Random.InitState(20260802);
            validationFailed = false;
            requestedStop = false;
            startTime = EditorApplication.timeSinceStartup;
            EditorApplication.update -= TickValidation;
            EditorApplication.update += TickValidation;
            EditorApplication.isPlaying = true;
            Debug.Log("开始验证原型关卡流程：地图、玩家、Boss、两种子弹、三格能量和大招。");
        }

        private static void TickValidation()
        {
            if (!EditorApplication.isPlaying)
            {
                if (requestedStop)
                {
                    requestedStop = false;
                    EditorApplication.update -= TickValidation;
                    Debug.Log(validationFailed ? "原型关卡流程验证失败。" : "原型关卡流程验证通过。");
                    EditorApplication.Exit(validationFailed ? 1 : 0);
                }

                return;
            }

            if (EditorApplication.timeSinceStartup - startTime < 12d)
            {
                return;
            }

            GameManager gameManager = Object.FindFirstObjectByType<GameManager>();
            PlayerController2D player = Object.FindFirstObjectByType<PlayerController2D>();
            PotatoBossController boss = Object.FindFirstObjectByType<PotatoBossController>();
            PlayerEnergyMeter meter = Object.FindFirstObjectByType<PlayerEnergyMeter>();

            if (gameManager == null || player == null || boss == null)
            {
                validationFailed = true;
                Debug.LogError("关卡流程验证失败：没有同时找到 GameManager、玩家和土豆 Boss。");
            }
            else if (player.Energy == null || player.Energy.MaxEnergy != 3 || meter == null)
            {
                validationFailed = true;
                Debug.LogError("关卡流程验证失败：玩家能量组件或三格能量 UI 未正确初始化。");
            }
            else
            {
                if (BossProjectile.TotalNormalSpawned <= 0 || BossProjectile.TotalPinkSpawned <= 0)
                {
                    validationFailed = true;
                    Debug.LogError($"关卡流程验证失败：Boss 子弹生成不完整，普通 {BossProjectile.TotalNormalSpawned}，紫色 {BossProjectile.TotalPinkSpawned}。");
                }

                player.Energy.AddEnergy(player.Energy.MaxEnergy);
                bool superActivated = player.TryActivateSuper();
                if (!superActivated || player.Energy.CurrentEnergy != 0 || !player.IsUsingSuper)
                {
                    validationFailed = true;
                    Debug.LogError("关卡流程验证失败：满能量后无法正常消耗能量并释放大招。");
                }

                Debug.Log($"关卡流程检查数据：Boss 生命 {boss.CurrentHealth}，普通子弹 {BossProjectile.TotalNormalSpawned}，紫色子弹 {BossProjectile.TotalPinkSpawned}，大招状态 {player.IsUsingSuper}。");
            }

            requestedStop = true;
            EditorApplication.isPlaying = false;
        }
    }
}
#endif
