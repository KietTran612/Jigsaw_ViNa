using System;
using JigsawVina.Core.Services;
using VContainer;
using VContainer.Unity;

namespace JigsawVina.Presentation.Screens
{
    public class HomeLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<PictureSelectView>();
            builder.RegisterComponentInHierarchy<DifficultySelectView>();
            builder.RegisterComponentInHierarchy<CollectionView>();
            builder.Register<PictureSelectPresenter>(Lifetime.Singleton);
            builder.Register<DifficultySelectPresenter>(Lifetime.Singleton);
            builder.Register<CollectionPresenter>(Lifetime.Singleton);
            builder.RegisterEntryPoint<HomeFlowController>();
        }
    }

    public class HomeFlowController : IStartable, IDisposable
    {
        private readonly PictureSelectView _pictureSelectView;
        private readonly DifficultySelectView _difficultySelectView;
        private readonly DifficultySelectPresenter _difficultySelectPresenter;
        private readonly CollectionView _collectionView;
        private readonly CollectionPresenter _collectionPresenter;
        private readonly ProgressionService _progressionService;

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
            ProgressionService progressionService)
        {
            _pictureSelectView = pictureSelectView;
            _difficultySelectView = difficultySelectView;
            _ = pictureSelectPresenter;
            _difficultySelectPresenter = difficultySelectPresenter;
            _collectionView = collectionView;
            _collectionPresenter = collectionPresenter;
            _progressionService = progressionService;
        }

        public void Start()
        {
            _pictureSelectView.SetActive(true);
            _difficultySelectView.SetActive(false);
            _collectionView?.SetActive(false);

            _pictureSelectView.OnPictureSelected += HandlePictureSelected;
            _pictureSelectView.OnCollectionRequested += HandleCollectionRequested;
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

        public void Dispose()
        {
            if (_pictureSelectView != null)
            {
                _pictureSelectView.OnPictureSelected -= HandlePictureSelected;
                _pictureSelectView.OnCollectionRequested -= HandleCollectionRequested;
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
