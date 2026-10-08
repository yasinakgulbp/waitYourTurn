using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using WaitYourTurn.Combat;
using WaitYourTurn.Enemies;
using WaitYourTurn.Player;
using WaitYourTurn.Run;

namespace WaitYourTurn.Sandbox
{
    /// <summary>Temporary controls/fixture; runtime run flow has no dependency on this HUD.</summary>
    public sealed class RunSandboxController : MonoBehaviour
    {
        [SerializeField] private RunDriver run;
        [SerializeField] private StationSpawner spawner;
        [SerializeField] private RunPresentation presentation;
        private bool checking;
        private string result = "WASD/left joystick. Automatic aim, fire and reload. Repair near yellow marker.";
        public void Configure(RunDriver owner, StationSpawner spawn, RunPresentation visual)
        { run = owner; spawner = spawn; presentation = visual; }
        private void Update()
        {
            if (!checking && run.Flow != null && Input.GetKeyDown(KeyCode.F9)) StartCoroutine(CheckRepair());
        }
        private void OnGUI()
        {
            if (run.Flow == null || run.CurrentWagon == null) return;
            Rect panel = new Rect(12, 12, 455, 315 + run.Wagons.Length * 25);
            run.Player.GetComponent<MoveInput>().BlockedScreenArea = panel;
            GUILayout.BeginArea(panel, GUI.skin.box);
            GUILayout.Label($"{run.Wagons.Length} WAGONS | STATION {run.Flow.Station} | {run.CurrentWagon.Id} | {run.Flow.Phase} {run.Flow.Remaining:F1}s");
            HitscanWeapon weapon = run.Weapon;
            GUILayout.Label($"HP {run.Player.Current}/{run.Player.Maximum} | {weapon.DisplayName} {weapon.Rounds}/{weapon.State.Spec.MagazineSize} + {(weapon.State.Spec.InfiniteReserve ? "INF" : weapon.Reserve.ToString())} {(weapon.Reloading ? "RELOADING" : weapon.State.Empty ? "EMPTY" : "")}");
            foreach (WagonRuntime wagon in run.Wagons)
                GUILayout.Label($"{wagon.Id}: door {wagon.Doors[0].Durability.Current}/{wagon.Doors[0].Durability.Maximum}, enemies {wagon.Enemies.Active.Count}/{wagon.Enemies.CreatedCount}");
            GUILayout.Label($"Repair {run.Repair.Progress * 100:F0}% | Train speed {presentation.Speed:F1} | Assignments {run.Assignments}");
            GUI.enabled = !checking;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Restart run")) { spawner.ResetSchedule(); run.Restart(); }
            if (GUILayout.Button("+3 current wagon")) for (int i = 0; i < 3; i++) if (run.Flow.Phase == RunPhase.Defense) run.CurrentWagon.SpawnOutside(i);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            for (int i = 0; i < weapon.WeaponCount; i++)
                if (GUILayout.Button(weapon.WeaponName(i))) { weapon.Equip(i); run.Player.GetComponent<AutoAim>().ClearTarget(); }
            GUILayout.EndHorizontal();
            if (weapon.WeaponCount > 1 && GUILayout.Button("Check all weapons / glass / walls")) StartCoroutine(CheckWeapons());
            if (GUILayout.Button("Check damaged door / repair interruptions")) StartCoroutine(CheckRepair());
            if (GUILayout.Button("Check 10 fast station transitions")) StartCoroutine(CheckTransitions());
            if (GUILayout.Button("Check bounded crowd (6 seconds)")) StartCoroutine(CheckCrowd());
            if (GUILayout.Button("Kill player (visible gameplay)"))
                run.Player.TryApplyDamage(new DamageContext(1000, default, Team.Enemy, 1, run.Player.LifeVersion));
            GUI.enabled = true;
            GUILayout.Label(run.Failure ?? result);
            GUILayout.EndArea();
            if (presentation.Darkness > 0)
            {
                GUI.color = new Color(0, 0, 0, presentation.Darkness);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture); GUI.color = Color.white;
                if (run.Flow.Phase == RunPhase.Hidden)
                    GUI.Label(new Rect(Screen.width / 2 - 160, Screen.height / 2 - 30, 320, 100),
                        $"Vagon dağılımı hazırlanıyor...\n{Mathf.CeilToInt(run.Flow.Remaining)}\nTren yolculuğu sürüyor.");
            }
            if (run.Flow.Phase == RunPhase.GameOver)
                GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height / 2, 250, 60), "KOŞU BİTTİ — Restart run");
        }
        private IEnumerator CheckRepair()
        {
            checking = true; spawner.enabled = false; run.ControlsAllowed = run.AimAllowed = false;
            run.Restart(new RunTimings { initialApproach = 300 }); run.Player.Invulnerable = true;
            yield return null;
            var door = run.CurrentWagon.Doors[0]; var health = door.Durability;
            var motor = run.Player.GetComponent<PlayerMotor>(); uint life = health.LifeVersion;
            motor.Place(door.RepairPosition + Vector3.up * .05f);
            health.TryApplyDamage(new DamageContext(health.Maximum * .5f, default, Team.Enemy, 1, life));
            yield return null; yield return null;
            float damaged = health.Current;
            yield return new WaitForSeconds(1);
            if (run.Repair.Progress <= 0 || health.Current != damaged || door.Portal.IsOpen)
            { Finish(false, "Damaged intact door did not start timed repair without partial healing.", "Repair"); yield break; }
            float progressBeforeHit = run.Repair.Progress;
            run.Player.Invulnerable = false;
            run.Player.TryApplyDamage(new DamageContext(1, default, Team.Enemy, 2, run.Player.LifeVersion));
            health.TryApplyDamage(new DamageContext(1, default, Team.Enemy, 3, life));
            yield return null; yield return null;
            bool shouldInterrupt = run.Repair.InterruptOnDoorDamage || run.Repair.InterruptOnActorDamage;
            if ((shouldInterrupt ? run.Repair.Progress > .1f : run.Repair.Progress < progressBeforeHit) ||
                health.Current != damaged - 1 || run.Player.Current != 99)
            { Finish(false, "Damage/repair interruption policy was not respected.", "Repair"); yield break; }
            float deadline = Time.time + 4;
            while (health.Current < health.Maximum && Time.time < deadline) yield return null;
            if (health.Current != health.Maximum || health.LifeVersion != life || door.Portal.IsOpen)
            { Finish(false, "Intact maintenance changed identity or failed to restore/keep the closed door.", "Repair"); yield break; }

            health.TryApplyDamage(new DamageContext(1000, default, Team.Enemy, 3, life));
            motor.Place(door.Portal.transform.position + new Vector3(0, .05f, .25f));
            yield return null; yield return null;
            deadline = Time.time + 4;
            while (!health.IsAlive && Time.time < deadline) yield return null;
            if (!health.IsAlive || health.LifeVersion != life + 1 || !door.Portal.IsOpen || !door.Portal.ClosePending)
            { Finish(false, "Broken repair did not create one life or closed on a doorway occupant.", "Repair"); yield break; }
            motor.Place(door.RepairPosition + Vector3.up * .05f);
            deadline = Time.time + 2;
            while (door.Portal.IsOpen && Time.time < deadline) yield return null;
            if (door.Portal.IsOpen)
            { Finish(false, "Repaired doorway failed to close after clearance.", "Repair"); yield break; }
            Finish(true, "Damaged intact maintenance, configured damage policy without partial healing, same-life healing, broken reconstruction and occupied-doorway clearance passed.", "Repair");
        }
        private IEnumerator CheckWeapons()
        {
            checking = true; spawner.enabled = false; run.ControlsAllowed = run.AimAllowed = false;
            run.Restart(new RunTimings { initialApproach = 300 }); run.Player.Invulnerable = true;
            yield return null;
            WagonRuntime wagon = run.CurrentWagon; HitscanWeapon weapon = run.Weapon;
            var motor = run.Player.GetComponent<PlayerMotor>(); var door = wagon.Doors[0];
            Vector3 Point(float x, float z) => wagon.transform.TransformPoint(new Vector3(x, .05f, z));
            if (!wagon.Enemies.TrySpawn(Point(0, -4))) { Finish(false, "Weapon fixture spawn failed.", "Weapons"); yield break; }
            EnemyBrain enemy = wagon.Enemies.Active[0]; enemy.SetPaused(true);
            enemy.Health.ResetForSpawn(1000, Team.Enemy);
            float doorHealth = door.Durability.Current;
            for (int index = 0; index < weapon.WeaponCount; index++)
            {
                motor.Place(Point(0, 2.2f));
                if (!enemy.GetComponent<WaitYourTurn.Navigation.AgentMotor>().TryPlace(Point(0, -4)))
                { Finish(false, "Window target placement failed.", "Weapons"); yield break; }
                Physics.SyncTransforms(); weapon.ResetWeapon(); weapon.Equip(index);
                string name = weapon.DisplayName; float before = enemy.Health.Current;
                int traces = 0; Vector3 centerImpact = default;
                void Observe(ShotNotice shot) { traces++; if (shot.Pellet == 0) centerImpact = shot.End; }
                weapon.Fired += Observe;
                bool fired = weapon.HasSight(enemy.Health) && weapon.TryFire(enemy.transform.position + Vector3.up * .85f - weapon.Muzzle);
                weapon.Fired -= Observe;
                if (!fired || enemy.Health.Current >= before || traces != weapon.State.Spec.Pellets ||
                    weapon.Rounds != weapon.State.Spec.MagazineSize - 1 || door.Durability.Current != doorHealth || door.Portal.IsOpen)
                { Finish(false, name + ": intact window shot/pellet/ammo rule failed.", "Weapons"); yield break; }

                motor.Place(Point(3, 2.2f));
                if (!enemy.GetComponent<WaitYourTurn.Navigation.AgentMotor>().TryPlace(Point(3, -4)))
                { Finish(false, "Wall target placement failed.", "Weapons"); yield break; }
                Physics.SyncTransforms(); weapon.ResetWeapon(); weapon.Equip(index); before = enemy.Health.Current;
                if (weapon.HasSight(enemy.Health) || !weapon.TryFire(enemy.transform.position + Vector3.up * .85f - weapon.Muzzle) || enemy.Health.Current != before)
                { Finish(false, name + ": wall leaked damage.", "Weapons"); yield break; }

                motor.Place(Point(0, 2.2f)); Physics.SyncTransforms(); weapon.ResetWeapon(); weapon.Equip(index);
                weapon.Fired += Observe; weapon.TryFire(door.Portal.transform.position + Vector3.up * .35f - weapon.Muzzle); weapon.Fired -= Observe;
                if (Mathf.Abs(centerImpact.z - door.Portal.transform.position.z) > .2f || centerImpact.y > .7f || door.Durability.Current != doorHealth)
                { Finish(false, name + ": lower panel failed.", "Weapons"); yield break; }
                yield return null;
            }
            // Restore the pooled template's normal health before returning it.
            enemy.Health.ResetForSpawn(10, Team.Enemy);
            Finish(true, $"All {weapon.WeaponCount} weapon profiles: intact glass hits, wall/lower-panel obstruction, pellet notices and one-round-per-trigger. Cosmetic flash/traces active; doors unchanged.", "Weapons");
        }
        private IEnumerator CheckTransitions()
        {
            checking = true; spawner.enabled = false; run.ControlsAllowed = run.AimAllowed = false;
            run.Restart(new RunTimings { initialApproach = .5f, nextApproach = .2f, defense = .3f,
                departureWarning = .2f, departure = .2f, fadeOut = .2f, hidden = .4f, fadeIn = .2f });
            run.Player.TryApplyDamage(new DamageContext(15, default, Team.Enemy, 2, run.Player.LifeVersion));
            run.Player.Invulnerable = true; // Fixture isolation: real combat continues in the normal scene.
            if (run.Weapon.WeaponCount > 1) run.Weapon.Equip(1);
            run.Weapon.TryFire(Vector3.up);
            int equipped = run.Weapon.EquippedIndex, reserve = run.Weapon.Reserve;
            int count = run.Wagons.Length;
            var survivors = new EnemyBrain[count]; var health = new float[count]; var lives = new uint[count];
            var frozenEnemies = new Vector3[count]; var visited = new HashSet<string>();
            for (int i = 0; i < count; i++)
            {
                WagonRuntime wagon = run.Wagons[i]; var door = wagon.Doors[0];
                door.Durability.TryApplyDamage(new DamageContext(i == 0 ? 1000 : 3 + i,
                    default, Team.Enemy, (ulong)(3 + i), door.Durability.LifeVersion));
                health[i] = door.Durability.Current; lives[i] = door.Durability.LifeVersion;
            }
            // Carving is updated by Unity after the obstacle changes, not synchronously in SetOpen.
            yield return null;
            yield return null;
            for (int i = 0; i < count; i++)
            {
                WagonRuntime wagon = run.Wagons[i]; var door = wagon.Doors[0];
                Vector3 inside = i == 0 ? door.Portal.transform.position + door.Portal.transform.forward * .1f :
                    wagon.transform.TransformPoint(new Vector3(0, 0, 5.5f));
                if (!wagon.Enemies.TrySpawn(wagon.transform.TransformPoint(new Vector3(-1.2f, 0, -7))) ||
                    !wagon.Enemies.TrySpawn(inside)) { Finish(false, "Fixture spawn failed: " + wagon.Id); yield break; }
                survivors[i] = wagon.Enemies.Active[1];
            }
            survivors[0].SetPaused(true); // Hold precisely at the departure boundary.
            if (!survivors[0].IsInside || !survivors[0].NeedsSafeDeparture)
            { Finish(false, "Doorway fixture did not occupy the inner threshold."); yield break; }
            run.Player.GetComponent<PlayerMotor>().Place(run.Wagons[0].Doors[0].RepairPosition + Vector3.up * .05f);
            var camera = UnityEngine.Object.FindAnyObjectByType<Camera>();
            Vector3 cameraOffset = presentation.FollowOffset;
            int lastAssignment = -1;
            uint playerLife = run.Player.LifeVersion;
            int ammo = run.Weapon.Rounds; ulong hitId = 10;
            bool wasPaused = false; float frozenTime = 0, repairProgress = 0;
            float deadline = Time.realtimeSinceStartup + 35;
            while (run.Flow.Station < 11 && Time.realtimeSinceStartup < deadline)
            {
                if (run.Flow.Phase == RunPhase.Faulted) { Finish(false, run.Failure); yield break; }
                if (run.Player.Current != 85 || run.Player.LifeVersion != playerLife || run.Weapon.Rounds != ammo ||
                    run.Weapon.EquippedIndex != equipped || run.Weapon.Reserve != reserve)
                { Finish(false, "Player/ammo/door state reset between phases."); yield break; }
                visited.Add(run.CurrentWagon.Id);
                Vector3 center = run.Player.transform.position + Vector3.up * .85f;
                if ((run.CurrentWagon.Area.Constrain(center, .3f) - center).sqrMagnitude > .001f ||
                    (lastAssignment == run.Assignments &&
                    (camera.transform.position - run.CurrentWagon.transform.position - cameraOffset).sqrMagnitude > .001f))
                { Finish(false, "Player/camera bound to wrong wagon."); yield break; }
                lastAssignment = run.Assignments;
                if (run.Flow.Paused)
                {
                    if (!wasPaused)
                    { frozenTime = Time.time; repairProgress = run.Repair.Progress;
                        for (int i = 0; i < count; i++) frozenEnemies[i] = survivors[i].transform.position; }
                    if (Mathf.Abs(Time.time - frozenTime) > .001f)
                    { Finish(false, "Dark passage advanced gameplay clock."); yield break; }
                    if (run.Flow.Phase != RunPhase.FadeIn && Mathf.Abs(run.Repair.Progress - repairProgress) > .001f)
                    { Finish(false, "Repair advanced during darkness."); yield break; }
                }
                for (int i = 0; i < count; i++)
                {
                    WagonRuntime wagon = run.Wagons[i]; var door = wagon.Doors[0].Durability; EnemyBrain enemy = survivors[i];
                    if (door.Current != health[i] || door.LifeVersion != lives[i] || enemy == null ||
                        !wagon.Enemies.Active.ContainsReference(enemy) || enemy.WagonId != wagon.Id)
                    { Finish(false, "Door state/enemy ownership changed: " + wagon.Id); yield break; }
                    if (run.Flow.Paused && (!enemy.Paused || (enemy.transform.position - frozenEnemies[i]).sqrMagnitude > .0001f ||
                        door.TryApplyDamage(new DamageContext(1, default, Team.Enemy, hitId++, lives[i])).Applied))
                    { Finish(false, "Dark movement/damage advanced: " + wagon.Id); yield break; }
                    if (run.Flow.Station > 1 && (wagon.Enemies.Active.Count != 1 || wagon.Enemies.DeathCount != 0 ||
                        !enemy.OnBoard || enemy.NeedsSafeDeparture))
                    { Finish(false, "Outside cleanup/inside retention failed: " + wagon.Id); yield break; }
                }
                wasPaused = run.Flow.Paused;
                yield return null;
            }
            int created = 0; foreach (WagonRuntime wagon in run.Wagons) created += wagon.Enemies.CreatedCount;
            if (run.Flow.Station != 11 || run.Assignments != 10 || created != count * 12 || visited.Count != count)
            { Finish(false, "Ten transitions, one assignment each or bounded pools failed."); yield break; }
            run.Player.Invulnerable = false;
            run.Player.TryApplyDamage(new DamageContext(1000, default, Team.Enemy, 99, run.Player.LifeVersion));
            yield return new WaitForSecondsRealtime(.2f);
            if (run.Flow.Phase != RunPhase.GameOver || run.Player.IsAlive || run.Assignments != 10 || run.Flow.Station != 11)
            { Finish(false, "Death resumed the run or reset a life."); yield break; }
            Finish(true, $"10 transitions, all {count} wagons visited: HP/equipped weapon/magazine/reserve/independent doors persist; camera/player binding correct; dark clock/motion/repair/damage pause; inside/threshold survivors retained, outside cleanup has no kill; {created} pooled bodies; terminal death.");
        }

        private IEnumerator CheckCrowd()
        {
            checking = true; spawner.enabled = false; run.ControlsAllowed = run.AimAllowed = false;
            run.Restart(new RunTimings { initialApproach = .2f, defense = 30 });
            run.Player.Invulnerable = true;
            while (run.Flow.Phase == RunPhase.Approach) yield return null;
            foreach (WagonRuntime wagon in run.Wagons)
                for (int i = 0; i < wagon.Enemies.CreatedCount; i++)
                    if (!wagon.SpawnOutside(i)) { Finish(false, "Crowd spawn failed: " + wagon.Id, "Crowd"); yield break; }
            var samples = new List<float>(1024);
            float started = Time.realtimeSinceStartup;
            int total = 0;
            while (Time.realtimeSinceStartup - started < 6)
            {
                total = 0;
                foreach (WagonRuntime wagon in run.Wagons)
                    foreach (EnemyBrain enemy in wagon.Enemies.Active)
                    {
                        total++;
                        Vector3 local = wagon.transform.InverseTransformPoint(enemy.transform.position);
                        if (enemy.WagonId != wagon.Id || Mathf.Abs(local.x) > 5 || local.z < -9.2f || local.z > 9.2f ||
                            !enemy.GetComponent<WaitYourTurn.Navigation.AgentMotor>().Ready)
                        { Finish(false, "Crowd left its wagon navigation/ownership: " + wagon.Id, "Crowd"); yield break; }
                    }
                if (total != run.Wagons.Length * 12) { Finish(false, "Crowd count/pool budget changed.", "Crowd"); yield break; }
                if (Time.realtimeSinceStartup - started > 1) samples.Add(Time.unscaledDeltaTime * 1000);
                yield return null;
            }
            foreach (WagonRuntime wagon in run.Wagons)
                if (wagon.Doors[0].Durability.Current >= wagon.Doors[0].Durability.Maximum)
                { Finish(false, "No door attack in crowd sample: " + wagon.Id, "Crowd"); yield break; }
            samples.Sort(); float sum = 0; foreach (float ms in samples) sum += ms;
            float mean = samples.Count > 0 ? sum / samples.Count : 0;
            float p95 = samples.Count > 0 ? samples[Mathf.Min(samples.Count - 1, Mathf.FloorToInt(samples.Count * .95f))] : 0;
            Finish(samples.Count > 0, $"{total} simultaneous agents, all wagon doors attacked, ownership/navigation and fixed pools hold. Windows Editor frame mean {mean:F2} ms, p95 {p95:F2} ms ({samples.Count} samples). Diagnostic only; not Android FPS.", "Crowd");
        }
        private void Finish(bool passed, string message, string suffix = "")
        {
            result = $"{(passed ? "PASS" : "FAIL")}: {message}";
#if UNITY_EDITOR
            Directory.CreateDirectory("Logs");
            string lab = run.Wagons.Length == 5 ? "TrainSandbox" : "RunSandbox";
            File.WriteAllText($"Logs/{lab}{suffix}Report.json", JsonUtility.ToJson(new Report
            { passed = passed, station = run.Flow.Station, assignments = run.Assignments, message = message,
                platform = Application.platform.ToString(), utc = DateTime.UtcNow.ToString("O") }, true));
#endif
            if (passed) Debug.Log("[RunSandbox] " + result); else Debug.LogError("[RunSandbox] " + result);
            checking = false; run.Player.Invulnerable = false; run.ControlsAllowed = run.AimAllowed = true;
            if (suffix == "Weapons") foreach (WagonRuntime wagon in run.Wagons)
                foreach (EnemyBrain enemy in wagon.Enemies.Active) enemy.Health.ResetForSpawn(10, Team.Enemy);
            spawner.ResetSchedule(); spawner.enabled = true; run.Restart();
        }
        [Serializable] private sealed class Report { public bool passed; public int station, assignments; public string message, platform, utc; }
    }
    internal static class LabReferences
    {
        public static bool ContainsReference(this System.Collections.Generic.IReadOnlyList<EnemyBrain> enemies, EnemyBrain target)
        { foreach (EnemyBrain enemy in enemies) if (enemy == target) return true; return false; }
    }
}
