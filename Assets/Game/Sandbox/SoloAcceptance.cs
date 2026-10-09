using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using WaitYourTurn.Combat;
using WaitYourTurn.Navigation;
using WaitYourTurn.Player;
using WaitYourTurn.Run;

namespace WaitYourTurn.Sandbox
{
    /// <summary>Real CharacterController/NavMesh/save regression; isolated test save, bounded fixture.</summary>
    public sealed class SoloAcceptance : MonoBehaviour
    {
        [SerializeField] private RunDriver run;
        [SerializeField] private SoloProgression solo;
        [SerializeField] private RunEconomy economy;
        [SerializeField] private StationSpawner spawner;
        [SerializeField] private RunPersistence save;
        private bool checking;
        private string productionPath;
        private Behaviour[] views;
        private bool[] enabledViews;
        public void Configure(RunDriver owner, SoloProgression policy, RunEconomy shop, StationSpawner waves, RunPersistence persistence)
        { run = owner; solo = policy; economy = shop; spawner = waves; save = persistence; }
        private void Update() { if (!checking && Input.GetKeyDown(KeyCode.F2)) StartCoroutine(Check()); }
        private void OnGUI()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (checking || run.Flow == null || run.Flow.Paused) return;
            Rect safe = Screen.safeArea;
            float scale = Mathf.Max(.1f, Mathf.Min(safe.width / 960, safe.height / 540));
            var old = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(new Vector2(safe.x, Screen.height - safe.yMax), Quaternion.identity, new Vector3(scale, scale, 1));
            if (GUI.Button(new Rect(12, 82, 110, 25), "Solo check F2")) StartCoroutine(Check());
            GUI.matrix = old;
#endif
        }
        public IEnumerator Check()
        {
            if (checking) yield break;
            checking = true; productionPath = save.SavePath;
            views = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Where(v => v != this && (v.GetType().Name.EndsWith("Acceptance") || v is PersistenceHud || v is TrainIntegrationController || v is SoloHud)).Cast<Behaviour>().ToArray();
            enabledViews = views.Select(v => v.enabled).ToArray(); foreach (var view in views) view.enabled = false;
            save.UseTestStore(Path.Combine(Application.persistentDataPath, "acceptance-only", "solo-v2.json"));
            run.Match.SetMode(RunMode.Solo); run.ControlsAllowed = run.AimAllowed = false; run.Repair.enabled = false;
            spawner.SpawningAllowed = false; run.Player.Invulnerable = true;
            foreach (var wagon in run.Wagons) while (wagon.Enemies.WarmOne()) { }
            var motor = run.Player.GetComponent<PlayerMotor>();
            motor.ConfigureWorldControl(run.Player, 3.5f);
            yield return null;
            if (!Require(run.CurrentWagon == run.Wagons[0] && solo.UnlockedThrough == 0 && !solo.Connections[0].IsOpen, "Rear start / locked gate")) yield break;
            var gate = solo.Connections[0];
            motor.Place(gate.transform.position + Vector3.left * 1.5f + Vector3.up * .05f);
            economy.Wallet.Reset(0);
            if (!Require(!solo.TryUnlock(0) && solo.UnlockedThrough == 0, "Insufficient funds unlocked gate")) yield break;
            economy.Wallet.Reset(1000); int balance = economy.Wallet.Balance;
            if (!Require(solo.TryUnlock(0) && economy.Wallet.Balance == balance - gate.UnlockPrice && !gate.IsOpen, "Unlock transaction")) yield break;
            balance = economy.Wallet.Balance;
            if (!Require(!solo.TryUnlock(0) && economy.Wallet.Balance == balance, "Duplicate charge")) yield break;
            if (!Require(solo.TryToggle(0), "Open unlocked gate")) yield break;
            yield return null; yield return null;
            if (!Require(economy.Defenses.TryBuy(0) && economy.Drone.TryBuy(), "Solo defense setup")) yield break;
            var originalRack = economy.Defenses.CurrentRack;
            // Continuous movement, including the radius-inset join; no teleport masquerading as passage.
            ulong context = economy.Context; float health = run.Player.Current;
            motor.SetWorldMove(Vector3.right);
            yield return new WaitForSeconds(1.2f); motor.SetWorldMove(Vector3.zero);
            yield return new WaitForSeconds(.6f);
            if (!Require(economy.Drone.Actor.transform.position.x > gate.transform.position.x + .5f &&
                originalRack.Mounts.Any(m => m.Occupied) && economy.Defenses.CurrentRack != originalRack,
                "Drone physical follow / turret remains in original wagon")) yield break;
            if (!Require(run.CurrentWagon == run.Wagons[1] && run.Player.transform.position.x > gate.transform.position.x + 1 &&
                run.Player.Current == health && run.Player.DamageProtectionRemaining == 0 && economy.Context != context, "Physical forward walk/context/shield")) yield break;
            motor.SetWorldMove(Vector3.left); yield return new WaitForSeconds(.65f); motor.SetWorldMove(Vector3.zero);
            motor.Place(gate.transform.position + Vector3.up * .05f); yield return null;
            if (!Require(!solo.TryToggle(0) && gate.IsOpen, "Occupied connector close must fail")) yield break;
            // Pose in the gap must survive validation and a real JSON round-trip.
            run.Flow.Tick(1000); spawner.SpawningAllowed = true; yield return null; spawner.SpawningAllowed = false;
            var before = save.Capture();
            if (!Require(save.Validate(before, out string reason) && save.SaveNow(), "Connector save: " + reason + " / " + save.Status)) yield break;
            yield return save.LoadNow();
            if (!Require(save.Status == "Run resumed locally" && solo.UnlockedThrough == 1 && gate.IsOpen &&
                economy.Drone.Actor.Occupied && originalRack.Mounts.Any(m => m.Occupied) &&
                (run.Player.transform.position - before.participants[0].body.position).sqrMagnitude < .02f && economy.Wallet.Balance == balance, "Connector restore")) yield break;
            run.Player.Invulnerable = true;
            motor.Place(gate.transform.position + Vector3.right * 2.5f + Vector3.up * .05f); yield return null;
            economy.Drone.Actor.transform.position = gate.transform.position + Vector3.up * economy.Drone.Definition.hoverHeight;
            if (!Require(!solo.TryToggle(0) && gate.IsOpen, "Close on drone must fail")) yield break;
            float droneDeadline = Time.time + 3;
            var droneRegion = gate.Passage; droneRegion.Expand(economy.Drone.Definition.flightRadius * 2);
            while (Time.time < droneDeadline && droneRegion.Contains(gate.transform.InverseTransformPoint(economy.Drone.Actor.transform.position))) yield return null;
            if (!Require(solo.TryToggle(0) && !gate.IsOpen, "Close empty connector")) yield break;
            motor.SetWorldMove(Vector3.left); yield return new WaitForSeconds(.85f); motor.SetWorldMove(Vector3.zero);
            if (!Require(run.Player.transform.position.x > gate.transform.position.x, "Closed gate let player cross")) yield break;
            motor.Place(gate.transform.position + Vector3.right * 1.5f + Vector3.up * .05f);
            if (!Require(solo.TryToggle(0), "Reopen gate")) yield break;
            yield return null; yield return null;
            motor.SetWorldMove(Vector3.left); yield return new WaitForSeconds(1.15f); motor.SetWorldMove(Vector3.zero);
            if (!Require(run.CurrentWagon == run.Wagons[0], "Free return walk")) yield break;
            // Drone following/closing/save were checked above; isolate the hostile pursuit fixture.
            economy.Drone.Actor.Clear();
            var first = run.Wagons[0]; var second = run.Wagons[1];
            var profile = spawner.Programs[0].bands[0].profile;
            if (!Require(first.SpawnOutside(0, profile, 1), "Passenger spawn")) yield break;
            var enemy = first.Enemies.Active.Last(); enemy.Health.Invulnerable = true;
            enemy.GetComponent<AgentMotor>().TryPlace(gate.transform.position + Vector3.left * 1.6f); enemy.RestoreTravel(true);
            motor.Place(gate.transform.position + Vector3.right * 2.5f + Vector3.up * .05f); yield return null;
            enemy.SetPaused(true); enemy.GetComponent<AgentMotor>().TryPlace(gate.transform.position + Vector3.left * 3);
            if (!Require(solo.TryToggle(0) && !gate.IsOpen, "Close gate before pursuit")) yield break;
            enemy.SetPaused(false); yield return new WaitForSeconds(.7f);
            if (!Require(enemy.transform.position.x < gate.transform.position.x, "Enemy crossed closed gate")) yield break;
            if (!Require(solo.TryToggle(0), "Open pursuit gate")) yield break;
            float deadline = Time.time + 5;
            while (Time.time < deadline && enemy.transform.position.x < gate.transform.position.x + .5f) yield return null;
            if (!Require(enemy.transform.position.x > gate.transform.position.x + .5f && first.Enemies.Active.Contains(enemy), "Onboard chase / stable pool owner")) yield break;
            enemy.SetPaused(true);
            var chaseSave = save.Capture();
            if (!Require(save.Validate(chaseSave, out reason), "Cross-wagon enemy validation: " + reason)) yield break;
            yield return save.Restore(chaseSave);
            if (!Require(save.Status == "Run resumed locally" && first.Enemies.Active.Count == 1 && first.Enemies.Active[0].OnBoard, "Cross-wagon enemy restore")) yield break;
            first.Enemies.ClearAlive(); second.Enemies.ClearAlive(); run.Player.Invulnerable = true;
            // Spawn schedule is built once in Defense; moving changes the destination only.
            run.Flow.Tick(3); spawner.SpawningAllowed = true; yield return null; spawner.SpawningAllowed = false;
            if (!Require(second.Enemies.Active.Count > 0 && first.Enemies.Active.Count == 0, "New spawn must target current second wagon")) yield break;
            var schedule = spawner.Capture(); var flow = run.Flow;
            motor.Place(first.transform.position + Vector3.up * .05f); yield return null;
            if (!Require(run.Flow == flow && spawner.Capture().station == schedule.station &&
                spawner.Capture().schedule.emitted.SequenceEqual(schedule.schedule.emitted), "Walking reset station budget")) yield break;
            run.Flow.Tick(spawner.Definition.bands[0].interval);
            spawner.SpawningAllowed = true; yield return null; spawner.SpawningAllowed = false;
            if (!Require(first.Enemies.Active.Count > 0 && second.Enemies.Active.Count > 0 && spawner.Spawned > schedule.schedule.spawned,
                "Next spawn destination changed / existing enemy retained")) yield break;
            // External opening remains a barrier to the human, even while all connectors are open.
            var door = first.Doors[0]; door.Portal.SetOpen(true); yield return null;
            motor.Place(door.RepairPosition + Vector3.up * .05f);
            motor.MoveDisplacement(-door.Portal.transform.forward * 5);
            if (!Require(solo.Contains(run.Player.transform.position), "Solo exterior escape")) yield break;
            var position = run.Player.transform.position; var current = run.CurrentWagon;
            for (int phase = 0; phase < 6; phase++) run.Flow.Tick(1000);
            if (!Require(run.CurrentWagon == current && (run.Player.transform.position - position).sqrMagnitude < .001f &&
                run.Assignments == 0, "Solo station fade teleported player")) yield break;
            run.Match.SetMode(RunMode.Battle); yield return null; yield return null;
            if (!Require(!run.UsesSoloProgression && solo.Connections.All(x => !x.IsOpen) && run.Wagons.Count(w => w.HasLivingDefender) == 5, "Battle regression")) yield break;
            Finish(true, "Locked/paid/manual gates; continuous walk both ways; drone physical follow and turret retention; occupied close refusal; context without shield; connector and migrated enemy JSON restore; persistent station budget; exterior containment; no Solo random relocation; Battle gates/participants preserved.");
        }
        private bool Require(bool value, string message) { if (!value) Finish(false, message); return value; }
        private void Finish(bool pass, string message)
        {
            string report = (pass ? "PASS: " : "FAIL: ") + message;
            string directory = Application.isEditor ? "Logs" : Application.persistentDataPath;
            Directory.CreateDirectory(directory); File.WriteAllText(Path.Combine(directory, "SoloAcceptance.txt"), DateTime.UtcNow.ToString("O") + "\n" + report);
            if (pass) Debug.Log("[SoloAcceptance] " + report); else Debug.LogError("[SoloAcceptance] " + report);
            run.Player.GetComponent<PlayerMotor>().SetWorldMove(Vector3.zero);
            run.Player.GetComponent<PlayerMotor>().Configure(run.Player.GetComponent<MoveInput>(), run.Player, Camera.main);
            run.SetLoadingGate(false); run.ControlsAllowed = run.AimAllowed = true; run.Repair.enabled = true;
            spawner.SpawningAllowed = true; run.Match.AiAllowed = true; run.Player.Invulnerable = false;
            run.Match.SetMode(RunMode.Battle); save.UseTestStore(productionPath);
            for (int i = 0; i < views.Length; i++) views[i].enabled = enabledViews[i]; checking = false;
        }
    }
}
