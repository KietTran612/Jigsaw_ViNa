using System.Collections.Generic;
using System.Linq;
using JigsawVina.Core.Data;

namespace JigsawVina.Core.Services
{
    public class DropRewardService : IDropRewardService
    {
        private readonly IStaticDataService _staticDataService;
        private readonly IRandomSource _randomSource;

        public DropRewardService(IStaticDataService staticDataService, IRandomSource randomSource)
        {
            _staticDataService = staticDataService;
            _randomSource = randomSource;
        }

        public List<DropRewardResult> RollDropRewards(int dropTableId, PlayerSave save)
        {
            var rewards = new List<DropRewardResult>();
            if (save == null || dropTableId <= 0)
            {
                return rewards;
            }

            save.Normalize();
            foreach (var dropItem in _staticDataService.GetDropTableItems(dropTableId))
            {
                if (dropItem.Status != "active")
                {
                    continue;
                }

                var item = _staticDataService.GetItemById(dropItem.ItemId);
                if (item == null || item.status != "active")
                {
                    continue;
                }

                if (item.item_type == "key_item" &&
                    save.OwnedItemIds.Contains(item.id))
                {
                    continue;
                }

                int availableCapacity = int.MaxValue;
                if (item.item_type == "consumable")
                {
                    int currentAmount = save.Inventory
                        .FirstOrDefault(entry => entry.ItemId == item.id)?.Amount ?? 0;
                    availableCapacity = item.max_stack - currentAmount;
                    if (availableCapacity <= 0)
                    {
                        continue;
                    }
                }

                var dailyCount = save.DailyDropCounts
                    .FirstOrDefault(entry => entry.ItemId == item.id);
                int successCount = dailyCount?.Count ?? 0;
                float currentRate = System.Math.Max(
                    dropItem.MinRate,
                    dropItem.BaseRate - successCount * dropItem.DecayPerSuccess);
                if (_randomSource.NextFloat() >= currentRate)
                {
                    continue;
                }

                int amount = _randomSource.NextRange(
                    dropItem.AmountMin,
                    dropItem.AmountMax + 1);
                amount = System.Math.Min(amount, availableCapacity);
                if (amount <= 0)
                {
                    continue;
                }

                if (dailyCount == null)
                {
                    dailyCount = new DailyDropCount { ItemId = item.id };
                    save.DailyDropCounts.Add(dailyCount);
                }
                dailyCount.Count++;

                rewards.Add(new DropRewardResult
                {
                    ItemId = item.id,
                    Amount = amount
                });
            }

            return rewards;
        }
    }
}
