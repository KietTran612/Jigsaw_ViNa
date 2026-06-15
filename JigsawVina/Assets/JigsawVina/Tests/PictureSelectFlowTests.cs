using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using JigsawVina.Presentation.Screens;
using JigsawVina.Core.Data;
using JigsawVina.Core.Services;
using System.Collections.Generic;

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
            _cardPrefab = prefabGo.AddComponent<PictureSelectCard>();

            var btnGo = new GameObject("Button");
            btnGo.transform.SetParent(prefabGo.transform);
            var btn = btnGo.AddComponent<Button>();

            var txtGo = new GameObject("Text");
            txtGo.transform.SetParent(prefabGo.transform);
            txtGo.AddComponent<TMPro.TextMeshProUGUI>();

            // Lưu ý: Không gán _thumbnailImage để tránh trigger Resources.Load trong test
            var cardSO = new UnityEditor.SerializedObject(_cardPrefab);
            cardSO.FindProperty("_button").objectReferenceValue = btn;
            cardSO.FindProperty("_displayNameText").objectReferenceValue = txtGo.GetComponent<TMPro.TextMeshProUGUI>();
            cardSO.ApplyModifiedProperties();

            var containerGo = new GameObject("Container", typeof(RectTransform));
            containerGo.transform.SetParent(_holder.transform);
            _container = (RectTransform)containerGo.transform;

            var viewSO = new UnityEditor.SerializedObject(_view);
            viewSO.FindProperty("_cardPrefab").objectReferenceValue = _cardPrefab;
            viewSO.FindProperty("_contentContainer").objectReferenceValue = _container;
            viewSO.ApplyModifiedProperties();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_holder);
            if (_cardPrefab != null)
            {
                Object.DestroyImmediate(_cardPrefab.gameObject);
            }
        }

        private void TriggerCardAwake(PictureSelectCard card)
        {
            var method = typeof(PictureSelectCard).GetMethod("Awake", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            method?.Invoke(card, null);
        }

        [Test]
        public void Setup_SpawnsCorrectNumberOfCards()
        {
            var configs = new List<PictureConfig>
            {
                new PictureConfig(1, "pic1", "Pic 1", "Textures/pic1", "key.name", "key.desc"),
                new PictureConfig(2, "pic2", "Pic 2", "Textures/pic2", "key.name", "key.desc")
            };

            _view.Setup(configs);

            Assert.AreEqual(2, _view.InstantiatedCards.Count);
        }

        [UnityEngine.TestTools.UnityTest]
        public System.Collections.IEnumerator Setup_Twice_ClearsExistingCards_AndDestroysGameObjects()
        {
            var configs1 = new List<PictureConfig>
            {
                new PictureConfig(1, "pic1", "Pic 1", "Textures/pic1", "key.name", "key.desc")
            };
            var configs2 = new List<PictureConfig>
            {
                new PictureConfig(2, "pic2", "Pic 2", "Textures/pic2", "key.name", "key.desc"),
                new PictureConfig(3, "pic3", "Pic 3", "Textures/pic3", "key.name", "key.desc")
            };

            _view.Setup(configs1);
            Assert.AreEqual(1, _view.InstantiatedCards.Count);

            _view.Setup(configs2);
            Assert.AreEqual(2, _view.InstantiatedCards.Count);

            yield return null; // Đợi một frame để Unity Destroy() thực thi xong

            Assert.AreEqual(2, _container.childCount);
        }

        [Test]
        public void CardClick_InvokesOnPictureSelectedWithCorrectId()
        {
            var configs = new List<PictureConfig>
            {
                new PictureConfig(3, "pic3", "Pic 3", "Textures/pic3", "key.name", "key.desc")
            };
            _view.Setup(configs);
            TriggerCardAwake(_view.InstantiatedCards[0]);

            int selectedId = 0;
            _view.OnPictureSelected += id => selectedId = id;

            var cardSO = new UnityEditor.SerializedObject(_view.InstantiatedCards[0]);
            var btn = (Button)cardSO.FindProperty("_button").objectReferenceValue;
            btn.onClick.Invoke();

            Assert.AreEqual(3, selectedId);
        }

        [Test]
        public void Presenter_OnDispose_UnsubscribesFromView()
        {
            var staticData = new MockStaticDataService();
            var session = new GameSessionService();
            var presenter = new PictureSelectPresenter(_view, session, staticData);

            presenter.Dispose();

            int selectedId = 0;
            _view.OnPictureSelected += id => selectedId = id;

            var configs = new List<PictureConfig> { new PictureConfig(5, "pic5", "Pic 5", "Textures/pic5", "key.name", "key.desc") };
            _view.Setup(configs);
            TriggerCardAwake(_view.InstantiatedCards[0]);

            var cardSO = new UnityEditor.SerializedObject(_view.InstantiatedCards[0]);
            var btn = (Button)cardSO.FindProperty("_button").objectReferenceValue;
            btn.onClick.Invoke();

            // Session ID should remain default (0) because presenter is disposed
            Assert.AreEqual(0, session.SelectedPictureId);
            Assert.AreEqual(5, selectedId); // Direct view subscription still works
        }

        [Test]
        public void FlowController_OnDispose_UnsubscribesFromView()
        {
            var mockDiffViewGo = new GameObject("DiffView");
            var diffView = mockDiffViewGo.AddComponent<DifficultySelectView>();
            var staticData = new MockStaticDataService();
            var session = new GameSessionService();
            var presenter = new PictureSelectPresenter(_view, session, staticData);

            // Wire mock BackButton to verify cleanup
            var backBtnGo = new GameObject("BackButton");
            backBtnGo.transform.SetParent(mockDiffViewGo.transform);
            var backBtn = backBtnGo.AddComponent<Button>();

            var diffViewSO = new UnityEditor.SerializedObject(diffView);
            diffViewSO.FindProperty("_backButton").objectReferenceValue = backBtn;
            diffViewSO.ApplyModifiedProperties();

            // Pass null for presenters since they aren't called by start/dispose flow
            var controller = new HomeFlowController(_view, diffView, null, null);
            controller.Start();
            controller.Dispose();

            // Selection event must no longer switch from picture view to difficulty view.
            _view.SetActive(true);
            diffView.SetActive(false);

            var configs = new List<PictureConfig>
            {
                new PictureConfig(5, "pic5", "Pic 5", "Textures/pic5", "key.name", "key.desc")
            };
            _view.Setup(configs);
            TriggerCardAwake(_view.InstantiatedCards[0]);

            var cardSO = new UnityEditor.SerializedObject(_view.InstantiatedCards[0]);
            var btn = (Button)cardSO.FindProperty("_button").objectReferenceValue;
            btn.onClick.Invoke();

            Assert.IsTrue(_view.gameObject.activeSelf);
            Assert.IsFalse(diffView.gameObject.activeSelf);

            // Back button must also no longer switch from difficulty view to picture view.
            _view.SetActive(false);
            diffView.SetActive(true);

            backBtn.onClick.Invoke();

            Assert.IsFalse(_view.gameObject.activeSelf);
            Assert.IsTrue(diffView.gameObject.activeSelf);

            Object.DestroyImmediate(mockDiffViewGo);
            presenter.Dispose();
        }

        private class MockStaticDataService : IStaticDataService
        {
            public IReadOnlyList<PictureConfig> GetAllPictures() => new List<PictureConfig>
            {
                new PictureConfig(1, "pic1", "Pic 1", "Textures/pic1", "key.name", "key.desc")
            };
            public PictureConfig GetPictureById(int id) => default;
            public PictureDifficultyConfig GetPictureDifficulty(int pictureId, int difficultyId) => default;
            public ItemDto GetItemById(int id) => null;
            public IReadOnlyList<ItemDto> GetAllItems() => null;
        }
    }
}
