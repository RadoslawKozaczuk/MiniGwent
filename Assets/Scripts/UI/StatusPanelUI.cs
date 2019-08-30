using Assets.Core;
using System;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    [DisallowMultipleComponent]
    class StatusPanelUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _text;

        // subscribe to GameLogic
        void Awake() => GameLogic.GameLogicStatusChangedEventHandler += HandleGameLogicStatusChanged;

        void HandleGameLogicStatusChanged(object sender, GameLogicStatusChangedEventArgs eventArgs) 
            => _text.text
                = "Game Logic Internal Status:"
                + Environment.NewLine
                + eventArgs.CurrentStatus
                + Environment.NewLine
                + "Top Strength: " + eventArgs.OverallTopStrength
                + Environment.NewLine
                + "Bot Strength: " + eventArgs.OverallBotStrength;
    }
}
