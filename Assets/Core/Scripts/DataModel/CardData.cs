using Assets.Core.CardSkills;

namespace Assets.Core.DataModel
{
    public class CardData
    {
        public readonly CardSkill Skill;
        public readonly string Title;
        public readonly string Description;
        public readonly int Strength;

        public CardData(string title, string description, int strength, CardSkill skill = null)
        {
            Skill = skill;
            Title = title;
            Description = description;
            Strength = strength;
        }
    }
}
