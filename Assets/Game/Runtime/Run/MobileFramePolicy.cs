using UnityEngine;

namespace WaitYourTurn.Run
{
    /// <summary>Explicit mobile frame budget; gameplay does not depend on display refresh.</summary>
    public static class MobileFramePolicy
    {
        public static int Target { get; private set; } = 60;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (!Application.isMobilePlatform) return;
            SetTarget(60);
        }
        public static void SetTarget(int frames)
        {
            Target = frames <= 30 ? 30 : 60;
            if (Application.isMobilePlatform) Application.targetFrameRate = Target;
        }
    }
}
