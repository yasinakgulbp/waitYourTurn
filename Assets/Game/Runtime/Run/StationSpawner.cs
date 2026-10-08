using System;
using UnityEngine;

namespace WaitYourTurn.Run
{
    /// <summary>Run adapter for data-driven bounded spawning; old labs retain their original small schedule.</summary>
    public sealed class StationSpawner : MonoBehaviour
    {
        [SerializeField] private RunDriver run;
        [SerializeField] private StationDefinition[] programs;
        [SerializeField, Min(.1f)] private float firstDelay = 2;
        [SerializeField, Min(.1f)] private float interval = 2;
        [SerializeField, Min(1)] private int initialBudgetPerWagon = 8;
        [SerializeField, Min(1)] private int maximumBudgetPerWagon = 20;
        private int station, warmCursor, totalActive;
        private int[] attempted, activeCounts;
        private float nextAt;
        private RunFlow observedFlow;
        private SpawnSchedule schedule;
        private StationDefinition definition;
        private Func<SpawnRequest, SpawnResult> spawn;
        public int Attempts { get; private set; }
        public int CreatedThisFrame { get; private set; }
        public int SpawnedThisFrame { get; private set; }
        public int Pending => schedule?.Pending ?? 0;
        public int Spawned => schedule?.Spawned ?? 0;
        public int Dropped => schedule?.Dropped ?? 0;
        public int SuppressedStreams => schedule?.SuppressedStreams ?? 0;
        public string Diagnostic { get; private set; }
        public StationDefinition Definition => definition;
        public StationDefinition[] Programs => programs;
        public int TotalActive { get { int count = 0; foreach (var wagon in run.Wagons) count += wagon.Enemies.Active.Count; return count; } }
        public void Configure(RunDriver owner) => run = owner;
        public void ConfigurePrograms(StationDefinition[] values) => programs = values;
        private void Awake()
        {
            attempted = new int[run.Wagons.Length]; activeCounts = new int[run.Wagons.Length]; spawn = TrySpawn;
        }
        private void Update()
        {
            CreatedThisFrame = SpawnedThisFrame = 0;
            if (!ReferenceEquals(observedFlow, run.Flow)) { observedFlow = run.Flow; ResetSchedule(); }
            bool dataDriven = programs != null && programs.Length > 0;
            if (dataDriven) WarmPools(Mathf.Clamp(definition != null ? definition.warmupPerFrame : 4, 1, 32));
            if (run.Flow == null) return;
            if (run.Flow.Phase != RunPhase.Defense) { schedule?.Clear(); return; }
            if (station != run.Flow.Station)
            {
                station = run.Flow.Station; Array.Clear(attempted, 0, attempted.Length); nextAt = Time.time + firstDelay;
                if (dataDriven && !BeginStation()) return;
            }
            if (dataDriven)
            {
                if (schedule == null || run.Flow.Paused || !run.Player.IsAlive) return;
                totalActive = 0;
                for (int i = 0; i < activeCounts.Length; i++)
                { activeCounts[i] = run.Wagons[i].Enemies.Active.Count; totalActive += activeCounts[i]; }
                int dropped = schedule.Dropped;
                schedule.Tick(run.Flow.Elapsed, definition.requestsPerFrame, spawn); Attempts = schedule.Attempts;
                if (dropped == 0 && schedule.Dropped > 0)
                    Debug.LogWarning($"[StationSpawner] Station {station}: no valid spawn after {definition.positionAttempts} positions; request dropped.", this);
                return;
            }
            if (Time.time < nextAt) return;
            nextAt = Time.time + interval;
            int budget = Mathf.Min(maximumBudgetPerWagon, initialBudgetPerWagon + station - 1);
            for (int i = 0; i < run.Wagons.Length; i++)
            {
                if (!run.Wagons[i].HasLivingDefender) continue;
                if (attempted[i] >= budget) continue;
                run.Wagons[i].SpawnOutside(attempted[i]++); Attempts++;
            }
        }
        private bool BeginStation()
        {
            definition = null; schedule = null; Diagnostic = null;
            foreach (var candidate in programs)
            {
                if (candidate == null) return Reject("Missing station definition.");
                if (!candidate.Validate(run.Wagons.Length, out string reason)) return Reject(candidate.name + ": " + reason);
                if (candidate.firstStation <= station && (definition == null || candidate.firstStation > definition.firstStation)) definition = candidate;
            }
            if (definition == null) return Reject("No program starts at or before this station.");
            foreach (var wagon in run.Wagons)
                if (definition.wagonLiveLimit > wagon.Enemies.Capacity || wagon.Geometry == null || wagon.Geometry.SpawnCount == 0)
                    return Reject(wagon.Id + ": live limit exceeds pool capacity, or geometry/spawn anchors are missing.");
            schedule = new SpawnSchedule(definition.BuildStreams(run.Wagons.Length), definition.queueCapacity,
                definition.positionAttempts, definition.retryDelay);
            return true;
        }
        private bool Reject(string reason)
        { Diagnostic = reason; Debug.LogError("[StationSpawner] " + reason, this); return false; }
        private SpawnResult TrySpawn(SpawnRequest request)
        {
            var wagon = run.Wagons[request.Wagon];
            if (!wagon.HasLivingDefender) return SpawnResult.Unoccupied;
            if (totalActive >= definition.globalLiveLimit || activeCounts[request.Wagon] >= definition.wagonLiveLimit || !wagon.Enemies.CanRent)
                return SpawnResult.CapacityFull;
            // Each retry uses another authored anchor; neither moving scenery nor windows are spawn sources.
            int anchor = attempted[request.Wagon]++;
            if (!wagon.SpawnOutside(anchor, definition.bands[request.Profile].profile, station)) return SpawnResult.InvalidPosition;
            activeCounts[request.Wagon]++; totalActive++; SpawnedThisFrame++; return SpawnResult.Spawned;
        }
        private void WarmPools(int budget)
        {
            int inspected = 0, created = 0;
            while (created < budget && inspected++ < run.Wagons.Length * budget)
            {
                var pool = run.Wagons[warmCursor].Enemies; warmCursor = (warmCursor + 1) % run.Wagons.Length;
                if (pool.WarmOne()) { created++; CreatedThisFrame++; }
            }
        }
        public void ResetSchedule()
        { station = 0; Attempts = 0; schedule = null; definition = null; Diagnostic = null; }
        private void OnDisable() => ResetSchedule();
    }
}
