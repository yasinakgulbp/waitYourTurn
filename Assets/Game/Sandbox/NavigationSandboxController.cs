using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;
using WaitYourTurn.Navigation;

namespace WaitYourTurn.Sandbox
{
    /// <summary>Temporary controls and acceptance checks; not the final enemy or door logic.</summary>
    public sealed class NavigationSandboxController : MonoBehaviour
    {
        [SerializeField] private EntryPortal portal;
        [SerializeField] private PortalNavigator agentTemplate;
        [SerializeField] private Renderer doorVisual;
        [SerializeField] private Transform visualEnvironment;
        private readonly List<PortalNavigator> agents = new List<PortalNavigator>();
        private MaterialPropertyBlock doorProperties;
        private NavMeshPath queryPath;
        private string result = "Ready. Open the gate to let the zombie enter.";
        private bool checking;
        private int frameCount;
        private float frameTime;
        private float averageFrameMs;
        private uint visualRevision = uint.MaxValue;
        private float visualStartX;

        public void Configure(EntryPortal entry, PortalNavigator template, Renderer visual, Transform environment)
        {
            portal = entry;
            agentTemplate = template;
            doorVisual = visual;
            visualEnvironment = environment;
        }

        private void Awake()
        {
            doorProperties = new MaterialPropertyBlock();
            queryPath = new NavMeshPath();
        }

        private void Start()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            Application.targetFrameRate = 60; // Lab measurement target; final game quality profiles come later.
#endif
            agents.Add(agentTemplate);
            visualStartX = visualEnvironment.position.x;
            ResetAgents();
        }

        private void Update()
        {
            if (!checking)
            {
                if (Input.GetKeyDown(KeyCode.Space)) ToggleGate();
                if (Input.GetKeyDown(KeyCode.R)) ResetAgents();
                if (Input.GetKeyDown(KeyCode.T)) StartCoroutine(CheckTenCycles());
            }
            // Presentation-only movement: no colliders or nav sources in this hierarchy.
            Vector3 visualPosition = visualEnvironment.position;
            visualPosition.x = visualStartX + Mathf.Sin(Time.time * 0.3f) * 2f;
            visualEnvironment.position = visualPosition;
            frameTime += Time.unscaledDeltaTime;
            frameCount++;
            if (frameTime >= 1f)
            {
                averageFrameMs = 1000f * frameTime / frameCount;
                frameTime = 0f;
                frameCount = 0;
            }
            if (visualRevision != portal.Revision)
            {
                visualRevision = portal.Revision;
                doorVisual.enabled = !portal.IsOpen;
                doorProperties.SetColor("_Color", portal.ClosePending ? Color.yellow : new Color(0.9f, 0.22f, 0.17f));
                doorVisual.SetPropertyBlock(doorProperties);
            }
        }

        private void OnGUI()
        {
            float scale = Mathf.Clamp(Screen.width / 1100f, 0.8f, 1.6f);
            Matrix4x4 previous = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(Vector3.one * scale);
            GUILayout.BeginArea(new Rect(16, 16, 450, 290), GUI.skin.box);
            GUILayout.Label("NAVIGATION LAB  |  M1");
            GUILayout.Label("Station (blue) -> gate -> wagon (green)");
            GUILayout.Label("Gate: " + (portal.ClosePending ? "CLOSING: waiting for passage" : portal.IsOpen ? "OPEN" : "CLOSED"));
            GUILayout.Label("Zombie: " + agentTemplate.State + "  |  Agents: " + agents.Count);
            GUILayout.Label($"Frame average: {averageFrameMs:F1} ms  |  {Application.platform}");
            GUI.enabled = !checking;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Open / Close [Space]", GUILayout.Height(34))) ToggleGate();
            if (GUILayout.Button("Reset [R]", GUILayout.Height(34))) ResetAgents();
            GUILayout.EndHorizontal();
            if (GUILayout.Button("Check 10 cycles [T]", GUILayout.Height(34))) StartCoroutine(CheckTenCycles());
            GUILayout.BeginHorizontal();
            foreach (int count in new[] { 1, 10, 30, 60, 100 })
                if (GUILayout.Button(count.ToString(), GUILayout.Height(28))) SetAgentCount(count);
            GUILayout.EndHorizontal();
            GUI.enabled = true;
            GUILayout.Label(result, GUILayout.Height(48));
            GUILayout.Label("Lab gate control only. Health/repair comes in M2/M3.");
            GUILayout.EndArea();
            GUI.matrix = previous;
        }

        private void ToggleGate() => portal.SetOpen(!portal.IsOpen || portal.ClosePending);

        private Vector3 OutsideSpawn(int index)
        {
            if (index == 0) return new Vector3(0f, 0f, -7f);
            return new Vector3(((index % 10) - 4.5f) * 0.8f, 0f, -2.5f - (index / 10) * 0.6f);
        }

        private void ResetAgents()
        {
            for (int i = 0; i < agents.Count; i++)
            {
                agents[i].TryPlace(OutsideSpawn(i));
                agents[i].SetGoal(true);
            }
            portal.SetOpen(false);
            result = "Reset: zombies approach the closed gate.";
        }

        private void SetAgentCount(int count)
        {
            // In crowd runs the template also gets its own grid slot. Its single-agent
            // acceptance goal would otherwise overlap the slot of a later clone.
            agentTemplate.Configure(portal,
                count == 1 ? Vector3.zero : new Vector3(-2.925f, 0f, 0f),
                count == 1 ? Vector3.zero : new Vector3(-2.925f, 0f, -4f));
            for (int i = agents.Count - 1; i >= count; i--)
            {
                Destroy(agents[i].gameObject);
                agents.RemoveAt(i);
            }
            while (agents.Count < count)
            {
                int index = agents.Count;
                PortalNavigator clone = Instantiate(agentTemplate, OutsideSpawn(index), Quaternion.identity);
                clone.name = "Lab Zombie " + (index + 1);
                Vector3 waiting = new Vector3(((index % 10) - 4.5f) * 0.65f, 0f, -(index / 10) * 0.55f);
                Vector3 arrival = new Vector3(((index % 10) - 4.5f) * 0.65f, 0f, (index / 10) * 0.65f - 4f);
                clone.Configure(portal, waiting, arrival);
                agents.Add(clone);
            }
            ResetAgents();
        }

        private IEnumerator CheckTenCycles()
        {
            checking = true;
            SetAgentCount(1);
            yield return null;
            for (int cycle = 1; cycle <= 10; cycle++)
            {
                ResetAgents();
                yield return null;
                if (HasCompletePath(agentTemplate.transform.position, agentTemplate.InsideGoal))
                { Finish(false, cycle - 1, "Closed gate still has an inside path."); yield break; }

                yield return WaitForPosition(portal.OutsideApproach, 8f);
                if (Vector3.Distance(agentTemplate.transform.position, portal.OutsideApproach) > 0.5f ||
                    portal.IsInside(agentTemplate.transform.position))
                { Finish(false, cycle - 1, "Zombie did not wait outside the closed gate."); yield break; }

                Vector3 before = agentTemplate.transform.position;
                if (agentTemplate.TryPlace(new Vector3(50f, 0f, 50f)) ||
                    Vector3.Distance(before, agentTemplate.transform.position) > 0.01f)
                { Finish(false, cycle - 1, "Invalid spawn was accepted or moved the agent."); yield break; }

                portal.SetOpen(true);
                yield return null;
                if (!HasCompletePath(agentTemplate.transform.position, agentTemplate.InsideGoal))
                { Finish(false, cycle - 1, "Open gate did not connect the surfaces."); yield break; }

                // Request closure during traversal; an occupied passage must not close on the agent.
                float crossingDeadline = Time.time + 8f;
                while (!agentTemplate.Agent.isOnOffMeshLink && Time.time < crossingDeadline) yield return null;
                if (!agentTemplate.Agent.isOnOffMeshLink)
                { Finish(false, cycle - 1, "Agent did not use the portal link."); yield break; }
                portal.SetOpen(false);
                if (!portal.IsOpen || !portal.ClosePending)
                { Finish(false, cycle - 1, "Occupied passage closed immediately."); yield break; }

                yield return WaitForPosition(agentTemplate.InsideGoal, 8f);
                if (Vector3.Distance(agentTemplate.transform.position, agentTemplate.InsideGoal) > 0.5f || portal.IsOpen)
                { Finish(false, cycle - 1, "Crossing or safe closure did not finish."); yield break; }

                portal.SetOpen(true);
                agentTemplate.SetGoal(false);
                yield return WaitForPosition(portal.OutsideApproach, 8f);
                if (Vector3.Distance(agentTemplate.transform.position, portal.OutsideApproach) > 0.5f)
                { Finish(false, cycle - 1, "Return traversal failed."); yield break; }
                portal.SetOpen(false);
                result = $"Acceptance: {cycle}/10 cycles passed.";
            }
            ResetAgents();
            Finish(true, 10, "Closed/open paths, actual crossings, safe closure and invalid spawn checks passed.");
        }

        private IEnumerator WaitForPosition(Vector3 target, float seconds)
        {
            float deadline = Time.time + seconds;
            while (Vector3.Distance(agentTemplate.transform.position, target) > 0.4f && Time.time < deadline)
                yield return null;
        }

        private bool HasCompletePath(Vector3 from, Vector3 to)
        {
            return NavMesh.CalculatePath(from, to, NavMesh.AllAreas, queryPath) &&
                queryPath.status == NavMeshPathStatus.PathComplete;
        }

        private void Finish(bool passed, int cycles, string message)
        {
            checking = false;
            result = (passed ? "PASS: " : "FAIL: ") + cycles + "/10. " + message;
            var report = new AcceptanceReport
            {
                passed = passed, completedCycles = cycles, message = message,
                platform = Application.platform.ToString(), unityVersion = Application.unityVersion,
                utc = DateTime.UtcNow.ToString("O")
            };
#if UNITY_EDITOR
            string reportPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../Logs/NavigationSandboxReport.json"));
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath));
            File.WriteAllText(reportPath, JsonUtility.ToJson(report, true));
#endif
            if (passed) Debug.Log("[NavigationSandbox] " + result);
            else Debug.LogError("[NavigationSandbox] " + result);
        }

        [Serializable]
        private sealed class AcceptanceReport
        {
            public bool passed;
            public int completedCycles;
            public string message;
            public string platform;
            public string unityVersion;
            public string utc;
        }
    }
}
