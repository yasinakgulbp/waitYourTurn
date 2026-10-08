using UnityEngine;

namespace WaitYourTurn.Run
{
    [CreateAssetMenu(menuName = "Wait Your Turn/Bot Profile")]
    public sealed class BotProfile : ScriptableObject
    {
        public string skill = "Regular";
        [Min(.05f)] public float decisionInterval = .35f, aimInterval = .2f;
        [Min(0)] public float reactionDelay = .25f;
        [Min(.1f)] public float moveSpeed = 3.2f, dangerDistance = 1.45f, purchaseInterval = 2;
        [Range(0, 1)] public float repairBelow = .65f, healBelow = .5f;
        [Min(1)] public int preferredWeapon = 2;
        public bool buyTurrets;
        public Color color = new Color(.3f, .65f, 1);
        public bool Valid => !string.IsNullOrWhiteSpace(skill) && Positive(decisionInterval) && Positive(aimInterval) &&
            Positive(moveSpeed) && Positive(dangerDistance) && Positive(purchaseInterval) && Finite(reactionDelay) && reactionDelay >= 0 &&
            Finite(repairBelow) && repairBelow >= 0 && repairBelow <= 1 && Finite(healBelow) && healBelow >= 0 && healBelow <= 1 && preferredWeapon > 0;
        private static bool Positive(float x) => Finite(x) && x > 0;
        private static bool Finite(float x) => !float.IsNaN(x) && !float.IsInfinity(x);
    }
}
