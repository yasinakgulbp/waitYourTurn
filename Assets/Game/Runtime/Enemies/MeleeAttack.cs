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
        private bool defaultsCaptured;
        private Vector4 defaults;
        private float actorReach = .92f;
        private float startedAt;
        private HealthComponent poseTarget;
        private float poseUntil;
        public uint SwingVersion { get; private set; }
        public HealthComponent PoseTarget => Time.time <= poseUntil ? poseTarget : null;
        public float SwingAge => Time.time - startedAt;
        public float Windup => windup;
        public float ActorReach => actorReach;
        public void SetActorReach(float value) => actorReach = Mathf.Clamp(value, .65f, 1.2f);
        public float Range => range;
        public float Damage => damage;
        public float Interval => interval;
        public bool WindingUp => pending != null;
        public float CooldownRemaining => Mathf.Max(0, nextAttackAt - Time.time);
        public void RestoreCooldown(float remaining) { Cancel(); nextAttackAt = Time.time + remaining; }
        public void Configure(HealthComponent health) => source = health;
        public void ApplyProfile(EnemyProfile profile)
        {
            if (!defaultsCaptured) { defaults = new Vector4(damage, interval, windup, range); defaultsCaptured = true; }
            damage = profile != null ? profile.damage : defaults.x;
            interval = profile != null ? profile.attackInterval : defaults.y;
            windup = profile != null ? profile.windup : defaults.z;
            range = profile != null ? profile.range : defaults.w;
            actorReach = profile != null && profile.id == "fast" ? .82f : profile != null && profile.id == "tough" ? 1.02f : .92f;
        }
        public void ResetAttack() { pending = poseTarget = null; impactAt = nextAttackAt = poseUntil = 0f; attackId = 0; SwingVersion = 0; }
        public void Cancel() { pending = poseTarget = null; }
        public bool CanReach(HealthComponent target)
        {
            if (target == null || !target.IsAlive || !source.IsAlive) return false;
            Vector3 delta = target.transform.position - transform.position;
            delta.y = 0;
            // Door reservations use authored door range. Living defenders must be within arm contact.
            float reach = target.Team == Team.Player ? Mathf.Min(range, actorReach) : range;
            if (delta.sqrMagnitude > reach * reach) return false;
            return !Physics.Linecast(transform.position + Vector3.up * 0.85f,
                target.transform.position + Vector3.up * 0.85f, out RaycastHit hit, Physics.AllLayers,
                QueryTriggerInteraction.Ignore) || hit.collider.GetComponentInParent<HealthComponent>() == target;
        }
        public void Tick(HealthComponent desired)
        {
            if (pending != null && (pending != desired || pending.LifeVersion != pendingLife || !CanReach(pending)))
                Cancel();
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
            poseTarget = desired;
            startedAt = Time.time;
            poseUntil = startedAt + windup + .38f;
            unchecked { SwingVersion++; }
            pendingLife = desired.LifeVersion;
            impactAt = Time.time + windup;
            nextAttackAt = Time.time + Mathf.Max(interval, windup);
        }
    }
}
