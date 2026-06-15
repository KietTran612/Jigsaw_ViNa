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
                NoOpDropRewardService.Instance)
        {
        }

        [Inject]
        public RewardSummaryPresenter(
            RewardSummaryView view,
            GameSessionService sessionService,
            ISaveDataService saveDataService,
            IStaticDataService staticDataService,
            IDropRewardService dropRewardService)
        {
            _view = view;
            _sessionService = sessionService;
            _saveDataService = saveDataService;
            _staticDataService = staticDataService;
            _dropRewardService = dropRewardService ?? NoOpDropRewardService.Instance;
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

                if (reward.ItemId == 1)
                {
                    save.Coins += reward.Amount;
                    coins += reward.Amount;
                    continue;
                }

                if (reward.ItemId == 2)
                {
                    save.Hints += reward.Amount;
                    rewardedNames.Add($"Hint x{reward.Amount}");
                    continue;
                }

                var item = _staticDataService.GetItemById(reward.ItemId);
                if (item == null)
                {
                    continue;
                }

                if (item.item_type == "key_item")
                {
                    if (!save.OwnedItemIds.Contains(item.id))
                    {
                        save.OwnedItemIds.Add(item.id);
                        rewardedNames.Add(item.display_name);
                    }
                    continue;
                }

                if (item.item_type != "consumable")
                {
                    continue;
                }

                var inventoryItem = save.Inventory
                    .FirstOrDefault(entry => entry.ItemId == item.id);
                int currentAmount = inventoryItem?.Amount ?? 0;
                int newAmount = System.Math.Min(
                    item.max_stack,
                    currentAmount + reward.Amount);
                int appliedAmount = newAmount - currentAmount;
                if (appliedAmount <= 0)
                {
                    continue;
                }

                if (inventoryItem == null)
                {
                    inventoryItem = new InventoryItem { ItemId = item.id };
                    save.Inventory.Add(inventoryItem);
                }
                inventoryItem.Amount = newAmount;
                rewardedNames.Add($"{item.display_name} x{appliedAmount}");
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
