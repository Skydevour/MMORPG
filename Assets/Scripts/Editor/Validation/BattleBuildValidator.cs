#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MMORPG.EditorTools.Validation
{
    public static class BattleBuildValidator
    {
        public static void BuildGarden()
        {
            MMORPG.EditorTools.Import.GardenAssetPreparation.Prepare();
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/MainScene.unity" },
                locationPathName = "Builds/DEV-011/BossBattle.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            });
            if (report.summary.result != BuildResult.Succeeded) throw new BuildFailedException("三形态播放器构建失败。");
            Debug.Log("三形态播放器构建成功：Builds/DEV-011/BossBattle.exe。");
        }
        public static void Build()
        {
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/MainScene.unity" },
                locationPathName = "Builds/DEV-006/BossBattle.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException("Boss 关卡播放器构建失败。");
            Debug.Log("Boss 关卡播放器构建成功：Builds/DEV-006/BossBattle.exe。");
        }
    }
}
#endif
