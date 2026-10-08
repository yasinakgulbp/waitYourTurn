using UnityEngine;
using WaitYourTurn.Combat;

namespace WaitYourTurn.Defenses
{
    public sealed class TurretMount : MonoBehaviour
    {
        [SerializeField] private string slotId;
        [SerializeField] private TurretController actor;
        private readonly Collider[] overlaps = new Collider[16];
        public string Id => slotId;
        public TurretController Actor => actor;
        public bool Occupied => actor.Deployed || actor.Retiring;
        public void Configure(string id, TurretController turret) { slotId = id; actor = turret; }
        public bool CanPlace(Vector3 position, HealthComponent buyer)
        {
            if (Occupied) return false;
            Physics.SyncTransforms();
            int count = Physics.OverlapBoxNonAlloc(position + Vector3.up * .35f, new Vector3(.13f, .3f, .13f),
                overlaps, transform.rotation, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            if (count == overlaps.Length) return false;
            for (int i = 0; i < count; i++)
                if (overlaps[i].GetComponentInParent<HealthComponent>() != buyer) return false;
            return true;
        }
        public bool TryDeploy(TurretDefinition definition, HealthComponent owner, TargetRegistry registry, Vector3 position)
        {
            if (!CanPlace(position, owner)) return false;
            transform.position = position;
            return actor.TryDeploy(definition, owner, registry);
        }
    }
}
