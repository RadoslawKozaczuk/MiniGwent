using Assets.Core.CardSkills;
using Assets.Core.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Core
{
    public class GameLogic
    {
        public const int MAX_NUMBER_OF_CARDS_IN_LINE = 10;
        const int NUMBER_OF_CARDS_IN_DECK = 6;

        public static event EventHandler<GameLogicStatusChangedEventArgs> GameLogicStatusChangedEventHandler;
        public static event EventHandler<GameLogicLogUpdateEventArgs> GameLogicLogUpdateEventHandler;
        internal static readonly DummyDB DB = new DummyDB();

        int TopTotalStrength => _topBackline.Sum(c => c.CurrentStrength) + _topFrontline.Sum(c => c.CurrentStrength);
        int BotTotalStrength => _botBackline.Sum(c => c.CurrentStrength) + _botFrontline.Sum(c => c.CurrentStrength);

        internal PlayerIndicator CurrentPlayer = PlayerIndicator.Bot; // bot always starts the game whether it is a human or AI
        internal bool EndTurnMsgSent; // when true it indicates that next return control signal will start a new turn

        readonly List<CardModel> _topDeck = new List<CardModel>();
        readonly List<CardModel> _topBackline = new List<CardModel>();
        readonly List<CardModel> _topFrontline = new List<CardModel>();
        readonly List<CardModel> _botFrontline = new List<CardModel>();
        readonly List<CardModel> _botBackline = new List<CardModel>();
        readonly List<CardModel> _botDeck = new List<CardModel>();
        readonly List<CardModel>[] _lines;
        readonly AI _aiTop;
        readonly AI _aiBot; // null when controlled by human
        readonly AI[] _aiReferences;

        // top player is always AI
        public GameLogic(PlayerControl botControlType)
        {
            _lines = new List<CardModel>[6] { _topDeck, _topBackline, _topFrontline, _botFrontline, _botBackline, _botDeck };
            _aiTop = new AI(PlayerIndicator.Top, _topDeck, _topBackline, _topFrontline, gameLogic: this, fakeThinking: true);

            if(botControlType == PlayerControl.AI)
                _aiBot = new AI(PlayerIndicator.Bot, _botDeck, _botBackline, _botFrontline, gameLogic: this, fakeThinking: true);

            _aiReferences = new AI[] { _aiTop, _aiBot };
        }

        /// <summary>
        /// Starts new turn.
        /// </summary>
        public void StartNextTurn()
        {
            CurrentPlayer = CurrentPlayer.Opposite();
            StartNextTurnInternal();
        }

        internal List<CardModel> GetLine(PlayerIndicator player, PlayerLine line) 
            => player == PlayerIndicator.Top 
                ? _lines[(int)line] 
                : _lines[5 - (int)line];

        internal List<CardModel> GetLine(LineIndicator line) => _lines[(int)line];

        internal LineIndicator GetLineIndicator(PlayerIndicator player, PlayerLine line)
            => player == PlayerIndicator.Top
                ? (LineIndicator)(int)line
                : (LineIndicator)(5 - (int)line);

        void StartNextTurnInternal()
        {
            EndTurnMsgSent = false;

            //Debug.Log("Start of a new turn, the current player is: " + CurrentPlayer.ToString());

            if (CurrentPlayer == PlayerIndicator.Top)
            {
                if (_topDeck.Count == 0)
                {
                    // AI has nothing else to do
                    BroadcastGameOver_StatusUpdate(); // for now player always plays first so we can safely broadcast game over here
                }
                else
                    _aiTop.StartAITurn();
            }
            else
            {
                if (_botDeck.Count == 0)
                {
                    // AI has nothing else to do
                    BroadcastGameOver_StatusUpdate(); // for now player always plays first so we can safely broadcast game over here
                }
                else
                    _aiBot.StartAITurn();
            }
        }

        /// <summary>
        ///  
        /// </summary>
        public void ReturnControl()
        {
            //Debug.Log($"return control - curretn player = {CurrentPlayer.ToString()}");

            bool theOtherPlayerIsAI = _aiReferences[(int)CurrentPlayer] != null;

            if (EndTurnMsgSent && theOtherPlayerIsAI)
            {
                StartNextTurnInternal();
            }
            else
            {
                if (CurrentPlayer == PlayerIndicator.Top)
                    _aiTop.ReturnControl();
                else if (CurrentPlayer == PlayerIndicator.Bot && _aiBot != null)
                    _aiBot.ReturnControl();
            }
        }

        internal LineIndicator MapPlayerLine(PlayerIndicator player, PlayerLine line)
        {
            if (player == PlayerIndicator.Top)
                switch(line)
                {
                    case PlayerLine.Deck: return LineIndicator.TopDeck;
                    case PlayerLine.Backline: return LineIndicator.TopBackline;
                    case PlayerLine.Frontline: return LineIndicator.TopFrontline;
                }
            else if(player == PlayerIndicator.Bot)
                switch (line)
                {
                    case PlayerLine.Deck: return LineIndicator.BotDeck;
                    case PlayerLine.Backline: return LineIndicator.BotBackline;
                    case PlayerLine.Frontline: return LineIndicator.BotFrontline;
                }

            throw new Exception("Unreachable code reached! "
                + "PlayerIndicator or PlayerLine enumerator must have been extended without extending the MapPlayerLine function.");
        }

        public void MoveCardForUI(
            LineIndicator fromLine, int fromSlotNumber, 
            LineIndicator targetLine, int targetSlotNumber, 
            bool applySkill = true)
        {
            #region Assertions
#if UNITY_EDITOR
            MoveDataAssertions(fromLine, fromSlotNumber, targetLine, targetSlotNumber);
#endif
            #endregion

            List<CardModel> fLine = _lines[(int)fromLine];
            List<CardModel> tLine = _lines[(int)targetLine];

            CardModel card = fLine[fromSlotNumber];
            card.SlotNumber = targetSlotNumber;

            fLine.RemoveAt(fromSlotNumber);

            // all cards on the right must have their SlotNumber reduced by one
            fLine.AllOnTheRight(fromSlotNumber, c => c.SlotNumber--);
            tLine.AllOnTheRight(targetSlotNumber, c => c.SlotNumber++);

            if (targetSlotNumber == tLine.Count)
                tLine.Add(card);
            else
                tLine.Insert(targetSlotNumber, card);

            BroadcastCardMove_LogUpdate(CurrentPlayer, new MoveData(card, fromLine, fromSlotNumber, targetLine, targetSlotNumber));

            if (applySkill)
            {
                ApplySkillEffectIfAny(card, targetLine, targetSlotNumber);

                // it's important to evaluate strengths before RemoveDeadOnes is called
                // as we want our data match the upper logic's data 
                // UI removes its elements later on
                List<List<int>> cardStrengths = GetCardStrengths();

                // remove dead ones
                RemoveDeadOnes();

                // inform upper logic about new strengths
                BroadcastUpdateStrength_StatusUpdate(cardStrengths);
            }
        }

        public void ApplySkillForUI(SkillTargetData target, CardSkill skill)
        {
#if UNITY_EDITOR
            if (skill.ExecutionTime == SkillExecutionTime.OnDeployAutomatic)
                throw new ArgumentException("skill", "this method should be used only for manually resolved skills");
#endif

            // parse target to model
            CardModel model = _lines[(int)target.Line][target.SlotNumber];
            skill.Effect(model); // apply the skill

            // it's important to evaluate strengths before RemoveDeadOnes is called
            // as we want our data match the upper logic's data 
            // UI removes its elements later on
            List<List<int>> cardStrengths = GetCardStrengths();

            // remove dead ones
            RemoveDeadOnes();

            // inform upper logic about new strengths
            BroadcastUpdateStrength_StatusUpdate(cardStrengths);
        }

        public void MoveCardForAI(PlayerIndicator player, MoveData moveData)
        {
            LineIndicator fromLine = moveData.FromLine;
            int fromSlotNumber = moveData.FromSlotNumber;
            LineIndicator targetLine = moveData.TargetLine;
            int targetSlotNumber = moveData.TargetSlotNumber;

            #region Assertions
#if UNITY_EDITOR
            MoveDataAssertions(fromLine, fromSlotNumber, targetLine, targetSlotNumber);
#endif
            #endregion

            List<CardModel> fLine = _lines[(int)fromLine];
            List<CardModel> tLine = _lines[(int)targetLine];

            CardModel card = fLine[fromSlotNumber];
            card.SlotNumber = targetSlotNumber;

            fLine.RemoveAt(fromSlotNumber);

            // all cards on the right must have their SlotNumber reduced by one
            fLine.AllOnTheRight(fromSlotNumber, c => c.SlotNumber--);
            tLine.AllOnTheRight(targetSlotNumber, c => c.SlotNumber++);

            if (targetSlotNumber == tLine.Count)
                tLine.Add(card);
            else
                tLine.Insert(targetSlotNumber, card);

            BroadcastCardMove_LogUpdate(player, moveData);
        }

        void RemoveDeadOnes()
        {
            for (int i = 1; i < _lines.Count() - 1; i++) // we omit top and bot decks
            {
                List<CardModel> line = _lines[i];
                for (int j = 0; j < line.Count; j++)
                    if (line[j].CurrentStrength <= 0)
                        line.RemoveAt(j--);
            }
        }

        void ApplySkillEffectIfAny(CardModel card, LineIndicator deployLine, int slotNumber)
        {
            CardSkill skill = DB[card.CardId].Skill;
            if (skill != null)
            {
                var targets = new List<CardModel>();

                foreach (SkillTarget target in skill.Targets)
                    targets.AddRange(ParseOnDeployAutomaticSkillTarget(deployLine, slotNumber, target));

                foreach (CardModel target in targets)
                    skill.Effect(target);

                BroadcastSkillEffect_LogUpdate(CurrentPlayer, targets, skill);
            }
        }

        internal List<CardModel> ParseOnDeployAutomaticSkillTarget(LineIndicator line, int slotNumber, SkillTarget target)
        {
            if (target == SkillTarget.CorrespondingEnemyLine)
                return _lines[(int)line.Opposite()];

            List<CardModel> cards = _lines[(int)line];
            if (target == SkillTarget.AllInLineExceptMe)
                return cards.GetAllExceptOne(slotNumber).ToList();
            if (target == SkillTarget.LeftNeighbor)
                return cards.GetLeftNeighbor(slotNumber).ToList();
            if (target == SkillTarget.BothNeighbors)
                return cards.GetBothNeighbors(slotNumber).ToList();
            if (target == SkillTarget.RightNeighbor)
                return cards.GetRightNeighbor(slotNumber).ToList();

            throw new Exception("Unreachable code reached! "
                + "CardSkillTarget enumerator must have been extended without extending the ParseCardSkillTarget function.");
        }

        internal void ApplySkillEffectForAI(CardSkill skill, List<SkillTargetData> targets)
        {
            var models = new List<CardModel>(targets.Count);

            foreach (SkillTargetData target in targets)
            {
                CardModel model = _lines[(int)target.Line][target.SlotNumber];
                models.Add(model);
                skill.Effect(model);
            }

            BroadcastSkillEffect_LogUpdate(CurrentPlayer, models, skill);
        }

        internal void ApplySkillEffectForAISingleTarget(CardSkill skill, LineIndicator targetLine, int slotNumber)
        {
            CardModel card = _lines[(int)targetLine][slotNumber];
            skill.Effect(card);

            BroadcastSkillEffect_LogUpdate(CurrentPlayer, new List<CardModel>(1) { card }, skill);
        }

        /// <summary>
        /// For safety reasons it returns a copy of the list.
        /// </summary>
        public List<CardModel> SpawnRandomDeck(PlayerIndicator player, bool doNotSendCopy = false)
        {
            List<CardModel> targetLine = _lines[player == PlayerIndicator.Top ? 0 : 5];
            for (int i = 0; i < NUMBER_OF_CARDS_IN_DECK; i++)
            {
                int cardType = UnityEngine.Random.Range(0, DB.Length);
                var card = new CardModel(cardType, player) { SlotNumber = i };
                targetLine.Add(card);
            }

            BroadcastUpdateStrength_StatusUpdate();

            if (doNotSendCopy)
                return null;

            return new List<CardModel>(targetLine); // encapsulation
        }

        string GetCurrentStatus()
        {
            var sb = new StringBuilder();

            for (int i = 0; i < _lines.Length; i++)
            {
                sb.Append($"{i + 1}. ");
                foreach (CardModel c in _lines[i])
                    sb.Append(c);
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public List<List<int>> GetCardStrengths()
        {
            var currentStrengths = new List<List<int>>(4); // size of 4 because we don't need to send decks
            for (int i = 1; i < _lines.Length - 1; i++)
            {
                var line = new List<int>();
                _lines[i].ForEach(model => line.Add(model.CurrentStrength));
                currentStrengths.Add(line);
            }

            return currentStrengths;
        }

        #region Event Broadcasting
        internal void BroadcastMoveCard_StatusUpdate(MoveData move)
        {
            if (GameLogicStatusChangedEventHandler == null)
                ReturnControl(); // interface not attached - immediately return control
            else
                GameLogicStatusChangedEventHandler.Invoke(
                    this,
                    new GameLogicStatusChangedEventArgs(GameLogicMessageType.MoveCard, CurrentPlayer)
                    {
                        LastMove = move,
                        TopTotalStrength = TopTotalStrength,
                        BotTotalStrength = BotTotalStrength,
                    });
        }

        internal void BroadcastPlayVFX_StatusUpdate(List<SkillTargetData> targets, SkillVisualEffect visualEffect)
        {
            if (GameLogicStatusChangedEventHandler == null)
                ReturnControl(); // interface not attached - immediately return control
            else
                GameLogicStatusChangedEventHandler.Invoke(
                this,
                new GameLogicStatusChangedEventArgs(GameLogicMessageType.PlaySkillVFX, CurrentPlayer)
                {
                    Targets = targets,
                    VisualEffect = visualEffect,
                    TopTotalStrength = TopTotalStrength,
                    BotTotalStrength = BotTotalStrength
                });
        }

        // early evaluation
        internal void BroadcastUpdateStrength_StatusUpdate(List<List<int>> cardStrengths)
        {
            if (GameLogicStatusChangedEventHandler == null)
                ReturnControl(); // interface not attached - immediately return control
            else
                GameLogicStatusChangedEventHandler.Invoke(
                    this,
                    new GameLogicStatusChangedEventArgs(GameLogicMessageType.UpdateStrength, CurrentPlayer)
                    {
                        CardStrengths = cardStrengths,
                        TopTotalStrength = TopTotalStrength,
                        BotTotalStrength = BotTotalStrength,
                    });
        }

        // late evaluation
        internal void BroadcastUpdateStrength_StatusUpdate()
        {
            if (GameLogicStatusChangedEventHandler == null)
                ReturnControl(); // interface not attached - immediately return control
            else
                GameLogicStatusChangedEventHandler.Invoke(
                    this,
                    new GameLogicStatusChangedEventArgs(GameLogicMessageType.UpdateStrength, CurrentPlayer)
                    {
                        CardStrengths = GetCardStrengths(),
                        TopTotalStrength = TopTotalStrength,
                        BotTotalStrength = BotTotalStrength,
                    });
        }

        internal void BroadcastEndTurn_StatusUpdate()
        {
            CurrentPlayer = CurrentPlayer.Opposite();

            if (GameLogicStatusChangedEventHandler == null)
                ReturnControl(); // interface not attached - immediately return control
            else
                GameLogicStatusChangedEventHandler.Invoke(
                    this,
                    new GameLogicStatusChangedEventArgs(GameLogicMessageType.EndTurn, CurrentPlayer)
                    {
                        TopTotalStrength = TopTotalStrength,
                        BotTotalStrength = BotTotalStrength,
                    });
        }

        internal void BroadcastGameOver_StatusUpdate()
        {
            if (GameLogicStatusChangedEventHandler == null)
                ReturnControl(); // interface not attached - immediately return control
            else
                GameLogicStatusChangedEventHandler.Invoke(
                    this,
                    new GameLogicStatusChangedEventArgs(GameLogicMessageType.GameOver, CurrentPlayer)
                    {
                        TopTotalStrength = TopTotalStrength,
                        BotTotalStrength = BotTotalStrength,
                    });
        }

        internal void BroadcastCardMove_LogUpdate(PlayerIndicator player, MoveData move)
        {
#if UNITY_EDITOR
            if (move == null)
                throw new ArgumentNullException("move", "Move argument cannot be null in this context.");
#endif

            string cardTitle = DB[move.Card.CardId].Title;
            string lastExecutedCommand = 
                $"Player <color=yellow><b>{player.ToString()}</b></color> moved card <color=yellow><b>{cardTitle}</b></color> "
                + $"from {move.FromLine.ToString()} slot {move.FromSlotNumber} "
                + $"to {move.TargetLine.ToString()} slot {move.TargetSlotNumber}";

            GameLogicLogUpdateEventHandler?.Invoke(
                this,
                new GameLogicLogUpdateEventArgs(
                    player, GetCurrentStatus(), lastExecutedCommand, TopTotalStrength, BotTotalStrength));
        }

        internal void BroadcastSkillEffect_LogUpdate(PlayerIndicator player, List<CardModel> targets, CardSkill skill)
        {
#if UNITY_EDITOR
            if (skill == null)
                throw new ArgumentNullException("skill", "Skill argument cannot be null in this context.");
#endif

            if (targets == null || targets.Count == 0)
                return;

            // a bit of a hack
            CardModel cardDummy = new CardModel(0, PlayerIndicator.Bot);
            skill.Effect(cardDummy);

            // true - heals, false - damages
            bool heals = cardDummy.CurrentStrength > cardDummy.DefaultStrength;

            string cardTitle = "ToBeDetermined";
            string lastExecutedCommand;
            if (targets.Count == 1)
            {
                int howPowerful = Math.Abs(cardDummy.DefaultStrength - cardDummy.CurrentStrength);
                lastExecutedCommand = $"<color=yellow><b>{CurrentPlayer.ToString()}</b></color> Player's card <b>{cardTitle}</b> "
                    + $"{HealsOrDamages()} enemy's {DB[targets[0].CardId].Title} for {howPowerful} damage";
            }
            else // many targets
                lastExecutedCommand = $"<b>{CurrentPlayer.ToString()}</b> Player's card <color=yellow><b>{cardTitle}</b></color> "
                    + $"{HealsOrDamages()} following cards: " + string.Join(", ", targets);

            string HealsOrDamages() => heals ? "<color=green><b>heals</b></color>" : "<color=red>damages</b></color>";

            GameLogicLogUpdateEventHandler?.Invoke(
                this,
                new GameLogicLogUpdateEventArgs(
                    player, GetCurrentStatus(), lastExecutedCommand, TopTotalStrength, BotTotalStrength));
        }
        #endregion

        #region Assertions
#if UNITY_EDITOR
        void MoveDataAssertions(LineIndicator fromLine, int fromSlotNumber, LineIndicator targetLine, int targetSlotNumber)
        {
            if (fromSlotNumber < 0 || fromSlotNumber > _lines[(int)fromLine].Count)
                throw new System.ArgumentOutOfRangeException(
                    "fromSlotNumber",
                    "FromSlotNumber cannot be lower than 0 or greater than the number of cards in the line.");

            if (targetSlotNumber < 0 || targetSlotNumber > _lines[(int)targetLine].Count)
                throw new System.ArgumentOutOfRangeException(
                    "targetSlotNumber",
                    "TargetSlotNumber cannot be lower than 0 or greater than the number of cards in the line.");

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
