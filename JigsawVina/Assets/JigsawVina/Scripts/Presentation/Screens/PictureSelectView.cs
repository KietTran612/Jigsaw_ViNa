using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JigsawVina.Core.Data;
using UnityEngine;

[assembly: InternalsVisibleTo("JigsawVina.Tests")]

namespace JigsawVina.Presentation.Screens
{
    public class PictureSelectView : MonoBehaviour
    {
        public event Action<int> OnPictureSelected;

        [SerializeField] private PictureSelectCard _cardPrefab;
        [SerializeField] private RectTransform _contentContainer;

        private readonly List<PictureSelectCard> _instantiatedCards = new();

        internal IReadOnlyList<PictureSelectCard> InstantiatedCards => _instantiatedCards;

        public void Setup(IReadOnlyList<PictureConfig> pictures)
        {
            ClearExistingCards();

            if (pictures == null || pictures.Count == 0)
            {
                Debug.LogError("[JigsawVina] PictureSelectView: Pictures list is null or empty.");
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

            foreach (var picture in pictures)
            {
                if (picture.Id <= 0)
                {
                    Debug.LogError($"[JigsawVina] Data error: Picture has an invalid ID ({picture.Id}).");
                    continue;
                }

                var cardInstance = Instantiate(_cardPrefab, _contentContainer, false);
                cardInstance.gameObject.name = $"PictureCard_{picture.Id}_{picture.IdString}";
                cardInstance.gameObject.SetActive(true);

                cardInstance.Bind(picture, id => OnPictureSelected?.Invoke(id));
                _instantiatedCards.Add(cardInstance);
            }
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
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
