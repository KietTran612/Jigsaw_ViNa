using System.Collections.Generic;
using JigsawVina.Core.Data;

namespace JigsawVina.Core.Services
{
    public struct DropRewardResult
    {
        public int ItemId;
        public int Amount;
    }

    public interface IDropRewardService
    {
        List<DropRewardResult> RollDropRewards(int dropTableId, PlayerSave save);
    }
}
