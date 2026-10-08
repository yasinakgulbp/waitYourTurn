using System;
using UnityEngine;

namespace WaitYourTurn.Combat
{
    [DisallowMultipleComponent]
    public sealed class HealthComponent : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(0.01f)] private float startingMaxHealth = 100f;
        [SerializeField] private Team team;
        private Health health;
        private bool notifying;
        private float damageProtectedUntil;
        private Health Model => health ?? (health = new Health(startingMaxHealth, team));

        public float Current => Model.Current;
        public float Maximum => Model.Maximum;
        public bool IsAlive => Model.IsAlive;
        public uint LifeVersion => Model.LifeVersion;
        /// <summary>Changes only for applied damage, including a lethal hit. Healing never hides a hit.</summary>
        public ulong DamageRevision { get; private set; }
        public Team Team => Model.Team;
        public EntityIdentity Identity => new EntityIdentity(EntityId.ToULong(GetEntityId()), LifeVersion);
        public bool Invulnerable { get => Model.Invulnerable; set { if (!notifying) Model.Invulnerable = value; } }
        // Separate from pause/debug invulnerability; scaled time preserves protection while gameplay is paused.
        public float DamageProtectionRemaining => Mathf.Max(0, damageProtectedUntil - Time.time);
        public bool SetDamageProtection(float seconds)
        {
            if (notifying || seconds < 0 || float.IsNaN(seconds) || float.IsInfinity(seconds) ||
                float.IsInfinity(Time.time + seconds) || seconds > 0 && !IsAlive) return false;
            damageProtectedUntil = Time.time + seconds; return true;
        }
        public event Action<HealthSnapshot> HealthChanged;
        public event Action<DeathNotice> Died;

        private void Awake() { _ = Model; }

        public DamageResult TryApplyDamage(DamageContext context)
        {
            if (notifying) return DamageResult.Reject(DamageRejection.NotificationInProgress);
            if (IsAlive && DamageProtectionRemaining > 0) return DamageResult.Reject(DamageRejection.Invulnerable);
            DamageResult result = Model.TryApplyDamage(context);
            if (!result.Applied) return result;
            unchecked { DamageRevision++; }
            EntityIdentity target = Identity;
            notifying = true;
            try
            {
                try { HealthChanged?.Invoke(Model.Snapshot); }
                finally { if (result.Killed) Died?.Invoke(new DeathNotice(target, context, result)); }
            }
            finally { notifying = false; }
            return result;
        }

        public float TryHeal(float amount)
        {
            if (notifying) return 0f;
            float applied = Model.TryHeal(amount);
            if (applied > 0f) NotifyChanged();
            return applied;
        }

        public float TryShopHeal() => TryHeal(Mathf.Max(30f, Current * 0.5f));

        public bool ChangeMaxHealth(float maximum, MaxHealthPolicy policy = MaxHealthPolicy.PreserveCurrent)
        {
            if (notifying || !Model.ChangeMaxHealth(maximum, policy)) return false;
            NotifyChanged();
            return true;
        }

        public bool ResetForSpawn(float maximum, Team newTeam)
        {
            if (notifying || !Model.ResetForSpawn(maximum, newTeam)) return false;
            damageProtectedUntil = 0;
            NotifyChanged();
            return true;
        }

        private void NotifyChanged()
        {
            notifying = true;
            try { HealthChanged?.Invoke(Model.Snapshot); }
            finally { notifying = false; }
        }
    }
}
