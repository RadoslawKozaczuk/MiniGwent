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

        void Awake()
        {
            GameLogic.GameLogicLogUpdateEventHandler += HandleGameLogicLogUpdate;
            _statusText.text = "";
            _executionStackText.text = "";
        }

        void HandleGameLogicLogUpdate(object sender, GameLogicLogUpdateEventArgs eventArgs)
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
