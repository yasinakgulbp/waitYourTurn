using UnityEngine;

namespace WaitYourTurn.Run
{
    /// <summary>Only visual roots/camera move. Gameplay surfaces and colliders stay fixed.</summary>
    public sealed class RunPresentation : MonoBehaviour
    {
        [SerializeField] private RunDriver run;
        [SerializeField] private Transform environment;
        [SerializeField] private Transform stationVisuals;
        [SerializeField] private Transform scenery;
        [SerializeField, Min(.1f)] private float cruiseSpeed = 5;
        [SerializeField, Min(2)] private float sceneryRepeat = 20;
        [SerializeField] private Camera view;
        [SerializeField] private bool followPlayerAlongTrain;
        public void ConfigurePlayerFollow(bool follow) => followPlayerAlongTrain = follow;
        private Vector3 environmentOrigin, stationOrigin, sceneryOrigin, cameraOffset;
        private Quaternion cameraRotation;
        private float distance;
        private RunFlow observedFlow;
        public float Speed { get; private set; }
        public float StationOffset { get; private set; }
        public Transform TrackVisuals => environment;
        public Transform StationVisuals => stationVisuals;
        public Transform Scenery => scenery;
        public Vector3 FollowOffset => cameraOffset;
        public float Darkness => run.Flow?.Phase switch
        { RunPhase.FadeOut => run.Flow.Progress, RunPhase.Hidden => 1, RunPhase.FadeIn => 1 - run.Flow.Progress, _ => 0 };
        public void Configure(RunDriver owner, Transform visualRoot, Camera camera)
        { run = owner; environment = visualRoot; view = camera; }
        public void ConfigureJourney(Transform stationRoot, Transform sceneryRoot)
        { stationVisuals = stationRoot; scenery = sceneryRoot; }
        private void Awake()
        {
            environmentOrigin = environment.position;
            if (stationVisuals != null) stationOrigin = stationVisuals.position;
            if (scenery != null) sceneryOrigin = scenery.position;
            cameraOffset = view.transform.position; cameraRotation = view.transform.rotation;
        }
        private void LateUpdate()
        {
            if (run.Flow == null || run.CurrentWagon == null) return;
            if (!ReferenceEquals(observedFlow, run.Flow))
            { observedFlow = run.Flow; distance = 0; }
            Speed = JourneyMotion.Speed(run.Flow, cruiseSpeed);
            // The scenery period is an exact multiple of the two-unit sleeper spacing.
            // Both are bounded; neither can accumulate floating-point drift over a long run.
            float period = Mathf.Max(2, Mathf.Round(sceneryRepeat / 2) * 2);
            distance = Mathf.Repeat(distance + Speed * Time.unscaledDeltaTime, period);
            Vector3 axis = environment.right;
            environment.position = environmentOrigin - axis * Mathf.Repeat(distance, 2);
            if (scenery != null) scenery.position = sceneryOrigin - axis * distance;
            if (stationVisuals != null && !run.Flow.Terminal)
            {
                // The station swaps between visits only while black; it never loops with sleepers.
                bool visible = run.Flow.Phase != RunPhase.Hidden;
                if (stationVisuals.gameObject.activeSelf != visible) stationVisuals.gameObject.SetActive(visible);
                if (visible)
                {
                    StationOffset = JourneyMotion.StationOffset(run.Flow, cruiseSpeed);
                    stationVisuals.position = stationOrigin + axis * StationOffset;
                }
            }
            Vector3 cameraPosition = run.CurrentWagon.transform.position + cameraOffset;
            if (followPlayerAlongTrain) cameraPosition.x = run.Player.transform.position.x + cameraOffset.x;
            view.transform.SetPositionAndRotation(cameraPosition, cameraRotation);
        }
    }
}
