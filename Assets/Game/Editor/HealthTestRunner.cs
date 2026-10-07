using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace WaitYourTurn.Editor
{
    public static class HealthTestRunner
    {
        [MenuItem("Wait Your Turn/Combat/Run Health Tests")]
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            var callbacks = new Results(api);
            TestRunnerApi.RegisterTestCallback(callbacks);
            try
            {
                api.Execute(new ExecutionSettings(new Filter
                { testMode = TestMode.EditMode, assemblyNames = new[] { "WaitYourTurn.Combat.Tests" } })
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
            public Results(TestRunnerApi runner) { api = runner; }
            public void RunStarted(ITestAdaptor test) { }
            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result)
            {
                if (result.FailCount > 0) Debug.LogError("[HealthTests] " + result.Test.FullName + ": " + result.Message);
            }
            public void RunFinished(ITestResultAdaptor result)
            {
                string path = Path.GetFullPath("Logs/HealthTests.xml");
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                TestRunnerApi.SaveResultToFile(result, path);
                string message = $"[HealthTests] {result.PassCount} passed, {result.FailCount} failed. {path}";
                if (result.FailCount == 0 && result.PassCount > 0) Debug.Log(message);
                else Debug.LogError(message);
                TestRunnerApi.UnregisterTestCallback(this);
                Object.DestroyImmediate(api);
            }
        }
    }
}
