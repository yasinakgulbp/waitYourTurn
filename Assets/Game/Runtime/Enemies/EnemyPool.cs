using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using WaitYourTurn.Combat;
using WaitYourTurn.Train;

namespace WaitYourTurn.Enemies
{
    /// <summary>Bounded M3 factory. Death returns in LateUpdate, outside the health notification stack.</summary>
    public sealed class EnemyPool : MonoBehaviour
    {
        [SerializeField] private EnemyBrain template;
        [SerializeField] private DoorController door;
        [SerializeField] private HealthComponent player;
        [SerializeField] private TargetRegistry registry;
        [SerializeField, Min(1)] private int capacity = 12;
        private readonly List<EnemyBrain> all = new List<EnemyBrain>(12);
        private readonly List<EnemyBrain> active = new List<EnemyBrain>(12);
        private readonly Stack<EnemyBrain> available = new Stack<EnemyBrain>(12);
        private int spawnSequence;
        public IReadOnlyList<EnemyBrain> Active => active;
        public int CreatedCount => all.Count;
        public int DeathCount { get; private set; }
        public void Configure(EnemyBrain prefab, DoorController entry, HealthComponent hero, TargetRegistry targets)
        { template = prefab; door = entry; player = hero; registry = targets; }
        private void Awake()
        {
            for (int i = 0; i < capacity; i++)
            {
                EnemyBrain enemy = Instantiate(template, transform);
                enemy.name = "Pooled Zombie " + (i + 1);
                enemy.gameObject.SetActive(false);
                all.Add(enemy);
                available.Push(enemy);
            }
        }
        public bool TrySpawn(Vector3 point)
        {
            if (!player.IsAlive || available.Count == 0) return false;
            // Validate before renting/activating. A invalid request neither grows nor drains the pool.
            var filter = new NavMeshQueryFilter { agentTypeID = template.AgentTypeId, areaMask = template.AreaMask };
            if (!NavMesh.SamplePosition(point, out NavMeshHit hit, 0.5f, filter)) return false;
            EnemyBrain enemy = available.Pop();
            if (!enemy.Spawn(hit.position, door, player, spawnSequence++ % capacity))
            { enemy.Despawn(); enemy.gameObject.SetActive(false); available.Push(enemy); return false; }
            enemy.gameObject.SetActive(true);
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
        private void OnDestroy()
        {
            if (registry == null) return;
            foreach (EnemyBrain enemy in active) if (enemy != null) registry.Unregister(enemy.Health);
        }
    }
}
