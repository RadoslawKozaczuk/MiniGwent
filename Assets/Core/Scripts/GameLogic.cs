using Assets.Core.DataModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using UnityEngine;
using Assets.Core.CardSkills;
using System.Threading.Tasks;

namespace Assets.Core
{ 
    [DisallowMultipleComponent]
    public class GameLogic : MonoBehaviour
    {
        public const int MAX_NUMBER_OF_CARDS_IN_LINE = 10;
        const int NUMBER_OF_CARDS_IN_DECK = 6;

        /// <summary>
        /// Subscribe to this event to receive notifications each time resource number has changed.
        /// </summary>
        public static event EventHandler<GameLogicStatusChangedEventArgs> GameLogicStatusChangedEventHandler;

        readonly List<CardModel> TopDeck = new List<CardModel>();
        readonly List<CardModel> TopBackline = new List<CardModel>();
        readonly List<CardModel> TopFrontline = new List<CardModel>();
        readonly List<CardModel> BotFrontline = new List<CardModel>();
        readonly List<CardModel> BotBackline = new List<CardModel>();
        readonly List<CardModel> BotDeck = new List<CardModel>();

        readonly List<CardModel>[] _lines;

        public MoveData LastAIMove;

        int TopStrength;
        int BotStrength;

        internal static readonly DummyDB DB = new DummyDB();

        bool _blockExternalCalls; // when AI is moving 
        readonly AI _ai;
        bool _isDirty;

        bool _waitingForUpperLogicResponse;
        public bool WaitingForUpperLogicResponse
        {
            get => _waitingForUpperLogicResponse;
            set
            {
                _waitingForUpperLogicResponse = value;
            }
        }

        public GameLogic()
        {
            _lines = new List<CardModel>[6] { TopDeck, TopBackline, TopFrontline, BotFrontline, BotBackline, BotDeck };
            _ai = new AI(PlayerIndicator.Top, TopDeck, TopBackline, TopFrontline, this, true);
        }

        void Update()
        {
            if (_waitingForUpperLogicResponse)
                return;

            // proceed with something
        }

        void LateUpdate()
        {
            if(_isDirty)
            {
                UpdateStrengths();
                BroadcastGameLogicStatusChanged();

                _isDirty = false;
            }
        }

        public void StartAITurn()
        {
            _ai.StartAITurn();
        }

        public void ControlReturn()
        {
            _ai.ControlReturn();
            Debug.Log("ControlReturn");
        }

        /// <summary>
        /// Removes given card from the line.
        /// CardNumber indicates the card number from left to right. First card from the left is 0.
        /// </summary>
        public void RemoveCardFromLine(Line targetLine, int cardNumber)
        {
            _lines[(int)targetLine].RemoveAt(cardNumber);
            _isDirty = true;
        }

        /// <summary>
        /// This is for AI
        /// </summary>
        internal void MoveCardAI(PlayerIndicator player, int fromSlotNumber, PlayerLine targetLine, int targetSlotNumber)
        {
            // AI use abstraction so we need these values to be mapped
            Line fLine = MapPlayerLine(player, PlayerLine.Deck);
            Line tLine = MapPlayerLine(player, targetLine);

            LastAIMove = new MoveData(fLine, fromSlotNumber, tLine, targetSlotNumber);
            MoveCardForUI(fLine, fromSlotNumber, tLine, targetSlotNumber);
        }

        internal Line MapPlayerLine(PlayerIndicator player, PlayerLine line)
        {
            if (player == PlayerIndicator.Top)
                switch(line)
                {
                    case PlayerLine.Deck: return Line.TopDeck;
                    case PlayerLine.Backline: return Line.TopBackline;
                    case PlayerLine.Frontline: return Line.TopFrontline;
                }
            else if(player == PlayerIndicator.Bot)
                switch (line)
                {
                    case PlayerLine.Deck: return Line.BotDeck;
                    case PlayerLine.Backline: return Line.BotBackline;
                    case PlayerLine.Frontline: return Line.BotFrontline;
                }

            throw new Exception("Unrecognized parameter. "
                + "Either Player or PlayerLine enumerator has been extended without extending the mapping function.");
        }

        public void MoveCardForUI(Line fromLine, int fromSlotNumber, Line targetLine, int targetSlotNumber)
        {
            #region Assertions
#if UNITY_EDITOR
            MoveDataAssertions(fromLine, fromSlotNumber, targetLine, targetSlotNumber);
#endif
            #endregion

            List<CardModel> fLine = _lines[(int)fromLine];
            List<CardModel> tLine = _lines[(int)targetLine];

            CardModel card = fLine[fromSlotNumber];
            card.SlotNumber = targetSlotNumber;

            fLine.RemoveAt(fromSlotNumber);

            // all cards on the right must have their SlotNumber reduced by one
            fLine.AllOnTheRight(fromSlotNumber, c => c.SlotNumber--);
            tLine.AllOnTheRight(targetSlotNumber, c => c.SlotNumber++);

            if (targetSlotNumber == tLine.Count)
                tLine.Add(card);
            else
                tLine.Insert(targetSlotNumber, card);

            ApplySkillEffectIfAny(card, targetLine, targetSlotNumber);

            // it's important to evaluate strengths before RemoveDeadOnes is called
            // as we want our data match the upper logic's data 
            // UI removes its elements later on
            List<List<int>> cardStrengths = GetCardStrengths();

            // remove dead ones
            RemoveDeadOnes();

            //_isDirty = true; // not needed here

            // inform upper logic about new strengths
            BroadcastUpdateStrength(cardStrengths);
        }

        public void MoveCardForAI(MoveData moveData)
        {
            Line fromLine = moveData.FromLine;
            int fromSlotNumber = moveData.FromSlotNumber;
            Line targetLine = moveData.TargetLine;
            int targetSlotNumber = moveData.TargetSlotNumber;

            #region Assertions
#if UNITY_EDITOR
            MoveDataAssertions(fromLine, fromSlotNumber, targetLine, targetSlotNumber);
#endif
            #endregion

            List<CardModel> fLine = _lines[(int)fromLine];
            List<CardModel> tLine = _lines[(int)targetLine];

            CardModel card = fLine[fromSlotNumber];
            card.SlotNumber = targetSlotNumber;

            fLine.RemoveAt(fromSlotNumber);

            // all cards on the right must have their SlotNumber reduced by one
            fLine.AllOnTheRight(fromSlotNumber, c => c.SlotNumber--);
            tLine.AllOnTheRight(targetSlotNumber, c => c.SlotNumber++);

            if (targetSlotNumber == tLine.Count)
                tLine.Add(card);
            else
                tLine.Insert(targetSlotNumber, card);

            _isDirty = true;
        }

        void RemoveDeadOnes()
        {
            for (int i = 1; i < _lines.Count() - 1; i++) // we omit top and bot decks
            {
                List<CardModel> line = _lines[i];
                for (int j = 0; j < line.Count; j++)
                    if (line[j].CurrentStrength <= 0)
                        line.RemoveAt(j--);
            }
        }

        void ApplySkillEffectIfAny(CardModel card, Line deployLine, int slotNumber)
        {
            CardSkill skill = DB[card.CardId].Skill;
            if (skill != null)
            {
                var targets = new List<CardModel>();

                foreach (SkillTarget target in skill.Targets)
                    targets.AddRange(ParseCardSkillTarget(deployLine, slotNumber, target));

                foreach (CardModel target in targets)
                    skill.Effect(target);
            }
        }

        internal void ApplySkillEffectForAI(CardSkill skill, Line deployLine, int slotNumber)
        {
            var targets = new List<CardModel>();

            foreach (SkillTarget target in skill.Targets)
                targets.AddRange(ParseCardSkillTarget(deployLine, slotNumber, target));

            foreach (CardModel target in targets)
                skill.Effect(target);
        }

        List<CardModel> ParseCardSkillTarget(Line targetLine, int targetSlotNumber, SkillTarget target)
        {
            if (target == SkillTarget.CorrespondingEnemyLine)
            {
                int correspondingLineID = 5 - (int)targetLine;
                return _lines[correspondingLineID];
            }

            List<CardModel> cards = _lines[(int)targetLine];
            if (target == SkillTarget.AllInLineExceptMe)
                return cards.GetAllExceptOne(targetSlotNumber).ToList();
            if (target == SkillTarget.LeftNeighbor)
                return cards.GetLeftNeighbor(targetSlotNumber).ToList();
            if (target == SkillTarget.BothNeighbors)
                return cards.GetBothNeighbors(targetSlotNumber).ToList();
            if (target == SkillTarget.RightNeighbor)
                return cards.GetRightNeighbor(targetSlotNumber).ToList();

            throw new Exception("Unreachable code reached! "
                + "CardSkillTarget enumerator must have been extended without extending the ParseCardSkillTarget method logic.");
        }

        /// <summary>
        /// For safety reasons it returns a copy of the list.
        /// </summary>
        public List<CardModel> SpawnRandomDeck(PlayerIndicator player)
        {
            List<CardModel> targetLine = _lines[player == PlayerIndicator.Top ? 0 : 5];
            for (int i = 0; i < NUMBER_OF_CARDS_IN_DECK; i++)
            {
                int cardType = UnityEngine.Random.Range(0, DB.Length);
                var card = new CardModel(cardType, player) { SlotNumber = i };
                targetLine.Add(card);
            }

            _isDirty = true;

            return new List<CardModel>(targetLine); // encapsulation
        }

        string GetCurrentStatus()
        {
            var sb = new StringBuilder();

            for (int i = 0; i < _lines.Length; i++)
            {
                sb.Append($"{i + 1}. ");
                foreach (CardModel c in _lines[i])
                    sb.Append(c);
                sb.AppendLine();
            }

            return sb.ToString();
        }

        void UpdateStrengths()
        {
            TopStrength = TopBackline.Sum(c => c.CurrentStrength) + TopFrontline.Sum(c => c.CurrentStrength);
            BotStrength = BotBackline.Sum(c => c.CurrentStrength) + BotFrontline.Sum(c => c.CurrentStrength);
        }

        // we call the event - if there is no subscribers we will get a null exception error therefore we use a safe call (null check)
        void BroadcastGameLogicStatusChanged()
        {
            Debug.Log("GameLogic broadcasted the event.");

            // create a list of lists
            var currentStrengths = new List<List<int>>(4); // size of 4 because we don't need to send decks
            for (int i = 1; i < _lines.Length - 1; i++)
            {
                var line = new List<int>();
                _lines[i].ForEach(model => line.Add(model.CurrentStrength));
                currentStrengths.Add(line);
            }

            //var args = new GameLogicStatusChangedEventArgs(GetCurrentStatus(), TopStrength, BotStrength, currentStrengths);

            //LastAIMove = null;

            //// sprawdz czy zostaly ruchy
            //if (TopDeck.Count == 0 && BotDeck.Count == 0)
            //{
            //    // game over
            //    args.MessageType = GameLogicMessageType.GameOver;
            //}
            //else
            //{
            //    args.MessageType = GameLogicMessageType.CardMoved;
            //}

            //GameLogicStatusChangedEventHandler?.Invoke(this, args);
        }

        public List<List<int>> GetCardStrengths()
        {
            var currentStrengths = new List<List<int>>(4); // size of 4 because we don't need to send decks
            for (int i = 1; i < _lines.Length - 1; i++)
            {
                var line = new List<int>();
                _lines[i].ForEach(model => line.Add(model.CurrentStrength));
                currentStrengths.Add(line);
            }

            return currentStrengths;
        }

        // we call the event - if there is no subscribers we will get a null exception error therefore we use a safe call (null check)
        internal void BroadcastMoveCard(MoveData move) 
            => GameLogicStatusChangedEventHandler?.Invoke(
                this, 
                new GameLogicStatusChangedEventArgs(GameLogicMessageType.MoveCard) { LastMove = move });

        internal void BroadcastPlaySkillVFX((Line targetLine, int targetSlot) skillTarget, CardSkill skill)
            => GameLogicStatusChangedEventHandler?.Invoke(
                this, 
                new GameLogicStatusChangedEventArgs(GameLogicMessageType.PlaySkillVFX)
                {
                    SkillTarget = skillTarget,
                    Skill = skill
                });

        // early evaluation
        internal void BroadcastUpdateStrength(List<List<int>> cardStrengths)
            => GameLogicStatusChangedEventHandler?.Invoke(
                this,
                new GameLogicStatusChangedEventArgs(GameLogicMessageType.UpdateStrength) { CardStrengths = cardStrengths });

        // late evaluation
        internal void BroadcastUpdateStrength()
            => GameLogicStatusChangedEventHandler?.Invoke(
                this,
                new GameLogicStatusChangedEventArgs(GameLogicMessageType.UpdateStrength) { CardStrengths = GetCardStrengths() });

        internal void BroadcastEndTurn()
            => GameLogicStatusChangedEventHandler?.Invoke(this, new GameLogicStatusChangedEventArgs(GameLogicMessageType.EndTurn));

        #region Assertions
#if UNITY_EDITOR
        void MoveDataAssertions(Line fromLine, int fromSlotNumber, Line targetLine, int targetSlotNumber)
        {
            if (fromSlotNumber < 0 || fromSlotNumber > _lines[(int)fromLine].Count)
                throw new System.ArgumentOutOfRangeException(
                    "fromSlotNumber",
                    "FromSlotNumber cannot be lower than 0 or greater than the number of cards in the line.");

            if (targetSlotNumber < 0 || targetSlotNumber > _lines[(int)targetLine].Count)
                throw new System.ArgumentOutOfRangeException(
                    "targetSlotNumber",
                    "TargetSlotNumber cannot be lower than 0 or greater than the number of cards in the line.");

            if (targetLine == Line.TopDeck || targetLine == Line.BotDeck)
                throw new System.ArgumentException("Moving card to a deck is not allowed.");

            if (fromLine == targetLine)
                throw new System.ArgumentException("Moving card to the same line is not allowed.");

            if ((fromLine == Line.BotDeck || fromLine == Line.BotBackline || fromLine == Line.BotFrontline)
                &&
                (targetLine == Line.TopDeck || targetLine == Line.TopBackline || targetLine == Line.TopFrontline))
                throw new System.ArgumentException("Moving card from any bot line to any top line is not allowed.");

            if ((fromLine == Line.TopDeck || fromLine == Line.TopBackline || fromLine == Line.TopFrontline)
                &&
                (targetLine == Line.BotDeck || targetLine == Line.BotBackline || targetLine == Line.BotFrontline))
                throw new System.ArgumentException("Moving card from any top line to any bot line is not allowed.");
        }
#endif
        #endregion
    }
}
