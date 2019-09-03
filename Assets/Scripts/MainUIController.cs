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
        public static Sprite[] Icons;
        public static CardUI CardBeingDraged;
        public static CardUI CardMouseOver; // card with mouse over it
        public static CardUI CardSelected;
        public static LineUI LineMouseOver;

        // control flags
        public static bool BlockDragAction;
        public static bool SingleTargetSelectMode;
        public static bool EnemyLineSelectMode;
        public static bool AllyLineSelectMode;
        public static bool ShowUIMode;
        public static bool MouseHoveringOverPopulatedAllyLine;
        public static bool MouseHoveringOverAllyUnit;
        public static bool MouseHoveringOverPopulatedEnemyLine;
        public static bool MouseHoveringOverEnemyUnit;

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

        // public UI elements
        public CardInfoUI CardInfoPanel;
        public RectTransform ObjectDump;
        public Canvas MainCanvas; // everything is here
        public Canvas SecondaryCanvas; // only dragged items are here

        [SerializeField] EndGamePanelUI _endGamePanel;
        [SerializeField] GameObject _blackBackground;
        [SerializeField] StatusPanelUI _statusPanel;
        [SerializeField] GameObject _linesGroup;
        [SerializeField] EndTurnPanelUI _endTurnPanel;
        [SerializeField] VFXController _vfxController;

        readonly PlayerControl _topPlayerControl = PlayerControl.AI; // always AI
        PlayerControl _botPlayerControl;
        GameLogic _gameLogic;
        LineUI[] _lines;

        #region Unity life-cycle methods
        void Awake()
        {
            Instance = this;
            Icons = Resources.LoadAll("Icons", typeof(Sprite)).Cast<Sprite>().ToArray(); ;

            // for convenience
            _lines = new LineUI[6] { _topDeck, _topBackline, _topFrontline, _botFrontline, _botBackline, _botDeck };
        }

        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space) && _endTurnPanel.Interactable)
                HandleEndTurnAction();
            else if(Input.GetMouseButtonDown(0))
            {
                if (SingleTargetSelectMode && MouseHoveringOverEnemyUnit)
                    HandleSingleTargetSelected();
                else if(EnemyLineSelectMode && MouseHoveringOverPopulatedEnemyLine)
                {
                    EnemyLineSelectMode = false;
                    HandleLineSelected();
                }
                else if (AllyLineSelectMode && MouseHoveringOverPopulatedAllyLine)
                {
                    AllyLineSelectMode = false;
                    HandleLineSelected();
                }
            }
        }
        #endregion

        #region Interface Implementation
        public async void HandleInterfaceMoveCardRequest(LineIndicator fromLine, int fromSlotNumber, LineIndicator targetLine, int targetSlotNumber)
        {
            BlockDragAction = true;

            #region Assertions
#if UNITY_EDITOR
            MoveCardAssertions(fromLine, fromSlotNumber, targetLine, targetSlotNumber);
#endif
            #endregion

            CardUI movedCard = MoveCardOnUI(fromLine, fromSlotNumber, targetLine, targetSlotNumber);
            CardSkill skill = DB[movedCard.Id].Skill;

            if (skill == null)
            {
                _gameLogic.MoveCardForUI(fromLine, fromSlotNumber, targetLine, targetSlotNumber, false);
                _endTurnPanel.SetNothingElseToDo();
            }
            else if (skill.ExecutionTime == SkillExecutionTime.OnDeployAutomatic)
            {
                await DisplayVisualEffectsIfAny(movedCard, targetLine, targetSlotNumber);
                _gameLogic.MoveCardForUI(fromLine, fromSlotNumber, targetLine, targetSlotNumber, applySkill: true);
                _endTurnPanel.SetNothingElseToDo();
            }
            else if (skill.ExecutionTime == SkillExecutionTime.OnDeployManual
                &&
                // ally line and ally targets present
                ((skill.Targets[0] == SkillTarget.AllyLine && (_botBackline.Count > 0 || _botFrontline.Count > 0))
                // enemy and enemy target present
                || _topBackline.Count > 0 || _topFrontline.Count > 0))
            {
                if (skill.Targets[0] == SkillTarget.AllyLine)
                    AllyLineSelectMode = true;
                else if (skill.Targets[0] == SkillTarget.SingleEnemy)
                    SingleTargetSelectMode = true;
                else if (skill.Targets[0] == SkillTarget.EnemyLine)
                    EnemyLineSelectMode = true;

                _gameLogic.MoveCardForUI(fromLine, fromSlotNumber, targetLine, targetSlotNumber, false);

                // waiting for user response
                _endTurnPanel.SetSelectTarget();
                CardSelected = _lines[(int)targetLine][targetSlotNumber];
            }
            else // on deploy but no valid targets
            {
                _endTurnPanel.SetNothingElseToDo();
            }
        }

        public void HandleEndTurnAction()
        {
            Debug.Log("BlockDrag set to true");
            BlockDragAction = true;

            CurrentPlayer = CurrentPlayer.Opposite();

            _endTurnPanel.SetAiThinking();
            _gameLogic.StartNextTurn();
        }

        public async void HandleSingleTargetSelected()
        {
            SingleTargetSelectMode = false;

            var targets = new List<SkillTargetData>(1)
            {
                new SkillTargetData(CardMouseOver.ParentLineUI.LineIndicator, CardMouseOver.SlotNumber)
            };

            CardSkill skill = DB[CardSelected.Id].Skill;
            CardSelected = null;

            await DisplayVisualEffects(targets, skill.VisualEffect);

            // apply the effects - this will also apply card skill effects if any
            _gameLogic.ApplySkillForUI(targets, skill);

            // later on add some extra logic like check if all cards action are played or something like that
            _endTurnPanel.SetNothingElseToDo();
        }

        public async void HandleLineSelected()
        {
            var targets = new List<SkillTargetData>(LineMouseOver.Cards.Count);
            foreach(CardUI model in LineMouseOver.Cards)
                targets.Add(new SkillTargetData(LineMouseOver.LineIndicator, model.SlotNumber));

            CardSkill skill = DB[CardSelected.Id].Skill;
            CardSelected = null;

            await DisplayVisualEffects(targets, skill.VisualEffect);

            _gameLogic.ApplySkillForUI(targets, skill);

            _endTurnPanel.SetNothingElseToDo();
        }
        #endregion

        public void StartGame(PlayerControl botControl, bool showUI, bool fastAI)
        {
            _gameLogic = new GameLogic(botControl, fastAI);

            _botPlayerControl = botControl;
            ShowUIMode = showUI;

            if(ShowUIMode)
            {
                _linesGroup.gameObject.SetActive(true);
                if (_botPlayerControl == PlayerControl.Human)
                {
                    _endTurnPanel.gameObject.SetActive(true);
                    _endTurnPanel.CurrentTurn = PlayerIndicator.Bot;
                }
                else
                {
                    BlockDragAction = true;
                }

                // subscribe
                GameLogic.GameLogicStatusChangedEventHandler += HandleGameLogicStatusChanged;
            }

            _statusPanel.gameObject.SetActive(true);
            _blackBackground.SetActive(false);

            if(ShowUIMode)
            {
                // if both players are AI both decs are visible
                SpawnDeck(PlayerIndicator.Top, hidden: _botPlayerControl == PlayerControl.Human);
                SpawnDeck(PlayerIndicator.Bot, hidden: false);
            }
            else
            {
                // use internal logic only
                _gameLogic.SpawnRandomDeck(PlayerIndicator.Top, doNotSendCopy: true);
                _gameLogic.SpawnRandomDeck(PlayerIndicator.Bot, doNotSendCopy: true);
            }

            if (_botPlayerControl == PlayerControl.AI)
                _gameLogic.StartNextTurn();
            else
                _endTurnPanel.SetYourTurn();
        }

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

            cards.ForEach(cardModel => targetLine.AddCard(
                CreateUICardRepresentation(cardModel, player, hidden)));
        }

        // this is some sort of a constructor, could be extracted out for better encapsulation 
        // but that would require to have another class which I think is not worth it
        CardUI CreateUICardRepresentation(CardModel cardModel, PlayerIndicator player, bool hidden)
        {
            GameObject card = Instantiate(CardPrefab, transform);

            CardUI ui = card.GetComponent<CardUI>();
            ui.Id = cardModel.CardId;
            ui.Image.sprite = Icons[cardModel.CardId];
            ui.TitleText.text = DB[cardModel.CardId].Title;

            ui.DefaultStrength = cardModel.DefaultStrength;
            ui.CurrentStrength = cardModel.DefaultStrength;

            ui.Hidden = hidden;
            ui.Draggable = !hidden;

            ui.PlayerIndicator = player;

            return ui;
        }

        /// <summary>
        /// This method handles whole communication with GameLogic.
        /// </summary>
        async void HandleGameLogicStatusChanged(object sender, GameLogicStatusChangedEventArgs eventArgs)
        {
            CurrentPlayer = eventArgs.CurrentPlayer;

            // AI wants me to move a card
            if (eventArgs.MessageType == GameLogicMessageType.MoveCard)
            {
                MoveData move = eventArgs.LastMove;

#if UNITY_EDITOR
                if (move == null)
                    throw new System.ArgumentNullException("LastMove", "LastMove cannot be null in this context.");
#endif

                MoveCardOnUI(move);
            }

            // AI wants me to play VFXs
            else if(eventArgs.MessageType == GameLogicMessageType.PlaySkillVFX)
                await DisplayVisualEffects(eventArgs.Targets, eventArgs.VisualEffect);

            // AI wants me to update strengths
            else if(eventArgs.MessageType == GameLogicMessageType.UpdateStrength)
            {
                ApplyStrengthChanges(eventArgs.CardStrengths);
                RemoveDeadOnes();
            }

            // AI informs me that its turn has ended
            else if(eventArgs.MessageType == GameLogicMessageType.EndTurn)
            {
                if (_botPlayerControl == PlayerControl.Human)
                {
                    if(_botDeck.Count == 0)
                        GameOver(eventArgs.TopTotalStrength, eventArgs.BotTotalStrength);
                    else
                        _endTurnPanel.SetYourTurn();
                }
            }

            // AI informs me that the game is over
            else if(eventArgs.MessageType == GameLogicMessageType.GameOver)
                GameOver(eventArgs.TopTotalStrength, eventArgs.BotTotalStrength);

            _gameLogic.ReturnControl();
        }

        void GameOver(int topStrength, int botStrength)
        {
            _blackBackground.SetActive(true);
            _endGamePanel.SetData(topStrength, botStrength);
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

            Transform cardContainer = fLine.transform.GetChild(move.FromSlotNumber);
            cardContainer.SetParent(tLine.transform);
            cardContainer.SetSiblingIndex(move.TargetSlotNumber);

            CardUI card = cardContainer.transform.GetChild(0).GetComponent<CardUI>();
            card.Hidden = false; // cards in this game never go hidden again so we can safely set it to false here
            card.ParentLineUI = tLine;
            card.SlotNumber = move.TargetSlotNumber;

            // add to list and increase slot number on the right by one
            if (move.TargetSlotNumber == tLine.Cards.Count)
                tLine.Cards.Add(card);
            else
            {
                tLine.Cards.Insert(move.TargetSlotNumber, card);
                tLine.Cards.GetLast(tLine.Cards.Count - move.TargetSlotNumber - 1).ToList()
                    .ForEach(c => c.SlotNumber++);
            }

            // remove from list and decrease slot number on the right by one
            fLine.Cards.RemoveAt(move.FromSlotNumber);
            fLine.Cards.GetLast(fLine.Cards.Count - move.FromSlotNumber).ToList()
                .ForEach(c => c.SlotNumber--);
        }

        async Task DisplayVisualEffectsIfAny(CardUI movedCard, LineIndicator targetLine, int targetSlotNumber)
        {
            // upper logic does not keep any data related to skills so we have to check in the DB
            CardSkill skill = DB[movedCard.Id].Skill;

            // if there are any skills to do, do them
            if (skill == null)
                return;

            var targets = new List<CardUI>();
            if (skill.ExecutionTime == SkillExecutionTime.OnDeployAutomatic)
            {
                foreach (SkillTarget target in skill.Targets)
                    targets.AddRange(ParseCardSkillTarget(targetLine, targetSlotNumber, target));

                _vfxController.ScheduleParticleEffect(targets, skill.VisualEffect);

                await _vfxController.PlayScheduledParticleEffects();
            }
        }

        async Task DisplayVisualEffects(List<SkillTargetData> targets, SkillVisualEffect visualEffect)
        {
            var targetCards = new List<CardUI>(targets.Count);

            foreach (SkillTargetData target in targets)
            {
                LineUI l = _lines[(int)target.Line];
                if(target.SlotNumber < l.Cards.Count)
                    targetCards.Add(l[target.SlotNumber]);
            }

            _vfxController.ScheduleParticleEffect(targetCards, visualEffect);

            await _vfxController.PlayScheduledParticleEffects();
        }

        void ApplyStrengthChanges(List<List<int>> currentCardStrengths)
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
