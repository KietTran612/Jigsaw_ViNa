using System;
using System.Collections.Generic;
using JigsawVina.Core.Data;
using JigsawVina.Core.Services;

namespace JigsawVina.Presentation.Screens
{
    public class DailyRewardPresenter : IDisposable
    {
        private readonly DailyRewardView _view;
        private readonly IDailyRewardService _dailyRewardService;
        private readonly ISaveDataService _saveDataService;
        private readonly IStaticDataService _staticDataService;

        public event Action OnRewardClaimed;

        public DailyRewardPresenter(
            DailyRewardView view,
            IDailyRewardService dailyRewardService,
            ISaveDataService saveDataService,
            IStaticDataService staticDataService)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _dailyRewardService = dailyRewardService ?? throw new ArgumentNullException(nameof(dailyRewardService));
            _saveDataService = saveDataService ?? throw new ArgumentNullException(nameof(saveDataService));
            _staticDataService = staticDataService ?? throw new ArgumentNullException(nameof(staticDataService));

            _view.OnClaimRequested += Claim;
            _view.OnCloseRequested += ClosePopup;
        }

        public void OpenPopup()
        {
            Refresh();
            _view.SetActive(true);
        }

        public void ClosePopup()
        {
            _view.SetActive(false);
        }

        private void Refresh()
        {
            var save = _saveDataService.Load() ?? new PlayerSave();
            save.Normalize();

            bool canClaim = _dailyRewardService.CanClaimToday(save);
            int nextDay = _dailyRewardService.GetNextRewardDayIndex(save);
            var configs = _staticDataService.GetDailyRewards();

            var slotDatas = new List<DailyRewardView.SlotData>();
            foreach (var config in configs)
            {
                var item = _staticDataService.GetItemById(config.ItemId);
                slotDatas.Add(new DailyRewardView.SlotData
                {
                    DayIndex = config.DayIndex,
                    Amount = config.Amount,
                    AssetPath = item != null ? item.asset_path : "",
                    DisplayName = item != null ? item.display_name : ""
                });
            }

            _view.SetDailyRewardSlots(slotDatas, nextDay, canClaim);
        }

        private void Claim()
        {
            var save = _saveDataService.Load() ?? new PlayerSave();
            save.Normalize();

            var result = _dailyRewardService.ClaimDailyReward(save);
            if (result.Success)
            {
                _saveDataService.Save(save);
                _view.ShowRewardClaimedFeedback(result.ApplyResult.DisplayName, result.ApplyResult.AppliedAmount, result.ApplyResult.IsCompensated);
                Refresh();
                OnRewardClaimed?.Invoke();
            }
        }

        public void Dispose()
        {
            if (_view != null)
            {
                _view.OnClaimRequested -= Claim;
                _view.OnCloseRequested -= ClosePopup;
            }
        }
    }
}
