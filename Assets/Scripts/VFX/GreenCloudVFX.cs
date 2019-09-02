using UnityEngine;

namespace Assets.VFX
{
    internal class GreenCloudVFX : ParticleEffect
    {
        [SerializeField] ParticleSystem _smoke;
        [SerializeField] ParticleSystem _flash;
        [SerializeField] ParticleSystem _flash2;

        internal override void PlayAllEffects()
        {
            _smoke.Play();
            _flash.Play();
            _flash2.Play();
        }
    }
}
