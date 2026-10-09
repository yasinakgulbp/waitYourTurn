using System;
using System.IO;
using NUnit.Framework;

namespace WaitYourTurn.Run.Tests
{
    public sealed class PersistenceTests
    {
        [Test] public void RandomContinuesFromExplicitState()
        {
            var original = new RunRandom(781); for (int i = 0; i < 37; i++) original.Next(5);
            var resumed = new RunRandom(1) { State = original.State };
            for (int i = 0; i < 500; i++) Assert.That(resumed.Next(5), Is.EqualTo(original.Next(5)));
        }
        [Test] public void FlowRestoresEveryNonterminalPhaseWithoutCallbacks()
        {
            var config = new RunTimings();
            for (int p = 0; p <= (int)RunPhase.FadeIn; p++)
            {
                var flow = new RunFlow(config); int calls = 0; flow.Changed += _ => calls++;
                flow.Restore((RunPhase)p, 4, .25f);
                Assert.That(flow.Station, Is.EqualTo(4)); Assert.That(flow.Elapsed, Is.EqualTo(.25f)); Assert.That(calls, Is.Zero);
                flow.Tick(flow.Remaining); Assert.That(calls, Is.EqualTo(1));
            }
            Assert.Throws<ArgumentException>(() => new RunFlow(config).Restore(RunPhase.GameOver, 3, 0));
            Assert.Throws<ArgumentException>(() => new RunFlow(config).Restore(RunPhase.Defense, 3, float.NaN));
        }
        [Test] public void RosterRestorePreservesDeadRanksAndRejectsFinishedRuns()
        {
            var roster = new BattleRoster(5); roster.Eliminate(new[] { false, true, false, true, false });
            var resumed = new BattleRoster(5); resumed.Restore(roster.CapturePlaces());
            Assert.That(resumed.Living, Is.EqualTo(3)); Assert.That(resumed.Place(1), Is.EqualTo(4)); Assert.That(resumed.IsAlive(3), Is.False);
            Assert.Throws<ArgumentException>(() => resumed.Restore(new[] { 0, 1, 0, 0, 0 }));
            Assert.Throws<ArgumentException>(() => resumed.Restore(new[] { 0, 2, 3, 4, 5 }));
        }
        [Test] public void SpawnRestoreKeepsProgressAndBoundedPendingWork()
        {
            var streams = new[] { new SpawnStream(0, 0, 10, 0, 1), new SpawnStream(1, 0, 10, 0, 1) };
            var first = new SpawnSchedule(streams, 4, 3, .25f); first.Tick(2, 1, _ => SpawnResult.Spawned);
            first.Tick(3, 1, _ => SpawnResult.CapacityFull); var saved = first.Capture();
            var resumed = new SpawnSchedule(streams, 4, 3, .25f); Assert.That(resumed.Restore(saved), Is.True);
            for (int i = 4; i < 12; i++)
            {
                first.Tick(i, 1, _ => SpawnResult.Spawned); resumed.Tick(i, 1, _ => SpawnResult.Spawned);
                Assert.That(resumed.Spawned, Is.EqualTo(first.Spawned)); Assert.That(resumed.Pending, Is.EqualTo(first.Pending));
            }
            saved.emitted[0] = -1; Assert.That(resumed.Restore(saved), Is.False);
        }
        [Test] public void StoreChecksumBackupAndTerminalTombstone()
        {
            string folder = Path.Combine(Path.GetTempPath(), "wyt-save-test-" + Guid.NewGuid().ToString("N"));
            string path = Path.Combine(folder, "run.json"); var store = new LocalRunStore(path);
            try
            {
                Assert.That(store.Save(new RunSnapshot { station = 2, runId = "first" }), Is.True, store.Diagnostic);
                Assert.That(store.Save(new RunSnapshot { station = 3, runId = "second" }), Is.True, store.Diagnostic);
                Assert.That(store.Load().station, Is.EqualTo(3)); File.WriteAllText(path, "truncated");
                Assert.That(store.Load().station, Is.EqualTo(2)); Assert.That(store.Invalidate(), Is.True);
                Assert.That(store.Load(), Is.Null); File.WriteAllText(path, "truncated"); Assert.That(store.Load(), Is.Null);
                File.WriteAllText(path, new string('x', LocalRunStore.MaximumBytes + 1)); Assert.That(store.Load(), Is.Null);
            }
            finally { foreach (var file in new[] { path, path + ".bak", path + ".tmp", path + ".ended", path + ".ended.tmp" }) if (File.Exists(file)) File.Delete(file); if (Directory.Exists(folder)) Directory.Delete(folder); }
        }
    }
}
