using System.Collections.Generic;

namespace Assets.Core.Scripts
{
    /// <summary>
    /// This event data is debug data.
    /// Contains current status of the board as well as execution stack.
    /// </summary>
    public sealed class GameLogicDebugLogEventArgs
    {
        public readonly string CurrentStatus;
        public readonly int OverallTopStrength;
        public readonly int OverallBotStrength;
        public readonly List<List<int>> CurrentCardStrengths;
        public readonly string LastExecutionCall;

        public GameLogicMessageType MessageType;

        public GameLogicDebugLogEventArgs(GameLogicMessageType type, string internalStatus,
            int topStrength, int botStrength)
        {
            MessageType = type;
            CurrentStatus = internalStatus;
            OverallTopStrength = topStrength;
            OverallBotStrength = botStrength;
        }

        public GameLogicDebugLogEventArgs(string internalStatus, int topStrength, int botStrength, List<List<int>> currentCardStrengths)
        {
            CurrentStatus = internalStatus;
            OverallTopStrength = topStrength;
            OverallBotStrength = botStrength;
            CurrentCardStrengths = currentCardStrengths;
        }
    }
}
