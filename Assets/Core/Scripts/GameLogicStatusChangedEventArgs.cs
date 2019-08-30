using Assets.Core.DataModel;

namespace Assets.Core
{
    public sealed class GameLogicStatusChangedEventArgs
    {
        public MoveData LastMove;

        public string CurrentStatus;
        public int OverallTopStrength;
        public int OverallBotStrength;
    }
}
