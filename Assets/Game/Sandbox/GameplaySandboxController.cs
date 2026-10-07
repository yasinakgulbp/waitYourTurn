using System;
using System.Collections;
using System.IO;
using UnityEngine;
using WaitYourTurn.Combat;
using WaitYourTurn.Enemies;
using WaitYourTurn.Player;
using WaitYourTurn.Train;

namespace WaitYourTurn.Sandbox
{
    /// <summary>Temporary fixture and HUD. Runtime gameplay never depends on this component.</summary>
    public sealed class GameplaySandboxController : MonoBehaviour
    {
        [SerializeField] private DoorController door;
        [SerializeField] private HealthComponent player;
        [SerializeField] private PlayerMotor motor;
        [SerializeField] private MoveInput input;
        [SerializeField] private AutoAim aim;
        [SerializeField] private HitscanPistol pistol;
        [SerializeField] private ProximityRepair repair;
        [SerializeField] private EnemyPool enemies;
        [SerializeField] private Renderer doorVisual;
        [SerializeField] private LineRenderer trace;
        private bool checking;
        private float traceUntil;
        private string result = "Move with WASD/arrows or the left touch joystick. Aim/fire/reload are automatic.";
        private MaterialPropertyBlock tint;
        public void Configure(DoorController gate, HealthComponent hero, PlayerMotor movement, MoveInput controls,
            AutoAim targeting, HitscanPistol gun, ProximityRepair interaction, EnemyPool pool, Renderer visual, LineRenderer line)
        { door = gate; player = hero; motor = movement; input = controls; aim = targeting; pistol = gun;
            repair = interaction; enemies = pool; doorVisual = visual; trace = line; }
        private void OnEnable() => pistol.Fired += OnShot;
        private void OnDisable() => pistol.Fired -= OnShot;
        private void Awake() => tint = new MaterialPropertyBlock();
        private void Start() { ResetRound(); Spawn(3); }
        private void OnShot(ShotNotice shot)
        { trace.SetPosition(0, shot.Start); trace.SetPosition(1, shot.End); trace.enabled = true; traceUntil = Time.time + 0.08f; }
        private void Update()
        {
            doorVisual.enabled = !door.Portal.IsOpen;
            tint.SetColor("_Color", door.State == DoorState.WaitingForClearance ? Color.yellow : Color.red);
            doorVisual.SetPropertyBlock(tint);
            if (trace.enabled && Time.time >= traceUntil) trace.enabled = false;
            input.InputEnabled = !checking && player.IsAlive;
        }
        private void ResetRound()
        {
            enemies.ClearAlive();
            repair.Cancel();
            player.ResetForSpawn(100, Team.Player);
            motor.Place(new Vector3(0, 0.05f, 5.5f));
            door.Durability.ResetForSpawn(10, Team.Neutral);
            pistol.ResetWeapon();
            aim.ClearTarget();
            trace.enabled = false;
        }
        private void Spawn(int count)
        {
            for (int i = 0; i < count; i++)
                enemies.TrySpawn(new Vector3((i % 3 - 1) * 1.2f, 0, -4f - i / 3));
        }
        private void OnGUI()
        {
            Rect panel = new Rect(12, 12, Mathf.Min(420, Screen.width - 24), 210);
            input.BlockedScreenArea = panel;
            GUILayout.BeginArea(panel, GUI.skin.box);
            GUILayout.Label("ONE WAGON | M3 | Auto aim / fire");
            GUILayout.Label($"HP {player.Current}/{player.Maximum} | Door {door.Durability.Current}/{door.Durability.Maximum} : {door.State}");
            GUILayout.Label($"Ammo {pistol.Rounds}/8 {(pistol.Reloading ? "RELOADING" : "")} | Enemies {enemies.Active.Count} / pool {enemies.CreatedCount}");
            GUILayout.Label($"Repair: {(repair.Progress * 100):F0}% — stay near yellow marker for 3 seconds");
            Rect bar = GUILayoutUtility.GetRect(100, 10);
            GUI.Box(bar, "");
            GUI.Box(new Rect(bar.x, bar.y, bar.width * repair.Progress, bar.height), "");
            GUI.enabled = !checking;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Restart")) { ResetRound(); Spawn(3); }
            if (GUILayout.Button("+1 zombie")) Spawn(1);
            if (GUILayout.Button("+6 zombies")) Spawn(6);
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Check 10 door / repair / pool cycles")) StartCoroutine(CheckCycles());
            GUI.enabled = true;
            GUILayout.Label(player.IsAlive ? result : "PLAYER DIED — Restart starts a new test run.");
            GUILayout.EndArea();
        }
        private IEnumerator CheckCycles()
        {
            checking = true;
            aim.enabled = false;
            HealthComponent previous = null;
            DamageContext staleHit = default;
            for (int cycle = 1; cycle <= 10; cycle++)
            {
                ResetRound();
                Spawn(1);
                if (enemies.Active.Count != 1) { Finish(false, cycle - 1, "Validated spawn failed."); yield break; }
                EnemyBrain enemy = enemies.Active[0];
                if (pistol.HasSight(enemy.Health))
                { Finish(false, cycle - 1, "A closed door exposed a newly spawned target to the pistol."); yield break; }
                if (previous == enemy.Health && enemy.Health.TryApplyDamage(staleHit).Rejection != DamageRejection.StaleLife)
                { Finish(false, cycle - 1, "Old hit damaged a reused enemy life."); yield break; }
                float deadline = Time.time + 15f;
                while (door.State != DoorState.Broken && Time.time < deadline) yield return null;
                if (door.State != DoorState.Broken || !door.Portal.AcceptsEntry)
                { Finish(false, cycle - 1, "Enemy did not break/open the door."); yield break; }
                deadline = Time.time + 10f;
                while (player.Current >= player.Maximum && Time.time < deadline) yield return null;
                if (!enemy.IsInside || player.Current >= player.Maximum)
                { Finish(false, cycle - 1, "Enemy did not enter and damage the player."); yield break; }

                motor.Place(door.RepairPosition + Vector3.up * 0.05f);
                yield return new WaitForSeconds(0.8f);
                if (repair.Progress <= 0) { Finish(false, cycle - 1, "Proximity repair did not start."); yield break; }
                motor.Place(new Vector3(0, 0.05f, 6));
                yield return null;
                yield return null;
                if (repair.Progress != 0 || door.State != DoorState.Broken)
                { Finish(false, cycle - 1, "Leaving partially repaired the door."); yield break; }

                // First cycle holds a real CharacterController in the broken doorway.
                motor.Place(cycle == 1 ? door.Portal.transform.position + new Vector3(0, 0.05f, 0.25f)
                    : door.RepairPosition + Vector3.up * 0.05f);
                deadline = Time.time + 5f;
                while (!door.Durability.IsAlive && Time.time < deadline) yield return null;
                if (!door.Durability.IsAlive) { Finish(false, cycle - 1, "Repair did not restore durability."); yield break; }
                if (cycle == 1)
                {
                    if (!door.Portal.IsOpen || !door.Portal.ClosePending)
                    { Finish(false, cycle - 1, "Repair closed on a doorway occupant."); yield break; }
                    motor.Place(door.RepairPosition + Vector3.up * 0.05f);
                }
                deadline = Time.time + 5f;
                while (door.Portal.IsOpen && Time.time < deadline) yield return null;
                if (door.Portal.IsOpen) { Finish(false, cycle - 1, "Repaired doorway did not safely close."); yield break; }

                previous = enemy.Health;
                staleHit = new DamageContext(1000, player.Identity, Team.Player, (ulong)cycle, previous.LifeVersion);
                previous.TryApplyDamage(staleHit);
                yield return null;
                yield return null;
                if (enemies.Active.Count != 0 || enemies.DeathCount != 1 || enemies.CreatedCount != 12)
                { Finish(false, cycle - 1, "Death/return grew the pool or counted twice."); yield break; }
                if (enemies.TrySpawn(new Vector3(100, 0, 100)))
                { Finish(false, cycle - 1, "Invalid navigation spawn was accepted."); yield break; }
                result = $"Checks: {cycle}/10 passed.";
            }
            Finish(true, 10, "Enemy damage/entry, cancelled repair, safe closure, player damage and pool reuse passed.");
        }
        private void Finish(bool passed, int cycles, string message)
        {
            checking = false;
            aim.enabled = true;
            input.InputEnabled = player.IsAlive;
            result = $"{(passed ? "PASS" : "FAIL")}: {cycles}/10 — {message}";
            var report = new GameplayReport { passed = passed, cycles = cycles, message = message,
                platform = Application.platform.ToString(), utc = DateTime.UtcNow.ToString("O") };
#if UNITY_EDITOR
            Directory.CreateDirectory("Logs");
            File.WriteAllText("Logs/GameplaySandboxReport.json", JsonUtility.ToJson(report, true));
#endif
            if (passed) Debug.Log("[GameplaySandbox] " + result); else Debug.LogError("[GameplaySandbox] " + result);
        }
        [Serializable] private sealed class GameplayReport
        { public bool passed; public int cycles; public string message, platform, utc; }
    }
}
