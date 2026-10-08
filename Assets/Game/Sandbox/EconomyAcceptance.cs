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
    /// <summary>PC-only integration fixture. F9; production economy has no dependency on this component.</summary>
    public sealed class EconomyAcceptance : MonoBehaviour
    {
        [SerializeField] private RunDriver run;
        [SerializeField] private RunEconomy economy;
        [SerializeField] private StationSpawner spawner;
        private bool checking;
        private Behaviour[] views;
        private bool[] enabledViews;
        public void Configure(RunDriver owner, RunEconomy currency, StationSpawner director)
        { run = owner; economy = currency; spawner = director; }
        private void Update() { if (!checking && Input.GetKeyDown(KeyCode.F9)) StartCoroutine(Check()); }
        private IEnumerator Check()
        {
            run.Match?.SetSuppressed(true); checking = true;
            views = new Behaviour[] { FindAnyObjectByType<ShopHud>(), FindAnyObjectByType<TrainIntegrationController>(), FindAnyObjectByType<SpawnAcceptance>() };
            enabledViews = views.Select(v => v != null && v.enabled).ToArray();
            foreach (var view in views) if (view != null) view.enabled = false;
            run.ControlsAllowed = run.AimAllowed = false; run.Repair.enabled = false;
            run.Restart(new RunTimings { initialApproach = .2f, defense = 300 }); run.Player.Invulnerable = true;
            yield return new WaitForSeconds(1);
            spawner.enabled = false;
            foreach (var wagon in run.Wagons) wagon.Enemies.ClearAlive();
            var current = run.CurrentWagon; var other = run.Wagons.First(w => w != current);
            var profile = spawner.Programs[0].bands[0].profile;
            var player = run.Player; var gun = run.Weapon; var motor = player.GetComponent<PlayerMotor>();
            int heal = Index(ShopEffect.Heal), repair = Index(ShopEffect.RepairWagon), weapon = Index(ShopEffect.Weapon);
            if (!CheckThat(heal >= 0 && repair >= 0 && weapon >= 0 && economy.CanShop && gun.StartingWeaponOnly && !gun.IsOwned(1), "Missing initial shop/inventory")) yield break;
            int starting = economy.Wallet.Balance;
            if (!CheckThat(current.SpawnOutside(0, profile), "Cannot spawn reward fixture")) yield break;
            var enemy = current.Enemies.Active[0]; enemy.SetPaused(true);
            enemy.Health.ResetForSpawn(1, Team.Enemy);
            var identity = enemy.Health.Identity;
            motor.Place(current.Doors[0].RepairPosition + Vector3.up * .05f); Physics.SyncTransforms();
            if (!CheckThat(gun.HasSight(enemy.Health) && gun.TryFire(enemy.transform.position + Vector3.up * .85f - gun.Muzzle) && !enemy.Health.IsAlive &&
                economy.Wallet.Balance == starting + profile.reward, "Real pistol kill did not credit exactly one reward")) yield break;
            enemy.Health.TryApplyDamage(new DamageContext(1000, player.Identity, Team.Player, 100, identity.LifeVersion));
            yield return null;
            if (!CheckThat(economy.Wallet.Balance == starting + profile.reward && current.Enemies.Active.Count == 0, "Dead/return callback duplicated reward")) yield break;
            if (!CheckThat(current.SpawnOutside(0, profile), "Cannot reuse reward fixture")) yield break;
            enemy = current.Enemies.Active[0]; enemy.SetPaused(true);
            if (!CheckThat(enemy.Health.Identity.RuntimeId == identity.RuntimeId && enemy.Health.LifeVersion != identity.LifeVersion, "Reward fixture was not same pooled actor/new life")) yield break;
            enemy.Health.TryApplyDamage(new DamageContext(10000, new EntityIdentity(600, 1), Team.Player, 101, enemy.Health.LifeVersion, rewardOwner: player.Identity));
            if (!CheckThat(economy.Wallet.Balance == starting + profile.reward * 2, "Owned defense kill not credited")) yield break;
            yield return null;
            if (!CheckThat(other.SpawnOutside(0, profile), "Cannot spawn other wagon")) yield break;
            var foreign = other.Enemies.Active[0]; foreign.SetPaused(true);
            foreign.Health.TryApplyDamage(new DamageContext(10000, new EntityIdentity(601, 1), Team.Player, 102, foreign.Health.LifeVersion));
            yield return null;
            if (!CheckThat(economy.Wallet.Balance == starting + profile.reward * 2, "Foreign owner got player currency")) yield break;
            other.SpawnOutside(0, profile); other.Enemies.ClearAlive();
            if (!CheckThat(economy.Wallet.Balance == starting + profile.reward * 2, "Station cleanup earned currency")) yield break;

            economy.Wallet.Reset(1000);
            int before = economy.Wallet.Balance;
            if (!CheckThat(Buy(heal) == PurchaseResult.Unavailable && economy.Wallet.Balance == before, "Full health charged currency")) yield break;
            player.Invulnerable = false; player.SetDamageProtection(0);
            player.TryApplyDamage(new DamageContext(90, default, Team.Enemy, 103, player.LifeVersion)); player.Invulnerable = true;
            ulong request = economy.NextRequest(); ulong context = economy.Context;
            if (!CheckThat(economy.Buy(heal, context, request) == PurchaseResult.Success && player.Current == 40 &&
                economy.Wallet.Balance == before - economy.Catalog.items[heal].price, "10/100 shop heal failed")) yield break;
            int paid = economy.Wallet.Balance;
            economy.Buy(heal, context, request); economy.Buy(heal, context, economy.NextRequest());
            if (!CheckThat(player.Current == 40 && economy.Wallet.Balance == paid, "Double input charged twice")) yield break;
            yield return Cooldown();
            if (!CheckThat(economy.Buy(heal, context, request) == PurchaseResult.Duplicate && economy.Wallet.Balance == paid, "Old command replayed")) yield break;
            player.TryHeal(10);
            if (!CheckThat(Buy(heal) == PurchaseResult.Success && player.Current == 80, "50/100 shop heal failed")) yield break;
            yield return Cooldown();
            if (!CheckThat(Buy(heal) == PurchaseResult.Success && player.Current == 100, "Heal exceeds max or failed at 80")) yield break;
            yield return Cooldown();
            economy.Wallet.Reset(0);
            if (!CheckThat(Buy(weapon) == PurchaseResult.InsufficientFunds && !gun.IsOwned(1) && economy.Wallet.Balance == 0, "Unfunded weapon granted")) yield break;
            economy.Wallet.Reset(1000);
            var localDoor = current.Doors[0]; var remoteDoor = other.Doors[0];
            localDoor.Durability.TryApplyDamage(new DamageContext(10000, default, Team.Enemy, 104, localDoor.Durability.LifeVersion));
            remoteDoor.Durability.TryApplyDamage(new DamageContext(1, default, Team.Enemy, 105, remoteDoor.Durability.LifeVersion));
            float remoteHealth = remoteDoor.Durability.Current;
            before = economy.Wallet.Balance;
            if (!CheckThat(Buy(repair) == PurchaseResult.Success && current.Doors.All(d => !d.NeedsRepair) && remoteDoor.Durability.Current == remoteHealth &&
                economy.Wallet.Balance == before - economy.Catalog.items[repair].price, "Wagon repair wrong scope or price")) yield break;
            yield return Cooldown();
            before = economy.Wallet.Balance;
            if (!CheckThat(Buy(repair) == PurchaseResult.Unavailable && economy.Wallet.Balance == before, "Healthy doors charged money")) yield break;
            if (!CheckThat(Buy(weapon) == PurchaseResult.Success && gun.IsOwned(1) && gun.EquippedIndex == 1, "Weapon purchase/unlock failed")) yield break;
            yield return Cooldown();
            before = economy.Wallet.Balance;
            if (!CheckThat(Buy(weapon) == PurchaseResult.Unavailable && economy.Wallet.Balance == before, "Full ammunition charged money")) yield break;
            gun.TryFire(Vector3.up);
            if (!CheckThat(Buy(weapon) == PurchaseResult.Success && !gun.State.NeedsRefill && !gun.TryFire(Vector3.up), "Ammo refill bypasses interval or did not refill")) yield break;
            yield return new WaitForSeconds(gun.State.Spec.Interval + .03f);
            if (!CheckThat(gun.TryFire(Vector3.up), "Cannot spend purchased ammunition")) yield break;
            int rounds = gun.Rounds, reserve = gun.Reserve; paid = economy.Wallet.Balance;
            ulong oldContext = economy.Context;
            var positions = run.Wagons.Select(w => w.Geometry.StationSpawn(0)).ToArray();
            // Keep ownership/ammo/currency through repeated random assignments; no scene reload.
            run.Flow.Tick(1000); // Defense -> warning
            float deadline = Time.realtimeSinceStartup + 40;
            // Accelerate only the pure phase clock; normal Update still owns assignment and gate operations.
            while (run.Assignments < 10 && !run.Flow.Terminal && Time.realtimeSinceStartup < deadline)
            {
                run.Flow.Tick(1000);
                yield return null;
                if (run.Flow.Paused && economy.Buy(heal, economy.Context, economy.NextRequest()) != PurchaseResult.Closed)
                { Finish(false, "Shop accepted during dark transition"); yield break; }
            }
            while (run.Flow.Paused && !run.Flow.Terminal) { run.Flow.Tick(1000); yield return null; }
            if (!CheckThat(!run.Flow.Terminal && run.Assignments == 10 && economy.Wallet.Balance == paid && gun.IsOwned(1) &&
                gun.Rounds == rounds && gun.Reserve == reserve && run.Wagons.Select((w, i) => w.Geometry.StationSpawn(0) == positions[i]).All(x => x), "Currency/inventory/ammo changed across 10 assignments")) yield break;
            if (!CheckThat(economy.Buy(repair, oldContext, economy.NextRequest()) == PurchaseResult.StaleContext, "Old wagon shop request accepted")) yield break;
            player.Invulnerable = false; player.SetDamageProtection(0);
            player.TryApplyDamage(new DamageContext(10000, default, Team.Enemy, 106, player.LifeVersion));
            if (!CheckThat(!economy.CanShop && Buy(heal) == PurchaseResult.Closed && economy.Wallet.Balance == paid, "Death allows shop/resurrection")) yield break;
            run.Restart();
            if (!CheckThat(economy.Wallet.Balance == economy.Catalog.startingCoins && gun.IsOwned(0) && !gun.IsOwned(1) && economy.TrackedActors == 0,
                "New run leaked currency, inventory or reward history")) yield break;
            Finish(true, "Actual pistol kill + owned-defense credit; pooled life/replay/foreign-owner/cleanup; heal 10->40,50->80,80->100; full/unfunded/duplicate reject; current-wagon repair; weapon unlock/refill/cooldown; 10 assignments preserve currency/inventory/ammo; dark/death/old context reject; new run resets. PC only.");
        }
        private int Index(ShopEffect effect) => System.Array.FindIndex(economy.Catalog.items, p => p.effect == effect);
        private PurchaseResult Buy(int index) => economy.Buy(index, economy.Context, economy.NextRequest());
        private WaitForSecondsRealtime Cooldown() => new WaitForSecondsRealtime(economy.Catalog.purchaseCooldown + .03f);
        private bool CheckThat(bool condition, string failure) { if (!condition) Finish(false, failure); return condition; }
        private void Finish(bool pass, string detail)
        {
            Directory.CreateDirectory("Logs");
            string message = (pass ? "PASS: " : "FAIL: ") + detail;
            File.WriteAllText("Logs/EconomyAcceptance.txt", System.DateTime.UtcNow.ToString("O") + "\n" + message);
            if (pass) Debug.Log("[Economy] " + message); else Debug.LogError("[Economy] " + message);
            checking = false;
            if (views != null) for (int i = 0; i < views.Length; i++) if (views[i] != null) views[i].enabled = enabledViews[i];
            spawner.enabled = true; run.Repair.enabled = true; run.ControlsAllowed = run.AimAllowed = true; run.Match?.SetSuppressed(false); run.Restart();
        }
    }
}
