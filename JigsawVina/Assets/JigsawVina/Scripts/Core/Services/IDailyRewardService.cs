using JigsawVina.Core.Data;

namespace JigsawVina.Core.Services
{
    public struct ClaimDailyRewardResult
    {
        public int DayIndex;
        public RewardApplyResult ApplyResult;
        public bool Success;
    }

    public interface IDailyRewardService
    {
        bool CanClaimToday(PlayerSave save);
        int GetNextRewardDayIndex(PlayerSave save);
        ClaimDailyRewardResult ClaimDailyReward(PlayerSave save);
    }
}
