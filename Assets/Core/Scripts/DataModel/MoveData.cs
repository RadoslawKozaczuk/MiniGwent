namespace Assets.Core.DataModel
{
    public sealed class MoveData
    {
        public readonly CardModel Card;
        public readonly Line FromLine;
        public readonly int FromSlotNumber;
        public readonly Line TargetLine;
        public readonly int TargetSlotNumber;

        // AI uses abstractions and requires mapping
        public MoveData(CardModel card, PlayerIndicator playerIndicator, int fromSlotNumber, PlayerLine targetLine, int targetSlotNumber)
        {
            Card = card;

            FromLine = playerIndicator == PlayerIndicator.Top ? Line.TopDeck : Line.BotDeck;

            // TopDeck = 0, TopBackline = 1, TopFrontline = 2, BotFrontline = 3, BotBackline = 4, BotDeck = 5
            // Deck = 0, Backline = 1, Frontline = 2
            TargetLine = playerIndicator == PlayerIndicator.Bot 
                ? (Line)(5 - (int)targetLine) 
                : (Line)(int)targetLine;

            FromSlotNumber = fromSlotNumber;
            TargetSlotNumber = targetSlotNumber;
        }

        public MoveData(Line fromLine, int fromSlotNumber, Line targetLine, int targetSlotNumber)
        {
            FromLine = fromLine;
            FromSlotNumber = fromSlotNumber;
            TargetLine = targetLine;
            TargetSlotNumber = targetSlotNumber;
        }
    }
}
