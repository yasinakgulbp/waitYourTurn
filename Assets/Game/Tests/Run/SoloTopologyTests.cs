using NUnit.Framework;
using UnityEngine;
using WaitYourTurn.Enemies;
using WaitYourTurn.Player;

namespace WaitYourTurn.Run.Tests
{
    public sealed class SoloTopologyTests
    {
        [Test] public void CapsuleCanWalkAcrossRoomConnectorSeamsWithoutEscapingSides()
        {
            var root = new GameObject("test regions");
            try
            {
                var area = root.AddComponent<MovementArea>();
                area.ConfigureRegions(new[] { new Bounds(Vector3.zero, new Vector3(11.6f, 3, 4.02f)),
                    new Bounds(Vector3.right * 13, new Vector3(11.6f, 3, 4.02f)),
                    new Bounds(Vector3.right * 6.5f, new Vector3(3.4f, 2, 1.4f)) });
                for (float x = 4; x <= 9; x += .025f)
                {
                    var point = new Vector3(x, .9f, 0);
                    Assert.That((area.Constrain(point, .4f) - point).sqrMagnitude, Is.LessThan(.000001f), "Blocked seam at " + x);
                }
                var exterior = area.Constrain(new Vector3(6.5f, .9f, 3), .4f);
                Assert.That(Mathf.Abs(exterior.z), Is.LessThanOrEqualTo(1.61f));
                area.ConfigureRegions(new[] { new Bounds(Vector3.zero, new Vector3(11.6f, 3, 4.02f)) });
                Assert.That(area.Constrain(new Vector3(6.5f, .9f, 0), .4f).x, Is.EqualTo(5.4f).Within(.0001f));
            }
            finally { Object.DestroyImmediate(root); }
        }
        [Test] public void SoloHasOneMobileBudgetWhileBattleHasOnePerWagon()
        {
            var profile = ScriptableObject.CreateInstance<EnemyProfile>();
            var definition = ScriptableObject.CreateInstance<StationDefinition>();
            try
            {
                definition.bands = new[] { new SpawnBand { profile = profile, count = 8, firstAt = 0, interval = 1 } };
                Assert.That(definition.BuildStreams(5).Length, Is.EqualTo(5));
                var streams = definition.BuildSoloStreams(5);
                Assert.That(streams.Length, Is.EqualTo(1));
                var schedule = new SpawnSchedule(streams, 4, 3, .25f);
                int destination = 0; int count = 0;
                for (int time = 0; time < 15; time++)
                { destination = time % 2; schedule.Tick(time, 1, _ => { count++; return SpawnResult.Spawned; }); }
                Assert.That(count, Is.EqualTo(8)); Assert.That(schedule.Spawned, Is.EqualTo(8));
                Assert.That(schedule.SuppressedStreams, Is.Zero);
            }
            finally { Object.DestroyImmediate(profile); Object.DestroyImmediate(definition); }
        }
    }
}
