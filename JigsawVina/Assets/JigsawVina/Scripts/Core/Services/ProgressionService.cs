using System;
using System.Collections.Generic;
using JigsawVina.Core.Services;

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

        public PictureCardState GetPictureState(int pictureId) => throw new NotImplementedException();
        public UnlockResult TryUnlockPicture(int pictureId) => throw new NotImplementedException();
        public bool IsDifficultyUnlocked(int pictureId, int difficultyId) => throw new NotImplementedException();
        public IReadOnlyList<ItemSourceHint> GetItemSourceHints(int itemId) => throw new NotImplementedException();
    }
}
