using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using WaitYourTurn.Combat;
using WaitYourTurn.Player;
using WaitYourTurn.Run;
using WaitYourTurn.Navigation;

namespace WaitYourTurn.Sandbox
{
    /// <summary>F4 real scene round-trip. Uses a separate test file, never the player's save.</summary>
    public sealed class PersistenceAcceptance : MonoBehaviour
    {
        [SerializeField] private RunDriver run;
        [SerializeField] private RunEconomy economy;
        [SerializeField] private StationSpawner spawner;
        [SerializeField] private RunPersistence save;
        private bool checking;
        private string productionPath;
        private Behaviour[] views;
        private bool[] enabledViews;
        public void Configure(RunDriver owner, RunEconomy shop, StationSpawner spawn, RunPersistence persistence)
        { run = owner; economy = shop; spawner = spawn; save = persistence; }
        private void Update() { if (!checking && Input.GetKeyDown(KeyCode.F4)) StartCoroutine(Check()); }
        private IEnumerator Check()
        {
            checking = true; productionPath = save.SavePath;
            views = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                .Where(v => v != this && (v.GetType().Name.EndsWith("Acceptance") || v is PersistenceHud || v is TrainIntegrationController)).Cast<Behaviour>().ToArray();
            enabledViews = views.Select(v => v.enabled).ToArray(); foreach (var v in views) v.enabled = false;
            save.UseTestStore(Path.Combine(Application.persistentDataPath, "acceptance-only", "run.json"));
            run.Match.SetMode(RunMode.Battle); run.Match.AiAllowed = false; run.ControlsAllowed = run.AimAllowed = false;
            run.Repair.enabled = false; spawner.SpawningAllowed = false;
            var motor = run.Player.GetComponent<PlayerMotor>(); var input = run.Player.GetComponent<MoveInput>();
            motor.ConfigureWorldControl(run.Player, 3.5f);
            foreach (var direction in new[] { Vector3.right, Vector3.left, Vector3.forward, Vector3.back })
            {
                motor.SetWorldMove(direction); yield return new WaitForSeconds(.12f);
                if (!Require(Vector3.Dot(run.Player.transform.forward, direction) > .99f, "Idle-target walking facing")) yield break;
            }
            motor.SetWorldMove(Vector3.zero); motor.Configure(input, run.Player, Camera.main);
            run.Restart(); run.Match.AiAllowed = false;
            foreach (var wagon in run.Wagons) while (wagon.Enemies.WarmOne()) { }
            yield return null;
            var current = run.CurrentWagon; var targetPoint = run.Player.transform.position + Vector3.forward * .9f;
            if (!Require(current.SpawnOutside(0, spawner.Programs[0].bands[0].profile), "Facing target setup")) yield break;
            var target = current.Enemies.Active.Last(); target.GetComponent<AgentMotor>().TryPlace(targetPoint); target.SetPaused(true); target.Health.Invulnerable = true;
            run.AimAllowed = true; motor.ConfigureWorldControl(run.Player, 3.5f); motor.SetWorldMove(Vector3.right);
            yield return new WaitForSeconds(.22f);
            Vector3 toward = target.transform.position - run.Player.transform.position; toward.y = 0;
            if (!Require(Vector3.Dot(run.Player.transform.forward, toward.normalized) > .99f, "Target facing must override walking")) yield break;
            motor.SetWorldMove(Vector3.zero); motor.Configure(input, run.Player, Camera.main); run.AimAllowed = false;
            current.Enemies.ClearAlive(); run.Restart(); run.Match.AiAllowed = false; spawner.SpawningAllowed = true;
            run.Flow.Tick(1000); yield return null; spawner.SpawningAllowed = false;
            current = run.CurrentWagon;
            run.Player.TryApplyDamage(new DamageContext(23, default, Team.Enemy, 1, run.Player.LifeVersion));
            economy.Wallet.TrySpend(37); run.Weapon.TryGrantOrRefill(1); run.Weapon.TryGrantOrRefill(2); run.Weapon.Equip(1);
            run.Weapon.TryFire(Vector3.up);
            if (!Require(economy.Defenses.TryBuy(0) && economy.Drone.TryBuy(), "Defense setup")) yield break;
            var door = current.Doors[0]; motor.Place(door.RepairPosition + Vector3.up * .05f);
            door.Durability.TryApplyDamage(new DamageContext(door.Durability.Maximum * .5f, default, Team.Enemy, 2, door.Durability.LifeVersion));
            run.Repair.SetDoor(door); run.Repair.Tick(1);
            var doomed = run.Match.Bots[1]; doomed.Health.Invulnerable = false;
            doomed.Health.TryApplyDamage(new DamageContext(100000, default, Team.Enemy, 3, doomed.Health.LifeVersion));
            yield return null;
            if (!Require(current.SpawnOutside(0, spawner.Programs[0].bands[0].profile), "Saved passenger setup")) yield break;
            var passenger = current.Enemies.Active.Last(); passenger.GetComponent<AgentMotor>().TryPlace(current.transform.position + Vector3.right * 2);
            passenger.RestoreTravel(true); passenger.SetPaused(true);
            passenger.Health.TryApplyDamage(new DamageContext(1, default, Team.Player, 4, passenger.Health.LifeVersion));
            run.Match.Bots[0].Wallet.TrySpend(29);
            var before = save.Capture();
            if (!Require(save.Validate(before, out var reason), "Capture validation: " + reason)) yield break;
            if (!Require(save.SaveNow(), "SaveNow: " + save.Status)) yield break;
            var store = new LocalRunStore(save.SavePath); var disk = store.Load();
            if (!Require(disk != null && disk.participants[0].coins == before.participants[0].coins, "Disk JSON round-trip")) yield break;
            ulong oldContext = economy.Context;
            yield return save.Restore(disk);
            var after = save.Capture();
            if (!Require(save.Status == "Run resumed locally" && after.station == before.station && after.assignments == before.assignments &&
                Math.Abs(after.elapsed - before.elapsed) < .2f && after.humanRandom == before.humanRandom && after.botRandom == before.botRandom &&
                before.ranks.SequenceEqual(after.ranks) && economy.Context != oldContext, "Flow/RNG/ranks/context restore: " + save.Status)) yield break;
            for (int i = 0; i < before.participants.Length; i++)
            {
                var a = before.participants[i]; var b = after.participants[i];
                if (!Require(a.body.current == b.body.current && a.body.maximum == b.body.maximum && a.wagon == b.wagon && a.coins == b.coins &&
                    a.weapon.equipped == b.weapon.equipped && a.weapon.owned.SequenceEqual(b.weapon.owned) &&
                    a.weapon.ammo.Select(x => x.rounds).SequenceEqual(b.weapon.ammo.Select(x => x.rounds)) &&
                    a.weapon.ammo.Select(x => x.reserve).SequenceEqual(b.weapon.ammo.Select(x => x.reserve)), "Participant state " + i)) yield break;
            }
            for (int w = 0; w < before.wagons.Length; w++)
            {
                var a = before.wagons[w]; var b = after.wagons[w];
                if (!Require(a.doors.Select(d => d.current).SequenceEqual(b.doors.Select(d => d.current)) && a.enemies.Length == b.enemies.Length &&
                    a.enemies.Select(e => e.body.current).SequenceEqual(b.enemies.Select(e => e.body.current)) &&
                    a.turrets.Length == b.turrets.Length && a.spawnSequence == b.spawnSequence, "Wagon state " + w)) yield break;
            }
            if (!Require(after.drone != null && after.drone.weapon.ammo[0].rounds == before.drone.weapon.ammo[0].rounds &&
                after.spawner.schedule.spawned == before.spawner.schedule.spawned &&
                after.spawner.schedule.emitted.SequenceEqual(before.spawner.schedule.emitted) && !run.Match.Bots[1].gameObject.activeSelf &&
                run.Repair.Progress >= before.participants[0].repairProgress - .01f, "Drone/spawn/repair/eliminated bot restore")) yield break;
            var invalid = JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(disk)); invalid.participants[0].weapon.ammo[0].rounds = 999;
            int coins = economy.Wallet.Balance;
            yield return save.Restore(invalid);
            if (!Require(save.Status.StartsWith("Load rejected") && economy.Wallet.Balance == coins, "Invalid snapshot mutated live run")) yield break;
            // Every phase resumes without replaying assignment/departure side effects.
            for (int p = 0; p <= (int)RunPhase.FadeIn; p++)
            {
                var phaseData = JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(disk)); phaseData.phase = (RunPhase)p; phaseData.elapsed = .25f;
                if (p != (int)RunPhase.Defense) phaseData.spawner.schedule = null;
                yield return save.Restore(phaseData);
                if (!Require(save.Status == "Run resumed locally" && run.Flow.Phase == (RunPhase)p && run.Assignments == disk.assignments, "Phase restore " + p + ": " + save.Status)) yield break;
            }
            var rng = new RunRandom(1) { State = disk.humanRandom }; int expected = rng.Next(run.Wagons.Length);
            var hidden = JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(disk)); hidden.phase = RunPhase.Hidden; hidden.elapsed = .25f; hidden.spawner.schedule = null;
            yield return save.Restore(hidden); run.Flow.Tick(1000);
            if (!Require(Array.IndexOf(run.Wagons, run.CurrentWagon) == expected && run.Assignments == disk.assignments + 1, "Next random assignment continuation")) yield break;
            run.Match.SetMode(RunMode.Solo); yield return null;
            if (!Require(save.SaveNow(), "Solo without drone disk save")) yield break;
            var solo = store.Load(); yield return save.Restore(solo);
            if (!Require(save.Status == "Run resumed locally" && !run.Match.Active && !solo.hasDrone && !economy.Drone.Actor.Occupied &&
                run.Wagons.Count(w => w.HasLivingDefender) == 1, "Solo without drone JSON restore")) yield break;
            if (!Require(save.SaveNow(), "Solo disk save")) yield break;
            run.Player.Invulnerable = false; run.Player.SetDamageProtection(0);
            run.Player.TryApplyDamage(new DamageContext(100000, default, Team.Enemy, 9, run.Player.LifeVersion)); yield return null;
            if (!Require(store.Load() == null && !save.SaveNow(), "Dead run was resumable")) yield break;
            Finish(true, "Human walking faces 4 directions; visible target overrides movement. Real disk JSON: Battle participant HP/max/coins/guns/ammo/roster/RNG; door damage/repair progress, retained passenger, turret and drone, spawn progress restored; eliminated bot stays out; all 7 phases preserve assignment count; next draw continues; invalid ammo rejected before mutation; Solo and death tombstone passed. PC only; Android pause/kill and device IO pending.");
        }
        private bool Require(bool condition, string message) { if (!condition) Finish(false, message); return condition; }
        private void Finish(bool pass, string detail)
        {
            string result = (pass ? "PASS: " : "FAIL: ") + detail;
            Directory.CreateDirectory("Logs"); File.WriteAllText("Logs/PersistenceAcceptance.txt", DateTime.UtcNow.ToString("O") + "\n" + result);
            if (pass) Debug.Log("[Save] " + result); else Debug.LogError("[Save] " + result);
            run.Player.GetComponent<PlayerMotor>().Configure(run.Player.GetComponent<MoveInput>(), run.Player, Camera.main);
            run.SetLoadingGate(false); run.ControlsAllowed = run.AimAllowed = true; run.Repair.enabled = true;
            spawner.SpawningAllowed = true; run.Match.AiAllowed = true;
            run.Match.SetMode(RunMode.Battle); save.UseTestStore(productionPath);
            for (int i = 0; i < views.Length; i++) views[i].enabled = enabledViews[i]; checking = false;
        }
    }
}
