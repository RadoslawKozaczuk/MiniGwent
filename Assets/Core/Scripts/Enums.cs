namespace Assets.Core
{
    public enum Line { TopDeck, TopBackline, TopFrontline, BotFrontline, BotBackline, BotDeck }

    /// <summary>
    /// From player's perspective there are only three lines. 
    /// This enumerator helps express that.
    /// </summary>
    public enum PlayerLine { Deck, Backline, Frontline }

    public enum PlayerIndicator { Top, Bot }

    public enum PlayerControl { Human, AI }

    public enum GameType { Human_vs_AI, AI_vs_AI }

    public enum GameLogicMessageType { Normal, GameOver }

    public enum CardSkillExecutionMoment { OnDeploy, EndOfTurn }

    public enum CardSkillExecutionControlType { Automatic, Manual } // not used atm
    
    public enum CardSkillTarget { RightNeighbor, LeftNeighbor, BothNeighbors, AllInLineExceptMe, CorrespondingEnemyLine }

    public enum VisualEffect { GreenCloud }
}
