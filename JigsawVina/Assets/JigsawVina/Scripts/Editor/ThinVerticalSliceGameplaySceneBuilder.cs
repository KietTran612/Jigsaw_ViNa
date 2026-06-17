#if UNITY_EDITOR
using System.Collections.Generic;
using JigsawVina.Presentation.Screens;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace JigsawVina.Editor
{
    internal static class ThinVerticalSliceGameplaySceneBuilder
    {
        public static void CreateGameplayScene(string gameplayScenePath)
        {
            if (ThinVerticalSliceSceneSetup.CheckSceneAlreadyUpdated(gameplayScenePath, "SetupVersionMarker_v4"))
            {
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Gameplay";

            new GameObject("SetupVersionMarker_v4");

            ThinVerticalSliceUiFactory.CreateCamera();
            ThinVerticalSliceUiFactory.CreateEventSystem();

            var scope = new GameObject("GameplayLifetimeScope");
            scope.AddComponent<JigsawVina.Presentation.Screens.GameplayLifetimeScope>();

            var canvas = ThinVerticalSliceUiFactory.CreateCanvas("GameplayCanvas");
            var playingScreen = ThinVerticalSliceUiFactory.CreateScreen(canvas.transform, "PuzzlePlayingScreen");
            var playingView = playingScreen.AddComponent<PuzzlePlayingView>();
            playingScreen.AddComponent<CanvasGroup>();

            var dragContainerObj = new GameObject("DragContainer", typeof(RectTransform));
            dragContainerObj.transform.SetParent(playingScreen.transform, false);
            var dragContainerRect = (RectTransform)dragContainerObj.transform;
            dragContainerRect.anchorMin = Vector2.zero;
            dragContainerRect.anchorMax = Vector2.one;
            dragContainerRect.offsetMin = Vector2.zero;
            dragContainerRect.offsetMax = Vector2.zero;

            var topBar = new GameObject("TopBar", typeof(RectTransform));
            topBar.transform.SetParent(playingScreen.transform, false);
            var topBarRect = (RectTransform)topBar.transform;
            topBarRect.anchorMin = new Vector2(0f, 1f);
            topBarRect.anchorMax = new Vector2(1f, 1f);
            topBarRect.pivot = new Vector2(0.5f, 1f);
            topBarRect.anchoredPosition = Vector2.zero;
            topBarRect.sizeDelta = new Vector2(0f, 100f);

            var backButton = ThinVerticalSliceUiFactory.CreateButton(topBar.transform, "BackButton", "Quay lại", new Vector2(-800f, 0f), new Vector2(200f, 60f));
            var titleText = ThinVerticalSliceUiFactory.AddHeader(topBar.transform, "Hồ Gươm - Dễ", new Vector2(-400f, 0f), new Vector2(400f, 60f));
            var timerText = ThinVerticalSliceUiFactory.AddHeader(topBar.transform, "Thời gian: 00:00", new Vector2(0f, 0f), new Vector2(300f, 60f));
            var previewOpacityText = ThinVerticalSliceUiFactory.AddHeader(topBar.transform, "Ảnh gốc 20%", new Vector2(275f, 0f), new Vector2(180f, 60f));
            previewOpacityText.fontSize = 22f;
            var previewOpacitySlider = ThinVerticalSliceUiFactory.CreateSlider(topBar.transform, "PreviewOpacitySlider", new Vector2(435f, 0f), new Vector2(150f, 30f), 0.2f);
            var hintButton = ThinVerticalSliceUiFactory.CreateButton(topBar.transform, "HintButton", "Gợi ý", new Vector2(590f, 0f), new Vector2(150f, 60f));
            var returnToTrayButton = ThinVerticalSliceUiFactory.CreateButton(topBar.transform, "ReturnToTrayButton", "Xếp lại", new Vector2(755f, 0f), new Vector2(150f, 60f));
            var cheatButton = ThinVerticalSliceUiFactory.CreateButton(topBar.transform, "CheatWinButton", "Debug Win", new Vector2(900f, 0f), new Vector2(120f, 60f));

            var mainArea = new GameObject("MainArea", typeof(RectTransform));
            mainArea.transform.SetParent(playingScreen.transform, false);
            var mainAreaRect = (RectTransform)mainArea.transform;
            mainAreaRect.anchorMin = Vector2.zero;
            mainAreaRect.anchorMax = Vector2.one;
            mainAreaRect.offsetMin = Vector2.zero;
            mainAreaRect.offsetMax = new Vector2(0f, -100f);

            var boardArea = new GameObject("BoardArea", typeof(RectTransform));
            boardArea.transform.SetParent(mainArea.transform, false);
            var boardAreaRect = (RectTransform)boardArea.transform;
            boardAreaRect.anchorMin = Vector2.zero;
            boardAreaRect.anchorMax = new Vector2(0.75f, 1f);
            boardAreaRect.offsetMin = Vector2.zero;
            boardAreaRect.offsetMax = Vector2.zero;

            var boardGo = new GameObject("Board", typeof(RectTransform));
            boardGo.transform.SetParent(boardArea.transform, false);
            var boardRect = (RectTransform)boardGo.transform;
            boardRect.anchorMin = new Vector2(0.5f, 0.5f);
            boardRect.anchorMax = new Vector2(0.5f, 0.5f);
            boardRect.pivot = new Vector2(0.5f, 0.5f);
            boardRect.anchoredPosition = Vector2.zero;
            boardRect.sizeDelta = new Vector2(800f, 600f);
            boardGo.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f);

            var boardView = boardGo.AddComponent<PuzzleBoardView>();

            var previewOverlay = new GameObject("PreviewOverlay", typeof(RectTransform));
            previewOverlay.transform.SetParent(boardGo.transform, false);
            var previewOverlayRect = (RectTransform)previewOverlay.transform;
            previewOverlayRect.anchorMin = Vector2.zero;
            previewOverlayRect.anchorMax = Vector2.one;
            previewOverlayRect.offsetMin = Vector2.zero;
            previewOverlayRect.offsetMax = Vector2.zero;
            var previewOverlayImage = previewOverlay.AddComponent<Image>();
            previewOverlayImage.color = new Color(1f, 1f, 1f, 0.2f);

            var lockedPieces = new GameObject("LockedPieces", typeof(RectTransform));
            lockedPieces.transform.SetParent(boardGo.transform, false);
            var lockedPiecesRect = (RectTransform)lockedPieces.transform;
            lockedPiecesRect.anchorMin = Vector2.zero;
            lockedPiecesRect.anchorMax = Vector2.one;
            lockedPiecesRect.offsetMin = Vector2.zero;
            lockedPiecesRect.offsetMax = Vector2.zero;

            ThinVerticalSliceUiFactory.Assign(boardView, "_previewImage", previewOverlayImage);
            ThinVerticalSliceUiFactory.Assign(boardView, "_lockedPiecesContainer", lockedPiecesRect);

            var trayArea = new GameObject("TrayArea", typeof(RectTransform));
            trayArea.transform.SetParent(mainArea.transform, false);
            var trayAreaRect = (RectTransform)trayArea.transform;
            trayAreaRect.anchorMin = new Vector2(0.75f, 0f);
            trayAreaRect.anchorMax = new Vector2(1f, 1f);
            trayAreaRect.offsetMin = Vector2.zero;
            trayAreaRect.offsetMax = Vector2.zero;
            trayArea.AddComponent<Image>().color = new Color(0.1f, 0.11f, 0.12f);

            var scrollView = new GameObject("ScrollView", typeof(RectTransform));
            scrollView.transform.SetParent(trayArea.transform, false);
            var scrollViewRect = (RectTransform)scrollView.transform;
            scrollViewRect.anchorMin = Vector2.zero;
            scrollViewRect.anchorMax = Vector2.one;
            scrollViewRect.offsetMin = new Vector2(10f, 10f);
            scrollViewRect.offsetMax = new Vector2(-10f, -10f);

            var scrollRect = scrollView.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            var viewport = new GameObject("Viewport", typeof(RectTransform));
            viewport.transform.SetParent(scrollView.transform, false);
            var viewportRect = (RectTransform)viewport.transform;
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            viewport.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.1f);
            viewport.AddComponent<Mask>().showMaskGraphic = false;

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            var contentRect = (RectTransform)content.transform;
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0f, 300f);

            var grid = content.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(120f, 90f);
            grid.spacing = new Vector2(10f, 10f);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 2;

            content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;

            ThinVerticalSliceUiFactory.Assign(playingView, "_titleText", titleText);
            ThinVerticalSliceUiFactory.Assign(playingView, "_timerText", timerText);
            ThinVerticalSliceUiFactory.Assign(playingView, "_previewOpacityText", previewOpacityText);
            ThinVerticalSliceUiFactory.Assign(playingView, "_backButton", backButton);
            ThinVerticalSliceUiFactory.Assign(playingView, "_previewOpacitySlider", previewOpacitySlider);
            ThinVerticalSliceUiFactory.Assign(playingView, "_hintButton", hintButton);
            ThinVerticalSliceUiFactory.Assign(playingView, "_returnToTrayButton", returnToTrayButton);
            ThinVerticalSliceUiFactory.Assign(playingView, "_cheatWinButton", cheatButton);
            ThinVerticalSliceUiFactory.Assign(playingView, "_boardView", boardView);
            ThinVerticalSliceUiFactory.Assign(playingView, "_trayContent", contentRect);
            ThinVerticalSliceUiFactory.Assign(playingView, "_dragContainer", dragContainerRect);
            ThinVerticalSliceUiFactory.Assign(playingView, "_canvas", canvas);
            dragContainerRect.SetAsLastSibling();

            var rewardScreen = ThinVerticalSliceUiFactory.CreateScreen(canvas.transform, "RewardSummaryScreen");
            var rewardView = rewardScreen.AddComponent<RewardSummaryView>();

            var starsText = ThinVerticalSliceUiFactory.AddHeader(rewardScreen.transform, "Stars: 0", new Vector2(0f, 120f), new Vector2(600f, 80f));
            var coinsText = ThinVerticalSliceUiFactory.AddHeader(rewardScreen.transform, "Coins Earned: 0", new Vector2(0f, 40f), new Vector2(600f, 80f));
            var keyItemsText = ThinVerticalSliceUiFactory.AddHeader(rewardScreen.transform, "", new Vector2(0f, -30f), new Vector2(800f, 60f));
            keyItemsText.fontSize = 26f;
            var returnButton = ThinVerticalSliceUiFactory.CreateButton(rewardScreen.transform, "ReturnHomeButton", "Return Home", new Vector2(0f, -100f), new Vector2(300f, 60f));
            ThinVerticalSliceUiFactory.Assign(rewardView, "_starsText", starsText);
            ThinVerticalSliceUiFactory.Assign(rewardView, "_coinsText", coinsText);
            ThinVerticalSliceUiFactory.Assign(rewardView, "_keyItemsText", keyItemsText);
            ThinVerticalSliceUiFactory.Assign(rewardView, "_returnButton", returnButton);

            rewardScreen.SetActive(false);

            EditorSceneManager.SaveScene(scene, gameplayScenePath);
        }
    }
}
#endif
