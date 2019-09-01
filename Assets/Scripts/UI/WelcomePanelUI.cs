using Assets.Core;
using UnityEngine;

namespace Assets.Scripts.UI
{
    class WelcomePanelUI : MonoBehaviour
    {
        public (PlayerIndicator position, PlayerControl control) TopPlayer = (PlayerIndicator.Top, PlayerControl.AI);
        public (PlayerIndicator position, PlayerControl control) BotPlayer;

        public void ChangeBotPlayer(int id) 
            => BotPlayer = (PlayerIndicator.Bot, id == 0 ? PlayerControl.Human : PlayerControl.AI);

        public void StartGame()
        {

        }
    }
}
