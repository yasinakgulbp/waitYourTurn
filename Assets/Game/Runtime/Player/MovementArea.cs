using UnityEngine;

namespace WaitYourTurn.Player
{
    /// <summary>Optional upright local-space movement region. Has no physics collider or train dependency.</summary>
    public sealed class MovementArea : MonoBehaviour
    {
        [SerializeField] private Vector2 minimum = new Vector2(-3.9f, 1.1f);
        [SerializeField] private Vector2 maximum = new Vector2(3.9f, 8.9f);
        public Vector3 Constrain(Vector3 worldCenter, float worldRadius)
        {
            Vector3 local = transform.InverseTransformPoint(worldCenter);
            Vector3 scale = transform.lossyScale;
            float xInset = worldRadius / Mathf.Max(0.001f, Mathf.Abs(scale.x));
            float zInset = worldRadius / Mathf.Max(0.001f, Mathf.Abs(scale.z));
            xInset = Mathf.Min(xInset, (maximum.x - minimum.x) * 0.5f);
            zInset = Mathf.Min(zInset, (maximum.y - minimum.y) * 0.5f);
            local.x = Mathf.Clamp(local.x, minimum.x + xInset, maximum.x - xInset);
            local.z = Mathf.Clamp(local.z, minimum.y + zInset, maximum.y - zInset);
            return transform.TransformPoint(local);
        }
    }
}
