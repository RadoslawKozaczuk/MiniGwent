using TMPro;
using UnityEngine;
using Assets.Core;
using System;

namespace Assets.Scripts.UI
{
    class StatusPanelUI : MonoBehaviour
    {
        public TextMeshProUGUI Text;

        void Awake()
        {
            // subscribe to GameLogic
            GameLogic.GameLogicStatusChangedEventHandler += GameStatusChanged;
        }


        public void GameStatusChanged(object sender, GameLogicStatusChangedEventArgs eventArgs)
        {
            Text.text = eventArgs.CurrentStatus + Environment.NewLine + eventArgs.CurrentStrength;
        }
    }
}
