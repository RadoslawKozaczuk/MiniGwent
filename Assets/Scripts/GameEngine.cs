using Assets.Core;
using Assets.Core.DataModel;
using Assets.Scripts.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    public class GameEngine : MonoBehaviour
    {
        public static GameEngine Instance;

        // lines
        public LineUI TopDeck;
        public LineUI TopBackline;
        public LineUI TopFrontline;
        public LineUI BotFrontline;
        public LineUI BotBackline;
        public LineUI BotDeck;

        // prefabs
        public GameObject CardContainerPrefab;
        public GameObject CardPrefab;
        public GameObject TargetSlotIndicatorPrefab;

        // static ui elements
        public CardInfoUI CardInfoPanel;
        public EndGamePanelUI EndGamePanel;
        public GameObject BlackBackground;
        [SerializeField] GameLogic _gameLogic;
        public EndTurnPanelUI EndTurnPanel;

        public RectTransform ObjectDump;

        // canvases
        public Canvas MainCanvas; // everything is here
        public Canvas SecondaryCanvas; // only dragged items are here

        public static readonly DummyDB DB = new DummyDB();

        public static Sprite[] Icons;

        public static CardUI CardBeingDraged; // this is used as a condition for lines whether they suppose to pulsate or not

        LineUI[] _lines;

        static public bool BlockDragAction;

        #region Unity life-cycle methods
        void Awake()
        {
            Instance = this;
            Icons = Resources.LoadAll("Icons", typeof(Sprite)).Cast<Sprite>().ToArray(); ;

            // for convenience
            _lines = new LineUI[6] { TopDeck, TopBackline, TopFrontline, BotFrontline, BotBackline, BotDeck };

            // subscribe
            GameLogic.GameLogicStatusChangedEventHandler += HandleGameLogicStatusChanged;
        }

        void Start()
        {
            SpawnDeck(PlayerIndicator.Top, true);
            SpawnDeck(PlayerIndicator.Bot, false);
        }

        void Update()
        {
                
        }
        #endregion

        internal void HandleEndTurnAction()
        {
            EndTurnPanel.SetAiThinking();
            _gameLogic.StartAITurn();

            BlockDragAction = true;
        }

        internal void HandleInterfaceMoveCardRequest(Line fromLine, int fromSlotNumber, Line targetLine, int targetSlotNumber)
        {
#if UNITY_EDITOR
            MoveCardAssertions(fromLine, fromSlotNumber, targetLine, targetSlotNumber);
#endif

            MoveCard(fromLine, fromSlotNumber, targetLine, targetSlotNumber);

            _gameLogic.MoveCard(fromLine, fromSlotNumber, targetLine, targetSlotNumber); // inform game logic

            // later on add some extra logic like check if all cards action are played or something like that
            EndTurnPanel.SetReady();
        }

        void SpawnDeck(PlayerIndicator player, bool hidden)
        {
            List<CardModel> cards = _gameLogic.SpawnRandomDeck(player);
            LineUI targetLine = _lines[player == PlayerIndicator.Top ? 0 : 5];

            cards.ForEach(cardModel => targetLine.InsertCard(
                CreateUICardRepresentation(cardModel, player, hidden).UpdateStrengthText()));
        }

        CardUI CreateUICardRepresentation(CardModel cardModel, PlayerIndicator player, bool hidden)
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

            ui.PlayerIndicator = player;

            return ui;
        }

        void HandleGameLogicStatusChanged(object sender, GameLogicStatusChangedEventArgs eventArgs)
        {
            MoveData move = eventArgs.LastMove;

            if (move != null)
            {
                MoveCard(move.FromLine, move.FromSlotNumber, move.TargetLine, move.TargetSlotNumber);
                EndTurnPanel.SetOff();
                BlockDragAction = false;
            }

            if(eventArgs.MessageType == GameLogicMessageType.GameOver)
            {
                BlackBackground.SetActive(true);
                EndGamePanel.SetData(eventArgs.OverallTopStrength, eventArgs.OverallBotStrength);
            }
        }

        void MoveCard(Line fromLine, int fromSlotNumber, Line targetLine, int targetSlotNumber)
        {
#if UNITY_EDITOR
            MoveCardAssertions(fromLine, fromSlotNumber, targetLine, targetSlotNumber);
#endif

            LineUI fLine = _lines[(int)fromLine];
            LineUI tLine = _lines[(int)targetLine];

            CardUI card = fLine[fromSlotNumber];
            card.Hidden = false; // cards in this game never go hidden again so we can safely set it to false here

            fLine.RemoveFromLine(fromSlotNumber);
            tLine.InsertCard(card, targetSlotNumber);
        }

#if UNITY_EDITOR
        void MoveCardAssertions(Line fromLine, int fromSlotNumber, Line targetLine, int targetSlotNumber)
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
    }
}
