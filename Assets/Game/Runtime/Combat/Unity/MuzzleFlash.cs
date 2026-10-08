using UnityEngine;

namespace WaitYourTurn.Combat
{
    /// <summary>One reusable cosmetic particle hierarchy; never owns projectile or hit logic.</summary>
    public sealed class MuzzleFlash : MonoBehaviour
    {
        [SerializeField] private HitscanWeapon weapon;
        [SerializeField] private ParticleSystem flash;
        public void Configure(HitscanWeapon gun, ParticleSystem particles) { weapon = gun; flash = particles; }
        private void OnEnable() { if (weapon != null) weapon.Fired += OnShot; }
        private void OnDisable()
        { if (weapon != null) weapon.Fired -= OnShot; if (flash != null) flash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); }
        private void OnShot(ShotNotice shot)
        {
            if (shot.Pellet != 0 || flash == null) return;
            flash.transform.position = shot.Start;
            Vector3 direction = shot.End - shot.Start;
            if (direction.sqrMagnitude > .001f) flash.transform.rotation = Quaternion.LookRotation(direction);
            flash.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); flash.Play(true);
        }
    }
}
