using System.Collections.Generic;
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

        private string _lastRewardedItemsLabel = "";

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
            _lastRewardedItemsLabel = "";
            ProcessRewards(elapsedTimeSeconds);
            DisplayProcessedReward();
        }

        public void ProcessRewards(float elapsedTimeSeconds)
        {
            var config = _staticDataService.GetPictureDifficulty(_sessionService.SelectedPictureId, _sessionService.SelectedDifficultyId);
            int stars = config.StarReward;

            _sessionService.LastStarCount = stars;
            _sessionService.LastElapsedTimeSeconds = elapsedTimeSeconds;

            if (!_sessionService.IsRewardProcessed)
            {
                var save = _saveDataService.Load();

                var existing = save.CompletedPuzzles.Find(p =>
                    p.PictureId == _sessionService.SelectedPictureId &&
                    p.DifficultyId == _sessionService.SelectedDifficultyId);

                int coins = 0;
                if (existing != null)
                {
                    // Replay reward
                    coins = config.ReplayCoin > 0 ? config.ReplayCoin : (stars * 10);
                    save.Coins += coins;

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
                    // First Clear reward
                    coins = config.FirstClearCoin > 0 ? config.FirstClearCoin : (stars * 10);
                    save.Coins += coins;
                    save.Hints += config.FirstClearHint;

                    var rewardedNames = new List<string>();
                    if (config.FirstClearRewardItemIds != null)
                    {
                        if (save.OwnedItemIds == null)
                        {
                            save.OwnedItemIds = new List<int>();
                        }
                        foreach (var itemId in config.FirstClearRewardItemIds)
                        {
                            if (!save.OwnedItemIds.Contains(itemId))
                            {
                                save.OwnedItemIds.Add(itemId);

                                var itemDto = _staticDataService.GetItemById(itemId);
                                if (itemDto != null)
                                {
                                    rewardedNames.Add(itemDto.display_name);
                                }
                                else
                                {
                                    rewardedNames.Add($"Mục #{itemId}");
                                }
                            }
                        }
                    }

                    _lastRewardedItemsLabel = rewardedNames.Count > 0 ? string.Join(", ", rewardedNames) : "";

                    save.CompletedPuzzles.Add(new CompletedPuzzleData
                    {
                        PictureId = _sessionService.SelectedPictureId,
                        DifficultyId = _sessionService.SelectedDifficultyId,
                        BestTimeSeconds = _sessionService.LastElapsedTimeSeconds,
                        BestStar = stars
                    });
                }

                _sessionService.LastCoinEarned = coins;
                _saveDataService.Save(save);
                _sessionService.IsRewardProcessed = true;
            }
        }

        public void DisplayProcessedReward()
        {
            if (_view != null)
            {
                _view.DisplayReward(_sessionService.LastStarCount, _sessionService.LastCoinEarned, _lastRewardedItemsLabel);
            }
        }
    }
}
