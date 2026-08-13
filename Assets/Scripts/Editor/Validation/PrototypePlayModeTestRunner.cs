#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace MMORPG.EditorTools.Validation
{
    public static class PrototypePlayModeTestRunner
    {
        private const string TestAssemblyName = "MMORPG.Tests.PlayMode";
        private const string ResultPath = "Plan/playmode_results_2026-08-02.xml";

        public static void Run()
        {
            TestRunnerApi testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            testRunnerApi.RegisterCallbacks(new Callbacks());
            testRunnerApi.Execute(new ExecutionSettings(new Filter
            {
                assemblyNames = new[] { TestAssemblyName },
                testMode = TestMode.PlayMode
            }));
            Debug.Log("已启动原型 PlayMode 测试程序集，等待测试回调完成。");
        }

        private sealed class Callbacks : ICallbacks
        {
            public void RunStarted(ITestAdaptor testsToRun)
            {
                Debug.Log("开始执行原型 PlayMode 测试。");
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                string absoluteResultPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, ResultPath);
                Directory.CreateDirectory(Path.GetDirectoryName(absoluteResultPath));
                TestRunnerApi.SaveResultToFile(result, absoluteResultPath);
                Debug.Log($"原型 PlayMode 测试完成：状态 {result.TestStatus}，通过 {result.PassCount}，失败 {result.FailCount}。\n结果文件：{absoluteResultPath}");
                EditorApplication.Exit(result.TestStatus == TestStatus.Passed ? 0 : 1);
            }

            public void TestStarted(ITestAdaptor test)
            {
            }

            public void TestFinished(ITestResultAdaptor result)
            {
                if (result.TestStatus == TestStatus.Failed)
                {
                    Debug.LogError($"原型 PlayMode 测试失败：{result.FullName}，原因：{result.Message}");
                }
            }
        }
    }
}
#endif