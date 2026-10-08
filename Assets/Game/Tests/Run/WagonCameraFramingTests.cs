using NUnit.Framework;
using UnityEngine;
using WaitYourTurn.Run;

namespace WaitYourTurn.Tests
{
    public sealed class WagonCameraFramingTests
    {
        [TestCase(16f / 9)] [TestCase(20f / 9)] [TestCase(4f / 3)] [TestCase(3f / 4)]
        public void BothApproachesAndWholeWagonFitProtectedViewport(float aspect)
        {
            var rotation = Quaternion.Euler(55, 0, 0);
            var volume = new Bounds(new Vector3(0, .4f, 0), new Vector3(16.3f, 2.4f, 8.2f));
            var viewport = WagonCameraFraming.ProtectedViewport;
            float distance = WagonCameraFraming.Distance(volume, rotation, 35, aspect, viewport);
            var camera = new GameObject("framing test").AddComponent<Camera>();
            try
            {
                camera.aspect = aspect; camera.fieldOfView = 35;
                camera.transform.SetPositionAndRotation(-(rotation * Vector3.forward * distance), rotation);
                for (int i = 0; i < 8; i++)
                {
                    Vector3 point = volume.center + Vector3.Scale(volume.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                    Vector3 v = camera.WorldToViewportPoint(point);
                    Assert.That(v.z, Is.GreaterThan(.1f));
                    Assert.That(v.x, Is.InRange(viewport.xMin - .0001f, viewport.xMax + .0001f));
                    Assert.That(v.y, Is.InRange(viewport.yMin - .0001f, viewport.yMax + .0001f));
                }
            }
            finally { Object.DestroyImmediate(camera.gameObject); }
        }
    }
}
