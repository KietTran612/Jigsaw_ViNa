using JigsawVina.Core.Data;

namespace JigsawVina.Core.Services
{
    public enum RewardApplyPolicy
    {
        Standard,         // Used for Drop Table replay rewards (no duplicate/full compensation)
        WithCompensation  // Used for Daily Rewards and first clear rewards (grants coins on duplicate/full)
    }

    public struct RewardApplyResult
    {
        public bool Success;
        public int ItemId;
        public int AppliedAmount;
        public string DisplayName;
        public bool IsCompensated;
    }

    public interface IRewardApplier
    {
        RewardApplyResult Apply(PlayerSave save, int itemId, int amount, RewardApplyPolicy policy);
    }
}
