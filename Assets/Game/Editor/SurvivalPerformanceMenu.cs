using UnityEditor;
using UnityEngine;
using WaitYourTurn.Run;
using WaitYourTurn.Sandbox;

namespace WaitYourTurn.Editor
{
    public static class SurvivalPerformanceMenu
    {
        [MenuItem("Wait Your Turn/Survival/Sample PC Baseline %&F6")]
        public static void Baseline() => Sample("baseline");
        [MenuItem("Wait Your Turn/Survival/Sample PC After %&F5")]
        public static void After() => Sample("after");
        private static void Sample(string label)
        {
            if (!EditorApplication.isPlaying || Object.FindAnyObjectByType<SurvivalFrameProbe>() != null) return;
            var run = Object.FindAnyObjectByType<RunDriver>();
            if (run != null && run.UsesOpenTrainSurvival) run.gameObject.AddComponent<SurvivalFrameProbe>().Begin(run, label);
        }
    }
}
