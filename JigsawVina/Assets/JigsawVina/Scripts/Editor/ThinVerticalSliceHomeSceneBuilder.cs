#if UNITY_EDITOR
using System.Collections.Generic;
using JigsawVina.Presentation.Screens;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace JigsawVina.Editor
{
    internal static class ThinVerticalSliceHomeSceneBuilder
    {
        public static void CreateHomeScene(string homeScenePath)
        {
            if (ThinVerticalSliceSceneSetup.CheckSceneAlreadyUpdated(homeScenePath, "SetupVersionMarker_v7"))
            {
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Home";

            new GameObject("SetupVersionMarker_v7");

            ThinVerticalSliceUiFactory.CreateCamera();
            ThinVerticalSliceUiFactory.CreateEventSystem();

            var scope = new GameObject("HomeLifetimeScope");
            scope.AddComponent<JigsawVina.Presentation.Screens.HomeLifetimeScope>();

            var canvas = ThinVerticalSliceUiFactory.CreateCanvas("HomeCanvas");
            var pictureScreen = ThinVerticalSliceUiFactory.CreateScreen(canvas.transform, "PictureSelectScreen");
            var pictureView = pictureScreen.AddComponent<PictureSelectView>();

            ThinVerticalSliceUiFactory.AddHeader(pictureScreen.transform, "Chọn Tranh", new Vector2(0f, 320f), new Vector2(760f, 80f));

            var collectionButton = ThinVerticalSliceUiFactory.CreateButton(
                pictureScreen.transform,
                "CollectionButton",
                "Collection",
                new Vector2(690f, 320f),
                new Vector2(260f, 60f));

            var dailyRewardButton = ThinVerticalSliceUiFactory.CreateButton(
                pictureScreen.transform,
                "DailyRewardButton",
                "Daily Reward",
                new Vector2(410f, 320f),
                new Vector2(260f, 60f));

            var badgeGo = new GameObject("DailyRewardNotificationBadge", typeof(RectTransform), typeof(Image));
            badgeGo.transform.SetParent(dailyRewardButton.transform, false);
            var badgeRect = (RectTransform)badgeGo.transform;
            badgeRect.anchorMin = new Vector2(1f, 1f);
            badgeRect.anchorMax = new Vector2(1f, 1f);
            badgeRect.pivot = new Vector2(0.5f, 0.5f);
            badgeRect.anchoredPosition = new Vector2(0f, 0f);
            badgeRect.sizeDelta = new Vector2(20f, 20f);
            var badgeImage = badgeGo.GetComponent<Image>();
            badgeImage.color = Color.red;
            badgeGo.SetActive(false);

            var scrollViewObj = new GameObject("PictureScrollView", typeof(RectTransform));
            scrollViewObj.transform.SetParent(pictureScreen.transform, false);
            var scrollViewRect = (RectTransform)scrollViewObj.transform;
            scrollViewRect.anchoredPosition = new Vector2(0f, -50f);
            scrollViewRect.sizeDelta = new Vector2(600f, 600f);

            var scrollRect = scrollViewObj.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            var viewportObj = new GameObject("Viewport", typeof(RectTransform));
            viewportObj.transform.SetParent(scrollViewObj.transform, false);
            var viewportRect = (RectTransform)viewportObj.transform;
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            viewportObj.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.05f);
            viewportObj.AddComponent<Mask>().showMaskGraphic = false;

            var contentObj = new GameObject("Content", typeof(RectTransform));
            contentObj.transform.SetParent(viewportObj.transform, false);
            var contentRect = (RectTransform)contentObj.transform;
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0f, 600f);

            var layoutGroup = contentObj.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = 20;
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childAlignment = TextAnchor.UpperCenter;

            contentObj.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;

            // Load card prefab
            const string cardPrefabPath = "Assets/JigsawVina/Prefabs/PictureSelectCard.prefab";
            var cardPrefab = AssetDatabase.LoadAssetAtPath<PictureSelectCard>(cardPrefabPath);
            if (cardPrefab == null)
            {
                Debug.LogError($"[JigsawVina] Prefab not found at path: {cardPrefabPath}. Make sure Task 28 was run successfully.");
            }

            ThinVerticalSliceUiFactory.Assign(pictureView, "_cardPrefab", cardPrefab);
            ThinVerticalSliceUiFactory.Assign(pictureView, "_contentContainer", contentRect);
            ThinVerticalSliceUiFactory.Assign(pictureView, "_scrollRect", scrollRect);
            ThinVerticalSliceUiFactory.Assign(pictureView, "_collectionButton", collectionButton);
            ThinVerticalSliceUiFactory.Assign(pictureView, "_dailyRewardButton", dailyRewardButton);
            ThinVerticalSliceUiFactory.Assign(pictureView, "_dailyRewardNotificationBadge", badgeGo);
            
            var difficultyScreen = ThinVerticalSliceUiFactory.CreateScreen(canvas.transform, "DifficultySelectScreen");
            var difficultyView = difficultyScreen.AddComponent<DifficultySelectView>();

            ThinVerticalSliceUiFactory.AddHeader(difficultyScreen.transform, "Chọn Độ Khó", new Vector2(0f, 210f), new Vector2(760f, 60f));
            var easyButton = ThinVerticalSliceUiFactory.CreateButton(difficultyScreen.transform, "EasyButton", "Dễ", new Vector2(0f, 120f), new Vector2(520f, 60f));
            var normalButton = ThinVerticalSliceUiFactory.CreateButton(difficultyScreen.transform, "NormalButton", "Trung bình", new Vector2(0f, 20f), new Vector2(520f, 60f));
            var hardButton = ThinVerticalSliceUiFactory.CreateButton(difficultyScreen.transform, "HardButton", "Khó", new Vector2(0f, -80f), new Vector2(520f, 60f));
            var backButton = ThinVerticalSliceUiFactory.CreateButton(difficultyScreen.transform, "BackButton", "Quay lại", new Vector2(0f, -180f), new Vector2(520f, 60f));
            
            ThinVerticalSliceUiFactory.Assign(difficultyView, "_easyButton", easyButton);
            ThinVerticalSliceUiFactory.Assign(difficultyView, "_normalButton", normalButton);
            ThinVerticalSliceUiFactory.Assign(difficultyView, "_hardButton", hardButton);
            ThinVerticalSliceUiFactory.Assign(difficultyView, "_backButton", backButton);

            var lock0 = ThinVerticalSliceUiFactory.CreateLockIcon(easyButton.transform);
            var lock1 = ThinVerticalSliceUiFactory.CreateLockIcon(normalButton.transform);
            var lock2 = ThinVerticalSliceUiFactory.CreateLockIcon(hardButton.transform);

            var text0 = ThinVerticalSliceUiFactory.CreateAchievementText(difficultyScreen.transform, "AchievementText_Easy", new Vector2(0f, 75f));
            var text1 = ThinVerticalSliceUiFactory.CreateAchievementText(difficultyScreen.transform, "AchievementText_Normal", new Vector2(0f, -25f));
            var text2 = ThinVerticalSliceUiFactory.CreateAchievementText(difficultyScreen.transform, "AchievementText_Hard", new Vector2(0f, -125f));

            var viewSo = new SerializedObject(difficultyView);
            var lockIconsProp = viewSo.FindProperty("_lockIcons");
            lockIconsProp.arraySize = 3;
            lockIconsProp.GetArrayElementAtIndex(0).objectReferenceValue = lock0;
            lockIconsProp.GetArrayElementAtIndex(1).objectReferenceValue = lock1;
            lockIconsProp.GetArrayElementAtIndex(2).objectReferenceValue = lock2;

            var achievementTextsProp = viewSo.FindProperty("_achievementTexts");
            achievementTextsProp.arraySize = 3;
            achievementTextsProp.GetArrayElementAtIndex(0).objectReferenceValue = text0;
            achievementTextsProp.GetArrayElementAtIndex(1).objectReferenceValue = text1;
            achievementTextsProp.GetArrayElementAtIndex(2).objectReferenceValue = text2;
            
            viewSo.ApplyModifiedPropertiesWithoutUndo();

            difficultyScreen.SetActive(false);

            var collectionScreen = ThinVerticalSliceUiFactory.CreateScreen(canvas.transform, "CollectionScreen");
            var collectionView = collectionScreen.AddComponent<CollectionView>();
            collectionScreen.AddComponent<Image>().color =
                new Color(0.04f, 0.06f, 0.1f, 0.98f);

            ThinVerticalSliceUiFactory.AddHeader(
                collectionScreen.transform,
                "Collection",
                new Vector2(0f, 430f),
                new Vector2(760f, 70f));
            var closeCollectionButton = ThinVerticalSliceUiFactory.CreateButton(
                collectionScreen.transform,
                "CloseCollectionButton",
                "Close",
                new Vector2(760f, 430f),
                new Vector2(220f, 55f));

            var itemContentObject = new GameObject(
                "CollectionItemContent",
                typeof(RectTransform));
            itemContentObject.transform.SetParent(collectionScreen.transform, false);
            var itemContentRect = (RectTransform)itemContentObject.transform;
            itemContentRect.anchoredPosition = new Vector2(-560f, -20f);
            itemContentRect.sizeDelta = new Vector2(420f, 760f);
            var itemLayout = itemContentObject.AddComponent<VerticalLayoutGroup>();
            itemLayout.spacing = 12f;
            itemLayout.childControlWidth = false;
            itemLayout.childControlHeight = false;
            itemLayout.childForceExpandWidth = false;
            itemLayout.childForceExpandHeight = false;
            itemLayout.childAlignment = TextAnchor.UpperCenter;

            var itemButtonTemplate = ThinVerticalSliceUiFactory.CreateButton(
                itemContentRect,
                "ItemButtonTemplate",
                "Item",
                Vector2.zero,
                new Vector2(380f, 64f));
            itemButtonTemplate.gameObject.SetActive(false);

            var thumbnailObject = new GameObject(
                "CollectionItemThumbnail",
                typeof(RectTransform),
                typeof(Image));
            thumbnailObject.transform.SetParent(collectionScreen.transform, false);
            var thumbnailRect = (RectTransform)thumbnailObject.transform;
            thumbnailRect.anchoredPosition = new Vector2(-80f, 180f);
            thumbnailRect.sizeDelta = new Vector2(260f, 220f);
            var thumbnailImage = thumbnailObject.GetComponent<Image>();
            thumbnailImage.preserveAspect = true;
            thumbnailImage.color = new Color(1f, 1f, 1f, 0.9f);

            var itemNameText = ThinVerticalSliceUiFactory.AddHeader(
                collectionScreen.transform,
                "Select an item",
                new Vector2(300f, 280f),
                new Vector2(650f, 70f));
            var itemDescriptionText = ThinVerticalSliceUiFactory.AddHeader(
                collectionScreen.transform,
                "",
                new Vector2(300f, 170f),
                new Vector2(650f, 140f));
            itemDescriptionText.fontSize = 22f;
            itemDescriptionText.enableWordWrapping = true;

            var sourceContentObject = new GameObject(
                "CollectionSourceContent",
                typeof(RectTransform));
            sourceContentObject.transform.SetParent(collectionScreen.transform, false);
            var sourceContentRect = (RectTransform)sourceContentObject.transform;
            sourceContentRect.anchoredPosition = new Vector2(300f, -170f);
            sourceContentRect.sizeDelta = new Vector2(650f, 420f);
            var sourceLayout = sourceContentObject.AddComponent<VerticalLayoutGroup>();
            sourceLayout.spacing = 10f;
            sourceLayout.childControlWidth = false;
            sourceLayout.childControlHeight = false;
            sourceLayout.childForceExpandWidth = false;
            sourceLayout.childForceExpandHeight = false;
            sourceLayout.childAlignment = TextAnchor.UpperCenter;

            var sourceButtonTemplate = ThinVerticalSliceUiFactory.CreateButton(
                sourceContentRect,
                "SourceButtonTemplate",
                "Item source",
                Vector2.zero,
                new Vector2(600f, 58f));
            sourceButtonTemplate.gameObject.SetActive(false);

            ThinVerticalSliceUiFactory.Assign(collectionView, "_itemContent", itemContentRect);
            ThinVerticalSliceUiFactory.Assign(collectionView, "_itemButtonTemplate", itemButtonTemplate);
            ThinVerticalSliceUiFactory.Assign(collectionView, "_itemNameText", itemNameText);
            ThinVerticalSliceUiFactory.Assign(collectionView, "_itemDescriptionText", itemDescriptionText);
            ThinVerticalSliceUiFactory.Assign(collectionView, "_itemThumbnail", thumbnailImage);
            ThinVerticalSliceUiFactory.Assign(collectionView, "_sourceContent", sourceContentRect);
            ThinVerticalSliceUiFactory.Assign(collectionView, "_sourceButtonTemplate", sourceButtonTemplate);
            ThinVerticalSliceUiFactory.Assign(collectionView, "_closeButton", closeCollectionButton);
            collectionScreen.SetActive(false);

            // Create Daily Reward Popup Panel
            var dailyRewardPopup = ThinVerticalSliceUiFactory.CreateScreen(canvas.transform, "DailyRewardPopup");
            var dailyRewardView = dailyRewardPopup.AddComponent<DailyRewardView>();
            dailyRewardPopup.AddComponent<Image>().color = new Color(0.04f, 0.06f, 0.1f, 0.98f);

            ThinVerticalSliceUiFactory.AddHeader(
                dailyRewardPopup.transform,
                "Daily Login Rewards",
                new Vector2(0f, 430f),
                new Vector2(760f, 70f));

            var closeDailyRewardButton = ThinVerticalSliceUiFactory.CreateButton(
                dailyRewardPopup.transform,
                "CloseDailyRewardButton",
                "Close",
                new Vector2(0f, -430f),
                new Vector2(220f, 55f));

            var claimDailyRewardButton = ThinVerticalSliceUiFactory.CreateButton(
                dailyRewardPopup.transform,
                "ClaimDailyRewardButton",
                "Claim",
                new Vector2(0f, -340f),
                new Vector2(300f, 60f));

            var feedbackText = ThinVerticalSliceUiFactory.AddText(
                dailyRewardPopup.transform,
                "",
                new Vector2(0f, -260f),
                new Vector2(800f, 50f));
            feedbackText.color = new Color(0.18f, 0.65f, 0.95f);

            var slotsContainer = new GameObject("SlotsContainer", typeof(RectTransform));
            slotsContainer.transform.SetParent(dailyRewardPopup.transform, false);
            var slotsContainerRect = (RectTransform)slotsContainer.transform;
            slotsContainerRect.anchoredPosition = new Vector2(0f, 50f);
            slotsContainerRect.sizeDelta = new Vector2(1200f, 300f);

            var slotsLayout = slotsContainer.AddComponent<HorizontalLayoutGroup>();
            slotsLayout.spacing = 15;
            slotsLayout.childControlWidth = false;
            slotsLayout.childControlHeight = false;
            slotsLayout.childForceExpandWidth = false;
            slotsLayout.childForceExpandHeight = false;
            slotsLayout.childAlignment = TextAnchor.MiddleCenter;

            var slotsList = new List<DailyRewardView.RewardSlotUI>();

            for (int d = 1; d <= 7; d++)
            {
                var slotGo = new GameObject($"DaySlot_{d}", typeof(RectTransform), typeof(Image));
                slotGo.transform.SetParent(slotsContainerRect.transform, false);
                var slotRect = (RectTransform)slotGo.transform;
                slotRect.sizeDelta = new Vector2(140f, 220f);
                slotGo.GetComponent<Image>().color = new Color(0.12f, 0.18f, 0.28f, 0.95f);

                var dayTxt = ThinVerticalSliceUiFactory.AddText(slotGo.transform, $"Day {d}", new Vector2(0f, 80f), new Vector2(120f, 30f));
                dayTxt.fontSize = 20;

                var imgGo = new GameObject("RewardImage", typeof(RectTransform), typeof(Image));
                imgGo.transform.SetParent(slotGo.transform, false);
                var imgRect = (RectTransform)imgGo.transform;
                imgRect.anchoredPosition = new Vector2(0f, 10f);
                imgRect.sizeDelta = new Vector2(64f, 64f);
                var slotImg = imgGo.GetComponent<Image>();
                slotImg.color = Color.white;
                slotImg.preserveAspect = true;

                var amtTxt = ThinVerticalSliceUiFactory.AddText(slotGo.transform, "+50", new Vector2(0f, -50f), new Vector2(120f, 30f));
                amtTxt.fontSize = 18;

                var claimedOverlay = new GameObject("ClaimedOverlay", typeof(RectTransform), typeof(Image));
                claimedOverlay.transform.SetParent(slotGo.transform, false);
                var claimedRect = (RectTransform)claimedOverlay.transform;
                claimedRect.anchorMin = Vector2.zero;
                claimedRect.anchorMax = Vector2.one;
                claimedRect.offsetMin = Vector2.zero;
                claimedRect.offsetMax = Vector2.zero;
                claimedOverlay.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);
                var claimedTxt = ThinVerticalSliceUiFactory.AddText(claimedOverlay.transform, "Claimed", Vector2.zero, new Vector2(120f, 40f));
                claimedTxt.fontSize = 20;
                claimedTxt.color = Color.green;
                claimedOverlay.SetActive(false);

                var nextHighlight = new GameObject("NextHighlight", typeof(RectTransform), typeof(Image));
                nextHighlight.transform.SetParent(slotGo.transform, false);
                var highlightRect = (RectTransform)nextHighlight.transform;
                highlightRect.anchorMin = Vector2.zero;
                highlightRect.anchorMax = Vector2.one;
                highlightRect.offsetMin = new Vector2(-4f, -4f);
                highlightRect.offsetMax = new Vector2(4f, 4f);
                var highlightImg = nextHighlight.GetComponent<Image>();
                highlightImg.color = Color.yellow;
                nextHighlight.transform.SetAsFirstSibling();
                nextHighlight.SetActive(false);

                var lockedOverlay = new GameObject("LockedOverlay", typeof(RectTransform), typeof(Image));
                lockedOverlay.transform.SetParent(slotGo.transform, false);
                var lockedRect = (RectTransform)lockedOverlay.transform;
                lockedRect.anchorMin = Vector2.zero;
                lockedRect.anchorMax = Vector2.one;
                lockedRect.offsetMin = Vector2.zero;
                lockedRect.offsetMax = Vector2.zero;
                lockedOverlay.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.4f);
                var lockedTxt = ThinVerticalSliceUiFactory.AddText(lockedOverlay.transform, "Locked", Vector2.zero, new Vector2(120f, 40f));
                lockedTxt.fontSize = 20;
                lockedTxt.color = Color.gray;
                lockedOverlay.SetActive(false);

                slotsList.Add(new DailyRewardView.RewardSlotUI
                {
                    dayText = dayTxt,
                    rewardImage = slotImg,
                    amountText = amtTxt,
                    claimedOverlay = claimedOverlay,
                    nextClaimableHighlight = nextHighlight,
                    lockedOverlay = lockedOverlay
                });
            }

            var dailyRewardSo = new SerializedObject(dailyRewardView);
            ThinVerticalSliceUiFactory.Assign(dailyRewardView, "_claimButton", claimDailyRewardButton);
            ThinVerticalSliceUiFactory.Assign(dailyRewardView, "_closeButton", closeDailyRewardButton);
            ThinVerticalSliceUiFactory.Assign(dailyRewardView, "_popupPanel", dailyRewardPopup);
            ThinVerticalSliceUiFactory.Assign(dailyRewardView, "_feedbackText", feedbackText);

            var slotsProp = dailyRewardSo.FindProperty("_slots");
            slotsProp.arraySize = 7;
            for (int i = 0; i < 7; i++)
            {
                var element = slotsProp.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("dayText").objectReferenceValue = slotsList[i].dayText;
                element.FindPropertyRelative("rewardImage").objectReferenceValue = slotsList[i].rewardImage;
                element.FindPropertyRelative("amountText").objectReferenceValue = slotsList[i].amountText;
                element.FindPropertyRelative("claimedOverlay").objectReferenceValue = slotsList[i].claimedOverlay;
                element.FindPropertyRelative("nextClaimableHighlight").objectReferenceValue = slotsList[i].nextClaimableHighlight;
                element.FindPropertyRelative("lockedOverlay").objectReferenceValue = slotsList[i].lockedOverlay;
            }
            dailyRewardSo.ApplyModifiedPropertiesWithoutUndo();

            dailyRewardPopup.SetActive(false);

            EditorSceneManager.SaveScene(scene, homeScenePath);
        }
    }
}
#endif
