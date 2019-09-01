﻿namespace Assets.Core
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

    public enum SkillExecutionTime { OnDeploy, EndOfTurn }

    public enum SkillExecutionControlType { Automatic, Manual } // not used atm
    
    public enum SkillTarget { RightNeighbor, LeftNeighbor, BothNeighbors, AllInLineExceptMe, CorrespondingEnemyLine }

    public enum SkillVisualEffect { GreenCloud }
}
