using NUnit.Framework;
using WaitYourTurn.Combat;

namespace WaitYourTurn.Weapons.Tests
{
    public sealed class AmmoPersistenceTests
    {
        [Test] public void ReloadAndFireDeadlineRebaseWithoutRefill()
        {
            var spec = new WeaponSpec(2, .35f, 10, 2, 1.2f, false, 5);
            var original = new WeaponState(spec); original.TryFire(10); original.TryFire(10.4f);
            var saved = original.Capture(10.7f); var resumed = new WeaponState(spec);
            Assert.That(resumed.Restore(saved, 200), Is.True); Assert.That(resumed.Rounds, Is.Zero); Assert.That(resumed.Reserve, Is.EqualTo(5));
            Assert.That(resumed.TryFire(200), Is.False); resumed.Tick(200.91f);
            Assert.That(resumed.Rounds, Is.EqualTo(2)); Assert.That(resumed.Reserve, Is.EqualTo(3));
            saved.rounds = 999; Assert.That(resumed.Restore(saved, 300), Is.False);
        }
    }
}
