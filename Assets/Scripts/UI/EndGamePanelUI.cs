﻿using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    [DisallowMultipleComponent]
    public class EndGamePanelUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _title;
        [SerializeField] TextMeshProUGUI _whoWon;
        [SerializeField] TextMeshProUGUI _score;

        /// <summary>
        /// Make the panel visible on screen as well as fills it with appropriate data based on the parameters given.
        /// </summary>
        public void SetData(int topStrength, int botStrength)
        {
            gameObject.SetActive(true);

            if(topStrength > botStrength)
            {
                // topinio won
                _title.text = "<color=red>Defeat</color>";
                _whoWon.text = "AI killed you!";
                _score.text = $"{botStrength} - <color=orange>{topStrength}</color>";
            }
            else if (topStrength == botStrength)
            {
                // draw
                _title.text = "<color=orange>Draw</color>";
                _whoWon.text = "Both sides equally good.";
                _score.text = $"{botStrength} - {topStrength}";
            }
            else
            {
                // botinio won
                _title.text = "<color=green>Victory</color>";
                _whoWon.text = "Player's Victory!";
                _score.text = $"<color=orange>{botStrength}</color> - {topStrength}";
            }
        }
    }
}