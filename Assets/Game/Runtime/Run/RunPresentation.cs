using UnityEngine;

namespace WaitYourTurn.Run
{
    /// <summary>Only visual roots/camera move. Gameplay surfaces and colliders stay fixed.</summary>
    public sealed class RunPresentation : MonoBehaviour
    {
        [SerializeField] private RunDriver run;
        [SerializeField] private Transform environment;
        [SerializeField] private Camera view;
        private Vector3 environmentOrigin, cameraOffset;
        private Quaternion cameraRotation;
        private float distance;
        public float Speed { get; private set; }
        public Vector3 FollowOffset => cameraOffset;
        public float Darkness => run.Flow?.Phase switch
        { RunPhase.FadeOut => run.Flow.Progress, RunPhase.Hidden => 1, RunPhase.FadeIn => 1 - run.Flow.Progress, _ => 0 };
        public void Configure(RunDriver owner, Transform visualRoot, Camera camera)
        { run = owner; environment = visualRoot; view = camera; }
        private void Awake()
        { environmentOrigin = environment.position; cameraOffset = view.transform.position; cameraRotation = view.transform.rotation; }
        private void LateUpdate()
        {
            if (run.Flow == null || run.CurrentWagon == null) return;
            float p = Mathf.SmoothStep(0, 1, run.Flow.Progress);
            Speed = run.Flow.Phase switch
            {
                RunPhase.Approach => 5 * (1 - p), RunPhase.Departing => 5 * p,
                RunPhase.FadeOut or RunPhase.Hidden or RunPhase.FadeIn => 5, _ => 0
            };
            distance = Mathf.Repeat(distance + Speed * Time.unscaledDeltaTime, 2);
            environment.position = environmentOrigin - Vector3.forward * distance;
            view.transform.SetPositionAndRotation(run.CurrentWagon.transform.position + cameraOffset, cameraRotation);
        }
    }
}
