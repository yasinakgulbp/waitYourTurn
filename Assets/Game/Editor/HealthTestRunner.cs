using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace WaitYourTurn.Editor
{
    public static class HealthTestRunner
    {
        [MenuItem("Wait Your Turn/Combat/Run Health Tests")]
        public static void Run() => Execute("WaitYourTurn.Combat.Tests", "HealthTests");

        [MenuItem("Wait Your Turn/Run/Run Flow Tests")]
        public static void RunFlowTests() => Execute("WaitYourTurn.Run.Tests", "RunFlowTests");

        [MenuItem("Wait Your Turn/Weapons/Run Weapon Tests")]
        public static void RunWeaponTests() => Execute("WaitYourTurn.Weapon.Tests", "WeaponTests");

        private static void Execute(string assembly, string reportName)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            var callbacks = new Results(api, reportName);
            TestRunnerApi.RegisterTestCallback(callbacks);
            try
            {
                api.Execute(new ExecutionSettings(new Filter
                { testMode = TestMode.EditMode, assemblyNames = new[] { assembly } })
                { runSynchronously = true });
            }
            catch
            {
                TestRunnerApi.UnregisterTestCallback(callbacks);
                Object.DestroyImmediate(api);
                throw;
            }
        }

        private sealed class Results : ICallbacks
        {
            private readonly TestRunnerApi api;
            private readonly string reportName;
            public Results(TestRunnerApi runner, string name) { api = runner; reportName = name; }
            public void RunStarted(ITestAdaptor test) { }
            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result)
            {
                if (result.FailCount > 0) Debug.LogError($"[{reportName}] " + result.Test.FullName + ": " + result.Message);
            }
            public void RunFinished(ITestResultAdaptor result)
            {
                string path = Path.GetFullPath($"Logs/{reportName}.xml");
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                TestRunnerApi.SaveResultToFile(result, path);
                string message = $"[{reportName}] {result.PassCount} passed, {result.FailCount} failed. {path}";
                if (result.FailCount == 0 && result.PassCount > 0) Debug.Log(message);
                else Debug.LogError(message);
                TestRunnerApi.UnregisterTestCallback(this);
                Object.DestroyImmediate(api);
            }
        }
    }
}
