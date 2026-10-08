using UnityEngine;

namespace WaitYourTurn.Combat
{
    [CreateAssetMenu(menuName = "Wait Your Turn/Weapon")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        public string displayName = "Pistol";
        [Min(.01f)] public float damage = 2, fireInterval = .35f, range = 10, reloadSeconds = 1.2f;
        [Min(1)] public int magazineSize = 8;
        public bool infiniteReserve = true;
        [Min(0)] public int initialReserve;
        [Range(1, 16)] public int pellets = 1;
        [Range(0, 45)] public float spreadDegrees;
        public Color tracerColor = Color.yellow;
        public WeaponSpec CreateSpec() => new WeaponSpec(damage, fireInterval, range, magazineSize,
            reloadSeconds, infiniteReserve, initialReserve, pellets, spreadDegrees);
    }
}
