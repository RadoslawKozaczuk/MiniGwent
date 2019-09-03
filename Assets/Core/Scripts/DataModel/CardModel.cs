namespace Assets.Core.DataModel
{
    public class CardModel
    {
        public readonly PlayerIndicator PlayerIndicator;
        public readonly int DefaultStrength;
        public readonly int CardId;

        public LineIndicator CurrentLine;
        public int CurrentStrength;
        public int SlotNumber;

        public CardModel(int id, PlayerIndicator playerIndicator)
        {
            PlayerIndicator = playerIndicator;

            CardId = id;
            DefaultStrength = GameLogic.DB[id].Strength;
            CurrentStrength = DefaultStrength;
        }

        public override string ToString() => $"[{(int)CurrentLine},{SlotNumber}]";

        public string ToStringIdStrDefstr() => $"[{CardId}/{CurrentStrengthStr()}/{DefaultStrength}]";

        string CurrentStrengthStr() 
            => CurrentStrength < DefaultStrength
                ? $"<color=red>{CurrentStrength}</color>"
                : CurrentStrength == DefaultStrength
                    ? $"{CurrentStrength}"
                    : $"<color=green>{CurrentStrength}</color>";
    }
}
