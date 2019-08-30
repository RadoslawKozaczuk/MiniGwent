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

        public void LoadDataForId(int id)
        {
            CardData data = GameEngine.DB[id];
            _title.text = data.Title;
            _description.text = data.Description;
            _strength.text = "STR: " + data.Strength;
            _portrait.sprite = GameEngine.Icons[id];
        }
    }
}