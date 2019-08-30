using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    [RequireComponent(typeof(Outline))]
    public class OutlineController : MonoBehaviour
    {
        static Color _cyanColor = new Color(0, 255, 235, 100); // cyan color

        public bool ForcedPulsation;
        public bool BigPulsation;

        [SerializeField] Outline _outline;

        bool _pulsation;

        #region Unity life-cycle methods
        void Awake() => _outline.effectColor = _cyanColor;

        void Update()
        {
            if (ForcedPulsation || _pulsation)
                _outline.effectDistance = BigPulsation ? BigSinus() : SmallSinus();
        }
        #endregion

        public void TurnPulsationOn() => _pulsation = true;

        public void TurnPulsationOff()
        {
            _pulsation = false;
            _outline.effectDistance = Vector2.zero;
        }

        /// <summary>
        /// Returns values from 3 to 5 with period equal to 1.57s
        /// </summary>
        Vector2 BigSinus()
        {
            // multiplied by 4 for more frequent pulses
            float value = Mathf.Sin(Time.time * 4) + 4f; // from 3 to 5
            return new Vector2(value, value);
        }

        /// <summary>
        /// Returns values from 0 to 2 with period equal to 1.57s
        /// </summary>
        Vector2 SmallSinus()
        {
            float value = Mathf.Sin(Time.time * 4) + 1f; // from 0 to 2
            return new Vector2(value, value);
        }
    }
}
