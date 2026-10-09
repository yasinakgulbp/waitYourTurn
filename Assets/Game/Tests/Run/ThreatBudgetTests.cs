using System.Linq;
using NUnit.Framework;
using UnityEngine;
using WaitYourTurn.Run;
using WaitYourTurn.Enemies;

namespace WaitYourTurn.Tests
{
    public sealed class ThreatBudgetTests
    {
        private StationDefinition program;
        private EnemyProfile profile;
        [SetUp] public void SetUp()
        {
            profile = ScriptableObject.CreateInstance<EnemyProfile>();
            program = ScriptableObject.CreateInstance<StationDefinition>(); program.firstStation = 4; program.useThreatBudget = true;
            program.bands = new[] { Band(1, 6, 4), Band(2, 2, 5), Band(4, 1, 8) };
        }
        private SpawnBand Band(int cost, int weight, int unlock) => new SpawnBand
        { profile = profile, threatCost = cost, threatWeight = weight, unlockStation = unlock, maximumCount = 120 };
        [TearDown] public void TearDown() { Object.DestroyImmediate(program); Object.DestroyImmediate(profile); }
        [Test] public void TypesUnlockGraduallyAndBudgetNeverOverspends()
        {
            Assert.That(program.BudgetCountsAt(4), Is.EqualTo(new[] { 13, 0, 0 }));
            Assert.That(program.BudgetCountsAt(5)[1], Is.GreaterThan(0)); Assert.That(program.BudgetCountsAt(7)[2], Is.Zero);
            Assert.That(program.BudgetCountsAt(8)[2], Is.GreaterThan(0));
            for (int station = 4; station <= 100; station++)
            {
                var counts = program.BudgetCountsAt(station); int spent = counts.Select((n, i) => n * program.bands[i].threatCost).Sum();
                Assert.That(spent, Is.LessThanOrEqualTo(program.ThreatBudgetAt(station)));
                Assert.That(program.ThreatBudgetAt(station), Is.LessThanOrEqualTo(160));
            }
        }
        [Test] public void ThreeWagonsShareOneBudgetAndRebuildIdenticallyForSave()
        {
            var plan = program.BuildStreams(3, 8); var again = program.BuildStreams(3, 8);
            Assert.That(plan.Select(s => s.Count).Sum(), Is.EqualTo(program.BudgetCountsAt(8).Sum()));
            Assert.That(plan.Select(s => s.Wagon).Distinct().Count(), Is.EqualTo(3));
            Assert.That(again.Select(s => (s.Wagon, s.Profile, s.Count, s.Start, s.Interval)),
                Is.EqualTo(plan.Select(s => (s.Wagon, s.Profile, s.Count, s.Start, s.Interval))));
        }
        [Test] public void InvalidCostAndMissingStartingTypeAreRejected()
        {
            program.bands[0].threatCost = 0; Assert.That(program.Validate(3, out _), Is.False);
            program.bands[0].threatCost = 1; program.bands[0].unlockStation = 9;
            Assert.That(program.Validate(3, out _), Is.False);
        }
    }
}
