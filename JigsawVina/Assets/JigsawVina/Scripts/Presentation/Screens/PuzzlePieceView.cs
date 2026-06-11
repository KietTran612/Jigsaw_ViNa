using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JigsawVina.Presentation.Screens
{
    public class PuzzlePieceView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
    {
        public event Action<PuzzlePieceView> OnPiecePointerDown;
        public event Action<PuzzlePieceView, PointerEventData> OnPieceDragBegin;
        public event Action<PuzzlePieceView, PointerEventData> OnPieceDrag;
        public event Action<PuzzlePieceView, Vector2> OnPieceDragEnd;

        [SerializeField] private Image _image;
        
        public int Index { get; private set; }
        public bool IsLocked { get; private set; }

        private Vector2 _startPosition;
        private bool _isClassificationPending;
        private bool _isDraggingPiece;
        private bool _isScrollingTray;
        private ScrollRect _activeScrollRect;
        private RectTransform _rectTransform;
        private Outline _invalidOutline;
        private Coroutine _invalidFeedbackCoroutine;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public void Initialize(int index, Sprite sprite, Vector2 size)
        {
            Index = index;
            IsLocked = false;
            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }
            if (_image == null)
            {
                _image = GetComponent<Image>();
            }
            if (_invalidOutline == null)
            {
                _invalidOutline = GetComponent<Outline>();
                if (_invalidOutline == null)
                {
                    _invalidOutline = gameObject.AddComponent<Outline>();
                }

                _invalidOutline.effectColor = new Color(0.95f, 0.12f, 0.12f, 1f);
                _invalidOutline.effectDistance = new Vector2(4f, -4f);
                _invalidOutline.useGraphicAlpha = true;
                _invalidOutline.enabled = false;
            }
            if (_image != null)
            {
                _image.sprite = sprite;
            }
            if (_rectTransform != null)
            {
                _rectTransform.sizeDelta = size;
            }
        }

        public void SetLocked(bool locked)
        {
            IsLocked = locked;
            if (locked)
            {
                ClearIncorrectFeedback();
            }
            if (_image != null)
            {
                _image.raycastTarget = !locked;
            }
        }

        public void ShowIncorrectFeedback()
        {
            if (IsLocked || _rectTransform == null)
            {
                return;
            }

            if (_invalidOutline != null)
            {
                _invalidOutline.enabled = true;
            }

            if (_invalidFeedbackCoroutine != null)
            {
                StopCoroutine(_invalidFeedbackCoroutine);
            }
            _invalidFeedbackCoroutine = StartCoroutine(ShakeIncorrectPiece());
        }

        public void ClearIncorrectFeedback()
        {
            if (_invalidFeedbackCoroutine != null)
            {
                StopCoroutine(_invalidFeedbackCoroutine);
                _invalidFeedbackCoroutine = null;
            }

            if (_invalidOutline != null)
            {
                _invalidOutline.enabled = false;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (IsLocked) return;
            OnPiecePointerDown?.Invoke(this);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (IsLocked) return;

            ClearIncorrectFeedback();
            _startPosition = eventData.position;
            _isClassificationPending = true;
            _isDraggingPiece = false;
            _isScrollingTray = false;
            _activeScrollRect = GetComponentInParent<ScrollRect>();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (IsLocked) return;

            if (_isClassificationPending)
            {
                Vector2 delta = eventData.position - _startPosition;
                if (delta.magnitude >= 10f)
                {
                    _isClassificationPending = false;
                    if (_activeScrollRect == null || Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    {
                        _isDraggingPiece = true;
                        OnPieceDragBegin?.Invoke(this, eventData);
                    }
                    else
                    {
                        _isScrollingTray = true;
                        _activeScrollRect.OnBeginDrag(eventData);
                    }
                }
            }

            if (!_isClassificationPending)
            {
                if (_isDraggingPiece)
                {
                    OnPieceDrag?.Invoke(this, eventData);
                }
                else if (_isScrollingTray && _activeScrollRect != null)
                {
                    _activeScrollRect.OnDrag(eventData);
                }
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (IsLocked) return;

            if (_isDraggingPiece)
            {
                OnPieceDragEnd?.Invoke(this, eventData.position);
            }
            else if (_isScrollingTray && _activeScrollRect != null)
            {
                _activeScrollRect.OnEndDrag(eventData);
            }

            _isClassificationPending = false;
            _isDraggingPiece = false;
            _isScrollingTray = false;
            _activeScrollRect = null;
        }

        private IEnumerator ShakeIncorrectPiece()
        {
            Vector2 basePosition = _rectTransform.anchoredPosition;
            const float duration = 0.4f;
            const float amplitude = 12f;
            const float frequency = 28f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float strength = 1f - Mathf.Clamp01(elapsed / duration);
                float offset = Mathf.Sin(elapsed * frequency) * amplitude * strength;
                _rectTransform.anchoredPosition = basePosition + Vector2.right * offset;
                yield return null;
            }

            _rectTransform.anchoredPosition = basePosition;
            _invalidFeedbackCoroutine = null;
        }
    }
}
