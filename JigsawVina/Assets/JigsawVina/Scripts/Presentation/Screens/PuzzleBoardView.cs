using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace JigsawVina.Presentation.Screens
{
    public class PuzzleBoardView : MonoBehaviour
    {
        private const float DefaultPreviewOpacity = 0.2f;

        [SerializeField] private Image _previewImage;
        [SerializeField] private RectTransform _lockedPiecesContainer;

        public Image PreviewImage => _previewImage;
        public RectTransform LockedPiecesContainer => _lockedPiecesContainer;
        public RectTransform RectTransform => (RectTransform)transform;

        public void Initialize(Sprite sprite)
        {
            if (_previewImage != null)
            {
                _previewImage.sprite = sprite;
                SetPreviewOpacity(DefaultPreviewOpacity);
            }
        }

        public void SetPreviewOpacity(float opacity)
        {
            if (_previewImage != null)
            {
                var color = _previewImage.color;
                color.a = opacity;
                _previewImage.color = color;
            }
        }

        public async UniTask PlayWinAnimationAsync(CancellationToken cancellationToken)
        {
            if (_previewImage == null)
            {
                return;
            }

            _previewImage.gameObject.SetActive(true);
            var color = _previewImage.color;
            color.a = 0f;
            _previewImage.color = color;

            float duration = 1.0f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                cancellationToken.ThrowIfCancellationRequested();
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                color.a = t;
                _previewImage.color = color;
                await UniTask.Yield(cancellationToken);
            }

            color.a = 1f;
            _previewImage.color = color;
        }
    }
}
