using System;
using System.Linq;
using NUnit.Framework;
using WaitYourTurn.Run;

namespace WaitYourTurn.Tests
{
    public sealed class BattleRosterTests
    {
        [Test] public void EliminationRanksAndWinnerFollowLivingParticipants()
        {
            var roster = new BattleRoster(5);
            roster.Eliminate(new[] { false, true, false, false, false });
            Assert.AreEqual(5, roster.Place(1)); Assert.IsFalse(roster.Finished);
            roster.Eliminate(new[] { false, true, true, true, false });
            Assert.AreEqual(3, roster.Place(2)); Assert.AreEqual(3, roster.Place(3)); Assert.AreEqual(2, roster.Living);
            roster.Eliminate(new[] { false, true, true, true, true });
            Assert.AreEqual(2, roster.Place(4)); Assert.AreEqual(1, roster.Place(0)); Assert.IsTrue(roster.HumanWon);
            roster.Eliminate(new[] { false, true, true, true, true }); Assert.AreEqual(1, roster.Living);
        }
        [Test] public void HumanDeathEndsTheirMatchWithoutInventingOtherRanks()
        {
            var roster = new BattleRoster(5); roster.Eliminate(new[] { true, false, false, false, false });
            Assert.AreEqual(5, roster.Place(0)); Assert.IsTrue(roster.Finished); Assert.IsFalse(roster.HumanWon);
            Assert.AreEqual(0, roster.Place(1));
        }
        [Test] public void SameFrameLastDeathsShareFirstPlaceWithoutAFalseWinner()
        {
            var roster = new BattleRoster(2); roster.Eliminate(new[] { true, true });
            Assert.AreEqual(0, roster.Living); Assert.AreEqual(1, roster.Place(0)); Assert.AreEqual(1, roster.Place(1)); Assert.IsFalse(roster.HumanWon);
        }
        [Test] public void AssignmentsAreUniqueIncludeEmptyWagonsAndNeverRespawnEliminatedBots()
        {
            var random = new Random(7); var living = new[] { true, true, false, true, false }; var seen = new bool[5];
            for (int i = 0; i < 100; i++)
            {
                var plan = BattleRoster.Assign(5, random.Next(5), living, random);
                Assert.AreEqual(-1, plan[2]); Assert.AreEqual(-1, plan[4]);
                Assert.AreEqual(3, plan.Where(x => x >= 0).Distinct().Count()); seen[plan[0]] = true;
            }
            Assert.IsTrue(seen.All(x => x));
        }
        [Test] public void SeededAssignmentIsRepeatableAndRejectsOverCapacity()
        {
            var living = new[] { true, true, true, true, true };
            CollectionAssert.AreEqual(BattleRoster.Assign(5, 2, living, new Random(12)), BattleRoster.Assign(5, 2, living, new Random(12)));
            Assert.Throws<ArgumentException>(() => BattleRoster.Assign(4, 2, living, new Random(12)));
            Assert.Throws<ArgumentOutOfRangeException>(() => new BattleRoster(1));
        }
    }
}
