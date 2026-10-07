using System;

namespace WaitYourTurn.Combat
{
    /// <summary>Pure health rules. No navigation, Unity objects, rewards, UI or per-frame work.</summary>
    public sealed class Health : IDamageable
    {
        public float Current { get; private set; }
        public float Maximum { get; private set; }
        public uint LifeVersion { get; private set; } = 1;
        public Team Team { get; private set; }
        public bool Invulnerable { get; set; }
        public bool IsAlive => Current > 0f;
        public HealthSnapshot Snapshot => new HealthSnapshot(Current, Maximum, LifeVersion, Team);

        public Health(float maximum, Team team = Team.Neutral)
        {
            if (!ValidPositive(maximum)) throw new ArgumentOutOfRangeException(nameof(maximum));
            Current = Maximum = maximum;
            Team = team;
        }

        public DamageResult TryApplyDamage(DamageContext context)
        {
            if (!IsAlive) return DamageResult.Reject(DamageRejection.Dead);
            if (context.TargetLifeVersion != LifeVersion) return DamageResult.Reject(DamageRejection.StaleLife);
            if (!ValidPositive(context.Amount)) return DamageResult.Reject(DamageRejection.InvalidAmount);
            if (Invulnerable) return DamageResult.Reject(DamageRejection.Invulnerable);
            if (!context.AllowFriendlyFire && Team != Team.Neutral && context.SourceTeam == Team)
                return DamageResult.Reject(DamageRejection.FriendlyFire);
            float applied = Math.Min(Current, context.Amount);
            Current -= applied;
            return new DamageResult(applied, !IsAlive);
        }

        public float TryHeal(float amount)
        {
            if (!IsAlive || !ValidPositive(amount)) return 0f;
            float applied = Math.Min(Maximum - Current, amount);
            Current += applied;
            return applied;
        }

        public float TryShopHeal() => TryHeal(Math.Max(30f, Current * 0.5f));

        public bool ChangeMaxHealth(float maximum, MaxHealthPolicy policy = MaxHealthPolicy.PreserveCurrent)
        {
            if (!ValidPositive(maximum) ||
                (policy != MaxHealthPolicy.PreserveCurrent && policy != MaxHealthPolicy.HealAddedCapacity)) return false;
            float added = Math.Max(0f, maximum - Maximum);
            Maximum = maximum;
            if (IsAlive) Current = Math.Min(maximum,
                policy == MaxHealthPolicy.HealAddedCapacity ? Current + added : Current);
            return true;
        }

        /// <summary>Explicit new life for spawning; ordinary healing never resurrects a dead target.</summary>
        public bool ResetForSpawn(float maximum, Team team)
        {
            if (!ValidPositive(maximum)) return false;
            unchecked { LifeVersion++; if (LifeVersion == 0) LifeVersion++; }
            Current = Maximum = maximum;
            Team = team;
            Invulnerable = false;
            return true;
        }

        private static bool ValidPositive(float value) => value > 0f && !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
