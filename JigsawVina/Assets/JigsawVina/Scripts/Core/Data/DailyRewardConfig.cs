namespace JigsawVina.Core.Data
{
    public readonly struct DailyRewardConfig
    {
        public readonly int DayIndex;
        public readonly int ItemId;
        public readonly int Amount;

        public DailyRewardConfig(int dayIndex, int itemId, int amount)
        {
            DayIndex = dayIndex;
            ItemId = itemId;
            Amount = amount;
        }
    }
}
