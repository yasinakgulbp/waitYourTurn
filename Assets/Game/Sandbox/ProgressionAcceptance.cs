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
    /// <summary>Short combined check, isolated run/profile files, no production currency changes.</summary>
    public sealed class ProgressionAcceptance : MonoBehaviour
    {
        [SerializeField] private RunDriver run;
        [SerializeField] private RunEconomy economy;
        [SerializeField] private RunPersistence save;
        [SerializeField] private RunProfile profile;
        [SerializeField] private StationSpawner spawner;
        private bool checking;
        public void Configure(RunDriver owner, RunEconomy shop, RunPersistence persistence, RunProfile permanent, StationSpawner waves)
        { run = owner; economy = shop; save = persistence; profile = permanent; spawner = waves; }
        public IEnumerator Check()
        {
            if (checking) yield break;
            checking = true;
            string runPath = save.SavePath, profilePath = profile.ProfilePath;
            var views = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude)
                .Where(v => v != this && (v.GetType().Name.EndsWith("Acceptance") || v is TrainIntegrationController || v is ProfileHud)).Cast<Behaviour>().ToArray();
            bool[] enabledViews = views.Select(v => v.enabled).ToArray();
            bool controls = run.ControlsAllowed, aim = run.AimAllowed, repair = run.Repair.enabled, spawning = spawner.SpawningAllowed, ai = run.Match.AiAllowed;
            string folder = Path.Combine(Application.persistentDataPath, "acceptance-only", "progression-" + Guid.NewGuid().ToString("N"));
            var motor = run.Player.GetComponent<PlayerMotor>();
            var report = ""; bool pass = false;
            try
            {
                foreach (var view in views) view.enabled = false;
                save.UseTestStore(Path.Combine(folder, "run.json")); profile.UseStore(Path.Combine(folder, "profile.json"), true);
                spawner.SpawningAllowed = false; run.Match.AiAllowed = false;
                run.ControlsAllowed = run.AimAllowed = false; run.Repair.enabled = false;
                run.Match.SetMode(RunMode.Solo); yield return null;
                Require(!profile.WeaponUnlocked(1), "New profile must start with pistol only");
                var gunProduct = economy.Catalog.items.First(x => x.Product.Effect == ShopEffect.Weapon && x.Product.WeaponIndex == 1).Product;
                Require(!economy.CanApply(gunProduct), "Locked weapon shop gate");
                Require(run.Weapon.TryGrantOrRefill(1) && run.Weapon.StateAt(1).TryFire(Time.time), "Fixture ammunition consumption");
                int equipped = run.Weapon.EquippedIndex;
                motor.Place(run.Solo.Connections[0].transform.position + Vector3.left * 1.5f + Vector3.up * .05f);
                Require(run.Solo.TryUnlock(0) && run.Solo.TryToggle(0), "Paid first wagon");
                // Existing continuous-walk suite proved the passage; here check the new pickup only.
                motor.Place(run.Wagons[1].transform.TransformPoint(run.Loot.LocalPosition)); yield return null;
                Require(run.CurrentWagon == run.Wagons[1] && run.Loot.Capture()[1] && !run.Loot.TryCollect(1) &&
                    !run.Weapon.StateAt(1).NeedsRefill && !run.Weapon.IsOwned(2) && run.Weapon.EquippedIndex == equipped, "One pickup / owned ammo only / equipped weapon");
                motor.Place(run.Solo.Connections[1].transform.position + Vector3.left * 1.5f + Vector3.up * .05f);
                Require(run.Solo.TryUnlock(1) && run.Solo.TryToggle(1), "Second wagon unlock");
                motor.Place(run.Wagons[2].transform.TransformPoint(run.Loot.LocalPosition)); yield return null;
                Require(!run.Loot.Capture()[2] && !run.Loot.TryCollect(2), "Full ammo must preserve crate");
                Require(save.SaveNow(), "Save loot: " + save.Status);
                yield return save.LoadNow();
                Require(save.Status == "Run resumed locally" && run.Loot.Capture()[1] && !run.Loot.Capture()[2] &&
                    !run.Weapon.StateAt(1).NeedsRefill, "Loot and ammo JSON restore");
                Kill(run.Player); yield return null; yield return null;
                int tokens = profile.Tokens;
                Require(tokens > 0 && profile.BestSoloStation == 1 && !profile.TrySettle() && profile.Tokens == tokens &&
                    new LocalProfileStore(profile.ProfilePath).Load().tokens == tokens && !save.SaveNow(), "Solo death / once-only durable result / no dead resume");
                string id = save.RunId;
                Require(!new LocalProfileStore(profile.ProfilePath).Load().TryReward(id, 99, 1), "Receipt survives file reload");
                run.Restart(); yield return null;
                Require(run.Flow.Station == 1 && run.CurrentWagon == run.Wagons[0] && run.Solo.UnlockedThrough == 0 &&
                    run.Loot.Capture().All(x => !x) && !run.Weapon.IsOwned(1) && profile.Tokens == tokens &&
                    economy.Wallet.Balance == economy.Catalog.startingCoins && new LocalRunStore(save.SavePath).Load() == null, "Death means fresh challenge, permanent wallet retained");
                run.Match.SetMode(RunMode.Battle); yield return null;
                foreach (var bot in run.Match.Bots) Kill(bot.Health);
                yield return null; yield return null;
                Require(run.Match.HumanPlace == 1 && profile.Tokens > tokens && !profile.TrySettle(), "Battle rank reward once");
                tokens = profile.Tokens;
                Require(!profile.TryUnlock(3) && profile.Tokens == tokens, "Insufficient permanent funds");
                Require(profile.TryUnlock(1) && profile.Tokens == tokens - profile.WeaponPrice(1), "Permanent unlock transaction");
                tokens = profile.Tokens;
                Require(!profile.TryUnlock(1) && profile.Tokens == tokens && new LocalProfileStore(profile.ProfilePath).Load().weapons[1], "No duplicate debit / unlock durable");
                run.Restart(); yield return null;
                Require(profile.WeaponUnlocked(1) && !run.Weapon.IsOwned(1) && economy.CanApply(gunProduct), "Battle unlock does not grant run gun");
                int item = Array.FindIndex(economy.Catalog.items, x => x.Product.Effect == ShopEffect.Weapon && x.Product.WeaponIndex == 1);
                Require(economy.Buy(item, economy.Context, economy.NextRequest()) == PurchaseResult.Success && run.Weapon.IsOwned(1) &&
                    profile.Tokens == tokens, "Run purchase spends run coins only");
                run.Match.SetMode(RunMode.Solo); yield return null;
                Require(profile.WeaponUnlocked(1) && economy.CanApply(gunProduct) && !run.Weapon.IsOwned(1), "Permanent unlock shared by Solo");
                var candidate = new PlayerProfile(); string receipt = Guid.NewGuid().ToString("N");
                Require(candidate.TryReward(receipt, 7, 2), "Profile transaction candidate");
                Directory.CreateDirectory(Path.Combine(folder, "blocked.json"));
                Require(!new LocalProfileStore(Path.Combine(folder, "blocked.json")).Save(candidate), "IO failure must report failure");
                report = "Owned ammo once; full crate retained; loot/ammo JSON resume; Solo death fresh rear/station 1 with reset coins/guns/loot; permanent wallet retained; Solo/Battle result once with durable receipt; insufficient/duplicate unlock no debit; unlock shared by modes, run purchase separate; IO failure reported. Test profile isolated from production.";
                pass = true;
            }
            finally
            {
                // finally also restores controls if a critical assertion interrupts this short fixture.
                profile.UseStore(profilePath, false); save.UseTestStore(runPath);
                run.SetLoadingGate(false); run.ControlsAllowed = controls; run.AimAllowed = aim; run.Repair.enabled = repair;
                spawner.SpawningAllowed = spawning; run.Match.AiAllowed = ai; run.Player.Invulnerable = false;
                run.Match.SetMode(RunMode.Battle);
                for (int i = 0; i < views.Length; i++) views[i].enabled = enabledViews[i]; checking = false;
                Directory.CreateDirectory("Logs"); File.WriteAllText("Logs/ProgressionAcceptance.txt", DateTime.UtcNow.ToString("O") + "\n" + (pass ? "PASS: " : "FAIL: ") + report);
                if (pass) Debug.Log("[ProgressionAcceptance] PASS: " + report);
            }
        }
        private static void Kill(HealthComponent body)
        {
            body.Invulnerable = false; body.SetDamageProtection(0);
            body.TryApplyDamage(new DamageContext(body.Maximum, new EntityIdentity(99999, 1), Team.Enemy, 1, body.LifeVersion));
        }
        private static void Require(bool condition, string message)
        { if (!condition) throw new InvalidOperationException("[ProgressionAcceptance] " + message); }
    }
}
