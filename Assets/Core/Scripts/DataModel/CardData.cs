using Assets.Core.CardSkills;
using System.Collections.Generic;

namespace Assets.Core.DataModel
{
    public class CardData
    {
        public readonly List<CardSkill> Skills;
        public readonly string Title;
        public readonly string Description;
        public readonly int Strength;

        public CardData(string title, string description, int strength, List<CardSkill> skills = null)
        {
            Skills = skills ?? new List<CardSkill>(0);
            Title = title;
            Description = description;
            Strength = strength;
        }
    }
}
