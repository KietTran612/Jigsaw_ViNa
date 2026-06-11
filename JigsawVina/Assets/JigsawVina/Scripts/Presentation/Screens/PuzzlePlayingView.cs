using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JigsawVina.Presentation.Screens
{
    public class PuzzlePlayingView : MonoBehaviour
    {
        public event Action OnBackClicked;
        public event Action OnPreviewClicked;
        public event Action OnHintClicked;
        public event Action OnReturnToTrayClicked;
        public event Action OnCheatWinClicked;

        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _previewButton;
        [SerializeField] private Button _hintButton;
        [SerializeField] private Button _returnToTrayButton;
        [SerializeField] private Button _cheatWinButton;

        [SerializeField] private PuzzleBoardView _boardView;
        [SerializeField] private RectTransform _trayContent;
        [SerializeField] private RectTransform _dragContainer;
        [SerializeField] private Canvas _canvas;

        public PuzzleBoardView BoardView => _boardView;
        public RectTransform TrayContent => _trayContent;
        public RectTransform DragContainer => _dragContainer;
        public Canvas Canvas => _canvas;

        private void Awake()
        {
            if (_backButton != null)
                _backButton.onClick.AddListener(() => OnBackClicked?.Invoke());
            if (_previewButton != null)
                _previewButton.onClick.AddListener(() => OnPreviewClicked?.Invoke());
            if (_hintButton != null)
                _hintButton.onClick.AddListener(() => OnHintClicked?.Invoke());
            if (_returnToTrayButton != null)
                _returnToTrayButton.onClick.AddListener(() => OnReturnToTrayClicked?.Invoke());
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (_cheatWinButton != null)
                _cheatWinButton.onClick.AddListener(() => OnCheatWinClicked?.Invoke());
#else
            if (_cheatWinButton != null)
                _cheatWinButton.gameObject.SetActive(false);
#endif

            if (_dragContainer != null)
                _dragContainer.SetAsLastSibling();
        }

        public void Setup(string pictureName, string difficultyName)
        {
            if (_titleText != null)
            {
                _titleText.text = $"{pictureName} - {difficultyName}";
            }
        }

        public void UpdateTimer(float elapsedSeconds)
        {
            if (_timerText != null)
            {
                int minutes = Mathf.FloorToInt(elapsedSeconds / 60f);
                int seconds = Mathf.FloorToInt(elapsedSeconds % 60f);
                _timerText.text = $"Thời gian: {minutes:00}:{seconds:00}";
            }
        }

        public void UpdateHintButtonLabel(int hintCount)
        {
            if (_hintButton != null)
            {
                var textComponent = _hintButton.GetComponentInChildren<TMP_Text>();
                if (textComponent != null)
                {
                    textComponent.text = $"Gợi ý ({hintCount})";
                }
            }
        }

        public void DisableAllInput()
        {
            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
            if (active)
            {
                var canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                }
            }
        }
    }
}
