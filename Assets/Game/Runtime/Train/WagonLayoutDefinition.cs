using System;
using UnityEngine;

namespace WaitYourTurn.Train
{
    [Flags]
    public enum DoorSlots
    {
        SouthLeft = 1, NorthLeft = 2, SouthCenter = 4,
        NorthCenter = 8, SouthRight = 16, NorthRight = 32,
        All = 63
    }

    /// <summary>Physical authoring data. Visual models never define the gameplay bounds.</summary>
    [CreateAssetMenu(menuName = "Wait Your Turn/Wagon Layout")]
    public sealed class WagonLayoutDefinition : ScriptableObject
    {
        public float length = 12;
        public float width = 4.2f;
        public float wallHeight = 2.1f;
        public float doorWidth = 1.45f;
        public float outerDoorX = 3.8f;
        public float doorHealth = 10;
        public float windowBottom = .7f;
        public float windowTop = 1.6f;
        public float windowBorder = .24f;
        public float windowPost = .12f;
        public float maxWindowWidth = 2.2f;
        public DoorSlots openings = DoorSlots.All;
        public int DoorCount
        {
            get { int count = 0; for (int slot = 0; slot < 6; slot++) if (HasDoor(slot)) count++; return count; }
        }
        public bool HasDoor(int slot) => (openings & (DoorSlots)(1 << slot)) != 0;
        public float DoorX(int slot) => (slot / 2 - 1) * outerDoorX;
        public bool Valid => Finite(length) && Finite(width) && Finite(wallHeight) &&
            Finite(doorWidth) && Finite(outerDoorX) && Finite(doorHealth) &&
            Finite(windowBottom) && Finite(windowTop) && Finite(windowBorder) && Finite(windowPost) && Finite(maxWindowWidth) &&
            windowBottom >= .6f && windowTop > windowBottom + .2f && windowTop < wallHeight &&
            windowBorder >= .24f && windowPost >= .08f && maxWindowWidth >= .5f &&
            length >= 8 && width >= 3 && wallHeight >= 1.7f && doorWidth >= 1 &&
            doorHealth > 0 && outerDoorX > doorWidth + .3f &&
            outerDoorX + doorWidth * .5f < length * .5f - .3f &&
            (openings & ~DoorSlots.All) == 0 && DoorCount >= 4 && DoorCount <= 6 &&
            (openings & (DoorSlots.SouthLeft | DoorSlots.NorthLeft | DoorSlots.SouthRight | DoorSlots.NorthRight)) ==
            (DoorSlots.SouthLeft | DoorSlots.NorthLeft | DoorSlots.SouthRight | DoorSlots.NorthRight);
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
