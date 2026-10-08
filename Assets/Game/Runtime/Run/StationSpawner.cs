using UnityEngine;

namespace WaitYourTurn.Run
{
    /// <summary>Small bounded M4 station adapter; richer enemy profiles/programs belong to M6.</summary>
    public sealed class StationSpawner : MonoBehaviour
    {
        [SerializeField] private RunDriver run;
        [SerializeField, Min(0.1f)] private float firstDelay = 2;
        [SerializeField, Min(0.1f)] private float interval = 2;
        [SerializeField, Min(1)] private int initialBudgetPerWagon = 8;
        [SerializeField, Min(1)] private int maximumBudgetPerWagon = 20;
        private int station;
        private int[] attempted;
        private float nextAt;
        private RunFlow observedFlow;
        public int Attempts { get; private set; }
        public void Configure(RunDriver owner) => run = owner;
        private void Awake() => attempted = new int[run.Wagons.Length];
        private void Update()
        {
            if (!ReferenceEquals(observedFlow, run.Flow)) { observedFlow = run.Flow; ResetSchedule(); }
            if (run.Flow == null || run.Flow.Phase != RunPhase.Defense) return;
            if (station != run.Flow.Station)
            { station = run.Flow.Station; System.Array.Clear(attempted, 0, attempted.Length); nextAt = Time.time + firstDelay; }
            if (Time.time < nextAt) return;
            nextAt = Time.time + interval; // No queued/catch-up bursts.
            int budget = Mathf.Min(maximumBudgetPerWagon, initialBudgetPerWagon + station - 1);
            for (int i = 0; i < run.Wagons.Length; i++)
            {
                if (attempted[i] >= budget) continue;
                run.Wagons[i].SpawnOutside(attempted[i]++); Attempts++;
            }
        }
        public void ResetSchedule() { station = 0; Attempts = 0; }
    }
}
