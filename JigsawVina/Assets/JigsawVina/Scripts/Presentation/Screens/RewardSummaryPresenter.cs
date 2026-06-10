using JigsawVina.Core.Data;
using JigsawVina.Core.Services;

namespace JigsawVina.Presentation.Screens
{
    public class RewardSummaryPresenter
    {
        private readonly RewardSummaryView _view;
        private readonly GameSessionService _sessionService;
        private readonly ISaveDataService _saveDataService;

        public RewardSummaryPresenter(
            RewardSummaryView view,
            GameSessionService sessionService,
            ISaveDataService saveDataService)
        {
            _view = view;
            _sessionService = sessionService;
            _saveDataService = saveDataService;
        }

        public void ProcessRewardsAndDisplay(float elapsedTimeSeconds)
        {
            int stars = _sessionService.SelectedDifficultyId switch
            {
                0 => 1,
                1 => 2,
                2 => 3,
                _ => 1
            };
            int coins = stars * 10;

            _sessionService.LastStarCount = stars;
            _sessionService.LastElapsedTimeSeconds = elapsedTimeSeconds;

            var save = _saveDataService.Load();
            save.Coins += coins;

            var existing = save.CompletedPuzzles.Find(p =>
                p.PictureId == _sessionService.SelectedPictureId &&
                p.DifficultyId == _sessionService.SelectedDifficultyId);

            if (existing == null)
            {
                save.CompletedPuzzles.Add(new CompletedPuzzleData
                {
                    PictureId = _sessionService.SelectedPictureId,
                    DifficultyId = _sessionService.SelectedDifficultyId,
                    BestTimeSeconds = elapsedTimeSeconds,
                    BestStar = stars
                });
            }
            else
            {
                if (elapsedTimeSeconds < existing.BestTimeSeconds || existing.BestTimeSeconds <= 0f)
                {
                    existing.BestTimeSeconds = elapsedTimeSeconds;
                }

                if (stars > existing.BestStar)
                {
                    existing.BestStar = stars;
                }
            }

            _saveDataService.Save(save);
            _view?.DisplayReward(stars, coins);
        }
    }
}
