using UnityEngine;

namespace Assets.VFX
{
    internal class HealVFX : ParticleEffect
    {
        [SerializeField] ParticleSystem _bubble;
        [SerializeField] ParticleSystem _flash;
        [SerializeField] ParticleSystem _cube;
        [SerializeField] ParticleSystem _star;

        internal override void PlayAllEffects()
        {
            _bubble.Play();
            _flash.Play();
            _cube.Play();
            _star.Play();
        }
    }
}
