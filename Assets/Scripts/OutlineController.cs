using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    [RequireComponent(typeof(Outline))]
    public class OutlineController : MonoBehaviour
    {
        static Color _cyanColor = new Color(0, 255, 235, 100);
        static Color _orange = new Color(255, 190, 0, 255);

        public bool BigPulsation;

        [SerializeField] Outline _outline;

        bool _pulsation;

        #region Unity life-cycle methods
        void Awake() => _outline.effectColor = _cyanColor;

        void Update()
        {
            if (_pulsation)
                _outline.effectDistance = BigPulsation ? BigSinus() : SmallSinus();
        }
        #endregion

        /// <summary>
        /// Turns pulsation on. Cyan by default. Can be red if necessary.
        /// </summary>
        public void TurnPulsationOn(bool orange = false)
        {
            _outline.effectColor = orange ? _orange : _cyanColor;
            _pulsation = true;
        }

        public void TurnPulsationOff()
        {
            _pulsation = false;
            _outline.effectDistance = Vector2.zero;
        }

        /// <summary>
        /// Returns values from 4 to 6 with period equal to 1.57s
        /// </summary>
        Vector2 BigSinus()
        {
            // multiplied by 4 for more frequent pulses
            float value = Mathf.Sin(Time.time * 4) + 5f; // from 4 to 6
            return new Vector2(value, value);
        }

        /// <summary>
        /// Returns values from 1 to 3 with period equal to 1.57s
        /// </summary>
        Vector2 SmallSinus()
        {
            float value = Mathf.Sin(Time.time * 4) + 2f; // from 1 to 3
            return new Vector2(value, value);
        }
    }
}
