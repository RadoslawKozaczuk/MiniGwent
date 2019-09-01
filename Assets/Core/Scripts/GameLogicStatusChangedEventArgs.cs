using Assets.Core.DataModel;
using System.Collections.Generic;

namespace Assets.Core
{
    public sealed class GameLogicStatusChangedEventArgs
    {
        public readonly MoveData LastMove;
        public readonly string CurrentStatus;
        public readonly int OverallTopStrength;
        public readonly int OverallBotStrength;
        public readonly List<List<int>> CurrentCardStrengths;

        public GameLogicMessageType MessageType;

        public GameLogicStatusChangedEventArgs(GameLogicMessageType type, string internalStatus, 
            int topStrength, int botStrength)
        {
            MessageType = type;
            CurrentStatus = internalStatus;
            OverallTopStrength = topStrength;
            OverallBotStrength = botStrength;
        }

        public GameLogicStatusChangedEventArgs(string internalStatus, int topStrength, int botStrength, 
            MoveData lastMove, List<List<int>> currentCardStrengths)
        {
            CurrentStatus = internalStatus;
            OverallTopStrength = topStrength;
            OverallBotStrength = botStrength;
            LastMove = lastMove;
            CurrentCardStrengths = currentCardStrengths;
        }
    }
}
