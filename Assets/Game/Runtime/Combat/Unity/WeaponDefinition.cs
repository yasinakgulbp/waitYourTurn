using UnityEngine;

namespace WaitYourTurn.Combat
{
    public enum ShotDelivery { Hitscan, Grenade }
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
        public ShotDelivery delivery;
        [Min(.1f)] public float projectileSpeed = 14;
        [Range(.02f, .3f)] public float projectileRadius = .1f;
        [Min(.1f)] public float blastRadius = 2.4f;
        public bool ValidDelivery => delivery == ShotDelivery.Hitscan || delivery == ShotDelivery.Grenade &&
            projectileSpeed > 0 && projectileSpeed <= 100 && projectileRadius >= .02f && projectileRadius <= .3f &&
            blastRadius > 0 && blastRadius <= 10 && pellets == 1;
        public WeaponSpec CreateSpec() => new WeaponSpec(damage, fireInterval, range, magazineSize,
            reloadSeconds, infiniteReserve, initialReserve, pellets, spreadDegrees);
    }
}
