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
    public class PictureSelectFlowTests
    {
        private GameObject _holder;
        private PictureSelectView _view;
        private PictureSelectCard _cardPrefab;
        private RectTransform _container;

        [SetUp]
        public void SetUp()
        {
            _holder = new GameObject("TestHolder");
            _view = _holder.AddComponent<PictureSelectView>();

            var prefabGo = new GameObject("CardPrefab");
            prefabGo.SetActive(false);
            _cardPrefab = prefabGo.AddComponent<PictureSelectCard>();

            var button = CreateButton(prefabGo.transform, "Button");
            var displayNameText = CreateText(prefabGo.transform, "DisplayName");

            var lockOverlay = new GameObject("LockOverlay");
            lockOverlay.transform.SetParent(prefabGo.transform);
            var hintText = CreateText(lockOverlay.transform, "MissingItemsHint");
            var unlockButton = CreateButton(lockOverlay.transform, "UnlockButton");

            var cardSo = new UnityEditor.SerializedObject(_cardPrefab);
            cardSo.FindProperty("_button").objectReferenceValue = button;
            cardSo.FindProperty("_displayNameText").objectReferenceValue = displayNameText;
            cardSo.FindProperty("_lockOverlay").objectReferenceValue = lockOverlay;
            cardSo.FindProperty("_missingItemsHintText").objectReferenceValue = hintText;
            cardSo.FindProperty("_unlockButton").objectReferenceValue = unlockButton;
            cardSo.ApplyModifiedProperties();

            var containerGo = new GameObject("Container", typeof(RectTransform));
            containerGo.transform.SetParent(_holder.transform);
            _container = (RectTransform)containerGo.transform;

            var viewSo = new UnityEditor.SerializedObject(_view);
            viewSo.FindProperty("_cardPrefab").objectReferenceValue = _cardPrefab;
            viewSo.FindProperty("_contentContainer").objectReferenceValue = _container;
            viewSo.ApplyModifiedProperties();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_holder);
            if (_cardPrefab != null)
            {
                UnityEngine.Object.DestroyImmediate(_cardPrefab.gameObject);
            }
        }

        [Test]
        public void Card_ExposesProgressionBindContract()
        {
            var bindMethod = typeof(PictureSelectCard).GetMethod(
                "Bind",
                new[]
                {
                    typeof(PictureCardPresentationModel),
                    typeof(Action<int>),
                    typeof(Action<int>)
                });

            Assert.IsNotNull(bindMethod);
        }

        [Test]
        public void Setup_SpawnsCorrectNumberOfCards()
        {
            _view.Setup(new List<PictureCardPresentationModel>
            {
                CreateModel(1),
                CreateModel(2)
            });

            Assert.AreEqual(2, _view.InstantiatedCards.Count);
        }

        [UnityEngine.TestTools.UnityTest]
        public System.Collections.IEnumerator Setup_Twice_ClearsExistingCards_AndDestroysGameObjects()
        {
            _view.Setup(new List<PictureCardPresentationModel> { CreateModel(1) });
            Assert.AreEqual(1, _view.InstantiatedCards.Count);

            _view.Setup(new List<PictureCardPresentationModel>
            {
                CreateModel(2),
                CreateModel(3)
            });
            Assert.AreEqual(2, _view.InstantiatedCards.Count);

            yield return null;

            Assert.AreEqual(2, _container.childCount);
        }

        [Test]
        public void Card_Unlocked_EnablesSelectionAndHidesLockUi()
        {
            _view.Setup(new List<PictureCardPresentationModel>
            {
                CreateModel(3, PictureCardState.Unlocked)
            });

            var card = _view.InstantiatedCards[0];
            TriggerCardAwake(card);
            int selectedId = 0;
            _view.OnPictureSelected += id => selectedId = id;

            Assert.IsTrue(GetField<Button>(card, "_button").interactable);
            Assert.IsFalse(GetField<GameObject>(card, "_lockOverlay").activeSelf);
            Assert.IsFalse(GetField<Button>(card, "_unlockButton").gameObject.activeSelf);

            GetField<Button>(card, "_button").onClick.Invoke();
            Assert.AreEqual(3, selectedId);
        }

        [Test]
        public void Card_Locked_DisablesSelectionAndShowsMissingItemHint()
        {
            const string hint = "Missing: Key 101. Source: Pic 1 - Easy";
            _view.Setup(new List<PictureCardPresentationModel>
            {
                CreateModel(2, PictureCardState.Locked, hint)
            });

            var card = _view.InstantiatedCards[0];
            Assert.IsFalse(GetField<Button>(card, "_button").interactable);
            Assert.IsTrue(GetField<GameObject>(card, "_lockOverlay").activeSelf);
            Assert.IsFalse(GetField<Button>(card, "_unlockButton").gameObject.activeSelf);
            Assert.AreEqual(hint, GetField<TMP_Text>(card, "_missingItemsHintText").text);
        }

        [Test]
        public void Card_ReadyToUnlock_ShowsUnlockButtonAndRaisesRequest()
        {
            _view.Setup(new List<PictureCardPresentationModel>
            {
                CreateModel(2, PictureCardState.ReadyToUnlock)
            });

            int unlockId = 0;
            _view.OnPictureUnlockRequested += id => unlockId = id;

            var card = _view.InstantiatedCards[0];
            TriggerCardAwake(card);
            var unlockButton = GetField<Button>(card, "_unlockButton");
            Assert.IsFalse(GetField<Button>(card, "_button").interactable);
            Assert.IsTrue(GetField<GameObject>(card, "_lockOverlay").activeSelf);
            Assert.IsTrue(unlockButton.gameObject.activeSelf);
            Assert.IsTrue(unlockButton.interactable);

            unlockButton.onClick.Invoke();
            Assert.AreEqual(2, unlockId);
        }

        [Test]
        public void Presenter_UnlockSuccess_RefreshesCardAsUnlocked()
        {
            var staticData = new MockStaticDataService();
            var saveData = new MockSaveDataService();
            saveData.SaveData.OwnedItemIds.Add(101);
            var progression = new ProgressionService(staticData, saveData);
            var session = new GameSessionService();
            var presenter = new PictureSelectPresenter(
                _view,
                session,
                staticData,
                saveData,
                progression);

            var lockedCard = _view.InstantiatedCards.Single(card =>
                card.gameObject.name.StartsWith("PictureCard_2_"));
            TriggerCardAwake(lockedCard);
            GetField<Button>(lockedCard, "_unlockButton").onClick.Invoke();

            Assert.Contains(2, saveData.SaveData.UnlockedPictureIds);
            var refreshedCard = _view.InstantiatedCards.Single(card =>
                card.gameObject.name.StartsWith("PictureCard_2_"));
            Assert.IsTrue(GetField<Button>(refreshedCard, "_button").interactable);
            Assert.IsFalse(GetField<GameObject>(refreshedCard, "_lockOverlay").activeSelf);

            presenter.Dispose();
        }

        [Test]
        public void Presenter_BuildsMissingItemSourceHint()
        {
            var staticData = new MockStaticDataService();
            var saveData = new MockSaveDataService();
            var progression = new ProgressionService(staticData, saveData);
            var presenter = new PictureSelectPresenter(
                _view,
                new GameSessionService(),
                staticData,
                saveData,
                progression);

            var lockedCard = _view.InstantiatedCards.Single(card =>
                card.gameObject.name.StartsWith("PictureCard_2_"));
            string hint = GetField<TMP_Text>(lockedCard, "_missingItemsHintText").text;

            StringAssert.Contains("Key 101", hint);
            StringAssert.Contains("Pic 1 - Easy", hint);
            presenter.Dispose();
        }

        [Test]
        public void Presenter_OnDispose_UnsubscribesFromViewEvents()
        {
            var staticData = new MockStaticDataService();
            var saveData = new MockSaveDataService();
            saveData.SaveData.OwnedItemIds.Add(101);
            var session = new GameSessionService();
            var presenter = new PictureSelectPresenter(
                _view,
                session,
                staticData,
                saveData,
                new ProgressionService(staticData, saveData));

            presenter.Dispose();

            var pictureOne = _view.InstantiatedCards.Single(card =>
                card.gameObject.name.StartsWith("PictureCard_1_"));
            TriggerCardAwake(pictureOne);
            GetField<Button>(pictureOne, "_button").onClick.Invoke();

            var pictureTwo = _view.InstantiatedCards.Single(card =>
                card.gameObject.name.StartsWith("PictureCard_2_"));
            TriggerCardAwake(pictureTwo);
            GetField<Button>(pictureTwo, "_unlockButton").onClick.Invoke();

            Assert.AreEqual(0, session.SelectedPictureId);
            Assert.IsFalse(saveData.SaveData.UnlockedPictureIds.Contains(2));
        }

        [Test]
        public void FlowController_OnDispose_UnsubscribesFromView()
        {
            var difficultyViewObject = new GameObject("DifficultyView");
            var difficultyView = difficultyViewObject.AddComponent<DifficultySelectView>();
            var backButton = CreateButton(difficultyViewObject.transform, "BackButton");
            var difficultySo = new UnityEditor.SerializedObject(difficultyView);
            difficultySo.FindProperty("_backButton").objectReferenceValue = backButton;
            difficultySo.ApplyModifiedProperties();

            var controller = new HomeFlowController(_view, difficultyView, null, null);
            controller.Start();
            controller.Dispose();

            _view.SetActive(true);
            difficultyView.SetActive(false);
            _view.Setup(new List<PictureCardPresentationModel> { CreateModel(5) });
            TriggerCardAwake(_view.InstantiatedCards[0]);
            GetField<Button>(_view.InstantiatedCards[0], "_button").onClick.Invoke();

            Assert.IsTrue(_view.gameObject.activeSelf);
            Assert.IsFalse(difficultyView.gameObject.activeSelf);

            _view.SetActive(false);
            difficultyView.SetActive(true);
            backButton.onClick.Invoke();

            Assert.IsFalse(_view.gameObject.activeSelf);
            Assert.IsTrue(difficultyView.gameObject.activeSelf);
            UnityEngine.Object.DestroyImmediate(difficultyViewObject);
        }

        private static Button CreateButton(Transform parent, string name)
        {
            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent);
            return gameObject.AddComponent<Button>();
        }

        private static void TriggerCardAwake(PictureSelectCard card)
        {
            var method = typeof(PictureSelectCard).GetMethod(
                "Awake",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic);
            method?.Invoke(card, null);
        }

        private static TMP_Text CreateText(Transform parent, string name)
        {
            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent);
            return gameObject.AddComponent<TextMeshProUGUI>();
        }

        private static PictureCardPresentationModel CreateModel(
            int id,
            PictureCardState state = PictureCardState.Unlocked,
            string hint = "")
        {
            return new PictureCardPresentationModel
            {
                Config = new PictureConfig(
                    id,
                    $"pic{id}",
                    $"Pic {id}",
                    $"Textures/pic{id}",
                    "key.name",
                    "key.desc",
                    state == PictureCardState.Unlocked || state == PictureCardState.Completed,
                    "sequential",
                    new List<int>()),
                State = state,
                MissingItemsHint = hint
            };
        }

        private static T GetField<T>(PictureSelectCard card, string fieldName)
            where T : UnityEngine.Object
        {
            var serializedObject = new UnityEditor.SerializedObject(card);
            return (T)serializedObject.FindProperty(fieldName).objectReferenceValue;
        }

        private class MockStaticDataService : IStaticDataService
        {
            private readonly List<PictureConfig> _pictures = new()
            {
                new PictureConfig(
                    1, "pic1", "Pic 1", "Textures/pic1", "key.name", "key.desc",
                    true, "sequential", new List<int>()),
                new PictureConfig(
                    2, "pic2", "Pic 2", "Textures/pic2", "key.name", "key.desc",
                    false, "sequential", new List<int> { 101 })
            };

            private readonly List<PictureDifficultyConfig> _difficulties = new()
            {
                new PictureDifficultyConfig(
                    1, 0, "Easy", 6, 4, 1, 30, 0, 10, new List<int> { 101 }),
                new PictureDifficultyConfig(
                    2, 0, "Easy", 6, 4, 1, 30, 0, 10, new List<int>())
            };

            public IReadOnlyList<PictureConfig> GetAllPictures() => _pictures;

            public PictureConfig GetPictureById(int id) =>
                _pictures.FirstOrDefault(picture => picture.Id == id);

            public PictureDifficultyConfig GetPictureDifficulty(int pictureId, int difficultyId) =>
                _difficulties.First(difficulty =>
                    difficulty.PictureId == pictureId &&
                    difficulty.DifficultyId == difficultyId);

            public ItemDto GetItemById(int id) => id == 101
                ? new ItemDto { id = 101, display_name = "Key 101" }
                : null;

            public IReadOnlyList<ItemDto> GetAllItems() => new List<ItemDto>();

            public IReadOnlyList<PictureDifficultyConfig> GetPictureDifficulties(int pictureId) =>
                _difficulties.Where(difficulty => difficulty.PictureId == pictureId).ToList();

            public IReadOnlyList<PictureDifficultyConfig> GetAllPictureDifficulties() =>
                _difficulties;
        }
    }
}
