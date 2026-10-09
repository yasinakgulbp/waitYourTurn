using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using WaitYourTurn.Combat;
using WaitYourTurn.Economy;
using WaitYourTurn.Player;
using WaitYourTurn.Run;

namespace WaitYourTurn.Sandbox
{
    public sealed class SurvivalOnboardingAcceptance : MonoBehaviour
    {
        [SerializeField] private RunDriver run;
        [SerializeField] private StationSpawner spawner;
        [SerializeField] private RunPersistence save;
        [SerializeField] private RunEconomy economy;
        private bool checking;
        private string productionPath;
        public void Configure(RunDriver owner, StationSpawner waves, RunPersistence persistence, RunEconomy shop)
        { run = owner; spawner = waves; save = persistence; economy = shop; }
        public IEnumerator Check()
        {
            if (checking) yield break;
            checking = true; productionPath = save.SavePath;
            save.UseTestStore(Path.Combine(Application.persistentDataPath, "acceptance-only", "survival-onboarding.json"));
            run.Restart(); spawner.ResetSchedule(); run.ControlsAllowed = run.AimAllowed = false; run.Repair.enabled = false;
            spawner.SpawningAllowed = false; run.Player.Invulnerable = true;
            foreach (var wagon in run.Wagons) while (wagon.Enemies.WarmOne()) { }
            yield return null;
            if (!Require(run.CurrentWagon == run.Wagons[1] && AllHp(20), "Middle start / equal20HP")) yield break;
            if (!Require(spawner.Programs[0].BuildStreams(3, 1).Sum(s => s.Count) == 4 &&
                spawner.Programs[1].BuildStreams(3, 2).Sum(s => s.Count) == 7 &&
                spawner.Programs[2].BuildStreams(3, 3).Sum(s => s.Count) == 10 &&
                spawner.Programs[2].BuildStreams(3, 4).Sum(s => s.Count) == 13 &&
                spawner.Programs[2].BuildStreams(3, 999).Sum(s => s.Count) == 36, "4/7/10/+3 capped budgets")) yield break;
            string trace = $"initial={run.Flow.Phase}/{run.Flow.Remaining}/boarding={run.Flow.BoardingWaves}";
            run.Flow.Tick(run.Flow.Remaining + .01f); trace += $" tick={run.Flow.Phase}"; spawner.SpawningAllowed = true; yield return null;
            for (int n = 0; n < 4; n++)
            { run.Flow.Tick(n == 0 ? 3.1f : 3.5f); trace += $" n{n}={run.Flow.Phase}/{run.Flow.Elapsed}/{spawner.Spawned}"; for (int f = 0; f < 4; f++) yield return null; }
            run.Flow.Tick(1); for (int f = 0; f < 8; f++) yield return null;
            spawner.SpawningAllowed = false;
            if (!Require(spawner.WaveCompleted && spawner.Dropped == 0 && Counts(0, 4, 0),
                $"Actual first wave: {trace} phase={run.Flow.Phase} elapsed={run.Flow.Elapsed} spawned={spawner.Spawned} dropped={spawner.Dropped} completed={spawner.WaveCompleted} counts={run.Wagons[0].Enemies.Active.Count}/{run.Wagons[1].Enemies.Active.Count}/{run.Wagons[2].Enemies.Active.Count}")) yield break;
            foreach (var wagon in run.Wagons) wagon.Enemies.ClearAlive();
            run.Flow.Restore(RunPhase.Approach, 2, 0); spawner.ResetSchedule();
            run.Flow.Tick(run.Flow.Remaining + .01f); spawner.SpawningAllowed = true; yield return null;
            run.Flow.Tick(13); for (int f = 0; f < 12; f++) yield return null;
            spawner.SpawningAllowed = false;
            if (!Require(spawner.WaveCompleted && spawner.Dropped == 0 && Counts(2, 3, 2), "Actual second wave seven distributed")) yield break;
            var door = run.Wagons[1].Doors[0]; var broken = run.Wagons[1].Doors[1];
            run.Player.GetComponent<PlayerMotor>().Place(door.RepairPosition + Vector3.up * .05f);
            Hit(door.Durability, 5); Hit(broken.Durability, 999);
            int wood = Array.FindIndex(economy.Catalog.items, i => i.id == "wood"), wire = Array.FindIndex(economy.Catalog.items, i => i.id == "wire");
            int coins = economy.Wallet.Balance;
            if (!Require(economy.Buy(wire, economy.Context, economy.NextRequest()) == PurchaseResult.Unavailable &&
                economy.Wallet.Balance == coins && economy.Buy(wood, economy.Context, economy.NextRequest()) == PurchaseResult.Success &&
                AllHp(30) && door.Durability.Current == 25 && broken.Durability.Current == 0,
                "Ordered paid upgrade preserves missing health / broken door")) yield break;
            run.Repair.SetDoor(door); run.Repair.Cancel(); run.Repair.Tick(1); Hit(door.Durability, 1); run.Repair.Tick(1);
            if (!Require(!run.Repair.InterruptOnDoorDamage && Mathf.Approximately(run.Repair.Progress, 2f / 3), "Door hit preserves repair")) yield break;
            run.Repair.Tick(1); if (!Require(door.Durability.Current == 30, "Repair finishes under damage")) yield break;
            yield return new WaitForSeconds(.4f);
            if (!Require(economy.Buy(wire, economy.Context, economy.NextRequest()) == PurchaseResult.Success &&
                economy.Reinforcement.Level == 2 && AllHp(40) && !economy.CanApply(economy.Catalog.items[wire].Product), "Wire stage / no repeat charge")) yield break;
            if (!Require(run.Wagons.All(w => w.GetComponentsInChildren<WaitYourTurn.Train.DoorReinforcementVisual>(true)
                .All(v => v.GetComponentsInChildren<Collider>(true).Length == 0)), "Decorations have no shot/movement collider")) yield break;
            yield return null;
            if (!Require(save.Validate(save.Capture(), out string reason) && save.SaveNow(), "Reinforced save: " + reason + " / " + save.Status)) yield break;
            var corrupt = save.Capture(); corrupt.doorReinforcement = 1;
            if (!Require(!save.Validate(corrupt, out _), "Stage/HP mismatch rejected")) yield break;
            economy.Reinforcement.RestoreLevel(0); yield return save.LoadNow();
            if (!Require(save.Status == "Run resumed locally" && economy.Reinforcement.Level == 2 && AllHp(40), "Save restores stage/HP")) yield break;
            run.Restart();
            if (!Require(economy.Reinforcement.Level == 0 && run.CurrentWagon == run.Wagons[1] && AllHp(20) &&
                run.Wagons.All(w => w.Doors.All(d => d.Durability.Current == 20)), "New run resets stage and HP")) yield break;
            Finish(true, "Middle start; actual waves4 central /7 distributed; growth10/13 capped36; equal20HP; door-hit repair continuity; wood30/wire40 all-door ordered paid upgrades; no resurrection/collider changes; stage/HP save validation and restore; restart resets level/HP. PC only.");
        }
        private bool Counts(int a, int b, int c) => run.Wagons[0].Enemies.Active.Count == a && run.Wagons[1].Enemies.Active.Count == b && run.Wagons[2].Enemies.Active.Count == c;
        private bool AllHp(float hp) => run.Wagons.All(w => w.Doors.All(d => d.Durability.Maximum == hp));
        private static void Hit(HealthComponent target, float damage) => target.TryApplyDamage(new DamageContext(damage, default, Team.Enemy, 93001, target.LifeVersion));
        private bool Require(bool value, string reason) { if (!value) Finish(false, reason); return value; }
        private void Finish(bool pass, string detail)
        {
            string report = DateTime.UtcNow.ToString("O") + "\n" + (pass ? "PASS: " : "FAIL: ") + detail;
            Directory.CreateDirectory("Logs"); File.WriteAllText("Logs/SurvivalOnboardingAcceptance.txt", report);
            if (pass) Debug.Log("[SurvivalOnboarding] " + report); else Debug.LogError("[SurvivalOnboarding] " + report);
            run.ControlsAllowed = run.AimAllowed = true; run.Repair.enabled = true;
            run.Player.Invulnerable = false; spawner.SpawningAllowed = true;
            run.Restart(); save.UseTestStore(productionPath); checking = false;
        }
    }
}
