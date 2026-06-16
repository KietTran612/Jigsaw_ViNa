using System;
using JigsawVina.Core.Services;
using VContainer;
using VContainer.Unity;

namespace JigsawVina.Presentation.Screens
{
    public class HomeFlowController : IStartable, IDisposable
    {
        private readonly PictureSelectView _pictureSelectView;
        private readonly DifficultySelectView _difficultySelectView;
        private readonly PictureSelectPresenter _pictureSelectPresenter;
        private readonly DifficultySelectPresenter _difficultySelectPresenter;
        private readonly CollectionView _collectionView;
        private readonly CollectionPresenter _collectionPresenter;
        private readonly ProgressionService _progressionService;
        private readonly DailyRewardPresenter _dailyRewardPresenter;
        private readonly IDailyRewardService _dailyRewardService;
        private readonly ISaveDataService _saveDataService;

        public HomeFlowController(
            PictureSelectView pictureSelectView,
            DifficultySelectView difficultySelectView,
            PictureSelectPresenter pictureSelectPresenter,
            DifficultySelectPresenter difficultySelectPresenter)
            : this(
                pictureSelectView,
                difficultySelectView,
                pictureSelectPresenter,
                difficultySelectPresenter,
                null,
                null,
                null,
                null,
                null,
                null)
        {
        }

        public HomeFlowController(
            PictureSelectView pictureSelectView,
            DifficultySelectView difficultySelectView,
            PictureSelectPresenter pictureSelectPresenter,
            DifficultySelectPresenter difficultySelectPresenter,
            CollectionView collectionView,
            CollectionPresenter collectionPresenter,
            ProgressionService progressionService)
            : this(
                pictureSelectView,
                difficultySelectView,
                pictureSelectPresenter,
                difficultySelectPresenter,
                collectionView,
                collectionPresenter,
                progressionService,
                null,
                null,
                null)
        {
        }

        [Inject]
        public HomeFlowController(
            PictureSelectView pictureSelectView,
            DifficultySelectView difficultySelectView,
            PictureSelectPresenter pictureSelectPresenter,
            DifficultySelectPresenter difficultySelectPresenter,
            CollectionView collectionView,
            CollectionPresenter collectionPresenter,
            ProgressionService progressionService,
            DailyRewardPresenter dailyRewardPresenter,
            IDailyRewardService dailyRewardService,
            ISaveDataService saveDataService)
        {
            _pictureSelectView = pictureSelectView;
            _difficultySelectView = difficultySelectView;
            _pictureSelectPresenter = pictureSelectPresenter;
            _difficultySelectPresenter = difficultySelectPresenter;
            _collectionView = collectionView;
            _collectionPresenter = collectionPresenter;
            _progressionService = progressionService;
            _dailyRewardPresenter = dailyRewardPresenter;
            _dailyRewardService = dailyRewardService;
            _saveDataService = saveDataService;
        }

        public void Start()
        {
            _pictureSelectView.SetActive(true);
            _difficultySelectView.SetActive(false);
            _collectionView?.SetActive(false);

            _pictureSelectView.OnPictureSelected += HandlePictureSelected;
            _pictureSelectView.OnCollectionRequested += HandleCollectionRequested;
            _pictureSelectView.OnDailyRewardRequested += HandleDailyRewardRequested;

            if (_dailyRewardPresenter != null)
            {
                _dailyRewardPresenter.OnRewardClaimed += HandleDailyRewardClaimed;
            }

            if (_collectionView != null)
            {
                _collectionView.OnCloseRequested += HandleCollectionClosed;
            }
            if (_collectionPresenter != null)
            {
                _collectionPresenter.OnNavigateToPictureRequested +=
                    HandleCollectionNavigation;
            }

            if (_difficultySelectView.BackButton != null)
            {
                _difficultySelectView.BackButton.onClick.AddListener(HandleBackButtonClicked);
            }

            RefreshBadge();
        }

        private void HandlePictureSelected(int pictureId)
        {
            _difficultySelectPresenter?.Refresh(pictureId);
            _pictureSelectView.SetActive(false);
            _difficultySelectView.SetActive(true);
        }

        private void HandleCollectionRequested()
        {
            _collectionPresenter?.Refresh();
            _pictureSelectView.SetActive(false);
            _difficultySelectView.SetActive(false);
            _collectionView?.SetActive(true);
        }

        private void HandleCollectionClosed()
        {
            _collectionView?.SetActive(false);
            _pictureSelectView.SetActive(true);
        }

        private void HandleCollectionNavigation(int pictureId)
        {
            _collectionView?.SetActive(false);
            _difficultySelectView.SetActive(false);
            _pictureSelectView.SetActive(true);

            var state = _progressionService?.GetPictureState(pictureId)
                ?? PictureCardState.Locked;
            if (state == PictureCardState.Unlocked ||
                state == PictureCardState.Completed)
            {
                _pictureSelectView.RequestPictureSelection(pictureId);
            }
            else
            {
                _pictureSelectView.FocusCard(pictureId);
            }
        }

        private void HandleBackButtonClicked()
        {
            _difficultySelectView.SetActive(false);
            _pictureSelectView.SetActive(true);
        }

        private void HandleDailyRewardRequested()
        {
            _dailyRewardPresenter?.OpenPopup();
        }

        private void HandleDailyRewardClaimed()
        {
            RefreshBadge();
            _pictureSelectPresenter?.Refresh();
            _collectionPresenter?.Refresh();
        }

        private void RefreshBadge()
        {
            if (_saveDataService != null && _dailyRewardService != null && _pictureSelectView != null)
            {
                var save = _saveDataService.Load();
                if (save != null)
                {
                    save.Normalize();
                    _pictureSelectView.SetDailyRewardNotificationBadge(_dailyRewardService.CanClaimToday(save));
                }
            }
        }

        public void Dispose()
        {
            if (_pictureSelectView != null)
            {
                _pictureSelectView.OnPictureSelected -= HandlePictureSelected;
                _pictureSelectView.OnCollectionRequested -= HandleCollectionRequested;
                _pictureSelectView.OnDailyRewardRequested -= HandleDailyRewardRequested;
            }

            if (_dailyRewardPresenter != null)
            {
                _dailyRewardPresenter.OnRewardClaimed -= HandleDailyRewardClaimed;
            }

            if (_difficultySelectView != null && _difficultySelectView.BackButton != null)
            {
                _difficultySelectView.BackButton.onClick.RemoveListener(HandleBackButtonClicked);
            }

            if (_collectionView != null)
            {
                _collectionView.OnCloseRequested -= HandleCollectionClosed;
            }
            if (_collectionPresenter != null)
            {
                _collectionPresenter.OnNavigateToPictureRequested -=
                    HandleCollectionNavigation;
            }
        }
    }
}
