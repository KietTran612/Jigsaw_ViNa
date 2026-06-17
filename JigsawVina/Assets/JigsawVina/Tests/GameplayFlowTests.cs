using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using JigsawVina.Core.Data;
using JigsawVina.Core.Services;
using JigsawVina.Presentation.Screens;

namespace JigsawVina.Tests
{
    public class GameplayFlowTests
    {
        private GameObject _viewGo;
        private PuzzlePlayingView _view;
        private GameObject _rewardGo;
        private RewardSummaryView _rewardView;
        
        private GameSessionService _sessionService;
        private MockSaveDataService _saveService;
        private MockStaticDataService _staticData;
        private MockLocalizationService _localization;
        private MockAudioService _audio;
        private MockSceneLoader _sceneLoader;

        private PuzzlePlayingPresenter _presenter;
        private RewardSummaryPresenter _rewardPresenter;
        private GameplayFlowController _flowController;

        [SetUp]
        public void SetUp()
        {
            _viewGo = new GameObject("GameplayView");
            _view = _viewGo.AddComponent<PuzzlePlayingView>();
            BuildMinimalView(_viewGo.transform);
            
            _rewardGo = new GameObject("RewardView");
            _rewardView = _rewardGo.AddComponent<RewardSummaryView>();

            _sessionService = new GameSessionService();
            _saveService = new MockSaveDataService();
            _staticData = new MockStaticDataService();
            _localization = new MockLocalizationService();
            _audio = new MockAudioService();
            _sceneLoader = new MockSceneLoader();

            _presenter = new PuzzlePlayingPresenter(_view, _sessionService, _staticData, _saveService, _localization, _audio);
            _rewardPresenter = new RewardSummaryPresenter(_rewardView, _sessionService, _saveService, _staticData, new MockDailyDropService());

            _flowController = new GameplayFlowController(
                _view,
                _rewardView,
                _presenter,
                _rewardPresenter,
                _sceneLoader);
        }

        [TearDown]
        public void TearDown()
        {
            _flowController.Dispose();
            _presenter.Cleanup();
            UnityEngine.Object.DestroyImmediate(_viewGo);
            UnityEngine.Object.DestroyImmediate(_rewardGo);
        }

        [Test]
        public void FlowController_WhenActive_HandlesBackRequest()
        {
            _flowController.Start();
            _sceneLoader.LoadSceneCalled = false;

            var backDelegate = typeof(PuzzlePlayingPresenter).GetField("OnBackRequested", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var action = (Action)backDelegate.GetValue(_presenter);
            action.Invoke();

            Assert.IsTrue(_sceneLoader.LoadSceneCalled);
            Assert.AreEqual("Home", _sceneLoader.LoadedSceneName);
        }

        [Test]
        public void FlowController_AfterDispose_DoesNotHandleEvents()
        {
            _flowController.Start();
            _flowController.Dispose();
            _sceneLoader.LoadSceneCalled = false;

            var backDelegate = typeof(PuzzlePlayingPresenter).GetField("OnBackRequested", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var action = (Action)backDelegate.GetValue(_presenter);
            
            if (action != null)
            {
                action.Invoke();
            }

            Assert.IsFalse(_sceneLoader.LoadSceneCalled);
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
            private readonly PictureConfig _picture = new(1, "pic1", "Pic 1", "Textures/pic1", "pic.1.name", "pic.1.desc", true, "sequential", new List<int>());
            private readonly PictureDifficultyConfig _difficulty = new(1, 0, "Easy", 2, 2, 1, 10, 0, 5, new List<int>());

            public IReadOnlyList<PictureConfig> GetAllPictures() => new List<PictureConfig> { _picture };
            public PictureConfig GetPictureById(int id) => _picture;
            public PictureDifficultyConfig GetPictureDifficulty(int pictureId, int difficultyId) => _difficulty;
            public ItemDto GetItemById(int id) => null;
            public IReadOnlyList<ItemDto> GetAllItems() => new List<ItemDto>();
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
            public void SetLanguage(string langCode) {}
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

        private class MockSceneLoader : SceneLoader
        {
            public bool LoadSceneCalled;
            public string LoadedSceneName;

            public override Cysharp.Threading.Tasks.UniTask LoadSceneAsync(string sceneName)
            {
                LoadSceneCalled = true;
                LoadedSceneName = sceneName;
                return Cysharp.Threading.Tasks.UniTask.CompletedTask;
            }
        }

        private class MockDailyDropService : IDropRewardService
        {
            public List<DropRewardResult> Results = new();
            public List<DropRewardResult> RollDropRewards(int dropTableId, PlayerSave save) => Results;
        }
    }
}
