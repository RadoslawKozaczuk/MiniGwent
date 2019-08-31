using Assets.Core.CardSkills;
using Assets.Core.DataModel;
using System.Collections.Generic;

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
                "Stinks like a dumpster. When deployed inflicts 1 DMG to all units at your line (except the fish) and the corresponding enemy line.",
                1,
                new List<CardSkill>()
                {
                    new CardSkill(
                        CardSkillExecutionMoment.OnDeploy, 
                        CardSkillTarget.AllInLineExceptMe, 
                        card => card.CurrentStrength--,
                        VisualEffect.GreenCloud),
                    new CardSkill(
                        CardSkillExecutionMoment.OnDeploy, 
                        CardSkillTarget.CorrespondingEnemyLine, 
                        card => card.CurrentStrength--,
                        VisualEffect.GreenCloud)
                }),
            new CardData("Green Dude", "He can't do anything special but he is a decent warrior.", 3)
        };
    }
}
