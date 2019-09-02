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

        PlayerIndicator _currentPlayer;
        public PlayerIndicator CurrentPlayer
        {
            get =>_currentPlayer;
            set
            {
                _currentPlayer = value;
                _endTurnPanel.CurrentTurn = value;
            }
        }

        // lines
        [SerializeField] LineUI _topDeck;
        [SerializeField] LineUI _topBackline;
        [SerializeField] LineUI _topFrontline;
        [SerializeField] LineUI _botFrontline;
        [SerializeField] LineUI _botBackline;
        [SerializeField] LineUI _botDeck;

        // prefabs
        public GameObject CardContainerPrefab;
        public GameObject CardPrefab;
        public GameObject TargetSlotIndicatorPrefab;

        // static ui elements
        public CardInfoUI CardInfoPanel;
        public EndGamePanelUI EndGamePanel;
        public GameObject BlackBackground;
        GameLogic _gameLogic;

        [SerializeField] StatusPanelUI _statusPanel;
        [SerializeField] GameObject _linesGroup;
        [SerializeField] EndTurnPanelUI _endTurnPanel;

        // top control
        readonly PlayerControl _topPlayerControl = PlayerControl.AI; // always AI
        PlayerControl _botPlayerControl;

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
            _lines = new LineUI[6] { _topDeck, _topBackline, _topFrontline, _botFrontline, _botBackline, _botDeck };

            // subscribe
            GameLogic.GameLogicStatusChangedEventHandler += HandleGameLogicStatusChanged;
        }

        public void StartGame(PlayerControl botControl)
        {
            _gameLogic = new GameLogic(botControl);

            _botPlayerControl = botControl;

            _statusPanel.gameObject.SetActive(true);
            _linesGroup.gameObject.SetActive(true);
            _endTurnPanel.gameObject.SetActive(true);
            _endTurnPanel.CurrentTurn = PlayerIndicator.Bot;
            BlackBackground.SetActive(false);

            // if both players are AI both decs are visible
            SpawnDeck(PlayerIndicator.Top, hidden: _botPlayerControl == PlayerControl.Human);
            SpawnDeck(PlayerIndicator.Bot, hidden: false);

            if (_botPlayerControl == PlayerControl.AI)
                _gameLogic.StartNextTurn();
            else
                _endTurnPanel.SetYourTurn();
        }

        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space) && _endTurnPanel.Interactable)
                HandleEndTurnAction();
        }
        #endregion

        #region Interface Implementation
        public void HandleEndTurnAction()
        {
            CurrentPlayer = CurrentPlayer.Opposite();

            _endTurnPanel.SetAiThinking();
            _gameLogic.StartNextTurn();

            BlockDragAction = true;
        }

        public async void HandleInterfaceMoveCardRequest(LineIndicator fromLine, int fromSlotNumber, LineIndicator targetLine, int targetSlotNumber)
        {
            #region Assertions
#if UNITY_EDITOR
            MoveCardAssertions(fromLine, fromSlotNumber, targetLine, targetSlotNumber);
#endif
            #endregion

            CardUI movedCard = MoveCardOnUI(fromLine, fromSlotNumber, targetLine, targetSlotNumber);

            await DisplayVisualEffectsIfAny(movedCard, targetLine, targetSlotNumber);

            // apply the effects - this will also apply card skill effects if any
            _gameLogic.MoveCardForUI(fromLine, fromSlotNumber, targetLine, targetSlotNumber);

            // later on add some extra logic like check if all cards action are played or something like that
            _endTurnPanel.SetNothingElseToDo();

            BlockDragAction = true;
        }
        #endregion

        List<CardUI> ParseCardSkillTarget(LineIndicator targetLine, int targetSlotNumber, SkillTarget target)
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
            ui.TitleText.text = DB[cardModel.CardId].Title;

            ui.DefaultStrength = cardModel.DefaultStrength;
            ui.CurrentStrength = cardModel.DefaultStrength;

            ui.Hidden = hidden;
            ui.Draggable = !hidden;

            ui.PlayerIndicator = player;

            return ui;
        }

        async void HandleGameLogicStatusChanged(object sender, GameLogicStatusChangedEventArgs eventArgs)
        {
            CurrentPlayer = eventArgs.CurrentPlayer;

            // AI wants me to move a card
            if (eventArgs.MessageType == GameLogicMessageType.MoveCard)
            {
#if UNITY_EDITOR
                if (eventArgs.LastMove == null)
                    throw new System.ArgumentNullException("LastMove", "LastMove cannot be null.");
#endif

                Debug.Log($"MoveData: fLine:{eventArgs.LastMove.FromLine.ToString()}" 
                    + $" fSlot:{eventArgs.LastMove.FromSlotNumber}" 
                    + $" tLine:{eventArgs.LastMove.TargetLine.ToString()}" 
                    + $" tSlot:{eventArgs.LastMove.TargetSlotNumber}");

                MoveCardOnUI(eventArgs.LastMove);
                // TODO: add card move animation and return control after its over

                // control return

                Debug.Log("Return control after MoveCard msg handling");
                _gameLogic.ReturnControl();
            }

            // AI wants me to play some VFXs
            else if(eventArgs.MessageType == GameLogicMessageType.PlaySkillVFX)
            {
                CardSkill skill = eventArgs.Skill;

                // play VFXs
                await DisplayVisualEffects(eventArgs.Targets, eventArgs.VisualEffect);

                // control return
                //Debug.Log("Return control after PlaySkillVFX msg handling");
                _gameLogic.ReturnControl();
            }

            // AI wants me to update strength
            else if(eventArgs.MessageType == GameLogicMessageType.UpdateStrength)
            {
                // apply strength update to the UI
                ApplyCurrentCardStrengthsChanges(eventArgs.CardStrengths);

                // remove dead ones
                RemoveDeadOnes();

                // control return
                Debug.Log("Return control after UpdateStrength msg handling");
                _gameLogic.ReturnControl();
            }

            // AI informs me that its turn has ended
            else if(eventArgs.MessageType == GameLogicMessageType.EndTurn)
            {
                if (_botPlayerControl == PlayerControl.Human)
                {
                    if (_botDeck.Count == 0) // nothing else to play
                    {
                        GameOver(eventArgs.TopTotalStrength, eventArgs.BotTotalStrength);
                    }
                    else
                    {
                        _endTurnPanel.SetYourTurn();
                        BlockDragAction = false;
                    }
                }
                else
                {
                    if (_botDeck.Count == 0) // nothing else to play
                    {
                        GameOver(eventArgs.TopTotalStrength, eventArgs.BotTotalStrength);
                    }
                    else
                    {
                        _endTurnPanel.SetYourTurn();
                        BlockDragAction = false;
                        // control return
                        Debug.Log("Return control after EndTurn msg handling");
                        _gameLogic.ReturnControl();
                    }
                }
            }

            // AI informs me that the game is over
            else if(eventArgs.MessageType == GameLogicMessageType.GameOver)
            {
                GameOver(eventArgs.TopTotalStrength, eventArgs.BotTotalStrength);
            }
        }

        void GameOver(int topStrength, int botStrength)
        {
            BlackBackground.SetActive(true);
            EndGamePanel.SetData(topStrength, botStrength);
        }

        CardUI MoveCardOnUI(LineIndicator fromLine, int fromSlotNumber, LineIndicator targetLine, int targetSlotNumber)
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

        async Task DisplayVisualEffectsIfAny(CardUI movedCard, LineIndicator targetLine, int targetSlotNumber)
        {
            // upper logic does not keep any data related to skills so we have to check in the DB
            CardSkill skill = DB[movedCard.Id].Skill;

            // if there are any skills to do, do them
            if (skill != null)
            {
                var targets = new List<CardUI>();
                if (skill.ExecutionTime == SkillExecutionTime.OnDeployAutomatic)
                {
                    foreach (SkillTarget target in skill.Targets)
                        targets.AddRange(ParseCardSkillTarget(targetLine, targetSlotNumber, target));

                    VFXController.ScheduleParticleEffect(targets, skill.VisualEffect);

                    await VFXController.PlayScheduledParticleEffects();
                }
            }
        }

        async Task DisplayVisualEffects(List<SkillTargetData> targets, SkillVisualEffect visualEffect)
        {
            var targetCards = new List<CardUI>(targets.Count);

            foreach (SkillTargetData target in targets)
                targetCards.Add(_lines[(int)target.Line][target.SlotNumber]);

            VFXController.ScheduleParticleEffect(targetCards, visualEffect);

            await VFXController.PlayScheduledParticleEffects();
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
        void MoveCardAssertions(LineIndicator fromLine, int fromSlotNumber, LineIndicator targetLine, int targetSlotNumber)
        {
            if (fromSlotNumber < 0 || fromSlotNumber > _lines[(int)fromLine].Count)
                throw new System.ArgumentOutOfRangeException(
                    "fromSlotNumber",
                    $"FromSlotNumber = {fromSlotNumber} while it cannot be lower than 0 "
                    + $"or greater than the number of cards in the line ({_lines[(int)fromLine].Count}).");

            if (targetSlotNumber < 0 || targetSlotNumber > _lines[(int)targetLine].Count)
                throw new System.ArgumentOutOfRangeException(
                    "targetSlotNumber",
                    $"TargetSlotNumber = {targetSlotNumber} while it cannot be lower than 0 "
                    + $"or greater than the number of cards in the line ({_lines[(int)targetLine].Count}).");

            if (targetLine == LineIndicator.TopDeck || targetLine == LineIndicator.BotDeck)
                throw new System.ArgumentException("Moving card to a deck is not allowed.");

            if (fromLine == targetLine)
                throw new System.ArgumentException("Moving card to the same line is not allowed.");

            if ((fromLine == LineIndicator.BotDeck || fromLine == LineIndicator.BotBackline || fromLine == LineIndicator.BotFrontline)
                &&
                (targetLine == LineIndicator.TopDeck || targetLine == LineIndicator.TopBackline || targetLine == LineIndicator.TopFrontline))
                throw new System.ArgumentException("Moving card from any bot line to any top line is not allowed.");

            if ((fromLine == LineIndicator.TopDeck || fromLine == LineIndicator.TopBackline || fromLine == LineIndicator.TopFrontline)
                &&
                (targetLine == LineIndicator.BotDeck || targetLine == LineIndicator.BotBackline || targetLine == LineIndicator.BotFrontline))
                throw new System.ArgumentException("Moving card from any top line to any bot line is not allowed.");
        }
#endif
        #endregion
    }
}
