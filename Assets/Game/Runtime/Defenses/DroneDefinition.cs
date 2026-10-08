using UnityEngine;
using WaitYourTurn.Combat;

namespace WaitYourTurn.Defenses
{
    [CreateAssetMenu(menuName = "Wait Your Turn/Drone")]
    public sealed class DroneDefinition : ScriptableObject
    {
        public WeaponDefinition weapon;
        public float hoverHeight = 1.95f;
        public Vector2 followOffset = new Vector2(.4f, .35f);
        public float followSmoothTime = .22f;
        public float followSpeed = 7;
        public float selectionInterval = .2f;
        public float flightRadius = .14f;
        public float diveSpeed = 6;
        public float searchSeconds = 4;
        public float impactDistance = .3f;
        public float blastRadius = 1.8f;
        public float blastDamage = 18;
        public float bobAmplitude = .05f;
        public float bobFrequency = 1.6f;
        public Color color = Color.cyan;
        public bool Valid => ValidWeapon() &&
            weapon.pellets == 1 && Positive(hoverHeight) && Positive(followSmoothTime) && Positive(followSpeed) &&
            Positive(selectionInterval) && Positive(flightRadius) && Positive(diveSpeed) && Positive(searchSeconds) &&
            Positive(impactDistance) && impactDistance >= flightRadius && Positive(blastRadius) && Positive(blastDamage) &&
            Finite(followOffset.sqrMagnitude) && Finite(bobAmplitude) && bobAmplitude >= 0 && Positive(bobFrequency);
        private bool ValidWeapon()
        {
            if (weapon == null) return false;
            try { var spec = weapon.CreateSpec(); return !spec.InfiniteReserve && spec.InitialReserve == 0 && spec.Pellets == 1; }
            catch (System.ArgumentException) { return false; }
        }
        private static bool Positive(float v) => v > 0 && Finite(v);
        private static bool Finite(float v) => !float.IsNaN(v) && !float.IsInfinity(v);
    }
}
