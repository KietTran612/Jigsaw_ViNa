using System;
using System.Collections.Generic;
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
            throw new NotImplementedException();
        }
    }
}
