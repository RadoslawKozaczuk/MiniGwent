namespace Assets.Core.DataModel
{
    public sealed class MoveData
    {
        public readonly CardModel Card;
        public readonly LineIndicator FromLine;
        public readonly int FromSlotNumber;
        public readonly LineIndicator TargetLine;
        public readonly int TargetSlotNumber;

        // AI uses abstractions and requires mapping
        public MoveData(CardModel card, PlayerIndicator playerIndicator, int fromSlotNumber, PlayerLine targetLine, int targetSlotNumber)
        {
            Card = card;
            FromLine = playerIndicator == PlayerIndicator.Top ? LineIndicator.TopDeck : LineIndicator.BotDeck;
            TargetLine = playerIndicator == PlayerIndicator.Bot 
                ? (LineIndicator)(5 - (int)targetLine) 
                : (LineIndicator)(int)targetLine;

            FromSlotNumber = fromSlotNumber;
            TargetSlotNumber = targetSlotNumber;
        }

        public MoveData(CardModel card, LineIndicator fromLine, int fromSlotNumber, LineIndicator targetLine, int targetSlotNumber)
        {
            Card = card;
            FromLine = fromLine;
            FromSlotNumber = fromSlotNumber;
            TargetLine = targetLine;
            TargetSlotNumber = targetSlotNumber;
        }
    }
}
