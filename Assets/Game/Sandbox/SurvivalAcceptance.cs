using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using WaitYourTurn.Player;
using WaitYourTurn.Run;
using WaitYourTurn.Combat;

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
            run.Flow.Restore(RunPhase.Approach, 2, 0); spawner.ResetSchedule();
            run.Flow.Tick(run.Flow.Remaining + .01f); spawner.SpawningAllowed = true; yield return null;
            for (int round = 0; round < 4; round++)
            {
                run.Flow.Tick(round == 0 ? 5.1f : 4);
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
            // Complete the actual schedule, rather than using an empty queue as a false wave-end signal.
            run.ControlsAllowed = run.AimAllowed = false; run.Repair.enabled = false; run.Player.Invulnerable = true;
            run.Flow.Tick(100); spawner.SpawningAllowed = true;
            for (int frame = 0; frame < 8; frame++) yield return null;
            if (!Require(spawner.WaveCompleted && spawner.Dropped == 0 && run.Flow.Phase == RunPhase.Defense,
                "Untimed defense waits for exterior survivors after all 7 spawns")) yield break;
            spawner.SpawningAllowed = false;
            foreach (var wagon in run.Wagons) wagon.Enemies.ClearAlive();
            motor.Place(new Vector3(0, .05f, 0)); yield return new WaitForEndOfFrame();
            var view = Camera.main; Vector3 cameraBefore = view.transform.position;
            motor.Place(new Vector3(3, .05f, 1)); yield return new WaitForEndOfFrame();
            if (!Require(Vector3.Distance(view.transform.position - cameraBefore, new Vector3(3, 0, 1)) < .03f &&
                run.Loot == null && FindAnyObjectByType<SurvivalMiniMap>() != null, "Continuous player X/Z camera, no floor loot, minimap installed")) yield break;
            // Check offscreen anchors at the most exposed player positions and common landscape aspects.
            var visual = FindAnyObjectByType<RunPresentation>();
            foreach (float aspect in new[] { 4f / 3, 16f / 9, 20f / 9 })
            {
                Bounds volume = visual.FramingVolume; volume.Expand(new Vector3(1.3f, 0, 0));
                float distance = WagonCameraFraming.Distance(volume, view.transform.rotation, view.fieldOfView, aspect, WagonCameraFraming.ProtectedViewport);
                view.aspect = aspect;
                foreach (var wagon in run.Wagons) foreach (float z in new[] { -1.5f, 1.5f })
                {
                    view.transform.position = wagon.transform.position + new Vector3(0, .4f, z) - view.transform.forward * distance;
                    for (int anchor = 0; anchor < wagon.Geometry.SpawnCount; anchor++) foreach (float height in new[] { 0f, .85f, 1.7f })
                    {
                        Vector3 p = view.WorldToViewportPoint(wagon.Geometry.StationSpawn(anchor) + Vector3.up * height);
                        if (!Require(p.z <= 0 || p.x < 0 || p.x > 1 || p.y < 0 || p.y > 1, "Spawn silhouette outside camera / aspect " + aspect))
                        { view.ResetAspect(); yield break; }
                    }
                }
                Rect panel = SurvivalMiniMap.Panel(aspect * 540, 540);
                if (!Require(panel.xMin > 0 && panel.yMin > 0 && panel.xMax < aspect * 540 && panel.yMax < 540,
                    "Minimap safe-area bounds / aspect " + aspect)) { view.ResetAspect(); yield break; }
            }
            view.ResetAspect(); yield return new WaitForEndOfFrame();
            var entry = run.Wagons[0].Doors[0];
            motor.Place(entry.RepairPosition + Vector3.up * .05f);
            var boarder = pool.RestoreEnemy(entry.Portal.OutsideApproach, entry, spawner.Programs[0].bands[0].profile, 1, 0);
            var doomed = pool.RestoreEnemy(run.Wagons[0].Geometry.StationSpawn(1), run.Wagons[0].Doors[1], spawner.Programs[0].bands[0].profile, 1, 0);
            if (!Require(boarder != null && doomed != null, "Boarding and exterior-death setup")) yield break;
            doomed.Health.TryApplyDamage(new DamageContext(9999, default, Team.Player, 91001, doomed.Health.LifeVersion));
            spawner.SpawningAllowed = true; yield return null;
            if (!Require(run.Flow.Phase == RunPhase.Defense && FindAnyObjectByType<SurvivalJourney>().ExteriorAlive == 1,
                "Dead exterior enemy resolved; last live outsider still blocks departure")) yield break;
            entry.Durability.TryApplyDamage(new DamageContext(9999, default, Team.Enemy, 91002, entry.Durability.LifeVersion));
            deadline = Time.time + 5;
            while (run.Flow.Phase == RunPhase.Defense && Time.time < deadline) yield return null;
            if (!Require(run.Flow.Phase == RunPhase.Departing && boarder.OnBoard && pool.Active.Contains(boarder),
                "Actual last boarding departs immediately and retains onboard enemy")) yield break;
            run.Flow.Tick(run.Flow.Remaining + .01f); yield return new WaitForEndOfFrame();
            if (!Require(run.Flow.Phase == RunPhase.Cruising && !run.Flow.Paused && visual.Darkness == 0 &&
                visual.Speed > 0 && !boarder.Paused, "Visible moving cruise keeps gameplay active")) yield break;
            float hp = run.Player.Current; run.Player.Invulnerable = false;
            deadline = Time.time + 3;
            while (run.Player.Current >= hp && Time.time < deadline) yield return null;
            run.Player.Invulnerable = true;
            if (!Require(run.Player.Current < hp, "Onboard enemy inflicts damage during travel")) yield break;
            run.Repair.SetDoor(entry); run.Repair.Cancel(); run.Repair.Tick(3.1f);
            if (!Require(entry.Durability.Current == entry.Durability.Maximum && run.Flow.Phase == RunPhase.Cruising,
                "Repair during visible travel")) yield break;
            if (!Require(save.Validate(save.Capture(), out reason) && save.SaveNow(), "Cruise save: " + reason + " / " + save.Status)) yield break;
            yield return save.LoadNow();
            if (!Require(save.Status == "Run resumed locally" && run.Flow.Phase == RunPhase.Cruising && !run.Flow.Paused,
                "Visible cruise resumes from isolated save")) yield break;
            run.Flow.Tick(run.Flow.Remaining + .01f); yield return null;
            if (!Require(run.Flow.Phase == RunPhase.Approach && run.Flow.Station == 3 && visual.Darkness == 0,
                "Next station without fade or reassignment")) yield break;
            var battle = new RunFlow(new RunTimings());
            for (int boundary = 0; boundary < 4; boundary++) battle.Tick(1000);
            if (!Require(battle.Phase == RunPhase.FadeOut && battle.Paused, "Battle retains timed defense and dark transition")) yield break;
            Finish(true, "Open 3-wagon walking/navigation and pursuit; bilateral 7-enemy wave; untimed boarding departure including exterior kills; continuous player X/Z camera; offscreen spawn silhouettes at 4:3/16:9/20:9; minimap bounds; no floor loot; visible moving travel with enemy damage and repair; cruise save/resume; next station without fade/assignment; Battle timed/fade policy unchanged.");
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
