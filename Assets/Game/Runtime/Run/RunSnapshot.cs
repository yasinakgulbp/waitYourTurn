using System;
using UnityEngine;
using WaitYourTurn.Combat;
using WaitYourTurn.Defenses;

namespace WaitYourTurn.Run
{
    // Data only. No scene references, instance IDs, delegates or wall-clock attack deadlines on disk.
    [Serializable] public sealed class BodySave
    {
        public float current, maximum, protection;
        public Vector3 position;
        public Quaternion rotation;
    }
    [Serializable] public sealed class ParticipantSave
    {
        public string id;
        public int wagon, coins, repairDoor;
        public float purchaseRemaining, repairProgress;
        public BodySave body;
        public WeaponSnapshot weapon;
    }
    [Serializable] public sealed class DoorSave
    {
        public float current, maximum;
        public bool open, closing;
    }
    [Serializable] public sealed class EnemySave
    {
        public string profile;
        public int station, door;
        public bool onBoard;
        public float attackRemaining;
        public BodySave body;
    }
    [Serializable] public sealed class TurretSave
    {
        public int slot, type, owner;
        public Vector3 position;
        public Quaternion rotation;
        public WeaponSnapshot weapon;
    }
    [Serializable] public sealed class WagonSave
    {
        public string id;
        public int spawnSequence;
        public DoorSave[] doors;
        public EnemySave[] enemies;
        public TurretSave[] turrets;
    }
    [Serializable] public sealed class DroneSave
    {
        public DroneState state;
        public Vector3 position;
        public Quaternion rotation;
        public float searchRemaining;
        public WeaponSnapshot weapon;
    }
    [Serializable] public sealed class RunSnapshot
    {
        public const int Version = 1;
        public int version = Version;
        public string content, runId;
        public RunMode mode;
        public int seed, station, assignments;
        public RunPhase phase;
        public float elapsed;
        public uint humanRandom, botRandom;
        public int[] ranks;
        public RunTimings timings;
        public ParticipantSave[] participants;
        public WagonSave[] wagons;
        public SpawnerSnapshot spawner;
        // Unity serializes inline null classes as empty objects; presence must be explicit.
        public bool hasDrone;
        public DroneSave drone;
    }
}
