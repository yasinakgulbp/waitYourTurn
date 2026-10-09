using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using WaitYourTurn.Combat;
using WaitYourTurn.Economy;
using WaitYourTurn.Enemies;
using WaitYourTurn.Player;
using WaitYourTurn.Run;

namespace WaitYourTurn.Sandbox
{
    public sealed class ExplosivesAcceptance : MonoBehaviour
    {
        [SerializeField] private RunDriver run;
        [SerializeField] private RunEconomy economy;
        [SerializeField] private StationSpawner spawner;
        [SerializeField] private RunPersistence save;
        [SerializeField] private TargetRegistry registry;
        private bool checking;
        private string productionPath;
        public void Configure(RunDriver flow, RunEconomy shop, StationSpawner waves, RunPersistence persistence, TargetRegistry targets)
        { run = flow; economy = shop; spawner = waves; save = persistence; registry = targets; }
        private bool Require(bool value, string detail) { if (!value) Finish(false, detail); return value; }
        public IEnumerator Check()
        {
            if (checking) yield break; checking = true; productionPath = save.SavePath;
            save.UseTestStore(Path.Combine(Application.persistentDataPath, "acceptance-only", "explosives.json"));
            run.Restart(); spawner.ResetSchedule(); spawner.SpawningAllowed = false;
            run.AimAllowed = run.ControlsAllowed = false; run.Repair.enabled = false;
            yield return null;
            if (!Require(economy.Wallet.Balance == economy.Catalog.startingCoins && Price("heal") == 150 && Price("smg") == 300 && Price("rifle") == 2500 &&
                Price("shotgun") == 1000 && Price("turret-normal") == 500 && Price("turret-advanced") == 900 &&
                Price("wood") == 600 && Price("launcher") == 6000 && run.Weapon.WeaponCount == 5, "Requested prices / catalog starting coins / 5 weapons")) yield break;
            var pool = run.Wagons[1].Enemies; while (pool.WarmOne()) { }
            Vector3 center = run.Wagons[1].transform.position;
            run.Player.GetComponent<PlayerMotor>().Place(center + Vector3.up * .05f);
            economy.RestoreWallet(12000, 0); // Fixture-only; production restarts with catalog starting coins.
            if (!Require(Buy("mine") == PurchaseResult.Success && economy.Mines.Actors.Count(m => m.Deployed) == 1, "Purchase small mine at feet")) yield break;
            var mine = economy.Mines.Actors.First(m => m.Deployed); mine.SetPaused(true); mine.Tick(1);
            if (!Require(mine.Deployed && run.Player.Current == run.Player.Maximum && mine.GetComponentsInChildren<Collider>().Length == 0,
                "Player safe / collider-free mine")) yield break;
            var intro = spawner.Programs[0].bands[0].profile;
            var enemy = pool.RestoreEnemy(center + new Vector3(.4f, .05f, 0), run.Wagons[1].Doors[0], intro, 1, 0);
            if (!Require(enemy != null, "Fixture enemy rent")) yield break;
            enemy.RestoreTravel(true); enemy.SetPaused(true); Physics.SyncTransforms();
            int coins = economy.Wallet.Balance; mine.Tick(.2f); mine.Tick(.2f);
            if (!Require(!enemy.Health.IsAlive && !mine.Deployed && economy.Wallet.Balance == coins + intro.reward && mine.Detonations == 1,
                "Enemy-only single blast and exactly one kill reward")) yield break;
            yield return null; economy.RestoreWallet(12000, 0);
            if (!Require(Buy("launcher") == PurchaseResult.Success && run.Weapon.EquippedIndex == 4 && run.Weapon.Rounds == 6 && run.Weapon.Reserve == 12,
                "Buy finite launcher / no premature permanent gate")) yield break;
            var a = pool.RestoreEnemy(center + new Vector3(2, .05f, 0), run.Wagons[1].Doors[0], intro, 1, 0);
            var b = pool.RestoreEnemy(center + new Vector3(2, .05f, .7f), run.Wagons[1].Doors[0], intro, 1, 1);
            if (!Require(a != null && b != null, "Cluster fixture navigation")) yield break;
            a.RestoreTravel(true); b.RestoreTravel(true); a.SetPaused(true); b.SetPaused(true); Physics.SyncTransforms();
            coins = economy.Wallet.Balance;
            if (!Require(run.Weapon.TryFire(a.transform.position + Vector3.up * .85f - run.Weapon.Muzzle) && a.Health.IsAlive,
                "Moving projectile, no instant hit")) yield break;
            run.Weapon.Projectiles.Tick(.4f);
            if (!Require(!a.Health.IsAlive && !b.Health.IsAlive && run.Weapon.Rounds == 5 && economy.Wallet.Balance == coins + 2 * intro.reward,
                "Launcher cluster blast / ammo / two distinct kill rewards")) yield break;
            yield return null; yield return new WaitForSeconds(.4f); economy.RestoreWallet(12000, 0);
            run.Player.GetComponent<PlayerMotor>().Place(center + new Vector3(-2, .05f, 0));
            if (!Require(Buy("mine") == PurchaseResult.Success, "Persistent mine purchase")) yield break;
            run.Weapon.Projectiles.Launch(4, run.Weapon.Muzzle, Vector3.right, 8, run.Player);
            var capture = save.Capture();
            if (!Require(save.Validate(capture, out string reason) && save.SaveNow(), "Live mine/projectile save: " + reason)) yield break;
            economy.Mines.Clear(); run.Weapon.Projectiles.Clear(); yield return save.LoadNow();
            if (!Require(save.Status == "Run resumed locally" && economy.Mines.Actors.Count(m => m.Deployed) == 1 &&
                run.Weapon.Projectiles.ActiveCount == 1 && run.Weapon.Rounds == 5, "Mine + projectile resume without extra ammo")) yield break;
            var corrupt = save.Capture(); corrupt.mines[0].slot = 99;
            if (!Require(!save.Validate(corrupt, out _), "Corrupt mine slot rejected")) yield break;
            run.Restart();
            if (!Require(economy.Wallet.Balance == economy.Catalog.startingCoins && !economy.Mines.Actors.Any(m => m.Occupied) && run.Weapon.Projectiles.ActiveCount == 0 &&
                !run.Weapon.IsOwned(4), "New run clears mines / projectiles / purchased launcher")) yield break;
            Finish(true, "Requested prices and catalog starting coins; enemy-only single mine blast and killcredit; collider-free placement; finite launcher projectile+cluster damage; persisted live mine+projectile restore; corrupt slot rejection; new-run reset. Shared implementations, Survival composition only. PC.");
        }
        private int Price(string id) => economy.Catalog.items.First(i => i.id == id).price;
        private PurchaseResult Buy(string id) => economy.Buy(Array.FindIndex(economy.Catalog.items, i => i.id == id), economy.Context, economy.NextRequest());
        private void Finish(bool success, string detail)
        {
            string report = DateTime.UtcNow.ToString("O") + "\n" + (success ? "PASS: " : "FAIL: ") + detail;
            Directory.CreateDirectory("Logs"); File.WriteAllText("Logs/ExplosivesAcceptance.txt", report);
            if (success) Debug.Log("[Explosives] " + report); else Debug.LogError("[Explosives] " + report);
            run.ControlsAllowed = run.AimAllowed = true; run.Repair.enabled = true; spawner.SpawningAllowed = true;
            run.Restart(); save.UseTestStore(productionPath); checking = false;
        }
    }
}
