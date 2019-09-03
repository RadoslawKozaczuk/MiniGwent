using Assets.Core;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    [DisallowMultipleComponent]
    class StatusPanelUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _statusText;
        [SerializeField] TextMeshProUGUI _executionStackText;

        // subscribe to GameLogic
        void Awake()
        {
            GameLogic.GameLogicStatusChangedEventHandler += HandleGameLogicStatusChanged;
            _statusText.text = "";
            _executionStackText.text = "";
        }

        void HandleGameLogicStatusChanged(object sender, GameLogicStatusChangedEventArgs eventArgs)
        {
            if (!string.IsNullOrEmpty(eventArgs.CurrentStatus))
                _statusText.text
                    = "Game Logic Internal Status:"
                    + $"\n{eventArgs.CurrentStatus}"
                    + $"\nTop Strength: " + eventArgs.TopTotalStrength
                    + $"\nBot Strength: " + eventArgs.BotTotalStrength;

            if (!string.IsNullOrEmpty(eventArgs.LastExecutedCommand))
            {
                _executionStackText.text = string.IsNullOrEmpty(_executionStackText.text)
                    ? "-> " + eventArgs.LastExecutedCommand
                    : _executionStackText.text + $"\n-> " + eventArgs.LastExecutedCommand;
            }
        }
    }
}
