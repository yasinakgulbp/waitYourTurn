using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using WaitYourTurn.Player;
using WaitYourTurn.Run;

namespace WaitYourTurn.Sandbox
{
    /// <summary>Small combined check of the survival pivot, using real controller/nav and an isolated save.</summary>
    public sealed class SurvivalAcceptance : MonoBehaviour
    {
        [SerializeField] private RunDriver run;
        [SerializeField] private StationSpawner spawner;
        [SerializeField] private RunPersistence save;
        private bool checking;
        private string productionPath;
        public void Configure(RunDriver owner, StationSpawner waves, RunPersistence persistence)
        { run = owner; spawner = waves; save = persistence; }
        public IEnumerator Check()
        {
            if (checking) yield break;
            checking = true; productionPath = save.SavePath;
            save.UseTestStore(Path.Combine(Application.persistentDataPath, "acceptance-only", "survival-open-train.json"));
            run.Restart(); run.ControlsAllowed = run.AimAllowed = false; run.Repair.enabled = false;
            spawner.SpawningAllowed = false; run.Player.Invulnerable = true;
            var motor = run.Player.GetComponent<PlayerMotor>(); motor.ConfigureWorldControl(run.Player, 3.5f);
            foreach (var wagon in run.Wagons) while (wagon.Enemies.WarmOne()) { }
            yield return null;
            var solo = run.Solo;
            if (!Require(run.UsesSoloProgression && solo.OpenTrainSurvival && run.Wagons.Length == 3 &&
                run.Match.Bots.Length == 0 && solo.UnlockedThrough == 2 && solo.Connections.Length == 2 &&
                solo.Connections.All(g => g.IsOpen && g.PermanentlyOpen && Mathf.Approximately(g.Passage.size.z, 3) && !g.TrySetOpen(false)),
                "Three accessible wagons / permanently open 3m passages")) yield break;
            var path = new NavMeshPath(); var pool = run.Wagons[0].Enemies;
            var filter = new NavMeshQueryFilter { agentTypeID = pool.AgentTypeId, areaMask = pool.AreaMask };
            if (!Require(NavMesh.CalculatePath(run.Wagons[0].transform.position, run.Wagons[2].transform.position, filter, path) &&
                path.status == NavMeshPathStatus.PathComplete, "Complete rear-to-head nav route")) yield break;
            for (int i = 0; i < solo.Connections.Length; i++)
            {
                float x = solo.Connections[i].transform.position.x;
                motor.Place(new Vector3(x - 1.5f, .05f, 0)); yield return null;
                uint life = run.Player.LifeVersion;
                motor.SetWorldMove(Vector3.right); yield return new WaitForSeconds(1.2f); motor.SetWorldMove(Vector3.zero); yield return null;
                if (!Require(run.CurrentWagon == run.Wagons[i + 1] && run.Player.transform.position.x > x + 1 &&
                    run.Player.LifeVersion == life && run.Player.DamageProtectionRemaining == 0, "Forward physical passage " + i)) yield break;
                motor.SetWorldMove(Vector3.left); yield return new WaitForSeconds(1.2f); motor.SetWorldMove(Vector3.zero); yield return null;
                if (!Require(run.CurrentWagon == run.Wagons[i] && run.Player.transform.position.x < x, "Return physical passage " + i)) yield break;
            }
            // An enemy remains owned by the rear pool after reaching another wagon; it must cross the second connector too.
            motor.Place(new Vector3(23, .05f, 0)); yield return null;
            var pursuer = pool.RestoreEnemy(new Vector3(17.5f, 0, 0), run.Wagons[0].Doors[0], spawner.Programs[0].bands[0].profile, 1, 0);
            if (!Require(pursuer != null, "Onboard pursuit setup")) yield break;
            pursuer.RestoreTravel(true);
            float deadline = Time.time + 6;
            while (pursuer.transform.position.x < 21 && Time.time < deadline) yield return null;
            if (!Require(pursuer.transform.position.x > 21 && pursuer.OnBoard && pool.Active.Contains(pursuer),
                "Cross-wagon onboard pursuit / stable pool ownership")) yield break;
            foreach (var wagon in run.Wagons) wagon.Enemies.ClearAlive();
            // Check the real schedule with the player at the head: every wagon and both platform sides still receive enemies.
            run.Flow.Restore(RunPhase.Approach, 1, 0); spawner.ResetSchedule();
            run.Flow.Tick(run.Flow.Remaining + .01f); spawner.SpawningAllowed = true; yield return null;
            for (int round = 0; round < 4; round++)
            {
                run.Flow.Tick(round == 0 ? 4.1f : 6);
                yield return null; yield return null; yield return null;
            }
            spawner.SpawningAllowed = false;
            if (!Require(spawner.Diagnostic == null && run.Wagons.All(w =>
                w.Enemies.Active.Any(e => e.transform.position.z < 0) && w.Enemies.Active.Any(e => e.transform.position.z > 0)),
                "Bilateral spawning in all three wagons, including unoccupied rear")) yield break;
            var before = spawner.Capture(); var flow = run.Flow;
            motor.Place(new Vector3(0, .05f, 0)); yield return null;
            if (!Require(run.Flow == flow && spawner.Capture().schedule.emitted.SequenceEqual(before.schedule.emitted),
                "Walking preserves station budgets")) yield break;
            if (!Require(save.Validate(save.Capture(), out string reason) && save.SaveNow(), "Save validation: " + reason + " / " + save.Status)) yield break;
            var corrupt = save.Capture(); corrupt.interiorGates[0] = false;
            if (!Require(!save.Validate(corrupt, out _), "Closed survival connector must be rejected")) yield break;
            yield return save.LoadNow();
            if (!Require(save.Status == "Run resumed locally" && solo.UnlockedThrough == 2 && solo.Connections.All(g => g.IsOpen),
                "Isolated survival save/resume")) yield break;
            Finish(true, "3 accessible wagons; two permanent 3m passages; actual walking both ways; full NavMesh path; onboard pursuit across wagons; all-wagon bilateral bounded spawning; stable station budget; separate save/restore and invalid closed topology rejection.");
        }
        private bool Require(bool value, string message) { if (!value) Finish(false, message); return value; }
        private void Finish(bool pass, string message)
        {
            var report = DateTime.UtcNow.ToString("O") + "\n" + (pass ? "PASS: " : "FAIL: ") + message;
            Directory.CreateDirectory("Logs"); File.WriteAllText("Logs/SurvivalAcceptance.txt", report);
            if (pass) Debug.Log("[SurvivalAcceptance] " + report); else Debug.LogError("[SurvivalAcceptance] " + report);
            var motor = run.Player.GetComponent<PlayerMotor>(); motor.SetWorldMove(Vector3.zero);
            motor.Configure(run.Player.GetComponent<MoveInput>(), run.Player, Camera.main);
            run.ControlsAllowed = run.AimAllowed = true; run.Repair.enabled = true;
            spawner.SpawningAllowed = true; run.Player.Invulnerable = false;
            run.Restart(); save.UseTestStore(productionPath); checking = false;
        }
    }
}
