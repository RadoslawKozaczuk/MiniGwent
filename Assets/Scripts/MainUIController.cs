using Assets.Core;
using Assets.Core.CardSkills;
using Assets.Core.DataModel;
using Assets.Scripts.Interfaces;
using Assets.Scripts.UI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class MainUIController : MonoBehaviour, IMainUIController
    {
        public static MainUIController Instance;
        public static readonly DummyDB DB = new DummyDB();

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
        public VFXController VFXController;

        // canvases
        public Canvas MainCanvas; // everything is here
        public Canvas SecondaryCanvas; // only dragged items are here

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
            EndTurnPanel.SetYourTurn();
        }

        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space) && EndTurnPanel.Interactable)
                HandleEndTurnAction();
        }
        #endregion

        #region Interface Implementation
        public void HandleEndTurnAction()
        {
            EndTurnPanel.SetAiThinking();
            _gameLogic.StartAITurn();

            BlockDragAction = true;
        }

        public async void HandleInterfaceMoveCardRequest(Line fromLine, int fromSlotNumber, Line targetLine, int targetSlotNumber)
        {
            #region Assertions
#if UNITY_EDITOR
            MoveCardAssertions(fromLine, fromSlotNumber, targetLine, targetSlotNumber);
#endif
            #endregion

            CardUI movedCard = MoveCardOnUI(fromLine, fromSlotNumber, targetLine, targetSlotNumber);

            await DisplayVisualEffects(movedCard, targetLine, targetSlotNumber);

            // apply the effects - this will also apply card skill effects if any
            _gameLogic.MoveCardForUI(fromLine, fromSlotNumber, targetLine, targetSlotNumber);

            // later on add some extra logic like check if all cards action are played or something like that
            EndTurnPanel.SetNothingElseToDo();
        }
        #endregion

        List<CardUI> ParseCardSkillTarget(Line targetLine, int targetSlotNumber, SkillTarget target)
        {
            if (target == SkillTarget.CorrespondingEnemyLine)
            {
                // target line in this context is the deployment line
                int correspondingLineID = 5 - (int)targetLine;
                return _lines[correspondingLineID].Cards;
            }

            List<CardUI> cards = _lines[(int)targetLine].Cards;
            if (target == SkillTarget.AllInLineExceptMe)
                return cards.GetAllExceptOne(targetSlotNumber).ToList();
            if (target == SkillTarget.LeftNeighbor)
                return cards.GetLeftNeighbor(targetSlotNumber).ToList();
            if(target == SkillTarget.BothNeighbors)
                return cards.GetBothNeighbors(targetSlotNumber).ToList();
            if (target == SkillTarget.RightNeighbor)
                return cards.GetRightNeighbor(targetSlotNumber).ToList();

            throw new System.Exception("Unreachable code reached! "
                + "CardSkillTarget enumerator must have been extended without extending the ParseCardSkillTarget method logic.");
        }

        void SpawnDeck(PlayerIndicator player, bool hidden)
        {
            List<CardModel> cards = _gameLogic.SpawnRandomDeck(player);
            LineUI targetLine = _lines[player == PlayerIndicator.Top ? 0 : 5];

            cards.ForEach(cardModel => targetLine.InsertCard(
                CreateUICardRepresentation(cardModel, player, hidden)));
        }

        // this is some sort of a semi-constructor
        // could be extracted out but that would require to have another class which I think is not worth it
        CardUI CreateUICardRepresentation(CardModel cardModel, PlayerIndicator player, bool hidden)
        {
            GameObject card = Instantiate(CardPrefab, transform);

            CardUI ui = card.GetComponent<CardUI>();
            ui.Id = cardModel.CardId;
            ui.mainCanvas = MainCanvas;
            ui.secondaryCanvas = SecondaryCanvas;
            ui.Image.sprite = Icons[cardModel.CardId];

            ui.DefaultStrength = cardModel.DefaultStrength;
            ui.CurrentStrength = cardModel.DefaultStrength;

            ui.Hidden = hidden;
            ui.Draggable = !hidden;

            ui.PlayerIndicator = player;

            return ui;
        }

        async void HandleGameLogicStatusChanged(object sender, GameLogicStatusChangedEventArgs eventArgs)
        {
            // AI wants me to move a card
            if(eventArgs.MessageType == GameLogicMessageType.MoveCard)
            {
#if UNITY_EDITOR
                if (eventArgs.LastMove == null)
                    throw new System.ArgumentNullException("LastMove", "LastMove cannot be null.");
#endif

                MoveCardOnUI(eventArgs.LastMove);
                // TODO: add card move animation and return control after its over

                // control return
                _gameLogic.ControlReturn();
            }

            // AI wants me to play some VFXs
            else if(eventArgs.MessageType == GameLogicMessageType.PlaySkillVFX)
            {
                CardSkill skill = eventArgs.Skill;
                var (targetLine, targetSlot) = eventArgs.SkillTarget;

                // play VFXs
                await DisplayVisualEffects(skill, targetLine, targetSlot);

                // control return
                _gameLogic.ControlReturn();
            }

            // AI wants me to update strength
            else if(eventArgs.MessageType == GameLogicMessageType.UpdateStrength)
            {
                // apply strength update to the UI
                ApplyCurrentCardStrengthsChanges(eventArgs.CardStrengths);

                // remove dead ones
                RemoveDeadOnes();

                // control return
                _gameLogic.ControlReturn();
            }

            // AI informs me that its turn has ended
            else if(eventArgs.MessageType == GameLogicMessageType.EndTurn)
            {
                EndTurnPanel.SetYourTurn();
                BlockDragAction = false;
            }

            // AI informs me that the game is over
            else if(eventArgs.MessageType == GameLogicMessageType.GameOver)
            {
                BlackBackground.SetActive(true);
                EndGamePanel.SetData(eventArgs.OverallTopStrength, eventArgs.OverallBotStrength);
            }
        }

        CardUI MoveCardOnUI(Line fromLine, int fromSlotNumber, Line targetLine, int targetSlotNumber)
        {
            #region Assertions
#if UNITY_EDITOR
            MoveCardAssertions(fromLine, fromSlotNumber, targetLine, targetSlotNumber);
#endif
            #endregion

            LineUI fLine = _lines[(int)fromLine];
            LineUI tLine = _lines[(int)targetLine];

            CardUI card = fLine[fromSlotNumber];
            card.Hidden = false; // cards in this game never go hidden again so we can safely set it to false here

            fLine.RemoveFromLine(fromSlotNumber);
            tLine.InsertCard(card, targetSlotNumber);

            return card;
        }

        void MoveCardOnUI(MoveData move)
        {
            #region Assertions
#if UNITY_EDITOR
            MoveCardAssertions(move.FromLine, move.FromSlotNumber, move.TargetLine, move.TargetSlotNumber);
#endif
            #endregion

            LineUI fLine = _lines[(int)move.FromLine];
            LineUI tLine = _lines[(int)move.TargetLine];

            CardUI card = fLine[move.FromSlotNumber];
            card.Hidden = false; // cards in this game never go hidden again so we can safely set it to false here

            fLine.RemoveFromLine(move.FromSlotNumber);
            tLine.InsertCard(card, move.TargetSlotNumber);
        }

        async Task DisplayVisualEffects(CardUI movedCard, Line targetLine, int targetSlotNumber)
        {
            // upper logic does not keep any data related to skills so we have to check in the DB
            CardSkill skill = DB[movedCard.Id].Skill;

            // if there are any skills to do, do them
            if (skill != null)
            {
                var targets = new List<CardUI>();
                if (skill.ExecutionTime == SkillExecutionTime.OnDeploy)
                {
                    foreach (SkillTarget target in skill.Targets)
                        targets.AddRange(ParseCardSkillTarget(targetLine, targetSlotNumber, target));

                    VFXController.ScheduleParticleEffect(targets, skill.VisualEffect);

                    await VFXController.PlayScheduledParticleEffects();

                    // remove dead ones - does not work
                    RemoveDeadOnes();
                }
            }
        }

        async Task DisplayVisualEffects(CardSkill skill, Line targetLine, int targetSlotNumber)
        {
            var targets = new List<CardUI>();
            if (skill.ExecutionTime == SkillExecutionTime.OnDeploy)
            {
                foreach (SkillTarget target in skill.Targets)
                    targets.AddRange(ParseCardSkillTarget(targetLine, targetSlotNumber, target));

                VFXController.ScheduleParticleEffect(targets, skill.VisualEffect);

                await VFXController.PlayScheduledParticleEffects();
            }
        }

        void ApplyCurrentCardStrengthsChanges(List<List<int>> currentCardStrengths)
        {
            for (int i = 1; i < _lines.Count() - 1; i++) // we omit top and bot decks
            {
                List<CardUI> cards = _lines[i].Cards;
                for (int j = 0; j < cards.Count; j++)
                    cards[j].CurrentStrength = currentCardStrengths[i - 1][j]; // minus 1 because logic omits decks while sending
            }
        }

        void RemoveDeadOnes()
        {
            for (int i = 1; i < _lines.Count() - 1; i++) // we omit top and bot decks
            {
                LineUI line = _lines[i];
                for (int j = 0; j < line.Cards.Count; j++)
                    if(line[j].CurrentStrength <= 0)
                        line.RemoveFromLine(j--);
            }
        }

        #region Assertions
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
        #endregion
    }
}
