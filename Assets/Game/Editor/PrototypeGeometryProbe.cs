using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace WaitYourTurn.Editor
{
    public static class PrototypeGeometryProbe
    {
        [MenuItem("Wait Your Turn/Integration/Inspect Original Geometry %#F6")]
        public static void Inspect()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var text = new StringBuilder();
            foreach (string path in new[] { "Assets/ueni istasyon/yeni vagon/300.fbx", "Assets/ueni istasyon/Cevre2.fbx" })
            {
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var instance = Object.Instantiate(asset);
                text.AppendLine(path);
                foreach (var mesh in instance.GetComponentsInChildren<MeshFilter>())
                    text.AppendLine($"{mesh.name}: pos={mesh.transform.position:F3}, rotation={mesh.transform.eulerAngles:F3}, bounds={mesh.GetComponent<Renderer>()?.bounds.ToString("F3")}");
                Object.DestroyImmediate(instance);
            }
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene 2.unity", OpenSceneMode.Additive);
            foreach (var root in scene.GetRootGameObjects())
            {
                text.AppendLine($"ROOT {root.name} pos={root.transform.position:F3} scale={root.transform.localScale:F3} rot={root.transform.eulerAngles:F3}");
                if (root.name == "Vagons") foreach (Transform child in root.transform)
                    text.AppendLine($"CHILD {child.name} pos={child.position:F3} scale={child.lossyScale:F3}");
                foreach (var renderer in root.GetComponentsInChildren<Renderer>())
                    if (root.name == "Vagons")
                        text.AppendLine($"  {renderer.transform.parent.name}/{renderer.name}: {renderer.bounds.ToString("F3")}");
            }
            EditorSceneManager.CloseScene(scene, true);
            Directory.CreateDirectory("Logs");
            File.WriteAllText("Logs/PrototypeGeometry.txt", text.ToString());
            Debug.Log("[PrototypeGeometry] Reference measurements written to Logs/PrototypeGeometry.txt");
        }
    }
}
