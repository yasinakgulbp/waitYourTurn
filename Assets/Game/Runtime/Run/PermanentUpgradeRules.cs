using System;

namespace WaitYourTurn.Run
{
    public enum PermanentUpgrade { Health, Range }

    /// <summary>Bounded progression rules; costs and small steps are tunable, shared assets stay immutable.</summary>
    [Serializable]
    public sealed class PermanentUpgradeRules
    {
        public const int LevelLimit = 5;
        public const float MaximumStep = .02f;
        [UnityEngine.Range(0, MaximumStep)] public float healthStep = .02f, rangeStep = .02f;
        [UnityEngine.Min(1)] public int healthPrice = 10, rangePrice = 10;
        [UnityEngine.Min(0)] public int priceStep = 10;
        public bool Valid => StepValid(healthStep) && StepValid(rangeStep) && healthPrice > 0 && healthPrice <= 1000000 &&
            rangePrice > 0 && rangePrice <= 1000000 && priceStep >= 0 && priceStep <= 1000000;
        private static bool StepValid(float value) => value >= 0 && value <= MaximumStep;
        public int Price(PermanentUpgrade kind, int level) => Valid && level >= 0 && level < LevelLimit && Known(kind) ?
            (kind == PermanentUpgrade.Health ? healthPrice : rangePrice) + level * priceStep : 0;
        public float Multiplier(PermanentUpgrade kind, int level) => !Valid || !Known(kind) ? 1 :
            1 + Math.Max(0, Math.Min(LevelLimit, level)) * (kind == PermanentUpgrade.Health ? healthStep : rangeStep);
        public static bool Known(PermanentUpgrade kind) => kind == PermanentUpgrade.Health || kind == PermanentUpgrade.Range;
    }
}
