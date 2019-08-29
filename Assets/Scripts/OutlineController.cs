using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    [RequireComponent(typeof(Outline))]
    public class OutlineController : MonoBehaviour
    {
        [SerializeField] Outline _outline;

        Color color = new Color(0, 255, 235, 100); // cyan color

        bool _pulsation;

        public bool ForcedPulsation;
        public bool BigPulsation;

        void Awake() => _outline.effectColor = color;

        void Update()
        {
            if (ForcedPulsation || _pulsation)
            {
                _outline.effectDistance = BigPulsation ? BigSin() : SmallSin();
            }
        }

        Vector2 BigSin()
        {
            // multiplied by 4 for more frequent pulses
            float value = Mathf.Sin(Time.time * 4) + 4f; // from 3 to 5
            return new Vector2(value, value);
        }

        Vector2 SmallSin()
        {
            float value = Mathf.Sin(Time.time * 4) + 1f; // from 0 to 2
            return new Vector2(value, value);
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
