using System;
using JigsawVina.Core.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JigsawVina.Presentation.Screens
{
    public class PictureSelectCard : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _thumbnailImage;
        [SerializeField] private TMP_Text _displayNameText;
        [SerializeField] private GameObject _lockOverlay;
        [SerializeField] private TMP_Text _missingItemsHintText;
        [SerializeField] private Button _unlockButton;

        private Action<int> _onSelected;
        private Action<int> _onUnlockRequested;
        private int _pictureId;

        private void Awake()
        {
            if (_button != null)
            {
                _button.onClick.AddListener(() => _onSelected?.Invoke(_pictureId));
            }

            if (_unlockButton != null)
            {
                _unlockButton.onClick.AddListener(() => _onUnlockRequested?.Invoke(_pictureId));
            }
        }

        public void Bind(
            PictureCardPresentationModel model,
            Action<int> onSelected,
            Action<int> onUnlockRequested)
        {
            var config = model.Config;
            _pictureId = config.Id;
            _onSelected = onSelected;
            _onUnlockRequested = onUnlockRequested;

            bool isLocked = model.State == PictureCardState.Locked ||
                            model.State == PictureCardState.ReadyToUnlock;
            bool canUnlock = model.State == PictureCardState.ReadyToUnlock;

            if (_button != null)
            {
                _button.interactable = !isLocked;
            }

            if (_lockOverlay != null)
            {
                _lockOverlay.SetActive(isLocked);
            }

            if (_unlockButton != null)
            {
                _unlockButton.gameObject.SetActive(canUnlock);
                _unlockButton.interactable = canUnlock;
            }

            if (_missingItemsHintText != null)
            {
                _missingItemsHintText.text = model.MissingItemsHint ?? string.Empty;
                _missingItemsHintText.gameObject.SetActive(
                    isLocked && !string.IsNullOrEmpty(model.MissingItemsHint));
            }

            if (_displayNameText != null)
            {
                _displayNameText.text = config.DisplayName;
            }

            if (_thumbnailImage != null)
            {
                if (string.IsNullOrEmpty(config.AssetPath))
                {
                    Debug.LogError($"[JigsawVina] Data error: Picture ID {config.Id} ({config.DisplayName}) has a null or empty AssetPath.");
                    _thumbnailImage.sprite = null;
                    _thumbnailImage.color = Color.gray;
                    return;
                }

                // Load ảnh chính làm thumbnail (Chấp nhận chi phí RAM ở MVP)
                var sprite = Resources.Load<Sprite>(config.AssetPath);
                if (sprite != null)
                {
                    _thumbnailImage.sprite = sprite;
                    _thumbnailImage.color = Color.white;
                }
                else
                {
                    Debug.LogError($"[JigsawVina] Resources error: Failed to load Sprite for Picture ID: {config.Id} ({config.DisplayName}) at path: '{config.AssetPath}'");
                    _thumbnailImage.sprite = null;
                    _thumbnailImage.color = Color.gray; // Placeholder
                }
            }
        }

        public void Unbind()
        {
            _onSelected = null;
            _onUnlockRequested = null;
            _pictureId = 0;

            if (_button != null)
            {
                _button.interactable = false;
            }

            if (_thumbnailImage != null)
            {
                _thumbnailImage.sprite = null;
            }

            if (_displayNameText != null)
            {
                _displayNameText.text = string.Empty;
            }

            if (_missingItemsHintText != null)
            {
                _missingItemsHintText.text = string.Empty;
                _missingItemsHintText.gameObject.SetActive(false);
            }

            if (_unlockButton != null)
            {
                _unlockButton.interactable = false;
                _unlockButton.gameObject.SetActive(false);
            }

            if (_lockOverlay != null)
            {
                _lockOverlay.SetActive(false);
            }
        }
    }
}
