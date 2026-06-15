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
        public readonly bool IsInitiallyUnlocked;
        public readonly string DifficultyUnlockPolicy;
        public readonly IReadOnlyList<int> UnlockRequirements;

        public PictureConfig(
            int id, 
            string idString, 
            string displayName, 
            string assetPath, 
            string displayNameKey, 
            string descriptionKey,
            bool isInitiallyUnlocked,
            string difficultyUnlockPolicy,
            IReadOnlyList<int> unlockRequirements)
        {
            Id = id;
            IdString = idString;
            DisplayName = displayName;
            AssetPath = assetPath;
            DisplayNameKey = displayNameKey;
            DescriptionKey = descriptionKey;
            IsInitiallyUnlocked = isInitiallyUnlocked;
            DifficultyUnlockPolicy = difficultyUnlockPolicy;
            UnlockRequirements = unlockRequirements != null ? new List<int>(unlockRequirements) : new List<int>();
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
    public class DailyDropCount
    {
        public int ItemId;
        public int Count;
    }

    [Serializable]
    public class InventoryItem
    {
        public int ItemId;
        public int Amount;
    }

    [Serializable]
    public class PlayerSave
    {
        public int Coins;
        public int Hints;
        public List<CompletedPuzzleData> CompletedPuzzles = new();
        public List<int> OwnedItemIds = new();
        public List<int> UnlockedPictureIds = new();
        public string LastSaveDateString;
        public List<DailyDropCount> DailyDropCounts = new();
        public List<InventoryItem> Inventory = new();

        public void Normalize()
        {
            if (CompletedPuzzles == null) CompletedPuzzles = new();
            if (OwnedItemIds == null) OwnedItemIds = new();
            if (UnlockedPictureIds == null) UnlockedPictureIds = new();
            if (DailyDropCounts == null) DailyDropCounts = new();
            if (Inventory == null) Inventory = new();
        }

        public void Normalize(string localDateString)
        {
            Normalize();
            if (LastSaveDateString != localDateString)
            {
                DailyDropCounts.Clear();
                LastSaveDateString = localDateString;
            }
        }
    }
}
