using System;
using System.Collections.Generic;
using JigsawVina.Core.Data;
using JigsawVina.Core.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace JigsawVina.Presentation.Screens
{
    public class PuzzlePlayingPresenter
    {
        private const float DefaultPreviewOpacity = 0.2f;

        public event Action<float> OnPuzzleCompleted;
        public event Action OnBackRequested;
        public event Action OnQuitRequested;

        private readonly PuzzlePlayingView _view;
        private readonly GameSessionService _sessionService;
        private readonly IStaticDataService _staticDataService;
        private readonly ISaveDataService _saveDataService;
        private readonly ILocalizationService _localizationService;
        private readonly IAudioService _audioService;

        private PuzzleSession _puzzleSession;
        private List<PuzzlePieceView> _pieceViews = new();
        private Vector2 _boardCellSize;
        private Texture2D _texture;
        private bool _isCompleted;
        private Vector3 _dragPointerWorldOffset;

        public PuzzlePlayingPresenter(
            PuzzlePlayingView view,
            GameSessionService sessionService,
            IStaticDataService staticDataService,
            ISaveDataService saveDataService,
            ILocalizationService localizationService,
            IAudioService audioService)
        {
            _view = view;
            _sessionService = sessionService;
            _staticDataService = staticDataService;
            _saveDataService = saveDataService;
            _localizationService = localizationService;
            _audioService = audioService;
        }

        public void Initialize()
        {
            if (_sessionService.SelectedPictureId == 0)
            {
                var allPics = _staticDataService.GetAllPictures();
                if (allPics != null && allPics.Count > 0)
                {
                    _sessionService.SetSelectedPicture(allPics[0].Id);
                    _sessionService.SetSelectedDifficulty(0);
                }
            }

            var picture = _staticDataService.GetPictureById(_sessionService.SelectedPictureId);
            var config = _staticDataService.GetPictureDifficulty(_sessionService.SelectedPictureId, _sessionService.SelectedDifficultyId);

            _sessionService.BeginPuzzle();
            _puzzleSession = new PuzzleSession(config.Columns, config.Rows);
            _isCompleted = false;

            _view.UpdateTimer(0f);
            _view.SetPreviewOpacity(DefaultPreviewOpacity);

            var save = _saveDataService.Load();
            _view.UpdateHintButtonLabel(save.Hints);

            _texture = Resources.Load<Texture2D>(picture.AssetPath);
            if (_texture == null)
            {
                _texture = Resources.Load<Texture2D>("Textures/" + picture.AssetPath);
            }
            if (_texture == null)
            {
                _texture = new Texture2D(400, 300);
            }

            var boardSprite = Sprite.Create(_texture, new Rect(0, 0, _texture.width, _texture.height), new Vector2(0.5f, 0.5f));
            _view.BoardView.Initialize(boardSprite);

            float cellWidth = (float)_texture.width / config.Columns;
            float cellHeight = (float)_texture.height / config.Rows;

            foreach (Transform child in _view.TrayContent)
            {
                UnityEngine.Object.Destroy(child.gameObject);
            }
            foreach (Transform child in _view.BoardView.LockedPiecesContainer)
            {
                UnityEngine.Object.Destroy(child.gameObject);
            }
            _pieceViews.Clear();

            Vector2 boardSize = _view.BoardView.RectTransform.rect.size;
            _boardCellSize = new Vector2(boardSize.x / config.Columns, boardSize.y / config.Rows);

            for (int i = 0; i < _puzzleSession.PieceCount; i++)
            {
                int c = i % config.Columns;
                int r = i / config.Columns;

                var rect = new Rect(c * cellWidth, r * cellHeight, cellWidth, cellHeight);
                var sprite = Sprite.Create(_texture, rect, new Vector2(0.5f, 0.5f));

                var pieceGo = new GameObject($"Piece_{i}", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
                pieceGo.transform.SetParent(_view.TrayContent, false);

                var pieceView = pieceGo.AddComponent<PuzzlePieceView>();
                pieceView.Initialize(i, sprite, new Vector2(120f, 90f));

                pieceView.OnPiecePointerDown += HandlePiecePointerDown;
                pieceView.OnPieceDragBegin += HandlePieceDragBegin;
                pieceView.OnPieceDrag += HandlePieceDrag;
                pieceView.OnPieceDragEnd += HandlePieceDragEnd;

                _pieceViews.Add(pieceView);
            }

            ShuffleTrayPieces();

            _view.OnBackClicked += HandleBackClicked;
            _view.OnPauseClicked += HandlePauseClicked;
            _view.OnHintClicked += ApplyHint;
            _view.OnReturnToTrayClicked += ReturnAllFloatingToTray;
            _view.OnPreviewOpacityChanged += SetPreviewOpacity;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _view.OnCheatWinClicked += CheatWin;
#endif

            if (_view.SettingsPopup != null)
            {
                _view.SettingsPopup.Setup(_localizationService);
                _view.SettingsPopup.OnMusicToggleChanged += HandleMusicToggle;
                _view.SettingsPopup.OnSfxToggleChanged += HandleSfxToggle;
                _view.SettingsPopup.OnLanguageSelectionChanged += HandleLanguageSelection;
                _view.SettingsPopup.OnResumeClicked += HandleResume;
                _view.SettingsPopup.OnQuitClicked += HandleQuit;
                _view.SettingsPopup.Hide();
            }

            if (_localizationService != null)
            {
                _localizationService.OnLanguageChanged += TranslateTexts;
            }

            TranslateTexts();

            _audioService?.PlayBGM("Audio/BGM/bgm_gameplay");
        }

        public void Cleanup()
        {
            if (_view != null)
            {
                _view.OnBackClicked -= HandleBackClicked;
                _view.OnPauseClicked -= HandlePauseClicked;
                _view.OnHintClicked -= ApplyHint;
                _view.OnReturnToTrayClicked -= ReturnAllFloatingToTray;
                _view.OnPreviewOpacityChanged -= SetPreviewOpacity;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                _view.OnCheatWinClicked -= CheatWin;
#endif

                if (_view.SettingsPopup != null)
                {
                    _view.SettingsPopup.OnMusicToggleChanged -= HandleMusicToggle;
                    _view.SettingsPopup.OnSfxToggleChanged -= HandleSfxToggle;
                    _view.SettingsPopup.OnLanguageSelectionChanged -= HandleLanguageSelection;
                    _view.SettingsPopup.OnResumeClicked -= HandleResume;
                    _view.SettingsPopup.OnQuitClicked -= HandleQuit;
                }
            }

            if (_localizationService != null)
            {
                _localizationService.OnLanguageChanged -= TranslateTexts;
            }
        }

        private void TranslateTexts()
        {
            if (_view == null || _localizationService == null) return;

            var picture = _staticDataService.GetPictureById(_sessionService.SelectedPictureId);
            var config = _staticDataService.GetPictureDifficulty(_sessionService.SelectedPictureId, _sessionService.SelectedDifficultyId);

            string picTranslatedName = _localizationService.Get(picture.DisplayNameKey);
            string diffTranslatedName = _localizationService.Get(config.DisplayName); // Difficulty name might be direct key

            _view.Setup(picTranslatedName, diffTranslatedName);
            _view.UpdateTimer(_puzzleSession != null ? _puzzleSession.ElapsedTime : 0f);

            var save = _saveDataService.Load();
            _view.UpdateHintButtonLabel(save.Hints);

            // Translate Static UI Buttons
            var backBtnText = _view.transform.Find("TopBar/BackButton")?.GetComponentInChildren<TMP_Text>();
            if (backBtnText != null)
                backBtnText.text = _localizationService.Get(LocalizationKeys.GameplayBack);

            var resetBtnText = _view.transform.Find("TopBar/ReturnToTrayButton")?.GetComponentInChildren<TMP_Text>();
            if (resetBtnText != null)
                resetBtnText.text = _localizationService.Get(LocalizationKeys.GameplayReset);

            var cheatBtnText = _view.transform.Find("TopBar/CheatWinButton")?.GetComponentInChildren<TMP_Text>();
            if (cheatBtnText != null)
                cheatBtnText.text = _localizationService.Get(LocalizationKeys.GameplayCheat);
        }

        public float GetElapsedTime()
        {
            return _puzzleSession != null ? _puzzleSession.ElapsedTime : 0f;
        }

        public void Tick()
        {
            Tick(Time.deltaTime);
        }

        public void Tick(float deltaTime)
        {
            if (_puzzleSession == null || _puzzleSession.IsCompleted) return;
            _puzzleSession.Tick(deltaTime);
            _view.UpdateTimer(_puzzleSession.ElapsedTime);
        }

        private void HandleBackClicked()
        {
            if (_puzzleSession != null && _puzzleSession.IsPaused) return;
            _audioService?.PlaySFX("Audio/SFX/sfx_button_click");
            OnBackRequested?.Invoke();
        }

        private void HandlePauseClicked()
        {
            if (_puzzleSession == null || _puzzleSession.IsCompleted || _view.SettingsPopup == null) return;
            _audioService?.PlaySFX("Audio/SFX/sfx_button_click");

            _puzzleSession.IsPaused = true;
            var save = _saveDataService.Load();
            _view.SettingsPopup.Show(save.MusicEnabledState == 1, save.SfxEnabledState == 1, save.Language);
        }

        private void HandleMusicToggle(bool isOn)
        {
            _audioService?.SetMusicEnabled(isOn);
        }

        private void HandleSfxToggle(bool isOn)
        {
            _audioService?.SetSfxEnabled(isOn);
            _audioService?.PlaySFX("Audio/SFX/sfx_button_click");
        }

        private void HandleLanguageSelection(string langCode)
        {
            _localizationService?.SetLanguage(langCode);
        }

        private void HandleResume()
        {
            _audioService?.PlaySFX("Audio/SFX/sfx_button_click");
            if (_puzzleSession != null)
            {
                _puzzleSession.IsPaused = false;
            }
            _view.SettingsPopup?.Hide();
        }

        private void HandleQuit()
        {
            _audioService?.PlaySFX("Audio/SFX/sfx_button_click");
            if (_puzzleSession != null)
            {
                _puzzleSession.IsPaused = false;
            }
            _view.SettingsPopup?.Hide();
            OnQuitRequested?.Invoke();
        }

        private void HandlePiecePointerDown(PuzzlePieceView piece)
        {
            if (_puzzleSession == null || _puzzleSession.IsPaused) return;
            _puzzleSession.LastInteractedPieceIndex = piece.Index;
            _audioService?.PlaySFX("Audio/SFX/sfx_drag_start");
        }

        private void HandlePieceDragBegin(PuzzlePieceView piece, PointerEventData eventData)
        {
            if (_puzzleSession == null || _puzzleSession.IsPaused) return;
            _puzzleSession.LastInteractedPieceIndex = piece.Index;
            _puzzleSession.UpdatePieceState(piece.Index, PuzzleSession.PieceState.Floating);

            var pieceRect = piece.GetComponent<RectTransform>();
            piece.transform.SetParent(_view.DragContainer, true);
            pieceRect.sizeDelta = _boardCellSize;

            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                _view.DragContainer,
                eventData.position,
                eventData.pressEventCamera,
                out Vector3 pointerWorld))
            {
                _dragPointerWorldOffset = pieceRect.position - pointerWorld;
            }
            else
            {
                _dragPointerWorldOffset = Vector3.zero;
            }
        }

        private void HandlePieceDrag(PuzzlePieceView piece, PointerEventData eventData)
        {
            if (_puzzleSession == null || _puzzleSession.IsPaused) return;
            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
                _view.DragContainer,
                eventData.position,
                eventData.pressEventCamera,
                out Vector3 pointerWorld))
            {
                piece.transform.position = pointerWorld + _dragPointerWorldOffset;
            }
        }

        private void HandlePieceDragEnd(PuzzlePieceView piece, Vector2 screenPosition)
        {
            if (_puzzleSession == null || _puzzleSession.IsPaused) return;

            var boardRect = _view.BoardView.RectTransform;
            Vector2 boardSize = boardRect.rect.size;
            Vector2 localOnBoard = boardRect.InverseTransformPoint(piece.transform.position);
            bool snapped = _puzzleSession.CheckSnap(piece.Index, localOnBoard, boardSize);

            if (snapped)
            {
                LockPieceView(piece, boardSize);
                _audioService?.PlaySFX("Audio/SFX/sfx_snap_success");
                CheckWinCondition();
            }
            else
            {
                _puzzleSession.UpdatePieceState(piece.Index, PuzzleSession.PieceState.Floating);
                piece.ShowIncorrectFeedback();
                _audioService?.PlaySFX("Audio/SFX/sfx_snap_fail");
            }
        }

        public void ApplyHint()
        {
            if (_puzzleSession == null || _puzzleSession.IsPaused) return;

            var save = _saveDataService.Load();
            if (save.Hints <= 0) return;

            int hintIndex = _puzzleSession.GetHintPieceIndex();
            if (hintIndex == -1) return;

            save.Hints--;
            _saveDataService.Save(save);
            _view.UpdateHintButtonLabel(save.Hints);

            var piece = _pieceViews[hintIndex];
            _puzzleSession.LockPiece(hintIndex);

            var boardSize = _view.BoardView.RectTransform.rect.size;
            LockPieceView(piece, boardSize);

            _audioService?.PlaySFX("Audio/SFX/sfx_hint");
            CheckWinCondition();
        }

        public void ReturnAllFloatingToTray()
        {
            if (_puzzleSession == null || _puzzleSession.IsPaused) return;

            _audioService?.PlaySFX("Audio/SFX/sfx_button_click");
            for (int i = 0; i < _puzzleSession.Pieces.Count; i++)
            {
                if (_puzzleSession.Pieces[i].State == PuzzleSession.PieceState.Floating)
                {
                    var piece = _pieceViews[i];
                    piece.transform.SetParent(_view.TrayContent, false);
                    piece.GetComponent<RectTransform>().sizeDelta = new Vector2(120f, 90f);
                }
            }
            _puzzleSession.ReturnAllFloatingToTray();
        }

        private void SetPreviewOpacity(float opacity)
        {
            _view.BoardView.SetPreviewOpacity(opacity);
        }

        private void ShuffleTrayPieces()
        {
            int pieceCount = _pieceViews.Count;
            var shuffledIndices = new List<int>(pieceCount);
            for (int i = 0; i < pieceCount; i++)
            {
                shuffledIndices.Add(i);
            }

            for (int i = pieceCount - 1; i > 0; i--)
            {
                int swapIndex = UnityEngine.Random.Range(0, i + 1);
                (shuffledIndices[i], shuffledIndices[swapIndex]) = (shuffledIndices[swapIndex], shuffledIndices[i]);
            }

            bool orderChanged = false;
            for (int i = 0; i < pieceCount; i++)
            {
                if (shuffledIndices[i] != i)
                {
                    orderChanged = true;
                    break;
                }
            }

            if (!orderChanged && pieceCount > 1)
            {
                (shuffledIndices[0], shuffledIndices[1]) = (shuffledIndices[1], shuffledIndices[0]);
            }

            for (int siblingIndex = 0; siblingIndex < pieceCount; siblingIndex++)
            {
                _pieceViews[shuffledIndices[siblingIndex]].transform.SetSiblingIndex(siblingIndex);
            }
        }

        private void LockPieceView(PuzzlePieceView piece, Vector2 boardSize)
        {
            piece.transform.SetParent(_view.BoardView.LockedPiecesContainer, false);

            var pieceRect = piece.GetComponent<RectTransform>();
            pieceRect.anchorMin = Vector2.one * 0.5f;
            pieceRect.anchorMax = Vector2.one * 0.5f;
            pieceRect.pivot = Vector2.one * 0.5f;
            pieceRect.sizeDelta = _boardCellSize;
            pieceRect.anchoredPosition = _puzzleSession.GetLocalTargetPosition(piece.Index, boardSize);
            piece.SetLocked(true);
        }

        private void CheckWinCondition()
        {
            if (_puzzleSession.IsCompleted && !_isCompleted)
            {
                _isCompleted = true;
                _audioService?.PlaySFX("Audio/SFX/sfx_win");
                OnPuzzleCompleted?.Invoke(_puzzleSession.ElapsedTime);
            }
        }

        public void CheatWin()
        {
            if (_puzzleSession == null || _puzzleSession.IsPaused || _isCompleted) return;

            var boardSize = _view.BoardView.RectTransform.rect.size;
            for (int i = 0; i < _puzzleSession.PieceCount; i++)
            {
                if (_puzzleSession.Pieces[i].State != PuzzleSession.PieceState.Locked)
                {
                    _puzzleSession.LockPiece(i);
                    var piece = _pieceViews[i];
                    LockPieceView(piece, boardSize);
                }
            }
            CheckWinCondition();
        }
    }
}
