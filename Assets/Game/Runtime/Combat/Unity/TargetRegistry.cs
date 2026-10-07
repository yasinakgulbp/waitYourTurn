using System.Collections.Generic;
using UnityEngine;

namespace WaitYourTurn.Combat
{
    /// <summary>Explicit live targets. No scene-wide Find calls, collider scans or static state.</summary>
    public sealed class TargetRegistry : MonoBehaviour
    {
        private readonly List<HealthComponent> targets = new List<HealthComponent>(16);
        public IReadOnlyList<HealthComponent> Targets => targets;
        public void Register(HealthComponent target) { if (!targets.Contains(target)) targets.Add(target); }
        public void Unregister(HealthComponent target) => targets.Remove(target);
    }
}
