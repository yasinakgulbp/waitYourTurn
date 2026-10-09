using UnityEngine;

namespace WaitYourTurn.Run
{
    /// <summary>Boarding policy adapter. Shared flow, pools and train retain gameplay authority.</summary>
    public sealed class SurvivalJourney : MonoBehaviour
    {
        [SerializeField] private RunDriver run;
        [SerializeField] private StationSpawner spawner;
        public void Configure(RunDriver owner, StationSpawner waves) { run = owner; spawner = waves; }
        public int ExteriorAlive
        {
            get
            {
                int count = 0;
                foreach (var wagon in run.Wagons)
                    foreach (var enemy in wagon.Enemies.Active)
                        if (enemy.Health.IsAlive && !enemy.OnBoard) count++;
                return count;
            }
        }
        private void LateUpdate()
        {
            if (!run.UsesOpenTrainSurvival || run.Loading || !run.Player.IsAlive || !spawner.SpawningAllowed ||
                run.Flow == null || run.Flow.Phase != RunPhase.Defense || !spawner.WaveCompleted) return;
            if (ExteriorAlive == 0) run.Flow.CompleteBoardingWave();
        }
    }
}
