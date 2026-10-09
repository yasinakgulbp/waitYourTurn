using UnityEngine;
using WaitYourTurn.Combat;
using WaitYourTurn.Train;

namespace WaitYourTurn.Run
{
    /// <summary>Run-only strength. Uses authored base HP; never compounds successive multipliers.</summary>
    public sealed class RunReinforcement : MonoBehaviour
    {
        [SerializeField] private RunDriver run;
        [SerializeField] private float[] multipliers = { 1, 1.5f, 2 };
        [SerializeField] private DoorReinforcementVisual[] visuals;
        public int Level { get; private set; }
        public string DefinitionKey => multipliers == null ? "invalid" :
            string.Join("/", System.Array.ConvertAll(multipliers, value => value.ToString("R", System.Globalization.CultureInfo.InvariantCulture)));
        public void Configure(RunDriver owner, DoorReinforcementVisual[] views) { run = owner; visuals = views; }
        private void OnEnable() => run.Restarted += ResetLevel;
        private void OnDisable() => run.Restarted -= ResetLevel;
        private void ResetLevel() => RestoreLevel(0);
        public bool ValidLevel(int level) => multipliers != null && multipliers.Length == 3 && level >= 0 && level < 3 &&
            multipliers[0] == 1 && multipliers[1] > 1 && multipliers[2] > multipliers[1] && multipliers[2] <= 10;
        public float MaximumAt(float authored, int level) => ValidLevel(level) ? authored * multipliers[level] : 0;
        public bool CanUpgrade(int level) => isActiveAndEnabled && run.UsesOpenTrainSurvival && ValidLevel(level) && level == Level + 1;
        public bool TryUpgrade(int level)
        {
            if (!CanUpgrade(level)) return false;
            foreach (var wagon in run.Wagons) foreach (var door in wagon.Doors)
                door.Durability.ChangeMaxHealth(MaximumAt(door.Durability.StartingMaximum, level), MaxHealthPolicy.HealAddedCapacity);
            RestoreLevel(level); return true;
        }
        public void RestoreLevel(int level)
        {
            if (!ValidLevel(level)) throw new System.ArgumentOutOfRangeException(nameof(level));
            Level = level;
            if (visuals != null) foreach (var visual in visuals) if (visual != null) visual.SetLevel(level);
        }
    }
}
