using Assets.Core.DataModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Assets.Core
{ 
    public class GameLogic
    {
        const int NUMBER_OF_CARDS_IN_DECK = 5;

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

        AI _ai;

        public GameLogic()
        {
            _lines = new List<CardModel>[6] { TopDeck, TopBackline, TopFrontline, BotFrontline, BotBackline, BotDeck };
            _ai = new AI(Player.Top, TopDeck, TopBackline, TopFrontline, this);
        }

        public void StartAITurn()
        {
            // block external calls
            _ai.MakeMove();
        }

        /// <summary>
        /// Removes given card from the line.
        /// CardNumber indicates the card number from left to right. First card from the left is 0.
        /// </summary>
        public void RemoveCardFromLine(Line targetLine, int cardNumber)
        {
            _lines[(int)targetLine].RemoveAt(cardNumber);

            UpdateStrengths();
            BroadcastGameLogicStatusChanged();
        }

        /// <summary>
        /// this is for AI
        /// </summary>
        public void MoveCard(Player player, int fromSlotNumber, PlayerLine targetLine, int targetSlotNumber)
        {
            // AI use abstraction so we need these values to be mapped
            Line fLine = MapPlayerLine(player, PlayerLine.Deck);
            Line tLine = MapPlayerLine(player, targetLine);

            LastAIMove = new MoveData(fLine, fromSlotNumber, tLine, targetSlotNumber);
            MoveCard(fLine, fromSlotNumber, tLine, targetSlotNumber);
        }

        public Line MapPlayerLine(Player player, PlayerLine line)
        {
            if (player == Player.Top)
                switch(line)
                {
                    case PlayerLine.Deck: return Line.TopDeck;
                    case PlayerLine.Backline: return Line.TopBackline;
                    case PlayerLine.Frontline: return Line.TopFrontline;
                }
            else if(player == Player.Bot)
                switch (line)
                {
                    case PlayerLine.Deck: return Line.BotDeck;
                    case PlayerLine.Backline: return Line.BotBackline;
                    case PlayerLine.Frontline: return Line.BotFrontline;
                }

            throw new Exception("Unrecognized parameter. Either Player or PlayerLine enumerator has been extended without extending the mapping function.");
        }

        public void MoveCard(Line fromLine, int fromSlotNumber, Line targetLine, int targetSlotNumber)
        {
            #region Assertions
#if UNITY_EDITOR
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

            UpdateStrengths();
            BroadcastGameLogicStatusChanged();
        }

        /// <summary>
        /// For safety reasons it returns a copy of the list.
        /// </summary>
        public List<CardModel> SpawnRandomDeck(Line line)
        {
#if UNITY_EDITOR
            if (line != Line.TopDeck && line != Line.BotDeck)
                throw new System.ArgumentException("SpawnRandomDeck method can only target TopDeck or BotDeck lines.", "line");
#endif

            var deck = new List<CardModel>(NUMBER_OF_CARDS_IN_DECK);
            for (int i = 0; i < NUMBER_OF_CARDS_IN_DECK; i++)
            {
                CardModel card = new CardModel(UnityEngine.Random.Range(0, DB.Length)) { SlotNumber = i };
                _lines[(int)line].Add(card);
                deck.Add(card);
            }

            UpdateStrengths();
            BroadcastGameLogicStatusChanged();

            return new List<CardModel>(deck); // encapsulation
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
            GameLogicStatusChangedEventHandler?.Invoke(
                this,
                new GameLogicStatusChangedEventArgs()
                {
                    CurrentStatus = GetCurrentStatus(),
                    OverallTopStrength = TopStrength,
                    OverallBotStrength = BotStrength,
                    LastMove = LastAIMove
                });

            LastAIMove = null;
        }
    }
}
