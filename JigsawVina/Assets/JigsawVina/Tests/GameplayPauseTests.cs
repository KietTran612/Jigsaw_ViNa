using System;
using System.Collections.Generic;
using JigsawVina.Core.Data;
using JigsawVina.Core.Services;
using JigsawVina.Presentation.Screens;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace JigsawVina.Tests
{
    public class GameplayPauseTests
    {
        private GameObject _viewGo;
        private PuzzlePlayingView _view;
        private GameSessionService _sessionService;
        private MockSaveDataService _saveService;
        private MockStaticDataService _staticData;
        private MockLocalizationService _localization;
        private MockAudioService _audio;
        private PuzzlePlayingPresenter _presenter;

        [SetUp]
        public void SetUp()
        {
            _viewGo = new GameObject("GameplayView");
            _view = _viewGo.AddComponent<PuzzlePlayingView>();
            BuildMinimalView(_viewGo.transform);

            _sessionService = new GameSessionService();
            _saveService = new MockSaveDataService();
            _staticData = new MockStaticDataService();
            _localization = new MockLocalizationService();
            _audio = new MockAudioService();

            _presenter = new PuzzlePlayingPresenter(_view, _sessionService, _staticData, _saveService, _localization, _audio);
        }

        [TearDown]
        public void TearDown()
        {
            _presenter.Cleanup();
            UnityEngine.Object.DestroyImmediate(_viewGo);
        }

        [Test]
        public void PuzzleSession_Tick_WhenPaused_DoesNotIncrementElapsedTime()
        {
            var session = new PuzzleSession(2, 2);
            session.IsPaused = true;

            session.Tick(1.5f);

            Assert.AreEqual(0f, session.ElapsedTime);
        }

        [Test]
        public void PuzzleSession_Tick_WhenNotPaused_IncrementsElapsedTime()
        {
            var session = new PuzzleSession(2, 2);
            session.IsPaused = false;

            session.Tick(1.5f);

            Assert.AreEqual(1.5f, session.ElapsedTime);
        }

        [Test]
        public void ApplyHint_WhenPaused_DoesNotConsumeHintOrLockPiece()
        {
            _saveService.SaveData.Hints = 3;
            _presenter.Initialize();

            var session = GetSession();
            session.IsPaused = true;

            _presenter.ApplyHint();

            Assert.AreEqual(3, _saveService.SaveData.Hints);
            Assert.IsFalse(session.IsCompleted);
        }

        [Test]
        public void CheatWin_WhenPaused_DoesNotWinPuzzle()
        {
            _presenter.Initialize();

            var session = GetSession();
            session.IsPaused = true;

            _presenter.CheatWin();

            Assert.IsFalse(session.IsCompleted);
        }

        [Test]
        public void Pause_WhenSettingsPopupMissing_DoesNotThrowOrPause()
        {
            _presenter.Initialize();
            Assign(_view, "_settingsPopup", null);

            Assert.DoesNotThrow(() => InvokePrivate(_presenter, "HandlePauseClicked"));
            Assert.IsFalse(GetSession().IsPaused);
        }

        [Test]
        public void SettingsPopup_SetupTwice_ReplacesLocalizationSubscription()
        {
            var popupGo = new GameObject("Popup", typeof(GameSettingsPopup));
            var popup = popupGo.GetComponent<GameSettingsPopup>();
            var localization = new CountingLocalizationService();

            popup.Setup(localization);
            popup.Setup(localization);

            Assert.AreEqual(2, localization.AddCount);
            Assert.AreEqual(1, localization.RemoveCount);

            UnityEngine.Object.DestroyImmediate(popupGo);
        }

        private PuzzleSession GetSession()
        {
            var field = typeof(PuzzlePlayingPresenter).GetField(
                "_puzzleSession",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (PuzzleSession)field.GetValue(_presenter);
        }

        private static void InvokePrivate(object target, string methodName)
        {
            var method = target.GetType().GetMethod(
                methodName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(target, null);
        }

        private static void BuildMinimalView(Transform parent)
        {
            var boardGo = new GameObject("Board", typeof(RectTransform));
            boardGo.transform.SetParent(parent, false);
            boardGo.AddComponent<PuzzleBoardView>();
            var boardRect = (RectTransform)boardGo.transform;
            boardRect.sizeDelta = new Vector2(400f, 300f);

            var lockedPiecesGo = new GameObject("LockedPieces", typeof(RectTransform));
            lockedPiecesGo.transform.SetParent(boardGo.transform, false);

            var trayGo = new GameObject("Tray", typeof(RectTransform));
            trayGo.transform.SetParent(parent, false);

            var dragGo = new GameObject("Drag", typeof(RectTransform));
            dragGo.transform.SetParent(parent, false);

            var canvasGo = new GameObject("Canvas", typeof(Canvas));
            canvasGo.transform.SetParent(parent, false);

            var popupGo = new GameObject("Popup", typeof(RectTransform), typeof(CanvasGroup), typeof(GameSettingsPopup));
            popupGo.transform.SetParent(parent, false);

            var view = parent.GetComponent<PuzzlePlayingView>();
            var boardView = boardGo.GetComponent<PuzzleBoardView>();

            Assign(view, "_boardView", boardView);
            Assign(view, "_trayContent", trayGo.GetComponent<RectTransform>());
            Assign(view, "_dragContainer", dragGo.GetComponent<RectTransform>());
            Assign(view, "_canvas", canvasGo.GetComponent<Canvas>());
            Assign(view, "_settingsPopup", popupGo.GetComponent<GameSettingsPopup>());

            Assign(boardView, "_previewImage", boardGo.AddComponent<Image>());
            Assign(boardView, "_lockedPiecesContainer", lockedPiecesGo.GetComponent<RectTransform>());
        }

        private static void Assign(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(
                fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(target, value);
        }

        private class MockSaveDataService : ISaveDataService
        {
            public PlayerSave SaveData = new();
            public PlayerSave Load() => SaveData;
            public void Save(PlayerSave save) => SaveData = save;
        }

        private class MockStaticDataService : IStaticDataService
        {
            public List<PictureConfig> Pictures = new()
            {
                new PictureConfig(1, "pic1", "Pic 1", "Textures/pic1", "pic.1.name", "pic.1.desc", true, "sequential", new List<int>())
            };

            public List<ItemDto> Items = new();

            public IReadOnlyList<PictureConfig> GetAllPictures() => Pictures;
            public PictureConfig GetPictureById(int id) => Pictures[0];

            public PictureDifficultyConfig GetPictureDifficulty(int pictureId, int difficultyId)
            {
                return new PictureDifficultyConfig(1, 0, "Easy", 2, 2, 1, 10, 0, 5, new List<int>());
            }

            public ItemDto GetItemById(int id) => null;
            public IReadOnlyList<ItemDto> GetAllItems() => Items;
            public IReadOnlyList<PictureDifficultyConfig> GetPictureDifficulties(int pictureId) => new List<PictureDifficultyConfig>();
            public IReadOnlyList<PictureDifficultyConfig> GetAllPictureDifficulties() => new List<PictureDifficultyConfig>();
            public IReadOnlyList<DropTableConfig> GetAllDropTables() => new List<DropTableConfig>();
            public IReadOnlyList<DropTableItemConfig> GetDropTableItems(int dropTableId) => new List<DropTableItemConfig>();
            public IReadOnlyList<DropTableItemConfig> GetAllDropTableItems() => new List<DropTableItemConfig>();
            public IReadOnlyList<DailyRewardConfig> GetDailyRewards() => new List<DailyRewardConfig>();
        }

        private class MockLocalizationService : ILocalizationService
        {
            public event Action OnLanguageChanged;
            public string CurrentLanguage => "vi";
            public void SetLanguage(string langCode) { OnLanguageChanged?.Invoke(); }
            public string Get(string key) => key;
            public string GetFormat(string key, params object[] args) => key;
        }

        private class CountingLocalizationService : ILocalizationService
        {
            private Action _onLanguageChanged;

            public int AddCount { get; private set; }
            public int RemoveCount { get; private set; }
            public string CurrentLanguage => "vi";

            public event Action OnLanguageChanged
            {
                add
                {
                    AddCount++;
                    _onLanguageChanged += value;
                }
                remove
                {
                    RemoveCount++;
                    _onLanguageChanged -= value;
                }
            }

            public void SetLanguage(string langCode) => _onLanguageChanged?.Invoke();
            public string Get(string key) => key;
            public string GetFormat(string key, params object[] args) => key;
        }

        private class MockAudioService : IAudioService
        {
            public void PlayBGM(string clipPath, bool loop = true, float fadeDuration = 0.5f) {}
            public void StopBGM(float fadeDuration = 0.5f) {}
            public void PlaySFX(string clipPath, float volumeScale = 1f) {}
            public void SetMusicEnabled(bool enabled) {}
            public void SetSfxEnabled(bool enabled) {}
        }
    }
}
