using Assets.Core.DataModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Core
{
    class AI
    {
        PlayerIndicator _player;
        List<CardModel> _myDeck;
        List<CardModel> _myBackline;
        List<CardModel> _myFrontline;
        GameLogic _gameLogic;

        internal AI(PlayerIndicator player, List<CardModel> deck, List<CardModel> backline, List<CardModel> frontline, GameLogic gameLogic)
        {
            _player = player;
            _myDeck = deck;
            _myBackline = backline;
            _myFrontline = frontline;
            _gameLogic = gameLogic;
        }

        /// <summary>
        /// If fakeThinking parameter is set to true, AI will wait certain amount of time [1-3]s before the execution continues.
        /// </summary>
        internal async void MakeMove(bool fakeThinking = true)
        {
            // How to make a game-changing move in 3 steps:
            // 1. choose a random card from your deck
            int fromSlotNumber = Random.Range(0, _myDeck.Count);

            // 2. excellent choice, now pick randomly a line
            PlayerLine line = Random.Range(0, 2) == 0 
                ? PlayerLine.Backline 
                : PlayerLine.Frontline;
             
            // 3. brilliant! Finally choose a random slot and play that card there
            int maxSlotNumber = line == PlayerLine.Backline ? _myBackline.Count : _myFrontline.Count;
            int targetSlotNumber = Random.Range(0, maxSlotNumber + 1);

            if(fakeThinking)
            {
                // pretend it took you some time to come up with such amazing idea
                int delay = Random.Range(1000, 3000);
                var t = Task.Run(async delegate
                {
                    await Task.Delay(delay);
                    _gameLogic.MoveCard(_player, fromSlotNumber, line, targetSlotNumber);
                });

                await Task.WhenAll(t);
            }
            else
                _gameLogic.MoveCard(_player, fromSlotNumber, line, targetSlotNumber);
        }
    }
}
