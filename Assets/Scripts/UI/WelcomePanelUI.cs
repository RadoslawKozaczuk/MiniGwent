using Assets.Core;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    class WelcomePanelUI : MonoBehaviour
    {
        [SerializeField] TMP_Dropdown _botPlayerDropdown;
        [SerializeField] MainUIController _mainUIController;

        PlayerControl _botControl; // top is always AI

        public void ChangeBotPlayer(int id) => _botControl = id == 0 ? PlayerControl.Human : PlayerControl.AI;

        void Start() => _botControl = _botPlayerDropdown.value == 0 ? PlayerControl.Human : PlayerControl.AI;

        public void StartGame()
        {
            gameObject.SetActive(false);
            _mainUIController.StartGame(_botControl);
        }
    }
}
