using Assets.Core.CardSkills;

namespace Assets.Core.DataModel
{
    public class CardData
    {
        public readonly string Title;
        public readonly string Description;
        public readonly int Strength;

        public CardSkill Skill;

        public CardData(string title, string description, int strength)
        {
            Title = title;
            Description = description;
            Strength = strength;
        }
    }
}
