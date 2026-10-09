using NUnit.Framework;
using UnityEngine;
using WaitYourTurn.Combat;

namespace WaitYourTurn.Tests
{
    public sealed class HealthPersistenceTests
    {
        [Test] public void LoadDoesNotEmitCombatDeathAndRejectsPreviousRuntimeLife()
        {
            var obj = new GameObject("health restore test");
            try
            {
                var body = obj.AddComponent<HealthComponent>(); int deaths = 0; body.Died += _ => deaths++;
                uint oldLife = body.LifeVersion;
                Assert.That(body.Restore(37, 140), Is.True); Assert.That(body.Current, Is.EqualTo(37)); Assert.That(body.Maximum, Is.EqualTo(140));
                Assert.That(body.TryApplyDamage(new DamageContext(5, default, Team.Enemy, 1, oldLife)).Applied, Is.False);
                Assert.That(body.Restore(0, 140), Is.True); Assert.That(body.IsAlive, Is.False); Assert.That(deaths, Is.Zero);
                Assert.That(body.Restore(141, 140), Is.False); Assert.That(body.Current, Is.Zero);
            }
            finally { Object.DestroyImmediate(obj); }
        }
    }
}
