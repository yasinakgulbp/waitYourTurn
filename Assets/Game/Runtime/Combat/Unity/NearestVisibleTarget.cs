using UnityEngine;

namespace WaitYourTurn.Combat
{
    /// <summary>Shared bounded-registry selection. Occluded nearer bodies do not hide a visible candidate.</summary>
    public static class NearestVisibleTarget
    {
        public static HealthComponent Select(TargetRegistry registry, HitscanWeapon weapon, Vector3 origin)
        {
            HealthComponent target = null;
            float closest = weapon.Range * weapon.Range;
            foreach (HealthComponent candidate in registry.Targets)
            {
                if (candidate == null || !candidate.IsAlive) continue;
                float distance = (candidate.transform.position - origin).sqrMagnitude;
                if (distance >= closest || !weapon.HasSight(candidate)) continue;
                closest = distance; target = candidate;
            }
            return target;
        }
    }
}
