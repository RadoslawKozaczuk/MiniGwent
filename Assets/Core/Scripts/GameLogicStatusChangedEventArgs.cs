using Assets.Core.DataModel;

namespace Assets.Core
{
    public sealed class GameLogicStatusChangedEventArgs
    {
        public GameLogicMessageType MessageType;
        public MoveData LastMove;

        public string CurrentStatus;
        public int OverallTopStrength;
        public int OverallBotStrength;

        public GameLogicStatusChangedEventArgs(GameLogicMessageType type, string internalStatus, int topStrength, int botStrength)
        {
            MessageType = type;
            CurrentStatus = internalStatus;
            OverallTopStrength = topStrength;
            OverallBotStrength = botStrength;
        }

        public GameLogicStatusChangedEventArgs(string internalStatus, int topStrength, int botStrength, MoveData lastMove)
        {
            CurrentStatus = internalStatus;
            OverallTopStrength = topStrength;
            OverallBotStrength = botStrength;
            LastMove = lastMove;
        }
    }
}
