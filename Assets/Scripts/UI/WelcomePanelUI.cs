using Assets.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    class WelcomePanelUI : MonoBehaviour
    {
        [SerializeField] TMP_Dropdown _botPlayerDropdown;
        [SerializeField] MainUIController _mainUIController;
        [SerializeField] Toggle _showUIToggle;

        PlayerControl _botControl; // top is always AI

        public void ChangeBotPlayer(int id)
        {
            _botControl = id == 0 ? PlayerControl.Human : PlayerControl.AI;
            if(_botControl == PlayerControl.Human)
            {
                _showUIToggle.interactable = false;
                _showUIToggle.isOn = true;
            }
            else
            {
                _showUIToggle.interactable = true;
            }
        }

        void Start() => _botControl = _botPlayerDropdown.value == 0 ? PlayerControl.Human : PlayerControl.AI;

        public void StartGame()
        {
            gameObject.SetActive(false);
            _mainUIController.StartGame(_botControl, _showUIToggle.isOn);
        }
    }
}
