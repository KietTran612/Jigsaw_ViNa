using System.Collections.Generic;
using System.Linq;
using JigsawVina.Core.Data;
using JigsawVina.Core.Services;
using VContainer;

namespace JigsawVina.Presentation.Screens
{
    public class RewardSummaryPresenter
    {
        private readonly RewardSummaryView _view;
        private readonly GameSessionService _sessionService;
        private readonly ISaveDataService _saveDataService;
        private readonly IStaticDataService _staticDataService;
        private readonly IDropRewardService _dropRewardService;
        private readonly IRewardApplier _rewardApplier;

        private string _lastRewardedItemsLabel = "";

        public RewardSummaryPresenter(
            RewardSummaryView view,
            GameSessionService sessionService,
            ISaveDataService saveDataService,
            IStaticDataService staticDataService) : this(
                view,
                sessionService,
                saveDataService,
                staticDataService,
                NoOpDropRewardService.Instance,
                new RewardApplier(staticDataService))
        {
        }

        public RewardSummaryPresenter(
            RewardSummaryView view,
            GameSessionService sessionService,
            ISaveDataService saveDataService,
            IStaticDataService staticDataService,
            IDropRewardService dropRewardService) : this(
                view,
                sessionService,
                saveDataService,
                staticDataService,
                dropRewardService,
                new RewardApplier(staticDataService))
        {
        }

        [Inject]
        public RewardSummaryPresenter(
            RewardSummaryView view,
            GameSessionService sessionService,
            ISaveDataService saveDataService,
            IStaticDataService staticDataService,
            IDropRewardService dropRewardService,
            IRewardApplier rewardApplier)
        {
            _view = view;
            _sessionService = sessionService;
            _saveDataService = saveDataService;
            _staticDataService = staticDataService;
            _dropRewardService = dropRewardService ?? NoOpDropRewardService.Instance;
            _rewardApplier = rewardApplier ?? new RewardApplier(staticDataService);
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
                save.Normalize();

                var existing = save.CompletedPuzzles.Find(p =>
                    p.PictureId == _sessionService.SelectedPictureId &&
                    p.DifficultyId == _sessionService.SelectedDifficultyId);

                int coins = 0;
                if (existing != null)
                {
                    // Replay reward
                    coins = config.ReplayCoin > 0 ? config.ReplayCoin : (stars * 10);
                    save.Coins += coins;
                    var rewardedNames = new List<string>();
                    if (config.DropTableId > 0)
                    {
                        ApplyDropRewards(
                            save,
                            _dropRewardService.RollDropRewards(config.DropTableId, save),
                            rewardedNames,
                            ref coins);
                    }
                    _lastRewardedItemsLabel = rewardedNames.Count > 0
                        ? string.Join(", ", rewardedNames)
                        : "";

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
                        foreach (var itemId in config.FirstClearRewardItemIds)
                        {
                            var res = _rewardApplier.Apply(save, itemId, 1, RewardApplyPolicy.WithCompensation);
                            if (res.Success)
                            {
                                if (res.IsCompensated)
                                {
                                    coins += res.AppliedAmount; // Add 100 coins compensation to coins earned
                                    rewardedNames.Add($"Duplicate Compensation (+{res.AppliedAmount} {res.DisplayName})");
                                }
                                else
                                {
                                    rewardedNames.Add(res.DisplayName);
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

        private void ApplyDropRewards(
            PlayerSave save,
            IReadOnlyList<DropRewardResult> rewards,
            List<string> rewardedNames,
            ref int coins)
        {
            if (rewards == null)
            {
                return;
            }

            foreach (var reward in rewards)
            {
                if (reward.Amount <= 0)
                {
                    continue;
                }

                var res = _rewardApplier.Apply(save, reward.ItemId, reward.Amount, RewardApplyPolicy.Standard);
                if (res.Success)
                {
                    if (res.ItemId == 1) // Coin
                    {
                        coins += res.AppliedAmount;
                    }
                    else if (res.ItemId == 2) // Hint
                    {
                        rewardedNames.Add($"{res.DisplayName} x{res.AppliedAmount}");
                    }
                    else
                    {
                        var config = _staticDataService.GetItemById(res.ItemId);
                        if (config != null && config.item_type == "consumable")
                        {
                            rewardedNames.Add($"{res.DisplayName} x{res.AppliedAmount}");
                        }
                        else
                        {
                            rewardedNames.Add(res.DisplayName);
                        }
                    }
                }
            }
        }

        public void DisplayProcessedReward()
        {
            if (_view != null)
            {
                _view.DisplayReward(_sessionService.LastStarCount, _sessionService.LastCoinEarned, _lastRewardedItemsLabel);
            }
        }

        private sealed class NoOpDropRewardService : IDropRewardService
        {
            public static readonly NoOpDropRewardService Instance = new();

            public List<DropRewardResult> RollDropRewards(
                int dropTableId,
                PlayerSave save)
            {
                return new List<DropRewardResult>();
            }
        }
    }
}
