using System;
using System.Collections.Generic;

namespace JigsawVina.Core.Data
{
    public readonly struct PictureConfig
    {
        public readonly int Id;
        public readonly string IdString;
        public readonly string DisplayName;
        public readonly string AssetPath;
        public readonly string DisplayNameKey;
        public readonly string DescriptionKey;

        public PictureConfig(int id, string idString, string displayName, string assetPath, string displayNameKey, string descriptionKey)
        {
            Id = id;
            IdString = idString;
            DisplayName = displayName;
            AssetPath = assetPath;
            DisplayNameKey = displayNameKey;
            DescriptionKey = descriptionKey;
        }
    }

    [Serializable]
    public class CompletedPuzzleData
    {
        public int PictureId;
        public int DifficultyId;
        public float BestTimeSeconds;
        public int BestStar;
    }

    [Serializable]
    public class PlayerSave
    {
        public int Coins;
        public int Hints;
        public List<CompletedPuzzleData> CompletedPuzzles = new();
        public List<int> OwnedItemIds = new();
    }
}
