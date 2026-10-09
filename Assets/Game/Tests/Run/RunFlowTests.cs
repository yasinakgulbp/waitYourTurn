using System.Collections.Generic;
using NUnit.Framework;
using WaitYourTurn.Run;

namespace WaitYourTurn.Tests
{
    public sealed class RunFlowTests
    {
        [Test]
        public void SurvivalWaitsForBoardingAndTravelsWithoutPauseOrAssignment()
        {
            var flow = new RunFlow(new RunTimings(), true);
            flow.Tick(10); flow.Tick(1000);
            Assert.That(flow.Phase, Is.EqualTo(RunPhase.Defense));
            var restored = new RunFlow(new RunTimings(), true);
            restored.Restore(flow.Phase, flow.Station, flow.Elapsed);
            Assert.That(restored.CompleteBoardingWave(), Is.True);
            Assert.That(restored.CompleteBoardingWave(), Is.False);
            restored.Tick(3);
            Assert.That(restored.Phase, Is.EqualTo(RunPhase.Cruising));
            Assert.That(restored.Paused, Is.False);
            restored.Tick(10);
            Assert.That(restored.Phase, Is.EqualTo(RunPhase.Approach));
            Assert.That(restored.Station, Is.EqualTo(2));
            Assert.Throws<System.ArgumentException>(() => restored.Restore(RunPhase.Hidden, 1, 0));
            Assert.Throws<System.ArgumentException>(() => new RunFlow(new RunTimings()).Restore(RunPhase.Cruising, 1, 0));
        }
        [Test]
        public void LongFrameCannotSkipDeparturePauseOrAssignment()
        {
            var flow = new RunFlow(new RunTimings());
            var phases = new List<RunPhase>();
            flow.Changed += phases.Add;
            for (int i = 0; i < 7; i++) flow.Tick(1000);
            Assert.That(phases, Is.EqualTo(new[] { RunPhase.Defense, RunPhase.DepartureWarning,
                RunPhase.Departing, RunPhase.FadeOut, RunPhase.Hidden, RunPhase.FadeIn, RunPhase.Approach }));
            Assert.That(flow.Station, Is.EqualTo(2));
            Assert.That(flow.Remaining, Is.EqualTo(6));
        }

        [TestCase(0)] [TestCase(4)] [TestCase(5)] [TestCase(6)]
        public void DeathDuringApproachOrDarknessPreventsAllFutureTransitions(int boundaries)
        {
            var flow = new RunFlow(new RunTimings());
            for (int i = 0; i < boundaries; i++) flow.Tick(1000);
            int changes = 0;
            flow.Changed += _ => changes++;
            int station = flow.Station;
            flow.EndRun(); flow.Tick(1000); flow.EndRun(); flow.Fail();
            Assert.That(flow.Phase, Is.EqualTo(RunPhase.GameOver));
            Assert.That(flow.Station, Is.EqualTo(station));
            Assert.That(flow.Paused, Is.True);
            Assert.That(changes, Is.EqualTo(1));
        }

        [Test]
        public void FailedDepartureCannotProceedToAssignment()
        {
            var flow = new RunFlow(new RunTimings());
            var phases = new List<RunPhase>();
            flow.Changed += phase => { phases.Add(phase); if (phase == RunPhase.Departing) flow.Fail(); };
            for (int i = 0; i < 10; i++) flow.Tick(1000);
            Assert.That(flow.Phase, Is.EqualTo(RunPhase.Faulted));
            Assert.That(phases.Contains(RunPhase.FadeIn), Is.False);
            Assert.That(flow.Station, Is.EqualTo(1));
        }
    }
}
