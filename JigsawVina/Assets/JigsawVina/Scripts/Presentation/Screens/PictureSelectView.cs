using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

[assembly: InternalsVisibleTo("JigsawVina.Tests")]

namespace JigsawVina.Presentation.Screens
{
    public class PictureSelectView : MonoBehaviour
    {
        public event Action<int> OnPictureSelected;
        public event Action<int> OnPictureUnlockRequested;
        public event Action OnCollectionRequested;

        [SerializeField] private PictureSelectCard _cardPrefab;
        [SerializeField] private RectTransform _contentContainer;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private Button _collectionButton;

        private readonly List<PictureSelectCard> _instantiatedCards = new();

        internal IReadOnlyList<PictureSelectCard> InstantiatedCards => _instantiatedCards;

        private void Awake()
        {
            if (_collectionButton != null)
            {
                _collectionButton.onClick.AddListener(RequestCollection);
            }
        }

        private void OnDestroy()
        {
            if (_collectionButton != null)
            {
                _collectionButton.onClick.RemoveListener(RequestCollection);
            }
            ClearExistingCards();
        }

        public void Setup(IReadOnlyList<PictureCardPresentationModel> models)
        {
            ClearExistingCards();

            if (models == null || models.Count == 0)
            {
                Debug.LogError("[JigsawVina] PictureSelectView: Models list is null or empty.");
                return;
            }

            if (_cardPrefab == null)
            {
                Debug.LogError("[JigsawVina] UI error: Card Prefab is not assigned on PictureSelectView.");
                return;
            }

            if (_contentContainer == null)
            {
                Debug.LogError("[JigsawVina] UI error: Content Container is not assigned on PictureSelectView.");
                return;
            }

            foreach (var model in models)
            {
                if (model == null || model.Config.Id <= 0)
                {
                    Debug.LogError("[JigsawVina] Data error: Picture card model is null or has an invalid ID.");
                    continue;
                }

                var picture = model.Config;
                var cardInstance = Instantiate(_cardPrefab, _contentContainer, false);
                cardInstance.gameObject.name = $"PictureCard_{picture.Id}_{picture.IdString}";
                cardInstance.gameObject.SetActive(true);

                cardInstance.Bind(
                    model,
                    id => OnPictureSelected?.Invoke(id),
                    id => OnPictureUnlockRequested?.Invoke(id));
                _instantiatedCards.Add(cardInstance);
            }
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        public void RequestPictureSelection(int pictureId)
        {
            OnPictureSelected?.Invoke(pictureId);
        }

        public void FocusCard(int pictureId)
        {
            int index = _instantiatedCards.FindIndex(card =>
                card != null && card.PictureId == pictureId);
            if (index < 0)
            {
                return;
            }

            if (_scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                LayoutRebuilder.ForceRebuildLayoutImmediate(_contentContainer);
                _scrollRect.verticalNormalizedPosition = _instantiatedCards.Count <= 1
                    ? 1f
                    : 1f - (float)index / (_instantiatedCards.Count - 1);
            }
            _instantiatedCards[index].Highlight();
        }

        private void RequestCollection()
        {
            OnCollectionRequested?.Invoke();
        }

        private void ClearExistingCards()
        {
            foreach (var card in _instantiatedCards)
            {
                if (card != null)
                {
                    card.Unbind();
                    if (Application.isPlaying)
                    {
                        Destroy(card.gameObject);
                    }
                    else
                    {
                        DestroyImmediate(card.gameObject);
                    }
                }
            }
            _instantiatedCards.Clear();
        }
    }
}
