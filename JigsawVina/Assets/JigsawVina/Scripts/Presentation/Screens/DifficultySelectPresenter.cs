using System;
using Cysharp.Threading.Tasks;
using JigsawVina.Core.Data;
using JigsawVina.Core.Services;

namespace JigsawVina.Presentation.Screens
{
    public class DifficultySelectPresenter : IDisposable
    {
        private readonly DifficultySelectView _view;
        private readonly GameSessionService _sessionService;
        private readonly SceneLoader _sceneLoader;
        private readonly ProgressionService _progressionService;
        private readonly ISaveDataService _saveDataService;
        private readonly IStaticDataService _staticDataService;
        private int _selectedPictureId;

        public DifficultySelectPresenter(
            DifficultySelectView view,
            GameSessionService sessionService,
            SceneLoader sceneLoader,
            ProgressionService progressionService,
            ISaveDataService saveDataService,
            IStaticDataService staticDataService)
        {
            _view = view;
            _sessionService = sessionService;
            _sceneLoader = sceneLoader;
            _progressionService = progressionService;
            _saveDataService = saveDataService;
            _staticDataService = staticDataService;
            _view.OnDifficultySelected += HandleDifficultySelected;
        }

        public void Refresh(int pictureId)
        {
            _selectedPictureId = pictureId;
            var save = _saveDataService.Load() ?? new PlayerSave();
            save.Normalize();

            for (int difficultyId = 0; difficultyId < 3; difficultyId++)
            {
                bool isUnlocked = _progressionService.IsDifficultyUnlocked(pictureId, difficultyId);
                
                var difficultyConfig = _staticDataService.GetPictureDifficulty(pictureId, difficultyId);
                int maxStars = difficultyConfig.PictureId != 0 ? difficultyConfig.StarReward : (difficultyId + 1);

                var completion = save.CompletedPuzzles.Find(c => c.PictureId == pictureId && c.DifficultyId == difficultyId);
                int bestStar = completion != null ? completion.BestStar : 0;
                string bestTimeText = completion != null ? completion.BestTimeSeconds.ToString("F1") + "s" : "--";
                
                string achievementText = $"Best Star: {bestStar}/{maxStars}\nBest Time: {bestTimeText}";
                
                _view.SetDifficultyState(difficultyId, isUnlocked, achievementText);
            }
        }

        private void HandleDifficultySelected(int difficultyId)
        {
            if (!_progressionService.IsDifficultyUnlocked(_selectedPictureId, difficultyId))
            {
                return;
            }

            _sessionService.SetSelectedDifficulty(difficultyId);
            _sceneLoader.LoadSceneAsync("Gameplay").Forget();
        }

        public void Dispose()
        {
            if (_view != null)
            {
                _view.OnDifficultySelected -= HandleDifficultySelected;
            }
        }
    }
}
