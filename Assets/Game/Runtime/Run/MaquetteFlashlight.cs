using UnityEngine;

namespace WaitYourTurn.Run
{
    /// <summary>One player-attached light. No raycasts, allocations or competing camera follow.</summary>
    [ExecuteAlways, RequireComponent(typeof(Light)), DisallowMultipleComponent]
    public sealed class MaquetteFlashlight : MonoBehaviour
    {
        [SerializeField] private bool castMetalShadows = true;
        [SerializeField, Range(.1f, 5)] private float brightness = 1.8f;
        [SerializeField, Range(2, 12)] private float reach = 8;
        [SerializeField, Range(30, 110)] private float cone = 85;
        private Light lamp;
        public bool CastMetalShadows { get => castMetalShadows; set { castMetalShadows=value; Apply(); } }
        public void Configure(float intensity) { brightness=Mathf.Clamp(intensity,.1f,5); Apply(); }
        private void OnEnable() => Apply();
        private void OnValidate() => Apply();
        private void Apply()
        {
            if(lamp==null)lamp=GetComponent<Light>();
            lamp.type=LightType.Spot;lamp.renderMode=LightRenderMode.ForcePixel;
            lamp.color=new Color(1,.91f,.77f);lamp.intensity=brightness;
            lamp.range=reach;lamp.spotAngle=cone;lamp.innerSpotAngle=cone*.65f;
            lamp.shadows=castMetalShadows?LightShadows.Hard:LightShadows.None;
            lamp.shadowCustomResolution=512;lamp.shadowStrength=.9f;
            lamp.shadowBias=.015f;lamp.shadowNormalBias=.05f;lamp.shadowNearPlane=.08f;
        }
    }
}
