using Assets.Core.DataModel;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Assets.Core
{
    public sealed class GameLogicStatusChangedEventArgs
    {
        public string CurrentStatus;
        public string CurrentStrength;
    }

    public class GameLogic
    {
        const int NUMBER_OF_CARDS_IN_DECK = 5;

        /// <summary>
        /// Subscribe to this event to receive notifications each time resource number has changed.
        /// </summary>
        public static event EventHandler<GameLogicStatusChangedEventArgs> GameLogicStatusChangedEventHandler;

        List<CardModel> TopDeck = new List<CardModel>();
        List<CardModel> TopBackline = new List<CardModel>();
        List<CardModel> TopFrontline = new List<CardModel>();
        List<CardModel> BotFrontline = new List<CardModel>();
        List<CardModel> BotBackline = new List<CardModel>();
        List<CardModel> BotDeck = new List<CardModel>();

        readonly List<CardModel>[] _lines;

        int TopStrength;
        int BotStrength;

        internal static readonly DummyDB DB = new DummyDB();

        public GameLogic()
        {
            _lines = new List<CardModel>[6] { TopDeck, TopBackline, TopFrontline, BotFrontline, BotBackline, BotDeck };
        }

        /// <summary>
        /// Removes given card from the line.
        /// CardNumber indicates the card number from left to right. First card from the left is 0.
        /// </summary>
        public void RemoveCardFromLine(LineIndicator targetLine, int cardNumber)
        {
            _lines[(int)targetLine].RemoveAt(cardNumber);

            UpdateStrengths();
            BroadcastGameLogicStatusChanged();
        }

        public void MoveCard(LineIndicator fromLine, int fromSlotNumber, LineIndicator targetLine, int targetSlotNumber)
        {
            #region Assertions
#if UNITY_EDITOR
            if (fromSlotNumber < 0 || fromSlotNumber > _lines[(int)fromLine].Count)
                throw new System.ArgumentOutOfRangeException(
                    "fromSlotNumber", 
                    "argument cannot be lower than 0 or greater than the number of cards in the line.");

            if (targetSlotNumber < 0 || targetSlotNumber > _lines[(int)targetLine].Count)
                throw new System.ArgumentOutOfRangeException(
                    "targetSlotNumber", 
                    "argument cannot be lower than 0 or greater than the number of cards in the line.");

            if (targetLine == LineIndicator.TopDeck || targetLine == LineIndicator.BotDeck)
                throw new System.ArgumentException("Moving card to a deck is an invalid move.");

            if (fromLine == targetLine)
                throw new System.ArgumentException("Moving card to the same line is an invalid move.");
#endif
            #endregion

            List<CardModel> fLine = _lines[(int)fromLine];
            CardModel card = fLine[fromSlotNumber];

            List<CardModel> tLine = _lines[(int)targetLine];
            card.SlotNumber = tLine.Count;

            fLine.RemoveAt(fromSlotNumber);

            // all cards on the right must have their NumberInLine reduced by one
            var cardsOnTheRight = fLine.TakeLast(fLine.Count - fromSlotNumber - 1).ToList();
            cardsOnTheRight.ForEach(c => c.SlotNumber--);

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
        /// <param name="line"></param>
        /// <returns></returns>
        public List<CardModel> SpawnRandomDeck(LineIndicator line)
        {
            // assert line is bot or top

            var deck = new List<CardModel>(NUMBER_OF_CARDS_IN_DECK);
            for (int i = 0; i < NUMBER_OF_CARDS_IN_DECK; i++)
            {
                CardModel card = new CardModel(UnityEngine.Random.Range(0, DB.Length)) { SlotNumber = i };
                _lines[(int)line].Add(card);
                deck.Add(card);
            }

            UpdateStrengths();
            BroadcastGameLogicStatusChanged();

            return new List<CardModel>(deck);
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
            TopStrength = TopBackline.Sum(c => c.Strength) + TopFrontline.Sum(c => c.Strength);
            BotStrength = BotBackline.Sum(c => c.Strength) + BotFrontline.Sum(c => c.Strength);
        }

        string GetCurrentStrength() 
            => $"Top Strength: {TopStrength} \nBot Strength: {BotStrength}";

        // we call the event - if there is no subscribers we will get a null exception error therefore we use a safe call (null check)
        void BroadcastGameLogicStatusChanged() 
            => GameLogicStatusChangedEventHandler?.Invoke(
                this,
                new GameLogicStatusChangedEventArgs()
                {
                    CurrentStatus = GetCurrentStatus(),
                    CurrentStrength = GetCurrentStrength()
                });
    }
}
