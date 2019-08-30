using Assets.Core.DataModel;
using System.Collections.Generic;

namespace Assets.Core
{
    class AI
    {
        Player _player;
        List<CardModel> _myDeck;
        List<CardModel> _myBackline;
        List<CardModel> _myFrontline;
        GameLogic _gameLogic;

        public AI(Player player, List<CardModel> deck, List<CardModel> backline, List<CardModel> frontline, GameLogic gameLogic)
        {
            _player = player;
            _myDeck = deck;
            _myBackline = backline;
            _myFrontline = frontline;
            _gameLogic = gameLogic;
        }

        public void MakeMove()
        {
            // How to make a game-changing move in 3 steps:
            // 1. choose a random card from deck
            int fromSlotNumber = UnityEngine.Random.Range(0, _myDeck.Count);

            // 2. excellent choice, now pick randomly a line
            PlayerLine line = UnityEngine.Random.Range(0, 2) == 0 
                ? PlayerLine.Backline 
                : PlayerLine.Frontline;
             
            // 3. brilliant! Finally choose a random spot and play that card
            int maxSlotNumber = line == PlayerLine.Backline ? _myBackline.Count : _myFrontline.Count;
            int targetSlotNumber = UnityEngine.Random.Range(0, maxSlotNumber + 1);

            // inform the rest of the world about it
            _gameLogic.MoveCard(_player, fromSlotNumber, line, targetSlotNumber);
        }
    }
}
