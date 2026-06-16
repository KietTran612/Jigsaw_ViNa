using System;
using System.Collections.Generic;
using System.Linq;
using JigsawVina.Core.Data;
using JigsawVina.Core.Services;
using UnityEngine;

namespace JigsawVina.Presentation.Screens
{
    public class PictureSelectPresenter : IDisposable
    {
        private readonly PictureSelectView _view;
        private readonly GameSessionService _sessionService;
        private readonly IStaticDataService _staticDataService;
        private readonly ISaveDataService _saveDataService;
        private readonly ProgressionService _progressionService;

        public PictureSelectPresenter(
            PictureSelectView view,
            GameSessionService sessionService,
            IStaticDataService staticDataService,
            ISaveDataService saveDataService,
            ProgressionService progressionService)
        {
            _view = view;
            _sessionService = sessionService;
            _staticDataService = staticDataService;
            _saveDataService = saveDataService;
            _progressionService = progressionService;

            _view.OnPictureSelected += HandlePictureSelected;
            _view.OnPictureUnlockRequested += HandlePictureUnlockRequested;

            Refresh();
        }

        public void Refresh()
        {
            var pictures = _staticDataService.GetAllPictures();
            if (pictures == null || pictures.Count == 0)
            {
                Debug.LogError("[JigsawVina] StaticData error: No pictures found in IStaticDataService.");
                return;
            }

            var save = _saveDataService.Load() ?? new PlayerSave();
            save.Normalize();

            var models = pictures.Select(picture => new PictureCardPresentationModel
            {
                Config = picture,
                State = _progressionService.GetPictureState(picture.Id),
                MissingItemsHint = BuildMissingItemsHint(picture, save)
            }).ToList();

            _view.Setup(models);
        }

        private void HandlePictureSelected(int pictureId)
        {
            _sessionService.SetSelectedPicture(pictureId);
        }

        private void HandlePictureUnlockRequested(int pictureId)
        {
            if (_progressionService.TryUnlockPicture(pictureId) == UnlockResult.Success)
            {
                Refresh();
            }
        }

        private string BuildMissingItemsHint(PictureConfig picture, PlayerSave save)
        {
            var missingItemIds = picture.UnlockRequirements
                .Where(itemId => !save.OwnedItemIds.Contains(itemId))
                .ToList();
            if (missingItemIds.Count == 0)
            {
                return string.Empty;
            }

            var hints = new List<string>();
            foreach (int itemId in missingItemIds)
            {
                var item = _staticDataService.GetItemById(itemId);
                string itemName = item != null && !string.IsNullOrEmpty(item.display_name)
                    ? item.display_name
                    : $"Item #{itemId}";

                var sources = _progressionService.GetItemSourceHints(itemId);
                if (sources.Count == 0)
                {
                    hints.Add($"Missing: {itemName}. Source unavailable.");
                    continue;
                }

                var sourceLabels = new List<string>();
                foreach (var source in sources)
                {
                    var sourcePicture = _staticDataService.GetPictureById(source.PictureId);
                    var sourceDifficulty = _staticDataService.GetPictureDifficulty(
                        source.PictureId,
                        source.DifficultyId);
                    sourceLabels.Add($"{sourcePicture.DisplayName} - {sourceDifficulty.DisplayName}");
                }
                hints.Add($"Missing: {itemName}. Source: {string.Join(", ", sourceLabels)}");
            }

            return string.Join("\n", hints);
        }

        public void Dispose()
        {
            if (_view != null)
            {
                _view.OnPictureSelected -= HandlePictureSelected;
                _view.OnPictureUnlockRequested -= HandlePictureUnlockRequested;
            }
        }
    }
}
