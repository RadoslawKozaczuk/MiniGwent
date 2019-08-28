namespace Assets.Core.DataModel
{
    public class CardModel
    {
        public int CardId;
        public int Strength;

        public CardModel(int id, int strength)
        {
            CardId = id;
            Strength = strength;
        }

        public override string ToString() 
            => Utils.CountDigit(CardId) <= 1 
                ? $"[0{CardId}]" 
                : $"[{CardId}]";
    }
}
