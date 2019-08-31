using Assets.Core;
using Assets.Scripts.VFXControllers;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class VFXController : MonoBehaviour
    {
        const int VFX_DURATION = 2500;

        public GameObject[] Effects;
        readonly List<GameObject> _instanciatedEffects = new List<GameObject>();

        /// <summary>
        /// Schedules VFXs.
        /// Number is equal to given list size and the position match corresponding card position.
        /// Effects remain inactive until played.
        /// All effects are of the same given type.
        /// </summary>
        public void ScheduleParticleEffect(List<CardUI> cards, VisualEffect visualEffect)
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

            await Task.Delay(VFX_DURATION);

            foreach (GameObject instance in _instanciatedEffects)
            {
                instance.SetActive(false);
                Destroy(instance);
            }

            _instanciatedEffects.Clear();
        }

        void InstanciateEffect(Transform position, VisualEffect effect)
        {
            var instance = Instantiate(Effects[(int)effect], position);
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
