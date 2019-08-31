namespace Assets.Core.DataModel
{
    public class CardModel
    {
        public readonly PlayerIndicator PlayerIndicator;
        public readonly int DefaultStrength;

        public int CardId;
        public int CurrentStrength;
        public int SlotNumber;

        public CardModel(int id, PlayerIndicator playerIndicator)
        {
            PlayerIndicator = playerIndicator;

            CardId = id;
            DefaultStrength = GameLogic.DB[id].Strength;
            CurrentStrength = DefaultStrength;
        }

        //public override string ToString() 
        //    => Utils.CountDigit(CardId) <= 1 
        //        ? $"[0{CardId}]" 
        //        : $"[{CardId}]";


        public override string ToString() => $"[t{CardId}s{SlotNumber}]";
    }
}
