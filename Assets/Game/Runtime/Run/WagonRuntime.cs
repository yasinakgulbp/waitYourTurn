using UnityEngine;
using UnityEngine.AI;
using WaitYourTurn.Combat;
using WaitYourTurn.Enemies;
using WaitYourTurn.Player;
using WaitYourTurn.Train;

namespace WaitYourTurn.Run
{
    /// <summary>Scene composition for one persistent wagon. It does not advance station time.</summary>
    public sealed class WagonRuntime : MonoBehaviour
    {
        [SerializeField] private string wagonId;
        [SerializeField] private DoorController[] doors;
        [SerializeField] private EnemyPool enemies;
        [SerializeField] private MovementArea movementArea;
        [SerializeField] private WagonGeometry geometry;
        public WagonGeometry Geometry => geometry;
        public void ConfigureGeometry(WagonGeometry space) { geometry = space; enemies.ConfigureGeometry(space); }
        private readonly Collider[] nearby = new Collider[32];
        public string Id => wagonId;
        public DoorController[] Doors => doors;
        public EnemyPool Enemies => enemies;
        public MovementArea Area => movementArea;
        // Occupancy is gameplay state, not camera visibility. A future bot registers its own health here.
        public HealthComponent Defender { get; private set; }
        public bool HasLivingDefender => Defender != null && Defender.IsAlive;
        public void SetDefender(HealthComponent defender) => Defender = defender;
        public void Configure(string id, DoorController[] entries, EnemyPool pool, MovementArea area)
        { wagonId = id; doors = entries; enemies = pool; movementArea = area; }
        public void SetStationAccess(bool allowed) { foreach (DoorController door in doors) door.Portal.SetStationAccess(allowed); }
        public DoorController NearestDoor(Vector3 position)
        {
            DoorController nearest = doors[0]; float best = float.MaxValue;
            foreach (DoorController door in doors)
            {
                float distance = (position - door.RepairPosition).sqrMagnitude;
                if (distance < best) { best = distance; nearest = door; }
            }
            return nearest;
        }
        public bool TrySafePoint(HealthComponent movingBody, out Vector3 point, bool avoidEnemies = false)
        {
            Physics.SyncTransforms();
            point = default; bool found = false; float bestClearance = -1;
            for (int i = 0; i < (geometry != null ? geometry.SafePositionCount : 17); i++)
            {
                Vector3 local = i == 0 ? new Vector3(0, 0.05f, 5.5f) :
                    new Vector3(-2.4f + ((i - 1) % 4) * 1.6f, 0.05f, 2.5f + ((i - 1) / 4) * 1.5f);
                Vector3 candidate = geometry != null ? geometry.SafePosition(i) : transform.TransformPoint(local);
                var filter = new NavMeshQueryFilter { agentTypeID = enemies.AgentTypeId, areaMask = enemies.AreaMask };
                if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit, 0.15f, filter)) continue;
                candidate = hit.position + Vector3.up * 0.05f;
                Vector3 center = candidate + Vector3.up * 0.85f;
                if ((movementArea.Constrain(center, 0.4f) - center).sqrMagnitude > 0.001f) continue;
                int count = Physics.OverlapCapsuleNonAlloc(candidate + Vector3.up * 0.35f,
                    candidate + Vector3.up * 1.35f, 0.34f, nearby, Physics.AllLayers, QueryTriggerInteraction.Ignore);
                if (count == nearby.Length) continue;
                bool blocked = false;
                for (int j = 0; j < count; j++)
                {
                    if (nearby[j].GetComponentInParent<HealthComponent>() == movingBody) continue;
                    blocked = true; break;
                }
                if (blocked) continue;
                if (!avoidEnemies) { point = candidate; return true; }
                float clearance = float.MaxValue;
                foreach (var enemy in enemies.Active)
                {
                    if (!enemy.Health.IsAlive) continue;
                    Vector3 delta = enemy.transform.position - candidate; delta.y = 0;
                    clearance = Mathf.Min(clearance, delta.sqrMagnitude);
                }
                if (!found || clearance > bestClearance)
                { found = true; bestClearance = clearance; point = candidate; }
            }
            return found;
        }
        public bool ResolveDeparture()
        {
            SetStationAccess(false);
            foreach (EnemyBrain enemy in enemies.Active)
            {
                if (!enemy.Health.IsAlive || (!enemy.OnBoard && !enemy.IsInside)) continue;
                Vector3 safe = enemy.transform.position;
                if (enemy.NeedsSafeDeparture && !TrySafePoint(enemy.Health, out safe)) return false;
                if (!enemy.RetainForTravel(safe)) return false;
            }
            enemies.RemoveStationOutsiders();
            return true;
        }
        public bool SpawnOutside(int slot, EnemyProfile profile = null, int station = 0) => enemies.TrySpawn(
            geometry != null ? geometry.StationSpawn(slot) : transform.TransformPoint(
            new Vector3((slot % 3 - 1) * 1.2f, 0, -4 - (slot / 3) % 3)), profile, station);
    }
}
