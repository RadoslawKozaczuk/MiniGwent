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

    public enum GameType { HumanVsAi, AIVsAI }

    public enum GameLogicMessageType { Normal, GameOver }
}
