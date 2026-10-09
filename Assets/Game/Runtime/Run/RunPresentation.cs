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
        [SerializeField] private Transform incomingStation;
        [SerializeField, Min(.1f)] private float cruiseSpeed = 5;
        [SerializeField, Min(2)] private float sceneryRepeat = 20;
        [SerializeField] private Camera view;
        [SerializeField] private bool followPlayerAlongTrain;
        [SerializeField] private bool responsiveFraming;
        [SerializeField] private Bounds framingVolume = new Bounds(new Vector3(0, .3f, 0), new Vector3(16, 2.2f, 10));
        [SerializeField] private float playerFollowLimit = .65f;
        public bool ResponsiveFraming => responsiveFraming;
        public Bounds FramingVolume => framingVolume;
        public void ConfigureFraming(Bounds volume) { responsiveFraming = true; framingVolume = volume; }
        public void ConfigurePlayerFollow(bool follow) => followPlayerAlongTrain = follow;
        private Vector3 environmentOrigin, stationOrigin, sceneryOrigin, cameraOffset;
        private Quaternion cameraRotation;
        private Camera marginClear;
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
        public void ConfigureIncomingStation(Transform visualRoot) => incomingStation = visualRoot;
        private void Awake()
        {
            environmentOrigin = environment.position;
            if (stationVisuals != null) stationOrigin = stationVisuals.position;
            if (scenery != null) sceneryOrigin = scenery.position;
            cameraOffset = view.transform.position; cameraRotation = view.transform.rotation;
            // A partial safe-area viewport never clears the unused backbuffer margins.
            // Clear them before the game camera so transient touch/UI drawings cannot persist.
            if (responsiveFraming)
            {
                var background = new GameObject("Safe area background clear");
                background.transform.SetParent(transform, false);
                marginClear = background.AddComponent<Camera>();
                marginClear.clearFlags = CameraClearFlags.SolidColor;
                marginClear.backgroundColor = Color.black;
                marginClear.cullingMask = 0;
                marginClear.allowHDR = marginClear.allowMSAA = false;
                marginClear.useOcclusionCulling = false;
                marginClear.depth = view.depth - 1;
                marginClear.enabled = false;
            }
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
                // Survival uses two identical visual copies to bring the next station into view continuously.
                bool visible = run.Flow.Phase != RunPhase.Hidden;
                if (stationVisuals.gameObject.activeSelf != visible) stationVisuals.gameObject.SetActive(visible);
                if (visible)
                {
                    StationOffset = JourneyMotion.StationOffset(run.Flow, cruiseSpeed);
                    stationVisuals.position = stationOrigin + axis * StationOffset;
                    if (incomingStation != null)
                    {
                        float spacing = cruiseSpeed * (run.Flow.DepartureDuration * .5f +
                            run.Flow.CruiseDuration + run.Flow.NextApproachDuration * .5f);
                        incomingStation.position = stationOrigin + axis * (StationOffset + spacing);
                    }
                }
            }
            Vector3 cameraPosition = run.CurrentWagon.transform.position + cameraOffset;
            if (responsiveFraming)
            {
                Rect safe = Screen.safeArea;
                view.rect = new Rect(safe.x / Screen.width, safe.y / Screen.height, safe.width / Screen.width, safe.height / Screen.height);
                if (marginClear != null) marginClear.enabled = safe.width < Screen.width || safe.height < Screen.height;
                float follow = followPlayerAlongTrain ? Mathf.Clamp(run.Player.transform.position.x - run.CurrentWagon.transform.position.x,
                    -playerFollowLimit, playerFollowLimit) : 0;
                // Fit the whole wagon and both approaches even at either extreme of the bounded follow.
                Bounds protectedVolume = framingVolume;
                protectedVolume.Expand(new Vector3(playerFollowLimit * 2, 0, 0));
                float d = WagonCameraFraming.Distance(protectedVolume, cameraRotation, view.fieldOfView,
                    safe.width / Mathf.Max(1, safe.height), WagonCameraFraming.ProtectedViewport);
                cameraPosition = run.UsesOpenTrainSurvival ?
                    new Vector3(run.Player.transform.position.x, framingVolume.center.y, run.Player.transform.position.z) - cameraRotation * Vector3.forward * d :
                    run.CurrentWagon.transform.position + Vector3.right * follow - cameraRotation * Vector3.forward * d;
            }
            else if (followPlayerAlongTrain) cameraPosition.x = run.Player.transform.position.x + cameraOffset.x;
            // Walking through a Solo connector must not jump a whole wagon at the midpoint.
            if (run.UsesSoloProgression && !run.UsesOpenTrainSurvival)
                cameraPosition.x = Mathf.MoveTowards(view.transform.position.x, cameraPosition.x, 12 * Time.unscaledDeltaTime);
            view.transform.SetPositionAndRotation(cameraPosition, cameraRotation);
        }
    }
}
