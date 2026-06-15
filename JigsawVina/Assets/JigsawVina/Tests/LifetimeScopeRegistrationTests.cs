using JigsawVina.Presentation.Screens;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using VContainer.Unity;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace JigsawVina.Tests
{
    public class LifetimeScopeRegistrationTests
    {
        [Test]
        public void HomeLifetimeScope_LivesInPresentationScreensNamespace()
        {
            Assert.That(typeof(HomeLifetimeScope).Namespace, Is.EqualTo("JigsawVina.Presentation.Screens"));
            Assert.That(typeof(LifetimeScope).IsAssignableFrom(typeof(HomeLifetimeScope)), Is.True);
        }

        [Test]
        public void GameplayLifetimeScope_LivesInPresentationScreensNamespace()
        {
            Assert.That(typeof(GameplayLifetimeScope).Namespace, Is.EqualTo("JigsawVina.Presentation.Screens"));
            Assert.That(typeof(LifetimeScope).IsAssignableFrom(typeof(GameplayLifetimeScope)), Is.True);
        }

        [Test]
        public void HomeScene_UsesPresentationScreensHomeLifetimeScope()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Home.unity", OpenSceneMode.Additive);
            try
            {
                var scope = FindInScene<HomeLifetimeScope>(scene.GetRootGameObjects());

                Assert.That(scope, Is.Not.Null);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void GameplayScene_UsesPresentationScreensGameplayLifetimeScope()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Gameplay.unity", OpenSceneMode.Additive);
            try
            {
                var scope = FindInScene<GameplayLifetimeScope>(scene.GetRootGameObjects());

                Assert.That(scope, Is.Not.Null);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void HomeScene_PictureSelectView_IsWiredCorrectly()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Home.unity", OpenSceneMode.Additive);
            try
            {
                var view = FindInScene<PictureSelectView>(scene.GetRootGameObjects());
                Assert.That(view, Is.Not.Null);

                var viewSO = new UnityEditor.SerializedObject(view);
                var cardPrefabProperty = viewSO.FindProperty("_cardPrefab");
                var contentContainerProperty = viewSO.FindProperty("_contentContainer");

                Assert.That(cardPrefabProperty.objectReferenceValue, Is.Not.Null, "PictureSelectView _cardPrefab should not be null.");
                Assert.That(contentContainerProperty.objectReferenceValue, Is.Not.Null, "PictureSelectView _contentContainer should not be null.");

                var cardPrefab = (PictureSelectCard)cardPrefabProperty.objectReferenceValue;
                var cardSO = new UnityEditor.SerializedObject(cardPrefab);
                Assert.That(cardSO.FindProperty("_button").objectReferenceValue, Is.Not.Null, "PictureSelectCard _button should not be null.");
                Assert.That(cardSO.FindProperty("_thumbnailImage").objectReferenceValue, Is.Not.Null, "PictureSelectCard _thumbnailImage should not be null.");
                Assert.That(cardSO.FindProperty("_displayNameText").objectReferenceValue, Is.Not.Null, "PictureSelectCard _displayNameText should not be null.");
                Assert.That(cardSO.FindProperty("_lockOverlay").objectReferenceValue, Is.Not.Null, "PictureSelectCard _lockOverlay should not be null.");
                Assert.That(cardSO.FindProperty("_missingItemsHintText").objectReferenceValue, Is.Not.Null, "PictureSelectCard _missingItemsHintText should not be null.");
                Assert.That(cardSO.FindProperty("_unlockButton").objectReferenceValue, Is.Not.Null, "PictureSelectCard _unlockButton should not be null.");

                // Đảm bảo không còn hai button hardcode trong scene
                foreach (var go in scene.GetRootGameObjects())
                {
                    var hoGuomBtn = FindGameObjectByName(go, "Ho GuomButton");
                    var haLongBtn = FindGameObjectByName(go, "Ha LongButton");
                    Assert.That(hoGuomBtn, Is.Null, "Ho GuomButton should not exist in Home scene.");
                    Assert.That(haLongBtn, Is.Null, "Ha LongButton should not exist in Home scene.");
                }

                // Xác nhận Scroll View duy nhất và wire đúng
                var scrollViews = view.GetComponentsInChildren<ScrollRect>(true);
                Assert.That(scrollViews.Length, Is.EqualTo(1), "Should have exactly one ScrollRect under PictureSelectScreen.");

                var scrollRect = scrollViews[0];
                Assert.That(scrollRect.gameObject.name, Is.EqualTo("PictureScrollView"));
                Assert.That(scrollRect.content, Is.Not.Null, "ScrollRect content should be wired.");
                Assert.That(scrollRect.viewport, Is.Not.Null, "ScrollRect viewport should be wired.");
                Assert.That(scrollRect.content.name, Is.EqualTo("Content"));
                Assert.That(scrollRect.viewport.name, Is.EqualTo("Viewport"));

                // Kiểm tra các thông số Layout
                var layoutGroup = scrollRect.content.GetComponent<VerticalLayoutGroup>();
                Assert.That(layoutGroup, Is.Not.Null, "Content should have a VerticalLayoutGroup component.");
                Assert.That(layoutGroup.spacing, Is.EqualTo(20f), "VerticalLayoutGroup spacing should be 20.");
                Assert.That(layoutGroup.childControlWidth, Is.False, "childControlWidth should be false.");
                Assert.That(layoutGroup.childControlHeight, Is.False, "childControlHeight should be false.");
                Assert.That(layoutGroup.childForceExpandWidth, Is.False, "childForceExpandWidth should be false.");
                Assert.That(layoutGroup.childForceExpandHeight, Is.False, "childForceExpandHeight should be false.");
                Assert.That(layoutGroup.childAlignment, Is.EqualTo(TextAnchor.UpperCenter), "childAlignment should be UpperCenter.");

                var sizeFitter = scrollRect.content.GetComponent<ContentSizeFitter>();
                Assert.That(sizeFitter, Is.Not.Null, "Content should have a ContentSizeFitter component.");
                Assert.That(sizeFitter.verticalFit, Is.EqualTo(ContentSizeFitter.FitMode.PreferredSize), "verticalFit should be PreferredSize.");

                // Xác minh card prefab không lớn hơn ScrollView viewport để tránh bị clipping
                var scrollViewRect = scrollRect.GetComponent<RectTransform>();
                var cardRect = cardPrefab.GetComponent<RectTransform>();
                Assert.That(cardRect.rect.width, Is.LessThanOrEqualTo(scrollViewRect.rect.width), "Card prefab width must be less than or equal to ScrollView width to prevent horizontal clipping.");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void HomeScene_DifficultySelectView_IsWiredCorrectly()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/Home.unity", OpenSceneMode.Additive);
            try
            {
                var view = FindInScene<DifficultySelectView>(scene.GetRootGameObjects());
                Assert.That(view, Is.Not.Null, "DifficultySelectView should exist in Home scene.");

                var viewSO = new UnityEditor.SerializedObject(view);
                Assert.That(viewSO.FindProperty("_easyButton").objectReferenceValue, Is.Not.Null, "EasyButton should not be null.");
                Assert.That(viewSO.FindProperty("_normalButton").objectReferenceValue, Is.Not.Null, "NormalButton should not be null.");
                Assert.That(viewSO.FindProperty("_hardButton").objectReferenceValue, Is.Not.Null, "HardButton should not be null.");
                Assert.That(viewSO.FindProperty("_backButton").objectReferenceValue, Is.Not.Null, "BackButton should not be null.");

                Assert.That(view.LockIcons, Is.Not.Null, "LockIcons array should not be null.");
                Assert.That(view.LockIcons.Length, Is.EqualTo(3), "LockIcons array should have 3 elements.");
                for (int i = 0; i < 3; i++)
                {
                    Assert.That(view.LockIcons[i], Is.Not.Null, $"LockIcons[{i}] should not be null.");
                }

                Assert.That(view.AchievementTexts, Is.Not.Null, "AchievementTexts array should not be null.");
                Assert.That(view.AchievementTexts.Length, Is.EqualTo(3), "AchievementTexts array should have 3 elements.");
                for (int i = 0; i < 3; i++)
                {
                    Assert.That(view.AchievementTexts[i], Is.Not.Null, $"AchievementTexts[{i}] should not be null.");
                }
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void HomeScene_CollectionView_IsWiredCorrectly()
        {
            var scene = EditorSceneManager.OpenScene(
                "Assets/Scenes/Home.unity",
                OpenSceneMode.Additive);
            try
            {
                var pictureView = FindInScene<PictureSelectView>(
                    scene.GetRootGameObjects());
                var collectionView = FindInScene<CollectionView>(
                    scene.GetRootGameObjects());
                Assert.That(collectionView, Is.Not.Null);

                var pictureSo = new UnityEditor.SerializedObject(pictureView);
                Assert.That(
                    pictureSo.FindProperty("_scrollRect").objectReferenceValue,
                    Is.Not.Null);
                Assert.That(
                    pictureSo.FindProperty("_collectionButton").objectReferenceValue,
                    Is.Not.Null);

                var collectionSo = new UnityEditor.SerializedObject(collectionView);
                string[] requiredFields =
                {
                    "_itemContent",
                    "_itemButtonTemplate",
                    "_itemNameText",
                    "_itemDescriptionText",
                    "_itemThumbnail",
                    "_sourceContent",
                    "_sourceButtonTemplate",
                    "_closeButton"
                };
                foreach (string fieldName in requiredFields)
                {
                    Assert.That(
                        collectionSo.FindProperty(fieldName).objectReferenceValue,
                        Is.Not.Null,
                        $"{fieldName} should be wired.");
                }
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static T FindInScene<T>(UnityEngine.GameObject[] rootObjects)
            where T : UnityEngine.Component
        {
            foreach (var rootObject in rootObjects)
            {
                var component = rootObject.GetComponentInChildren<T>(true);
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }

        private static UnityEngine.GameObject FindGameObjectByName(UnityEngine.GameObject root, string name)
        {
            if (root.name == name) return root;
            for (int i = 0; i < root.transform.childCount; i++)
            {
                var result = FindGameObjectByName(root.transform.GetChild(i).gameObject, name);
                if (result != null) return result;
            }
            return null;
        }
    }
}
