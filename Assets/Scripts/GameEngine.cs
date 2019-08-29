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

            TopDeck.LineIndicator = LineIndicator.TopDeck;
            TopBackline.LineIndicator = LineIndicator.TopBackline;
            TopFrontline.LineIndicator = LineIndicator.TopFrontline;
            BotFrontline.LineIndicator = LineIndicator.BotFrontline;
            BotBackline.LineIndicator = LineIndicator.BotBackline;
            BotDeck.LineIndicator = LineIndicator.BotDeck;

            // for convenience
            _lines = new LineUI[6] { TopDeck, TopBackline, TopFrontline, BotFrontline, BotBackline, BotDeck };
        }

        void Start()
        {
            SpawnDeck(LineIndicator.TopDeck, true);
            SpawnDeck(LineIndicator.BotDeck, false);
        }

        void Update()
        {

        }

        void SpawnDeck(LineIndicator line, bool hidden)
        {
#if UNITY_EDITOR
            if (line != LineIndicator.TopDeck && line != LineIndicator.BotDeck)
                throw new System.ArgumentException("SpawnDeck method can only target TopDeck or BotDeck lines.", "line");
#endif

            GameLogic.SpawnRandomDeck(line)
                .ForEach(cardModel => _lines[(int)line].InsertCard(
                    CreateUICardRepresentation(cardModel, hidden).UpdateStrengthText()));
        }

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

            return ui;
        }
    }
}
