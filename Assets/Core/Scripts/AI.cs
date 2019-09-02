﻿using Assets.Core.CardSkills;
using Assets.Core.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Core
{
    sealed class AI
    {
        const int MIN_THINKING_TIME = 1000;
        const int MAX_THINKING_TIME = 2000;

        string MyIndicatorToStr => _myIndicator == PlayerIndicator.Top ? "Top" : "Bot";

        readonly PlayerIndicator _myIndicator;
        readonly List<CardModel> _myDeck;
        readonly List<CardModel> _myBackline;
        readonly List<CardModel> _myFrontline;
        readonly GameLogic _gameLogic;
        readonly bool _fakeThinking;

        // each turn AI creates a list of tasks and then executes them one by one each time waiting for the upper logic response
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

            Task<MoveData> task = CalculateBestMove(); // this already starts the task
            await Task.WhenAll(task);

            MoveData move = task.Result;
            _gameLogic.MoveCardForAI(move); // you moved this card
            // upper logic knows nothing at this point yet
            
            // create the rest of the plan
            CardSkill skill = GameLogic.DB[move.Card.CardId].Skill;
            if(skill != null)
            {
                List<SkillTargetData> targets = new List<SkillTargetData>(); // can be empty

                if (skill.ExecutionTime == SkillExecutionTime.OnDeployAutomatic)
                    targets = GetTargetsOnDeployAutomatic(skill, move.TargetLine, move.TargetSlotNumber);
                else if (skill.ExecutionTime == SkillExecutionTime.OnDeployManual)
                {
                    var singleTarget = ChooseBestSingleTarget();
                    if(singleTarget != null)
                        targets.Add(singleTarget);
                }

                // upon execution of this task the upper logic should start playing skill's animation
                _taskQueue.Enqueue(() => PlaySkillVFX(skill, targets));

                // upon execution of this task the upper logic should start applying skill's effect
                _taskQueue.Enqueue(() => ApplySkill(skill, targets));
            }

            // upon execution the upper logic should update card strengths
            _taskQueue.Enqueue(UpdateStrength);

            // upon execution the upper logic should give away the control to other player (human or AI)
            _taskQueue.Enqueue(EndTurn);

            // inform the upper logic about the card move you have done
            _gameLogic.BroadcastMoveCard(move);
        }

        internal void ReturnControl()
        {
            if(_taskQueue.Count > 0)
                _taskQueue.Dequeue().Invoke();
        }

        /// <summary>
        /// If fakeThinking parameter is true, AI will wait random amount of time 
        /// between MIN_THINKING_TIME and MAX_THINKING_TIME measured in milliseconds before the execution continues.
        /// </summary>
        async Task<MoveData> CalculateBestMove()
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
            Debug.Log("AI Move Calculation: MaxSlotNumber " + maxSlotNumber);

            int targetSlotNumber = maxSlotNumber == 0 
                ? 0 
                : UnityEngine.Random.Range(0, maxSlotNumber + 1);

            Debug.Log($"AI {MyIndicatorToStr} came up with a move idea: fSlot:{fromSlotNumber}" 
                + $" tLine:{line.ToString()}" 
                + $" tSlot:{targetSlotNumber}");

            if (_fakeThinking) // pretend it took you some time to come up with such an amazing idea
                await Task.Delay(UnityEngine.Random.Range(MIN_THINKING_TIME, MAX_THINKING_TIME)); 

            return new MoveData(_myDeck[fromSlotNumber], _myIndicator, fromSlotNumber, line, targetSlotNumber);
        }

        /// <summary>
        /// Returns null if there is no targets to chose from.
        /// </summary>
        SkillTargetData ChooseBestSingleTarget()
        {
            List<CardModel> enemyBackline = _gameLogic.GetLine(_myIndicator.Opposite(), PlayerLine.Backline);
            List<CardModel> enemyFrontline = _gameLogic.GetLine(_myIndicator.Opposite(), PlayerLine.Frontline);

            if (enemyBackline.Count > 0 && enemyFrontline.Count > 0)
                return UnityEngine.Random.Range(0, 2) == 0 // both enemy lines contains units so choose one randomly
                    ? CreateTargetData(PlayerLine.Backline, enemyBackline.Count)
                    : CreateTargetData(PlayerLine.Frontline, enemyFrontline.Count);
            else if(enemyBackline.Count > 0)
                return CreateTargetData(PlayerLine.Backline, enemyBackline.Count);
            else if(enemyFrontline.Count > 0)
                return CreateTargetData(PlayerLine.Frontline, enemyFrontline.Count);
            else
                return null;

            SkillTargetData CreateTargetData(PlayerLine line, int count) 
                => new SkillTargetData(
                    _gameLogic.GetLineIndicator(_myIndicator.Opposite(), line), 
                    UnityEngine.Random.Range(0, count));
        }

        List<SkillTargetData> GetTargetsOnDeployAutomatic(CardSkill skill, LineIndicator targetLine, int slotNumber)
        {
            var targets = new List<SkillTargetData>();

            foreach (SkillTarget target in skill.Targets)
                targets.AddRange(ParseOnDeployAutomaticSkillTargetInternal(targetLine, slotNumber, target));

            return targets;
        }

        internal List<SkillTargetData> ParseOnDeployAutomaticSkillTargetInternal(LineIndicator line, int slotNumber, SkillTarget target)
        {
            var targets = new List<SkillTargetData>();
            List<CardModel> cards;

            // corresponding enemy line
            if (target == SkillTarget.CorrespondingEnemyLine)
            {
                cards = _gameLogic.GetLine(line.Opposite());
                for (int i = 0; i < cards.Count; i++)
                    targets.Add(new SkillTargetData(line.Opposite(), i));

                return targets;
            }

            // our line
            if (target == SkillTarget.AllInLineExceptMe)
                cards = _gameLogic.GetLine(line).GetAllExceptOne(slotNumber).ToList();
            else if (target == SkillTarget.LeftNeighbor)
                cards = _gameLogic.GetLine(line).GetLeftNeighbor(slotNumber).ToList();
            else if (target == SkillTarget.BothNeighbors)
                cards = _gameLogic.GetLine(line).GetBothNeighbors(slotNumber).ToList();
            else if (target == SkillTarget.RightNeighbor)
                cards = _gameLogic.GetLine(line).GetRightNeighbor(slotNumber).ToList();
            else
                throw new Exception("Unreachable code reached! "
                    + "CardSkillTarget enumerator must have been extended without extending the ParseCardSkillTarget function.");

            foreach(CardModel model in cards)
                targets.Add(new SkillTargetData(line, model.SlotNumber));

            return targets;
        }

        void PlaySkillVFX(CardSkill skill, List<SkillTargetData> targets)
        {
            //Debug.Log($"AI {MyIndicator} invoked PlaySkillVFX action");
            _gameLogic.BroadcastPlaySkillVFX(targets, skill.VisualEffect);
        }

        void ApplySkill(CardSkill skill, List<SkillTargetData> targets)
        {
            //Debug.Log($"AI {MyIndicator} invoked ApplySkill action");
            _gameLogic.ApplySkillEffectForAI(skill, targets);
            _gameLogic.BroadcastUpdateStrength();
        }

        void UpdateStrength()
        {
            //Debug.Log($"AI {MyIndicator} invoked UpdateStrength action");
            _gameLogic.BroadcastUpdateStrength();
        }

        void EndTurn()
        {
            Debug.Log($"AI {MyIndicatorToStr} invoked EndTurn action");
            _gameLogic.EndTurnMsgSent = true; // indicates that AI is done
            _gameLogic.BroadcastEndTurn();
        }
    }
}
