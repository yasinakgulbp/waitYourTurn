using System;

namespace WaitYourTurn.Combat
{
    /// <summary>Immutable rules. Reserve excludes the loaded magazine; shared by any weapon owner.</summary>
    public readonly struct WeaponSpec
    {
        public readonly float Damage, Interval, Range, ReloadSeconds, SpreadDegrees;
        public readonly int MagazineSize, InitialReserve, Pellets;
        public readonly bool InfiniteReserve;
        public WeaponSpec(float damage, float interval, float range, int magazineSize, float reloadSeconds,
            bool infiniteReserve = true, int initialReserve = 0, int pellets = 1, float spreadDegrees = 0)
        {
            if (!Positive(damage) || !Positive(interval) || !Positive(range) || !Positive(reloadSeconds) ||
                magazineSize < 1 || initialReserve < 0 || pellets < 1 || pellets > 16 ||
                float.IsNaN(spreadDegrees) || float.IsInfinity(spreadDegrees) || spreadDegrees < 0 || spreadDegrees > 45)
                throw new ArgumentOutOfRangeException(nameof(damage), "Invalid weapon rules.");
            Damage = damage; Interval = interval; Range = range; MagazineSize = magazineSize;
            ReloadSeconds = reloadSeconds; InfiniteReserve = infiniteReserve;
            InitialReserve = initialReserve; Pellets = pellets; SpreadDegrees = spreadDegrees;
        }
        public static WeaponSpec Pistol => new WeaponSpec(2, .35f, 10, 8, 1.2f);
        private static bool Positive(float value) => value > 0 && !float.IsNaN(value) && !float.IsInfinity(value);
    }

    /// <summary>One weapon's ammo and scaled-clock deadlines; no input, Unity, VFX or inventory dependencies.</summary>
    public sealed class WeaponState
    {
        public WeaponSpec Spec { get; }
        public int Rounds { get; private set; }
        public int Reserve { get; private set; }
        public bool Reloading => reloadUntil >= 0;
        public bool Empty => Rounds == 0 && !Spec.InfiniteReserve && Reserve == 0;
        public bool NeedsRefill => Rounds < Spec.MagazineSize || !Spec.InfiniteReserve && Reserve < Spec.InitialReserve;
        private float nextShot, reloadUntil = -1;
        public WeaponState(WeaponSpec spec)
        {
            if (!(spec.Interval > 0) || spec.MagazineSize < 1) throw new ArgumentException("Uninitialized weapon rules.");
            Spec = spec; Rounds = spec.MagazineSize; Reserve = spec.InitialReserve;
        }
        public void Tick(float now)
        {
            if (float.IsNaN(now) || float.IsInfinity(now) || !Reloading || now < reloadUntil) return;
            int loaded = Spec.InfiniteReserve ? Spec.MagazineSize : Math.Min(Spec.MagazineSize, Reserve);
            Rounds = loaded;
            if (!Spec.InfiniteReserve) Reserve -= loaded;
            reloadUntil = -1;
        }
        public bool TryFire(float now)
        {
            if (float.IsNaN(now) || float.IsInfinity(now) || now < 0) return false;
            Tick(now);
            if (Reloading || Rounds <= 0 || now < nextShot) return false;
            Rounds--; nextShot = now + Spec.Interval; // No overdue burst after a slow frame.
            if (Rounds == 0 && (Spec.InfiniteReserve || Reserve > 0)) reloadUntil = now + Spec.ReloadSeconds;
            return true;
        }
        public bool TryRefill()
        {
            if (!NeedsRefill) return false;
            Rounds = Spec.MagazineSize; Reserve = Spec.InitialReserve; reloadUntil = -1;
            // Preserve nextShot: buying ammunition cannot bypass the weapon's fire interval.
            return true;
        }
    }
}
