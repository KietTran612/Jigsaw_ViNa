using System;
using System.Linq;
using JigsawVina.Core.Data;
using UnityEngine;

namespace JigsawVina.Core.Services
{
    public class RewardApplier : IRewardApplier
    {
        public const int DuplicateRewardCompensationCoins = 100;
        
        private readonly IStaticDataService _staticDataService;

        public RewardApplier(IStaticDataService staticDataService)
        {
            _staticDataService = staticDataService ?? throw new ArgumentNullException(nameof(staticDataService));
        }

        public RewardApplyResult Apply(PlayerSave save, int itemId, int amount, RewardApplyPolicy policy)
        {
            if (save == null)
            {
                Debug.LogWarning("[RewardApplier] PlayerSave is null.");
                return new RewardApplyResult { Success = false };
            }

            if (amount <= 0)
            {
                Debug.LogWarning($"[RewardApplier] Invalid amount: {amount}. Must be > 0.");
                return new RewardApplyResult { Success = false };
            }

            var config = _staticDataService.GetItemById(itemId);
            if (config == null)
            {
                Debug.LogWarning($"[RewardApplier] Item ID {itemId} not found in static data.");
                return new RewardApplyResult { Success = false };
            }

            if (config.status != "active")
            {
                Debug.LogWarning($"[RewardApplier] Item ID {itemId} is inactive.");
                return new RewardApplyResult { Success = false };
            }

            // Coin and Hint currencies are handled directly
            if (itemId == 1) // Coin
            {
                save.Coins += amount;
                return new RewardApplyResult
                {
                    Success = true,
                    ItemId = 1,
                    AppliedAmount = amount,
                    DisplayName = config.display_name,
                    IsCompensated = false
                };
            }

            if (itemId == 2) // Hint
            {
                save.Hints += amount;
                return new RewardApplyResult
                {
                    Success = true,
                    ItemId = 2,
                    AppliedAmount = amount,
                    DisplayName = config.display_name,
                    IsCompensated = false
                };
            }

            if (config.item_type == "key_item")
            {
                save.OwnedItemIds ??= new System.Collections.Generic.List<int>();
                if (save.OwnedItemIds.Contains(itemId))
                {
                    if (policy == RewardApplyPolicy.WithCompensation)
                    {
                        return GrantCompensation(save);
                    }
                    else
                    {
                        return new RewardApplyResult
                        {
                            Success = false,
                            ItemId = itemId,
                            AppliedAmount = 0,
                            DisplayName = config.display_name,
                            IsCompensated = false
                        };
                    }
                }

                save.OwnedItemIds.Add(itemId);
                return new RewardApplyResult
                {
                    Success = true,
                    ItemId = itemId,
                    AppliedAmount = 1,
                    DisplayName = config.display_name,
                    IsCompensated = false
                };
            }

            if (config.item_type == "consumable")
            {
                save.Inventory ??= new System.Collections.Generic.List<InventoryItem>();
                var existing = save.Inventory.FirstOrDefault(i => i.ItemId == itemId);
                int maxStack = config.max_stack > 0 ? config.max_stack : 99;

                if (existing != null)
                {
                    int room = maxStack - existing.Amount;
                    if (room <= 0)
                    {
                        if (policy == RewardApplyPolicy.WithCompensation)
                        {
                            return GrantCompensation(save);
                        }
                        else
                        {
                            return new RewardApplyResult
                            {
                                Success = false,
                                ItemId = itemId,
                                AppliedAmount = 0,
                                DisplayName = config.display_name,
                                IsCompensated = false
                            };
                        }
                    }

                    int added = Mathf.Min(amount, room);
                    existing.Amount += added;
                    return new RewardApplyResult
                    {
                        Success = true,
                        ItemId = itemId,
                        AppliedAmount = added,
                        DisplayName = config.display_name,
                        IsCompensated = false
                    };
                }
                else
                {
                    int added = Mathf.Min(amount, maxStack);
                    save.Inventory.Add(new InventoryItem { ItemId = itemId, Amount = added });
                    return new RewardApplyResult
                    {
                        Success = true,
                        ItemId = itemId,
                        AppliedAmount = added,
                        DisplayName = config.display_name,
                        IsCompensated = false
                    };
                }
            }

            Debug.LogWarning($"[RewardApplier] Unsupported item type: '{config.item_type}' for item ID {itemId}.");
            return new RewardApplyResult { Success = false };
        }

        private RewardApplyResult GrantCompensation(PlayerSave save)
        {
            var coinConfig = _staticDataService.GetItemById(1);
            if (coinConfig == null || coinConfig.status != "active")
            {
                Debug.LogWarning("[RewardApplier] Compensation failed. Coin item (ID 1) config is missing or inactive.");
                return new RewardApplyResult { Success = false };
            }
            save.Coins += DuplicateRewardCompensationCoins;
            return new RewardApplyResult
            {
                Success = true,
                ItemId = 1,
                AppliedAmount = DuplicateRewardCompensationCoins,
                DisplayName = coinConfig.display_name,
                IsCompensated = true
            };
        }
    }
}
