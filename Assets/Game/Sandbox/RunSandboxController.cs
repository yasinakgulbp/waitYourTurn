using System;
using System.Collections;
using System.IO;
using UnityEngine;
using WaitYourTurn.Combat;
using WaitYourTurn.Enemies;
using WaitYourTurn.Player;
using WaitYourTurn.Run;

namespace WaitYourTurn.Sandbox
{
    /// <summary>Temporary controls/fixture; runtime run flow has no dependency on this HUD.</summary>
    public sealed class RunSandboxController : MonoBehaviour
    {
        [SerializeField] private RunDriver run;
        [SerializeField] private StationSpawner spawner;
        [SerializeField] private RunPresentation presentation;
        private bool checking;
        private string result = "WASD/left joystick. Automatic aim, fire and reload. Repair near yellow marker.";
        public void Configure(RunDriver owner, StationSpawner spawn, RunPresentation visual)
        { run = owner; spawner = spawn; presentation = visual; }
        private void OnGUI()
        {
            if (run.Flow == null || run.CurrentWagon == null) return;
            Rect panel = new Rect(12, 12, 455, 255);
            run.Player.GetComponent<MoveInput>().BlockedScreenArea = panel;
            GUILayout.BeginArea(panel, GUI.skin.box);
            GUILayout.Label($"M4a | STATION {run.Flow.Station} | {run.CurrentWagon.Id} | {run.Flow.Phase} {run.Flow.Remaining:F1}s");
            GUILayout.Label($"HP {run.Player.Current}/{run.Player.Maximum} | Ammo {run.Pistol.Rounds}/8 {(run.Pistol.Reloading ? "RELOADING" : "")}");
            foreach (WagonRuntime wagon in run.Wagons)
                GUILayout.Label($"{wagon.Id}: door {wagon.Doors[0].Durability.Current}/{wagon.Doors[0].Durability.Maximum}, enemies {wagon.Enemies.Active.Count}/{wagon.Enemies.CreatedCount}");
            GUILayout.Label($"Repair {run.Repair.Progress * 100:F0}% | Train speed {presentation.Speed:F1} | Assignments {run.Assignments}");
            GUI.enabled = !checking;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Restart run")) { spawner.ResetSchedule(); run.Restart(); }
            if (GUILayout.Button("+3 current wagon")) for (int i = 0; i < 3; i++) if (run.Flow.Phase == RunPhase.Defense) run.CurrentWagon.SpawnOutside(i);
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Check 10 fast station transitions")) StartCoroutine(CheckTransitions());
            if (GUILayout.Button("Kill player (visible gameplay)"))
                run.Player.TryApplyDamage(new DamageContext(1000, default, Team.Enemy, 1, run.Player.LifeVersion));
            GUI.enabled = true;
            GUILayout.Label(run.Failure ?? result);
            GUILayout.EndArea();
            if (presentation.Darkness > 0)
            {
                GUI.color = new Color(0, 0, 0, presentation.Darkness);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture); GUI.color = Color.white;
                if (run.Flow.Phase == RunPhase.Hidden)
                    GUI.Label(new Rect(Screen.width / 2 - 160, Screen.height / 2 - 30, 320, 100),
                        $"Vagon dağılımı hazırlanıyor...\n{Mathf.CeilToInt(run.Flow.Remaining)}\nTren yolculuğu sürüyor.");
            }
            if (run.Flow.Phase == RunPhase.GameOver)
                GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2, 250, 60), "KOŞU BİTTİ — Restart run");
        }
        private IEnumerator CheckTransitions()
        {
            checking = true; spawner.enabled = false; run.ControlsAllowed = run.AimAllowed = false;
            run.Restart(new RunTimings { initialApproach = .2f, nextApproach = .2f, defense = .3f,
                departureWarning = .2f, departure = .2f, fadeOut = .2f, hidden = .4f, fadeIn = .2f });
            run.Player.TryApplyDamage(new DamageContext(15, default, Team.Enemy, 2, run.Player.LifeVersion));
            run.Player.Invulnerable = true; // Fixture isolation: real combat continues in the normal scene.
            run.Pistol.TryFire(Vector3.forward);
            var a = run.Wagons[0]; var b = run.Wagons[1];
            a.Doors[0].Durability.TryApplyDamage(new DamageContext(1000, default, Team.Enemy, 3, a.Doors[0].Durability.LifeVersion));
            b.Doors[0].Durability.TryApplyDamage(new DamageContext(4, default, Team.Enemy, 4, b.Doors[0].Durability.LifeVersion));
            run.Player.GetComponent<PlayerMotor>().Place(a.Doors[0].RepairPosition + Vector3.up * .05f);
            if (!a.Enemies.TrySpawn(a.transform.TransformPoint(new Vector3(-1.2f, 0, -7))) ||
                !b.Enemies.TrySpawn(b.transform.TransformPoint(new Vector3(0, 0, 5.5f))))
            { Finish(false, "Fixture spawn failed."); yield break; }
            EnemyBrain survivor = b.Enemies.Active[0];
            Vector3 thresholdPoint = a.Doors[0].Portal.transform.position + Vector3.forward * .1f;
            if (!a.Enemies.TrySpawn(thresholdPoint)) { Finish(false, "Doorway fixture spawn failed."); yield break; }
            EnemyBrain thresholdSurvivor = a.Enemies.Active[1];
            thresholdSurvivor.SetPaused(true); // Hold exactly at the departure boundary to exercise safe completion.
            if (!thresholdSurvivor.IsInside || !thresholdSurvivor.NeedsSafeDeparture)
            { Finish(false, "Doorway fixture did not occupy the inner threshold."); yield break; }
            uint playerLife = run.Player.LifeVersion, doorA = a.Doors[0].Durability.LifeVersion, doorB = b.Doors[0].Durability.LifeVersion;
            int ammo = run.Pistol.Rounds; ulong hitId = 10;
            bool wasPaused = false; float frozenTime = 0, repairProgress = 0; Vector3 frozenEnemy = default;
            float deadline = Time.realtimeSinceStartup + 35;
            while (run.Flow.Station < 11 && Time.realtimeSinceStartup < deadline)
            {
                if (run.Flow.Phase == RunPhase.Faulted) { Finish(false, run.Failure); yield break; }
                if (run.Player.Current != 85 || run.Player.LifeVersion != playerLife || run.Pistol.Rounds != ammo ||
                    a.Doors[0].Durability.Current != 0 || b.Doors[0].Durability.Current != 16 ||
                    a.Doors[0].Durability.LifeVersion != doorA || b.Doors[0].Durability.LifeVersion != doorB)
                { Finish(false, "Player/ammo/door state reset between phases."); yield break; }
                if (survivor == null || !b.Enemies.Active.ContainsReference(survivor) || survivor.WagonId != b.Id)
                { Finish(false, "Onboard enemy lost or changed wagon ownership."); yield break; }
                if (run.Flow.Paused)
                {
                    if (!wasPaused) { frozenTime = Time.time; frozenEnemy = survivor.transform.position; repairProgress = run.Repair.Progress; }
                    if (Mathf.Abs(Time.time - frozenTime) > .001f || (survivor.transform.position - frozenEnemy).sqrMagnitude > .0001f ||
                        !survivor.Paused || run.Pistol.enabled == false)
                    { Finish(false, "Dark passage advanced gameplay clock or enemy movement."); yield break; }
                    if (run.Flow.Phase != RunPhase.FadeIn && Mathf.Abs(run.Repair.Progress - repairProgress) > .001f)
                    { Finish(false, "Repair advanced during darkness."); yield break; }
                    var blocked = b.Doors[0].Durability.TryApplyDamage(new DamageContext(1, default, Team.Enemy, hitId++, doorB));
                    if (blocked.Applied) { Finish(false, "Damage applied during darkness."); yield break; }
                }
                wasPaused = run.Flow.Paused;
                if (run.Flow.Station > 1 && (a.Enemies.Active.Count != 1 || a.Enemies.DeathCount != 0 || !survivor.OnBoard ||
                    !thresholdSurvivor.OnBoard || thresholdSurvivor.NeedsSafeDeparture))
                { Finish(false, "Outside cleanup counted a kill or onboard membership was lost."); yield break; }
                yield return null;
            }
            if (run.Flow.Station != 11 || run.Assignments != 10 || a.Enemies.CreatedCount + b.Enemies.CreatedCount != 24)
            { Finish(false, "Ten transitions, one assignment each or bounded pools failed."); yield break; }
            run.Player.Invulnerable = false;
            run.Player.TryApplyDamage(new DamageContext(1000, default, Team.Enemy, 99, run.Player.LifeVersion));
            yield return new WaitForSecondsRealtime(.2f);
            if (run.Flow.Phase != RunPhase.GameOver || run.Player.IsAlive || run.Assignments != 10 || run.Flow.Station != 11)
            { Finish(false, "Death resumed the run or reset a life."); yield break; }
            Finish(true, "10 station transitions: HP/ammo/door lives persist; dark clock/motion/repair/damage pause; inside and doorway survivors retained safely, outside returned without kill; 10 assignments, 24 pooled bodies; death stays GameOver.");
        }
        private void Finish(bool passed, string message)
        {
            result = $"{(passed ? "PASS" : "FAIL")}: {message}";
#if UNITY_EDITOR
            Directory.CreateDirectory("Logs");
            File.WriteAllText("Logs/RunSandboxReport.json", JsonUtility.ToJson(new Report
            { passed = passed, station = run.Flow.Station, assignments = run.Assignments, message = message,
                platform = Application.platform.ToString(), utc = DateTime.UtcNow.ToString("O") }, true));
#endif
            if (passed) Debug.Log("[RunSandbox] " + result); else Debug.LogError("[RunSandbox] " + result);
            checking = false; run.Player.Invulnerable = false; run.ControlsAllowed = run.AimAllowed = true;
            spawner.ResetSchedule(); spawner.enabled = true; run.Restart();
        }
        [Serializable] private sealed class Report { public bool passed; public int station, assignments; public string message, platform, utc; }
    }
    internal static class LabReferences
    {
        public static bool ContainsReference(this System.Collections.Generic.IReadOnlyList<EnemyBrain> enemies, EnemyBrain target)
        { foreach (EnemyBrain enemy in enemies) if (enemy == target) return true; return false; }
    }
}
