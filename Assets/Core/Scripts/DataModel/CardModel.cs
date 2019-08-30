namespace Assets.Core.DataModel
{
    public class CardModel
    {
        public int CardId;
        public int DefaultStrength;
        public int CurrentStrength;
        public int SlotNumber;

        public CardModel(int id)
        {
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
