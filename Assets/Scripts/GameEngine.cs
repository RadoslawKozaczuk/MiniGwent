using Assets.Core;
using Assets.Core.DataModel;
using Assets.Scripts.UI;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    public class GameEngine : MonoBehaviour
    {
        public static GameEngine Instance;

        public LineUI TopDeck;
        public LineUI TopBackline;
        public LineUI TopFrontline;
        public LineUI BotFrontline;
        public LineUI BotBackline;
        public LineUI BotDeck;

        public GameObject CardContainerPrefab;
        public GameObject CardPrefab;
        public CardInfoUI CardInfoPanel;
        public GameObject TargetSlotIndicatorPrefab;

        public RectTransform ObjectDump;

        public Canvas MainCanvas; // everything is here
        public Canvas SecondaryCanvas; // only dragged items are here

        public static readonly DummyDB DB = new DummyDB();

        public static Sprite[] Icons;

        public static GameLogic GameLogic = new GameLogic();

        public static CardUI CardBeingDraged; // this is used as a condition for lines whether they suppose to pulsate or not

        LineUI[] _lines;

        void Awake()
        {
            Instance = this;
            Icons = Resources.LoadAll("Icons", typeof(Sprite)).Cast<Sprite>().ToArray(); ;

            TopDeck.LineIndicator = Line.TopDeck;
            TopBackline.LineIndicator = Line.TopBackline;
            TopFrontline.LineIndicator = Line.TopFrontline;
            BotFrontline.LineIndicator = Line.BotFrontline;
            BotBackline.LineIndicator = Line.BotBackline;
            BotDeck.LineIndicator = Line.BotDeck;

            // for convenience
             _lines = new LineUI[6] { TopDeck, TopBackline, TopFrontline, BotFrontline, BotBackline, BotDeck };

            // subscribe
            GameLogic.GameLogicStatusChangedEventHandler += GameStatusChanged;
        }

        void Start()
        {
            SpawnDeck(Line.TopDeck, true);
            SpawnDeck(Line.BotDeck, false);
        }

        void Update()
        {
            // spacja powoduje ruch komputera
            if(Input.GetKeyDown(KeyCode.Space))
            {
                // give away control to AI
                GameLogic.StartAITurn();
                // podnoszenie kart powinno byc przyblokowane
            }
        }

        void SpawnDeck(Line line, bool hidden)
        {
#if UNITY_EDITOR
            if (line != Line.TopDeck && line != Line.BotDeck)
                throw new System.ArgumentException("SpawnDeck method can only target TopDeck or BotDeck lines.", "line");
#endif

            GameLogic.SpawnRandomDeck(line)
                .ForEach(cardModel => _lines[(int)line].InsertCard(
                    CreateUICardRepresentation(cardModel, hidden).UpdateStrengthText()));
        }

        /// <summary>
        /// 
        /// </summary>
        CardUI CreateUICardRepresentation(CardModel cardModel, bool hidden)
        {
            GameObject card = Instantiate(CardPrefab, transform);

            CardUI ui = card.GetComponent<CardUI>();
            ui.Id = cardModel.CardId;
            ui.mainCanvas = MainCanvas;
            ui.secondaryCanvas = SecondaryCanvas;
            ui.Image.sprite = Icons[cardModel.CardId];

            ui.MaxStrength = cardModel.DefaultStrength;
            ui.CurrentStrength = cardModel.DefaultStrength;

            ui.Hidden = hidden;
            ui.Draggable = !hidden;

            return ui;
        }

        public void GameStatusChanged(object sender, GameLogicStatusChangedEventArgs eventArgs)
        {
            LastMove move = eventArgs.LastMove;

            if(move != null)
                MoveCard(move.FromLine, move.FromSlotNumber, move.TargetLine, move.TargetSlotNumber);
        }

        void MoveCard(Line fromLine, int fromSlotNumber, Line targetLine, int targetSlotNumber)
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

            LineUI fLine = _lines[(int)fromLine];
            LineUI tLine = _lines[(int)targetLine];

            Debug.Log("fromSlot: " + fromSlotNumber + " fLine.count:" + fLine.Cards.Count);

            CardUI card = fLine[fromSlotNumber];
            card.Hidden = false; // cards in this game never go hidden again so we can safely set it to false here

            fLine.RemoveFromLine(fromSlotNumber, false);
            tLine.InsertCard(card, targetSlotNumber);
        }
    }
}
