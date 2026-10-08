using NUnit.Framework;
using UnityEngine;
using WaitYourTurn.Train;

namespace WaitYourTurn.Tests
{
    public sealed class WagonLayoutTests
    {
        [TestCase(DoorSlots.All, 6)]
        [TestCase(DoorSlots.All & ~DoorSlots.NorthCenter, 5)]
        [TestCase(DoorSlots.All & ~(DoorSlots.SouthCenter | DoorSlots.NorthCenter), 4)]
        public void DoorVariantsKeepBothSidesAndDoNotChangeBodyDimensions(DoorSlots slots, int count)
        {
            var layout = ScriptableObject.CreateInstance<WagonLayoutDefinition>();
            try
            {
                layout.openings = slots;
                Assert.IsTrue(layout.Valid); Assert.AreEqual(count, layout.DoorCount);
                Assert.AreEqual(12, layout.length); Assert.AreEqual(4.2f, layout.width);
                Assert.IsTrue(layout.HasDoor(0)); Assert.IsTrue(layout.HasDoor(1));
                Assert.IsTrue(layout.HasDoor(4)); Assert.IsTrue(layout.HasDoor(5));
            }
            finally { Object.DestroyImmediate(layout); }
        }
        [Test]
        public void WindowMustStayWithinWallAndHavePhysicalMetalFrames()
        {
            var layout = ScriptableObject.CreateInstance<WagonLayoutDefinition>();
            try
            {
                Assert.IsTrue(layout.Valid);
                layout.windowTop = layout.wallHeight; Assert.IsFalse(layout.Valid);
                layout.windowTop = 1.6f; layout.windowBottom = 1.5f; Assert.IsFalse(layout.Valid);
                layout.windowBottom = .7f; layout.windowPost = 0; Assert.IsFalse(layout.Valid);
                layout.windowPost = .12f; layout.windowBorder = 0; Assert.IsFalse(layout.Valid);
                layout.windowBorder = .24f; layout.maxWindowWidth = float.NaN; Assert.IsFalse(layout.Valid);
            }
            finally { Object.DestroyImmediate(layout); }
        }
        [Test]
        public void InvalidDimensionsAndUnsupportedDoorMasksAreRejectedBeforeBake()
        {
            var layout = ScriptableObject.CreateInstance<WagonLayoutDefinition>();
            try
            {
                layout.doorWidth = 10; Assert.IsFalse(layout.Valid);
                layout.doorWidth = 1.45f; layout.openings = (DoorSlots)127; Assert.IsFalse(layout.Valid);
                layout.openings = DoorSlots.SouthLeft; Assert.IsFalse(layout.Valid);
                layout.openings = DoorSlots.All; layout.width = float.NaN; Assert.IsFalse(layout.Valid);
            }
            finally { Object.DestroyImmediate(layout); }
        }
    }
}
