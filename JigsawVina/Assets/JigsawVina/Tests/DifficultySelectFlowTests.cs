using System;
using System.Collections.Generic;
using System.Linq;
using JigsawVina.Core.Data;
using JigsawVina.Core.Services;
using JigsawVina.Presentation.Screens;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JigsawVina.Tests
{
    public class DifficultySelectFlowTests
    {
        private GameObject _holder;
        private DifficultySelectView _view;
        
        private Button _easyButton;
        private Button _normalButton;
        private Button _hardButton;
        private Button _backButton;
        
        private GameObject[] _lockIcons;
        private TMP_Text[] _achievementTexts;

        [SetUp]
        public void SetUp()
        {
            _holder = new GameObject("TestHolder");
            _view = _holder.AddComponent<DifficultySelectView>();

            _easyButton = CreateButton(_holder.transform, "EasyButton");
            _normalButton = CreateButton(_holder.transform, "NormalButton");
            _hardButton = CreateButton(_holder.transform, "HardButton");
            _backButton = CreateButton(_holder.transform, "BackButton");

            _lockIcons = new GameObject[3];
            _lockIcons[0] = new GameObject("Lock0");
            _lockIcons[0].transform.SetParent(_holder.transform);
            _lockIcons[1] = new GameObject("Lock1");
            _lockIcons[1].transform.SetParent(_holder.transform);
            _lockIcons[2] = new GameObject("Lock2");
            _lockIcons[2].transform.SetParent(_holder.transform);

            _achievementTexts = new TMP_Text[3];
            _achievementTexts[0] = CreateText(_holder.transform, "Achievement0");
            _achievementTexts[1] = CreateText(_holder.transform, "Achievement1");
            _achievementTexts[2] = CreateText(_holder.transform, "Achievement2");

            var viewSo = new UnityEditor.SerializedObject(_view);
            viewSo.FindProperty("_easyButton").objectReferenceValue = _easyButton;
            viewSo.FindProperty("_normalButton").objectReferenceValue = _normalButton;
            viewSo.FindProperty("_hardButton").objectReferenceValue = _hardButton;
            viewSo.FindProperty("_backButton").objectReferenceValue = _backButton;

            var lockIconsProperty = viewSo.FindProperty("_lockIcons");
            lockIconsProperty.arraySize = 3;
            for (int i = 0; i < 3; i++)
            {
                lockIconsProperty.GetArrayElementAtIndex(i).objectReferenceValue = _lockIcons[i];
            }

            var achievementTextsProperty = viewSo.FindProperty("_achievementTexts");
            achievementTextsProperty.arraySize = 3;
            for (int i = 0; i < 3; i++)
            {
                achievementTextsProperty.GetArrayElementAtIndex(i).objectReferenceValue = _achievementTexts[i];
            }

            viewSo.ApplyModifiedProperties();
            TriggerViewAwake(_view);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_holder);
        }

        [Test]
        public void View_ExposesRequiredElements()
        {
            Assert.AreEqual(_lockIcons, _view.LockIcons);
            Assert.AreEqual(_achievementTexts, _view.AchievementTexts);
            Assert.AreEqual(_backButton, _view.BackButton);
        }

        [Test]
        public void Presenter_Refresh_UpdatesUIAccordingToProgression()
        {
            var staticData = new MockStaticDataService();
            var saveData = new MockSaveDataService();
            var progression = new ProgressionService(staticData, saveData);
            var session = new GameSessionService();
            var sceneLoader = new MockSceneLoader();

            var presenter = new DifficultySelectPresenter(
                _view,
                session,
                sceneLoader,
                progression,
                saveData,
                staticData);

            // Setup: Picture 1 has sequential policy. Save has completion on difficulty 0 (Easy) with 1 star, 15 seconds.
            saveData.SaveData.CompletedPuzzles.Add(new CompletedPuzzleData
            {
                PictureId = 1,
                DifficultyId = 0,
                BestStar = 1,
                BestTimeSeconds = 15f
            });

            presenter.Refresh(1);

            // Easy should be unlocked (difficultyId = 0)
            Assert.IsTrue(_easyButton.interactable);
            Assert.IsFalse(_lockIcons[0].activeSelf);
            Assert.AreEqual("Best Star: 1/1\nBest Time: 15.0s", _achievementTexts[0].text);

            // Normal should be unlocked (since Easy is completed with >0 star)
            Assert.IsTrue(_normalButton.interactable);
            Assert.IsFalse(_lockIcons[1].activeSelf);
            Assert.AreEqual("Best Star: 0/2\nBest Time: --", _achievementTexts[1].text);

            // Hard should be locked (since Normal is not completed yet)
            Assert.IsFalse(_hardButton.interactable);
            Assert.IsTrue(_lockIcons[2].activeSelf);
            Assert.AreEqual("Best Star: 0/3\nBest Time: --", _achievementTexts[2].text);

            presenter.Dispose();
        }

        [Test]
        public void Presenter_HandleDifficultySelected_LoadsSceneIfUnlocked()
        {
            var staticData = new MockStaticDataService();
            var saveData = new MockSaveDataService();
            var progression = new ProgressionService(staticData, saveData);
            var session = new GameSessionService();
            var sceneLoader = new MockSceneLoader();

            var presenter = new DifficultySelectPresenter(
                _view,
                session,
                sceneLoader,
                progression,
                saveData,
                staticData);

            presenter.Refresh(1); // Easy is always unlocked for picture 1

            _easyButton.onClick.Invoke();

            Assert.AreEqual(0, session.SelectedDifficultyId); // Easy has difficultyId = 0
            Assert.AreEqual("Gameplay", sceneLoader.LoadedSceneName);

            presenter.Dispose();
        }

        [Test]
        public void Presenter_HandleDifficultySelected_DoesNotLoadSceneIfLocked()
        {
            var staticData = new MockStaticDataService();
            var saveData = new MockSaveDataService();
            var progression = new ProgressionService(staticData, saveData);
            var session = new GameSessionService();
            var sceneLoader = new MockSceneLoader();

            var presenter = new DifficultySelectPresenter(
                _view,
                session,
                sceneLoader,
                progression,
                saveData,
                staticData);

            presenter.Refresh(1); // Normal is locked initially

            _normalButton.onClick.Invoke();

            Assert.AreEqual(0, session.SelectedDifficultyId);
            Assert.IsNull(sceneLoader.LoadedSceneName);

            presenter.Dispose();
        }

        [Test]
        public void Presenter_OnDispose_UnsubscribesFromViewEvents()
        {
            var staticData = new MockStaticDataService();
            var saveData = new MockSaveDataService();
            var progression = new ProgressionService(staticData, saveData);
            var session = new GameSessionService();
            var sceneLoader = new MockSceneLoader();

            var presenter = new DifficultySelectPresenter(
                _view,
                session,
                sceneLoader,
                progression,
                saveData,
                staticData);

            presenter.Refresh(1);
            presenter.Dispose();

            _easyButton.onClick.Invoke();

            Assert.IsNull(sceneLoader.LoadedSceneName);
        }

        [Test]
        public void HomeFlowController_RefreshesPresenter_OnPictureSelected()
        {
            var selectViewGo = new GameObject("PictureSelectView");
            var selectView = selectViewGo.AddComponent<PictureSelectView>();

            var cardPrefabGo = new GameObject("CardPrefab");
            cardPrefabGo.SetActive(false);
            var cardPrefab = cardPrefabGo.AddComponent<PictureSelectCard>();
            var btn = CreateButton(cardPrefabGo.transform, "Button");
            var text = CreateText(cardPrefabGo.transform, "Text");

            var cardSo = new UnityEditor.SerializedObject(cardPrefab);
            cardSo.FindProperty("_button").objectReferenceValue = btn;
            cardSo.FindProperty("_displayNameText").objectReferenceValue = text;
            cardSo.ApplyModifiedProperties();

            var containerGo = new GameObject("Container", typeof(RectTransform));
            containerGo.transform.SetParent(selectViewGo.transform);
            var container = (RectTransform)containerGo.transform;

            var viewSo = new UnityEditor.SerializedObject(selectView);
            viewSo.FindProperty("_cardPrefab").objectReferenceValue = cardPrefab;
            viewSo.FindProperty("_contentContainer").objectReferenceValue = container;
            viewSo.ApplyModifiedProperties();
            
            var staticData = new MockStaticDataService();
            var saveData = new MockSaveDataService();
            var progression = new ProgressionService(staticData, saveData);
            var session = new GameSessionService();
            var sceneLoader = new MockSceneLoader();

            var presenter = new DifficultySelectPresenter(
                _view,
                session,
                sceneLoader,
                progression,
                saveData,
                staticData);

            var controller = new HomeFlowController(selectView, _view, null, presenter);
            controller.Start();

            // Setup selectView with one picture card
            selectView.Setup(new List<PictureCardPresentationModel>
            {
                new PictureCardPresentationModel
                {
                    Config = new PictureConfig(1, "pic1", "Pic 1", "Textures/pic1", "key.name", "key.desc", true, "sequential", new List<int>()),
                    State = PictureCardState.Unlocked,
                    MissingItemsHint = ""
                }
            });

            // Trigger click on the picture card to select it
            var card = selectView.InstantiatedCards[0];
            TriggerCardAwake(card);
            
            var selectButton = GetField<Button>(card, "_button");
            selectButton.onClick.Invoke();

            // Difficulty select should be refreshed (Easy is unlocked, lock icon 0 active status is false)
            Assert.IsTrue(_easyButton.interactable);
            Assert.IsFalse(_lockIcons[0].activeSelf);

            controller.Dispose();
            presenter.Dispose();
            UnityEngine.Object.DestroyImmediate(selectViewGo);
            UnityEngine.Object.DestroyImmediate(cardPrefabGo);
        }

        private static Button CreateButton(Transform parent, string name)
        {
            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent);
            return gameObject.AddComponent<Button>();
        }

        private static TMP_Text CreateText(Transform parent, string name)
        {
            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent);
            return gameObject.AddComponent<TextMeshProUGUI>();
        }

        private static void TriggerViewAwake(DifficultySelectView view)
        {
            var method = typeof(DifficultySelectView).GetMethod(
                "Awake",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic);
            method?.Invoke(view, null);
        }

        private static void TriggerCardAwake(PictureSelectCard card)
        {
            var method = typeof(PictureSelectCard).GetMethod(
                "Awake",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic);
            method?.Invoke(card, null);
        }

        private static T GetField<T>(PictureSelectCard card, string fieldName)
            where T : UnityEngine.Object
        {
            var serializedObject = new UnityEditor.SerializedObject(card);
            return (T)serializedObject.FindProperty(fieldName).objectReferenceValue;
        }

        private class MockSceneLoader : SceneLoader
        {
            public string LoadedSceneName { get; private set; }

            public override Cysharp.Threading.Tasks.UniTask LoadSceneAsync(string sceneName)
            {
                LoadedSceneName = sceneName;
                return Cysharp.Threading.Tasks.UniTask.CompletedTask;
            }
        }

        private class MockStaticDataService : IStaticDataService
        {
            private readonly List<PictureConfig> _pictures = new()
            {
                new PictureConfig(
                    1, "pic1", "Pic 1", "Textures/pic1", "key.name", "key.desc",
                    true, "sequential", new List<int>())
            };

            private readonly List<PictureDifficultyConfig> _difficulties = new()
            {
                new PictureDifficultyConfig(1, 0, "Easy", 6, 4, 1, 30, 0, 10, new List<int>()),
                new PictureDifficultyConfig(1, 1, "Normal", 8, 6, 2, 60, 0, 20, new List<int>()),
                new PictureDifficultyConfig(1, 2, "Hard", 12, 8, 3, 120, 0, 40, new List<int>())
            };

            public IReadOnlyList<PictureConfig> GetAllPictures() => _pictures;

            public PictureConfig GetPictureById(int id) =>
                _pictures.FirstOrDefault(picture => picture.Id == id);

            public PictureDifficultyConfig GetPictureDifficulty(int pictureId, int difficultyId) =>
                _difficulties.First(difficulty =>
                    difficulty.PictureId == pictureId &&
                    difficulty.DifficultyId == difficultyId);

            public ItemDto GetItemById(int id) => null;

            public IReadOnlyList<ItemDto> GetAllItems() => new List<ItemDto>();

            public IReadOnlyList<PictureDifficultyConfig> GetPictureDifficulties(int pictureId) =>
                _difficulties.Where(difficulty => difficulty.PictureId == pictureId).ToList();

            public IReadOnlyList<PictureDifficultyConfig> GetAllPictureDifficulties() =>
                _difficulties;
        }
    }
}
