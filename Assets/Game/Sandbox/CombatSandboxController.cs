using UnityEngine;
using WaitYourTurn.Combat;

namespace WaitYourTurn.Sandbox
{
    /// <summary>M2 desktop proof only; mobile input, weapons and door repair are separate milestones.</summary>
    public sealed class CombatSandboxController : MonoBehaviour
    {
        [SerializeField] private HealthComponent player;
        [SerializeField] private HealthComponent zombie;
        [SerializeField] private HealthComponent door;
        [SerializeField] private Camera view;
        [SerializeField] private LineRenderer tracer;
        private ulong attackId;
        private float nextShot;
        private float hideTracerAt;
        private int rewardSignals;
        private bool runEnded;
        private string result = "Click the zombie or door to fire the test pistol (2 damage).";

        public void Configure(HealthComponent hero, HealthComponent enemy, HealthComponent gate, Camera camera, LineRenderer line)
        { player = hero; zombie = enemy; door = gate; view = camera; tracer = line; }

        private void OnEnable()
        {
            player.Died += OnPlayerDied;
            zombie.Died += OnZombieDied;
        }
        private void OnDisable()
        {
            player.Died -= OnPlayerDied;
            zombie.Died -= OnZombieDied;
        }
        private void OnPlayerDied(DeathNotice death) { runEnded = true; result = "PLAYER DIED: run-end signal received."; }
        private void OnZombieDied(DeathNotice death) { rewardSignals++; }

        private DamageResult Hit(HealthComponent target, float amount, HealthComponent source)
        {
            DamageResult damage = target.TryApplyDamage(new DamageContext(amount, source.Identity,
                source.Team, ++attackId, target.LifeVersion));
            result = $"{target.name}: {damage.AppliedAmount} damage; killed={damage.Killed}; {damage.Rejection}.";
            return damage;
        }

        private void Update()
        {
            if (tracer.enabled && Time.time >= hideTracerAt) tracer.enabled = false;
            if (!player.IsAlive || !Input.GetMouseButtonDown(0) || Time.time < nextShot) return;
            Vector3 mouse = Input.mousePosition;
            if (mouse.x < 475 && Screen.height - mouse.y < 365) return; // Ignore lab control panel clicks.
            Ray ray = view.ScreenPointToRay(mouse);
            if (!Physics.Raycast(ray, out RaycastHit hit, 100f, Physics.AllLayers, QueryTriggerInteraction.Ignore)) return;
            HealthComponent target = hit.collider.GetComponentInParent<HealthComponent>();
            if (target == null || target == player) return;
            nextShot = Time.time + 0.3f;
            Hit(target, 2f, player);
            tracer.SetPosition(0, player.transform.position + Vector3.up);
            tracer.SetPosition(1, hit.point);
            tracer.enabled = true;
            hideTracerAt = Time.time + 0.12f;
        }

        private void ResetLives()
        {
            player.ResetForSpawn(100, Team.Player);
            zombie.ResetForSpawn(10, Team.Enemy);
            door.ResetForSpawn(10, Team.Neutral);
            runEnded = false;
            rewardSignals = 0;
            tracer.enabled = false;
            result = "New test lives. Old target life versions are no longer valid.";
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(16, 16, 450, 335), GUI.skin.box);
            GUILayout.Label("COMBAT LAB | M2 | Desktop proof");
            GUILayout.Label($"Player: {player.Current}/{player.Maximum} | Zombie: {zombie.Current}/{zombie.Maximum} | Door: {door.Current}/{door.Maximum}");
            GUILayout.Label($"Run ended: {runEnded} | Zombie death/reward signals: {rewardSignals}");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Door -8", GUILayout.Height(30))) Hit(door, 8f, zombie);
            if (GUILayout.Button("Door -2", GUILayout.Height(30))) Hit(door, 2f, zombie);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Player -> 10 HP", GUILayout.Height(30))) Hit(player, Mathf.Max(0, player.Current - 10f), zombie);
            if (GUILayout.Button("Shop heal", GUILayout.Height(30))) result = $"Healed {player.TryShopHeal()} HP.";
            if (GUILayout.Button("Max HP +25", GUILayout.Height(30))) player.ChangeMaxHealth(player.Maximum + 25f);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Kill player", GUILayout.Height(30))) Hit(player, 1000f, zombie);
            if (GUILayout.Button("Reset test lives", GUILayout.Height(30))) ResetLives();
            GUILayout.EndHorizontal();
            GUILayout.Label(result, GUILayout.Height(60));
            GUILayout.Label("Green: player | White: zombie | Red: door. Click white/red targets below the panel.");
            GUILayout.EndArea();
        }
    }
}
