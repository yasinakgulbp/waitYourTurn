using UnityEngine;
using WaitYourTurn.Combat;

namespace WaitYourTurn.Defenses
{
    /// <summary>Wagon-local capacity and authored mounts. Unaware of prices or run phases.</summary>
    public sealed class TurretRack : MonoBehaviour
    {
        [SerializeField] private TurretMount[] mounts;
        [SerializeField, Min(1)] private int perTypeLimit = 2;
        [SerializeField, Min(.1f)] private float purchaseRange = 1.35f;
        public TurretMount[] Mounts => mounts;
        public int PerTypeLimit => perTypeLimit;
        public float PurchaseRange => purchaseRange;
        public void Configure(TurretMount[] slots) => mounts = slots;
        public int Count(TurretDefinition definition)
        {
            int count = 0;
            foreach (var mount in mounts) if (mount.Occupied && mount.Actor.Definition == definition) count++;
            return count;
        }
        public TurretMount Nearest(Vector3 position)
        {
            TurretMount nearest = null; float best = purchaseRange * purchaseRange;
            foreach (var mount in mounts)
            {
                Vector3 delta = mount.transform.position - position; delta.y = 0;
                if (delta.sqrMagnitude > best) continue;
                nearest = mount; best = delta.sqrMagnitude;
            }
            return nearest;
        }
        public bool CanDeploy(TurretDefinition definition, Vector3 buyerPosition)
        {
            if (definition == null || !definition.Valid || Count(definition) >= perTypeLimit) return false;
            var mount = Nearest(buyerPosition);
            return mount != null && mount.CanPlace();
        }
        public bool TryDeploy(TurretDefinition definition, HealthComponent owner, TargetRegistry registry)
        {
            if (owner == null || !owner.IsAlive || !CanDeploy(definition, owner.transform.position)) return false;
            return Nearest(owner.transform.position).TryDeploy(definition, owner, registry);
        }
        public void SetPaused(bool paused) { foreach (var mount in mounts) mount.Actor.SetPaused(paused); }
        public void Clear() { foreach (var mount in mounts) mount.Actor.Clear(); }
    }
}
