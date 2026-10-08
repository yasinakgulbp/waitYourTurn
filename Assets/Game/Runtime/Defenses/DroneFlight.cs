using UnityEngine;
using WaitYourTurn.Combat;

namespace WaitYourTurn.Defenses
{
    /// <summary>Reusable swept flight volume. Glass boundaries block bodies, combatants don't block a flying drone.</summary>
    public sealed class DroneFlight
    {
        private readonly RaycastHit[] hits = new RaycastHit[64];
        private readonly Collider[] overlaps = new Collider[64];
        public int Overflows { get; private set; }
        public bool Sweep(Vector3 from, Vector3 to, float radius, LayerMask mask, out Vector3 reachable)
        {
            reachable = from;
            Vector3 delta = to - from; float length = delta.magnitude;
            if (float.IsNaN(length) || float.IsInfinity(length) || radius <= 0 || float.IsNaN(radius) || float.IsInfinity(radius)) return false;
            int occupied = Physics.OverlapSphereNonAlloc(from, radius, overlaps, mask, QueryTriggerInteraction.Ignore);
            if (occupied == overlaps.Length) { Overflows++; return false; }
            for (int i = 0; i < occupied; i++)
            {
                var health = overlaps[i].GetComponentInParent<HealthComponent>();
                if (health == null || health.Team == Team.Neutral) return false;
            }
            if (length < .0001f) { reachable = to; return true; }
            int count = Physics.SphereCastNonAlloc(from, radius, delta / length, hits, length, mask, QueryTriggerInteraction.Ignore);
            if (count == hits.Length) { Overflows++; return false; }
            float distance = length; bool clear = true;
            for (int i = 0; i < count; i++)
            {
                var health = hits[i].collider.GetComponentInParent<HealthComponent>();
                if (health != null && health.Team != Team.Neutral) continue;
                if (hits[i].distance > distance) continue;
                clear = false; distance = Mathf.Max(0, hits[i].distance - .002f);
            }
            reachable = from + delta / length * distance;
            return clear;
        }
    }
}
