using System;
using System.Globalization;
using System.Linq;
using JigsawVina.Core.Data;
using UnityEngine;

namespace JigsawVina.Core.Services
{
    public class DailyRewardService : IDailyRewardService
    {
        private readonly IStaticDataService _staticDataService;
        private readonly IRewardApplier _rewardApplier;
        private readonly ILocalDateProvider _localDateProvider;

        public DailyRewardService(
            IStaticDataService staticDataService,
            IRewardApplier rewardApplier,
            ILocalDateProvider localDateProvider)
        {
            _staticDataService = staticDataService ?? throw new ArgumentNullException(nameof(staticDataService));
            _rewardApplier = rewardApplier ?? throw new ArgumentNullException(nameof(rewardApplier));
            _localDateProvider = localDateProvider ?? throw new ArgumentNullException(nameof(localDateProvider));
        }

        public bool CanClaimToday(PlayerSave save)
        {
            if (save == null) return false;

            if (string.IsNullOrEmpty(save.LastDailyRewardClaimDateString))
            {
                return true;
            }

            string todayString = _localDateProvider.GetCurrentLocalDateString();

            if (todayString == save.LastDailyRewardClaimDateString)
            {
                return false;
            }

            if (!DateTime.TryParseExact(
                save.LastDailyRewardClaimDateString, 
                "yyyy-MM-dd", 
                CultureInfo.InvariantCulture, 
                DateTimeStyles.None, 
                out DateTime lastClaimDate))
            {
                // Defensive: If parsing fails, allow claiming to avoid lockout
                return true;
            }

            if (!DateTime.TryParseExact(
                todayString, 
                "yyyy-MM-dd", 
                CultureInfo.InvariantCulture, 
                DateTimeStyles.None, 
                out DateTime todayDate))
            {
                return true;
            }

            // Guard against clock drift / going backward
            if (todayDate < lastClaimDate)
            {
                return false;
            }

            return todayDate > lastClaimDate;
        }

        public int GetNextRewardDayIndex(PlayerSave save)
        {
            if (save == null) return 1;

            int sanitizedStreak = (save.DailyRewardStreak < 0 || save.DailyRewardStreak > 7) ? 0 : save.DailyRewardStreak;

            if (string.IsNullOrEmpty(save.LastDailyRewardClaimDateString))
            {
                return 1;
            }

            string todayString = _localDateProvider.GetCurrentLocalDateString();

            if (!DateTime.TryParseExact(
                save.LastDailyRewardClaimDateString, 
                "yyyy-MM-dd", 
                CultureInfo.InvariantCulture, 
                DateTimeStyles.None, 
                out DateTime lastClaimDate))
            {
                return 1;
            }

            if (!DateTime.TryParseExact(
                todayString, 
                "yyyy-MM-dd", 
                CultureInfo.InvariantCulture, 
                DateTimeStyles.None, 
                out DateTime todayDate))
            {
                return 1;
            }

            int daysDiff = (todayDate - lastClaimDate).Days;

            if (daysDiff == 1)
            {
                return (sanitizedStreak % 7) + 1;
            }
            else if (daysDiff > 1)
            {
                return 1;
            }
            else
            {
                // Already claimed today (daysDiff == 0) or clock drift (daysDiff < 0)
                return (sanitizedStreak % 7) + 1;
            }
        }

        public ClaimDailyRewardResult ClaimDailyReward(PlayerSave save)
        {
            if (save == null)
            {
                return new ClaimDailyRewardResult { Success = false };
            }

            if (!CanClaimToday(save))
            {
                return new ClaimDailyRewardResult { Success = false };
            }

            int claimDay = GetNextRewardDayIndex(save);

            var rewards = _staticDataService.GetDailyRewards();
            var config = rewards.FirstOrDefault(r => r.DayIndex == claimDay);
            if (config.DayIndex == 0)
            {
                Debug.LogError($"[DailyRewardService] No daily reward configuration found for Day {claimDay}.");
                return new ClaimDailyRewardResult { Success = false };
            }

            var applyResult = _rewardApplier.Apply(save, config.ItemId, config.Amount, RewardApplyPolicy.WithCompensation);
            if (!applyResult.Success)
            {
                Debug.LogWarning($"[DailyRewardService] Failed to apply daily reward for Day {claimDay}.");
                return new ClaimDailyRewardResult { Success = false };
            }

            // Mutate save after successful reward application
            save.DailyRewardStreak = claimDay;
            save.LastDailyRewardClaimDateString = _localDateProvider.GetCurrentLocalDateString();

            return new ClaimDailyRewardResult
            {
                Success = true,
                DayIndex = claimDay,
                ApplyResult = applyResult
            };
        }
    }
}
