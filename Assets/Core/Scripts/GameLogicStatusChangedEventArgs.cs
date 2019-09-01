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
        public MoveData LastMove;
        public string CurrentStatus;
        public int OverallTopStrength;
        public int OverallBotStrength;
        public List<List<int>> CardStrengths;

        public (Line targetLine, int targetSlot) SkillTarget;
        public CardSkill Skill;

        public GameLogicStatusChangedEventArgs(GameLogicMessageType type)
        {
            MessageType = type;
        }

        //public GameLogicStatusChangedEventArgs(GameLogicMessageType type, string internalStatus, 
        //    int topStrength, int botStrength)
        //{
        //    MessageType = type;
        //    CurrentStatus = internalStatus;
        //    OverallTopStrength = topStrength;
        //    OverallBotStrength = botStrength;
        //}

        //public GameLogicStatusChangedEventArgs(string internalStatus, int topStrength, int botStrength, 
        //    List<List<int>> currentCardStrengths)
        //{
        //    CurrentStatus = internalStatus;
        //    OverallTopStrength = topStrength;
        //    OverallBotStrength = botStrength;
        //    CurrentCardStrengths = currentCardStrengths;
        //}
    }
}
