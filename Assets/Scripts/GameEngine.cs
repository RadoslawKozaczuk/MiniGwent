using Assets.GameLogic;
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
        public LineUI BottomFrontline;
        public LineUI BottomBackline;
        public LineUI BottomDeck;

        public GameObject CardContainer;
        public GameObject CardPrefab;
        public CardInfoUI CardInfoPanel;

        public Canvas MainCanvas; // everything is here
        public Canvas SecondaryCanvas; // only dragged item are here

        public static readonly DummyDB DB = new DummyDB();

        public static Sprite[] Icons;

        bool _lateSpawn = true;

        public static CardUI CardBeingDraged; // this is used as a condition for lines whether they suppose to pulsate or not

        private void Awake()
        {
            Instance = this;
            Icons = Resources.LoadAll("Icons", typeof(Sprite)).Cast<Sprite>().ToArray(); ;
        }

        void Start()
        {
            
        }

        void Update()
        {
            if(_lateSpawn)
            {
                // randomly spawn 3 cards and add them to the deck
                for (int i = 0; i < 3; i++)
                {
                    CardUI card = CreateRandomCard();
                    TopDeck.PutInLine(card);
                }

                _lateSpawn = false;
            }
        }

        CardUI CreateRandomCard()
        {
            int id = Random.Range(0, DummyDB.Length);
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
