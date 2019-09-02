using UnityEngine;

namespace Assets.VFX
{
    internal class FireBallVFX : ParticleEffect
    {
        [SerializeField] ParticleSystem _flash;
        [SerializeField] ParticleSystem _smoke;
        [SerializeField] ParticleSystem _spark;

        internal override void PlayAllEffects()
        {
            _flash.Play();
            _smoke.Play();
            _spark.Play();
        }
    }
}