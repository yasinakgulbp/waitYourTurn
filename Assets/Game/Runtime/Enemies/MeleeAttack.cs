using UnityEngine;
using WaitYourTurn.Combat;

namespace WaitYourTurn.Enemies
{
    /// <summary>Wind-up captures a target life; impact rechecks life, range and line of sight.</summary>
    public sealed class MeleeAttack : MonoBehaviour
    {
        [SerializeField] private HealthComponent source;
        [SerializeField, Min(0.01f)] private float damage = 2f;
        [SerializeField, Min(0.1f)] private float interval = 1f;
        [SerializeField, Min(0f)] private float windup = 0.25f;
        [SerializeField, Min(0.1f)] private float range = 1.6f;
        private HealthComponent pending;
        private uint pendingLife;
        private float impactAt;
        private float nextAttackAt;
        private ulong attackId;
        public float Range => range;
        public bool WindingUp => pending != null;
        public void Configure(HealthComponent health) => source = health;
        public void ResetAttack() { pending = null; impactAt = nextAttackAt = 0f; attackId = 0; }
        public void Cancel() => pending = null;
        public bool CanReach(HealthComponent target)
        {
            if (target == null || !target.IsAlive || !source.IsAlive) return false;
            Vector3 delta = target.transform.position - transform.position;
            delta.y = 0;
            if (delta.sqrMagnitude > range * range) return false;
            return !Physics.Linecast(transform.position + Vector3.up * 0.85f,
                target.transform.position + Vector3.up * 0.85f, out RaycastHit hit, Physics.AllLayers,
                QueryTriggerInteraction.Ignore) || hit.collider.GetComponentInParent<HealthComponent>() == target;
        }
        public void Tick(HealthComponent desired)
        {
            if (pending != null && (pending != desired || pending.LifeVersion != pendingLife || !CanReach(pending)))
                pending = null;
            if (pending != null)
            {
                if (Time.time < impactAt) return;
                HealthComponent target = pending;
                pending = null;
                target.TryApplyDamage(new DamageContext(damage, source.Identity, source.Team, ++attackId, pendingLife));
                return;
            }
            if (Time.time < nextAttackAt || !CanReach(desired)) return;
            pending = desired;
            pendingLife = desired.LifeVersion;
            impactAt = Time.time + windup;
            nextAttackAt = Time.time + Mathf.Max(interval, windup);
        }
    }
}
