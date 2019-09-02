using Assets.Core;
using Assets.Scripts.VFXControllers;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    [DisallowMultipleComponent]
    public class VFXController : MonoBehaviour
    {
        [Tooltip("In milliseconds")]
        [SerializeField] int _vfxDuration = 2500;
        [SerializeField] GameObject[] _effects;

        readonly List<GameObject> _instanciatedEffects = new List<GameObject>();

        /// <summary>
        /// Schedules VFXs.
        /// Number is equal to given list size and the position match corresponding card position.
        /// Effects remain inactive until played.
        /// All effects are of the same given type.
        /// </summary>
        public void ScheduleParticleEffect(List<CardUI> cards, SkillVisualEffect visualEffect)
        {
            foreach (CardUI card in cards)
                InstanciateEffect(card.transform, visualEffect);
        }

        /// <summary>
        /// Plays all scheduled VFXs. After 2.5s all effects disappear.
        /// </summary>
        public async Task PlayScheduledParticleEffects()
        {
            if (_instanciatedEffects.Count == 0)
                return;

            foreach (GameObject instance in _instanciatedEffects)
                instance.SetActive(true);

            await Task.Delay(_vfxDuration);

            foreach (GameObject instance in _instanciatedEffects)
            {
                instance.SetActive(false);
                Destroy(instance);
            }

            _instanciatedEffects.Clear();
        }

        void InstanciateEffect(Transform position, SkillVisualEffect effect)
        {
            var instance = Instantiate(_effects[(int)effect], position);
            instance.SetActive(false);
            _instanciatedEffects.Add(instance);

            // for now there is only cloud to chose from
            GreenCloud cloud = instance.GetComponent<GreenCloud>();
            cloud.Smoke.Play();
            cloud.Flash.Play();
            cloud.Flash2.Play();
        }
    }
}
