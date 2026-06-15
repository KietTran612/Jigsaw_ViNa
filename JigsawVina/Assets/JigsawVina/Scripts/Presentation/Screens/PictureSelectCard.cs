using System;
using JigsawVina.Core.Data;
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

        private Action<int> _onClicked;
        private int _pictureId;

        private void Awake()
        {
            if (_button != null)
            {
                _button.onClick.AddListener(() => _onClicked?.Invoke(_pictureId));
            }
        }

        public void Bind(PictureConfig config, Action<int> onClicked)
        {
            _pictureId = config.Id;
            _onClicked = onClicked;

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
            _onClicked = null;
            if (_thumbnailImage != null)
            {
                _thumbnailImage.sprite = null;
            }
        }
    }
}
