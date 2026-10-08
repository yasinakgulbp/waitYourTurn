using UnityEngine;
using UnityEngine.AI;
using WaitYourTurn.Combat;
using WaitYourTurn.Economy;
using WaitYourTurn.Player;
using WaitYourTurn.Train;

namespace WaitYourTurn.Run
{
    public enum BotState { Inactive, Holding, Engaging, Evading, Repairing, Eliminated }

    /// <summary>Decision adapter. Uses the same motor, aim, weapon, repair and transaction rules as the human.</summary>
    public sealed class BotController : MonoBehaviour, IShopEffects
    {
        [SerializeField] private string nickname;
        [SerializeField] private BotProfile profile;
        [SerializeField] private HealthComponent health;
        [SerializeField] private PlayerMotor motor;
        [SerializeField] private AutoAim aim;
        [SerializeField] private HitscanWeapon weapon;
        [SerializeField] private ProximityRepair repair;
        private RunDriver run;
        private ShopCatalog catalog;
        private RunDefenses defenses;
        private readonly Wallet wallet = new Wallet();
        private KillRewards rewards;
        private ShopService shop;
        private ulong context, request;
        private float nextDecision, nextPurchase;
        private bool paused;
        private Vector3 goal;
        private NavMeshPath path;
        private readonly Vector3[] corners = new Vector3[32];
        private int cornerCount, corner;
        public string Nickname => nickname;
        public BotProfile Profile => profile;
        public HealthComponent Health => health;
        public HitscanWeapon Weapon => weapon;
        public PlayerMotor Motor => motor;
        public ProximityRepair Repair => repair;
        public Wallet Wallet => wallet;
        public WagonRuntime Wagon { get; private set; }
        public BotState State { get; private set; }
        public bool ThinkingEnabled { get; set; } = true;
        public int Shots { get; private set; }
        public int Purchases { get; private set; }
        public bool CanShop => health.IsAlive && Wagon != null && run.Flow != null && !run.Flow.Paused;
        public void Configure(string name, BotProfile skill, HealthComponent life, PlayerMotor movement,
            AutoAim targeting, HitscanWeapon gun, ProximityRepair interaction)
        { nickname = name; profile = skill; health = life; motor = movement; aim = targeting; weapon = gun; repair = interaction; }
        private void Awake() => path = new NavMeshPath();
        private void OnEnable() { health.Died += OnDeath; weapon.Fired += OnShot; }
        private void OnDisable() { health.Died -= OnDeath; weapon.Fired -= OnShot; }
        private void OnShot(ShotNotice shot) { if (shot.Pellet == 0) Shots++; }
        private void OnDeath(DeathNotice death) { State = BotState.Eliminated; SetPaused(true); }
        public void Begin(RunDriver owner, ShopCatalog products, RunDefenses turrets)
        {
            run = owner; catalog = products; defenses = turrets;
            if (rewards == null) { rewards = new KillRewards(wallet); shop = new ShopService(wallet, this); }
            health.Invulnerable = false; health.ResetForSpawn(run.Player.Maximum, Team.Player);
            weapon.ConfigureInventory(true); weapon.ResetWeapon(); repair.Cancel(); aim.ClearTarget();
            aim.ConfigureReaction(profile.aimInterval, profile.reactionDelay); motor.ConfigureWorldControl(health, profile.moveSpeed);
            wallet.Reset(catalog.startingCoins); rewards.BeginRun(health.Identity);
            context++; shop.SetContext(context); nextDecision = nextPurchase = 0; Shots = Purchases = 0; ThinkingEnabled = true;
            Wagon = null; State = BotState.Holding; SetPaused(true); gameObject.SetActive(true);
        }
        public void Clear()
        { SetPaused(true); Wagon = null; State = BotState.Inactive; gameObject.SetActive(false); }
        public void Assign(WagonRuntime wagon, Vector3 point, bool arrival)
        {
            Wagon = wagon; motor.SetMovementArea(wagon.Area); motor.Place(point); aim.ClearTarget(); repair.Cancel();
            repair.SetDoor(wagon.NearestDoor(point)); goal = point; cornerCount = 0; nextDecision = 0;
            wagon.SetDefender(health); context++; shop.SetContext(context);
            if (arrival) health.SetDamageProtection(run.ArrivalProtectionSeconds);
        }
        public void SetPaused(bool value)
        {
            paused = value; weapon.Paused = value; aim.enabled = !value && health.IsAlive;
            repair.Paused = value; if (value) motor.SetWorldMove(Vector3.zero);
        }
        public void ObserveKill(DeathNotice death, int reward)
        { rewards?.Observe(death, reward, CanShop); }
        private void Update()
        {
            if (paused || !health.IsAlive || Wagon == null || Time.deltaTime <= 0 || !ThinkingEnabled)
            { motor.SetWorldMove(Vector3.zero); return; }
            if (Time.time >= nextDecision) { nextDecision = Time.time + profile.decisionInterval; Decide(); }
            Vector3 move = Vector3.zero;
            if (corner < cornerCount)
            {
                move = corners[corner] - transform.position; move.y = 0;
                if (move.sqrMagnitude < .12f * .12f) { corner++; move = Vector3.zero; }
            }
            motor.SetWorldMove(move.sqrMagnitude > .001f ? move.normalized : Vector3.zero);
        }
        private void Decide()
        {
            TryShopping();
            HealthComponent nearest = null; float closest = float.MaxValue;
            foreach (var enemy in Wagon.Enemies.Active)
            {
                if (!enemy.Health.IsAlive || !enemy.IsInside) continue;
                Vector3 delta = enemy.transform.position - transform.position; delta.y = 0;
                if (delta.sqrMagnitude < closest) { closest = delta.sqrMagnitude; nearest = enemy.Health; }
            }
            if (nearest != null && closest < profile.dangerDistance * profile.dangerDistance)
            { repair.SetDoor(null); State = BotState.Evading; SetGoal(SafestPoint(nearest.transform.position)); return; }
            DoorController damaged = repair.Target;
            if (damaged == null || !damaged.NeedsRepair) damaged = RepairCandidate();
            if (damaged != null)
            {
                repair.SetDoor(damaged); State = BotState.Repairing;
                SetGoal(damaged.RepairPosition + Vector3.up * .05f); return;
            }
            repair.SetDoor(null);
            // Follow the most threatened door with small personal offsets, not a scripted animation.
            HealthComponent threat = null; float distance = float.MaxValue;
            foreach (var enemy in Wagon.Enemies.Active)
            {
                if (!enemy.Health.IsAlive) continue;
                float d = (enemy.transform.position - transform.position).sqrMagnitude;
                if (d < distance) { distance = d; threat = enemy.Health; }
            }
            if (threat != null)
            {
                State = BotState.Engaging;
                var door = Wagon.NearestDoor(threat.transform.position);
                Vector3 at = door.RepairPosition + door.Portal.transform.forward * .55f;
                if (threat != nearest) SetGoal(Wagon.Geometry.Constrain(at, .42f));
                else SetGoal(SafestPoint(threat.transform.position));
            }
            else { State = BotState.Holding; SetGoal(Wagon.transform.position + Vector3.up * .05f); }
        }
        private DoorController RepairCandidate()
        {
            DoorController best = null; float score = float.MaxValue;
            foreach (var door in Wagon.Doors)
            {
                if (!door.NeedsRepair || door.Durability.IsAlive && door.Durability.Current / door.Durability.Maximum > profile.repairBelow) continue;
                float s = (door.RepairPosition - transform.position).sqrMagnitude + (door.Durability.IsAlive ? 4 : 0);
                if (s < score) { best = door; score = s; }
            }
            return best;
        }
        private Vector3 SafestPoint(Vector3 threat)
        {
            Vector3 best = transform.position; float clearance = -1;
            for (int i = 0; i < Wagon.Geometry.SafePositionCount; i++)
            {
                Vector3 candidate = Wagon.Geometry.Constrain(Wagon.Geometry.SafePosition(i), .42f);
                float value = (candidate - threat).sqrMagnitude;
                foreach (var enemy in Wagon.Enemies.Active)
                    if (enemy.Health.IsAlive && enemy.IsInside) value = Mathf.Min(value, (candidate - enemy.transform.position).sqrMagnitude);
                if (value > clearance && HasPath(candidate)) { best = candidate; clearance = value; }
            }
            return best;
        }
        private bool HasPath(Vector3 point)
        {
            var filter = new NavMeshQueryFilter { agentTypeID = Wagon.Enemies.AgentTypeId, areaMask = Wagon.Enemies.AreaMask };
            return NavMesh.SamplePosition(transform.position, out NavMeshHit from, .4f, filter) &&
                NavMesh.SamplePosition(point, out NavMeshHit to, .4f, filter) && Wagon.Geometry.Contains(to.position) &&
                NavMesh.CalculatePath(from.position, to.position, filter, path) && path.status == NavMeshPathStatus.PathComplete;
        }
        private void SetGoal(Vector3 point)
        {
            point = Wagon.Geometry.Constrain(point, .42f);
            // Refresh on each decision, including when a newly installed turret changes the route.
            goal = point; cornerCount = corner = 0;
            if (!HasPath(goal)) return;
            int count = path.GetCornersNonAlloc(corners);
            if (count == corners.Length) return;
            for (int i = 0; i < count; i++) if (!Wagon.Geometry.Contains(corners[i])) return;
            cornerCount = count; corner = count > 1 ? 1 : 0;
        }
        private void TryShopping()
        {
            if (Time.time < nextPurchase) return;
            nextPurchase = Time.time + Mathf.Max(profile.purchaseInterval, catalog.purchaseCooldown);
            ShopItem chosen = null;
            if (health.Current / health.Maximum < profile.healBelow) chosen = Find(ShopEffect.Heal);
            if (chosen == null && weapon.CanGrantOrRefill(profile.preferredWeapon)) chosen = Find(ShopEffect.Weapon, profile.preferredWeapon);
            if (chosen == null && profile.buyTurrets && Wagon.Enemies.Active.Count > 0) chosen = Find(ShopEffect.Turret);
            if (chosen != null && CanApply(chosen.Product) && wallet.Available >= chosen.price) Buy(chosen.Product);
            if (weapon.State.Empty && weapon.EquippedIndex != 0) weapon.Equip(0);
        }
        private ShopItem Find(ShopEffect effect, int index = -1)
        { foreach (var item in catalog.items) if (item.effect == effect && (index < 0 || item.weaponIndex == index)) return item; return null; }
        public PurchaseResult Buy(ShopProduct product) => shop.Buy(product, context, ++request, CanShop);
        public bool CanApply(ShopProduct product)
        {
            if (!CanShop) return false;
            switch (product.Effect)
            {
                case ShopEffect.Heal: return health.Current < health.Maximum;
                case ShopEffect.Weapon: return weapon.CanGrantOrRefill(product.WeaponIndex);
                case ShopEffect.RepairWagon: foreach (var door in Wagon.Doors) if (door.NeedsRepair) return true; return false;
                case ShopEffect.Turret: return defenses != null && defenses.CanBuyFor(product.TurretIndex, health, Wagon);
                default: return false;
            }
        }
        public bool TryApply(ShopProduct product)
        {
            if (!CanApply(product)) return false;
            bool applied;
            switch (product.Effect)
            {
                case ShopEffect.Heal: applied = health.TryShopHeal() > 0; break;
                case ShopEffect.Weapon: applied = weapon.TryGrantOrRefill(product.WeaponIndex); break;
                case ShopEffect.RepairWagon:
                    applied = false; foreach (var door in Wagon.Doors) if (door.TryRepair()) applied = true;
                    if (applied) repair.Cancel(); break;
                case ShopEffect.Turret: applied = defenses.TryBuyFor(product.TurretIndex, health, Wagon); break;
                default: return false;
            }
            if (applied) Purchases++; return applied;
        }
    }
}
