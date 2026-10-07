namespace WaitYourTurn.Combat
{
    public enum Team { Neutral, Player, Enemy }
    public enum DamageKind { Physical, Fire, Infection }
    public enum DamageRejection { None, InvalidAmount, Dead, StaleLife, FriendlyFire, Invulnerable, NotificationInProgress }
    public enum MaxHealthPolicy { PreserveCurrent, HealAddedCapacity }

    public readonly struct EntityIdentity
    {
        public readonly ulong RuntimeId;
        public readonly uint LifeVersion;
        public EntityIdentity(ulong runtimeId, uint lifeVersion) { RuntimeId = runtimeId; LifeVersion = lifeVersion; }
    }

    public readonly struct DamageContext
    {
        public readonly float Amount;
        public readonly DamageKind Kind;
        public readonly EntityIdentity Source;
        public readonly EntityIdentity RewardOwner;
        public readonly Team SourceTeam;
        public readonly ulong AttackId;
        public readonly uint TargetLifeVersion;
        public readonly bool AllowFriendlyFire;

        public DamageContext(float amount, EntityIdentity source, Team sourceTeam, ulong attackId,
            uint targetLifeVersion, DamageKind kind = DamageKind.Physical, bool allowFriendlyFire = false,
            EntityIdentity? rewardOwner = null)
        {
            Amount = amount; Source = source; SourceTeam = sourceTeam; AttackId = attackId;
            TargetLifeVersion = targetLifeVersion; Kind = kind; AllowFriendlyFire = allowFriendlyFire;
            RewardOwner = rewardOwner ?? source;
        }
    }

    public readonly struct DamageResult
    {
        public readonly float AppliedAmount;
        public readonly bool Killed;
        public readonly DamageRejection Rejection;
        public bool Applied => Rejection == DamageRejection.None;
        public DamageResult(float amount, bool killed, DamageRejection rejection = DamageRejection.None)
        { AppliedAmount = amount; Killed = killed; Rejection = rejection; }
        public static DamageResult Reject(DamageRejection reason) => new DamageResult(0f, false, reason);
    }

    public readonly struct HealthSnapshot
    {
        public readonly float Current;
        public readonly float Maximum;
        public readonly uint LifeVersion;
        public readonly Team Team;
        public HealthSnapshot(float current, float maximum, uint lifeVersion, Team team)
        { Current = current; Maximum = maximum; LifeVersion = lifeVersion; Team = team; }
    }

    public readonly struct DeathNotice
    {
        public readonly EntityIdentity Target;
        public readonly DamageContext Damage;
        public readonly DamageResult Result;
        public DeathNotice(EntityIdentity target, DamageContext damage, DamageResult result)
        { Target = target; Damage = damage; Result = result; }
    }

    public interface IDamageable
    {
        uint LifeVersion { get; }
        DamageResult TryApplyDamage(DamageContext context);
    }
}
