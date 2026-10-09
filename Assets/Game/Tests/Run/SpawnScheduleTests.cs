using System;
using NUnit.Framework;
using UnityEngine;
using WaitYourTurn.Enemies;
using WaitYourTurn.Run;

namespace WaitYourTurn.Tests
{
    public sealed class SpawnScheduleTests
    {
        [Test]
        public void CompletionWaitsForFutureRequestsAndCapacityRetriesAndSurvivesRestore()
        {
            var streams = new[] { new SpawnStream(0, 0, 2, 0, 10) };
            var schedule = new SpawnSchedule(streams, 1, 3, .1f);
            schedule.Tick(0, 1, _ => SpawnResult.Spawned);
            Assert.That(schedule.Pending, Is.Zero);
            Assert.That(schedule.Completed, Is.False);
            schedule.Tick(10, 1, _ => SpawnResult.CapacityFull);
            Assert.That(schedule.Completed, Is.False);
            schedule.Tick(11, 1, _ => SpawnResult.Spawned);
            Assert.That(schedule.Completed, Is.True);
            var restored = new SpawnSchedule(streams, 1, 3, .1f);
            Assert.That(restored.Restore(schedule.Capture()), Is.True);
            Assert.That(restored.Completed, Is.True);
            restored.Clear(); Assert.That(restored.Completed, Is.False);
        }
        [Test]
        public void EmptyWagonCancelsItsStationStreamWithoutBacklogOrStarvingOccupiedWagon()
        {
            var streams = new[] { new SpawnStream(0, 0, 100, 0, .01f), new SpawnStream(1, 0, 3, 0, .1f) };
            var schedule = new SpawnSchedule(streams, 1, 3, .01f);
            int emptyCalls = 0, occupied = 0;
            for (int frame = 0; frame < 20; frame++)
                schedule.Tick(frame, 1, r => { if (r.Wagon == 0) { emptyCalls++; return SpawnResult.Unoccupied; }
                    occupied++; return SpawnResult.Spawned; });
            Assert.That(emptyCalls, Is.EqualTo(1)); Assert.That(occupied, Is.EqualTo(3));
            Assert.That(schedule.SuppressedStreams, Is.EqualTo(1)); Assert.That(schedule.Dropped, Is.Zero);
            schedule.Tick(100, 8, _ => throw new Exception("Empty station stream must stay cancelled"));
            var next = new SpawnSchedule(streams, 1, 3, .01f);
            next.Tick(0, 1, _ => SpawnResult.Spawned); Assert.That(next.Spawned, Is.EqualTo(1));
        }
        [Test]
        public void DefenderEliminationRejectsAlreadyQueuedRequests()
        {
            var schedule = new SpawnSchedule(new[] { new SpawnStream(0, 0, 5, 0, .1f) }, 1, 3, .1f);
            schedule.Tick(0, 1, _ => SpawnResult.Spawned);
            schedule.Tick(.2f, 1, _ => SpawnResult.CapacityFull);
            schedule.Tick(.4f, 1, _ => SpawnResult.Unoccupied);
            schedule.Tick(5, 1, _ => throw new Exception("Eliminated defender cannot receive new enemies"));
            Assert.That(schedule.Spawned, Is.EqualTo(1)); Assert.That(schedule.SuppressedStreams, Is.EqualTo(1));
        }
        [Test]
        public void HugeTimeJumpStillHonorsPerFrameBudgetAndQueueBound()
        {
            var streams = new SpawnStream[10];
            for (int i = 0; i < streams.Length; i++) streams[i] = new SpawnStream(i, 0, 1000, 0, .01f);
            var schedule = new SpawnSchedule(streams, 3, 3, .25f);
            int calls = 0;
            for (int frame = 0; frame < 100; frame++)
            {
                int previous = calls;
                schedule.Tick(10000, 2, _ => { calls++; return SpawnResult.CapacityFull; });
                Assert.That(calls - previous, Is.LessThanOrEqualTo(2)); Assert.That(schedule.Pending, Is.LessThanOrEqualTo(3));
            }
            Assert.That(schedule.Dropped, Is.Zero); // Capacity is never an invalid navigation attempt.
        }
        [Test]
        public void FullWagonCannotStarveAnotherWagon()
        {
            var schedule = new SpawnSchedule(new[] { new SpawnStream(0, 0, 20, 0, .01f), new SpawnStream(1, 0, 20, 0, .01f) }, 2, 3, .01f);
            int other = 0;
            for (int frame = 0; frame < 100; frame++)
                schedule.Tick(frame * .1f, 1, r => { if (r.Wagon == 0) return SpawnResult.CapacityFull; other++; return SpawnResult.Spawned; });
            Assert.That(other, Is.EqualTo(20)); Assert.That(schedule.Pending, Is.LessThanOrEqualTo(2));
        }
        [Test]
        public void BadPositionRetriesAreDelayedAndFinite()
        {
            var schedule = new SpawnSchedule(new[] { new SpawnStream(0, 0, 1, 0, 1) }, 8, 3, .25f);
            int calls = 0;
            Func<SpawnRequest, SpawnResult> fail = _ => { calls++; return SpawnResult.InvalidPosition; };
            schedule.Tick(0, 8, fail); schedule.Tick(.1f, 8, fail);
            Assert.That(calls, Is.EqualTo(1));
            schedule.Tick(.25f, 8, fail); schedule.Tick(.5f, 8, fail); schedule.Tick(100, 8, fail);
            Assert.That(calls, Is.EqualTo(3)); Assert.That(schedule.Dropped, Is.EqualTo(1)); Assert.That(schedule.Pending, Is.Zero);
        }
        [Test]
        public void StartTimeAndFiniteStreamCountArePreserved()
        {
            var schedule = new SpawnSchedule(new[] { new SpawnStream(2, 4, 2, 3, 2) }, 8, 3, .1f);
            int calls = 0;
            Func<SpawnRequest, SpawnResult> spawn = r => { Assert.That(r.Wagon, Is.EqualTo(2)); Assert.That(r.Profile, Is.EqualTo(4)); calls++; return SpawnResult.Spawned; };
            schedule.Tick(2.9f, 8, spawn); Assert.That(calls, Is.Zero);
            schedule.Tick(3, 8, spawn); schedule.Tick(4.9f, 8, spawn); Assert.That(calls, Is.EqualTo(1));
            schedule.Tick(5, 8, spawn); schedule.Tick(100, 8, spawn); Assert.That(calls, Is.EqualTo(2));
        }
        [Test]
        public void StationEndDropsOnlyPendingRequestsAndNewScheduleStartsFresh()
        {
            var streams = new[] { new SpawnStream(0, 0, 2, 0, .1f) };
            var old = new SpawnSchedule(streams, 8, 3, .1f); old.Tick(0, 1, _ => SpawnResult.CapacityFull); old.Clear();
            var fresh = new SpawnSchedule(streams, 8, 3, .1f);
            Assert.That(old.Pending, Is.Zero); Assert.That(fresh.Attempts, Is.Zero);
            fresh.Tick(0, 1, _ => SpawnResult.Spawned); Assert.That(fresh.Spawned, Is.EqualTo(1));
        }
        [Test]
        public void GlobalLimitCountsCarriedBodiesAndReusesReleasedCapacity()
        {
            int alive = 4; // Four survivors before new spawning starts.
            var schedule = new SpawnSchedule(new[] { new SpawnStream(0, 0, 20, 0, .01f) }, 8, 3, .01f);
            Func<SpawnRequest, SpawnResult> spawn = _ => { if (alive >= 5) return SpawnResult.CapacityFull; alive++; return SpawnResult.Spawned; };
            for (int i = 0; i < 20; i++) { schedule.Tick(i, 2, spawn); Assert.That(alive, Is.LessThanOrEqualTo(5)); }
            Assert.That(schedule.Spawned, Is.EqualTo(1)); alive--;
            schedule.Tick(21, 2, spawn); Assert.That(alive, Is.EqualTo(5)); Assert.That(schedule.Spawned, Is.EqualTo(2));
        }
        [TestCase(float.NaN)] [TestCase(float.PositiveInfinity)] [TestCase(-1)]
        public void InvalidClockNeverSpawns(float time)
        {
            var schedule = new SpawnSchedule(new[] { new SpawnStream(0, 0, 1, 0, 1) }, 1, 1, .1f);
            schedule.Tick(time, 1, _ => throw new Exception("Must not run")); Assert.That(schedule.Attempts, Is.Zero);
        }
        [Test]
        public void DefinitionsExpandActualWagonCountAndExplicitTarget()
        {
            var profile = ScriptableObject.CreateInstance<EnemyProfile>(); var definition = ScriptableObject.CreateInstance<StationDefinition>();
            try
            {
                definition.bands = new[] { new SpawnBand { profile = profile }, new SpawnBand { profile = profile, wagon = 2 } };
                Assert.That(definition.Validate(5, out _), Is.True);
                var streams = definition.BuildStreams(5); Assert.That(streams.Length, Is.EqualTo(6));
                int targeted = 0; foreach (var stream in streams) if (stream.Profile == 1) { targeted++; Assert.That(stream.Wagon, Is.EqualTo(2)); }
                Assert.That(targeted, Is.EqualTo(1));
                Assert.That(definition.Validate(2, out string reason), Is.False); Assert.That(reason, Does.Contain("Band 1"));
                definition.bands[1].wagon = -1; profile.speed = float.NaN; Assert.That(definition.Validate(5, out _), Is.False);
            }
            finally { UnityEngine.Object.DestroyImmediate(profile); UnityEngine.Object.DestroyImmediate(definition); }
        }
        [Test]
        public void MalformedLimitsCannotAllocateUnboundedQueues()
        {
            var definition = ScriptableObject.CreateInstance<StationDefinition>();
            try { definition.queueCapacity = int.MaxValue; Assert.That(definition.Validate(5, out string reason), Is.False); Assert.That(reason, Does.Contain("limits")); }
            finally { UnityEngine.Object.DestroyImmediate(definition); }
            Assert.Throws<ArgumentException>(() => new SpawnSchedule(new[] { new SpawnStream(0, 0, 1, 0, float.NaN) }, 8, 3, .1f));
        }
    }
}
