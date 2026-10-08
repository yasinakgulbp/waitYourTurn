using UnityEngine;

namespace WaitYourTurn.Combat
{
    /// <summary>Closest solid collider wins, including friendly bodies. Ignore own colliders.
    /// One collider per pellet prevents compound targets multiplying damage.</summary>
    public sealed class HitscanResolver
    {
        private readonly RaycastHit[] hits = new RaycastHit[64];
        public bool Cast(Vector3 start, Vector3 direction, float range, LayerMask mask,
            HealthComponent owner, out RaycastHit closest)
        {
            closest = default;
            int count = Physics.RaycastNonAlloc(start, direction, hits, range, mask, QueryTriggerInteraction.Ignore);
            // Overflow is unordered. Fail closed rather than shoot through an omitted wall.
            if (count == hits.Length) { closest.distance = 0; closest.point = start; return true; }
            float distance = float.PositiveInfinity; bool found = false;
            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = hits[i];
                if (owner != null && hit.collider.GetComponentInParent<HealthComponent>() == owner) continue;
                if (hit.distance >= distance) continue;
                closest = hit; distance = hit.distance; found = true;
            }
            return found;
        }
        public static bool Hostile(HealthComponent owner, HealthComponent target) =>
            owner != null && target != null && target.IsAlive && target.Team != Team.Neutral && target.Team != owner.Team;
        public static Vector3 PelletDirection(Vector3 forward, int pellet, int count, float degrees, ulong shot)
        {
            forward.Normalize();
            if (pellet == 0 || count <= 1 || degrees <= 0) return forward;
            Vector3 right = Vector3.Cross(Mathf.Abs(forward.y) > .99f ? Vector3.forward : Vector3.up, forward).normalized;
            Vector3 up = Vector3.Cross(forward, right);
            float radius = Mathf.Tan(degrees * Mathf.Deg2Rad) * Mathf.Sqrt((float)pellet / (count - 1));
            float angle = pellet * 2.399963f + (shot % 997) * .37f;
            return (forward + radius * (right * Mathf.Cos(angle) + up * Mathf.Sin(angle))).normalized;
        }
    }
}
