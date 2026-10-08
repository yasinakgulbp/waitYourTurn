using System.Collections.Generic;
using WaitYourTurn.Combat;

namespace WaitYourTurn.Economy
{
    /// <summary>One observed life per bounded pool actor, not an ever-growing set of historical kills.</summary>
    public sealed class KillRewards
    {
        private readonly Dictionary<ulong, uint> observed = new Dictionary<ulong, uint>();
        private readonly Wallet wallet;
        public EntityIdentity Owner { get; private set; }
        public int TrackedActors => observed.Count;
        public KillRewards(Wallet currency) => wallet = currency;
        public void BeginRun(EntityIdentity owner) { Owner = owner; observed.Clear(); }
        public bool Observe(DeathNotice death, int amount, bool accepting)
        {
            if (!accepting || !death.Result.Killed || !death.Result.Applied || death.Target.RuntimeId == 0 ||
                death.Target.LifeVersion == 0 || death.Damage.TargetLifeVersion != death.Target.LifeVersion) return false;
            if (observed.TryGetValue(death.Target.RuntimeId, out uint previous) && previous >= death.Target.LifeVersion) return false;
            observed[death.Target.RuntimeId] = death.Target.LifeVersion;
            var credit = death.Damage.RewardOwner;
            if (death.Damage.SourceTeam != Team.Player || credit.RuntimeId != Owner.RuntimeId ||
                credit.LifeVersion != Owner.LifeVersion || Owner.RuntimeId == 0) return false;
            return wallet.TryCredit(amount);
        }
    }
}
