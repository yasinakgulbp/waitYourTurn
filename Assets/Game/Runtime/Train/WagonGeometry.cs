using UnityEngine;
using UnityEngine.AI;
using WaitYourTurn.Navigation;

namespace WaitYourTurn.Train
{
    /// <summary>Authored wagon space shared by navigation and run composition. No player or enemy dependency.</summary>
    public sealed class WagonGeometry : MonoBehaviour
    {
        [SerializeField] private Bounds interior;
        [SerializeField] private DoorController[] doors;
        [SerializeField] private Transform[] stationSpawns;
        [SerializeField] private Transform[] safePositions;
        public Bounds Interior => interior;
        public int SafePositionCount => safePositions.Length;
        public int SpawnCount => stationSpawns.Length;
        public void Configure(Bounds space, DoorController[] entries, Transform[] spawns, Transform[] safe)
        { interior = space; doors = entries; stationSpawns = spawns; safePositions = safe; }
        public bool Contains(Vector3 position) => interior.Contains(transform.InverseTransformPoint(position));
        public Vector3 SafePosition(int index) => safePositions[index].position;
        public Vector3 StationSpawn(int index) => stationSpawns[index % stationSpawns.Length].position;
        public Vector3 Constrain(Vector3 position, float radius)
        {
            Vector3 local = transform.InverseTransformPoint(position), scale = transform.lossyScale;
            Vector3 min = interior.min, max = interior.max;
            float x = Mathf.Min(radius / Mathf.Abs(scale.x), interior.extents.x);
            float z = Mathf.Min(radius / Mathf.Abs(scale.z), interior.extents.z);
            local.x = Mathf.Clamp(local.x, min.x + x, max.x - x);
            local.z = Mathf.Clamp(local.z, min.z + z, max.z - z);
            return transform.TransformPoint(local);
        }
        public bool InPassage(Vector3 position, float radius) => PassageAt(position, radius) != null;
        public EntryPortal PassageAt(Vector3 position, float radius)
        {
            foreach (DoorController door in doors) if (door.Portal.IsInPassage(position, radius)) return door.Portal;
            return null;
        }
        public DoorController SelectEntry(Vector3 from, int agentType, int mask, NavMeshPath path)
        {
            DoorController best = null; float bestCost = float.MaxValue;
            var filter = new NavMeshQueryFilter { agentTypeID = agentType, areaMask = mask };
            foreach (DoorController door in doors)
            {
                Vector3 goal = door.Portal.OutsideApproach;
                // Entry candidates must face the source platform; never route around the train to its far side.
                if (Vector3.Dot(from - door.Portal.transform.position, door.Portal.transform.forward) > 0) continue;
                if (!NavMesh.CalculatePath(from, goal, filter, path) || path.status != NavMeshPathStatus.PathComplete) continue;
                float cost = (goal - from).sqrMagnitude;
                if (cost < bestCost) { best = door; bestCost = cost; }
            }
            return best;
        }
    }
}
