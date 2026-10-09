using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using WaitYourTurn.Combat;
using WaitYourTurn.Defenses;
using WaitYourTurn.Enemies;
using WaitYourTurn.Player;
using WaitYourTurn.Train;

namespace WaitYourTurn.Run
{
    /// <summary>Local persistence adapter. Captures settled state after combat/repair/match callbacks.</summary>
    [DefaultExecutionOrder(2000)]
    public sealed class RunPersistence : MonoBehaviour
    {
        [SerializeField] private RunDriver run;
        [SerializeField] private RunEconomy economy;
        [SerializeField] private StationSpawner spawner;
        [SerializeField] private bool resumeInEditor;
        [SerializeField, Min(1)] private float autosaveSeconds = 5;
        [SerializeField] private string contentRevision = "train-integration-1";
        private LocalRunStore store;
        private string runId;
        private float nextSave;
        private bool restoring, started, invalidated, ownsSave;
        public bool Automatic => !Application.isEditor || resumeInEditor;
        public string Status { get; private set; } = "Local save ready";
        public bool Busy => restoring;
        public void Configure(RunDriver owner, RunEconomy shop, StationSpawner waves)
        { run = owner; economy = shop; spawner = waves; }
        public string SavePath => store.Path;
        public string RunId => runId;
        public bool IsTestStore => store != null && store.Path != Path.Combine(Application.persistentDataPath, "active-run.json");
        public void UseTestStore(string path) { store = new LocalRunStore(path); }
        private void Awake() { store = new LocalRunStore(Path.Combine(Application.persistentDataPath, "active-run.json")); }
        private void OnEnable() { run.Restarted += OnRestart; }
        private void OnDisable() { run.Restarted -= OnRestart; }
        private IEnumerator Start()
        {
            runId = Guid.NewGuid().ToString("N");
            if (Automatic)
            {
                var data = store.Load();
                if (data != null) yield return Restore(data);
            }
            started = true; nextSave = Time.unscaledTime + autosaveSeconds;
        }
        private void OnRestart()
        {
            if (restoring) return;
            runId = Guid.NewGuid().ToString("N"); invalidated = false;
            if (run.Match != null && run.Match.Suppressed) { ownsSave = false; return; }
            if (started && (Automatic || ownsSave)) { store.Invalidate(); ownsSave = false; Status = "New run; previous run retired"; }
        }
        private bool Available => run.Flow != null && !run.Loading && !restoring &&
            (run.Match == null || !run.Match.Suppressed) && !run.Flow.Terminal && run.Player.IsAlive;
        private void LateUpdate()
        {
            if (!started || restoring || run.Match != null && run.Match.Suppressed) return;
            if (run.Flow != null && (run.Flow.Terminal || !run.Player.IsAlive))
            { if (!invalidated && (Automatic || ownsSave)) { invalidated = store.Invalidate(); Status = invalidated ? "Finished run retired" : store.Diagnostic; } return; }
            if (Automatic && Time.unscaledTime >= nextSave) { SaveNow(); nextSave = Time.unscaledTime + autosaveSeconds; }
        }
        private void OnApplicationPause(bool paused) { if (paused && started && Automatic) SaveOrRetire(); }
        private void OnApplicationFocus(bool focused) { if (!focused && started && Automatic) SaveOrRetire(); }
        private void OnApplicationQuit() { if (started && Automatic) SaveOrRetire(); }
        private void SaveOrRetire()
        {
            if (!isActiveAndEnabled || restoring || run.Match != null && run.Match.Suppressed) return;
            if (run.Flow != null && (run.Flow.Terminal || !run.Player.IsAlive)) store.Invalidate(); else SaveNow();
        }
        public bool SaveNow()
        {
            if (!Available) { Status = "Save unavailable during restore, tests or finished run"; return false; }
            var data = Capture();
            if (!Validate(data, out string reason)) { Status = "Save rejected: " + reason; return false; }
            bool ok = store.Save(data); if (ok) ownsSave = true; Status = ok ? "Run saved locally" : store.Diagnostic; return ok;
        }
        public IEnumerator LoadNow()
        {
            if (restoring) yield break;
            var data = store.Load();
            if (data == null) { Status = store.Diagnostic; yield break; }
            yield return Restore(data);
        }
        private HealthComponent Body(int index) => index == 0 ? run.Player : run.Match.Bots[index - 1].Health;
        private static BodySave CaptureBody(HealthComponent body) => new BodySave { current = body.Current, maximum = body.Maximum,
            protection = body.DamageProtectionRemaining, position = body.transform.position, rotation = body.transform.rotation };
        private ParticipantSave CaptureParticipant(int index)
        {
            var bot = index == 0 ? null : run.Match.Bots[index - 1]; var body = Body(index);
            var wagon = index == 0 ? run.CurrentWagon : bot.Wagon;
            var repair = index == 0 ? run.Repair : bot.Repair;
            return new ParticipantSave { id = index == 0 ? "human" : bot.Nickname,
                wagon = body.IsAlive ? Array.IndexOf(run.Wagons, wagon) : -1,
                coins = index == 0 ? economy.Wallet.Available : bot.Wallet.Available,
                purchaseRemaining = index == 0 ? economy.PurchaseRemaining : bot.PurchaseRemaining,
                body = CaptureBody(body), weapon = (index == 0 ? run.Weapon : bot.Weapon).Capture(),
                repairDoor = wagon != null ? Array.IndexOf(wagon.Doors, repair.Target) : -1, repairProgress = repair.Progress };
        }
        public RunSnapshot Capture()
        {
            bool battle = run.Match != null && run.Match.Active;
            var data = new RunSnapshot { content = ContentKey(), runId = runId, mode = battle ? RunMode.Battle : RunMode.Solo,
                seed = run.ActualRunSeed, station = run.Flow.Station, phase = run.Flow.Phase, elapsed = run.Flow.Elapsed,
                assignments = run.Assignments, humanRandom = run.RandomState, botRandom = battle ? run.Match.RandomState : 0,
                ranks = battle ? run.Match.Roster.CapturePlaces() : null, timings = run.Timings,
                participants = new ParticipantSave[battle ? run.Match.Bots.Length + 1 : 1], wagons = new WagonSave[run.Wagons.Length],
                spawner = spawner.Capture(), soloUnlockedThrough = run.UsesSoloProgression ? run.Solo.UnlockedThrough : 0,
                interiorGates = run.UsesSoloProgression ? run.Solo.CaptureGates() : null,
                claimedLoot = run.UsesSoloProgression && run.Loot != null ? run.Loot.Capture() : null };
            for (int i = 0; i < data.participants.Length; i++) data.participants[i] = CaptureParticipant(i);
            for (int w = 0; w < data.wagons.Length; w++)
            {
                var wagon = run.Wagons[w]; var enemies = new List<EnemySave>(); var turrets = new List<TurretSave>();
                var doors = new DoorSave[wagon.Doors.Length];
                for (int d = 0; d < doors.Length; d++)
                { var door = wagon.Doors[d]; doors[d] = new DoorSave { current = door.Durability.Current, maximum = door.Durability.Maximum,
                    open = door.Portal.IsOpen, closing = door.Portal.ClosePending }; }
                foreach (var enemy in wagon.Enemies.Active) if (enemy.Health.IsAlive)
                    enemies.Add(new EnemySave { body = CaptureBody(enemy.Health), profile = enemy.Profile != null ? enemy.Profile.name : "",
                        station = enemy.SpawnStation, onBoard = enemy.OnBoard, attackRemaining = enemy.AttackRemaining, door = Array.IndexOf(wagon.Doors, enemy.Entry) });
                var mounts = economy.Defenses.Racks[w].Mounts;
                for (int t = 0; t < mounts.Length; t++)
                {
                    var actor = mounts[t].Actor; if (!actor.Deployed || actor.CreditedOwner == null || !actor.CreditedOwner.IsAlive) continue;
                    int owner = -1; for (int p = 0; p < data.participants.Length; p++) if (Body(p) == actor.CreditedOwner) owner = p;
                    turrets.Add(new TurretSave { slot = t, type = Array.IndexOf(economy.Defenses.Definitions, actor.Definition), owner = owner,
                        position = mounts[t].transform.position, rotation = mounts[t].transform.rotation, weapon = actor.Weapon.Capture() });
                }
                data.wagons[w] = new WagonSave { id = wagon.Id, doors = doors, enemies = enemies.ToArray(), turrets = turrets.ToArray(), spawnSequence = wagon.Enemies.SpawnSequence };
            }
            var drone = economy.Drone.Actor;
            data.hasDrone = drone.Occupied && drone.State != DroneState.Retiring;
            if (data.hasDrone)
                data.drone = new DroneSave { state = drone.State, position = drone.transform.position, rotation = drone.transform.rotation,
                    searchRemaining = drone.SearchRemaining, weapon = drone.Weapon.Capture() };
            return data;
        }
        private Dictionary<string, EnemyProfile> Profiles()
        {
            var result = new Dictionary<string, EnemyProfile>();
            foreach (var program in spawner.Programs) foreach (var band in program.bands)
            { if (result.TryGetValue(band.profile.name, out var previous) && previous != band.profile) throw new InvalidOperationException("Duplicate enemy profile save ID."); result[band.profile.name] = band.profile; }
            return result;
        }
        public string ContentKey()
        {
            // Revision is explicit for migrations; authored geometry/configuration also reject mismatched old saves.
            var text = new StringBuilder(contentRevision);
            if (run.Loot != null) text.Append("/owned-ammo-loot-v3/").Append(JsonUtility.ToJson(run.Loot.LocalPosition)).Append(run.Loot.PickupRadius);
            if (run.Solo != null)
                foreach (var connection in run.Solo.Connections)
                    text.Append("/interior-v2/").Append(JsonUtility.ToJson(connection.transform.position)).Append(JsonUtility.ToJson(connection.Passage)).Append(connection.UnlockPrice);
            foreach (var wagon in run.Wagons)
            {
                text.Append(wagon.Id).Append(JsonUtility.ToJson(wagon.Geometry.Interior)).Append(JsonUtility.ToJson(wagon.transform.position));
                foreach (var door in wagon.Doors) text.Append(door.name).Append(JsonUtility.ToJson(door.transform.position));
            }
            for (int i = 0; i < run.Weapon.WeaponCount; i++)
            {
                var spec = run.Weapon.StateAt(i).Spec;
                text.Append(run.Weapon.WeaponName(i)).Append(FormattableString.Invariant($"/{spec.Damage}/{spec.Interval}/{spec.Range}/{spec.MagazineSize}/{spec.ReloadSeconds}/{spec.InfiniteReserve}/{spec.InitialReserve}/{spec.Pellets}/{spec.SpreadDegrees}"));
            }
            foreach (var program in spawner.Programs)
            {
                text.Append(program.name).Append(program.firstStation).Append(program.queueCapacity).Append(program.positionAttempts);
                foreach (var band in program.bands) text.Append(band.profile.name).Append(JsonUtility.ToJson(band.profile))
                    .Append(band.wagon).Append(band.count).Append(band.firstAt.ToString("R", System.Globalization.CultureInfo.InvariantCulture))
                    .Append(band.interval.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
            }
            foreach (var definition in economy.Defenses.Definitions) text.Append(definition.name).Append(JsonUtility.ToJson(definition.weapon));
            text.Append(economy.Drone.Definition.name).Append(JsonUtility.ToJson(economy.Drone.Definition.weapon));
            if (run.Match != null) foreach (var bot in run.Match.Bots) text.Append(bot.Nickname).Append(JsonUtility.ToJson(bot.Profile));
            return LocalRunStore.Hash(text.ToString());
        }
        private static bool Finite(float x) => !float.IsNaN(x) && !float.IsInfinity(x);
        private static bool Pose(Vector3 p, Quaternion q) => Finite(p.sqrMagnitude) && p.sqrMagnitude < 100000000 &&
            Finite(q.x) && Finite(q.y) && Finite(q.z) && Finite(q.w) && Mathf.Abs(Quaternion.Dot(q, q) - 1) < .01f;
        private static bool Life(BodySave b) => b != null && b.maximum > 0 && b.maximum <= 1000000 &&
            b.current >= 0 && b.current <= b.maximum && b.protection >= 0 && b.protection <= 60 && Pose(b.position, b.rotation);
        private static bool Ammo(WeaponSnapshot saved, WeaponSpec spec) => saved != null && saved.equipped == 0 &&
            saved.owned != null && saved.owned.Length == 1 && saved.owned[0] && saved.ammo != null && saved.ammo.Length == 1 &&
            new WeaponState(spec).CanRestore(saved.ammo[0]) && saved.triggerRemaining >= 0 && saved.triggerRemaining <= 60;
        public bool Validate(RunSnapshot data, out string reason)
        {
            reason = "Invalid schema or content";
            if (data == null || data.version != RunSnapshot.Version || data.content != ContentKey() || string.IsNullOrEmpty(data.runId) ||
                data.runId.Length > 64 || data.timings == null || !data.timings.Valid || JsonUtility.ToJson(data.timings) != JsonUtility.ToJson(run.Timings) ||
                data.mode != RunMode.Solo && data.mode != RunMode.Battle || data.humanRandom == 0 || data.assignments < 0 || data.assignments > 100000 ||
                data.participants == null || data.wagons == null || data.wagons.Length != run.Wagons.Length ||
                data.participants.Length != (data.mode == RunMode.Battle ? run.Match.Bots.Length + 1 : 1)) return false;
            try { new RunFlow(run.Timings).Restore(data.phase, data.station, data.elapsed); }
            catch (ArgumentException) { return false; }
            if (data.mode == RunMode.Battle)
            { try { var roster = new BattleRoster(data.participants.Length); roster.Restore(data.ranks); if (data.botRandom == 0) return false; } catch (ArgumentException) { return false; } }
            var used = new bool[run.Wagons.Length];
            bool solo = data.mode == RunMode.Solo && run.Solo != null;
            if (solo)
            {
                if (data.soloUnlockedThrough < 0 || data.soloUnlockedThrough >= used.Length || data.interiorGates == null ||
                    data.interiorGates.Length != run.Solo.Connections.Length) return false;
                for (int i = data.soloUnlockedThrough; i < data.interiorGates.Length; i++) if (data.interiorGates[i]) return false;
                if (run.Loot != null && !run.Loot.Valid(data.claimedLoot, data.soloUnlockedThrough)) return false;
            }
            for (int i = 0; i < data.participants.Length; i++)
            {
                reason = "Invalid participant " + i; var p = data.participants[i];
                if (p == null || p.id != (i == 0 ? "human" : run.Match.Bots[i - 1].Nickname) || !Life(p.body) || p.coins < 0 ||
                    !(p.purchaseRemaining >= 0) || p.purchaseRemaining > 60 || !(p.repairProgress >= 0) || p.repairProgress > 1 ||
                    !(i == 0 ? run.Weapon : run.Match.Bots[i - 1].Weapon).CanRestore(p.weapon)) return false;
                bool living = p.body.current > 0;
                if (i == 0 && !living || data.mode == RunMode.Battle && living != (data.ranks[i] == 0)) return false;
                if (!living) { if (p.wagon != -1) return false; continue; }
                if (p.wagon < 0 || p.wagon >= used.Length || used[p.wagon]) return false;
                var wagon = run.Wagons[p.wagon];
                if (!(solo ? p.wagon <= data.soloUnlockedThrough && run.Solo.ContainsSaved(p.body.position, data.soloUnlockedThrough, data.interiorGates) :
                    wagon.Geometry.Contains(p.body.position)) || p.repairDoor < -1 || p.repairDoor >= wagon.Doors.Length) return false;
                used[p.wagon] = true;
            }
            var profiles = Profiles();
            for (int w = 0; w < data.wagons.Length; w++)
            {
                reason = "Invalid wagon " + w; var saved = data.wagons[w]; var wagon = run.Wagons[w];
                if (saved == null || saved.id != wagon.Id || saved.spawnSequence < 0 || saved.doors == null || saved.doors.Length != wagon.Doors.Length ||
                    saved.enemies == null || saved.enemies.Length > wagon.Enemies.Capacity || saved.turrets == null || saved.turrets.Length > economy.Defenses.Racks[w].Mounts.Length) return false;
                foreach (var door in saved.doors) if (door == null || !(door.maximum > 0) || door.maximum > 1000000 ||
                    !(door.current >= 0) || door.current > door.maximum || door.current == 0 && (!door.open || door.closing) ||
                    door.current > 0 && door.open != door.closing) return false;
                foreach (var enemy in saved.enemies)
                    if (enemy == null || !Life(enemy.body) || enemy.body.current <= 0 || enemy.profile == null ||
                        enemy.profile.Length > 128 || enemy.profile != "" && !profiles.ContainsKey(enemy.profile) || enemy.door < 0 ||
                        enemy.door >= saved.doors.Length || enemy.station < 0 || enemy.station > data.station ||
                        !(enemy.attackRemaining >= 0) || enemy.attackRemaining > 60 ||
                        (!solo || !enemy.onBoard) && (enemy.body.position - wagon.transform.position).sqrMagnitude > 2500 ||
                        enemy.onBoard && !(solo ? run.Solo.ContainsSaved(enemy.body.position, data.soloUnlockedThrough, data.interiorGates) :
                            wagon.Geometry.Contains(enemy.body.position))) return false;
                var slots = new bool[economy.Defenses.Racks[w].Mounts.Length]; var types = new int[economy.Defenses.Definitions.Length];
                foreach (var turret in saved.turrets)
                {
                    if (turret == null || turret.slot < 0 || turret.slot >= slots.Length || slots[turret.slot] || turret.type < 0 || turret.type >= types.Length ||
                        turret.owner < 0 || turret.owner >= data.participants.Length || data.participants[turret.owner].body.current <= 0 ||
                        !Pose(turret.position, turret.rotation) || !wagon.Geometry.Contains(turret.position) ||
                        !Ammo(turret.weapon, economy.Defenses.Definitions[turret.type].weapon.CreateSpec()) ||
                        ++types[turret.type] > economy.Defenses.Racks[w].PerTypeLimit) return false;
                    slots[turret.slot] = true;
                }
            }
            reason = "Invalid drone or spawn schedule";
            if (data.hasDrone && (data.drone == null || data.drone.state < DroneState.Following || data.drone.state > DroneState.Diving ||
                !Pose(data.drone.position, data.drone.rotation) || !(data.drone.searchRemaining >= 0) || data.drone.searchRemaining > economy.Drone.Definition.searchSeconds ||
                !Ammo(data.drone.weapon, economy.Drone.Definition.weapon.CreateSpec()))) return false;
            if (data.spawner == null || data.spawner.attempted == null || data.spawner.attempted.Length != used.Length || data.spawner.station < 0 || data.spawner.station > data.station) return false;
            foreach (var attempts in data.spawner.attempted) if (attempts < 0 || attempts > 1000000) return false;
            if (data.phase == RunPhase.Defense)
            {
                StationDefinition definition = null;
                foreach (var candidate in spawner.Programs) if (candidate.firstStation <= data.station && (definition == null || definition.firstStation < candidate.firstStation)) definition = candidate;
                if (definition == null || data.spawner.station != data.station) return false;
                var streams = spawner.BuildStreams(definition, solo);
                if (streams.Length == 0 || !new SpawnSchedule(streams, definition.queueCapacity,
                    definition.positionAttempts, definition.retryDelay).Restore(data.spawner.schedule)) return false;
            }
            reason = null; return true;
        }
        public IEnumerator Restore(RunSnapshot data)
        {
            if (restoring) yield break;
            if (!Validate(data, out string reason)) { Status = "Load rejected: " + reason; yield break; }
            restoring = true; bool ok = false, prepared = false; run.SetLoadingGate(true);
            // Navigation obstacle changes need a frame; gameplay remains gated throughout.
            try
            {
                run.Match.SetMode(data.mode); run.SetLoadingGate(true);
                if (data.mode == RunMode.Solo && run.Solo != null) run.Solo.Restore(data.soloUnlockedThrough, data.interiorGates);
                if (data.mode == RunMode.Solo && run.Loot != null) run.Loot.Restore(data.claimedLoot);
                foreach (var wagon in run.Wagons) foreach (var door in wagon.Doors) door.Portal.Restore(true, false, false);
                foreach (var rack in economy.Defenses.Racks) rack.Clear(); economy.Drone.Actor.Clear();
                prepared = true;
            }
            catch (Exception e) { Status = "Load preparation failed: " + e.GetType().Name; }
            if (!prepared) { run.Restart(); run.SetLoadingGate(false); restoring = false; yield break; }
            yield return null; yield return null;
            try
            {
                run.RestoreFlow(data.seed, data.humanRandom, data.phase, data.station, data.elapsed, data.assignments); run.SetLoadingGate(true);
                run.Match.BeginRun(data.seed);
                if (data.mode == RunMode.Battle) run.Match.RestoreRoster(data.ranks, data.botRandom);
                foreach (var wagon in run.Wagons) wagon.SetDefender(null);
                for (int i = 0; i < data.participants.Length; i++)
                {
                    var p = data.participants[i]; var body = Body(i);
                    if (!body.Restore(p.body.current, p.body.maximum, p.body.protection)) throw new InvalidOperationException("Participant health");
                    if (!(i == 0 ? run.Weapon : run.Match.Bots[i - 1].Weapon).Restore(p.weapon)) throw new InvalidOperationException("Participant ammo");
                    if (i != 0) run.Match.Bots[i - 1].RestoreWallet(p.coins, p.purchaseRemaining);
                    if (p.body.current <= 0) { run.Match.Bots[i - 1].Clear(); continue; }
                    var wagon = run.Wagons[p.wagon];
                    if (i == 0) run.ApplyPlayerAssignment(wagon, p.body.position, false);
                    else run.Match.Bots[i - 1].Assign(wagon, p.body.position, false);
                    body.transform.rotation = p.body.rotation;
                    if (i == 0) economy.RestoreWallet(p.coins, p.purchaseRemaining);
                }
                var profiles = Profiles();
                for (int w = 0; w < data.wagons.Length; w++)
                {
                    var wagon = run.Wagons[w]; var saved = data.wagons[w];
                    for (int d = 0; d < saved.doors.Length; d++)
                    { var door = saved.doors[d]; wagon.Doors[d].Durability.Restore(door.current, door.maximum); }
                    foreach (var savedEnemy in saved.enemies)
                    {
                        var enemy = wagon.Enemies.RestoreEnemy(savedEnemy.body.position, wagon.Doors[savedEnemy.door],
                            savedEnemy.profile == "" ? null : profiles[savedEnemy.profile], savedEnemy.station, wagon.Enemies.Active.Count);
                        if (enemy == null) throw new InvalidOperationException("Enemy navigation placement");
                        enemy.Health.Restore(savedEnemy.body.current, savedEnemy.body.maximum, savedEnemy.body.protection);
                        enemy.transform.rotation = savedEnemy.body.rotation; enemy.RestoreTravel(savedEnemy.onBoard); enemy.RestoreAttack(savedEnemy.attackRemaining); enemy.SetPaused(true);
                    }
                    wagon.Enemies.SpawnSequence = saved.spawnSequence;
                    for (int d = 0; d < saved.doors.Length; d++)
                    { var door = saved.doors[d]; wagon.Doors[d].Portal.Restore(door.open, door.closing, data.phase == RunPhase.Defense || data.phase == RunPhase.DepartureWarning); }
                    foreach (var turret in saved.turrets)
                        if (!economy.Defenses.RestoreTurret(w, turret.slot, turret.type, turret.position, turret.rotation, Body(turret.owner), turret.weapon))
                            throw new InvalidOperationException("Turret restore");
                }
                for (int i = 0; i < data.participants.Length; i++)
                {
                    var p = data.participants[i]; if (p.wagon < 0) continue;
                    (i == 0 ? run.Repair : run.Match.Bots[i - 1].Repair).RestoreProgress(p.repairDoor < 0 ? null : run.Wagons[p.wagon].Doors[p.repairDoor], p.repairProgress);
                }
                if (data.hasDrone && !economy.Drone.Restore(data.drone.weapon, data.drone.state, data.drone.position, data.drone.rotation, data.drone.searchRemaining))
                    throw new InvalidOperationException("Drone restore");
                if (!spawner.Restore(data.spawner)) throw new InvalidOperationException("Spawn schedule restore");
                if (economy.Profile != null && !economy.Profile.ApplyStats(false)) throw new InvalidOperationException("Human profile stats");
                Physics.SyncTransforms(); runId = data.runId; invalidated = false; ownsSave = true; Status = "Run resumed locally"; ok = true;
            }
            catch (Exception e) { Status = "Load failed safely; new run: " + e.Message; }
            finally
            {
                if (!ok) run.Restart();
                run.SetLoadingGate(false); restoring = false; nextSave = Time.unscaledTime + autosaveSeconds;
            }
        }
    }
}
