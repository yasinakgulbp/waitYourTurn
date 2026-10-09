#if UNITY_ANDROID && DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;
using WaitYourTurn.Run;

namespace WaitYourTurn.Sandbox
{
    /// <summary>Development APK only. Local ten-second samples; opt-in bounded synthetic combat load.</summary>
    [DefaultExecutionOrder(2500)]
    public sealed class AndroidPlaytestDiagnostics : MonoBehaviour
    {
        private RunDriver run;
        private RunPersistence persistence;
        private RunEconomy economy;
        private readonly float[] frames = new float[12000];
        private int count, peak, sequence;
        private double sum, windowStart, fixtureStart;
        private float nextSpawn, nextDefense;
        private bool fixture, paused, restoring;
        private string originalStore, display = "Collecting frames...";
        private StreamWriter log;
        private GUIStyle style;
        private AndroidJavaObject activity, battery, power;
        private bool sensorsUnavailable;
        private int previousSleep;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            var owner = FindAnyObjectByType<RunDriver>();
            if (owner != null && owner.gameObject.scene.name == "TrainIntegration")
                owner.gameObject.AddComponent<AndroidPlaytestDiagnostics>();
        }
        private IEnumerator Start()
        {
            run = GetComponent<RunDriver>();
            persistence = FindAnyObjectByType<RunPersistence>(); economy = FindAnyObjectByType<RunEconomy>();
            while (run.Flow == null || persistence.Busy) yield return null;
            string file = Path.Combine(Application.persistentDataPath, "perf-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss") + ".csv");
            log = new StreamWriter(file, false);
            log.WriteLine("# Development build; wired readings cannot measure app battery drain. Fixture = synthetic invulnerable defenders, normal station loop, extra bounded spawns, normal weapon/door/AI components.");
            log.WriteLine("utc,fixture,elapsed_s,phase,station,target,frames,fps,p95_ms,p99_ms,max_ms,over_33ms,over_50ms,peak_enemies,battery_C,thermal,plugged,battery_percent,unity_alloc_MB,managed_MB,gc0,width,height");
            ResetWindow(); Debug.Log("[AndroidPerf] " + file);
            try
            {
                using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                battery = activity.Call<AndroidJavaObject>("getSystemService", "batterymanager");
                power = activity.Call<AndroidJavaObject>("getSystemService", "power");
            }
            catch (Exception e) { sensorsUnavailable = true; Debug.LogWarning("[AndroidPerf] Sensors unavailable: " + e.Message); }
        }
        private void ResetWindow() { count = peak = 0; sum = 0; windowStart = Time.realtimeSinceStartupAsDouble; }
        private void Update()
        {
            if (log == null || paused || restoring) return;
            double now = Time.realtimeSinceStartupAsDouble;
            if (count < frames.Length) { frames[count++] = Time.unscaledDeltaTime * 1000; sum += Time.unscaledDeltaTime; }
            int alive = 0; foreach (var wagon in run.Wagons) alive += wagon.Enemies.Active.Count;
            peak = Mathf.Max(peak, alive);
            if (fixture)
            {
                run.Player.Invulnerable = true;
                foreach (var bot in run.Match.Bots) bot.Health.Invulnerable = true;
                if (run.Flow.Terminal || now - fixtureStart >= 600) { StartCoroutine(StopFixture()); return; }
                if (run.Flow.Phase == RunPhase.Defense && Time.unscaledTime >= nextSpawn)
                {
                    nextSpawn = Time.unscaledTime + .25f;
                    foreach (var wagon in run.Wagons)
                        if (alive < 60 && wagon.HasLivingDefender && wagon.Enemies.Active.Count < wagon.Enemies.Capacity &&
                            wagon.SpawnOutside(sequence++)) alive++;
                }
                if (!run.Flow.Paused && Time.unscaledTime >= nextDefense)
                {
                    nextDefense = Time.unscaledTime + 5;
                    economy.Defenses.TryBuy(sequence % 2); economy.Drone.TryBuy();
                }
            }
            if (now - windowStart >= 10) Sample(now);
        }
        private void Sample(double now)
        {
            if (count == 0) { ResetWindow(); return; }
            Array.Sort(frames, 0, count);
            int slow33 = 0, slow50 = 0;
            for (int i = 0; i < count; i++) { if (frames[i] > 33.34f) slow33++; if (frames[i] > 50) slow50++; }
            float temp = -1; int thermal = -1, plugged = -1, level = -1;
            if (!sensorsUnavailable)
            {
                try
                {
                    using var filter = new AndroidJavaObject("android.content.IntentFilter", "android.intent.action.BATTERY_CHANGED");
                    using var status = activity.Call<AndroidJavaObject>("registerReceiver", null, filter);
                    temp = status.Call<int>("getIntExtra", "temperature", -10) / 10f;
                    plugged = status.Call<int>("getIntExtra", "plugged", -1);
                    level = battery.Call<int>("getIntProperty", 4);
                    using var version = new AndroidJavaClass("android.os.Build$VERSION");
                    if (version.GetStatic<int>("SDK_INT") >= 29) thermal = power.Call<int>("getCurrentThermalStatus");
                }
                catch (Exception e) { sensorsUnavailable = true; Debug.LogWarning("[AndroidPerf] Sensor read failed: " + e.Message); }
            }
            double fps = count / sum;
            float p95 = frames[Mathf.Clamp(Mathf.CeilToInt(count * .95f) - 1, 0, count - 1)];
            float p99 = frames[Mathf.Clamp(Mathf.CeilToInt(count * .99f) - 1, 0, count - 1)];
            string row = string.Join(",", DateTime.UtcNow.ToString("O"), fixture ? "1" : "0",
                F(fixture ? now - fixtureStart : now), run.Flow.Phase, run.Flow.Station, MobileFramePolicy.Target, count,
                F(fps), F(p95), F(p99), F(frames[count - 1]), slow33, slow50, peak, F(temp), thermal, plugged, level,
                F(Profiler.GetTotalAllocatedMemoryLong() / 1048576d), F(GC.GetTotalMemory(false) / 1048576d), GC.CollectionCount(0), Screen.width, Screen.height);
            log.WriteLine(row); log.Flush();
            display = $"{fps:F1} FPS | p95 {p95:F1}ms\nBattery {temp:F1}C | Thermal {thermal}\nPeak enemies {peak} | Target {MobileFramePolicy.Target}";
            Debug.Log("[AndroidPerf] " + row); ResetWindow();
        }
        private static string F(double value) => value.ToString("F2", CultureInfo.InvariantCulture);
        private void ChangeTarget(int target)
        {
            // Do not label frames collected under the previous cap with the newly selected target.
            Sample(Time.realtimeSinceStartupAsDouble); MobileFramePolicy.SetTarget(target);
        }
        private void StartFixture()
        {
            if (fixture || restoring || persistence.Busy || log == null) return;
            persistence.SaveNow(); originalStore = persistence.SavePath;
            // Redirect BEFORE restart invalidates a save. Never overwrite the user's active run.
            persistence.UseTestStore(Path.Combine(Application.persistentDataPath, "perf-fixture-run.json"));
            run.Match.SetMode(RunMode.Battle);
            run.Player.Invulnerable = true; foreach (var bot in run.Match.Bots) bot.Health.Invulnerable = true;
            fixture = true; fixtureStart = Time.realtimeSinceStartupAsDouble;
            previousSleep = Screen.sleepTimeout; Screen.sleepTimeout = SleepTimeout.NeverSleep;
            nextSpawn = nextDefense = 0; sequence = 0;
            ResetWindow(); log.WriteLine("# fixture_start " + DateTime.UtcNow.ToString("O")); log.Flush();
        }
        private IEnumerator StopFixture()
        {
            Sample(Time.realtimeSinceStartupAsDouble);
            fixture = false; restoring = true;
            run.Player.Invulnerable = false; foreach (var bot in run.Match.Bots) bot.Health.Invulnerable = false;
            Screen.sleepTimeout = previousSleep;
            log.WriteLine("# fixture_stop " + DateTime.UtcNow.ToString("O") + " " + run.Flow.Phase); log.Flush();
            // Restart still targets the disposable store; otherwise OnRestart would retire the saved user run.
            run.Restart();
            // Restart can reopen a dark-phase gate and restore its cached invulnerability flags.
            run.Player.Invulnerable = false; foreach (var bot in run.Match.Bots) bot.Health.Invulnerable = false;
            persistence.UseTestStore(originalStore); yield return persistence.LoadNow();
            restoring = false; ResetWindow();
        }
        private void OnApplicationPause(bool value)
        {
            paused = value;
            if (log != null) { log.WriteLine("# pause=" + value + " " + DateTime.UtcNow.ToString("O")); log.Flush(); }
            // Background time is not counted as active combat testing.
            if (fixture && value) StartCoroutine(StopFixture());
            ResetWindow();
        }
        private void OnGUI()
        {
            if (run == null || log == null) return;
            var safe = Screen.safeArea;
            float scale = Mathf.Min(safe.width / 960, safe.height / 540);
            Matrix4x4 old = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(new Vector3(safe.x, Screen.height - safe.yMax), Quaternion.identity, new Vector3(scale, scale, 1));
            float x = safe.width / scale - 234;
            style ??= new GUIStyle(GUI.skin.label) { fontSize = 12 };
            GUI.Box(new Rect(x, 118, 224, 106), "");
            GUI.Label(new Rect(x + 8, 120, 214, 57), display, style);
            GUI.enabled = !restoring;
            if (GUI.Button(new Rect(x + 8, 181, 50, 29), "30")) ChangeTarget(30);
            if (GUI.Button(new Rect(x + 62, 181, 50, 29), "60")) ChangeTarget(60);
            if (GUI.Button(new Rect(x + 116, 181, 98, 29), fixture ? "Stop load" : "10m load"))
            { if (fixture) StartCoroutine(StopFixture()); else StartFixture(); }
            GUI.enabled = true; GUI.matrix = old;
        }
        private void OnDestroy()
        {
            if (fixture) { run.Player.Invulnerable = false; foreach (var bot in run.Match.Bots) bot.Health.Invulnerable = false;
                Screen.sleepTimeout = previousSleep; if (persistence != null) persistence.UseTestStore(originalStore); }
            log?.Dispose(); activity?.Dispose(); battery?.Dispose(); power?.Dispose();
        }
    }
}
#endif
