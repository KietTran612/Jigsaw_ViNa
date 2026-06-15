using System;
using System.Collections.Generic;
using System.Linq;
using JigsawVina.Core.Data;
using JigsawVina.Core.Services;

namespace JigsawVina.Presentation.Screens
{
    public class CollectionPresenter : IDisposable
    {
        private readonly CollectionView _view;
        private readonly ISaveDataService _saveDataService;
        private readonly IStaticDataService _staticDataService;

        public event Action<int> OnNavigateToPictureRequested;

        public IReadOnlyList<CollectionItemPresentationModel> CurrentModels { get; private set; } =
            Array.Empty<CollectionItemPresentationModel>();

        public CollectionPresenter(
            CollectionView view,
            ISaveDataService saveDataService,
            IStaticDataService staticDataService)
        {
            _view = view;
            _saveDataService = saveDataService;
            _staticDataService = staticDataService;

            _view.OnItemSelected += HandleItemSelected;
            _view.OnNavigateToPictureRequested += HandleNavigationRequested;
            Refresh();
        }

        public void Refresh()
        {
            var save = _saveDataService.Load() ?? new PlayerSave();
            save.Normalize();

            CurrentModels = _staticDataService.GetAllItems()
                .Where(item =>
                    item != null &&
                    item.item_type == "key_item" &&
                    save.OwnedItemIds.Contains(item.id))
                .OrderBy(item => item.sort_order)
                .ThenBy(item => item.id)
                .Select(item => new CollectionItemPresentationModel
                {
                    ItemId = item.id,
                    DisplayName = item.display_name,
                    Description = item.description,
                    AssetPath = item.asset_path,
                    Sources = BuildSources(item.id)
                })
                .ToList();

            _view.Setup(CurrentModels);
        }

        private IReadOnlyList<CollectionSourcePresentationModel> BuildSources(int itemId)
        {
            var dropTableIds = _staticDataService.GetAllDropTableItems()
                .Where(item =>
                    item.Status == "active" &&
                    item.ItemId == itemId)
                .Select(item => item.DropTableId)
                .ToHashSet();

            return _staticDataService.GetAllPictureDifficulties()
                .Where(difficulty =>
                    difficulty.FirstClearRewardItemIds.Contains(itemId) ||
                    dropTableIds.Contains(difficulty.DropTableId))
                .GroupBy(difficulty => new
                {
                    difficulty.PictureId,
                    difficulty.DifficultyId
                })
                .Select(group => group.First())
                .OrderBy(difficulty => difficulty.PictureId)
                .ThenBy(difficulty => difficulty.DifficultyId)
                .Select(difficulty =>
                {
                    var picture = _staticDataService.GetPictureById(
                        difficulty.PictureId);
                    return new CollectionSourcePresentationModel
                    {
                        PictureId = difficulty.PictureId,
                        DifficultyId = difficulty.DifficultyId,
                        Label = $"{picture.DisplayName} - {difficulty.DisplayName}"
                    };
                })
                .ToList();
        }

        private void HandleItemSelected(int itemId)
        {
            _view.ShowItem(CurrentModels.FirstOrDefault(model => model.ItemId == itemId));
        }

        private void HandleNavigationRequested(int pictureId)
        {
            OnNavigateToPictureRequested?.Invoke(pictureId);
        }

        public void Dispose()
        {
            if (_view != null)
            {
                _view.OnItemSelected -= HandleItemSelected;
                _view.OnNavigateToPictureRequested -= HandleNavigationRequested;
            }
        }
    }
}
