using UnityEngine;
using UnityEngine.AI;

namespace WaitYourTurn.Navigation
{
    /// <summary>M1 movement adapter. Enemy decisions and combat will be added separately.</summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [DisallowMultipleComponent]
    public sealed class PortalNavigator : MonoBehaviour
    {
        [SerializeField] private EntryPortal portal;
        [SerializeField] private Vector3 waitingOffset;
        [SerializeField] private Vector3 destinationOffset;
        private NavMeshAgent agent;
        private bool dirty = true;
        private bool wantsInside = true;

        public NavMeshAgent Agent => agent;
        public bool WantsInside => wantsInside;
        public string State { get; private set; } = "Initializing";
        public Vector3 InsideGoal => portal.InsideDestination + destinationOffset;

        public void Configure(EntryPortal entry, Vector3 queueOffset, Vector3 arrivalOffset)
        {
            if (Application.isPlaying && isActiveAndEnabled && portal != null)
                portal.Changed -= InvalidateRoute;
            portal = entry;
            if (Application.isPlaying && isActiveAndEnabled && portal != null)
                portal.Changed += InvalidateRoute;
            waitingOffset = queueOffset;
            destinationOffset = arrivalOffset;
            dirty = true;
        }

        private void Awake() => agent = GetComponent<NavMeshAgent>();
        private void OnEnable()
        {
            if (portal != null) portal.Changed += InvalidateRoute;
            dirty = true;
        }
        private void OnDisable()
        {
            if (portal != null) portal.Changed -= InvalidateRoute;
        }
        private void InvalidateRoute() => dirty = true;

        public void SetGoal(bool inside)
        {
            wantsInside = inside;
            dirty = true;
        }

        public bool TryPlace(Vector3 point)
        {
            if (agent == null) agent = GetComponent<NavMeshAgent>();
            var filter = new NavMeshQueryFilter { agentTypeID = agent.agentTypeID, areaMask = agent.areaMask };
            if (!NavMesh.SamplePosition(point, out NavMeshHit hit, 0.75f, filter)) return false;
            if (!agent.enabled)
            {
                transform.position = hit.position;
                agent.enabled = true;
            }
            if (!agent.Warp(hit.position)) return false;
            dirty = true;
            return true;
        }

        private void Update()
        {
            if (portal == null || !agent.enabled || !agent.isOnNavMesh)
            {
                State = "No navigation surface";
                return;
            }
            // ResetPath during an off-mesh traversal would forcibly complete the crossing.
            if (agent.isOnOffMeshLink)
            {
                State = "Crossing";
                return;
            }
            if (dirty)
            {
                dirty = false;
                bool isInside = portal.IsInside(transform.position);
                Vector3 destination;
                if (wantsInside)
                    destination = isInside || portal.AcceptsEntry
                        ? InsideGoal : portal.OutsideApproach + waitingOffset;
                else
                    destination = !isInside || portal.AcceptsEntry
                        ? portal.OutsideApproach + waitingOffset : InsideGoal;

                agent.ResetPath();
                agent.isStopped = false;
                if (!agent.SetDestination(destination)) State = "Invalid destination";
            }
            if (agent.pathPending) { State = "Planning"; return; }
            if (agent.pathStatus != NavMeshPathStatus.PathComplete)
            {
                State = "Waiting for route";
                return;
            }
            bool reached = agent.remainingDistance <= agent.stoppingDistance + 0.08f;
            State = reached ? (portal.IsInside(transform.position) ? "Inside wagon" : "Waiting outside")
                : "Walking";
        }
    }
}
