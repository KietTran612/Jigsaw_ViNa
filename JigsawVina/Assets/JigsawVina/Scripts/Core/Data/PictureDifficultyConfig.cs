namespace JigsawVina.Core.Data
{
    public readonly struct PictureDifficultyConfig
    {
        public readonly int PictureId;
        public readonly int DifficultyId;
        public readonly string DisplayName;
        public readonly int Columns;
        public readonly int Rows;
        public readonly int StarReward;

        public int PieceCount => Columns * Rows;

        public PictureDifficultyConfig(
            int pictureId,
            int difficultyId,
            string displayName,
            int columns,
            int rows,
            int starReward)
        {
            PictureId = pictureId;
            DifficultyId = difficultyId;
            DisplayName = displayName;
            Columns = columns;
            Rows = rows;
            StarReward = starReward;
        }
    }
}
