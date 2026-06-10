using Cysharp.Threading.Tasks;
using JigsawVina.Core.Services;

namespace JigsawVina.Presentation.Screens
{
    public class DifficultySelectPresenter
    {
        private readonly GameSessionService _sessionService;
        private readonly SceneLoader _sceneLoader;

        public DifficultySelectPresenter(
            DifficultySelectView view,
            GameSessionService sessionService,
            SceneLoader sceneLoader)
        {
            _sessionService = sessionService;
            _sceneLoader = sceneLoader;
            view.OnDifficultySelected += HandleDifficultySelected;
        }

        private void HandleDifficultySelected(int difficultyId)
        {
            _sessionService.SetSelectedDifficulty(difficultyId);
            _sceneLoader.LoadSceneAsync("Gameplay").Forget();
        }
    }
}
