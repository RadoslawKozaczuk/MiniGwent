using Assets.Core;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    class WelcomePanelUI : MonoBehaviour
    {
        public (PlayerIndicator position, PlayerControl control) TopPlayer = (PlayerIndicator.Top, PlayerControl.AI);
        public (PlayerIndicator position, PlayerControl control) BotPlayer;

        [SerializeField] TMP_Dropdown _botPlayerDropdown;
        [SerializeField] MainUIController _mainUIController;

        public void ChangeBotPlayer(int id) 
            => BotPlayer = (PlayerIndicator.Bot, id == 0 ? PlayerControl.Human : PlayerControl.AI);

        void Start() => BotPlayer = (PlayerIndicator.Bot, _botPlayerDropdown.value == 0 ? PlayerControl.Human : PlayerControl.AI);

        public void StartGame()
        {
            gameObject.SetActive(false);
            _mainUIController.StartGame();
        }
    }
}
