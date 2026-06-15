using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JigsawVina.Presentation.Screens
{
    public sealed class CollectionSourcePresentationModel
    {
        public int PictureId { get; set; }
        public int DifficultyId { get; set; }
        public string Label { get; set; }
    }

    public sealed class CollectionItemPresentationModel
    {
        public int ItemId { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string AssetPath { get; set; }
        public IReadOnlyList<CollectionSourcePresentationModel> Sources { get; set; }
    }

    public class CollectionView : MonoBehaviour
    {
        public event Action OnCloseRequested;
        public event Action<int> OnItemSelected;
        public event Action<int> OnNavigateToPictureRequested;

        [SerializeField] private RectTransform _itemContent;
        [SerializeField] private Button _itemButtonTemplate;
        [SerializeField] private TMP_Text _itemNameText;
        [SerializeField] private TMP_Text _itemDescriptionText;
        [SerializeField] private Image _itemThumbnail;
        [SerializeField] private RectTransform _sourceContent;
        [SerializeField] private Button _sourceButtonTemplate;
        [SerializeField] private Button _closeButton;

        private readonly List<GameObject> _generatedItems = new();
        private readonly List<GameObject> _generatedSources = new();
        private IReadOnlyList<CollectionItemPresentationModel> _models =
            Array.Empty<CollectionItemPresentationModel>();

        public IReadOnlyList<CollectionItemPresentationModel> Models => _models;

        private void Awake()
        {
            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(RequestClose);
            }
        }

        private void OnDestroy()
        {
            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveListener(RequestClose);
            }
            ClearGenerated(_generatedItems);
            ClearGenerated(_generatedSources);
        }

        public void Setup(IReadOnlyList<CollectionItemPresentationModel> models)
        {
            _models = models ?? Array.Empty<CollectionItemPresentationModel>();
            ClearGenerated(_generatedItems);

            if (_itemButtonTemplate != null && _itemContent != null)
            {
                foreach (var model in _models)
                {
                    var button = Instantiate(_itemButtonTemplate, _itemContent, false);
                    button.gameObject.SetActive(true);
                    button.gameObject.name = $"CollectionItem_{model.ItemId}";
                    SetButtonLabel(button, model.DisplayName);
                    int itemId = model.ItemId;
                    button.onClick.AddListener(() => OnItemSelected?.Invoke(itemId));
                    _generatedItems.Add(button.gameObject);
                }
            }

            ShowItem(_models.Count > 0 ? _models[0] : null);
        }

        public void ShowItem(CollectionItemPresentationModel model)
        {
            if (_itemNameText != null)
            {
                _itemNameText.text = model?.DisplayName ?? string.Empty;
            }
            if (_itemDescriptionText != null)
            {
                _itemDescriptionText.text = model?.Description ?? string.Empty;
            }
            if (_itemThumbnail != null)
            {
                _itemThumbnail.sprite = string.IsNullOrEmpty(model?.AssetPath)
                    ? null
                    : Resources.Load<Sprite>(model.AssetPath);
            }

            ClearGenerated(_generatedSources);
            if (model == null || _sourceButtonTemplate == null || _sourceContent == null)
            {
                return;
            }

            foreach (var source in model.Sources)
            {
                var button = Instantiate(_sourceButtonTemplate, _sourceContent, false);
                button.gameObject.SetActive(true);
                button.gameObject.name =
                    $"CollectionSource_{source.PictureId}_{source.DifficultyId}";
                SetButtonLabel(button, source.Label);
                int pictureId = source.PictureId;
                button.onClick.AddListener(
                    () => OnNavigateToPictureRequested?.Invoke(pictureId));
                _generatedSources.Add(button.gameObject);
            }
        }

        public void RequestClose()
        {
            OnCloseRequested?.Invoke();
        }

        public void RequestNavigation(int pictureId)
        {
            OnNavigateToPictureRequested?.Invoke(pictureId);
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        private static void SetButtonLabel(Button button, string label)
        {
            var text = button.GetComponentInChildren<TMP_Text>(true);
            if (text != null)
            {
                text.text = label ?? string.Empty;
            }
        }

        private static void ClearGenerated(List<GameObject> objects)
        {
            foreach (var generatedObject in objects)
            {
                if (generatedObject == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Destroy(generatedObject);
                }
                else
                {
                    DestroyImmediate(generatedObject);
                }
            }
            objects.Clear();
        }
    }
}
