using Assets.Core.CardSkills;
using Assets.Core.DataModel;

namespace Assets.Core
{
    public sealed class DummyDB
    {
        public CardData this[int id] => _cards[id];

        public int Length => _cards.Length;

        static readonly CardData[] _cards = new CardData[]
        {
            new CardData(
                "Dog",
                "Likes to chase mailmen. Once a turn he attacks first enemy unit that goes from frontline to backline inflicting 1 dmg.",
                2),
            new CardData(
                "Smelly Fish",
                "Stinks like a dumpster, when deployed inflicts 1 dmg to all units at your line and the corresponding enemy line.",
                1)
            {
                Skill = new CardSkill(CardSkillExecutionMoment.OnDeploy, CardSkillExecutionControlType.Automatic)
            },
            new CardData("Green Dude", "He can't do anything special but he is a decent warrior.", 3)
        };
    }
}
