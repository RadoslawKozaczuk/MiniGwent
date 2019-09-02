namespace Assets.Core
{
    public enum LineIndicator { TopDeck, TopBackline, TopFrontline, BotFrontline, BotBackline, BotDeck }

    /// <summary>
    /// From player's perspective there are only three lines. 
    /// This enumerator helps express that.
    /// </summary>
    public enum PlayerLine { Deck, Backline, Frontline }

    public enum PlayerIndicator { Top, Bot }

    public enum PlayerControl { Human, AI }

    public enum GameLogicMessageType { MoveCard, PlaySkillVFX, UpdateStrength, EndTurn, GameOver }

    public enum SkillExecutionTime { OnDeployAutomatic, OnDeployManual, UponDeath }

    public enum SkillTarget {
        // use together with OnDeployAutomatic and UponDeath
        RightNeighbor, LeftNeighbor, BothNeighbors, AllInLineExceptMe, CorrespondingEnemyLine,
        // use together with OnDeployManual
        SingleEnemy, EnemyLine
    }

    public enum SkillVisualEffect { GreenCloud }
}
