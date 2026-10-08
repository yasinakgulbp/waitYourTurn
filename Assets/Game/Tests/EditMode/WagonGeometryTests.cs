using NUnit.Framework;
using UnityEngine;
using WaitYourTurn.Train;

namespace WaitYourTurn.Tests
{
    public sealed class WagonGeometryTests
    {
        [Test]
        public void OppositePlatformAndAdjacentWagonAreOutsideEvenAfterRotation()
        {
            var root=new GameObject("rotated wagon");
            try
            {
                root.transform.SetPositionAndRotation(new Vector3(17,2,-8),Quaternion.Euler(0,90,0));
                var geometry=root.AddComponent<WagonGeometry>();
                geometry.Configure(new Bounds(new Vector3(0,1,0),new Vector3(6,3,2.54f)),new DoorController[0],new Transform[0],new Transform[0]);
                Assert.IsTrue(geometry.Contains(root.transform.TransformPoint(new Vector3(0,0,0))));
                Assert.IsFalse(geometry.Contains(root.transform.TransformPoint(new Vector3(0,0,4))));
                Assert.IsFalse(geometry.Contains(root.transform.TransformPoint(new Vector3(0,0,-4))));
                Assert.IsFalse(geometry.Contains(root.transform.TransformPoint(new Vector3(7.41f,0,0))));
                Assert.IsFalse(geometry.Contains(root.transform.TransformPoint(new Vector3(0,5,0))));
            }
            finally { Object.DestroyImmediate(root); }
        }
        [Test]
        public void PursuitGoalKeepsBodyWithinBothSideWallsWithScaledRotatedWagon()
        {
            var root=new GameObject("scaled wagon");
            try
            {
                root.transform.SetPositionAndRotation(new Vector3(17,2,-8),Quaternion.Euler(0,90,0));root.transform.localScale=new Vector3(2,1,2);
                var geometry=root.AddComponent<WagonGeometry>();
                geometry.Configure(new Bounds(new Vector3(0,1,0),new Vector3(6,3,2.54f)),new DoorController[0],new Transform[0],new Transform[0]);
                Vector3 goal=geometry.Constrain(root.transform.TransformPoint(new Vector3(8,0,-8)),.4f);
                Vector3 local=root.transform.InverseTransformPoint(goal);
                Assert.That(local.x,Is.EqualTo(2.8f).Within(.001));Assert.That(local.z,Is.EqualTo(-1.07f).Within(.001));
            }
            finally { Object.DestroyImmediate(root); }
        }
    }
}
