using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using WaitYourTurn.Combat;

namespace WaitYourTurn.Tests
{
    public sealed class RadialDamageTests
    {
        private readonly List<GameObject> objects = new List<GameObject>();
        private GameObject ObjectAt(string name, Vector3 position)
        { var obj = new GameObject(name); obj.transform.position = position; objects.Add(obj); return obj; }
        private HealthComponent Body(Vector3 position, Team team, float hp = 100)
        {
            var obj = ObjectAt("Body", position); var body = obj.AddComponent<HealthComponent>(); body.ResetForSpawn(hp, team);
            var box = obj.AddComponent<BoxCollider>(); box.center = Vector3.up * .85f; box.size = new Vector3(.4f, 1.7f, .4f); return body;
        }
        [TearDown] public void Cleanup() { foreach (var obj in objects) UnityEngine.Object.DestroyImmediate(obj); objects.Clear(); }
        [Test] public void EachRegisteredBodyGetsOneHitAndCombatantsDoNotOccludeBlast()
        {
            var registry = ObjectAt("Registry", Vector3.zero).AddComponent<TargetRegistry>();
            var source = Body(Vector3.zero, Team.Player); var first = Body(Vector3.forward, Team.Enemy, 5);
            first.gameObject.AddComponent<BoxCollider>();
            var second = Body(Vector3.forward * 2, Team.Enemy); var friend = Body(Vector3.left, Team.Player);
            registry.Register(first); registry.Register(first); registry.Register(second); registry.Register(friend);
            EntityIdentity credited = new EntityIdentity(123, 7); DeathNotice death = default; first.Died += d => death = d;
            Physics.SyncTransforms();
            Assert.That(new RadialDamage().Apply(registry, source, credited, Vector3.up * .9f, 3, 10, Physics.AllLayers), Is.EqualTo(2));
            Assert.That(second.Current, Is.EqualTo(90)); Assert.That(friend.Current, Is.EqualTo(100));
            Assert.That(death.Damage.RewardOwner.RuntimeId, Is.EqualTo(123)); Assert.That(death.Damage.RewardOwner.LifeVersion, Is.EqualTo(7));
        }
        [Test] public void MetalBlocksBlastButShotTransparentGlassDoesNot()
        {
            var registry = ObjectAt("Registry", Vector3.zero).AddComponent<TargetRegistry>();
            var source = Body(Vector3.zero, Team.Player); var target = Body(Vector3.forward * 2, Team.Enemy); registry.Register(target);
            var metal = ObjectAt("Neutral door metal", Vector3.forward);
            metal.AddComponent<HealthComponent>().ResetForSpawn(10, Team.Neutral);
            var box = metal.AddComponent<BoxCollider>(); box.center = Vector3.up * .9f; box.size = new Vector3(1, 2, .2f);
            int glass = LayerMask.NameToLayer("ShotTransparent"); Assert.That(glass, Is.GreaterThanOrEqualTo(0));
            var blast = new RadialDamage(); Physics.SyncTransforms();
            Assert.That(blast.Apply(registry, source, source.Identity, Vector3.up * .9f, 3, 10, ~(1 << glass)), Is.Zero);
            metal.layer = glass; Physics.SyncTransforms();
            Assert.That(blast.Apply(registry, source, source.Identity, Vector3.up * .9f, 3, 10, ~(1 << glass)), Is.EqualTo(1));
            Assert.That(target.Current, Is.EqualTo(90));
        }
        [Test] public void ResetTargetLifeDuringAnotherDeathCannotDamageTheNewLife()
        {
            var registry = ObjectAt("Registry", Vector3.zero).AddComponent<TargetRegistry>();
            var source = Body(Vector3.zero, Team.Player); var first = Body(Vector3.forward, Team.Enemy, 1);
            var reused = Body(Vector3.right, Team.Enemy); registry.Register(first); registry.Register(reused);
            first.Died += _ => reused.ResetForSpawn(100, Team.Enemy); Physics.SyncTransforms();
            Assert.That(new RadialDamage().Apply(registry, source, source.Identity, Vector3.up * .9f, 3, 10, Physics.AllLayers), Is.EqualTo(1));
            Assert.That(reused.Current, Is.EqualTo(100));
        }
        [Test] public void InvalidOrOutOfRangeBlastHasNoEffect()
        {
            var registry = ObjectAt("Registry", Vector3.zero).AddComponent<TargetRegistry>();
            var source = Body(Vector3.zero, Team.Player); var target = Body(Vector3.forward * 4, Team.Enemy); registry.Register(target);
            var blast = new RadialDamage(); Physics.SyncTransforms();
            Assert.That(blast.Apply(registry, source, source.Identity, Vector3.up * .9f, 1, 10, Physics.AllLayers), Is.Zero);
            Assert.That(blast.Apply(registry, source, source.Identity, Vector3.up * .9f, float.NaN, 10, Physics.AllLayers), Is.Zero);
            Assert.That(blast.Apply(registry, source, source.Identity, Vector3.up * .9f, 10, -1, Physics.AllLayers), Is.Zero);
            Assert.That(target.Current, Is.EqualTo(100));
        }
        [Test] public void OccludedNearerTargetDoesNotHideVisibleFartherTarget()
        {
            var registry = ObjectAt("Registry", Vector3.zero).AddComponent<TargetRegistry>(); var source = Body(Vector3.zero, Team.Player);
            var near = Body(Vector3.forward * 2, Team.Enemy); var far = Body(new Vector3(2, 0, 3), Team.Enemy);
            registry.Register(near); registry.Register(far);
            var cover = ObjectAt("Cover", Vector3.forward); var box = cover.AddComponent<BoxCollider>();
            box.center = Vector3.up; box.size = new Vector3(.6f, 2, .2f);
            var weapon = source.gameObject.AddComponent<HitscanWeapon>(); weapon.Configure(source); Physics.SyncTransforms();
            Assert.That(NearestVisibleTarget.Select(registry, weapon, source.transform.position), Is.EqualTo(far));
            Assert.That(weapon.TryFire(far.transform.position + Vector3.up * .85f - weapon.Muzzle), Is.True);
            Assert.That(far.Current, Is.EqualTo(98)); Assert.That(near.Current, Is.EqualTo(100));
        }
    }
}
