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
                title: "Stupid Dog",
                description: "Is so stupid that it bites its own allies for no reason. "
                    + "When deployed bites an ally unit on the right inflicting 1 DMG.",
                strength: 3
                ,
                skill: new CardSkill(
                    executionTime: SkillExecutionTime.OnDeployAutomatic,
                    targets: new List<SkillTarget> { SkillTarget.RightNeighbor },
                    effect: card => card.CurrentStrength--,
                    visualEffect: SkillVisualEffect.FireBall)
                ),
            new CardData(
                title: "Smelly Fish",
                description: "Stinks like a dumpster. "
                    + "When deployed inflicts 1 DMG to all units at your line (except the fish itself) and the corresponding enemy line.",
                strength: 2,
                skill: new CardSkill(
                    executionTime: SkillExecutionTime.OnDeployAutomatic, 
                    targets: new List<SkillTarget> { SkillTarget.AllInLineExceptMe, SkillTarget.CorrespondingEnemyLine }, 
                    effect: card => card.CurrentStrength--,
                    visualEffect: SkillVisualEffect.GreenCloud)
                ),
            new CardData(
                title: "Green Dude", 
                description: "He can't do anything special but he is a decent warrior.", 
                strength: 5),
            new CardData(
                title: "Elven Archer",
                description: "On deploy he shot one enemy of your choice in the eye inflicting 3 DMG.",
                strength: 3,
                skill: new CardSkill(
                    executionTime: SkillExecutionTime.OnDeployManual,
                    targets: new List<SkillTarget> { SkillTarget.SingleEnemy },
                    effect: card => card.CurrentStrength -= 3,
                    visualEffect: SkillVisualEffect.GreenCloud)),
            new CardData(
                title: "Tree of Eternity",
                description: "Heals allies on left and right. When deployed increases strength of nearby allies by 1.",
                strength: 6,
                skill: new CardSkill(
                    executionTime: SkillExecutionTime.OnDeployAutomatic,
                    targets: new List<SkillTarget> { SkillTarget.BothNeighbors },
                    effect: card => card.CurrentStrength++,
                    visualEffect: SkillVisualEffect.Heal)
                ),
            new CardData(
                title: "Troll",
                description: "When deployed he trolls an ally unit on the left side inflicting 1 DMG. "
                    + "But that's not all he also trolls the whole enemy corresponding line inflicting 1 DMG to every unit.",
                strength: 4,
                skill: new CardSkill(
                    executionTime: SkillExecutionTime.OnDeployAutomatic,
                    targets: new List<SkillTarget> { SkillTarget.LeftNeighbor, SkillTarget.CorrespondingEnemyLine },
                    effect: card => card.CurrentStrength--,
                    visualEffect: SkillVisualEffect.GreenCloud)
                ),
            new CardData(
                title: "Unicorn",
                description: "When deployed heal an ally on the left side by 3.",
                strength: 5,
                skill: new CardSkill(
                    executionTime: SkillExecutionTime.OnDeployAutomatic,
                    targets: new List<SkillTarget> { SkillTarget.LeftNeighbor },
                    effect: card => card.CurrentStrength += 3,
                    visualEffect: SkillVisualEffect.Heal)
                ),
            new CardData(
                title: "Sorceress",
                description: "When deployed she casts a powerful spell, healing 1 strength of each unit (including herself) in the line of choice.",
                strength: 3,
                skill: new CardSkill(
                    executionTime: SkillExecutionTime.OnDeployManual,
                    targets: new List<SkillTarget> { SkillTarget.AllyLine },
                    effect: card => card.CurrentStrength += 1,
                    visualEffect: SkillVisualEffect.Heal)
                ),
            new CardData(
                title: "Wizard",
                description: "When deployed he unleashes powerful fire magic, inflicting 2 damage to all units in the line of choice.",
                strength: 4,
                skill: new CardSkill(
                    executionTime: SkillExecutionTime.OnDeployManual,
                    targets: new List<SkillTarget> { SkillTarget.EnemyLine },
                    effect: card => card.CurrentStrength -= 2,
                    visualEffect: SkillVisualEffect.FireBall)
                )
        };
    }
}
