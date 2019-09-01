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
            // I added param names for readability (thx to that it looks more like JSON)
            new CardData(
                title: "Dog",
                description: "Likes to chase mailmen. "
                    + "Once a turn he attacks first enemy unit that goes from frontline to backline inflicting 1 DMG.",
                strength: 2),
            new CardData(
                title: "Smelly Fish",
                description: "Stinks like a dumpster. "
                    + "When deployed inflicts 1 DMG to all units at your line (except the fish itself) and the corresponding enemy line.",
                strength: 1,
                skill: new CardSkill(
                    executionTime: SkillExecutionTime.OnDeploy, 
                    targets: new List<SkillTarget> { SkillTarget.AllInLineExceptMe, SkillTarget.CorrespondingEnemyLine }, 
                    effect: card => card.CurrentStrength--,
                    visualEffect: SkillVisualEffect.GreenCloud)
                ),
            new CardData(
                title: "Green Dude", 
                description: "He can't do anything special but he is a decent warrior.", 
                strength: 3)
        };
    }
}
