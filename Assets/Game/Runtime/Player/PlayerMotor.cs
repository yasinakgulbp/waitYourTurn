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
        private CharacterController body;
        private float vertical;
        public void Configure(MoveInput controls, HealthComponent hero, Camera camera)
        { input = controls; health = hero; view = camera; }
        private void Awake() => body = GetComponent<CharacterController>();
        private void Update()
        {
            if (!health.IsAlive || !input.InputEnabled) return;
            Vector3 forward = view.transform.forward; forward.y = 0; forward.Normalize();
            Vector3 right = view.transform.right; right.y = 0; right.Normalize();
            Vector3 motion = (right * input.Move.x + forward * input.Move.y) * speed;
            vertical = body.isGrounded ? -2f : Mathf.Max(-20, vertical - 9.81f * Time.deltaTime);
            motion.y = vertical;
            body.Move(motion * Time.deltaTime);
        }
        public void Place(Vector3 point)
        {
            body.enabled = false;
            transform.position = point;
            vertical = 0f;
            body.enabled = true;
        }
    }
}
