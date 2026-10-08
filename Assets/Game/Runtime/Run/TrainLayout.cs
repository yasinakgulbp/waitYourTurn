using System;
using UnityEngine;

namespace WaitYourTurn.Run
{
    /// <summary>Authoring data only. Live health, enemies and station time belong to runtime objects.</summary>
    [CreateAssetMenu(menuName = "Wait Your Turn/Train Layout")]
    public sealed class TrainLayout : ScriptableObject
    {
        public WagonPlacement[] wagons;
    }

    [Serializable]
    public sealed class WagonPlacement
    {
        public string id;
        public Vector3 position;
        [Min(.01f)] public float doorHealth = 10;
    }
}
