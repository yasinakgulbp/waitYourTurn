using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using WaitYourTurn.Combat;
using WaitYourTurn.Enemies;
using WaitYourTurn.Run;

namespace WaitYourTurn.Editor
{
    public static class CardContactChecks
    {
        public static void Start()
        {
            if (!EditorApplication.isPlaying) throw new InvalidOperationException("Play Survival first");
            var run = UnityEngine.Object.FindAnyObjectByType<RunDriver>();
            run.StartCoroutine(Check(run));
        }
        private static void Require(bool ok, string message) { if (!ok) throw new InvalidOperationException(message); }
        private static IEnumerator Check(RunDriver run)
        {
            var persistence = UnityEngine.Object.FindAnyObjectByType<RunPersistence>(); string save = persistence.SavePath;
            var spawner = UnityEngine.Object.FindAnyObjectByType<StationSpawner>();
            var report = new System.Text.StringBuilder(DateTime.UtcNow.ToString("O") + "\nShort PC contact/death acceptance; not Android.\n");
            try
            {
                persistence.UseTestStore(Path.Combine(Application.persistentDataPath, "acceptance-only", "card-contact.json"));
                run.Restart(); spawner.SpawningAllowed = false; run.AimAllowed = false; run.ControlsAllowed = false; run.Repair.enabled = false;
                run.Flow.Tick(run.Flow.Remaining + .01f); yield return null;
                var pool = run.CurrentWagon.Enemies;
                while (pool.WarmOne()) { }
                foreach (string name in new[] { "Normal", "Fast", "Tough" })
                {
                    var profile = AssetDatabase.LoadAssetAtPath<EnemyProfile>("Assets/Game/Content/StationPrograms/" + name + ".asset");
                    Vector3 hero = run.Player.transform.position;
                    var enemy = pool.RestoreEnemy(hero + Vector3.forward * 1.25f, run.CurrentWagon.Doors[0], profile, 1, 0);
                    Require(enemy != null, "Contact fixture spawn " + name);
                    enemy.enabled = false; enemy.GetComponent<NavMeshAgent>().isStopped = true;
                    enemy.transform.rotation = Quaternion.LookRotation(Vector3.back);
                    var melee = enemy.GetComponent<MeleeAttack>(); var art = enemy.GetComponentInChildren<CardZombieVisual>();
                    yield return null;
                    Require(!melee.CanReach(run.Player), name + " must not strike from 1.25m");
                    float before = run.Player.Current;
                    for (int i = 0; i < 3; i++) { melee.Tick(run.Player); yield return null; }
                    Require(run.Player.Current == before, "Far attack dealt damage");
                    enemy.GetComponent<NavMeshAgent>().Warp(hero + Vector3.forward * (melee.ActorReach - .015f));
                    Physics.SyncTransforms(); melee.ResetAttack(); run.Player.Invulnerable = false; run.Player.SetDamageProtection(0);
                    Require(melee.CanReach(run.Player), "Close target unreachable " + name);
                    var blocker = new GameObject("Acceptance-only metal blocker");
                    blocker.transform.position = (hero + enemy.transform.position) * .5f + Vector3.up * .85f;
                    blocker.AddComponent<BoxCollider>().size = new Vector3(.8f, 1.7f, .04f);
                    Physics.SyncTransforms(); Require(!melee.CanReach(run.Player), "Metal must block melee " + name);
                    UnityEngine.Object.Destroy(blocker); yield return null; Physics.SyncTransforms();
                    before = run.Player.Current; float started = Time.time; bool hit = false; float nearest = 100;
                    while (Time.time - started < melee.Windup + .12f)
                    {
                        melee.Tick(run.Player);
                        yield return new WaitForEndOfFrame();
                        if (art.ContactBlend > .98f)
                            foreach (int hand in new[] { 5, 8 })
                                nearest = Mathf.Min(nearest, Vector3.Distance(art.Skin.bones[hand].TransformPoint(new Vector3(0, -.15f, .10f)), art.LastContactPoint));
                        hit |= run.Player.Current < before;
                    }
                    Require(hit && Mathf.Abs(before - run.Player.Current - profile.damage) < .01f, "Expected one contact hit " + name);
                    Require(nearest < .20f, "Visible claw contact missed " + name + ": " + nearest);
                    report.Append(name + ": PASS far target rejected; one impact after windup; closest claw gap=" + nearest.ToString("F3") + "m.\n");
                    melee.ResetAttack(); melee.Tick(run.Player);
                    enemy.GetComponent<NavMeshAgent>().Warp(hero + Vector3.forward * 1.25f); Physics.SyncTransforms();
                    before = run.Player.Current; started = Time.time;
                    while (Time.time - started < melee.Windup + .04f) { melee.Tick(run.Player); yield return null; }
                    Require(run.Player.Current == before && !melee.WindingUp, "Moved target must cancel impact");
                    enemy.Health.TryApplyDamage(new DamageContext(1, run.Player.Identity, Team.Player, 11001, enemy.Health.LifeVersion));
                    yield return new WaitForSeconds(.12f);
                    Require(art.HitBlend > .5f, "Applied damage must show recoil");
                    int rewards = 0; Action<DeathNotice, int> reward = (d, r) => rewards++;
                    pool.Killed += reward;
                    enemy.enabled = true;
                    enemy.Health.TryApplyDamage(new DamageContext(9999, run.Player.Identity, Team.Player, 11002, enemy.Health.LifeVersion));
                    pool.Killed -= reward;
                    yield return null;
                    Require(rewards == 1 && pool.Active.Count == 0 && pool.CanRent, "Death must pay/return once without waiting for art");
                    Require(pool.Corpses != null && pool.Corpses.ActiveCount > 0, "Death skin must survive original pooled life");
                    Require(pool.Corpses.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length == 3, "Fixed three-slot corpse budget");
                    Require(pool.Corpses.GetComponentsInChildren<HealthComponent>(true).Length == 0 &&
                        pool.Corpses.GetComponentsInChildren<Collider>(true).Length == 0, "Death skins must not own combat or collisions");
                    report.Append("PASS moving-target cancellation, recoil, immediate pool/reward return and collider-free death skin.\n");
                }
                yield return new WaitForSeconds(1.3f);
                Require(run.CurrentWagon.Enemies.Corpses.ActiveCount == 0, "Death pool must expire");
                File.WriteAllText("docs/generated/card-contact-check.txt", report.ToString()); Debug.Log("[CardContactCheck] " + report);
            }
            finally
            {
                foreach (var wagon in run.Wagons) foreach (var enemy in wagon.Enemies.Active) enemy.enabled = true;
                run.Repair.enabled = true; run.ControlsAllowed = run.AimAllowed = spawner.SpawningAllowed = true;
                persistence.UseTestStore(save); run.Restart();
            }
        }
    }
}
