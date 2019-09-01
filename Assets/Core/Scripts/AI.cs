using Assets.Core.CardSkills;
using Assets.Core.DataModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Core
{
    sealed class AI
    {
        const int MIN_THINKING_TIME = 1000;
        const int MAX_THINKING_TIME = 2000;

        string MyIndicator => _myIndicator == PlayerIndicator.Top ? "Top" : "Bot";

        readonly PlayerIndicator _myIndicator;
        readonly List<CardModel> _myDeck;
        readonly List<CardModel> _myBackline;
        readonly List<CardModel> _myFrontline;
        readonly GameLogic _gameLogic;
        readonly bool _fakeThinking;

        // things to do
        readonly Queue<Action> _taskQueue = new Queue<Action>();

        internal AI(PlayerIndicator player, List<CardModel> deck, List<CardModel> backline, 
            List<CardModel> frontline, GameLogic gameLogic, bool fakeThinking)
        {
            _myIndicator = player;
            _myDeck = deck;
            _myBackline = backline;
            _myFrontline = frontline;
            _gameLogic = gameLogic;
            _fakeThinking = fakeThinking;
        }

        internal async void StartAITurn()
        {
            _taskQueue.Clear();

            Task<MoveData> task = CalculateBestMove(); // this already starts

            await Task.WhenAll(task);

            MoveData move = task.Result;
            _gameLogic.MoveCardForAI(move); // you moved this card
            // upper logic knows nothing at this point
            
            // create the rest of the plan
            CardSkill skill = GameLogic.DB[move.Card.CardId].Skill;
            if(skill != null)
            {
                _taskQueue.Enqueue(() => PlaySkillVFX(skill, move.TargetLine, move.TargetSlotNumber));
                // upper logic should play skill animation now

                _taskQueue.Enqueue(() => ApplySkill(skill, move.TargetLine, move.TargetSlotNumber));
                // upper logic should 
            }

            _taskQueue.Enqueue(UpdateStrength);
            _taskQueue.Enqueue(EndTurn);

            // inform the upper logic about this move
            _gameLogic.BroadcastMoveCard(move);
        }

        public void ControlReturn()
        {
            if(_taskQueue.Count > 0)
                _taskQueue.Dequeue().Invoke();
        }

        /// <summary>
        /// If fakeThinking parameter is set to true, AI will wait certain amount of time [1-2]s before the execution continues.
        /// </summary>
        internal async Task<MoveData> CalculateBestMove()
        {
            // How to make a game-changing move in 3 steps:
            // 1. choose a random card from your deck
            int fromSlotNumber = UnityEngine.Random.Range(0, _myDeck.Count);

            // 2. excellent choice, now pick randomly a line
            PlayerLine line = UnityEngine.Random.Range(0, 2) == 0 
                ? PlayerLine.Backline 
                : PlayerLine.Frontline;
             
            // 3. brilliant! Finally choose a random slot and play that card there
            int maxSlotNumber = line == PlayerLine.Backline ? _myBackline.Count : _myFrontline.Count;
            int targetSlotNumber = UnityEngine.Random.Range(0, maxSlotNumber + 1);

            if (_fakeThinking) // pretend it took you some time to come up with such an amazing idea
                await Task.Delay(UnityEngine.Random.Range(MIN_THINKING_TIME, MAX_THINKING_TIME)); 

            return new MoveData(_myDeck[fromSlotNumber], _myIndicator, fromSlotNumber, line, targetSlotNumber);
        }

        internal void PlaySkillVFX(CardSkill skill, Line line, int slotNumber)
        {
            Debug.Log($"AI {MyIndicator} invoked PlaySkillVFX action");

            _gameLogic.BroadcastPlaySkillVFX((line, slotNumber), skill);
        }

        internal void ApplySkill(CardSkill skill, Line line, int slotNumber)
        {
            Debug.Log($"AI {MyIndicator} invoked ApplySkill action");

            _gameLogic.ApplySkillEffectForAI(skill, line, slotNumber);
            _gameLogic.BroadcastUpdateStrength();
        }

        internal void UpdateStrength()
        {
            Debug.Log($"AI {MyIndicator} invoked UpdateStrength action");

            _gameLogic.BroadcastUpdateStrength();
        }

        internal void EndTurn()
        {
            Debug.Log($"AI {MyIndicator} invoked EndTurn action");

            _gameLogic.BroadcastEndTurn();
        }
    }
}
