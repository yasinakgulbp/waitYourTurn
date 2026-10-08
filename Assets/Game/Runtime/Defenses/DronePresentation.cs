using UnityEngine;
using WaitYourTurn.Combat;

namespace WaitYourTurn.Defenses
{
    /// <summary>Cosmetic bob/rotors/trace; never applies damage or moves the flight root.</summary>
    public sealed class DronePresentation : MonoBehaviour
    {
        [SerializeField] private HitscanWeapon weapon;
        [SerializeField] private Transform visual, head, burst;
        [SerializeField] private Transform[] rotors;
        [SerializeField] private Renderer accent;
        [SerializeField] private LineRenderer trace;
        private DroneDefinition definition;
        private MaterialPropertyBlock tint;
        private float traceUntil, burstAt;
        private bool exhausted;
        public void Configure(HitscanWeapon gun, Transform model, Transform aim, Transform[] blades, Renderer color, LineRenderer line, Transform effect)
        { weapon = gun; visual = model; head = aim; rotors = blades; accent = color; trace = line; burst = effect; }
        private void OnEnable() => weapon.Fired += OnShot;
        private void OnDisable() { weapon.Fired -= OnShot; Clear(); }
        public void Show(DroneDefinition data)
        { definition = data; Clear(); visual.gameObject.SetActive(true); Tint(data.color); }
        public void Clear() { exhausted = false; trace.enabled = false; burst.gameObject.SetActive(false); visual.localPosition = Vector3.zero; }
        private void Tint(Color color)
        {
            if (tint == null) tint = new MaterialPropertyBlock();
            tint.SetColor("_BaseColor", color); tint.SetColor("_Color", color); accent.SetPropertyBlock(tint);
        }
        public void SetKamikaze() => Tint(new Color(1, .25f, .06f));
        public void AimAt(Vector3 point)
        {
            Vector3 direction = point - transform.position; direction.y = 0;
            if (direction.sqrMagnitude > .001f) head.rotation = Quaternion.LookRotation(direction);
        }
        public void Exhaust(bool explode)
        {
            exhausted = true; visual.gameObject.SetActive(false); trace.enabled = false;
            burst.gameObject.SetActive(explode); burst.localScale = Vector3.one * .2f; burstAt = Time.time;
        }
        private void OnShot(ShotNotice shot)
        {
            trace.startColor = trace.endColor = shot.Color; trace.SetPosition(0, shot.Start); trace.SetPosition(1, shot.End);
            trace.enabled = true; traceUntil = Time.time + .08f;
        }
        private void Update()
        {
            if (Time.time >= traceUntil) trace.enabled = false;
            if (exhausted) { burst.localScale = Vector3.one * Mathf.Lerp(.2f, .8f, (Time.time - burstAt) / .35f); return; }
            if (definition == null) return;
            visual.localPosition = new Vector3(Mathf.Sin(Time.time * 1.3f) * .025f, Mathf.Sin(Time.time * definition.bobFrequency * Mathf.PI * 2) * definition.bobAmplitude, 0);
            foreach (var rotor in rotors) rotor.Rotate(Vector3.up, 1800 * Time.deltaTime, Space.Self);
        }
    }
}
