using Cysharp.Threading.Tasks;
using JigsawVina.Core.Services;
using VContainer;
using VContainer.Unity;

namespace JigsawVina.Presentation.Screens
{
    public class GameplayLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<PuzzlePlayingView>();
            builder.RegisterComponentInHierarchy<RewardSummaryView>();
            builder.Register<PuzzlePlayingPresenter>(Lifetime.Singleton);
            builder.Register<RewardSummaryPresenter>(Lifetime.Singleton);
            builder.RegisterEntryPoint<GameplayFlowController>();
        }
    }

    public class GameplayFlowController : IStartable
    {
        private readonly PuzzlePlayingView _puzzlePlayingView;
        private readonly RewardSummaryView _rewardSummaryView;
        private readonly PuzzlePlayingPresenter _puzzlePlayingPresenter;
        private readonly RewardSummaryPresenter _rewardSummaryPresenter;
        private readonly SceneLoader _sceneLoader;

        public GameplayFlowController(
            PuzzlePlayingView puzzlePlayingView,
            RewardSummaryView rewardSummaryView,
            PuzzlePlayingPresenter puzzlePlayingPresenter,
            RewardSummaryPresenter rewardSummaryPresenter,
            SceneLoader sceneLoader)
        {
            _puzzlePlayingView = puzzlePlayingView;
            _rewardSummaryView = rewardSummaryView;
            _puzzlePlayingPresenter = puzzlePlayingPresenter;
            _rewardSummaryPresenter = rewardSummaryPresenter;
            _sceneLoader = sceneLoader;
        }

        public void Start()
        {
            _puzzlePlayingView.SetActive(true);
            _rewardSummaryView.SetActive(false);
            _puzzlePlayingPresenter.Initialize();

            _puzzlePlayingView.OnCheatWinClicked += () =>
            {
                _puzzlePlayingView.SetActive(false);
                _rewardSummaryView.SetActive(true);
                _rewardSummaryPresenter.ProcessRewardsAndDisplay(15f);
            };

            _rewardSummaryView.OnReturnClicked += () =>
            {
                _sceneLoader.LoadSceneAsync("Home").Forget();
            };
        }
    }
}
