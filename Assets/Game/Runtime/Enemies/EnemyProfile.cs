using UnityEngine;

namespace WaitYourTurn.Enemies
{
    [CreateAssetMenu(menuName = "Wait Your Turn/Enemy Profile")]
    public sealed class EnemyProfile : ScriptableObject
    {
        public string id = "normal";
        [Min(.01f)] public float health = 24;
        [Min(.1f)] public float speed = 2.6f;
        [Min(.01f)] public float damage = 2;
        [Min(.1f)] public float attackInterval = 1;
        [Min(0)] public float windup = .25f;
        [Min(.1f)] public float range = 1.6f;
        public Color blockoutTint = new Color(.68f, .39f, .27f);
        public bool Valid => !string.IsNullOrWhiteSpace(id) && Positive(health) && Positive(speed) &&
            Positive(damage) && Positive(attackInterval) && Positive(range) &&
            !float.IsNaN(windup) && !float.IsInfinity(windup) && windup >= 0 && windup <= attackInterval;
        private static bool Positive(float value) => value > 0 && !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
