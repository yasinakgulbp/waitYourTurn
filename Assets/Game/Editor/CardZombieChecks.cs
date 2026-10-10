using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using WaitYourTurn.Combat;
using WaitYourTurn.Enemies;
using WaitYourTurn.Player;
using WaitYourTurn.Run;

namespace WaitYourTurn.Editor
{
    public static class CardZombieChecks
    {
        private static bool checking;
        public static void Start()
        {
            if (!EditorApplication.isPlaying || checking) throw new InvalidOperationException("Play Survival first");
            var run = UnityEngine.Object.FindAnyObjectByType<RunDriver>();
            if (run == null || !run.UsesOpenTrainSurvival) throw new InvalidOperationException("Survival required");
            checking = true; run.StartCoroutine(Check(run));
        }
        private static void Require(bool ok, string reason) { if (!ok) throw new InvalidOperationException(reason); }
        private static IEnumerator Check(RunDriver run)
        {
            var save = UnityEngine.Object.FindAnyObjectByType<RunPersistence>(); string oldSave = save.SavePath;
            var spawner = UnityEngine.Object.FindAnyObjectByType<StationSpawner>();
            var presentation = UnityEngine.Object.FindAnyObjectByType<RunPresentation>();
            var journey = UnityEngine.Object.FindAnyObjectByType<SurvivalJourney>();
            var motor = run.Player.GetComponent<PlayerMotor>();
            var report = new System.Text.StringBuilder(DateTime.UtcNow.ToString("O") + "\nEditor-only acceptance; not Android acceptance.\n");
            try
            {
                save.UseTestStore(Path.Combine(Application.persistentDataPath, "acceptance-only", "card-zombies.json"));
                run.Restart(); spawner.SpawningAllowed = false; run.Player.Invulnerable = true; run.AimAllowed = false;
                motor.ConfigureWorldControl(run.Player, 3.5f);
                run.Player.transform.rotation = Quaternion.identity; motor.SetWorldMove(Vector3.right);
                yield return null;
                Require(Vector3.Angle(run.Player.transform.forward, Vector3.forward) < 40, "Turn must not snap on first frame");
                yield return new WaitForSeconds(.7f); motor.SetWorldMove(Vector3.zero);
                Require(Vector3.Dot(run.Player.transform.forward, Vector3.right) > .97f, "Eased turn converges");
                report.Append("PASS: gradual first-frame turn and convergence; blur .70.\n");
                run.Flow.Tick(run.Flow.Remaining + .01f); yield return null;
                Require(run.Flow.Phase == RunPhase.Defense && presentation.Speed == 0, "Stress fixture must use stopped station");
                foreach (var wagon in run.Wagons) while (wagon.Enemies.WarmOne()) { }
                var profiles = new[] { "Normal", "Fast", "Tough" }.Select(n =>
                    AssetDatabase.LoadAssetAtPath<EnemyProfile>("Assets/Game/Content/StationPrograms/" + n + ".asset")).ToArray();
                int count = 0;
                foreach (var wagon in run.Wagons) for (int i = 0; i < 8; i++)
                    if (wagon.Enemies.TrySpawn(wagon.transform.position + new Vector3(-4.5f + i % 4 * 3, 0, (i < 4 ? -1 : 1) * 5.5f), profiles[i % 3], 1)) count++;
                Require(count == 24, "Expected 24 stress actors");
                foreach (var wagon in run.Wagons) foreach (var enemy in wagon.Enemies.Active) enemy.Health.Invulnerable = true;
                yield return new WaitForSeconds(.5f);
                var times = new float[2048]; int frames = 0; double total = 0; float start = Time.unscaledTime;
                bool animated = false;
                while (Time.unscaledTime - start < 4 && frames < times.Length)
                {
                    motor.SetWorldMove(Vector3.right * (Mathf.Sin((Time.unscaledTime - start) * 3) > 0 ? 1 : -1));
                    yield return null; float ms = Time.unscaledDeltaTime * 1000; times[frames++] = ms; total += ms;
                    foreach (var wagon in run.Wagons) foreach (var enemy in wagon.Enemies.Active)
                    {
                        var art = enemy.GetComponentInChildren<CardZombieVisual>();
                        Require(art != null && art.Skin.bones.Length == 15 && art.Skin.sharedMaterials.Length == 1, "Single material skeletal art");
                        Require(art.Skin.sharedMesh.vertexCount < 4000 && art.Skin.quality == SkinQuality.Bone2, "Updated native mesh buffers and blended joints");
                        Require(enemy.GetComponentsInChildren<MeshRenderer>().All(r => !r.enabled), "Legacy capsule/stripe hidden");
                        Require((enemy.transform.position - enemy.GetComponent<NavMeshAgent>().nextPosition).sqrMagnitude < .01f, "Visual animation must not drift root from navigation");
                        animated |= art.WalkingBlend > .5f && Quaternion.Angle(art.Skin.bones[9].localRotation, Quaternion.identity) > 3;
                    }
                }
                motor.SetWorldMove(Vector3.zero); Require(animated, "Moving actors must animate legs"); Array.Sort(times, 0, frames);
                report.Append(FormattableString.Invariant($"PASS: 24 mixed-profile actors, one renderer/15 bones, real movement-driven poses and nav-root agreement.\nFrames={frames}; meanMs={total / frames:F2}; P95ms={times[(int)(frames * .95)]:F2}; view={Screen.width}x{Screen.height}; quality={QualitySettings.names[QualitySettings.GetQualityLevel()]}\n"));
                yield return new WaitForEndOfFrame(); ScreenCapture.CaptureScreenshot("docs/generated/card-zombies-game.png");
                // Real boarding-wave policy, with open entries so acceptance need not wait for door damage.
                run.Restart(); run.Player.Invulnerable = true; run.AimAllowed = false; motor.SetWorldMove(Vector3.zero);
                run.Repair.enabled = false; spawner.SpawningAllowed = true;
                run.Flow.Tick(run.Flow.Remaining + .01f);
                foreach (var wagon in run.Wagons) foreach (var door in wagon.Doors)
                    door.Durability.TryApplyDamage(new DamageContext(9999, default, Team.Enemy, 99101, door.Durability.LifeVersion));
                bool hadOutside = false; start = Time.unscaledTime;
                while (run.Flow.Phase == RunPhase.Defense && Time.unscaledTime - start < 24)
                {
                    yield return null;
                    if (journey.ExteriorAlive > 0)
                    { hadOutside = true; Require(run.Flow.Phase == RunPhase.Defense && presentation.Speed == 0, "Exterior attackers prevent departure"); }
                }
                Require(hadOutside && run.Flow.Phase == RunPhase.Departing && journey.ExteriorAlive == 0, "All real-wave attackers board before departure");
                Require(spawner.TotalActive > 0, "Boarded enemies are retained for travel");
                yield return new WaitForSeconds(.4f);
                yield return new WaitForEndOfFrame(); ScreenCapture.CaptureScreenshot("docs/generated/card-zombies-boarded.png");
                report.Append("PASS: real four-enemy intro wave blocks departure while any attacker is outside; boarded enemies retained, zero exterior actors at departure.\n");
                // Pool reuse must return to Normal art and clear motion from the old life.
                run.Restart(); spawner.SpawningAllowed = false; run.Flow.Tick(run.Flow.Remaining + .01f);
                Require(run.CurrentWagon.SpawnOutside(0, profiles[0], 1), "Pool reuse spawn");
                var reused = run.CurrentWagon.Enemies.Active[0].GetComponentInChildren<CardZombieVisual>();
                Require(reused.Skin.sharedMesh == AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Game/Art/CardZombies/Meshes/Normal.asset") && reused.transform.localScale == Vector3.one && reused.WalkingBlend == 0,
                    "Reused life resets mesh, scale and walking state");
                report.Append("PASS: pooled new life restores basic mesh/scale and clears old animation. Normal save isolated.\n");
                File.WriteAllText("docs/generated/card-zombies-check.txt", report.ToString()); Debug.Log("[CardZombieCheck] " + report);
            }
            finally
            {
                motor.SetWorldMove(Vector3.zero); motor.Configure(run.Player.GetComponent<MoveInput>(), run.Player, Camera.main);
                run.Repair.enabled = true; run.ControlsAllowed = run.AimAllowed = spawner.SpawningAllowed = true;
                save.UseTestStore(oldSave); checking = false; run.Restart();
            }
        }
    }
}
