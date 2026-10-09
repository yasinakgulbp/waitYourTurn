using UnityEngine;

namespace WaitYourTurn.Combat
{
    /// <summary>Constant-velocity intercept for straight projectiles; no target or navigation dependency.</summary>
    public static class ProjectileAim
    {
        public static bool TryLead(Vector3 offset, Vector3 velocity, float speed, float range, out Vector3 direction)
        {
            direction = offset;
            if (!(speed > 0 && speed <= 100 && range > 0) ||
                !(offset.sqrMagnitude > .0001f && offset.sqrMagnitude <= range * range) ||
                !(velocity.sqrMagnitude <= 400)) return false;
            float a = velocity.sqrMagnitude - speed * speed;
            float b = 2 * Vector3.Dot(offset, velocity), c = offset.sqrMagnitude, time;
            if (Mathf.Abs(a) < .0001f)
            {
                if (Mathf.Abs(b) < .0001f) return false;
                time = -c / b;
            }
            else
            {
                float discriminant = b * b - 4 * a * c;
                if (!(discriminant >= 0)) return false;
                float root = Mathf.Sqrt(discriminant);
                float first = (-b - root) / (2 * a), second = (-b + root) / (2 * a);
                time = first > 0 && second > 0 ? Mathf.Min(first, second) : Mathf.Max(first, second);
            }
            if (!(time > 0 && time <= range / speed)) return false;
            Vector3 lead = offset + velocity * time;
            if (!(lead.sqrMagnitude <= range * range)) return false;
            direction = lead; return true;
        }
    }
}
