using System;
using System.IO;
using Unity.Profiling;
using UnityEngine;
using WaitYourTurn.Run;

namespace WaitYourTurn.Sandbox
{
    /// <summary>On-demand eight-second Editor sample; never attached to production scenes.</summary>
    public sealed class SurvivalFrameProbe : MonoBehaviour
    {
        private readonly float[] frames = new float[8192];
        private ProfilerRecorder allocations, main;
        private RunDriver run;
        private string label;
        private float start, end;
        private int count, maxEnemies;
        private double total, allocated, mainTime;
        public void Begin(RunDriver owner, string name)
        {
            run = owner; label = name; start = Time.unscaledTime + 1; end = start + 8;
            allocations = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
            main = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread");
        }
        private void LateUpdate()
        {
            if (Time.unscaledTime < start) return;
            if (Time.unscaledTime >= end || count == frames.Length)
            {
                Array.Sort(frames, 0, count);
                string report = DateTime.UtcNow.ToString("O") + "\nEditor PC sample; includes Editor overhead, not Android acceptance.\n" +
                    $"Frames={count}; FPS={(count / Math.Max(.001, total)):F1}; P50ms={frames[Math.Max(0, count / 2)]:F2}; " +
                    $"P95ms={frames[Math.Max(0, (int)(count * .95))]:F2}; Maxms={frames[Math.Max(0, count - 1)]:F2}; MaxEnemies={maxEnemies}\n" +
                    $"GCcounterValid={allocations.Valid}; AverageFrameAllocKB={allocated / Math.Max(1, count) / 1024:F2}; " +
                    $"MainCounterValid={main.Valid}; AverageMainMs={mainTime / Math.Max(1, count) / 1000000:F2}\n";
                Directory.CreateDirectory("Logs"); File.WriteAllText("Logs/SurvivalPerformance-" + label + ".txt", report);
                Debug.Log("[SurvivalFrameProbe] " + report); Destroy(this); return;
            }
            float dt = Time.unscaledDeltaTime; frames[count++] = dt * 1000; total += dt;
            if (allocations.Valid) allocated += allocations.LastValue;
            if (main.Valid) mainTime += main.LastValue;
            int enemies = 0; foreach (var wagon in run.Wagons) enemies += wagon.Enemies.Active.Count;
            maxEnemies = Mathf.Max(maxEnemies, enemies);
        }
        private void OnDestroy() { allocations.Dispose(); main.Dispose(); }
    }
}
