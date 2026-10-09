using UnityEngine;
using WaitYourTurn.Combat;

namespace WaitYourTurn.Defenses
{
    /// <summary>Collider-free reusable trap. Only hostile bodies trigger it, solid cover blocks blast.</summary>
    public sealed class MineController : MonoBehaviour
    {
        [SerializeField] private HealthComponent source;
        [SerializeField] private Transform body, diode;
        [SerializeField] private ExplosionPulse explosion;
        private TargetRegistry registry;
        private HealthComponent credit;
        private uint creditLife;
        private LayerMask mask;
        private float arming, scanRemaining, blink;
        private bool paused;
        private readonly RadialDamage blast = new RadialDamage();
        private readonly HitscanResolver sight = new HitscanResolver();
        public MineDefinition Definition { get; private set; }
        public bool Deployed { get; private set; }
        public bool Occupied => Deployed || explosion.Visible;
        public float ArmingRemaining => arming;
        public int Detonations { get; private set; }
        public int LastBlastHits { get; private set; }
        public void Configure(HealthComponent damageSource, Transform shape, Transform indicator, ExplosionPulse burst)
        { source = damageSource; body = shape; diode = indicator; explosion = burst; }
        public bool TryDeploy(MineDefinition definition, HealthComponent owner, TargetRegistry targets, LayerMask shots, Vector3 position)
        {
            if (Occupied || definition == null || !definition.Valid || owner == null || !owner.IsAlive || targets == null) return false;
            Definition = definition; credit = owner; creditLife = owner.LifeVersion; registry = targets; mask = shots;
            transform.position = position; arming = definition.armingSeconds; scanRemaining = 0; blink = 0;
            source.ResetForSpawn(1, owner.Team); source.Invulnerable = true;
            body.gameObject.SetActive(true); diode.gameObject.SetActive(true); Deployed = true; return true;
        }
        public void RestoreArming(float remaining) => arming = remaining;
        public void SetPaused(bool value) => paused = value;
        private void Update() { if (!paused && Time.timeScale > 0) Tick(Time.deltaTime); }
        public void Tick(float seconds)
        {
            explosion.Tick(seconds);
            if (!Deployed) return;
            if (credit == null || !credit.IsAlive || credit.LifeVersion != creditLife) { Clear(); return; }
            arming = Mathf.Max(0, arming - seconds); blink += seconds;
            diode.gameObject.SetActive(blink % .65f < .3f);
            if (arming > 0) return;
            scanRemaining -= seconds; if (scanRemaining > 0) return;
            scanRemaining = Definition.scanInterval;
            Vector3 center = transform.position + Vector3.up * .85f;
            foreach (var target in registry.Targets)
            {
                if (!HitscanResolver.Hostile(source, target)) continue;
                Vector3 delta = target.transform.position - transform.position;
                if (Mathf.Abs(delta.y) > .8f || delta.x * delta.x + delta.z * delta.z > Definition.triggerRadius * Definition.triggerRadius ||
                    sight.BlockedByGeometry(center, target.transform.position + Vector3.up * .85f, mask)) continue;
                Deployed = false; body.gameObject.SetActive(false); diode.gameObject.SetActive(false); Detonations++;
                LastBlastHits = blast.Apply(registry, source, credit.Identity, center, Definition.blastRadius, Definition.damage, mask);
                explosion.Show(center, Definition.blastRadius); return;
            }
        }
        public void Clear()
        {
            Deployed = false; Definition = null; credit = null;
            body.gameObject.SetActive(false); diode.gameObject.SetActive(false); explosion.Clear();
        }
    }
}
