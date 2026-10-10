using UnityEngine;

namespace WaitYourTurn.Enemies
{
    /// <summary>In-place paper hinge animation. Never moves the enemy root or changes combat.</summary>
    [DisallowMultipleComponent, DefaultExecutionOrder(500)]
    public sealed class CardZombieVisual : MonoBehaviour
    {
        [SerializeField] private EnemyBrain brain;
        [SerializeField] private MeleeAttack melee;
        [SerializeField] private SkinnedMeshRenderer skin;
        [SerializeField] private Transform[] bones;
        [SerializeField] private Mesh normal, fast, tough;
        private Vector3 lastPosition;
        private Vector3 hipRest;
        private float phase, walking, strike;
        private float idlePhase;
        private uint life;
        private EnemyProfile observedProfile;
        public SkinnedMeshRenderer Skin => skin;
        public float WalkingBlend => walking;
        public void Configure(EnemyBrain owner, SkinnedMeshRenderer renderer, Transform[] skeleton,
            Mesh basic, Mesh runner, Mesh heavy)
        {
            brain = owner; melee = owner.GetComponent<MeleeAttack>(); skin = renderer;
            bones = skeleton; normal = basic; fast = runner; tough = heavy;
            ResetPose();
        }
        private void OnEnable() => ResetPose();
        private void ResetPose()
        {
            if (brain == null || skin == null || bones == null || bones.Length != 15) return;
            lastPosition = brain.transform.position; walking = strike = phase = 0;
            life = brain.Health.LifeVersion; observedProfile = null;
            idlePhase = brain.transform.GetEntityId().GetHashCode() % 17;
            hipRest = new Vector3(0, .8f, 0);
            for (int i = 0; i < bones.Length; i++) bones[i].localRotation = Quaternion.identity;
            bones[0].localPosition = hipRest;
            SelectMesh();
        }
        private void SelectMesh()
        {
            observedProfile = brain.Profile;
            string id = observedProfile != null ? observedProfile.id : "normal";
            bool runner = id == "fast", heavy = id == "tough";
            skin.sharedMesh = runner ? fast : heavy ? tough : normal;
            transform.localScale = Vector3.one * (runner ? .84f : heavy ? 1.13f : 1f);
        }
        private void LateUpdate()
        {
            if (brain == null || skin == null || bones.Length != 15) return;
            if (life != brain.Health.LifeVersion) ResetPose();
            if (observedProfile != brain.Profile) SelectMesh();
            Vector3 position = brain.transform.position;
            Vector3 delta = position - lastPosition; delta.y = 0; lastPosition = position;
            float dt = Time.deltaTime;
            if (dt <= 0 || brain.Paused || !brain.Health.IsAlive) return;
            float distance = delta.magnitude;
            // Warps/pool reuse do not create running or huge strides.
            if (distance > Mathf.Max(.5f, dt * 8)) { walking = strike = 0; distance = 0; }
            float speed = distance / dt;
            walking = Mathf.MoveTowards(walking, Mathf.Clamp01(speed / .65f), dt * 8);
            phase = Mathf.Repeat(phase + distance * (2 * Mathf.PI / 1.25f), 2 * Mathf.PI);
            strike = Mathf.MoveTowards(strike, melee != null && melee.WindingUp ? 1 : 0, dt * 10);
            // Hidden actors retain phase from real movement but need no pose evaluation.
            if (!skin.isVisible) return;
            float s = Mathf.Sin(phase), breath = Mathf.Sin(Time.time * 2 + idlePhase);
            bones[0].localPosition = hipRest + Vector3.up * (Mathf.Abs(s) * .025f * walking);
            Turn(0, 0, 0, s * 2 * walking);
            Turn(1, 5 + breath * 1.3f + strike * 7, s * 3 * walking, 0);
            Turn(2, -3, -s * 2 * walking, breath * 1.2f);
            Arm(3, -s); Arm(6, s);
            Leg(9, s); Leg(12, -s);
        }
        private void Arm(int i, float stride)
        {
            Turn(i, -20 + stride * 14 * walking - strike * 58, 0, i == 3 ? -4 : 4);
            Turn(i + 1, -8 - strike * 18, 0, 0);
            Turn(i + 2, strike * 12, 0, 0);
        }
        private void Leg(int i, float stride)
        {
            Turn(i, stride * 24 * walking, 0, 0);
            Turn(i + 1, Mathf.Max(0, -stride) * 32 * walking, 0, 0);
            Turn(i + 2, -stride * 10 * walking, 0, 0);
        }
        private void Turn(int i, float x, float y, float z) => bones[i].localRotation = Quaternion.Euler(x, y, z);
    }
}
