using UnityEngine;
using UnityEngine.AI;
using WaitYourTurn.Combat;

namespace WaitYourTurn.Defenses
{
    /// <summary>One reusable actor per authored mount. No shop, player, wagon or run dependency.</summary>
    public sealed class TurretController : MonoBehaviour
    {
        [SerializeField] private HealthComponent source;
        [SerializeField] private HitscanWeapon weapon;
        [SerializeField] private Collider body;
        [SerializeField] private NavMeshObstacle obstacle;
        [SerializeField] private Transform head;
        [SerializeField] private TurretPresentation presentation;
        private TargetRegistry registry;
        private HealthComponent creditedOwner, target;
        private uint ownerLife, targetLife;
        private float nextSelection;
        private bool paused;
        public bool Deployed { get; private set; }
        public bool Retiring { get; private set; }
        private readonly RadialDamage blast = new RadialDamage();
        public TurretDefinition Definition { get; private set; }
        public HitscanWeapon Weapon => weapon;
        public HealthComponent Target => target;
        public int ShotsFired { get; private set; }
        public int Exhaustions { get; private set; }
        public int BlastHits { get; private set; }
        public void Configure(HealthComponent owner, HitscanWeapon gun, Collider collider, NavMeshObstacle cut,
            Transform aimingHead, TurretPresentation view)
        { source = owner; weapon = gun; body = collider; obstacle = cut; head = aimingHead; presentation = view; }
        public bool TryDeploy(TurretDefinition definition, HealthComponent credit, TargetRegistry targets)
        {
            if (Deployed || Retiring || definition == null || !definition.Valid || credit == null || !credit.IsAlive || targets == null) return false;
            Definition = definition; creditedOwner = credit; ownerLife = credit.LifeVersion; registry = targets;
            target = null; nextSelection = Time.time; ShotsFired = 0; BlastHits = 0;
            source.ResetForSpawn(1, Team.Player); source.Invulnerable = true;
            weapon.Configure(source, credit); weapon.ConfigureDefinitions(new[] { definition.weapon }); weapon.ResetWeapon();
            Deployed = true; weapon.Paused = paused;
            gameObject.SetActive(true); body.enabled = obstacle.enabled = true;
            presentation.Show(definition.color);
            return true;
        }
        public void SetPaused(bool value)
        {
            if (value && !paused) target = null;
            paused = value; weapon.Paused = value || !Deployed;
        }
        public void Clear()
        {
            Deployed = false; target = null; creditedOwner = null; Definition = null;
            weapon.Paused = true; body.enabled = obstacle.enabled = false;
            presentation.Clear(); gameObject.SetActive(false);
        }
        private void Update()
        {
            if (!Deployed || paused || Time.timeScale <= 0) return;
            if (creditedOwner == null || !creditedOwner.IsAlive || creditedOwner.LifeVersion != ownerLife)
            { Clear(); return; }
            if (Time.time >= nextSelection)
            {
                nextSelection = Time.time + Definition.selectionInterval;
                target = NearestVisibleTarget.Select(registry, weapon, transform.position);
                if (target != null) targetLife = target.LifeVersion;
            }
            if (target == null || !target.IsAlive || target.LifeVersion != targetLife || !weapon.HasSight(target)) return;
            Vector3 direction = target.transform.position + Vector3.up * .85f - weapon.Muzzle;
            Vector3 facing = direction; facing.y = 0;
            if (facing.sqrMagnitude > .001f) head.rotation = Quaternion.LookRotation(facing);
            if (!weapon.TryFire(direction)) return;
            ShotsFired++;
            if (!weapon.State.Empty) return;
            // Last hit finishes before a single gameplay blast; the visual never applies damage.
            Retiring = true; Deployed = false; target = null; weapon.Paused = true;
            try { BlastHits = blast.Apply(registry, source, creditedOwner.Identity, weapon.Muzzle,
                Definition.blastRadius, Definition.blastDamage, weapon.HitMask); }
            finally { Retiring = false; body.enabled = obstacle.enabled = false; }
            Exhaustions++; presentation.Exhaust();
        }
    }
}
