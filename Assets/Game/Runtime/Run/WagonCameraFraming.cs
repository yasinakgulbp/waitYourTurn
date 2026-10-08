using UnityEngine;

namespace WaitYourTurn.Run
{
    /// <summary>Perspective fit of a protected world volume, independent of device pixels.</summary>
    public static class WagonCameraFraming
    {
        public static Rect ProtectedViewport => new Rect(.04f, .1f, .92f, .82f);
        public static float Distance(Bounds volume, Quaternion rotation, float fieldOfView, float aspect, Rect viewport)
        {
            float tanY = Mathf.Tan(fieldOfView * Mathf.Deg2Rad * .5f);
            float tanX = tanY * Mathf.Max(.1f, aspect);
            Quaternion inverse = Quaternion.Inverse(rotation);
            float distance = 1;
            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = volume.center + Vector3.Scale(volume.extents,
                    new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                Vector3 p = inverse * corner;
                float xLimit = p.x < 0 ? 1 - 2 * viewport.xMin : 2 * viewport.xMax - 1;
                float yLimit = p.y < 0 ? 1 - 2 * viewport.yMin : 2 * viewport.yMax - 1;
                distance = Mathf.Max(distance, Mathf.Abs(p.x) / (tanX * xLimit) - p.z);
                distance = Mathf.Max(distance, Mathf.Abs(p.y) / (tanY * yLimit) - p.z);
                distance = Mathf.Max(distance, .3f - p.z);
            }
            return distance;
        }
    }
}
