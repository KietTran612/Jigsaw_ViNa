using JigsawVina.Core.Data;
using JigsawVina.Core.Services;

namespace JigsawVina.Presentation.Screens
{
    public class RewardSummaryPresenter
    {
        private readonly RewardSummaryView _view;
        private readonly GameSessionService _sessionService;
        private readonly ISaveDataService _saveDataService;
        private readonly IStaticDataService _staticDataService;

        public RewardSummaryPresenter(
            RewardSummaryView view,
            GameSessionService sessionService,
            ISaveDataService saveDataService,
            IStaticDataService staticDataService)
        {
            _view = view;
            _sessionService = sessionService;
            _saveDataService = saveDataService;
            _staticDataService = staticDataService;
        }

        public void ProcessRewardsAndDisplay(float elapsedTimeSeconds)
        {
            ProcessRewards(elapsedTimeSeconds);
            DisplayProcessedReward();
        }

        public void ProcessRewards(float elapsedTimeSeconds)
        {
            var config = _staticDataService.GetPictureDifficulty(_sessionService.SelectedPictureId, _sessionService.SelectedDifficultyId);
            int stars = config.StarReward;
            int coins = stars * 10;

            _sessionService.LastStarCount = stars;
            _sessionService.LastElapsedTimeSeconds = elapsedTimeSeconds;

            if (!_sessionService.IsRewardProcessed)
            {
                var save = _saveDataService.Load();
                save.Coins += coins;

                var existing = save.CompletedPuzzles.Find(p =>
                    p.PictureId == _sessionService.SelectedPictureId &&
                    p.DifficultyId == _sessionService.SelectedDifficultyId);

                if (existing != null)
                {
                    // Update with best records
                    if (_sessionService.LastElapsedTimeSeconds < existing.BestTimeSeconds || existing.BestTimeSeconds <= 0)
                    {
                        existing.BestTimeSeconds = _sessionService.LastElapsedTimeSeconds;
                    }
                    if (stars > existing.BestStar)
                    {
                        existing.BestStar = stars;
                    }
                }
                else
                {
                    save.CompletedPuzzles.Add(new CompletedPuzzleData
                    {
                        PictureId = _sessionService.SelectedPictureId,
                        DifficultyId = _sessionService.SelectedDifficultyId,
                        BestTimeSeconds = _sessionService.LastElapsedTimeSeconds,
                        BestStar = stars
                    });
                }

                _saveDataService.Save(save);
                _sessionService.IsRewardProcessed = true;
            }
        }

        public void DisplayProcessedReward()
        {
            if (_view != null)
            {
                int coins = _sessionService.LastStarCount * 10;
                _view.DisplayReward(_sessionService.LastStarCount, coins);
            }
        }
    }
}
