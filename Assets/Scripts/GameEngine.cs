using Assets.Core;
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

        public Canvas MainCanvas; // everything is here
        public Canvas SecondaryCanvas; // only dragged items are here

        public static readonly DummyDB DB = new DummyDB();

        public static Sprite[] Icons;

        public static GameLogic GameLogic = new GameLogic();

        public static CardUI CardBeingDraged; // this is used as a condition for lines whether they suppose to pulsate or not

        private void Awake()
        {
            Instance = this;
            Icons = Resources.LoadAll("Icons", typeof(Sprite)).Cast<Sprite>().ToArray(); ;

            //
            TopDeck.LineIndicator = LineIndicator.TopDeck;
            TopBackline.LineIndicator = LineIndicator.TopBackline;
            TopFrontline.LineIndicator = LineIndicator.TopFrontline;
            BotFrontline.LineIndicator = LineIndicator.BotFrontline;
            BotBackline.LineIndicator = LineIndicator.BotBackline;
            BotDeck.LineIndicator = LineIndicator.BotDeck;
        }

        void Start()
        {
            var deck = GameLogic.SpawnRandomDeck(LineIndicator.TopDeck);
            deck.ForEach(c =>
            {
                CardUI ui = CreateUICardRepresentation(c.CardId);
                TopDeck.InsertCard(ui);
            });

            var botdeck = GameLogic.SpawnRandomDeck(LineIndicator.BotDeck);
            botdeck.ForEach(c =>
            {
                CardUI ui = CreateUICardRepresentation(c.CardId);
                BotDeck.InsertCard(ui);
            });
        }

        void Update()
        {

        }

        CardUI CreateUICardRepresentation(int id)
        {
            GameObject card = Instantiate(CardPrefab, transform);
            CardUI ui = card.GetComponent<CardUI>();
            ui.Id = id;
            ui.mainCanvas = MainCanvas;
            ui.secondaryCanvas = SecondaryCanvas;
            ui.Image.sprite = Icons[id];

            return ui;
        }
    }
}
