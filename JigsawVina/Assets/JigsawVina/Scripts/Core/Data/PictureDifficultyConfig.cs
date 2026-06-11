using System.Collections.Generic;

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
        public readonly int FirstClearCoin;
        public readonly int FirstClearHint;
        public readonly int ReplayCoin;
        public readonly IReadOnlyList<int> FirstClearRewardItemIds;

        public int PieceCount => Columns * Rows;

        public PictureDifficultyConfig(
            int pictureId,
            int difficultyId,
            string displayName,
            int columns,
            int rows,
            int starReward) : this(
                pictureId,
                difficultyId,
                displayName,
                columns,
                rows,
                starReward,
                starReward * 10,
                0,
                0,
                new List<int>())
        {
        }

        public PictureDifficultyConfig(
            int pictureId,
            int difficultyId,
            string displayName,
            int columns,
            int rows,
            int starReward,
            int firstClearCoin,
            int firstClearHint,
            int replayCoin,
            IReadOnlyList<int> firstClearRewardItemIds)
        {
            PictureId = pictureId;
            DifficultyId = difficultyId;
            DisplayName = displayName;
            Columns = columns;
            Rows = rows;
            StarReward = starReward;
            FirstClearCoin = firstClearCoin;
            FirstClearHint = firstClearHint;
            ReplayCoin = replayCoin;
            FirstClearRewardItemIds = firstClearRewardItemIds ?? new List<int>();
        }
    }
}
