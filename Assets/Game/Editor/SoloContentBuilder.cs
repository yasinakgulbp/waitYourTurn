using System.Linq;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using WaitYourTurn.Player;
using WaitYourTurn.Run;
using WaitYourTurn.Sandbox;
using WaitYourTurn.Train;

namespace WaitYourTurn.Editor
{
    public static class SoloContentBuilder
    {
        [MenuItem("Wait Your Turn/Solo/Install Wagon Passages %&F8")]
        public static void InstallCurrent()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var run = Object.FindAnyObjectByType<RunDriver>();
            if (run == null || run.Wagons.Length < 2) return;
            if (run.Solo == null) Install(run); else run.Solo.ConfigureInteractionDistance(2.8f);
            // Update the first passage draft without rebuilding the existing scene's combat content.
            foreach (var part in run.Wagons.SelectMany(w => w.GetComponentsInChildren<Transform>()))
                if (part.name == "Internal end header") part.localPosition = new Vector3(part.localPosition.x, 2.175f, part.localPosition.z);
            Bake(run.Wagons[0].transform.parent.GetComponent<NavMeshSurface>());
            ExportConnections(run.Solo);
            TrainIntegrationBuilder.ExportModelGuide(run.Wagons, run.Solo.OpenTrainSurvival);
            EditorSceneManager.MarkSceneDirty(run.gameObject.scene);
            EditorSceneManager.SaveScene(run.gameObject.scene); AssetDatabase.SaveAssets();
            Debug.Log("[Solo] Installed physical passages; existing side doors, weapons, bots and shop preserved.");
        }
        public static void Install(RunDriver run, bool survival = false)
        {
            if (run.Wagons.Length < 2 || run.Solo != null) return;
            var parent = run.Wagons[0].transform.parent;
            // This builder is for the documented aligned 12m integration blockout only.
            if (parent == null || parent.GetComponent<NavMeshSurface>()?.navMeshData == null ||
                run.Wagons.Any(w => w.transform.parent != parent || w.transform.rotation != Quaternion.identity ||
                    w.transform.lossyScale != Vector3.one || w.transform.Find("Fixed gameplay geometry") == null) ||
                Enumerable.Range(1, run.Wagons.Length - 1).Any(i =>
                    Vector3.Distance(run.Wagons[i].transform.position - run.Wagons[i - 1].transform.position, Vector3.right * 13) > .01f))
                throw new System.InvalidOperationException("Solo passage builder requires the aligned 12m wagons / 1m gap and saved navigation asset.");
            Material wall = AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Content/IntegrationBlockout/Wall.mat");
            Material floor = AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Content/IntegrationBlockout/Floor.mat");
            float passageWidth = survival ? 3f : 1.4f;
            var connections = new InteriorConnection[run.Wagons.Length - 1];
            for (int i = 0; i < connections.Length; i++)
            {
                CutEnd(run.Wagons[i], 1); CutEnd(run.Wagons[i + 1], -1);
                float a = run.Wagons[i].transform.position.x + 6, b = run.Wagons[i + 1].transform.position.x - 6;
                var root = new GameObject($"Internal passage {i + 1} - " + (survival ? "permanently open" : "manual gate")); root.transform.SetParent(parent, false);
                root.transform.position = new Vector3((a + b) * .5f, 0, 0);
                float length = b - a + .4f;
                Box("Connector floor - fixed navigation", root.transform, new Vector3(0, -.12f, 0), new Vector3(length, .24f, passageWidth + .2f), floor);
                for (int sign = -1; sign <= 1; sign += 2)
                    Box("Connector side metal", root.transform, new Vector3(0, .55f, sign * (passageWidth + .2f) * .5f), new Vector3(length, 1.1f, .2f), wall);
                var passage = new Bounds(new Vector3(0, 1, 0), new Vector3(b - a + 2.4f, 2, passageWidth));
                if (survival)
                {
                    var openConnection = root.AddComponent<InteriorConnection>();
                    openConnection.Configure(null, null, null, passage, 200 + 100 * i, true);
                    openConnection.Restore(true); connections[i] = openConnection; continue;
                }
                var panel = Box("Internal gate blocker", root.transform, new Vector3(0, 1, 0), new Vector3(.16f, 2, passageWidth), wall);
                panel.gameObject.AddComponent<NavMeshModifier>().ignoreFromBuild = true;
                var obstacle = panel.gameObject.AddComponent<NavMeshObstacle>(); obstacle.shape = NavMeshObstacleShape.Box;
                obstacle.size = Vector3.one; obstacle.carving = true; obstacle.carveOnlyStationary = false;
                // Visual is a separate child, so hiding it cannot disable the blocker or obstacle authority.
                var visual = new GameObject("Replaceable gate visual"); visual.transform.SetParent(root.transform, false);
                var display = Box("Gate display", visual.transform, new Vector3(0, 1, 0), new Vector3(.16f, 2, passageWidth), wall);
                Object.DestroyImmediate(display); panel.GetComponent<Renderer>().enabled = false;
                var gate = root.AddComponent<InteriorConnection>();
                gate.Configure(panel, obstacle, visual, passage, 200 + 100 * i);
                connections[i] = gate;
            }
            var authority = new GameObject("Solo unlocked topology and movement region"); authority.transform.SetParent(parent, false);
            var movement = authority.AddComponent<MovementArea>();
            var solo = authority.AddComponent<SoloProgression>();
            solo.Configure(run, Object.FindAnyObjectByType<RunEconomy>(), connections, movement, survival);
            if (!survival)
            {
                authority.AddComponent<SoloHud>().Configure(solo);
                authority.AddComponent<SoloAcceptance>().Configure(run, solo, Object.FindAnyObjectByType<RunEconomy>(),
                    Object.FindAnyObjectByType<StationSpawner>(), Object.FindAnyObjectByType<RunPersistence>());
            }
            Bake(parent.GetComponent<NavMeshSurface>());
            void CutEnd(WagonRuntime wagon, int sign)
            {
                // The authored blockout's 12m body is an explicit model contract.
                foreach (var transform in wagon.GetComponentsInChildren<Transform>().ToArray())
                    if ((transform.name == "End wall collider" || transform.name == "End wall visual" ||
                        survival && (transform.name == "Coupling" || transform.name == "Warm end lamp")) &&
                        Mathf.Sign(wagon.transform.InverseTransformPoint(transform.position).x) == sign)
                        Object.DestroyImmediate(transform.gameObject);
                var physics = wagon.transform.Find("Fixed gameplay geometry");
                float jambWidth = (wagon.Geometry.Interior.size.z - passageWidth) * .5f;
                for (int side = -1; side <= 1; side += 2)
                    Box("Internal end jamb", physics, new Vector3(sign * 5.9f, 1.05f, side * (passageWidth + jambWidth) * .5f), new Vector3(.2f, 2.1f, jambWidth), wall);
                Box("Internal end header", physics, new Vector3(sign * 5.9f, 2.175f, 0), new Vector3(.2f, .15f, passageWidth), wall);
            }
            ExportConnections(solo);
        }
        private static void Bake(NavMeshSurface surface)
        {
            var existing = surface.navMeshData;
            surface.BuildNavMesh();
            EditorUtility.CopySerialized(surface.navMeshData, existing);
            surface.RemoveData(); surface.navMeshData = existing; surface.AddData(); EditorUtility.SetDirty(existing);
        }
        private static void ExportConnections(SoloProgression solo)
        {
            var csv = new System.Text.StringBuilder("connection,kind,name,worldX,worldY,worldZ,localX,localY,localZ,sizeX,sizeY,sizeZ,unlockPrice\n");
            for (int i = 0; i < solo.Connections.Length; i++)
            {
                var gate = solo.Connections[i];
                Row("movement_region", "Open passage", gate.Passage.center, gate.Passage.size);
                foreach (var box in gate.GetComponentsInChildren<BoxCollider>())
                    Row("physics_box", box.name, gate.transform.InverseTransformPoint(box.transform.TransformPoint(box.center)), Vector3.Scale(box.size, box.transform.lossyScale));
                void Row(string kind, string name, Vector3 center, Vector3 size)
                {
                    var world = gate.transform.TransformPoint(center);
                    csv.Append(i).Append(',').Append(kind).Append(',').Append(name.Replace(',', '_'));
                    foreach (float value in new[] { world.x, world.y, world.z, center.x, center.y, center.z, size.x, size.y, size.z })
                        csv.Append(',').Append(value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture));
                    csv.Append(',').Append(gate.UnlockPrice).Append('\n');
                }
            }
            System.IO.Directory.CreateDirectory("docs/generated");
            System.IO.File.WriteAllText(solo.OpenTrainSurvival ? "docs/generated/survival-passage-dimensions.csv" : "docs/generated/solo-passage-dimensions.csv", csv.ToString());
        }
        private static BoxCollider Box(string name, Transform parent, Vector3 position, Vector3 size, Material material)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube); box.name = name;
            box.transform.SetParent(parent, false); box.transform.localPosition = position; box.transform.localScale = size;
            box.GetComponent<Renderer>().sharedMaterial = material;
            return box.GetComponent<BoxCollider>();
        }
    }
}
