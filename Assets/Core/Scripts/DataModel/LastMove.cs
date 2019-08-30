namespace Assets.Core.DataModel
{
    public sealed class MoveData
    {
        public Line FromLine;
        public int FromSlotNumber;
        public Line TargetLine;
        public int TargetSlotNumber;

        public MoveData(Line fromLine, int fromSlotNumber, Line targetLine, int targetSlotNumber)
        {
            FromLine = fromLine;
            FromSlotNumber = fromSlotNumber;
            TargetLine = targetLine;
            TargetSlotNumber = targetSlotNumber;
        }
    }
}
