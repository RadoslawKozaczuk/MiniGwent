using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    [RequireComponent(typeof(Outline))]
    class OutlineController : MonoBehaviour
    {
        [SerializeField] Outline _outline;

        Color color = new Color(0, 255, 235, 141); // cyan color

        bool _pulsation;

        void Awake() => _outline.effectColor = color;

        void Update()
        {
            if (_pulsation)
            {
                // multiplied by two to have more frequent pulses
                float value = (Mathf.Cos(Time.time * 2) + 2.5f) * 2; // from 4.5 to 7
                _outline.effectDistance = new Vector2(value, value);
            }
        }

        public void TurnPulsationOn()
        {
            _pulsation = true;
        }

        public void TurnPulsationOff()
        {
            _pulsation = false;
            _outline.effectDistance = Vector2.zero;
        }
    }
}
