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
            builder.Register<PictureSelectPresenter>(Lifetime.Singleton);
            builder.Register<DifficultySelectPresenter>(Lifetime.Singleton);
            builder.RegisterEntryPoint<HomeFlowController>();
        }
    }

    public class HomeFlowController : IStartable
    {
        private readonly PictureSelectView _pictureSelectView;
        private readonly DifficultySelectView _difficultySelectView;

        public HomeFlowController(
            PictureSelectView pictureSelectView,
            DifficultySelectView difficultySelectView,
            PictureSelectPresenter pictureSelectPresenter,
            DifficultySelectPresenter difficultySelectPresenter)
        {
            _pictureSelectView = pictureSelectView;
            _difficultySelectView = difficultySelectView;
            _ = pictureSelectPresenter;
            _ = difficultySelectPresenter;
        }

        public void Start()
        {
            _pictureSelectView.SetActive(true);
            _difficultySelectView.SetActive(false);

            _pictureSelectView.OnPictureSelected += _ =>
            {
                _pictureSelectView.SetActive(false);
                _difficultySelectView.SetActive(true);
            };

            if (_difficultySelectView.BackButton != null)
            {
                _difficultySelectView.BackButton.onClick.AddListener(() =>
                {
                    _difficultySelectView.SetActive(false);
                    _pictureSelectView.SetActive(true);
                });
            }
        }
    }
}
