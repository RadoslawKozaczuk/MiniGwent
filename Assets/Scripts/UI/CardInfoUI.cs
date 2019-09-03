using Assets.Core.DataModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    [DisallowMultipleComponent]
    public class CardInfoUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _title;
        [SerializeField] TextMeshProUGUI _description;
        [SerializeField] TextMeshProUGUI _strength;
        [SerializeField] Image _portrait;

        public void SetInfoForCard(CardUI card)
        {
            CardData data = MainUIController.DB[card.Id];
            _title.text = data.Title;
            _description.text = data.Description;
            _portrait.sprite = MainUIController.Icons[card.Id];

            UpdateStrengthText(card.CurrentStrength, card.DefaultStrength);
        }

        void UpdateStrengthText(int currentStrength, int defaultStrength)
        {
            if (currentStrength < defaultStrength)
                _strength.text = $"STR: <color=red>{currentStrength}</color>/{defaultStrength}";
            else if (currentStrength == defaultStrength)
                _strength.text = $"STR: {currentStrength}/{defaultStrength}";
            else
                _strength.text = $"STR: <color=green>{currentStrength}</color>/{defaultStrength}";
        }
    }
}