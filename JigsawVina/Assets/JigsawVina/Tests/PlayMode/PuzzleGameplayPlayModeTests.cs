using System;
using System.Collections;
using JigsawVina.Core.Data;
using JigsawVina.Core.Services;
using JigsawVina.Presentation.Screens;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace JigsawVina.Tests
{
    public class PuzzleGameplayPlayModeTests
    {
        private sealed class MockSaveDataService : ISaveDataService
        {
            public PlayerSave SaveData { get; private set; } = new();

            public PlayerSave Load()
            {
                return SaveData;
            }

            public void Save(PlayerSave save)
            {
                SaveData = save;
            }
        }

        private class MockLocalizationService : ILocalizationService
        {
            public event System.Action OnLanguageChanged;
            public string CurrentLanguage => "vi";
            public void SetLanguage(string langCode) { OnLanguageChanged?.Invoke(); }
            public string Get(string key) => key;
            public string GetFormat(string key, params object[] args) => string.Format(key, args);
        }

        private class MockAudioService : IAudioService
        {
            public void PlayBGM(string clipPath, bool loop = true, float fadeDuration = 0.5f) {}
            public void StopBGM(float fadeDuration = 0.5f) {}
            public void PlaySFX(string clipPath, float volumeScale = 1f) {}
            public void SetMusicEnabled(bool enabled) {}
            public void SetSfxEnabled(bool enabled) {}
        }

        private GameObject _root;
        private Canvas _canvas;
        private PuzzlePlayingView _view;
        private PuzzlePlayingPresenter _presenter;
        private GameSessionService _sessionService;
        private MockSaveDataService _saveService;
        private StaticDataService _staticDataService;
        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("TestRoot");
            new GameObject("EventSystem", typeof(EventSystem)).transform.SetParent(_root.transform);
            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(_root.transform);
            _canvas = canvasGo.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            // Setup Board Container (Position at world zero, size 800x600)
            var boardContainer = new GameObject("BoardContainer", typeof(RectTransform));
            boardContainer.transform.SetParent(_canvas.transform, false);
            var boardContainerRect = (RectTransform)boardContainer.transform;
            boardContainerRect.sizeDelta = new Vector2(800f, 600f);

            var boardGo = new GameObject("Board", typeof(RectTransform));
            boardGo.transform.SetParent(boardContainer.transform, false);
            var boardRect = (RectTransform)boardGo.transform;
            boardRect.sizeDelta = new Vector2(800f, 600f);
            boardGo.AddComponent<Image>();
            
            var previewObj = new GameObject("PreviewOverlay", typeof(RectTransform));
            previewObj.transform.SetParent(boardGo.transform, false);
            previewObj.AddComponent<Image>();

            var lockedObj = new GameObject("LockedPieces", typeof(RectTransform));
            lockedObj.transform.SetParent(boardGo.transform, false);
            var lockedRect = (RectTransform)lockedObj.transform;
            lockedRect.sizeDelta = new Vector2(800f, 600f);

            var boardView = boardGo.AddComponent<PuzzleBoardView>();
            AssignField(boardView, "_previewImage", previewObj.GetComponent<Image>());
            AssignField(boardView, "_lockedPiecesContainer", lockedRect);

            // Setup View Screen (canvas group)
            var viewGo = new GameObject("View", typeof(PuzzlePlayingView), typeof(CanvasGroup));
            viewGo.transform.SetParent(_canvas.transform, false);
            _view = viewGo.GetComponent<PuzzlePlayingView>();

            var trayContent = new GameObject("TrayContent", typeof(RectTransform));
            trayContent.transform.SetParent(viewGo.transform, false);

            var dragContainer = new GameObject("DragContainer", typeof(RectTransform));
            dragContainer.transform.SetParent(viewGo.transform, false);

            AssignField(_view, "_boardView", boardView);
            AssignField(_view, "_trayContent", (RectTransform)trayContent.transform);
            AssignField(_view, "_dragContainer", (RectTransform)dragContainer.transform);
            AssignField(_view, "_canvas", _canvas);

            // Services
            _sessionService = new GameSessionService();
            _sessionService.SetSelectedPicture(1);
            _sessionService.SetSelectedDifficulty(0); // Easy: 6x4 = 24

            _saveService = new MockSaveDataService();
            _staticDataService = new StaticDataService();
            var mockLoc = new MockLocalizationService();
            var mockAudio = new MockAudioService();

            _presenter = new PuzzlePlayingPresenter(_view, _sessionService, _staticDataService, _saveService, mockLoc, mockAudio);
        }

        [TearDown]
        public void TearDown()
        {
            _presenter.Cleanup();
            UnityEngine.Object.DestroyImmediate(_root);
        }

        [UnityTest]
        public IEnumerator PuzzlePlay_InitializesCorrectPieceCount()
        {
            _presenter.Initialize();
            yield return null;

            Assert.AreEqual(24, _view.TrayContent.childCount);
            Assert.AreEqual(0.2f, _view.BoardView.PreviewImage.color.a, 0.001f);

            bool orderChanged = false;
            for (int i = 0; i < _view.TrayContent.childCount; i++)
            {
                if (_view.TrayContent.GetChild(i).GetComponent<PuzzlePieceView>().Index != i)
                {
                    orderChanged = true;
                    break;
                }
            }
            Assert.IsTrue(orderChanged);
        }

        [UnityTest]
        public IEnumerator PuzzlePlay_DragPreservesPointerOffsetOnScaledCanvas()
        {
            _presenter.Initialize();
            yield return null;

            var piece = _view.TrayContent.GetChild(0).GetComponent<PuzzlePieceView>();
            var pieceRect = (RectTransform)piece.transform;
            Vector2 pieceScreenBefore = RectTransformUtility.WorldToScreenPoint(null, pieceRect.position);
            Vector2 pointerStart = pieceScreenBefore + new Vector2(25f, 10f);

            var eventData = new PointerEventData(EventSystem.current)
            {
                position = pointerStart,
                pressPosition = pointerStart
            };

            piece.OnBeginDrag(eventData);
            eventData.position = pointerStart + new Vector2(20f, 0f);
            piece.OnDrag(eventData);

            Vector2 pieceScreenAfterBegin = RectTransformUtility.WorldToScreenPoint(null, pieceRect.position);
            Assert.AreEqual(pieceScreenBefore.x, pieceScreenAfterBegin.x, 1f);
            Assert.AreEqual(pieceScreenBefore.y, pieceScreenAfterBegin.y, 1f);

            eventData.position += new Vector2(100f, 40f);
            piece.OnDrag(eventData);

            Vector2 pieceScreenAfterMove = RectTransformUtility.WorldToScreenPoint(null, pieceRect.position);
            Assert.AreEqual(pieceScreenBefore.x + 100f, pieceScreenAfterMove.x, 1f);
            Assert.AreEqual(pieceScreenBefore.y + 40f, pieceScreenAfterMove.y, 1f);
        }

        [UnityTest]
        public IEnumerator PuzzlePlay_ReturnToTray_RestoresFloatingPieces()
        {
            _presenter.Initialize();
            yield return null;

            PuzzlePieceView pieceView0 = null;
            for (int i = 0; i < _view.TrayContent.childCount; i++)
            {
                var p = _view.TrayContent.GetChild(i).GetComponent<PuzzlePieceView>();
                if (p.Index == 0)
                {
                    pieceView0 = p;
                    break;
                }
            }
            var piece0 = pieceView0.transform;
            piece0.SetParent(_view.DragContainer, false);

            Assert.AreEqual(23, _view.TrayContent.childCount);

            // Mark piece 0 as floating in session so ReturnAllFloatingToTray processes it
            var session = GetPrivateField<PuzzleSession>(_presenter, "_puzzleSession");
            session.UpdatePieceState(0, PuzzleSession.PieceState.Floating);

            TriggerEvent(_view, "OnReturnToTrayClicked");
            yield return null;

            Assert.AreEqual(24, _view.TrayContent.childCount);
        }

        [UnityTest]
        public IEnumerator FloatingPiece_VerticalGesture_StartsPieceDrag()
        {
            _presenter.Initialize();
            yield return null;

            var piece = _view.TrayContent.GetChild(0).GetComponent<PuzzlePieceView>();
            piece.transform.SetParent(_view.DragContainer, false);

            int dragBeginCount = 0;
            piece.OnPieceDragBegin += (_, _) => dragBeginCount++;

            var eventData = new PointerEventData(EventSystem.current)
            {
                position = Vector2.zero
            };
            piece.OnBeginDrag(eventData);
            eventData.position = new Vector2(0f, 20f);
            piece.OnDrag(eventData);

            Assert.AreEqual(1, dragBeginCount);
        }

        [UnityTest]
        public IEnumerator PuzzlePlay_HintConsumption_LocksCorrectPiece()
        {
            _saveService.SaveData.Hints = 5;
            _presenter.Initialize();
            yield return null;

            TriggerEvent(_view, "OnHintClicked");
            yield return null;

            Assert.AreEqual(4, _saveService.Load().Hints);
            Assert.AreEqual(1, _view.BoardView.LockedPiecesContainer.childCount);
            
            var piece = _view.BoardView.LockedPiecesContainer.GetChild(0).GetComponent<PuzzlePieceView>();
            Assert.IsTrue(piece.IsLocked);
        }

        [UnityTest]
        public IEnumerator PuzzlePlay_SnapClosePiece_LocksAssertsCorrectly()
        {
            _presenter.Initialize();
            yield return null;

            PuzzlePieceView pieceView = null;
            for (int i = 0; i < _view.TrayContent.childCount; i++)
            {
                var p = _view.TrayContent.GetChild(i).GetComponent<PuzzlePieceView>();
                if (p.Index == 0)
                {
                    pieceView = p;
                    break;
                }
            }
            
            // Move piece to Board local target (calculated from bottom-left corner offset in board space)
            pieceView.transform.SetParent(_view.DragContainer, false);
            // Piece 0 target local is (-333.33f, -225f)
            pieceView.transform.position = _view.BoardView.RectTransform.TransformPoint(new Vector3(-333.33f, -225f, 0f));

            // Target size cell size Easy (6x4) inside 800x600 = Vector2(133.33f, 150f)
            Vector2 expectedCellSize = new Vector2(800f / 6f, 600f / 4f);

            // Trigger OnPieceDragEnd directly to force snap logic with correct position
            TriggerPieceDragEnd(pieceView, pieceView.transform.position);
            yield return null;

            // Assertions checking size layout, locked state, locked container parenting, and position snap
            Assert.IsTrue(pieceView.IsLocked);
            Assert.AreEqual(_view.BoardView.LockedPiecesContainer, pieceView.transform.parent);
            
            var rect = pieceView.GetComponent<RectTransform>();
            Assert.AreEqual(expectedCellSize.x, rect.sizeDelta.x, 0.1f);
            Assert.AreEqual(expectedCellSize.y, rect.sizeDelta.y, 0.1f);
            Assert.AreEqual(Vector2.one * 0.5f, rect.anchorMin);
            Assert.AreEqual(Vector2.one * 0.5f, rect.anchorMax);
            Assert.AreEqual(-333.33f, rect.anchoredPosition.x, 0.1f);
            Assert.AreEqual(-225f, rect.anchoredPosition.y, 0.1f);
        }

        [UnityTest]
        public IEnumerator PuzzlePlay_FailedSnap_ShowsRedOutlineUntilNextDrag()
        {
            _presenter.Initialize();
            yield return null;

            var pieceView = _view.TrayContent.GetChild(0).GetComponent<PuzzlePieceView>();
            pieceView.transform.SetParent(_view.DragContainer, false);
            TriggerPieceDragEnd(pieceView, Vector2.zero);
            yield return null;

            var outline = pieceView.GetComponent<Outline>();
            Assert.IsNotNull(outline);
            Assert.IsTrue(outline.enabled);
            Assert.Greater(outline.effectColor.r, outline.effectColor.g);

            var eventData = new PointerEventData(EventSystem.current)
            {
                position = Vector2.zero
            };
            pieceView.OnBeginDrag(eventData);

            Assert.IsFalse(outline.enabled);
        }

        [UnityTest]
        public IEnumerator PuzzlePlay_CompleteLifecycle_LocksInputTimerAndPersistsSingleRecord()
        {
            var rewardGo = new GameObject("RewardView", typeof(RewardSummaryView));
            rewardGo.transform.SetParent(_canvas.transform, false);
            var rewardView = rewardGo.GetComponent<RewardSummaryView>();

            var starsTextObj = new GameObject("StarsText", typeof(RectTransform));
            starsTextObj.transform.SetParent(rewardGo.transform, false);
            var starsText = starsTextObj.AddComponent<TMPro.TextMeshProUGUI>();

            var coinsTextObj = new GameObject("CoinsText", typeof(RectTransform));
            coinsTextObj.transform.SetParent(rewardGo.transform, false);
            var coinsText = coinsTextObj.AddComponent<TMPro.TextMeshProUGUI>();

            AssignField(rewardView, "_starsText", starsText);
            AssignField(rewardView, "_coinsText", coinsText);

            var rewardPresenter = new RewardSummaryPresenter(rewardView, _sessionService, _saveService, _staticDataService);

            // Initialize flow
            var flowController = new GameplayFlowController(_view, rewardView, _presenter, rewardPresenter, new SceneLoader());
            flowController.Start();
            yield return null;

            // Check that Tick() increases elapsed time before completion (yield to let frame time advance)
            float initialTime = _presenter.GetElapsedTime();
            yield return null;
            _presenter.Tick(1.0f);
            float tickTime = _presenter.GetElapsedTime();
            Assert.IsTrue(tickTime > initialTime, $"Expected timer to increase during Tick, but tickTime was {tickTime}");
            
            // Trigger win condition by marking all piece data Locked in the presenter's active session, except the first piece
            var session = GetPrivateField<PuzzleSession>(_presenter, "_puzzleSession");
            for (int i = 1; i < session.PieceCount; i++)
            {
                session.Pieces[i].State = PuzzleSession.PieceState.Locked;
            }

            // Trigger final check snap callback to trigger win sequence
            PuzzlePieceView firstPiece = null;
            for (int i = 0; i < _view.TrayContent.childCount; i++)
            {
                var p = _view.TrayContent.GetChild(i).GetComponent<PuzzlePieceView>();
                if (p.Index == 0)
                {
                    firstPiece = p;
                    break;
                }
            }
            session.UpdatePieceState(0, PuzzleSession.PieceState.Floating);
            firstPiece.transform.position = _view.BoardView.RectTransform.TransformPoint(new Vector3(-333.33f, -225f, 0f));
            TriggerPieceDragEnd(firstPiece, firstPiece.transform.position);
            yield return null;

            // Check if controls are disabled on win
            var group = _view.GetComponent<CanvasGroup>();
            Assert.IsFalse(group.interactable);

            // Assert timer is stopped (Tick calls do not increase elapsed time after completion)
            float elapsedAtWin = _presenter.GetElapsedTime();
            _presenter.Tick();
            Assert.AreEqual(elapsedAtWin, _presenter.GetElapsedTime());

            // Completion is persisted before the presentation-only animation.
            var savedAtWin = _saveService.Load();
            Assert.AreEqual(1, savedAtWin.CompletedPuzzles.Count);

            yield return new WaitForSecondsRealtime(1.6f);

            // Assert Reward summary is displayed and duplicate processing is guarded.
            Assert.IsTrue(rewardView.gameObject.activeSelf);
            var save = _saveService.Load();
            Assert.AreEqual(1, save.CompletedPuzzles.Count);

            int coinsBefore = save.Coins;
            // Assert duplicate protection: process reward again, verifying it does NOT award extra coins or records
            rewardPresenter.ProcessRewardsAndDisplay(elapsedAtWin);
            
            var saveAfter = _saveService.Load();
            Assert.AreEqual(1, saveAfter.CompletedPuzzles.Count);
            Assert.AreEqual(coinsBefore, saveAfter.Coins); // Verified coins didn't double!
        }

        private static void AssignField(object target, string name, object value)
        {
            var field = target.GetType().GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(target, value);
        }

        private static T GetPrivateField<T>(object target, string name)
        {
            var field = target.GetType().GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (T)field?.GetValue(target);
        }

        private static void TriggerEvent(object target, string name)
        {
            var eventInfo = target.GetType().GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var del = eventInfo?.GetValue(target) as System.MulticastDelegate;
            del?.DynamicInvoke();
        }

        private static void TriggerPieceDragEnd(PuzzlePieceView pieceView, Vector2 pos)
        {
            var eventInfo = typeof(PuzzlePieceView).GetField("OnPieceDragEnd", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var del = eventInfo?.GetValue(pieceView) as System.MulticastDelegate;
            del?.DynamicInvoke(pieceView, pos);
        }
    }
}
