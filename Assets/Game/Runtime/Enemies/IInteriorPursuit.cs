using UnityEngine;

namespace WaitYourTurn.Enemies
{
    /// <summary>Optional onboard navigation domain; external entry and pool ownership stay unchanged.</summary>
    public interface IInteriorPursuit
    {
        uint Revision { get; }
        bool Contains(Vector3 point);
        Vector3 Constrain(Vector3 point, float radius);
    }
}
