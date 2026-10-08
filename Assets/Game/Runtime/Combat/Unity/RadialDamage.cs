using UnityEngine;

namespace WaitYourTurn.Combat
{
    /// <summary>Bounded live-registry snapshot, one hit per body/life, solid cover blocks blast damage.</summary>
    public sealed class RadialDamage
    {
        private readonly HealthComponent[] targets = new HealthComponent[128];
        private readonly uint[] lives = new uint[128];
        private readonly HitscanResolver resolver = new HitscanResolver();
        private ulong attack;
        private bool applying;
        public int Overflows { get; private set; }
        public int Apply(TargetRegistry registry, HealthComponent source, EntityIdentity credit,
            Vector3 center, float radius, float damage, LayerMask mask)
        {
            if (applying || registry == null || source == null || !source.IsAlive ||
                !Positive(radius) || !Positive(damage) || float.IsNaN(center.sqrMagnitude) || float.IsInfinity(center.sqrMagnitude)) return 0;
            int count = registry.Targets.Count;
            if (count > targets.Length) { Overflows++; return 0; }
            for (int i = 0; i < count; i++)
            { targets[i] = registry.Targets[i]; lives[i] = targets[i] != null ? targets[i].LifeVersion : 0; }
            applying = true; int hit = 0;
            EntityIdentity identity = source.Identity; Team team = source.Team;
            try
            {
                for (int i = 0; i < count; i++)
                {
                    var target = targets[i];
                    if (!HitscanResolver.Hostile(source, target) || target.LifeVersion != lives[i]) continue;
                    Vector3 aim = target.transform.position + Vector3.up * .85f;
                    if ((aim - center).sqrMagnitude > radius * radius || resolver.BlockedByGeometry(center, aim, mask)) continue;
                    if (target.TryApplyDamage(new DamageContext(damage, identity, team, ++attack, lives[i], rewardOwner: credit)).Applied) hit++;
                }
                return hit;
            }
            finally
            { applying = false; for (int i = 0; i < count; i++) targets[i] = null; }
        }
        private static bool Positive(float value) => value > 0 && !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
