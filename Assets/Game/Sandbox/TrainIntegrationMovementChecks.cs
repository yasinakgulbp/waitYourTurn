using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using WaitYourTurn.Combat;
using WaitYourTurn.Navigation;
using WaitYourTurn.Player;
using WaitYourTurn.Run;

namespace WaitYourTurn.Sandbox
{
    public sealed partial class TrainIntegrationController
    {
        // F3: real wagon/door/controller/crowd regression; no alternate movement implementation.
        private IEnumerator CheckMovement()
        {
            checking = true;
            bool fixedSeed = run.UseFixedSeed, randomInitial = run.RandomInitialWagon;
            bool spawning = spawner.enabled;
            var motor = run.Player.GetComponent<PlayerMotor>();
            var body = run.Player.GetComponent<CharacterController>();
            string failure = "Check interrupted before completion";
            try
            {
                run.Match.SetSuppressed(true); spawner.enabled = false;
                run.ConfigureRandomAssignments(true, false);
                run.ControlsAllowed = run.AimAllowed = false; run.Repair.enabled = false;
                run.Restart(new RunTimings { initialApproach = .1f, defense = 300 });
                run.Player.Invulnerable = true;
                motor.ConfigureWorldControl(run.Player, 3.5f);
                yield return new WaitForSeconds(.2f);
                var wagon = run.CurrentWagon;
                while (wagon.Enemies.WarmOne()) { }
                // Both platform sides, an open door and eight overlapping physical enemies.
                foreach (var door in new[] { wagon.Doors.First(), wagon.Doors.Last() })
                {
                    door.Durability.TryApplyDamage(new DamageContext(100000, default, Team.Enemy, 990, door.Durability.LifeVersion));
                    yield return new WaitForSeconds(.2f);
                    Vector3 inward = door.Portal.transform.forward;
                    Vector3 edge = wagon.Area.Constrain(door.transform.position, body.radius + body.skinWidth);
                    edge.y = .05f; motor.Place(edge);
                    for (int i = 0; i < 8; i++)
                    {
                        if (!wagon.SpawnOutside(i)) { failure = "Crowd spawn failed"; yield break; }
                        var enemy = wagon.Enemies.Active.Last();
                        Vector3 point = edge + inward * (.25f + i / 3 * .12f) + door.transform.right * ((i % 3 - 1) * .14f);
                        point.y = 0;
                        if (!enemy.GetComponent<AgentMotor>().TryPlace(point)) { failure = "Crowd placement failed"; yield break; }
                        enemy.SetPaused(true);
                    }
                    Physics.SyncTransforms();
                    for (int frame = 0; frame < 90; frame++)
                    {
                        motor.MoveDisplacement(-inward * .1f);
                        Vector3 center = body.transform.TransformPoint(body.center);
                        if ((center - wagon.Area.Constrain(center, body.radius + body.skinWidth)).sqrMagnitude > .000001f)
                        { failure = "Collision pushed player outside open door: " + door.name; yield break; }
                        yield return new WaitForEndOfFrame();
                        Vector3 physicsCenter = body.bounds.center;
                        if ((physicsCenter - wagon.Area.Constrain(physicsCenter, body.radius + body.skinWidth)).sqrMagnitude > .000001f)
                        { failure = "Physics capsule escaped after frame: " + door.name; yield break; }
                        yield return null;
                    }
                    // Entry remains available to actual zombies; the player-only region is not a door collider.
                    wagon.Enemies.ClearAlive(); motor.Place(wagon.transform.position + Vector3.up * .05f);
                    if (!wagon.SpawnOutside(Array.IndexOf(wagon.Doors, door))) { failure = "Entry spawn failed"; yield break; }
                    var entering = wagon.Enemies.Active.Last();
                    float deadline = Time.time + 12;
                    while (!entering.IsInside && Time.time < deadline) yield return null;
                    if (!entering.IsInside) { failure = "Player boundary blocked zombie entry"; yield break; }
                    wagon.Enemies.ClearAlive();
                }
                motor.Place(wagon.transform.position + Vector3.up * .05f);
                run.Player.transform.rotation = Quaternion.identity;
                motor.SetWorldMove(Vector3.right);
                yield return null; yield return new WaitForEndOfFrame();
                float angle = Quaternion.Angle(Quaternion.identity, run.Player.transform.rotation);
                if (angle <= 0 || angle >= 89.9f) { failure = "Walking turn snapped or never started"; yield break; }
                yield return new WaitForSeconds(.4f);
                if (Vector3.Dot(run.Player.transform.forward, Vector3.right) < .99f) { failure = "Walking turn did not settle"; yield break; }
                motor.SetWorldMove(Vector3.left); yield return new WaitForSeconds(.4f);
                if (Vector3.Dot(run.Player.transform.forward, Vector3.left) < .99f) { failure = "180 degree walking turn did not settle"; yield break; }
                motor.SetWorldMove(Vector3.zero);
                yield return null; yield return new WaitForEndOfFrame();
                Quaternion idle = run.Player.transform.rotation;
                yield return new WaitForSeconds(.15f);
                if (Quaternion.Angle(idle, run.Player.transform.rotation) > .01f) { failure = "Idle heading changed"; yield break; }
                // Region can still be removed for future station exploration.
                motor.SetMovementArea(null); motor.Place(wagon.transform.position + Vector3.up * .05f);
                motor.MoveDisplacement(Vector3.up * .01f);
                motor.SetMovementArea(wagon.Area);
                failure = null;
            }
            finally
            {
                motor.SetWorldMove(Vector3.zero);
                motor.Configure(run.Player.GetComponent<MoveInput>(), run.Player, Camera.main);
                run.ConfigureRandomAssignments(fixedSeed, randomInitial);
                run.ControlsAllowed = run.AimAllowed = true; run.Repair.enabled = true;
                spawner.enabled = spawning; run.Match.SetSuppressed(false); run.Restart();
                checking = false;
                result = failure == null ? "PASS: Both open doors retain the whole player capsule under eight-body crowd pressure for 90 frames each; zombies still enter; gradual 90/180 degree walking and idle heading." : "FAIL: " + failure;
                Directory.CreateDirectory("Logs");
                File.WriteAllText("Logs/PlayerMovementAcceptance.txt", DateTime.UtcNow.ToString("O") + "\n" + result);
                if (failure == null) Debug.Log("[Movement] " + result); else Debug.LogError("[Movement] " + result);
            }
        }
    }
}
