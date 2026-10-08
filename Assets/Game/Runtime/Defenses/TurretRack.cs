using UnityEngine;
using WaitYourTurn.Combat;

namespace WaitYourTurn.Defenses
{
    /// <summary>Wagon-local capacity and reusable actors; placement follows the buyer.</summary>
    public sealed class TurretRack : MonoBehaviour
    {
        [SerializeField] private TurretMount[] mounts;
        [SerializeField, Min(1)] private int perTypeLimit = 2;
        [SerializeField] private Bounds placementBounds;
        public TurretMount[] Mounts => mounts;
        public int PerTypeLimit => perTypeLimit;
        public void Configure(TurretMount[] slots, Bounds bounds) { mounts = slots; placementBounds = bounds; }
        public int Count(TurretDefinition definition)
        {
            int count = 0;
            foreach (var mount in mounts) if (mount.Occupied && mount.Actor.Definition == definition) count++;
            return count;
        }
        private TurretMount Available()
        {
            foreach (var mount in mounts) if (!mount.Occupied) return mount;
            return null;
        }
        private Vector3 Placement(HealthComponent buyer) => new Vector3(buyer.transform.position.x, transform.position.y, buyer.transform.position.z);
        public bool CanPlace(HealthComponent buyer)
        {
            if (buyer == null || !buyer.IsAlive) return false;
            Vector3 position = Placement(buyer), local = transform.InverseTransformPoint(position);
            if (float.IsNaN(local.x) || float.IsNaN(local.z) || float.IsInfinity(local.x) || float.IsInfinity(local.z) ||
                local.x < placementBounds.min.x + .13f || local.x > placementBounds.max.x - .13f ||
                local.z < placementBounds.min.z + .13f || local.z > placementBounds.max.z - .13f) return false;
            var mount = Available();
            return mount != null && mount.CanPlace(position, buyer);
        }
        public bool CanDeploy(TurretDefinition definition, HealthComponent buyer) =>
            definition != null && definition.Valid && Count(definition) < perTypeLimit && CanPlace(buyer);
        public bool TryDeploy(TurretDefinition definition, HealthComponent owner, TargetRegistry registry)
        {
            if (!CanDeploy(definition, owner)) return false;
            return Available().TryDeploy(definition, owner, registry, Placement(owner));
        }
        public void SetPaused(bool paused) { foreach (var mount in mounts) mount.Actor.SetPaused(paused); }
        public void Clear() { foreach (var mount in mounts) mount.Actor.Clear(); }
    }
}
