using UnityEngine;
using UnityEngine.AI;

namespace WaitYourTurn.Navigation
{
    /// <summary>Single owner of agent motion. Does not choose combat targets or door policies.</summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class AgentMotor : MonoBehaviour
    {
        private NavMeshAgent agent;
        private Vector3 lastGoal;
        private bool hasGoal;
        public NavMeshAgent Agent => agent != null ? agent : (agent = GetComponent<NavMeshAgent>());
        public bool Ready => Agent.enabled && Agent.isOnNavMesh;
        public bool HasRoute => Ready && !Agent.pathPending && Agent.pathStatus == NavMeshPathStatus.PathComplete;
        public bool TryPlace(Vector3 point)
        {
            var filter = new NavMeshQueryFilter { agentTypeID = Agent.agentTypeID, areaMask = Agent.areaMask };
            if (!NavMesh.SamplePosition(point, out NavMeshHit hit, 0.5f, filter)) return false;
            Agent.enabled = false;
            transform.position = hit.position;
            Agent.enabled = true;
            hasGoal = false;
            return Agent.isOnNavMesh && Agent.Warp(hit.position);
        }
        public bool GoTo(Vector3 point, bool force = false)
        {
            if (!Ready) return false;
            if (!force && hasGoal && (lastGoal - point).sqrMagnitude < 0.16f && !Agent.isStopped) return true;
            Agent.isStopped = false;
            lastGoal = point;
            hasGoal = Agent.SetDestination(point);
            return hasGoal;
        }
        public void Stop()
        {
            if (Ready) Agent.isStopped = true;
        }
        public void Clear()
        {
            if (Ready) Agent.ResetPath();
            Agent.enabled = false;
            hasGoal = false;
        }
    }
}
