using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using WaitYourTurn.Combat;
using WaitYourTurn.Defenses;
using WaitYourTurn.Economy;
using WaitYourTurn.Enemies;
using WaitYourTurn.Navigation;
using WaitYourTurn.Player;
using WaitYourTurn.Run;

namespace WaitYourTurn.Sandbox
{
    /// <summary>PC acceptance fixture. Production drone never references this component.</summary>
    public sealed class DroneAcceptance : MonoBehaviour
    {
        [SerializeField] private RunDriver run;
        [SerializeField] private RunEconomy economy;
        [SerializeField] private StationSpawner spawner;
        [SerializeField] private RunDrone drone;
        private bool checking, restoreFixedSeed, restoreRandomInitial;
        private TrainIntegrationController hud;
        private ShopHud shopHud;
        private DroneDefinition original, fast;
        private WeaponDefinition fastWeapon;
        private int product;
        private string status;
        public void Configure(RunDriver owner, RunEconomy shop, StationSpawner spawn, RunDrone companion)
        { run = owner; economy = shop; spawner = spawn; drone = companion; }
        private void Update() { if (!checking && Input.GetKeyDown(KeyCode.F8)) StartCoroutine(Check()); }
        private void OnGUI() { if (checking) GUI.Label(new Rect(20, 120, Screen.width - 40, 40), "Drone check: " + status); }
        private IEnumerator Check()
        {
            checking = true; status = "door alignment / shop / glass shot";
            restoreFixedSeed = run.UseFixedSeed; restoreRandomInitial = run.RandomInitialWagon;
            run.ConfigureRandomAssignments(true, false);
            hud = FindAnyObjectByType<TrainIntegrationController>(); shopHud = FindAnyObjectByType<ShopHud>();
            if (hud != null) hud.enabled = false; if (shopHud != null) shopHud.enabled = false;
            run.ControlsAllowed = run.AimAllowed = false; run.Repair.enabled = false;
            run.Restart(new RunTimings { initialApproach = .2f, defense = 300 }); run.Player.Invulnerable = true;
            yield return new WaitForSeconds(1); spawner.enabled = false;
            foreach (var w in run.Wagons) w.Enemies.ClearAlive(); yield return null;
            economy.Wallet.Reset(5000);
            original = drone.Definition;
            product = System.Array.FindIndex(economy.Catalog.items, i => i.effect == ShopEffect.Drone);
            var actor = drone.Actor; var id = actor.GetEntityId(); var wagon = run.CurrentWagon;
            var profile = spawner.Programs[0].bands[0].profile;
            var door = wagon.Doors[0]; door.Durability.Invulnerable = true;
            if (!Assert(product >= 0 && original.Valid && wagon.SpawnOutside(0, profile), "Missing drone product/type or alignment spawn")) yield break;
            var enemy = wagon.Enemies.Active[0];
            float deadline = Time.realtimeSinceStartup + 10;
            while (enemy.State != EnemyState.AttackingDoor && Time.realtimeSinceStartup < deadline) yield return null;
            yield return new WaitForSeconds(.6f);
            Vector3 local = door.Portal.transform.InverseTransformPoint(enemy.transform.position);
            if (!Assert(enemy.State == EnemyState.AttackingDoor && Mathf.Abs(local.x) < .24f &&
                Vector3.Dot(enemy.transform.forward, door.Portal.transform.forward) > .95f, "First door attacker did not arrive centered and face the door")) yield break;
            enemy.SetPaused(true); enemy.Health.ResetForSpawn(1, Team.Enemy);
            run.Player.GetComponent<PlayerMotor>().Place(door.RepairPosition + Vector3.up * .05f); Physics.SyncTransforms();
            float doorHp = door.Durability.Current; int balance = economy.Wallet.Balance;
            if (!Assert(Buy() == PurchaseResult.Success && actor.Occupied && actor.Weapon.Rounds == 60 &&
                economy.Wallet.Balance == balance - economy.Catalog.items[product].price, "First drone purchase/ammo/cost failed")) yield break;
            deadline = Time.realtimeSinceStartup + 2;
            while (enemy.Health.IsAlive && Time.realtimeSinceStartup < deadline) yield return null;
            if (!Assert(!enemy.Health.IsAlive && actor.ShotsFired == 1 && actor.Weapon.Rounds == 59 &&
                door.Durability.Current == doorHp && !door.Portal.IsOpen && economy.Wallet.Balance == balance - economy.Catalog.items[product].price + profile.reward,
                "Actual drone shot through closed door glass / owner reward failed")) yield break;
            yield return Cooldown(); balance = economy.Wallet.Balance;
            if (!Assert(Buy() == PurchaseResult.Unavailable && economy.Wallet.Balance == balance, "Second drone purchase charged money")) yield break;
            door.Durability.Invulnerable = false;

            status = "smooth follow / dark pause / ten assignments";
            Vector3 start = actor.transform.position;
            run.Player.GetComponent<PlayerMotor>().Place(wagon.Geometry.SafePosition(0));
            yield return new WaitForSeconds(.9f);
            if (!Assert(Vector3.Distance(start, actor.transform.position) > .25f && wagon.Geometry.Contains(actor.transform.position), "Drone did not follow owner inside wagon")) yield break;
            // Buying/relocating next to any wall must never start the swept volume inside that wall.
            foreach (var corner in new[] { new Vector3(-5.4f, .05f, -1.7f), new Vector3(-5.4f, .05f, 1.7f),
                new Vector3(5.4f, .05f, -1.7f), new Vector3(5.4f, .05f, 1.7f) })
            {
                run.Player.GetComponent<PlayerMotor>().Place(wagon.transform.TransformPoint(corner)); actor.RelocateToOwner(); Physics.SyncTransforms();
                if (!Assert(new DroneFlight().Sweep(actor.transform.position, actor.transform.position, original.flightRadius, Physics.AllLayers, out _),
                    "Follow/relocation goal overlaps wall at wagon corner")) yield break;
            }
            run.Player.GetComponent<PlayerMotor>().Place(wagon.Geometry.SafePosition(0)); actor.RelocateToOwner();
            int rounds = actor.Weapon.Rounds; bool sawPause = false;
            deadline = Time.realtimeSinceStartup + 15;
            while (run.Assignments < 10 && !run.Flow.Terminal && Time.realtimeSinceStartup < deadline)
            {
                run.Flow.Tick(1000); yield return null;
                if (run.Flow.Paused)
                {
                    sawPause = true;
                    if (!Assert(actor.Weapon.Paused && Buy() == PurchaseResult.Closed, "Dark gate allowed drone fire/purchase")) yield break;
                    Vector3 at = actor.transform.position; float clock = Time.time;
                    yield return new WaitForSecondsRealtime(.03f);
                    if (!Assert(Time.time == clock && actor.transform.position == at, "Drone advanced during paused gameplay")) yield break;
                }
            }
            while (run.Flow.Paused && !run.Flow.Terminal) { run.Flow.Tick(1000); yield return null; }
            if (!Assert(sawPause && run.Assignments >= 10 && !run.Flow.Terminal && actor.GetEntityId() == id &&
                actor.Weapon.Rounds == rounds && actor.Occupied && run.CurrentWagon.Geometry.Contains(actor.transform.position), "Assignments reset ammo or lost/duplicated drone")) yield break;

            status = "finite ammo / reachable dive / one blast / actor reuse";
            actor.Clear(); fast = Instantiate(original); fastWeapon = Instantiate(original.weapon); fast.weapon = fastWeapon;
            fastWeapon.magazineSize = 1; fastWeapon.damage = 1; fastWeapon.fireInterval = .04f;
            fast.searchSeconds = 2; drone.ConfigureDefinition(fast);
            wagon = run.CurrentWagon;
            run.Player.GetComponent<PlayerMotor>().Place(wagon.Geometry.SafePosition(0)); yield return Cooldown();
            if (!Assert(Buy() == PurchaseResult.Success, "One-round fixture purchase failed")) yield break;
            uint life = actor.GetComponent<HealthComponent>().LifeVersion;
            var front = SpawnAt(wagon, run.Player.transform.position + Vector3.right * 2.3f, 1000);
            var side = SpawnAt(wagon, run.Player.transform.position + Vector3.right * 3.3f, 5);
            if (!Assert(front != null && side != null, "Blast fixture placement failed")) yield break;
            balance = economy.Wallet.Balance; int explosions = actor.Explosions;
            deadline = Time.realtimeSinceStartup + 4;
            while (actor.Occupied && Time.realtimeSinceStartup < deadline) yield return null;
            if (!Assert(!actor.Occupied && actor.ShotsFired == 1 && actor.Weapon.Rounds == 0 && actor.Explosions == explosions + 1 &&
                actor.BlastHits == 2 && front.Health.Current == 1000 - 1 - fast.blastDamage && !side.Health.IsAlive &&
                economy.Wallet.Balance == balance + profile.reward, "Single dive/blast/reward or finite lifetime failed")) yield break;
            wagon.Enemies.ClearAlive(); yield return null; yield return Cooldown();
            if (!Assert(Buy() == PurchaseResult.Success && actor.GetEntityId() == id && actor.Weapon.Rounds == 1 &&
                actor.GetComponent<HealthComponent>().LifeVersion != life, "Rebuy did not reuse/reset same drone")) yield break;
            front = SpawnAt(wagon, run.Player.transform.position + Vector3.right * 3, 1000);
            if (!Assert(front != null, "Lost-target fixture failed")) yield break;
            deadline = Time.realtimeSinceStartup + 1;
            while (actor.State == DroneState.Following && Time.realtimeSinceStartup < deadline) yield return null;
            if (!Assert(actor.State != DroneState.Following && actor.Occupied, "Lost-target fixture never exhausted ammo")) yield break;
            int lostTimeouts = actor.TimedOut; explosions = actor.Explosions;
            float remaining = actor.SearchRemaining;
            wagon.Enemies.ClearAlive(); yield return null;
            int assignment = run.Assignments;
            deadline = Time.realtimeSinceStartup + 3;
            while (run.Assignments == assignment && !run.Flow.Terminal && Time.realtimeSinceStartup < deadline)
            { run.Flow.Tick(1000); yield return null; }
            while (run.Flow.Paused && !run.Flow.Terminal) { run.Flow.Tick(1000); yield return null; }
            if (!Assert(actor.SearchRemaining <= remaining && actor.GetEntityId() == id, "Lost target/reassignment extended kamikaze deadline")) yield break;
            deadline = Time.realtimeSinceStartup + 3;
            while (actor.Occupied && Time.realtimeSinceStartup < deadline) yield return null;
            if (!Assert(!actor.Occupied && actor.TimedOut == lostTimeouts + 1 && actor.Explosions == explosions,
                "Lost target left an infinite dive or unearned explosion")) yield break;
            actor.Clear();

            status = "closed door stops flight / bounded search / death / restart";
            wagon = run.CurrentWagon;
            door = wagon.Doors[0]; door.TryRepair();
            run.Player.GetComponent<PlayerMotor>().Place(door.RepairPosition + Vector3.up * .05f); yield return Cooldown();
            if (!Assert(Buy() == PurchaseResult.Success && wagon.SpawnOutside(0, profile), "Closed-door fixture setup failed")) yield break;
            enemy = wagon.Enemies.Active[0]; enemy.SetPaused(true);
            if (!Assert(enemy.GetComponent<AgentMotor>().TryPlace(door.Portal.OutsideApproach), "Closed-door target placement failed")) yield break;
            enemy.Health.ResetForSpawn(1000, Team.Enemy); Physics.SyncTransforms();
            int timeouts = actor.TimedOut; explosions = actor.Explosions;
            deadline = Time.realtimeSinceStartup + 4;
            while (actor.Occupied && Time.realtimeSinceStartup < deadline) yield return null;
            if (!Assert(!actor.Occupied && actor.ShotsFired == 1 && actor.TimedOut == timeouts + 1 &&
                actor.Explosions == explosions && enemy.Health.Current == 999 && !door.Portal.IsOpen,
                "Drone flew through closed door or search never expired")) yield break;
            wagon.Enemies.ClearAlive(); yield return null; yield return Cooldown();
            if (!Assert(Buy() == PurchaseResult.Success, "Death fixture rebuy failed")) yield break;
            run.Player.Invulnerable = false; run.Player.SetDamageProtection(0);
            run.Player.TryApplyDamage(new DamageContext(10000, default, Team.Enemy, 2, run.Player.LifeVersion)); yield return null;
            if (!Assert(run.Flow.Terminal && !actor.Occupied && Buy() == PurchaseResult.Closed, "Death left active drone or open shop")) yield break;
            run.Restart();
            if (!Assert(!actor.Occupied && actor.GetEntityId() == id, "Restart retained or duplicated drone")) yield break;
            Finish(true, "First zombie centers/faces closed door; drone purchase costs once, 60 rounds, duplicate blocked; actual glass shot credits owner; smooth follow and four wall corners stay clear; dark pauses movement/clock/fire/shop; 10 assignments preserve same actor/ammo; one-round fixture dives, blasts two targets once and credits kill; rebuy resets life/ammo; lost target/reassignment preserve finite deadline and close without explosion; closed door allows bullet but blocks dive until configurable timeout; death/restart clear companion. PC only; balance/Android pending.");
        }
        private EnemyBrain SpawnAt(WagonRuntime wagon, Vector3 point, float hp)
        {
            if (!wagon.SpawnOutside(0, spawner.Programs[0].bands[0].profile)) return null;
            var enemy = wagon.Enemies.Active.Last(); enemy.SetPaused(true);
            if (!enemy.GetComponent<AgentMotor>().TryPlace(point)) return null;
            enemy.Health.ResetForSpawn(hp, Team.Enemy); Physics.SyncTransforms(); return enemy;
        }
        private PurchaseResult Buy() => economy.Buy(product, economy.Context, economy.NextRequest());
        private WaitForSecondsRealtime Cooldown() => new WaitForSecondsRealtime(economy.Catalog.purchaseCooldown + .04f);
        private bool Assert(bool pass, string detail) { if (!pass) Finish(false, detail); return pass; }
        private void Finish(bool pass, string detail)
        {
            Directory.CreateDirectory("Logs"); string message = (pass ? "PASS: " : "FAIL: ") + detail;
            File.WriteAllText("Logs/DroneAcceptance.txt", System.DateTime.UtcNow.ToString("O") + "\n" + message);
            if (pass) Debug.Log("[Drone] " + message); else Debug.LogError("[Drone] " + message);
            drone.Actor.Clear(); if (original != null) drone.ConfigureDefinition(original);
            if (fast != null) Destroy(fast); if (fastWeapon != null) Destroy(fastWeapon); original = fast = null; fastWeapon = null;
            foreach (var w in run.Wagons) foreach (var door in w.Doors) door.Durability.Invulnerable = false;
            spawner.enabled = true; run.ControlsAllowed = run.AimAllowed = true; run.Repair.enabled = true;
            run.ConfigureRandomAssignments(restoreFixedSeed, restoreRandomInitial); run.Restart();
            if (hud != null) hud.enabled = true; if (shopHud != null) shopHud.enabled = true; checking = false;
        }
    }
}
