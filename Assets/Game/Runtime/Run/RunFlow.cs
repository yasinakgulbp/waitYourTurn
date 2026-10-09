using System;

namespace WaitYourTurn.Run
{
    public enum RunPhase { Approach, Defense, DepartureWarning, Departing, FadeOut, Hidden, FadeIn, GameOver, Faulted, Cruising }

    [Serializable]
    public sealed class RunTimings
    {
        public float initialApproach = 10, nextApproach = 6, defense = 60, departureWarning = 3;
        public float departure = 3, fadeOut = 1.5f, hidden = 10, fadeIn = 1.5f;
        public bool Valid => Positive(initialApproach) && Positive(nextApproach) && Positive(defense) &&
            Positive(departureWarning) && Positive(departure) && Positive(fadeOut) && Positive(hidden) && Positive(fadeIn);
        private static bool Positive(float x) => x > 0 && !float.IsNaN(x) && !float.IsInfinity(x);
    }

    /// <summary>Pure phase/timer authority. No scene, enemies, physics, UI or Unity clock dependency.</summary>
    public sealed class RunFlow
    {
        private readonly RunTimings timings;
        public bool BoardingWaves { get; }
        public float CruiseDuration { get; }
        public RunPhase Phase { get; private set; }
        public int Station { get; private set; } = 1;
        public float Elapsed { get; private set; }
        public bool Paused => Phase == RunPhase.FadeOut || Phase == RunPhase.Hidden || Phase == RunPhase.FadeIn ||
            Phase == RunPhase.GameOver || Phase == RunPhase.Faulted;
        public bool Terminal => Phase == RunPhase.GameOver || Phase == RunPhase.Faulted;
        public float Duration => Phase switch
        {
            RunPhase.Approach => Station == 1 ? timings.initialApproach : timings.nextApproach,
            RunPhase.Defense => timings.defense,
            RunPhase.DepartureWarning => timings.departureWarning,
            RunPhase.Departing => timings.departure,
            RunPhase.FadeOut => timings.fadeOut,
            RunPhase.Hidden => timings.hidden,
            RunPhase.FadeIn => timings.fadeIn,
            RunPhase.Cruising => CruiseDuration,
            _ => 0
        };
        public float Remaining => Math.Max(0, Duration - Elapsed);
        public float NextApproachDuration => timings.nextApproach;
        public float DepartureDuration => timings.departure;
        public float Progress => Duration > 0 ? Math.Min(1, Elapsed / Duration) : 0;
        public event Action<RunPhase> Changed;
        public RunFlow(RunTimings configuration, bool boardingWaves = false, float cruiseSeconds = 10)
        {
            if (configuration == null || !configuration.Valid) throw new ArgumentException("Run timings must be finite and positive.");
            if (!(cruiseSeconds > 0) || float.IsInfinity(cruiseSeconds)) throw new ArgumentException("Invalid cruise duration.");
            BoardingWaves = boardingWaves; CruiseDuration = cruiseSeconds;
            timings = configuration; Phase = RunPhase.Approach;
        }
        public void Tick(float realSeconds)
        {
            if (Terminal || realSeconds <= 0 || float.IsNaN(realSeconds) || float.IsInfinity(realSeconds)) return;
            Elapsed += realSeconds;
            if (BoardingWaves && Phase == RunPhase.Defense) return;
            if (Elapsed < Duration) return;
            // One boundary per tick: a slow frame cannot skip setup, pause or assignment callbacks.
            RunPhase next = Phase switch
            {
                RunPhase.Approach => RunPhase.Defense,
                RunPhase.Defense => RunPhase.DepartureWarning,
                RunPhase.DepartureWarning => RunPhase.Departing,
                RunPhase.Departing => BoardingWaves ? RunPhase.Cruising : RunPhase.FadeOut,
                RunPhase.FadeOut => RunPhase.Hidden,
                RunPhase.Hidden => RunPhase.FadeIn,
                _ => RunPhase.Approach
            };
            if (next == RunPhase.Approach) Station++;
            Set(next);
        }
        public void EndRun() => Set(RunPhase.GameOver);
        // The scene adapter resolves the wave and boarding. The clock cannot end this defense.
        public bool CompleteBoardingWave()
        {
            if (!BoardingWaves || Phase != RunPhase.Defense) return false;
            Set(RunPhase.Departing); return true;
        }
        public void Restore(RunPhase phase, int station, float elapsed)
        {
            bool validPhase = BoardingWaves ? phase == RunPhase.Approach || phase == RunPhase.Defense ||
                phase == RunPhase.Departing || phase == RunPhase.Cruising : phase >= RunPhase.Approach && phase <= RunPhase.FadeIn;
            if (!validPhase || station < 1 || station > 100000 ||
                float.IsNaN(elapsed) || float.IsInfinity(elapsed) || elapsed < 0) throw new ArgumentException("Invalid saved run flow.");
            Phase = phase; Station = station;
            if (!(BoardingWaves && phase == RunPhase.Defense) && elapsed > Duration) throw new ArgumentException("Saved phase timer exceeds duration.");
            Elapsed = elapsed;
        }
        public void Fail() => Set(RunPhase.Faulted);
        private void Set(RunPhase phase) { if (Terminal) return; Phase = phase; Elapsed = 0; Changed?.Invoke(phase); }
    }
}
