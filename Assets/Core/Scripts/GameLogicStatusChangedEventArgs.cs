using Assets.Core.CardSkills;
using Assets.Core.DataModel;
using System.Collections.Generic;

namespace Assets.Core
{
    /// <summary>
    /// This event data is sent when something changes.
    /// </summary>
    public sealed class GameLogicStatusChangedEventArgs
    {
        public GameLogicMessageType MessageType;
        public PlayerIndicator CurrentPlayer;
        public MoveData LastMove;
        public int TopTotalStrength;
        public int BotTotalStrength;
        public List<List<int>> CardStrengths;

        public List<SkillTargetData> Targets;
        public CardSkill Skill;
        public SkillVisualEffect VisualEffect;

        public GameLogicStatusChangedEventArgs(GameLogicMessageType type, PlayerIndicator currentPlayer)
        {
            MessageType = type;
            CurrentPlayer = currentPlayer;
        }
    }
}
