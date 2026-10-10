using UnityEngine;
using WaitYourTurn.Combat;

namespace WaitYourTurn.Enemies
{
    /// <summary>Distance-driven gait and contact poses. Never translates the navigation root.</summary>
    [DisallowMultipleComponent, DefaultExecutionOrder(500)]
    public sealed class CardZombieVisual : MonoBehaviour
    {
        [SerializeField] private EnemyBrain brain;
        [SerializeField] private MeleeAttack melee;
        [SerializeField] private SkinnedMeshRenderer skin;
        [SerializeField] private Transform[] bones;
        [SerializeField] private Mesh normal, fast, tough;
        private Vector3 lastPosition;
        private readonly Vector3 hipRest = new Vector3(0, .8f, 0);
        private float phase, walking, idlePhase, hitAt = -10;
        private uint life;
        private ulong damageRevision;
        private EnemyProfile observedProfile;
        private HealthComponent contactTarget;
        private Collider contactCollider;
        private bool runner, heavy;
        public SkinnedMeshRenderer Skin => skin;
        public float WalkingBlend => walking;
        public float HitBlend { get; private set; }
        public Vector3 LastContactPoint { get; private set; }
        public float ContactBlend { get; private set; }
        public void Configure(EnemyBrain owner, SkinnedMeshRenderer renderer, Transform[] skeleton,
            Mesh basic, Mesh sprinter, Mesh brute)
        {
            brain = owner; melee = owner.GetComponent<MeleeAttack>(); skin = renderer;
            bones = skeleton; normal = basic; fast = sprinter; tough = brute; ResetPose();
        }
        private void OnEnable() => ResetPose();
        private void ResetPose()
        {
            if (brain == null || skin == null || bones == null || bones.Length != 15) return;
            lastPosition = brain.transform.position; walking = phase = 0;
            hitAt = -10; HitBlend = ContactBlend = 0; contactTarget = null; contactCollider = null;
            life = brain.Health.LifeVersion; damageRevision = brain.Health.DamageRevision; observedProfile = null;
            idlePhase = Mathf.Abs(brain.GetEntityId().GetHashCode() % 17);
            for (int i = 0; i < bones.Length; i++) bones[i].localRotation = Quaternion.identity;
            bones[0].localPosition = hipRest; SelectMesh();
        }
        private void SelectMesh()
        {
            observedProfile = brain.Profile;
            string id = observedProfile != null ? observedProfile.id : "normal";
            runner = id == "fast"; heavy = id == "tough";
            skin.sharedMesh = runner ? fast : heavy ? tough : normal;
            transform.localScale = Vector3.one * (runner ? .84f : heavy ? 1.13f : 1f);
            melee.SetActorReach(runner ? .82f : heavy ? 1.02f : .92f);
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
            if (damageRevision != brain.Health.DamageRevision)
            { damageRevision = brain.Health.DamageRevision; hitAt = Time.time; }
            float distance = delta.magnitude;
            if (distance > Mathf.Max(.5f, dt * 8)) { walking = 0; distance = 0; }
            walking = Mathf.MoveTowards(walking, Mathf.Clamp01(distance / dt / .65f), dt * 7);
            phase = Mathf.Repeat(phase + distance * (2 * Mathf.PI / (runner ? .92f : heavy ? 1.4f : 1.22f)), 2 * Mathf.PI);
            // No hidden-bone work; phase/hit timing still follow the real simulation.
            if (!skin.isVisible) return;
            float s = Mathf.Sin(phase), breath = Mathf.Sin(Time.time * 1.8f + idlePhase);
            float age = Time.time - hitAt;
            HitBlend = age < .28f ? Mathf.Sin(Mathf.Clamp01(age / .28f) * Mathf.PI) : 0;
            var target = melee.PoseTarget;
            float swingAge = melee.SwingAge;
            float windup = Mathf.Max(.06f, melee.Windup);
            float prepare = target != null ? Mathf.Clamp01(swingAge / windup) : 0;
            float release = target != null ? Mathf.Clamp01((swingAge - windup - .065f) / .30f) : 1;
            // Anticipation -> rapid contact -> follow-through -> recovery, tied to the combat clock.
            float contact = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.50f, 1, prepare)) * (1 - Mathf.SmoothStep(0, 1, release));
            float coil = Mathf.Sin(prepare * Mathf.PI) * (1 - contact);
            ContactBlend = contact;
            float side = (melee.SwingVersion & 1) == 0 ? -1 : 1;
            bones[0].localPosition = hipRest + new Vector3(0, Mathf.Abs(s) * (heavy ? .018f : .028f) * walking, 0);
            Turn(0, heavy ? 3 : runner ? 9 : 0, 0, s * (heavy ? 3 : 2) * walking);
            Turn(1, (runner ? 16 : heavy ? 5 : 7) + breath + contact * (heavy ? 12 : 8) - coil * 8 - HitBlend * 22,
                s * 4 * walking + side * (runner ? 20 : heavy ? 14 : 7) * coil, side * HitBlend * 6);
            Turn(2, -5 - breath * 2 + HitBlend * 12, -s * 4 * walking, breath * 2);
            for (int i = 3; i <= 6; i += 3)
            {
                float sign = i == 3 ? -1 : 1;
                float stride = i == 3 ? -s : s;
                float baseAngle = runner ? -15 + stride * 32 * walking : heavy ? -15 + stride * 13 * walking : -48 + stride * 10 * walking;
                float pull = heavy ? 48 : runner ? 30 : 22;
                Turn(i, baseAngle + coil * pull + HitBlend * 16, sign * coil * 12, sign * (heavy ? 10 : 5));
                Turn(i + 1, runner ? -25 - coil * 35 : -12 - coil * (heavy ? 65 : 25), 0, 0);
                Turn(i + 2, -10 - coil * 20, 0, sign * 5);
            }
            Leg(9, s); Leg(12, -s);
            if (target != null && contact > .001f)
            {
                if (contactTarget != target)
                { contactTarget = target; contactCollider = target.GetComponent<Collider>(); }
                // Player capsule surface, rather than root position. Door poses use the authored plane.
                float height = Mathf.Min(bones[3].position.y - .06f, target.transform.position.y + 1.18f);
                Vector3 point = new Vector3(target.transform.position.x, height, target.transform.position.z);
                Vector3 toSource = brain.transform.position - point; toSource.y = 0;
                if (contactCollider != null && contactCollider.enabled)
                    point = contactCollider.ClosestPoint(point + toSource.normalized * 3);
                LastContactPoint = point;
                if (runner)
                    ReachArm(side < 0 ? 3 : 6, point, contact);
                else
                {
                    ReachArm(3, point - brain.transform.right * (heavy ? .09f : .06f), contact);
                    ReachArm(6, point + brain.transform.right * (heavy ? .09f : .06f), contact);
                }
            }
        }
        // Analytic two-bone reach, with an outward/downward elbow pole; no IK package or raycasts.
        private void ReachArm(int index, Vector3 contact, float blend)
        {
            Transform a = bones[index], b = bones[index + 1], hand = bones[index + 2];
            Quaternion ra = a.rotation, rb = b.rotation, rh = hand.rotation;
            Vector3 toward = contact - a.position;
            Vector3 wrist = contact - toward.normalized * (.13f * transform.localScale.x);
            Vector3 delta = wrist - a.position;
            float l1 = Vector3.Distance(a.position, b.position), l2 = Vector3.Distance(b.position, hand.position);
            float d = Mathf.Clamp(delta.magnitude, .01f, l1 + l2 - .001f);
            Vector3 axis = delta.normalized;
            Vector3 pole = brain.transform.right * (index == 3 ? -1 : 1) - Vector3.up;
            pole = Vector3.ProjectOnPlane(pole, axis).normalized;
            float along = (l1 * l1 + d * d - l2 * l2) / (2 * d);
            float lift = Mathf.Sqrt(Mathf.Max(0, l1 * l1 - along * along));
            Vector3 elbow = a.position + axis * along + pole * lift;
            a.rotation = Quaternion.FromToRotation(b.position - a.position, elbow - a.position) * a.rotation;
            b.rotation = Quaternion.FromToRotation(hand.position - b.position, wrist - b.position) * b.rotation;
            hand.rotation = Quaternion.FromToRotation(hand.TransformDirection(new Vector3(0, -.15f, .10f)), toward) * hand.rotation;
            Quaternion endA = a.rotation, endB = b.rotation, endH = hand.rotation;
            a.rotation = Quaternion.Slerp(ra, endA, blend);
            b.rotation = Quaternion.Slerp(rb, endB, blend);
            hand.rotation = Quaternion.Slerp(rh, endH, blend);
        }
        private void Leg(int i, float stride)
        {
            Turn(i, stride * (runner ? 39 : heavy ? 22 : 25) * walking + (heavy ? -8 : 0), 0, 0);
            Turn(i + 1, Mathf.Max(0, -stride) * (runner ? 54 : 34) * walking + (heavy ? 12 : 0), 0, 0);
            Turn(i + 2, -stride * 13 * walking, 0, 0);
        }
        private void Turn(int i, float x, float y, float z) => bones[i].localRotation = Quaternion.Euler(x, y, z);
    }
}
