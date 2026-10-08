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
        public bool CanPlace()
        {
            if (Occupied) return false;
            Physics.SyncTransforms();
            int count = Physics.OverlapBoxNonAlloc(transform.position + Vector3.up * .35f, new Vector3(.31f, .3f, .31f),
                overlaps, transform.rotation, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            return count == 0; // Fail closed on any occupant, including a full query buffer.
        }
        public bool TryDeploy(TurretDefinition definition, HealthComponent owner, TargetRegistry registry) =>
            CanPlace() && actor.TryDeploy(definition, owner, registry);
    }
}
