namespace Assets.Core.DataModel
{
    public sealed class SkillTargetData
    {
        public readonly LineIndicator Line;
        public readonly int SlotNumber;

        public SkillTargetData(LineIndicator line, int slotNumber)
        {
            Line = line;
            SlotNumber = slotNumber;
        }

        public override string ToString() => $"[{(int)Line},{SlotNumber}]";
    }
}
