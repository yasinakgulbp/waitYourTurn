using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using WaitYourTurn.Combat;
using WaitYourTurn.Defenses;

namespace WaitYourTurn.Tests
{
    public sealed class DroneFlightTests
    {
        private readonly List<GameObject> objects = new List<GameObject>();
        // Isolate from any currently open scene geometry.
        private readonly Vector3 origin = new Vector3(1000, 1000, 1000);
        private BoxCollider Box(float x, int layer = 0)
        {
            var obj = new GameObject("Flight test obstacle"); objects.Add(obj);
            obj.layer = layer; obj.transform.position = origin + Vector3.right * x;
            var box = obj.AddComponent<BoxCollider>(); box.size = new Vector3(.2f, 2, 2); return box;
        }
        [TearDown] public void Cleanup() { foreach (var o in objects) Object.DestroyImmediate(o); objects.Clear(); }
        [Test] public void GlassBlocksFlightEvenWhenExcludedFromBulletMask()
        {
            int glass = LayerMask.NameToLayer("ShotTransparent"); Assert.That(glass, Is.GreaterThanOrEqualTo(0));
            Box(1, glass); Physics.SyncTransforms();
            Assert.That(new DroneFlight().Sweep(origin, origin + Vector3.right * 2, .14f, Physics.AllLayers, out var at), Is.False);
            Assert.That(at.x, Is.LessThan(origin.x + .9f));
            Assert.That(Physics.Raycast(origin, Vector3.right, 2, ~(1 << glass), QueryTriggerInteraction.Ignore), Is.False);
        }
        [Test] public void RepairedDoorAroundDroneFailsClosed()
        {
            var box = Box(0); box.enabled = false; Physics.SyncTransforms(); var flight = new DroneFlight();
            Assert.That(flight.Sweep(origin, origin + Vector3.right * 2, .14f, Physics.AllLayers, out _), Is.True);
            box.enabled = true; Physics.SyncTransforms();
            Assert.That(flight.Sweep(origin, origin + Vector3.right * 2, .14f, Physics.AllLayers, out var at), Is.False);
            Assert.That(at, Is.EqualTo(origin));
        }
        [Test] public void CombatantBodiesDoNotBlockDiveButMetalBehindThemDoes()
        {
            var enemy = Box(.5f); enemy.gameObject.AddComponent<HealthComponent>().ResetForSpawn(10, Team.Enemy);
            var metal = Box(1.5f); Physics.SyncTransforms(); var flight = new DroneFlight();
            Assert.That(flight.Sweep(origin, origin + Vector3.right * 2, .14f, Physics.AllLayers, out var at), Is.False);
            Assert.That(at.x, Is.InRange(origin.x + 1.2f, origin.x + 1.3f));
            metal.enabled = false; Physics.SyncTransforms();
            Assert.That(flight.Sweep(origin, origin + Vector3.right * 2, .14f, Physics.AllLayers, out _), Is.True);
        }
        [Test] public void QuerySaturationNeverAllowsPassThrough()
        {
            for (int i = 0; i < 65; i++) Box(0);
            Physics.SyncTransforms(); var flight = new DroneFlight();
            Assert.That(flight.Sweep(origin, origin + Vector3.right * 2, .14f, Physics.AllLayers, out var at), Is.False);
            Assert.That(at, Is.EqualTo(origin)); Assert.That(flight.Overflows, Is.EqualTo(1));
        }
    }
}
