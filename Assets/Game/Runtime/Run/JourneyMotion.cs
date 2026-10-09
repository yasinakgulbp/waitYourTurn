using System;

namespace WaitYourTurn.Run
{
    /// <summary>Visual travel in train coordinates; never changes navigation or phase authority.</summary>
    public static class JourneyMotion
    {
        public static float Speed(RunFlow flow, float cruiseSpeed)
        {
            float p = flow.Progress;
            float smooth = p * p * (3 - 2 * p);
            return flow.Phase switch
            {
                RunPhase.Approach => cruiseSpeed * (1 - smooth),
                RunPhase.Departing => cruiseSpeed * smooth,
                RunPhase.FadeOut or RunPhase.Hidden or RunPhase.FadeIn or RunPhase.Cruising => cruiseSpeed,
                _ => 0
            };
        }

        public static float StationOffset(RunFlow flow, float cruiseSpeed)
        {
            float p = flow.Progress;
            // Exact integral of SmoothStep: alignment is independent of frame rate.
            float accelerationDistance = p * p * p - .5f * p * p * p * p;
            return flow.Phase switch
            {
                RunPhase.Approach => cruiseSpeed * flow.Duration * Math.Max(0, .5f - p + accelerationDistance),
                RunPhase.Departing => -cruiseSpeed * flow.Duration * accelerationDistance,
                RunPhase.Cruising => -cruiseSpeed * (flow.DepartureDuration * .5f + flow.Elapsed),
                RunPhase.FadeOut => -cruiseSpeed * (flow.DepartureDuration * .5f + flow.Elapsed),
                RunPhase.FadeIn => cruiseSpeed * (flow.NextApproachDuration * .5f + flow.Remaining),
                _ => 0
            };
        }
    }
}
