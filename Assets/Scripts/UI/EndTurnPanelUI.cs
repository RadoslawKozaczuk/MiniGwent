﻿using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    [DisallowMultipleComponent]
    public class EndTurnPanelUI : MonoBehaviour
    {
        [SerializeField] Button _endTurnButton;
        [SerializeField] TextMeshProUGUI _infoText;

        readonly Color32 _cyan = new Color(0, 255, 235, 0); // cyan color
        readonly Color32 _orange = new Color(255, 190, 0, 0); // cyan color
        readonly string _nothingToDo = "Nothing else to do";
        readonly string _aiIsThinking = "AI is thinking";

        Color32 _color;
        bool _pulsating;

        #region Unity life-cycle methods
        // we make a separate copy of a material to be able to set different parameters
        void Awake() => _infoText.material = new Material(_infoText.material);

        void Update()
        {
            if (_pulsating)
                _infoText.faceColor = new Color32(_color.r, _color.g, _color.b, PulsationFunction());
        }
        #endregion

        public void SetReady()
        {
            _color = _cyan;

            _endTurnButton.interactable = true;
            _pulsating = true;
            _infoText.gameObject.SetActive(true);

            _infoText.text = _nothingToDo;
        }

        public void SetAiThinking()
        {
            _color = _orange;

            _endTurnButton.interactable = false;
            _pulsating = true;
            _infoText.gameObject.SetActive(true);

            _infoText.text = _aiIsThinking;
        }

        public void SetOff()
        {
            _endTurnButton.interactable = false;
            _pulsating = false;
            _infoText.gameObject.SetActive(false);
        }

        public void EndTurnAction() => GameEngine.Instance.HandleEndTurnAction();

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