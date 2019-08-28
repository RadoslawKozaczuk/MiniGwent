using Assets.Core.DataModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Core
{
    public sealed class GameLogicStatusChangedEventArgs
    {
        public string CurrentStatus;
        public string CurrentStrength;
    }

    public class GameLogic
    {
        const int NUMBER_OF_CARDS_IN_DECK = 6;

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

        readonly DummyDB DB = new DummyDB();

        public GameLogic()
        {
            _lines = new List<CardModel>[6] { TopDeck, TopBackline, TopFrontline, BotFrontline, BotBackline, BotDeck };
        }

        public void AddCardToLine(LineIndicator targetLine, CardModel card)
        {
            _lines[(int)targetLine].Add(card);

            BroadcastGameLogicStatusChanged();
        }

        /// <summary>
        /// Removes given card from the line.
        /// CardNumber indicates the card number from left to right. First card from the left is 0.
        /// </summary>
        public void RemoveCardFromLine(LineIndicator targetLine, int cardNumber)
        {
            _lines[(int)targetLine].RemoveAt(cardNumber);

            BroadcastGameLogicStatusChanged();
        }

        public void MoveCard(LineIndicator fromLine, int fromCardNumber, LineIndicator targetLine)
        {
            CardModel card = _lines[(int)fromLine][fromCardNumber];

            RemoveCardFromLine(fromLine, fromCardNumber);
            AddCardToLine(targetLine, card);
        }

        public List<CardModel> SpawnRandomDeck(LineIndicator line)
        {
            // assert line is bot or top

            var deck = new List<CardModel>(NUMBER_OF_CARDS_IN_DECK);
            for (int i = 0; i < NUMBER_OF_CARDS_IN_DECK; i++)
            {
                CardModel card = GetRandomCard();
                AddCardToLine(line, card);
                deck.Add(card);
            }

            return new List<CardModel>(deck);
        }

        CardModel GetRandomCard()
        {
            int id = UnityEngine.Random.Range(0, DB.Length);
            CardData data = DB[id];
            return new CardModel(id, data.Strength);
        }

        string GetCurrentStatus()
        {
            var sb = new StringBuilder();

            for (int i = 0; i < _lines.Length; i++)
            {
                sb.Append($"{i + 1}. ");
                foreach (CardModel c in _lines[i])
                    sb.Append(c.ToString());
                sb.AppendLine();
            }

            return sb.ToString();
        }

        string GetCurrentStrength() => $"Top: {TopStrength} Bot: {BotStrength}";

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
