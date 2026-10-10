using UnityEngine;

namespace WaitYourTurn.Run
{
    /// <summary>Built-in pipeline only. One camera; bounded quarter-resolution blur, no depth prepass.</summary>
    [ExecuteAlways, RequireComponent(typeof(Camera)), DisallowMultipleComponent]
    public sealed class MaquetteCinema : MonoBehaviour
    {
        [SerializeField] private Shader filmShader;
        [SerializeField, Range(0, 1)] private float blurAmount = .70f;
        [SerializeField, Range(0, .5f)] private float vignette = .22f;
        [SerializeField] private bool useBlur = true;
        private Material material;
        private Camera view;
        public bool UseBlur { get => useBlur; set => useBlur = value; }
        public float BlurAmount { get => blurAmount; set => blurAmount = Mathf.Clamp01(value); }
        public void Configure(Shader shader) { filmShader = shader; }
        private void OnEnable() { view = GetComponent<Camera>(); }
        private void OnDisable()
        {
            if (material == null) return;
            if (Application.isPlaying) Destroy(material); else DestroyImmediate(material);
            material = null;
        }
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (filmShader == null || !filmShader.isSupported)
            { Graphics.Blit(source, destination); return; }
            if (material == null) material = new Material(filmShader) { hideFlags = HideFlags.HideAndDontSave };
            // Protect the complete wagon plus nearby attackers on both sides. Camera zoom,
            // aspect and walking position change this band without making actors lose focus.
            float x = view.transform.position.x;
            float a0 = view.WorldToViewportPoint(new Vector3(x,0,-3.3f)).y;
            float a1 = view.WorldToViewportPoint(new Vector3(x,2.2f,-2.1f)).y;
            float b0 = view.WorldToViewportPoint(new Vector3(x,0,3.3f)).y;
            float b1 = view.WorldToViewportPoint(new Vector3(x,2.2f,2.1f)).y;
            float low = Mathf.Min(Mathf.Min(a0,a1),Mathf.Min(b0,b1));
            float high = Mathf.Max(Mathf.Max(a0,a1),Mathf.Max(b0,b1));
            material.SetVector("_Focus", new Vector4(low-.025f, high+.025f, .16f, blurAmount));
            material.SetFloat("_Vignette", vignette);
            bool blur = useBlur && blurAmount > 0 && QualitySettings.GetQualityLevel() > 0;
            material.SetFloat("_UseBlur", blur ? 1 : 0);
            if (!blur) { material.SetTexture("_BlurTex", source); Graphics.Blit(source, destination, material, 1); return; }
            // Cap the long side at 384px; temporary pool reuse, no persistent full-size buffer.
            int w = Mathf.Max(1, source.width/4), h = Mathf.Max(1, source.height/4);
            float scale = Mathf.Min(1, 384f/Mathf.Max(w,h)); w = Mathf.Max(1,(int)(w*scale)); h = Mathf.Max(1,(int)(h*scale));
            var a = RenderTexture.GetTemporary(w,h,0,source.format);
            var b = RenderTexture.GetTemporary(w,h,0,source.format);
            a.filterMode = b.filterMode = FilterMode.Bilinear;
            try
            {
                material.SetVector("_Axis", new Vector4(1f/w,0,0,0)); Graphics.Blit(source,a,material,0);
                material.SetVector("_Axis", new Vector4(0,1f/h,0,0)); Graphics.Blit(a,b,material,0);
                material.SetTexture("_BlurTex",b); Graphics.Blit(source,destination,material,1);
            }
            finally { RenderTexture.ReleaseTemporary(a); RenderTexture.ReleaseTemporary(b); }
        }
    }
}
