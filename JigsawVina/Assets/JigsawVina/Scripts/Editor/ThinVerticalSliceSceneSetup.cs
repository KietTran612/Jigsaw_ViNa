#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using JigsawVina.Presentation.App;
using JigsawVina.Presentation.Screens;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer.Unity;

namespace JigsawVina.Editor
{
    public static class ThinVerticalSliceSceneSetup
    {
        private const string ScenesFolder = "Assets/Scenes";
        private const string PrefabsFolder = "Assets/JigsawVina/Prefabs";
        private const string SettingsFolder = "Assets/JigsawVina/Settings";
        private const string HomeScenePath = ScenesFolder + "/Home.unity";
        private const string GameplayScenePath = ScenesFolder + "/Gameplay.unity";
        private const string ProjectScopePrefabPath = PrefabsFolder + "/ProjectLifetimeScope.prefab";
        private const string VContainerSettingsPath = SettingsFolder + "/VContainerSettings.asset";
        private static readonly string[] PuzzleTexturePaths =
        {
            "Assets/Resources/Textures/ho_guom.png",
            "Assets/Resources/Textures/ha_long.png"
        };

        [MenuItem("JigsawVina/Setup Thin Vertical Slice Scenes")]
        public static void Setup()
        {
            EnsureFolders();
            ConfigurePuzzleTextureImporters();
            var projectScopePrefab = CreateProjectLifetimeScopePrefab();
            ConfigureVContainerSettings(projectScopePrefab);
            CreateHomeScene();
            CreateGameplayScene();
            ConfigureBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[JigsawVina] Thin vertical slice scene setup complete.");
        }

        private static void ConfigurePuzzleTextureImporters()
        {
            foreach (string texturePath in PuzzleTexturePaths)
            {
                if (AssetImporter.GetAtPath(texturePath) is not TextureImporter importer)
                {
                    continue;
                }

                bool changed = importer.textureType != TextureImporterType.Default
                    || importer.spriteImportMode != SpriteImportMode.None
                    || importer.isReadable;
                if (!changed)
                {
                    continue;
                }

                importer.textureType = TextureImporterType.Default;
                importer.spriteImportMode = SpriteImportMode.None;
                importer.isReadable = false;
                importer.SaveAndReimport();
            }
        }

        private static void EnsureFolders()
        {
            CreateFolderIfMissing("Assets/JigsawVina", "Prefabs");
            CreateFolderIfMissing("Assets/JigsawVina", "Settings");
        }

        private static void CreateFolderIfMissing(string parent, string folder)
        {
            string path = parent + "/" + folder;
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, folder);
            }
        }

        private static ProjectLifetimeScope CreateProjectLifetimeScopePrefab()
        {
            var existing = AssetDatabase.LoadAssetAtPath<ProjectLifetimeScope>(ProjectScopePrefabPath);
            if (existing != null)
            {
                return existing;
            }

            var go = new GameObject("ProjectLifetimeScope");
            var scope = go.AddComponent<ProjectLifetimeScope>();
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, ProjectScopePrefabPath);
            UnityEngine.Object.DestroyImmediate(go);
            return prefab.GetComponent<ProjectLifetimeScope>();
        }

        private static void ConfigureVContainerSettings(ProjectLifetimeScope projectScopePrefab)
        {
            var settings = AssetDatabase.LoadAssetAtPath<VContainerSettings>(VContainerSettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<VContainerSettings>();
                AssetDatabase.CreateAsset(settings, VContainerSettingsPath);
            }

            settings.RootLifetimeScope = projectScopePrefab;
            EditorUtility.SetDirty(settings);

            var preloadedAssets = new List<UnityEngine.Object>(PlayerSettings.GetPreloadedAssets());
            preloadedAssets.RemoveAll(asset => asset is VContainerSettings);
            preloadedAssets.Add(settings);
            PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
            VContainerSettings.LoadInstanceFromPreloadAssets();
        }

        private static bool CheckSceneAlreadyUpdated(string scenePath, string markerName)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
            {
                return false;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var marker = GameObject.Find(markerName);
            return marker != null;
        }

        private static void CreateHomeScene()
        {
            if (CheckSceneAlreadyUpdated(HomeScenePath, "SetupVersionMarker_v3"))
            {
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Home";

            new GameObject("SetupVersionMarker_v3");

            CreateCamera();
            CreateEventSystem();

            var scope = new GameObject("HomeLifetimeScope");
            scope.AddComponent<JigsawVina.Presentation.Screens.HomeLifetimeScope>();

            var canvas = CreateCanvas("HomeCanvas");
            var pictureScreen = CreateScreen(canvas.transform, "PictureSelectScreen");
            var pictureView = pictureScreen.AddComponent<PictureSelectView>();

            AddHeader(pictureScreen.transform, "Chọn Tranh", new Vector2(0f, 170f), new Vector2(760f, 80f));
            var pic1Button = CreateButton(pictureScreen.transform, "Ho GuomButton", "Hồ Gươm", new Vector2(0f, 70f), new Vector2(520f, 64f));
            var pic2Button = CreateButton(pictureScreen.transform, "Ha LongButton", "Hạ Long", new Vector2(0f, -10f), new Vector2(520f, 64f));
            Assign(pictureView, "_pic1Button", pic1Button);
            Assign(pictureView, "_pic2Button", pic2Button);

            var difficultyScreen = CreateScreen(canvas.transform, "DifficultySelectScreen");
            var difficultyView = difficultyScreen.AddComponent<DifficultySelectView>();

            AddHeader(difficultyScreen.transform, "Chọn Độ Khó", new Vector2(0f, 190f), new Vector2(760f, 80f));
            var easyButton = CreateButton(difficultyScreen.transform, "EasyButton", "Dễ", new Vector2(0f, 90f), new Vector2(520f, 64f));
            var normalButton = CreateButton(difficultyScreen.transform, "NormalButton", "Trung bình", new Vector2(0f, 10f), new Vector2(520f, 64f));
            var hardButton = CreateButton(difficultyScreen.transform, "HardButton", "Khó", new Vector2(0f, -70f), new Vector2(520f, 64f));
            var backButton = CreateButton(difficultyScreen.transform, "BackButton", "Quay lại", new Vector2(0f, -170f), new Vector2(520f, 64f));
            Assign(difficultyView, "_easyButton", easyButton);
            Assign(difficultyView, "_normalButton", normalButton);
            Assign(difficultyView, "_hardButton", hardButton);
            Assign(difficultyView, "_backButton", backButton);

            difficultyScreen.SetActive(false);

            EditorSceneManager.SaveScene(scene, HomeScenePath);
        }

        private static void CreateGameplayScene()
        {
            if (CheckSceneAlreadyUpdated(GameplayScenePath, "SetupVersionMarker_v4"))
            {
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Gameplay";

            new GameObject("SetupVersionMarker_v4");

            CreateCamera();
            CreateEventSystem();

            var scope = new GameObject("GameplayLifetimeScope");
            scope.AddComponent<JigsawVina.Presentation.Screens.GameplayLifetimeScope>();

            var canvas = CreateCanvas("GameplayCanvas");
            var playingScreen = CreateScreen(canvas.transform, "PuzzlePlayingScreen");
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

            var backButton = CreateButton(topBar.transform, "BackButton", "Quay lại", new Vector2(-800f, 0f), new Vector2(200f, 60f));
            var titleText = AddHeader(topBar.transform, "Hồ Gươm - Dễ", new Vector2(-400f, 0f), new Vector2(400f, 60f));
            var timerText = AddHeader(topBar.transform, "Thời gian: 00:00", new Vector2(0f, 0f), new Vector2(300f, 60f));
            var previewOpacityText = AddHeader(topBar.transform, "Ảnh gốc 20%", new Vector2(275f, 0f), new Vector2(180f, 60f));
            previewOpacityText.fontSize = 22f;
            var previewOpacitySlider = CreateSlider(topBar.transform, "PreviewOpacitySlider", new Vector2(435f, 0f), new Vector2(150f, 30f), 0.2f);
            var hintButton = CreateButton(topBar.transform, "HintButton", "Gợi ý", new Vector2(590f, 0f), new Vector2(150f, 60f));
            var returnToTrayButton = CreateButton(topBar.transform, "ReturnToTrayButton", "Xếp lại", new Vector2(755f, 0f), new Vector2(150f, 60f));
            var cheatButton = CreateButton(topBar.transform, "CheatWinButton", "Debug Win", new Vector2(900f, 0f), new Vector2(120f, 60f));

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

            Assign(boardView, "_previewImage", previewOverlayImage);
            Assign(boardView, "_lockedPiecesContainer", lockedPiecesRect);

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

            Assign(playingView, "_titleText", titleText);
            Assign(playingView, "_timerText", timerText);
            Assign(playingView, "_previewOpacityText", previewOpacityText);
            Assign(playingView, "_backButton", backButton);
            Assign(playingView, "_previewOpacitySlider", previewOpacitySlider);
            Assign(playingView, "_hintButton", hintButton);
            Assign(playingView, "_returnToTrayButton", returnToTrayButton);
            Assign(playingView, "_cheatWinButton", cheatButton);
            Assign(playingView, "_boardView", boardView);
            Assign(playingView, "_trayContent", contentRect);
            Assign(playingView, "_dragContainer", dragContainerRect);
            Assign(playingView, "_canvas", canvas);
            dragContainerRect.SetAsLastSibling();

            var rewardScreen = CreateScreen(canvas.transform, "RewardSummaryScreen");
            var rewardView = rewardScreen.AddComponent<RewardSummaryView>();

            var starsText = AddHeader(rewardScreen.transform, "Stars: 0", new Vector2(0f, 120f), new Vector2(600f, 80f));
            var coinsText = AddHeader(rewardScreen.transform, "Coins Earned: 0", new Vector2(0f, 40f), new Vector2(600f, 80f));
            var returnButton = CreateButton(rewardScreen.transform, "ReturnHomeButton", "Return Home", new Vector2(0f, -100f), new Vector2(300f, 60f));
            Assign(rewardView, "_starsText", starsText);
            Assign(rewardView, "_coinsText", coinsText);
            Assign(rewardView, "_returnButton", returnButton);

            rewardScreen.SetActive(false);

            EditorSceneManager.SaveScene(scene, GameplayScenePath);
        }

        private static void ConfigureBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(HomeScenePath, true),
                new EditorBuildSettingsScene(GameplayScenePath, true)
            };
        }

        private static void CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.09f, 0.1f);
            cameraObject.tag = "MainCamera";
        }

        private static void CreateEventSystem()
        {
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            var inputModuleType = Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputModuleType != null)
            {
                eventSystem.AddComponent(inputModuleType);
            }
            else
            {
                eventSystem.AddComponent<StandaloneInputModule>();
            }
        }

        private static Canvas CreateCanvas(string name)
        {
            var canvasObject = new GameObject(name);
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static GameObject CreateScreen(Transform parent, string name)
        {
            var screen = new GameObject(name, typeof(RectTransform));
            screen.transform.SetParent(parent, false);
            var rect = (RectTransform)screen.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return screen;
        }

        private static TMP_Text AddHeader(Transform parent, string text, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            var textObject = new GameObject(text.Replace(" ", string.Empty) + "Text", typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
            var rect = (RectTransform)textObject.transform;
            rect.sizeDelta = sizeDelta;
            rect.anchoredPosition = anchoredPosition;

            var label = textObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = 32f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            return label;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            var buttonObject = new GameObject(name, typeof(RectTransform));
            buttonObject.transform.SetParent(parent, false);
            var rect = (RectTransform)buttonObject.transform;
            rect.sizeDelta = sizeDelta;
            rect.anchoredPosition = anchoredPosition;

            var image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.18f, 0.42f, 0.75f);
            var button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;

            var textObject = new GameObject("Label", typeof(RectTransform));
            textObject.transform.SetParent(buttonObject.transform, false);
            var textRect = (RectTransform)textObject.transform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var text = textObject.AddComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = 24f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.raycastTarget = false;

            return button;
        }

        private static Slider CreateSlider(Transform parent, string name, Vector2 anchoredPosition, Vector2 sizeDelta, float value)
        {
            var sliderObject = new GameObject(name, typeof(RectTransform));
            sliderObject.transform.SetParent(parent, false);
            var sliderRect = (RectTransform)sliderObject.transform;
            sliderRect.sizeDelta = sizeDelta;
            sliderRect.anchoredPosition = anchoredPosition;

            var backgroundObject = new GameObject("Background", typeof(RectTransform), typeof(Image));
            backgroundObject.transform.SetParent(sliderObject.transform, false);
            var backgroundRect = (RectTransform)backgroundObject.transform;
            backgroundRect.anchorMin = new Vector2(0f, 0.25f);
            backgroundRect.anchorMax = new Vector2(1f, 0.75f);
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;
            backgroundObject.GetComponent<Image>().color = new Color(0.15f, 0.18f, 0.22f);

            var fillAreaObject = new GameObject("Fill Area", typeof(RectTransform));
            fillAreaObject.transform.SetParent(sliderObject.transform, false);
            var fillAreaRect = (RectTransform)fillAreaObject.transform;
            fillAreaRect.anchorMin = new Vector2(0f, 0.25f);
            fillAreaRect.anchorMax = new Vector2(1f, 0.75f);
            fillAreaRect.offsetMin = new Vector2(8f, 0f);
            fillAreaRect.offsetMax = new Vector2(-8f, 0f);

            var fillObject = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillObject.transform.SetParent(fillAreaObject.transform, false);
            var fillRect = (RectTransform)fillObject.transform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            var fillImage = fillObject.GetComponent<Image>();
            fillImage.color = new Color(0.18f, 0.65f, 0.95f);

            var handleAreaObject = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleAreaObject.transform.SetParent(sliderObject.transform, false);
            var handleAreaRect = (RectTransform)handleAreaObject.transform;
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.offsetMin = new Vector2(8f, 0f);
            handleAreaRect.offsetMax = new Vector2(-8f, 0f);

            var handleObject = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            handleObject.transform.SetParent(handleAreaObject.transform, false);
            var handleRect = (RectTransform)handleObject.transform;
            handleRect.sizeDelta = new Vector2(24f, 36f);
            var handleImage = handleObject.GetComponent<Image>();
            handleImage.color = Color.white;

            var slider = sliderObject.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;
            slider.direction = Slider.Direction.LeftToRight;
            slider.SetValueWithoutNotify(value);
            return slider;
        }

        private static void Assign(UnityEngine.Object target, string fieldName, UnityEngine.Object value)
        {
            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(fieldName);
            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
