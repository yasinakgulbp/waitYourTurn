using UnityEngine;

namespace WaitYourTurn.Player
{
    /// <summary>Optional upright local-space movement region. Has no physics collider or train dependency.</summary>
    public sealed class MovementArea : MonoBehaviour
    {
        [SerializeField] private Vector2 minimum = new Vector2(-3.9f, 1.1f);
        [SerializeField] private Vector2 maximum = new Vector2(3.9f, 8.9f);
        private Bounds[] regions;
        // Composition supplies overlapping rectangles. Connector rectangles extend into each room,
        // so capsule-radius insets never introduce a gap at a doorway.
        public void ConfigureRegions(Bounds[] values) => regions = values;
        public void Configure(Vector2 min, Vector2 max) { minimum = min; maximum = max; }
        public Vector3 Constrain(Vector3 worldCenter, float worldRadius)
        {
            Vector3 local = transform.InverseTransformPoint(worldCenter);
            Vector3 scale = transform.lossyScale;
            float xInset = worldRadius / Mathf.Max(0.001f, Mathf.Abs(scale.x));
            float zInset = worldRadius / Mathf.Max(0.001f, Mathf.Abs(scale.z));
            if (regions != null && regions.Length > 0)
            {
                Vector3 closest = local; float best = float.PositiveInfinity;
                foreach (var region in regions)
                {
                    float ix = Mathf.Min(xInset, region.extents.x), iz = Mathf.Min(zInset, region.extents.z);
                    Vector3 candidate = local;
                    candidate.x = Mathf.Clamp(local.x, region.min.x + ix, region.max.x - ix);
                    candidate.z = Mathf.Clamp(local.z, region.min.z + iz, region.max.z - iz);
                    float distance = (candidate - local).sqrMagnitude;
                    if (distance < best) { best = distance; closest = candidate; }
                }
                return transform.TransformPoint(closest);
            }
            xInset = Mathf.Min(xInset, (maximum.x - minimum.x) * 0.5f);
            zInset = Mathf.Min(zInset, (maximum.y - minimum.y) * 0.5f);
            local.x = Mathf.Clamp(local.x, minimum.x + xInset, maximum.x - xInset);
            local.z = Mathf.Clamp(local.z, minimum.y + zInset, maximum.y - zInset);
            return transform.TransformPoint(local);
        }
    }
}
