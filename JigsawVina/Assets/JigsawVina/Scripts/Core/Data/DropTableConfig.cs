namespace JigsawVina.Core.Data
{
    public readonly struct DropTableConfig
    {
        public readonly int Id;
        public readonly string IdString;
        public readonly string DisplayName;
        public readonly string DisplayNameKey;
        public readonly string DescriptionKey;
        public readonly string ResetRule;
        public readonly string Status;
        public readonly int SortOrder;

        public DropTableConfig(int id, string idString, string displayName, string displayNameKey, string descriptionKey, string resetRule, string status, int sortOrder)
        {
            Id = id;
            IdString = idString;
            DisplayName = displayName;
            DisplayNameKey = displayNameKey;
            DescriptionKey = descriptionKey;
            ResetRule = resetRule;
            Status = status;
            SortOrder = sortOrder;
        }
    }
}
