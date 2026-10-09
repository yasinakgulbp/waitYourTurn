using UnityEngine;
using WaitYourTurn.Combat;

namespace WaitYourTurn.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        [SerializeField] private MoveInput input;
        [SerializeField] private HealthComponent health;
        [SerializeField] private Camera view;
        [SerializeField, Min(0.1f)] private float speed = 3.5f;
        [SerializeField, Min(1f)] private float walkingTurnSpeed = 720f;
        [SerializeField] private MovementArea movementArea;
        private CharacterController body;
        private float vertical;
        [SerializeField] private bool worldControl;
        private Vector3 worldMove;
        private Vector3 walkingDirection;
        private AutoAim aim;
        public Vector3 LastMoveDirection { get; private set; }
        public void ConfigureWorldControl(HealthComponent actor, float moveSpeed)
        { health = actor; speed = moveSpeed; worldControl = true; }
        public void SetWorldMove(Vector3 direction) { direction.y = 0; worldMove = Vector3.ClampMagnitude(direction, 1); }
        public void Configure(MoveInput controls, HealthComponent hero, Camera camera)
        { input = controls; health = hero; view = camera; worldControl = false; }
        private void Awake() { body = GetComponent<CharacterController>(); aim = GetComponent<AutoAim>(); }
        public void SetMovementArea(MovementArea area) => movementArea = area;
        private void Update()
        {
            LastMoveDirection = Vector3.zero;
            walkingDirection = Vector3.zero;
            if (!health.IsAlive || (!worldControl && !input.InputEnabled)) return;
            Vector3 motion = worldMove * speed;
            if (!worldControl)
            {
                Vector3 forward = view.transform.forward; forward.y = 0; forward.Normalize();
                Vector3 right = view.transform.right; right.y = 0; right.Normalize();
                motion = (right * input.Move.x + forward * input.Move.y) * speed;
            }
            walkingDirection = motion;
            vertical = body.isGrounded ? -2f : Mathf.Max(-20, vertical - 9.81f * Time.deltaTime);
            motion.y = vertical;
            Vector3 before = transform.position;
            MoveDisplacement(motion * Time.deltaTime);
            LastMoveDirection = transform.position - before; LastMoveDirection = new Vector3(LastMoveDirection.x, 0, LastMoveDirection.z);
        }
        // One rotation owner, after movement and target selection regardless of their Update ordering.
        private void LateUpdate()
        {
            // Final pose also stays valid after other actors move, before presentation/save capture.
            ConstrainActualPosition();
            if (!health.IsAlive || Time.deltaTime <= 0) return;
            Vector3 facing = walkingDirection;
            bool aiming = false;
            if (aim != null && aim.TryFacing(out Vector3 targetDirection))
            { facing = targetDirection; aiming = true; }
            if (facing.sqrMagnitude <= .000001f) return;
            Quaternion rotation = Quaternion.LookRotation(facing);
            transform.rotation = aiming ? rotation : Quaternion.RotateTowards(transform.rotation, rotation, walkingTurnSpeed * Time.deltaTime);
        }
        private float WorldRadius
        {
            get
            {
                Vector3 scale = transform.lossyScale;
                return (body.radius + body.skinWidth) * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
            }
        }
        private void ConstrainActualPosition()
        {
            if (movementArea == null || !movementArea.isActiveAndEnabled || !body.enabled) return;
            Vector3 center = transform.TransformPoint(body.center);
            Vector3 correction = movementArea.Constrain(center, WorldRadius) - center;
            // Another Move would depenetrate against the same crowd and could escape again.
            // Direct pose correction keeps the controller enabled and preserves turret ignore pairs.
            if (correction.sqrMagnitude > .0000000001f) transform.position += correction;
        }
        public void MoveDisplacement(Vector3 displacement)
        {
            if (movementArea != null && movementArea.isActiveAndEnabled)
            {
                Vector3 center = transform.TransformPoint(body.center);
                displacement = movementArea.Constrain(center + displacement, WorldRadius) - center;
            }
            body.Move(displacement);
            // Collision resolution can move further than the requested displacement.
            ConstrainActualPosition();
        }
        public void Place(Vector3 point)
        {
            body.enabled = false;
            transform.position = point;
            vertical = 0f;
            LastMoveDirection = Vector3.zero;
            walkingDirection = Vector3.zero;
            body.enabled = true;
        }
    }
}
