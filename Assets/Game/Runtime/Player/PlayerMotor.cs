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
        [SerializeField] private MovementArea movementArea;
        private CharacterController body;
        private float vertical;
        [SerializeField] private bool worldControl;
        private Vector3 worldMove;
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
            if (!health.IsAlive || (!worldControl && !input.InputEnabled)) return;
            Vector3 motion = worldMove * speed;
            if (!worldControl)
            {
                Vector3 forward = view.transform.forward; forward.y = 0; forward.Normalize();
                Vector3 right = view.transform.right; right.y = 0; right.Normalize();
                motion = (right * input.Move.x + forward * input.Move.y) * speed;
            }
            vertical = body.isGrounded ? -2f : Mathf.Max(-20, vertical - 9.81f * Time.deltaTime);
            motion.y = vertical;
            Vector3 before = transform.position;
            MoveDisplacement(motion * Time.deltaTime);
            LastMoveDirection = transform.position - before; LastMoveDirection = new Vector3(LastMoveDirection.x, 0, LastMoveDirection.z);
        }
        // One rotation owner, after movement and target selection regardless of their Update ordering.
        private void LateUpdate()
        {
            if (!health.IsAlive || Time.deltaTime <= 0) return;
            Vector3 facing = aim != null && aim.TryFacing(out Vector3 targetDirection) ? targetDirection : LastMoveDirection;
            if (facing.sqrMagnitude > .000001f) transform.rotation = Quaternion.LookRotation(facing);
        }
        public void MoveDisplacement(Vector3 displacement)
        {
            if (movementArea != null && movementArea.isActiveAndEnabled)
            {
                Vector3 center = transform.TransformPoint(body.center);
                Vector3 scale = transform.lossyScale;
                float radius = (body.radius + body.skinWidth) * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
                displacement = movementArea.Constrain(center + displacement, radius) - center;
            }
            body.Move(displacement);
        }
        public void Place(Vector3 point)
        {
            body.enabled = false;
            transform.position = point;
            vertical = 0f;
            LastMoveDirection = Vector3.zero;
            body.enabled = true;
        }
    }
}
