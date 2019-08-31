using Assets.Scripts.VFXControllers;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class VFXController : MonoBehaviour
    {
        public GameObject[] Effects;

        void Start()
        {
            
        }

        public void PlayCloud(List<CardUI> positions)
        {
            foreach (CardUI card in positions)
                InstanciateGreenCloud(card.transform);
        }

        void InstanciateGreenCloud(Transform position)
        {
            var instance = Instantiate(Effects[0], position);

            GreenCloud cloud = instance.GetComponent<GreenCloud>();
            cloud.Smoke.Play();
            cloud.Flash.Play();
            cloud.Flash2.Play();
        }
    }
}
