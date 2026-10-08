using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using WaitYourTurn.Combat;
using WaitYourTurn.Train;

namespace WaitYourTurn.Enemies
{
    /// <summary>Bounded factory. Death returns in LateUpdate, outside the health notification stack.</summary>
    public sealed class EnemyPool : MonoBehaviour
    {
        [SerializeField] private EnemyBrain template;
        [SerializeField] private DoorController door;
        [SerializeField] private WagonGeometry geometry;
        private NavMeshPath entryPath;
        private readonly Collider[] spawnOverlap = new Collider[16];
        [SerializeField] private HealthComponent player;
        [SerializeField] private TargetRegistry registry;
        [SerializeField, Min(1)] private int capacity = 12;
        [SerializeField] private bool incrementalWarmup;
        private readonly List<EnemyBrain> all = new List<EnemyBrain>(12);
        private readonly List<EnemyBrain> active = new List<EnemyBrain>(12);
        private readonly Stack<EnemyBrain> available = new Stack<EnemyBrain>(12);
        private int spawnSequence;
        private bool playerPresent = true;
        private string wagonId = "lab-wagon";
        public IReadOnlyList<EnemyBrain> Active => active;
        public int CreatedCount => all.Count;
        public int Capacity => capacity;
        public bool CanRent => available.Count > 0;
        public int DeathCount { get; private set; }
        public event System.Action<DeathNotice, int> Killed;
        public int AgentTypeId => template.AgentTypeId;
        public int AreaMask => template.AreaMask;
        public void SetScope(string id, bool hasPlayer)
        {
            wagonId = id; playerPresent = hasPlayer;
            foreach (EnemyBrain enemy in active) enemy.SetScope(id, hasPlayer);
        }
        public void SetPaused(bool paused) { foreach (EnemyBrain enemy in active) enemy.SetPaused(paused); }
        public void Configure(EnemyBrain prefab, DoorController entry, HealthComponent hero, TargetRegistry targets)
        { template = prefab; door = entry; player = hero; registry = targets; }
        public void ConfigureGeometry(WagonGeometry space) => geometry = space;
        public void ConfigureIncrementalWarmup() => incrementalWarmup = true;
        private void Awake()
        {
            entryPath = new NavMeshPath();
            if (!incrementalWarmup) while (WarmOne()) { }
        }
        public bool WarmOne()
        {
            if (all.Count >= capacity) return false;
            EnemyBrain enemy = Instantiate(template, transform);
            enemy.name = "Pooled Zombie " + (all.Count + 1);
            enemy.gameObject.SetActive(false);
            enemy.PrepareWarmup();
            enemy.Health.Died += OnEnemyDeath;
            all.Add(enemy); available.Push(enemy); return true;
        }
        public bool TrySpawn(Vector3 point, EnemyProfile profile = null, int station = 0)
        {
            if (!player.IsAlive || available.Count == 0) return false;
            // Validate before renting/activating. An invalid request neither grows nor drains the pool.
            var filter = new NavMeshQueryFilter { agentTypeID = template.AgentTypeId, areaMask = template.AreaMask };
            if (!NavMesh.SamplePosition(point, out NavMeshHit hit, 0.5f, filter)) return false;
            if (profile != null)
            {
                // A spawn may not materialize inside another body or static geometry. Try another authored anchor later.
                int overlaps = Physics.OverlapCapsuleNonAlloc(hit.position + Vector3.up * .35f,
                    hit.position + Vector3.up * 1.35f, .3f, spawnOverlap, Physics.AllLayers, QueryTriggerInteraction.Ignore);
                if (overlaps > 0) return false;
            }
            DoorController entry = geometry != null ? geometry.SelectEntry(hit.position, AgentTypeId, AreaMask, entryPath) : door;
            if (entry == null) return false;
            EnemyBrain enemy = available.Pop();
            if (!enemy.Spawn(hit.position, entry, player, spawnSequence++ % capacity, geometry, profile, station))
            { enemy.Despawn(); enemy.gameObject.SetActive(false); available.Push(enemy); return false; }
            enemy.gameObject.SetActive(true);
            enemy.SetScope(wagonId, playerPresent);
            active.Add(enemy);
            registry.Register(enemy.Health);
            return true;
        }
        private void LateUpdate()
        {
            for (int i = active.Count - 1; i >= 0; i--)
            {
                if (active[i].Health.IsAlive) continue;
                DeathCount++;
                ReturnAt(i);
            }
        }
        private void OnEnemyDeath(DeathNotice death)
        {
            // The health notification still owns this life; profile/credit are captured before pool return.
            foreach (var enemy in active)
                if (enemy.Health.Identity.RuntimeId == death.Target.RuntimeId)
                { Killed?.Invoke(death, enemy.Profile != null ? enemy.Profile.reward : 0); return; }
        }
        private void ReturnAt(int index)
        {
            EnemyBrain enemy = active[index];
            registry.Unregister(enemy.Health);
            active.RemoveAt(index);
            enemy.Despawn();
            enemy.gameObject.SetActive(false);
            available.Push(enemy);
        }
        public void ClearAlive()
        {
            for (int i = active.Count - 1; i >= 0; i--) ReturnAt(i);
            DeathCount = 0;
            spawnSequence = 0;
        }
        public void RemoveStationOutsiders()
        {
            // Station cleanup is never a kill/reward. Inside decisions must be captured before calling.
            for (int i = active.Count - 1; i >= 0; i--)
                if (!active[i].OnBoard) ReturnAt(i);
        }
        private void OnDestroy()
        {
            foreach (var enemy in all) if (enemy != null) enemy.Health.Died -= OnEnemyDeath;
            if (registry == null) return;
            foreach (EnemyBrain enemy in active) if (enemy != null) registry.Unregister(enemy.Health);
        }
    }
}
