namespace JigsawVina.Core.Data
{
    public readonly struct DropTableItemConfig
    {
        public readonly int Id;
        public readonly string IdString;
        public readonly string DisplayName;
        public readonly int DropTableId;
        public readonly int ItemId;
        public readonly float BaseRate;
        public readonly float DecayPerSuccess;
        public readonly float MinRate;
        public readonly int AmountMin;
        public readonly int AmountMax;
        public readonly string Status;

        public DropTableItemConfig(int id, string idString, string displayName, int dropTableId, int itemId, float baseRate, float decayPerSuccess, float minRate, int amountMin, int amountMax, string status)
        {
            Id = id;
            IdString = idString;
            DisplayName = displayName;
            DropTableId = dropTableId;
            ItemId = itemId;
            BaseRate = baseRate;
            DecayPerSuccess = decayPerSuccess;
            MinRate = minRate;
            AmountMin = amountMin;
            AmountMax = amountMax;
            Status = status;
        }
    }
}
