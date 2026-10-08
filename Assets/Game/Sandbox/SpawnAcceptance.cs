using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using WaitYourTurn.Combat;
using WaitYourTurn.Enemies;
using WaitYourTurn.Navigation;
using WaitYourTurn.Run;

namespace WaitYourTurn.Sandbox
{
    /// <summary>PC acceptance fixture; no production dependency on this component.</summary>
    public sealed class SpawnAcceptance : MonoBehaviour
    {
        [SerializeField] private RunDriver run;
        [SerializeField] private StationSpawner spawner;
        [SerializeField] private TargetRegistry registry;
        private bool checking;
        private StationDefinition[] original;
        private StationDefinition stress;
        private TrainIntegrationController integration;
        private HealthComponent[] fixtureDefenders;
        private string status;
        public void Configure(RunDriver owner, StationSpawner director, TargetRegistry targets)
        { run = owner; spawner = director; registry = targets; }
        private void Update()
        { if (!checking && Input.GetKeyDown(KeyCode.F11)) StartCoroutine(Check()); }
        private void OnGUI()
        { if (checking) GUI.Label(new Rect(20, 95, Screen.width - 40, 35), "M6: " + status); }
        private IEnumerator Check()
        {
            checking = true; status = "profile reset / bounded station stress";
            integration = GetComponentInScene(); if (integration != null) integration.enabled = false;
            original = spawner.Programs;
            spawner.enabled = true; run.ControlsAllowed = run.AimAllowed = false; run.Repair.enabled = false;
            run.Restart(new RunTimings { initialApproach = .2f, defense = 300 }); run.Player.Invulnerable = true;
            yield return new WaitForSeconds(1);
            spawner.enabled = false;
            foreach (var w in run.Wagons) w.Enemies.ClearAlive();
            var wagon = run.CurrentWagon;
            var profiles = original.SelectMany(p => p.bands).Select(b => b.profile).Distinct().ToArray();
            EnemyBrain previous = null; uint previousLife = 0;
            for (int cycle = 0; cycle < 9; cycle++)
            {
                var profile = profiles[cycle % profiles.Length];
                if (!wagon.SpawnOutside(cycle, profile, 17)) { Finish(false, "Profile spawn rejected: " + profile.id); yield break; }
                var enemy = wagon.Enemies.Active[0]; enemy.SetPaused(true);
                var melee = enemy.GetComponent<MeleeAttack>();
                if (enemy.Profile != profile || enemy.Health.Maximum != profile.health || enemy.Health.Current != profile.health ||
                    enemy.SpawnStation != 17 || enemy.WagonId != wagon.Id || melee.Damage != profile.damage || melee.Interval != profile.attackInterval ||
                    Mathf.Abs(enemy.GetComponent<AgentMotor>().Agent.speed - profile.speed) > .17f || melee.WindingUp ||
                    (previous != null && (enemy != previous || enemy.Health.LifeVersion == previousLife)))
                { Finish(false, "Pooled profile state leaked: " + profile.id); yield break; }
                if (previousLife != 0 && enemy.Health.TryApplyDamage(new DamageContext(1, default, Team.Player, 400, previousLife)).Applied)
                { Finish(false, "Old life can damage a reused enemy"); yield break; }
                previous = enemy; previousLife = enemy.Health.LifeVersion;
                enemy.Health.TryApplyDamage(new DamageContext(1, default, Team.Player, 401, previousLife));
                wagon.Enemies.ClearAlive();
                if (registry.Targets.Count != 0) { Finish(false, "Registry leaked after pool return"); yield break; }
            }
            int created = wagon.Enemies.CreatedCount;
            if (wagon.Enemies.TrySpawn(Vector3.one * 10000, profiles[0], 17) || wagon.Enemies.CreatedCount != created || registry.Targets.Count != 0)
            { Finish(false, "Invalid navigation consumed or grew the pool"); yield break; }
            stress = ScriptableObject.CreateInstance<StationDefinition>(); stress.name = "Runtime acceptance only";
            stress.globalLiveLimit = 9; stress.wagonLiveLimit = 3; stress.queueCapacity = 4; stress.requestsPerFrame = 2;
            stress.bands = profiles.Select(p => new SpawnBand { profile = p, count = 30, firstAt = 0, interval = .02f }).ToArray();
            spawner.ConfigurePrograms(new[] { stress }); spawner.enabled = true;
            run.Restart(new RunTimings { initialApproach = .2f, defense = 300 }); run.Player.Invulnerable = true;
            // Production eligibility: only the current human has a living defender before bots are implemented.
            float emptyDeadline = Time.realtimeSinceStartup + 2;
            while (Time.realtimeSinceStartup < emptyDeadline) { FreezeEnemies(); yield return null; }
            if (run.CurrentWagon.Enemies.Active.Count != stress.wagonLiveLimit ||
                run.Wagons.Any(w => w != run.CurrentWagon && w.Enemies.Active.Count != 0) || spawner.SuppressedStreams == 0)
            { Finish(false, "Empty wagons received new enemies or occupied wagon did not fill"); yield break; }
            var occupied = run.CurrentWagon; int beforeEmpty = spawner.Spawned;
            occupied.SetDefender(null);
            float noDefenderDeadline = Time.realtimeSinceStartup + .5f;
            while (Time.realtimeSinceStartup < noDefenderDeadline) { FreezeEnemies(); yield return null; }
            if (spawner.Spawned != beforeEmpty || occupied.Enemies.Active.Count != stress.wagonLiveLimit)
            { Finish(false, "Vacating wagon removed survivors or created new enemies"); yield break; }
            // Global stress needs actual health-only fixture occupants, not a production spawn-policy bypass.
            run.Restart(new RunTimings { initialApproach = .2f, defense = 300 }); run.Player.Invulnerable = true;
            fixtureDefenders = new HealthComponent[run.Wagons.Length];
            for (int i = 0; i < run.Wagons.Length; i++)
            {
                if (run.Wagons[i] == run.CurrentWagon) continue;
                var body = new GameObject("Acceptance occupant - not a bot").AddComponent<HealthComponent>();
                body.ResetForSpawn(100, Team.Player); body.Invulnerable = true;
                fixtureDefenders[i] = body; run.Wagons[i].SetDefender(body);
            }
            float deadline = Time.realtimeSinceStartup + 5;
            while (spawner.TotalActive < stress.globalLiveLimit && Time.realtimeSinceStartup < deadline)
            {
                FreezeEnemies();
                if (!BoundsHold()) { Finish(false, "Initial spawn budgets/registry/scope failed"); yield break; }
                yield return null;
            }
            FreezeEnemies();
            if (spawner.TotalActive != stress.globalLiveLimit || spawner.Pending > stress.queueCapacity)
            { Finish(false, "Director did not fill its permitted capacity: " + spawner.Diagnostic); yield break; }
            int produced = spawner.Spawned;
            yield return new WaitForSeconds(.4f);
            if (spawner.Spawned != produced || !BoundsHold()) { Finish(false, "Full-capacity schedule exceeded its limit"); yield break; }
            var eliminated = fixtureDefenders.First(d => d != null);
            var vacant = run.Wagons.First(w => w.Defender == eliminated);
            int carried = vacant.Enemies.Active.Count;
            eliminated.Invulnerable = false;
            eliminated.TryApplyDamage(new DamageContext(1000, default, Team.Enemy, 403, eliminated.LifeVersion));
            yield return new WaitForSeconds(.4f);
            if (vacant.HasLivingDefender || vacant.Enemies.Active.Count != carried || spawner.Spawned != produced)
            { Finish(false, "Eliminated occupant still attracts new enemies or loses existing bodies"); yield break; }
            var victim = run.CurrentWagon.Enemies.Active.First();
            victim.Health.TryApplyDamage(new DamageContext(1000, default, Team.Player, 402, victim.Health.LifeVersion));
            deadline = Time.realtimeSinceStartup + 2;
            while (spawner.Spawned == produced && Time.realtimeSinceStartup < deadline) { FreezeEnemies(); yield return null; }
            if (spawner.Spawned != produced + 1 || !BoundsHold()) { Finish(false, "Released capacity was not reused exactly once"); yield break; }
            // Seed one real passenger per wagon, then let the production director run 30 automatic cycles.
            spawner.enabled = false;
            run.Restart(new RunTimings { initialApproach = .08f, nextApproach = .08f, defense = .3f,
                departureWarning = .08f, departure = .08f, fadeOut = .08f, hidden = .08f, fadeIn = .08f });
            run.Player.Invulnerable = true;
            var survivors = new EnemyBrain[run.Wagons.Length];
            for (int i = 0; i < run.Wagons.Length; i++)
            {
                var w = run.Wagons[i];
                if (!w.SpawnOutside(0, profiles[i % profiles.Length], 0)) { Finish(false, "Survivor seed failed"); yield break; }
                var e = w.Enemies.Active[0]; e.SetPaused(true);
                if (!e.GetComponent<AgentMotor>().TryPlace(w.Geometry.SafePosition(3)) || !e.RetainForTravel(e.transform.position))
                { Finish(false, "Survivor interior placement failed"); yield break; }
                survivors[i] = e;
            }
            spawner.enabled = true;
            int previousSpawned = 0, previousStation = run.Flow.Station;
            deadline = Time.realtimeSinceStartup + 60;
            int peak = 0;
            while (run.Assignments < 30 && !run.Flow.Terminal && Time.realtimeSinceStartup < deadline)
            {
                FreezeEnemies(); peak = Mathf.Max(peak, spawner.TotalActive);
                if (!BoundsHold() || survivors.Where((e, i) => !run.Wagons[i].Enemies.Active.Contains(e) || !e.OnBoard || e.SpawnStation != 0).Any())
                { Finish(false, "Long-run scope / live / queue / registry / survivor bound failed"); yield break; }
                if (run.Flow.Station == previousStation && run.Flow.Phase != RunPhase.Defense && spawner.Spawned > previousSpawned)
                { Finish(false, "Spawn happened outside Defense"); yield break; }
                previousStation = run.Flow.Station; previousSpawned = spawner.Spawned;
                yield return null;
            }
            if (run.Assignments < 30 || run.Flow.Terminal) { Finish(false, "30-cycle stress did not complete: " + run.Failure); yield break; }
            // Pause time must not consume arrival protection; expiration uses visible gameplay time.
            spawner.enabled = false;
            float protectedBefore = run.Player.DamageProtectionRemaining;
            if (run.ArrivalProtectionSeconds > 0 && protectedBefore <= 0)
            { Finish(false, "Assignment did not grant arrival protection"); yield break; }
            float pauseDeadline = Time.realtimeSinceStartup + .2f;
            while (run.Flow.Paused && Time.realtimeSinceStartup < pauseDeadline) yield return null;
            if (run.Flow.Paused && Mathf.Abs(protectedBefore - run.Player.DamageProtectionRemaining) > .01f)
            { Finish(false, "Dark transition consumed protection time"); yield break; }
            while (run.Flow.Paused) yield return null;
            // Hold the approach so another accelerated assignment cannot renew the protection under test.
            enabled = false;
            foreach (var w in run.Wagons) foreach (var enemy in w.Enemies.Active) enemy.SetPaused(true);
            run.Player.Invulnerable = false;
            float hp = run.Player.Current;
            if (run.ArrivalProtectionSeconds > 0 &&
                run.Player.TryApplyDamage(new DamageContext(1, default, Team.Enemy, 404, run.Player.LifeVersion)).Applied)
            { enabled = true; Finish(false, "Fresh arrival protection allowed damage"); yield break; }
            run.Player.Invulnerable = true;
            // The RunDriver clock is intentionally disabled only for this fixed-duration expiry check.
            run.enabled = false;
            yield return new WaitForSeconds(run.Player.DamageProtectionRemaining + .05f);
            run.Player.Invulnerable = false;
            bool expired = run.Player.TryApplyDamage(new DamageContext(1, default, Team.Enemy, 405, run.Player.LifeVersion)).Applied;
            run.enabled = true; enabled = true;
            if (!expired || run.Player.Current != hp - 1) { Finish(false, "Arrival protection never expired"); yield break; }
            Finish(true, $"Empty wagons suppress new spawn without removing survivors; occupied wagon still spawns; synthetic living occupants exercise global 9, wagon 3, queue 4, frame 2 limits; 3 profiles / 9 same-object reuse cycles; stale life rejected; invalid nav leaves pool intact; kill frees one slot; 30 station cycles retain {survivors.Length} original passengers; arrival protection rejects damage and expires in gameplay time; registry matches active bodies; fixed {run.Wagons.Sum(w => w.Enemies.CreatedCount)}-object pool, peak {peak} active. PC only; Android profile pending.");
        }
        private TrainIntegrationController GetComponentInScene() => FindAnyObjectByType<TrainIntegrationController>();
        private void FreezeEnemies()
        {
            foreach (var w in run.Wagons)
            { foreach (var d in w.Doors) d.Durability.Invulnerable = true; foreach (var e in w.Enemies.Active) e.SetPaused(true); }
        }
        private bool BoundsHold()
        {
            int alive = spawner.TotalActive;
            return alive <= stress.globalLiveLimit && spawner.Pending <= stress.queueCapacity &&
                spawner.SpawnedThisFrame <= stress.requestsPerFrame && spawner.CreatedThisFrame <= stress.warmupPerFrame && registry.Targets.Count == alive &&
                run.Wagons.All(w => w.Enemies.Active.Count <= stress.wagonLiveLimit && w.Enemies.CreatedCount == w.Enemies.Capacity &&
                    w.Enemies.Active.All(e => e.WagonId == w.Id && e.Profile != null && e.GetComponent<AgentMotor>().Ready));
        }
        private void Finish(bool pass, string detail)
        {
            Directory.CreateDirectory("Logs"); string message = (pass ? "PASS: " : "FAIL: ") + detail;
            File.WriteAllText("Logs/SpawnAcceptance.txt", System.DateTime.UtcNow.ToString("O") + "\n" + message);
            if (pass) Debug.Log("[SpawnAcceptance] " + message); else Debug.LogError("[SpawnAcceptance] " + message);
            spawner.enabled = false; spawner.ConfigurePrograms(original); spawner.enabled = true;
            if (stress != null) Destroy(stress);
            if (fixtureDefenders != null) foreach (var defender in fixtureDefenders) if (defender != null) Destroy(defender.gameObject);
            fixtureDefenders = null;
            run.ControlsAllowed = run.AimAllowed = true; run.Repair.enabled = true; run.Restart();
            if (integration != null) integration.enabled = true; checking = false;
        }
    }
}
