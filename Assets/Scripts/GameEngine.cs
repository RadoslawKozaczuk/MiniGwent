using Assets.GameLogic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    [RequireComponent(typeof(IconCollection))]
    class GameEngine : MonoBehaviour
    {
        public RectTransform TopDeck;
        public RectTransform TopBackline;
        public RectTransform TopFrontline;
        public RectTransform BottomFrontline;
        public RectTransform BottomBackline;
        public RectTransform BottomDeck;

        public GameObject EmptySlotPrefab;
        public GameObject CardPrefab;
        public CardInfoUI CardInfoPanel;

        readonly DummyDB _db = new DummyDB();
        public static IconCollection IconCollection;

        private Sprite[] icons;

        bool _lateSpawn = true;

        private void Awake()
        {
            icons = Resources.LoadAll("Icons", typeof(Sprite)).Cast<Sprite>().ToArray(); ;
            IconCollection = GetComponent<IconCollection>();
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
                    CreateRandomCard();

                _lateSpawn = false;
            }
        }

        void CreateRandomCard()
        {
            int id = Random.Range(0, DummyDB.Length - 1);
            CardData data = _db[id];

            GameObject slot = Instantiate(EmptySlotPrefab, TopDeck);
            GameObject card = Instantiate(CardPrefab, slot.transform);

            CardUI ui = card.GetComponent<CardUI>();
            ui.Title = data.Title;
            ui.Description = data.Description;
            ui.Strength = data.Strength;

            ui.CardInfoUI = CardInfoPanel;
            ui.Image.sprite = icons[id];
        }
    }
}
