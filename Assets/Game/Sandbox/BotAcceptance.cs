using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using WaitYourTurn.Combat;
using WaitYourTurn.Economy;
using WaitYourTurn.Enemies;
using WaitYourTurn.Navigation;
using WaitYourTurn.Player;
using WaitYourTurn.Run;

namespace WaitYourTurn.Sandbox
{
    /// <summary>F7 integration checks. No editor callbacks or production dependencies on this fixture.</summary>
    public sealed class BotAcceptance : MonoBehaviour
    {
        [SerializeField] private RunDriver run;
        [SerializeField] private RunMatch match;
        [SerializeField] private RunEconomy economy;
        [SerializeField] private StationSpawner spawner;
        private bool checking, restoreFixed, restoreInitial;
        private RunMode restoreMode;
        private Behaviour[] views;
        private bool[] viewEnabled;
        private string status;
        public void Configure(RunDriver owner, RunMatch participants, RunEconomy shop, StationSpawner spawn)
        { run = owner; match = participants; economy = shop; spawner = spawn; }
        private void Update() { if (!checking && Input.GetKeyDown(KeyCode.F7)) StartCoroutine(Check()); }
        private void OnGUI() { if (checking) GUI.Box(new Rect(20, 80, 600, 45), "BOT CHECK: " + status); }
        private IEnumerator Check()
        {
            checking = true; restoreFixed = run.UseFixedSeed; restoreInitial = run.RandomInitialWagon; restoreMode = match.Mode;
            views = new Behaviour[] { FindAnyObjectByType<TrainIntegrationController>(), FindAnyObjectByType<ShopHud>(), FindAnyObjectByType<BattleHud>(),
                FindAnyObjectByType<SpawnAcceptance>(), FindAnyObjectByType<EconomyAcceptance>(), FindAnyObjectByType<TurretAcceptance>(), FindAnyObjectByType<DroneAcceptance>(),
                FindAnyObjectByType<RunPersistence>(), FindAnyObjectByType<PersistenceHud>() };
            viewEnabled = views.Select(v => v != null && v.enabled).ToArray(); foreach (var view in views) if (view != null) view.enabled = false;
            run.ControlsAllowed = run.AimAllowed = false; run.Repair.enabled = false; spawner.enabled = false; match.AiAllowed = false;
            run.ConfigureRandomAssignments(true, false); run.ConfigureInitialWagon(2); match.SetMode(RunMode.Battle);
            run.Restart(new RunTimings { initialApproach = 300, defense = 300 }); yield return null;
            status = "unique assignments / skill profiles / separate economy";
            if (!Assert(match.Bots.Length == 4 && match.Bots.Select(b => b.Profile).Distinct().Count() == 3 && Unique(), "Participant/profile/occupancy count failed")) yield break;
            var bot = match.Bots[0]; var wagon = bot.Wagon; var door = wagon.Doors[0];
            var hp = run.Player.Current; int humanCoins = economy.Wallet.Balance; int coins = bot.Wallet.Balance;
            var profile = spawner.Programs[0].bands[0].profile;
            status = "bot gun through glass / owner-only kill reward / metal obstruction";
            bot.Motor.Place(door.RepairPosition + Vector3.up * .05f); Physics.SyncTransforms();
            if (!Assert(wagon.SpawnOutside(0, profile), "Bot reward fixture failed to spawn")) yield break;
            var enemy = wagon.Enemies.Active.Last(); enemy.SetPaused(true); enemy.Health.ResetForSpawn(1, Team.Enemy);
            enemy.GetComponent<AgentMotor>().TryPlace(door.Portal.OutsideApproach); Physics.SyncTransforms();
            bot.Weapon.Paused = false;
            bool shot = bot.Weapon.HasSight(enemy.Health) && bot.Weapon.TryFire(enemy.transform.position + Vector3.up * .85f - bot.Weapon.Muzzle);
            bot.Weapon.Paused = true;
            // Reward accepting is tied to gameplay, not whether decision AI is enabled for the fixture.
            if (!Assert(shot && !enemy.Health.IsAlive && bot.Wallet.Balance == coins + profile.reward && economy.Wallet.Balance == humanCoins, "Bot glass kill or isolated reward failed")) yield break;
            yield return null;
            if (!Assert(wagon.SpawnOutside(0, profile), "Metal fixture spawn failed")) yield break;
            enemy = wagon.Enemies.Active.Last(); enemy.SetPaused(true);
            enemy.GetComponent<AgentMotor>().TryPlace(wagon.transform.TransformPoint(new Vector3(-5.4f, 0, -4)));
            bot.Motor.Place(wagon.transform.TransformPoint(new Vector3(-5.4f, .05f, 0))); Physics.SyncTransforms();
            if (!Assert(!bot.Weapon.HasSight(enemy.Health), "Bot aim saw through solid end bay")) yield break;
            wagon.Enemies.ClearAlive(); yield return null;
            status = "real enemy pursues assigned bot rather than remote human";
            bot.Motor.Place(wagon.transform.position + Vector3.up * .05f);
            if (!Assert(wagon.SpawnOutside(0, profile), "Pursuit fixture spawn failed")) yield break;
            enemy = wagon.Enemies.Active.Last(); enemy.GetComponent<AgentMotor>().TryPlace(wagon.transform.position + new Vector3(2.5f, 0, 0));
            enemy.SetPaused(false); bot.Health.Invulnerable = false; bot.Health.SetDamageProtection(0);
            float before = bot.Health.Current; float deadline = Time.realtimeSinceStartup + 6;
            while (bot.Health.Current == before && Time.realtimeSinceStartup < deadline) yield return null;
            if (!Assert(bot.Health.Current < before && run.Player.Current == hp && enemy.State == EnemyState.AttackingPlayer, "Enemy did not attack its bot defender")) yield break;
            wagon.Enemies.ClearAlive(); yield return null;
            status = "same repair/damage/shop rules / actual AI movement and repair";
            bot.Motor.Place(door.RepairPosition + Vector3.up * .05f); bot.Repair.SetDoor(door); bot.Repair.Paused = false;
            door.Durability.TryApplyDamage(new DamageContext(door.Durability.Maximum * .6f, default, Team.Enemy, 1, door.Durability.LifeVersion));
            bot.Repair.Cancel(); bot.Repair.Tick(1);
            bot.Health.TryApplyDamage(new DamageContext(1, default, Team.Enemy, 2, bot.Health.LifeVersion)); bot.Repair.Tick(.1f);
            if (!Assert(bot.Repair.Progress > .3f, "Actor damage interrupted default bot repair")) yield break;
            door.Durability.TryApplyDamage(new DamageContext(1, default, Team.Enemy, 3, door.Durability.LifeVersion)); bot.Repair.Tick(.1f);
            if (!Assert(bot.Repair.Progress == 0, "Door damage failed to interrupt bot repair")) yield break;
            bot.Repair.Paused = true; bot.Motor.Place(wagon.transform.position + Vector3.up * .05f);
            match.AiAllowed = true;
            deadline = Time.realtimeSinceStartup + 8;
            while (door.NeedsRepair && Time.realtimeSinceStartup < deadline) yield return null;
            if (!Assert(!door.NeedsRepair && (bot.transform.position - wagon.transform.position).sqrMagnitude > .2f && bot.Purchases > 0 && bot.Weapon.IsOwned(bot.Profile.preferredWeapon), "Bot did not move/repair/buy with real components")) yield break;
            int shotsBefore = bot.Shots; coins = bot.Wallet.Balance;
            if (!Assert(wagon.SpawnOutside(0, profile), "Automatic fire fixture failed")) yield break;
            enemy = wagon.Enemies.Active.Last(); enemy.SetPaused(true); enemy.Health.ResetForSpawn(1, Team.Enemy);
            enemy.GetComponent<AgentMotor>().TryPlace(door.Portal.OutsideApproach); Physics.SyncTransforms();
            deadline = Time.realtimeSinceStartup + 4;
            while (enemy.Health.IsAlive && Time.realtimeSinceStartup < deadline) yield return null;
            if (!Assert(!enemy.Health.IsAlive && bot.Shots > shotsBefore && bot.Wallet.Balance >= coins + profile.reward, "Bot auto aim/fire did not kill or credit owner")) yield break;
            wagon.Enemies.ClearAlive(); yield return null;
            match.AiAllowed = false; yield return null;
            bot.Health.TryApplyDamage(new DamageContext(10000, default, Team.Enemy, 4, bot.Health.LifeVersion)); yield return null;
            if (!Assert(!wagon.HasLivingDefender && match.Roster.Place(1) == 5 && !bot.gameObject.activeSelf, "Elimination failed to release defender or rank")) yield break;
            // Preserve a passenger in the vacated wagon, then prove the scheduler does not add attackers there.
            var survivorOwner = run.Player;
            wagon.SetDefender(survivorOwner);
            if (!Assert(wagon.SpawnOutside(0, profile), "Survivor fixture spawn failed")) yield break;
            var survivor = wagon.Enemies.Active.Last(); survivor.SetPaused(true); survivor.GetComponent<AgentMotor>().TryPlace(wagon.Geometry.SafePosition(3));
            wagon.SetDefender(null); survivor.Health.Invulnerable = true;
            run.Flow.Tick(1000); spawner.enabled = true; run.Player.Invulnerable = true;
            foreach (var livingBot in match.Bots) if (livingBot.Health.IsAlive) livingBot.Health.Invulnerable = true;
            yield return new WaitForSeconds(6);
            if (!Assert(wagon.Enemies.Active.Count == 1 && wagon.Enemies.Active[0] == survivor && run.Wagons.Where(w => w.HasLivingDefender).Any(w => w.Enemies.Active.Count > 0), "Empty wagon accumulated new zombies or occupied wagons failed to spawn")) yield break;
            spawner.enabled = false;
            foreach (var coach in run.Wagons) coach.Enemies.ClearAlive();
            yield return null;
            status = "ten assignments preserve participants / ammo / wallets / dark pause";
            var identities = match.Bots.Select(b => b.Health.Identity).ToArray();
            var rounds = match.Bots.Select(b => b.Weapon.Rounds).ToArray(); var balances = match.Bots.Select(b => b.Wallet.Balance).ToArray();
            deadline = Time.realtimeSinceStartup + 15;
            bool sawPause = false;
            while (run.Assignments < 10 && !run.Flow.Terminal && Time.realtimeSinceStartup < deadline)
            {
                run.Flow.Tick(1000); yield return null;
                if (!Assert(Unique(), "Duplicate occupant or enemy target lost on assignment")) yield break;
                if (run.Flow.Paused)
                {
                    sawPause = true; float clock = Time.time; var positions = match.Bots.Select(b => b.transform.position).ToArray();
                    yield return new WaitForSecondsRealtime(.03f);
                    if (!Assert(Time.time == clock && match.Bots.Select((b, i) => b.transform.position == positions[i] && b.Weapon.Paused).All(v => v), "Bot moved/fired during dark pause")) yield break;
                }
            }
            if (!Assert(sawPause && run.Assignments >= 10 && !run.Flow.Terminal && match.Bots.Select((b, i) => b.Health.Identity.RuntimeId == identities[i].RuntimeId &&
                b.Health.LifeVersion == identities[i].LifeVersion && b.Weapon.Rounds == rounds[i] && b.Wallet.Balance == balances[i]).All(v => v) && !bot.Health.IsAlive,
                "Assignment reset participant state or respawned eliminated bot")) yield break;
            while (run.Flow.Paused && !run.Flow.Terminal) { run.Flow.Tick(1000); yield return null; }
            status = "human win / human elimination / solo mode / restart reuse";
            foreach (var other in match.Bots) if (other.Health.IsAlive)
            { other.Health.Invulnerable = false; other.Health.SetDamageProtection(0); other.Health.TryApplyDamage(new DamageContext(10000, default, Team.Enemy, 99, other.Health.LifeVersion)); }
            yield return null;
            if (!Assert(match.Result == BattleResult.Won && match.HumanPlace == 1 && run.Flow.Terminal, "Last human did not win")) yield break;
            run.Restart(new RunTimings { initialApproach = 300 }); yield return null;
            run.Player.Invulnerable = false; run.Player.SetDamageProtection(0);
            run.Player.TryApplyDamage(new DamageContext(10000, default, Team.Enemy, 100, run.Player.LifeVersion)); yield return null;
            if (!Assert(match.Result == BattleResult.Eliminated && match.HumanPlace == 5 && run.Flow.Terminal, "Human loss/rank failed")) yield break;
            match.SetMode(RunMode.Solo); yield return null;
            if (!Assert(!match.Active && match.Bots.All(b => !b.gameObject.activeSelf) && run.Wagons.Count(w => w.HasLivingDefender) == 1, "Solo retained bot occupancy")) yield break;
            match.SetMode(RunMode.Battle); yield return null;
            if (!Assert(Unique() && match.Bots.Select((b, i) => b.Health.Identity.RuntimeId == identities[i].RuntimeId && b.Health.IsAlive && b.Wallet.Balance == economy.Catalog.startingCoins).All(v => v), "Restart grew actors or retained previous economy/deaths")) yield break;
            Finish(true, "5 unique participants / 3 profiles; real glass kill credits bot only, metal blocks; zombie attacks assigned bot; actor damage preserves repair, door damage cancels; actual AI moves, repairs and purchases shared guns; death ranks/releases defender; no new empty-wagon spawn and survivors retained; 10 dark assignments preserve identity/ammo/money and never respawn eliminated; human win/loss, solo isolation and same-object restart passed. PC only; distant simulation, long-session balance and Android pending.");
        }
        private bool Unique()
        {
            var live = match.Bots.Where(b => b.Health.IsAlive).ToArray();
            return run.CurrentWagon.Defender == run.Player && live.All(b => b.Wagon != run.CurrentWagon && b.Wagon.Defender == b.Health &&
                b.Wagon.Enemies.Defender == b.Health && b.Wagon.Geometry.Contains(b.transform.position)) && live.Select(b => b.Wagon).Distinct().Count() == live.Length;
        }
        private bool Assert(bool value, string detail) { if (!value) Finish(false, detail); return value; }
        private void Finish(bool pass, string detail)
        {
            Directory.CreateDirectory("Logs"); string message = (pass ? "PASS: " : "FAIL: ") + detail;
            File.WriteAllText("Logs/BotAcceptance.txt", System.DateTime.UtcNow.ToString("O") + "\n" + message);
            if (pass) Debug.Log("[Battle] " + message); else Debug.LogError("[Battle] " + message);
            run.ConfigureRandomAssignments(restoreFixed, restoreInitial); match.AiAllowed = true;
            spawner.enabled = true; run.ControlsAllowed = run.AimAllowed = true; run.Repair.enabled = true;
            match.SetMode(restoreMode);
            for (int i = 0; i < views.Length; i++) if (views[i] != null) views[i].enabled = viewEnabled[i]; checking = false;
        }
    }
}
