using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using WaitYourTurn.Combat;
using Object = UnityEngine.Object;

namespace WaitYourTurn.Tests
{
    public sealed class WeaponTests
    {
        private readonly List<Object> created = new List<Object>();
        private float oldTimeScale;
        [SetUp] public void Before() { oldTimeScale = Time.timeScale; Time.timeScale = 1; }
        [TearDown] public void After()
        { for (int i = created.Count - 1; i >= 0; i--) if (created[i] != null) Object.DestroyImmediate(created[i]);
            created.Clear(); Time.timeScale = oldTimeScale; Physics.SyncTransforms(); }
        [Test] public void FiniteReserveLoadsPartialLastMagazineAndCannotCreateAmmo()
        {
            var state = new WeaponState(new WeaponSpec(2, .1f, 10, 2, 1, false, 1));
            Assert.That(state.TryFire(0)); Assert.That(state.TryFire(.2f));
            Assert.That(state.Reloading); Assert.That(state.TryFire(1), Is.False);
            state.Tick(1.3f); Assert.That(state.Rounds, Is.EqualTo(1)); Assert.That(state.Reserve, Is.Zero);
            Assert.That(state.TryFire(2)); state.Tick(100);
            Assert.That(state.Empty); Assert.That(state.Reloading, Is.False); Assert.That(state.TryFire(100), Is.False);
        }
        [Test] public void PistolAutoReloadHasUnlimitedReserve()
        {
            var state = new WeaponState(new WeaponSpec(2, .1f, 10, 1, 1));
            Assert.That(state.TryFire(0)); state.Tick(1); Assert.That(state.Rounds, Is.EqualTo(1));
            Assert.That(state.TryFire(2)); state.Tick(3); Assert.That(state.Rounds, Is.EqualTo(1));
        }
        [Test] public void SlowFrameNeverCatchesUpOrFiresTwiceAtSameTime()
        {
            var state = new WeaponState(WeaponSpec.Pistol);
            Assert.That(state.TryFire(0)); Assert.That(state.TryFire(100));
            Assert.That(state.TryFire(100), Is.False); Assert.That(state.Rounds, Is.EqualTo(6));
        }
        [Test] public void FrozenClockCannotFinishReload()
        {
            var state = new WeaponState(new WeaponSpec(2, .1f, 10, 1, 1, false, 2));
            state.TryFire(5); for (int i = 0; i < 100; i++) state.Tick(5);
            Assert.That(state.Reloading); Assert.That(state.Reserve, Is.EqualTo(2));
            state.Tick(6); Assert.That(state.Rounds, Is.EqualTo(1)); Assert.That(state.Reserve, Is.EqualTo(1));
        }
        [TestCase(0f)] [TestCase(float.NaN)] [TestCase(float.PositiveInfinity)]
        public void InvalidRulesRejected(float interval) => Assert.Throws<ArgumentOutOfRangeException>(() => new WeaponSpec(2, interval, 10, 8, 1));
        [Test] public void InvalidClockCannotConsumeAmmo()
        { var state = new WeaponState(WeaponSpec.Pistol); Assert.That(state.TryFire(float.NaN), Is.False);
            Assert.That(state.TryFire(float.PositiveInfinity), Is.False); Assert.That(state.Rounds, Is.EqualTo(8)); }
        [Test] public void SwitchRetainsAmmoAndDoesNotBypassTriggerCooldown()
        {
            HitscanWeapon gun = Gun(); gun.ConfigureDefinitions(new[] { Profile(1), Profile(1) });
            Assert.That(gun.TryFire(Vector3.forward)); WeaponState first = gun.State;
            Assert.That(gun.Equip(1)); Assert.That(gun.TryFire(Vector3.forward), Is.False);
            Assert.That(gun.Equip(0)); Assert.That(gun.State, Is.SameAs(first)); Assert.That(gun.Rounds, Is.EqualTo(7));
            Assert.That(gun.Equip(-1), Is.False); Assert.That(gun.Rounds, Is.EqualTo(7));
        }
        [Test] public void CompoundCollidersTakeOneHitPerPelletAndShotgunCostsOneRound()
        {
            HitscanWeapon gun = Gun(); gun.ConfigureDefinitions(new[] { Profile(6) });
            HealthComponent target = Body("Compound enemy", new Vector3(100, 0, 104), Team.Enemy);
            var child = new GameObject("Second collider"); child.transform.SetParent(target.transform, false);
            child.AddComponent<BoxCollider>().center = Vector3.up * .85f; Physics.SyncTransforms();
            Assert.That(gun.TryFire(Vector3.forward)); Assert.That(target.Current, Is.EqualTo(88));
            Assert.That(gun.Rounds, Is.EqualTo(7));
        }
        [Test] public void SolidWallAndFriendlyBodyBlockShotsButExcludedWindowLayerDoesNot()
        {
            HitscanWeapon gun = Gun(); HealthComponent target = Body("Enemy", new Vector3(100, 0, 104), Team.Enemy);
            GameObject wall = New("Wall"); wall.transform.position = new Vector3(100, .9f, 102); wall.AddComponent<BoxCollider>();
            Physics.SyncTransforms(); Assert.That(gun.HasSight(target), Is.False); gun.TryFire(Vector3.forward);
            Assert.That(target.Current, Is.EqualTo(100));
            wall.layer = 2; gun.SetHitMask(~(1 << 2)); gun.ResetWeapon(); Physics.SyncTransforms();
            Assert.That(gun.HasSight(target)); gun.TryFire(Vector3.forward); Assert.That(target.Current, Is.EqualTo(98));
            HealthComponent friend = wall.AddComponent<HealthComponent>(); friend.ResetForSpawn(100, Team.Player);
            wall.layer = 0; gun.ResetWeapon(); Physics.SyncTransforms(); gun.TryFire(Vector3.forward);
            Assert.That(target.Current, Is.EqualTo(98)); Assert.That(friend.Current, Is.EqualTo(100));
        }
        [Test] public void PausedOrDeadOwnerCannotFireOrEquip()
        {
            HitscanWeapon gun = Gun(); gun.Paused = true;
            Assert.That(gun.TryFire(Vector3.forward), Is.False); Assert.That(gun.Equip(0), Is.False);
            gun.Paused = false; HealthComponent owner = gun.GetComponent<HealthComponent>();
            owner.TryApplyDamage(new DamageContext(1000, default, Team.Enemy, 1, owner.LifeVersion));
            Assert.That(gun.TryFire(Vector3.forward), Is.False); Assert.That(gun.Equip(0), Is.False); Assert.That(gun.Rounds, Is.EqualTo(8));
        }
        [Test] public void ShotgunSpreadStaysInConeWithoutChangingUnityRandom()
        {
            UnityEngine.Random.State saved = UnityEngine.Random.state;
            Vector3 first = HitscanResolver.PelletDirection(Vector3.forward, 0, 6, 9, 1);
            Assert.That(first, Is.EqualTo(Vector3.forward));
            for (int i = 1; i < 6; i++) Assert.That(Vector3.Angle(Vector3.forward,
                HitscanResolver.PelletDirection(Vector3.forward, i, 6, 9, 1)), Is.LessThanOrEqualTo(9.001f));
            Assert.That(UnityEngine.Random.state, Is.EqualTo(saved));
        }
        private GameObject New(string name) { var obj = new GameObject(name); created.Add(obj); return obj; }
        private HitscanWeapon Gun()
        {
            GameObject obj = New("Weapon owner"); obj.transform.position = new Vector3(100, 0, 100);
            var owner = obj.AddComponent<HealthComponent>(); owner.ResetForSpawn(100, Team.Player);
            var gun = obj.AddComponent<HitscanWeapon>(); gun.Configure(owner); return gun;
        }
        private HealthComponent Body(string name, Vector3 position, Team team)
        { GameObject obj = New(name); obj.transform.position = position; BoxCollider body = obj.AddComponent<BoxCollider>();
            body.center = Vector3.up * .85f; body.size = new Vector3(1, 1.7f, 1);
            var health = obj.AddComponent<HealthComponent>(); health.ResetForSpawn(100, team); return health; }
        private WeaponDefinition Profile(int pellets)
        { var definition = ScriptableObject.CreateInstance<WeaponDefinition>(); created.Add(definition);
            definition.pellets = pellets; definition.infiniteReserve = false; definition.initialReserve = 3; return definition; }
    }
}
