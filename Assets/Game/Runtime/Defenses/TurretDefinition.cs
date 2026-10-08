using UnityEngine;
using WaitYourTurn.Combat;

namespace WaitYourTurn.Defenses
{
    [CreateAssetMenu(menuName = "Wait Your Turn/Turret")]
    public sealed class TurretDefinition : ScriptableObject
    {
        public string id = "turret";
        public WeaponDefinition weapon;
        [Min(.05f)] public float selectionInterval = .2f;
        [Min(.01f)] public float blastRadius = 1.8f, blastDamage = 12;
        public Color color = Color.cyan;
        public bool Valid
        {
            get
            {
                if (string.IsNullOrWhiteSpace(id) || weapon == null || selectionInterval < .05f ||
                    float.IsNaN(selectionInterval) || float.IsInfinity(selectionInterval) ||
                    !Positive(blastRadius) || !Positive(blastDamage)) return false;
                try
                {
                    var spec = weapon.CreateSpec();
                    return !spec.InfiniteReserve && spec.InitialReserve == 0 && spec.Pellets == 1;
                }
                catch (System.ArgumentException) { return false; }
            }
        }
        private static bool Positive(float value) => value > 0 && !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
