using NUnit.Framework;
using WaitYourTurn.Run;

namespace WaitYourTurn.Tests
{
    public sealed class JourneyMotionTests
    {
        [Test] public void ApproachReachesExactAlignmentAtZeroSpeed()
        {
            var flow = new RunFlow(new RunTimings());
            Assert.That(JourneyMotion.StationOffset(flow, 5), Is.EqualTo(25));
            Assert.That(JourneyMotion.Speed(flow, 5), Is.EqualTo(5));
            flow.Tick(5);
            Assert.That(JourneyMotion.StationOffset(flow, 5), Is.EqualTo(4.6875f).Within(.0001f));
            Assert.That(JourneyMotion.Speed(flow, 5), Is.EqualTo(2.5f));
            flow.Tick(5);
            Assert.That(flow.Phase, Is.EqualTo(RunPhase.Defense));
            Assert.That(JourneyMotion.StationOffset(flow, 5), Is.Zero);
            Assert.That(JourneyMotion.Speed(flow, 5), Is.Zero);
        }
        [Test] public void DepartureAndFadeOutMeetWithoutRepeatingStation()
        {
            var flow = new RunFlow(new RunTimings());
            for (int i = 0; i < 3; i++) flow.Tick(1000);
            Assert.That(JourneyMotion.StationOffset(flow, 5), Is.Zero);
            flow.Tick(1.5f);
            Assert.That(JourneyMotion.StationOffset(flow, 5), Is.EqualTo(-1.40625f).Within(.0001f));
            flow.Tick(1.5f);
            Assert.That(flow.Phase, Is.EqualTo(RunPhase.FadeOut));
            Assert.That(JourneyMotion.StationOffset(flow, 5), Is.EqualTo(-7.5f));
            flow.Tick(.5f);
            Assert.That(JourneyMotion.StationOffset(flow, 5), Is.EqualTo(-10));
        }
        [TestCase(6f)] [TestCase(2f)]
        public void NextStationFadeInMeetsConfiguredApproach(float duration)
        {
            var flow = new RunFlow(new RunTimings { nextApproach = duration });
            for (int i = 0; i < 6; i++) flow.Tick(1000);
            Assert.That(flow.Phase, Is.EqualTo(RunPhase.FadeIn));
            Assert.That(JourneyMotion.StationOffset(flow, 5), Is.EqualTo(5 * (duration * .5f + 1.5f)));
            flow.Tick(1.4999f);
            float before = JourneyMotion.StationOffset(flow, 5);
            flow.Tick(.001f);
            Assert.That(JourneyMotion.StationOffset(flow, 5), Is.EqualTo(before).Within(.001f));
            Assert.That(JourneyMotion.Speed(flow, 5), Is.EqualTo(5));
        }
        [Test] public void SamplingDoesNotAdvanceOrDependOnFrameHistory()
        {
            var single = new RunFlow(new RunTimings());
            var many = new RunFlow(new RunTimings());
            single.Tick(4); for (int i = 0; i < 16; i++) many.Tick(.25f);
            Assert.That(JourneyMotion.StationOffset(single, 5), Is.EqualTo(JourneyMotion.StationOffset(many, 5)));
            float elapsed = single.Elapsed; for (int i = 0; i < 100; i++) JourneyMotion.StationOffset(single, 5);
            Assert.That(single.Elapsed, Is.EqualTo(elapsed));
            single.EndRun(); Assert.That(JourneyMotion.Speed(single, 5), Is.Zero);
        }
    }
}
