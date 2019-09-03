namespace Assets.Core
{
    /// <summary>
    /// This event data is sent when something changes.
    /// </summary>
    public sealed class GameLogicLogUpdateEventArgs
    {
        public readonly PlayerIndicator CurrentPlayer;
        public readonly string CurrentStatus;
        public readonly string LastExecutedCommand;
        public readonly int TopTotalStrength;
        public readonly int BotTotalStrength;

        public GameLogicLogUpdateEventArgs(PlayerIndicator playerIndicator, string currentStatus, string lastExecutedCommand, 
            int topStrengthSum, int botStrengthSum)
        {
            CurrentPlayer = playerIndicator;
            CurrentStatus = currentStatus;
            LastExecutedCommand = lastExecutedCommand;
            TopTotalStrength = topStrengthSum;
            BotTotalStrength = botStrengthSum;
        }
    }
}
