using UnityEngine;

namespace WaitYourTurn.Defenses
{
    [CreateAssetMenu(menuName = "Wait Your Turn/Mine")]
    public sealed class MineDefinition : ScriptableObject
    {
        [Min(.01f)] public float damage = 80;
        [Range(.5f, 5)] public float blastRadius = 2;
        [Range(.2f, 1)] public float triggerRadius = .65f;
        [Range(0, 5)] public float armingSeconds = .6f;
        [Range(.05f, .5f)] public float scanInterval = .1f;
        public bool Valid => damage > 0 && damage <= 100000 && blastRadius >= .5f && blastRadius <= 5 &&
            triggerRadius >= .2f && triggerRadius <= 1 && armingSeconds >= 0 && armingSeconds <= 5 &&
            scanInterval >= .05f && scanInterval <= .5f;
        public string DefinitionKey => System.FormattableString.Invariant($"/mine-v1/{damage}/{blastRadius}/{triggerRadius}/{armingSeconds}/{scanInterval}");
    }
}
