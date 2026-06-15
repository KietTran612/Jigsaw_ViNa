using System.Collections.Generic;
using System.Linq;
using JigsawVina.Core.Data;

namespace JigsawVina.Core.Services
{
    public enum PictureCardState { Locked, ReadyToUnlock, Unlocked, Completed }
    public enum UnlockResult { Success, AlreadyUnlocked, MissingRequirements, PictureNotFound }
    public struct ItemSourceHint { public int PictureId; public int DifficultyId; }

    public class ProgressionService
    {
        private readonly IStaticDataService _staticData;
        private readonly ISaveDataService _saveData;

        public ProgressionService(IStaticDataService staticData, ISaveDataService saveData)
        {
            _staticData = staticData;
            _saveData = saveData;
        }

        public PictureCardState GetPictureState(int pictureId)
        {
            var picture = _staticData.GetPictureById(pictureId);
            if (picture.Id == 0)
            {
                return PictureCardState.Locked;
            }

            var save = LoadNormalizedSave();
            bool isUnlocked = picture.IsInitiallyUnlocked || save.UnlockedPictureIds.Contains(pictureId);
            if (!isUnlocked)
            {
                return HasAllRequirements(picture, save)
                    ? PictureCardState.ReadyToUnlock
                    : PictureCardState.Locked;
            }

            var difficulties = _staticData.GetPictureDifficulties(pictureId);
            bool isCompleted = difficulties.Count > 0 && difficulties.All(difficulty =>
                save.CompletedPuzzles.Any(completion =>
                    completion.PictureId == pictureId &&
                    completion.DifficultyId == difficulty.DifficultyId &&
                    completion.BestStar >= System.Math.Max(1, difficulty.StarReward)));

            return isCompleted ? PictureCardState.Completed : PictureCardState.Unlocked;
        }

        public UnlockResult TryUnlockPicture(int pictureId)
        {
            var picture = _staticData.GetPictureById(pictureId);
            if (picture.Id == 0)
            {
                return UnlockResult.PictureNotFound;
            }

            var save = LoadNormalizedSave();
            if (picture.IsInitiallyUnlocked || save.UnlockedPictureIds.Contains(pictureId))
            {
                return UnlockResult.AlreadyUnlocked;
            }

            if (!HasAllRequirements(picture, save))
            {
                return UnlockResult.MissingRequirements;
            }

            save.UnlockedPictureIds.Add(pictureId);
            _saveData.Save(save);
            return UnlockResult.Success;
        }

        public bool IsDifficultyUnlocked(int pictureId, int difficultyId)
        {
            var picture = _staticData.GetPictureById(pictureId);
            if (picture.Id == 0)
            {
                return false;
            }

            var configuredDifficulties = _staticData.GetPictureDifficulties(pictureId);
            if (!configuredDifficulties.Any(difficulty => difficulty.DifficultyId == difficultyId))
            {
                return false;
            }

            var save = LoadNormalizedSave();
            bool isPictureUnlocked = picture.IsInitiallyUnlocked ||
                                     save.UnlockedPictureIds.Contains(pictureId);
            if (!isPictureUnlocked)
            {
                return false;
            }

            if (picture.DifficultyUnlockPolicy == "all_unlocked")
            {
                return true;
            }

            if (picture.DifficultyUnlockPolicy != "sequential" || difficultyId < 0)
            {
                return false;
            }

            for (int previousDifficultyId = 0;
                 previousDifficultyId < difficultyId;
                 previousDifficultyId++)
            {
                bool previousCompleted = save.CompletedPuzzles.Any(completion =>
                    completion.PictureId == pictureId &&
                    completion.DifficultyId == previousDifficultyId &&
                    completion.BestStar > 0);
                if (!previousCompleted)
                {
                    return false;
                }
            }

            return true;
        }

        public IReadOnlyList<ItemSourceHint> GetItemSourceHints(int itemId)
        {
            return _staticData.GetAllPictureDifficulties()
                .Where(difficulty => difficulty.FirstClearRewardItemIds.Contains(itemId))
                .OrderBy(difficulty => difficulty.PictureId)
                .ThenBy(difficulty => difficulty.DifficultyId)
                .Select(difficulty => new ItemSourceHint
                {
                    PictureId = difficulty.PictureId,
                    DifficultyId = difficulty.DifficultyId
                })
                .ToList();
        }

        private PlayerSave LoadNormalizedSave()
        {
            var save = _saveData.Load() ?? new PlayerSave();
            save.Normalize();
            return save;
        }

        private static bool HasAllRequirements(
            PictureConfig picture,
            PlayerSave save)
        {
            return picture.UnlockRequirements.All(save.OwnedItemIds.Contains);
        }
    }
}
