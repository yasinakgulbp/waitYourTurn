using UnityEngine;
using UnityEngine.Rendering;

namespace WaitYourTurn.Enemies
{
    /// <summary>Three presentation-only corpses per wagon. No health, navigation or body colliders.</summary>
    public sealed class CardCorpsePool : MonoBehaviour
    {
        private sealed class Slot
        {
            public Transform root;
            public SkinnedMeshRenderer skin;
            public Transform[] bones;
            public Quaternion[] rotations;
            public Vector3 hip;
            public float age, duration, side;
            public int kind;
        }
        private readonly Slot[] slots = new Slot[3];
        private int next;
        public int ActiveCount { get; private set; }
        public void Warm(CardZombieVisual template)
        {
            if (template == null || slots[0] != null) return;
            for (int i = 0; i < slots.Length; i++)
            {
                var root = Instantiate(template.gameObject, transform).transform;
                root.name = "Card corpse - visual only " + i;
                root.GetComponent<CardZombieVisual>().enabled = false;
                root.gameObject.SetActive(false);
                var renderer = root.GetComponent<SkinnedMeshRenderer>();
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                slots[i] = new Slot { root = root, skin = renderer, bones = renderer.bones,
                    rotations = new Quaternion[renderer.bones.Length] };
            }
        }
        public void Show(CardZombieVisual live, string profile)
        {
            if (live == null || slots[0] == null) return;
            var s = slots[next]; next = (next + 1) % slots.Length;
            s.root.SetPositionAndRotation(live.transform.position, live.transform.rotation);
            s.root.localScale = live.transform.lossyScale;
            s.skin.sharedMesh = live.Skin.sharedMesh;
            for (int i = 0; i < s.bones.Length; i++)
            {
                s.bones[i].localPosition = live.Skin.bones[i].localPosition;
                s.bones[i].localRotation = s.rotations[i] = live.Skin.bones[i].localRotation;
            }
            s.hip = s.bones[0].localPosition; s.age = 0;
            s.kind = profile == "fast" ? 1 : profile == "tough" ? 2 : 0;
            s.duration = s.kind == 2 ? 1.25f : s.kind == 1 ? .9f : 1.1f;
            s.side = (next & 1) == 0 ? -1 : 1;
            s.root.gameObject.SetActive(true);
        }
        public void Clear()
        {
            foreach (var s in slots) if (s != null) s.root.gameObject.SetActive(false);
            ActiveCount = next = 0;
        }
        private void LateUpdate()
        {
            ActiveCount = 0;
            foreach (var s in slots)
            {
                if (s == null || !s.root.gameObject.activeSelf) continue;
                s.age += Time.deltaTime;
                if (s.age >= s.duration) { s.root.gameObject.SetActive(false); continue; }
                ActiveCount++;
                float t = Mathf.Clamp01(s.age / (s.duration * .65f));
                float fall = Mathf.SmoothStep(0, 1, t);
                float sink = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.78f, 1, s.age / s.duration));
                s.bones[0].localPosition = s.hip + new Vector3(0, -.54f * fall - 1.25f * sink, (s.kind == 2 ? .12f : -.08f) * fall);
                Rotate(s, 0, s.kind == 2 ? 77 : -74, s.kind == 1 ? s.side * 35 : 0, s.side * (s.kind == 1 ? 52 : 18), fall);
                Rotate(s, 1, s.kind == 2 ? 28 : -12, s.side * 12, 0, fall);
                Rotate(s, 2, 20, s.side * 17, 0, fall);
                Rotate(s, 9, -35, 0, -8, fall); Rotate(s, 12, -22, 0, 8, fall);
                Rotate(s, 10, 60, 0, 0, fall); Rotate(s, 13, 70, 0, 0, fall);
                Rotate(s, 3, -24, 0, -35, fall); Rotate(s, 6, 15, 0, 28, fall);
            }
        }
        private static void Rotate(Slot s, int i, float x, float y, float z, float t)
            => s.bones[i].localRotation = Quaternion.Slerp(s.rotations[i], Quaternion.Euler(x, y, z), t);
    }
}
