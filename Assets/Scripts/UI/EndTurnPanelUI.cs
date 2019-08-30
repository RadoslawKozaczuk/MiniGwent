using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    [DisallowMultipleComponent]
    class EndTurnPanelUI : MonoBehaviour
    {
        bool _ready;
        public bool Ready
        {
            get => _ready;
            set
            {
                _ready = value;
                _endTurnButton.enabled = value;
                _pulsating = value;
                _infoText.gameObject.SetActive(value);
            }
        }

        [SerializeField] Button _endTurnButton;
        [SerializeField] TextMeshProUGUI _infoText;

        readonly Color32 _cyanColor = new Color(0, 255, 235, 100); // cyan color
        bool _pulsating;

        // we make a separate copy of a material to be able to set different parameters
        void Awake() => _infoText.material = new Material(_infoText.material);

        void Update()
        {
            if (_pulsating)
                _infoText.faceColor = new Color32(_cyanColor.r, _cyanColor.g, _cyanColor.b, PulsationFunction());
        }

        /// <summary>
        /// Returns values from 55 to 255 with interval of 1.57s
        /// </summary>
        byte PulsationFunction()
        {
            float sin = (Mathf.Sin(Time.time * 4) + 1) * 100 + 55; // from 55 to 255
            return (byte)Mathf.FloorToInt(sin);
        }
    }
}
