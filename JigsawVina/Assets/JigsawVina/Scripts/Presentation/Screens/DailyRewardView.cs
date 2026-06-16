using System;
using System.Collections.Generic;
using JigsawVina.Core.Data;
using UnityEngine;
using UnityEngine.UI;

namespace JigsawVina.Presentation.Screens
{
    public class DailyRewardView : MonoBehaviour
    {
        [SerializeField] private Button _claimButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private GameObject _popupPanel;
        
        // Slot structures: serializable helper to bind UI items
        [Serializable]
        public struct RewardSlotUI
        {
            public Text dayText;
            public Image rewardImage;
            public Text amountText;
            public GameObject claimedOverlay;
            public GameObject nextClaimableHighlight;
            public GameObject lockedOverlay;
        }

        [SerializeField] private List<RewardSlotUI> _slots = new();
        [SerializeField] private Text _feedbackText;

        public event Action OnClaimRequested;
        public event Action OnCloseRequested;

        private void Awake()
        {
            if (_claimButton != null)
            {
                _claimButton.onClick.AddListener(() => OnClaimRequested?.Invoke());
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(() => OnCloseRequested?.Invoke());
            }

            // Ensure clean initial feedback
            if (_feedbackText != null)
            {
                _feedbackText.text = "";
            }
        }

        public void SetActive(bool active)
        {
            if (_popupPanel != null)
            {
                _popupPanel.SetActive(active);
            }
            else
            {
                gameObject.SetActive(active);
            }

            if (active && _feedbackText != null)
            {
                _feedbackText.text = "";
            }
        }

        public struct SlotData
        {
            public int DayIndex;
            public int Amount;
            public string AssetPath;
            public string DisplayName;
        }

        public void SetDailyRewardSlots(IReadOnlyList<SlotData> configs, int nextClaimableDay, bool canClaimToday)
        {
            if (configs == null) return;

            // Enable claim button based on whether claimant is eligible today
            if (_claimButton != null)
            {
                _claimButton.interactable = canClaimToday;
            }

            for (int i = 0; i < _slots.Count; i++)
            {
                var slot = _slots[i];
                if (i >= configs.Count)
                {
                    // Hide slot or clear if not configured
                    continue;
                }

                var config = configs[i];
                if (slot.dayText != null)
                {
                    slot.dayText.text = $"Ngày {config.DayIndex}";
                }

                var sprite = string.IsNullOrEmpty(config.AssetPath) ? null : Resources.Load<Sprite>(config.AssetPath);

                if (slot.rewardImage != null)
                {
                    slot.rewardImage.sprite = sprite;
                    slot.rewardImage.enabled = sprite != null;
                }

                if (slot.amountText != null)
                {
                    if (sprite == null && !string.IsNullOrEmpty(config.DisplayName))
                    {
                        slot.amountText.text = $"+{config.Amount} {config.DisplayName}";
                        slot.amountText.horizontalOverflow = HorizontalWrapMode.Wrap;
                        slot.amountText.verticalOverflow = VerticalWrapMode.Overflow;
                        slot.amountText.resizeTextForBestFit = true;
                        slot.amountText.resizeTextMinSize = 6;
                        slot.amountText.resizeTextMaxSize = 18;
                    }
                    else
                    {
                        slot.amountText.text = $"+{config.Amount}";
                        slot.amountText.horizontalOverflow = HorizontalWrapMode.Overflow;
                        slot.amountText.verticalOverflow = VerticalWrapMode.Overflow;
                        slot.amountText.resizeTextForBestFit = false;
                    }
                }

                // Determine slot state: Claimed, Next/Claimable, Locked
                bool isClaimed;
                bool isNext;
                bool isLocked;

                if (canClaimToday)
                {
                    isClaimed = config.DayIndex < nextClaimableDay;
                    isNext = config.DayIndex == nextClaimableDay;
                    isLocked = config.DayIndex > nextClaimableDay;
                }
                else
                {
                    isNext = false;
                    if (nextClaimableDay == 1)
                    {
                        isClaimed = true;
                        isLocked = false;
                    }
                    else
                    {
                        isClaimed = config.DayIndex < nextClaimableDay;
                        isLocked = config.DayIndex >= nextClaimableDay;
                    }
                }

                if (slot.claimedOverlay != null)
                {
                    slot.claimedOverlay.SetActive(isClaimed);
                }

                if (slot.nextClaimableHighlight != null)
                {
                    slot.nextClaimableHighlight.SetActive(isNext);
                }

                if (slot.lockedOverlay != null)
                {
                    slot.lockedOverlay.SetActive(isLocked);
                }
            }
        }

        public void ShowRewardClaimedFeedback(string itemName, int amount, bool isCompensated)
        {
            if (_feedbackText == null) return;

            if (isCompensated)
            {
                _feedbackText.text = $"Đã nhận: {amount} {itemName} (Bù đắp vật phẩm đã sở hữu)";
            }
            else
            {
                _feedbackText.text = $"Đã nhận thành công: {itemName} x{amount}!";
            }
        }
    }
}
