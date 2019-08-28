using Assets.Core.DataModel;

namespace Assets.Core
{
    public sealed class DummyDB
    {
        public CardData this[int id] => _cards[id];

        public int Length => _cards.Length;

        static readonly CardData[] _cards = new CardData[]
        {
            new CardData()
            {
                Title = "Dog",
                Description = "Likes to chase mailmen. Once a turn he attacks first enemy unit that goes " +
                    "from frontline to backline inflicting 1 dmg.",
                Strength = 2
            },
            new CardData()
            {
                Title = "Red Fish",
                Description = "Stinks like a dumpster, when deployed inflicts 1 dmg to all units at your line " +
                    "and the corresponding enemy line.",
                Strength = 1
            },
            new CardData()
            {
                Title = "Green Dude",
                Description = "He can't do anything special but he is a decent warrior.",
                Strength = 3
            }
        };
    }
}
