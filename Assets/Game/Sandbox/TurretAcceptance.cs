using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using WaitYourTurn.Combat;
using WaitYourTurn.Defenses;
using WaitYourTurn.Economy;
using WaitYourTurn.Navigation;
using WaitYourTurn.Player;
using WaitYourTurn.Run;

namespace WaitYourTurn.Sandbox
{
    /// <summary>PC integration fixture, not a production dependency or a balance benchmark.</summary>
    public sealed class TurretAcceptance : MonoBehaviour
    {
        [SerializeField] private RunDriver run;
        [SerializeField] private RunEconomy economy;
        [SerializeField] private StationSpawner spawner;
        [SerializeField] private RunDefenses defenses;
        [SerializeField] private TargetRegistry registry;
        private bool checking;
        private TrainIntegrationController hud;
        private ShopHud shopHud;
        private TurretDefinition original, fast;
        private WeaponDefinition fastWeapon;
        private string status;
        public void Configure(RunDriver owner, RunEconomy shop, StationSpawner spawn, RunDefenses turrets, TargetRegistry targets)
        { run = owner; economy = shop; spawner = spawn; defenses = turrets; registry = targets; }
        private void Update() { if (!checking && Input.GetKeyDown(KeyCode.F12)) StartCoroutine(Check()); }
        private void OnGUI() { if (checking) GUI.Label(new Rect(20, 120, Screen.width - 40, 40), "Turret check: " + status); }
        private IEnumerator Check()
        {
            checking = true; status = "shop / live shot / capacity";
            hud = FindAnyObjectByType<TrainIntegrationController>(); shopHud = FindAnyObjectByType<ShopHud>();
            if (hud != null) hud.enabled = false; if (shopHud != null) shopHud.enabled = false;
            run.ControlsAllowed = run.AimAllowed = false; run.Repair.enabled = false;
            run.Restart(new RunTimings { initialApproach = .2f, defense = 300 }); run.Player.Invulnerable = true;
            yield return new WaitForSeconds(1); spawner.enabled = false;
            foreach (var w in run.Wagons) w.Enemies.ClearAlive();
            economy.Wallet.Reset(5000);
            var wagon = run.CurrentWagon; var rack = defenses.CurrentRack;
            var actorIds = defenses.Racks.SelectMany(r => r.Mounts).Select(m => m.Actor.GetEntityId()).ToArray();
            var mount = rack.Mounts[0]; var profile = spawner.Programs[0].bands[0].profile;
            int normal = System.Array.FindIndex(economy.Catalog.items, i => i.effect == ShopEffect.Turret && i.turretIndex == 0);
            int advanced = System.Array.FindIndex(economy.Catalog.items, i => i.effect == ShopEffect.Turret && i.turretIndex == 1);
            if (!CheckThat(normal >= 0 && advanced >= 0 && defenses.Definitions.All(d => d.Valid), "Missing valid turret products/types")) yield break;
            int balance = economy.Wallet.Balance;
            run.Player.GetComponent<PlayerMotor>().Place(wagon.Geometry.SafePosition(0));
            if (!CheckThat(Buy(normal) == PurchaseResult.Unavailable && economy.Wallet.Balance == balance, "Remote pad purchase charged money")) yield break;
            StandBeside(mount); balance = economy.Wallet.Balance;
            if (!CheckThat(Buy(normal) == PurchaseResult.Success && mount.Occupied && mount.Actor.Weapon.Rounds == 50 &&
                economy.Wallet.Balance == balance - economy.Catalog.items[normal].price, "Normal turret purchase/ammo/price failed")) yield break;
            yield return Cooldown(); balance = economy.Wallet.Balance;
            if (!CheckThat(Buy(normal) == PurchaseResult.Unavailable && economy.Wallet.Balance == balance, "Occupied mount charged money")) yield break;
            if (!CheckThat(wagon.SpawnOutside(0, profile), "Actual-shot target spawn failed")) yield break;
            var enemy = wagon.Enemies.Active[0]; enemy.SetPaused(true); enemy.Health.ResetForSpawn(1, Team.Enemy);
            float deadline = Time.realtimeSinceStartup + 2;
            while (enemy.Health.IsAlive && Time.realtimeSinceStartup < deadline) yield return null;
            if (!CheckThat(!enemy.Health.IsAlive && mount.Actor.ShotsFired == 1 && mount.Actor.Weapon.Rounds == 49 &&
                economy.Wallet.Balance == balance + profile.reward, "Glass shot or turret owner reward failed")) yield break;
            yield return null;
            StandBeside(rack.Mounts[1]);
            if (!CheckThat(Buy(normal) == PurchaseResult.Success, "Second normal turret rejected")) yield break;
            yield return Cooldown(); StandBeside(rack.Mounts[2]); balance = economy.Wallet.Balance;
            if (!CheckThat(Buy(normal) == PurchaseResult.Unavailable && economy.Wallet.Balance == balance &&
                rack.Count(defenses.Definitions[0]) == 2, "Per-wagon normal limit failed")) yield break;
            if (!CheckThat(Buy(advanced) == PurchaseResult.Success && rack.Mounts[2].Actor.Weapon.Rounds == 80, "Advanced turret purchase/ammo failed")) yield break;
            yield return Cooldown(); StandBeside(rack.Mounts[3]);
            if (!CheckThat(Buy(advanced) == PurchaseResult.Success && rack.Count(defenses.Definitions[1]) == 2, "Second advanced turret rejected")) yield break;
            yield return Cooldown(); StandBeside(rack.Mounts[Mathf.Min(4, rack.Mounts.Length - 1)]); balance = economy.Wallet.Balance;
            if (!CheckThat(Buy(advanced) == PurchaseResult.Unavailable && economy.Wallet.Balance == balance, "Advanced cap charged money")) yield break;

            status = "door approach around installed turret";
            // Keep physical turret geometry active while isolating entry movement from turret kills.
            defenses.enabled = false;
            wagon.Doors[0].Durability.TryApplyDamage(new DamageContext(1000, default, Team.Enemy, 1, wagon.Doors[0].Durability.LifeVersion));
            yield return new WaitForSeconds(.3f);
            for (int i = 0; i < 3; i++)
            {
                if (!CheckThat(wagon.Enemies.TrySpawn(wagon.Geometry.StationSpawn(0) + Vector3.right * (i - 1) * .8f, profile), "Entry fixture spawn failed")) yield break;
                wagon.Enemies.Active.Last().Health.Invulnerable = true;
            }
            deadline = Time.realtimeSinceStartup + 12;
            while (wagon.Enemies.Active.Any(e => !e.IsInside) && Time.realtimeSinceStartup < deadline) yield return null;
            if (!CheckThat(wagon.Enemies.Active.Count == 3 && wagon.Enemies.Active.All(e => e.IsInside), "Installed turret blocked doorway navigation")) yield break;
            wagon.Enemies.ClearAlive(); yield return null; defenses.enabled = true;

            status = "dark gate / ten assignments / old wagon reward";
            var actors = rack.Mounts.Where(m => m.Occupied).Select(m => m.Actor).ToArray();
            var rounds = actors.Select(a => a.Weapon.Rounds).ToArray();
            var positions = actors.Select(a => a.transform.position).ToArray();
            deadline = Time.realtimeSinceStartup + 15;
            while ((run.Assignments < 10 || (run.Wagons.Length > 1 && run.CurrentWagon == wagon)) && !run.Flow.Terminal && Time.realtimeSinceStartup < deadline)
            {
                run.Flow.Tick(1000); yield return null;
                if (run.Flow.Paused && actors.Any(a => !a.Weapon.Paused))
                { Finish(false, "Turret not paused in dark/gameplay gate"); yield break; }
            }
            while (run.Flow.Paused && !run.Flow.Terminal) { run.Flow.Tick(1000); yield return null; }
            if (!CheckThat(run.Assignments >= 10 && !run.Flow.Terminal && (run.Wagons.Length == 1 || !wagon.HasLivingDefender) && actors.Select((a, i) => a.Deployed &&
                a.Weapon.Rounds == rounds[i] && a.transform.position == positions[i]).All(x => x), "Turret state leaked across assignments")) yield break;
            // A new per-wagon cap must not turn into a run-wide cap after assignment.
            if (defenses.CurrentRack != rack)
            {
                StandBeside(defenses.CurrentRack.Mounts[0]); yield return Cooldown();
                if (!CheckThat(Buy(normal) == PurchaseResult.Success, "Other wagon inherited previous wagon limit")) yield break;
                defenses.CurrentRack.Clear();
            }
            if (!CheckThat(wagon.SpawnOutside(0, profile), "Retained target spawn failed")) yield break;
            enemy = wagon.Enemies.Active[0]; enemy.SetPaused(true);
            if (!CheckThat(enemy.GetComponent<AgentMotor>().TryPlace(mount.transform.position + Vector3.right * 1.2f + Vector3.up * .05f) &&
                enemy.RetainForTravel(enemy.transform.position), "Retained target placement failed")) yield break;
            enemy.Health.ResetForSpawn(1, Team.Enemy); balance = economy.Wallet.Balance;
            deadline = Time.realtimeSinceStartup + 2;
            while (enemy.Health.IsAlive && Time.realtimeSinceStartup < deadline) yield return null;
            if (!CheckThat(!enemy.Health.IsAlive && economy.Wallet.Balance == balance + profile.reward, "Old wagon turret lost owner reward")) yield break;
            yield return null;

            status = "finite ammo / one blast / same actor reuse";
            foreach (var r in defenses.Racks) r.Clear();
            original = defenses.Definitions[0]; fast = Instantiate(original); fastWeapon = Instantiate(original.weapon);
            fast.weapon = fastWeapon; fastWeapon.magazineSize = 3; fastWeapon.fireInterval = .04f; fastWeapon.damage = 1;
            defenses.Definitions[0] = fast;
            rack = defenses.CurrentRack; wagon = run.CurrentWagon; mount = rack.Mounts[0];
            StandBeside(mount); yield return Cooldown();
            if (!CheckThat(Buy(normal) == PurchaseResult.Success, "Finite-ammo fixture purchase failed")) yield break;
            var actor = mount.Actor; uint life = actor.GetComponent<HealthComponent>().LifeVersion;
            run.Player.GetComponent<PlayerMotor>().Place(wagon.Geometry.SafePosition(0));
            if (!CheckThat(wagon.SpawnOutside(0, profile) && wagon.SpawnOutside(1, profile), "Blast targets spawn failed")) yield break;
            var front = wagon.Enemies.Active[0]; var side = wagon.Enemies.Active[1];
            front.SetPaused(true); side.SetPaused(true);
            if (!CheckThat(front.GetComponent<AgentMotor>().TryPlace(mount.transform.position + Vector3.forward * .85f + Vector3.up * .05f) &&
                side.GetComponent<AgentMotor>().TryPlace(mount.transform.position + Vector3.left * 1.1f + Vector3.up * .05f), "Blast targets placement failed")) yield break;
            front.Health.ResetForSpawn(1000, Team.Enemy); side.Health.ResetForSpawn(5, Team.Enemy);
            balance = economy.Wallet.Balance; int exhaustions = actor.Exhaustions;
            deadline = Time.realtimeSinceStartup + 2;
            while (mount.Occupied && Time.realtimeSinceStartup < deadline) yield return null;
            if (!CheckThat(!mount.Occupied && actor.ShotsFired == 3 && actor.Weapon.Rounds == 0 && actor.BlastHits == 2 &&
                actor.Exhaustions == exhaustions + 1 && front.Health.Current == 1000 - 3 - fast.blastDamage && !side.Health.IsAlive &&
                economy.Wallet.Balance == balance + profile.reward, "Finite ammo / single area damage / reward failed")) yield break;
            yield return new WaitForSeconds(.5f);
            if (!CheckThat(actor.Exhaustions == exhaustions + 1 && front.Health.Current == 1000 - 3 - fast.blastDamage &&
                !actor.gameObject.activeSelf, "Exhaustion repeated damage or actor never returned")) yield break;
            wagon.Enemies.ClearAlive(); yield return null; StandBeside(mount);
            if (!CheckThat(Buy(normal) == PurchaseResult.Success && mount.Actor == actor && actor.Weapon.Rounds == 3 &&
                actor.ShotsFired == 0 && actor.GetComponent<HealthComponent>().LifeVersion != life, "Same slot actor did not reset cleanly")) yield break;
            run.Player.Invulnerable = false; run.Player.SetDamageProtection(0);
            run.Player.TryApplyDamage(new DamageContext(10000, default, Team.Enemy, 2, run.Player.LifeVersion));
            yield return null;
            if (!CheckThat(run.Flow.Terminal && actor.Weapon.Paused && Buy(normal) == PurchaseResult.Closed, "Death allows turret purchase/fire")) yield break;
            run.Restart();
            if (!CheckThat(defenses.Racks.All(r => r.Mounts.All(m => !m.Occupied)) && actorIds.SequenceEqual(
                defenses.Racks.SelectMany(r => r.Mounts).Select(m => m.Actor.GetEntityId())), "New run leaked deployments or grew actors")) yield break;
            Finish(true, $"Normal/advanced purchases use 50/80 finite rounds; occupied/remote/per-type caps reject without charge; actual glass shot credits owner; 3 zombies enter around installed geometry; dark gate and 10 assignments preserve mounts/ammo; old wagon owner reward; 3-shot fixture exhausts once, blasts two enemies once and credits kill; same actor redeploy resets life/ammo; death closes; Restart clears all {actorIds.Length} fixed mount actors. PC only; final balance and Android pending.");
        }
        private void StandBeside(TurretMount mount) => run.Player.GetComponent<PlayerMotor>().Place(mount.transform.position + Vector3.right * .85f + Vector3.up * .05f);
        private PurchaseResult Buy(int index) => economy.Buy(index, economy.Context, economy.NextRequest());
        private WaitForSecondsRealtime Cooldown() => new WaitForSecondsRealtime(economy.Catalog.purchaseCooldown + .03f);
        private bool CheckThat(bool pass, string detail) { if (!pass) Finish(false, detail); return pass; }
        private void Finish(bool pass, string detail)
        {
            Directory.CreateDirectory("Logs"); string message = (pass ? "PASS: " : "FAIL: ") + detail;
            File.WriteAllText("Logs/TurretAcceptance.txt", System.DateTime.UtcNow.ToString("O") + "\n" + message);
            if (pass) Debug.Log("[Turrets] " + message); else Debug.LogError("[Turrets] " + message);
            if (original != null) defenses.Definitions[0] = original;
            if (fast != null) Destroy(fast); if (fastWeapon != null) Destroy(fastWeapon);
            original = fast = null; fastWeapon = null;
            defenses.enabled = true; spawner.enabled = true; run.ControlsAllowed = run.AimAllowed = true;
            run.Repair.enabled = true; run.Restart();
            if (hud != null) hud.enabled = true; if (shopHud != null) shopHud.enabled = true; checking = false;
        }
    }
}
