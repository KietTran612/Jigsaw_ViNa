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

        [MenuItem("JigsawVina/Setup Thin Vertical Slice Scenes")]
        public static void Setup()
        {
            EnsureFolders();
            var projectScopePrefab = CreateProjectLifetimeScopePrefab();
            ConfigureVContainerSettings(projectScopePrefab);
            CreateHomeScene();
            CreateGameplayScene();
            ConfigureBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[JigsawVina] Thin vertical slice scene setup complete.");
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

        private static void CreateHomeScene()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(HomeScenePath) != null)
            {
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Home";

            CreateCamera();
            CreateEventSystem();

            var scope = new GameObject("HomeLifetimeScope");
            scope.AddComponent<JigsawVina.Presentation.Screens.HomeLifetimeScope>();

            var canvas = CreateCanvas("HomeCanvas");
            var pictureScreen = CreateScreen(canvas.transform, "PictureSelectScreen");
            var pictureView = pictureScreen.AddComponent<PictureSelectView>();

            AddHeader(pictureScreen.transform, "Chon tranh", new Vector2(0f, 170f));
            var pic1Button = CreateButton(pictureScreen.transform, "Ho GuomButton", "Ho Guom", new Vector2(0f, 70f));
            var pic2Button = CreateButton(pictureScreen.transform, "Ha LongButton", "Ha Long", new Vector2(0f, -10f));
            Assign(pictureView, "_pic1Button", pic1Button);
            Assign(pictureView, "_pic2Button", pic2Button);

            var difficultyScreen = CreateScreen(canvas.transform, "DifficultySelectScreen");
            var difficultyView = difficultyScreen.AddComponent<DifficultySelectView>();

            AddHeader(difficultyScreen.transform, "Chon do kho", new Vector2(0f, 190f));
            var easyButton = CreateButton(difficultyScreen.transform, "EasyButton", "Easy", new Vector2(0f, 90f));
            var normalButton = CreateButton(difficultyScreen.transform, "NormalButton", "Normal", new Vector2(0f, 10f));
            var hardButton = CreateButton(difficultyScreen.transform, "HardButton", "Hard", new Vector2(0f, -70f));
            var backButton = CreateButton(difficultyScreen.transform, "BackButton", "Back", new Vector2(0f, -170f));
            Assign(difficultyView, "_easyButton", easyButton);
            Assign(difficultyView, "_normalButton", normalButton);
            Assign(difficultyView, "_hardButton", hardButton);
            Assign(difficultyView, "_backButton", backButton);

            EditorSceneManager.SaveScene(scene, HomeScenePath);
        }

        private static void CreateGameplayScene()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(GameplayScenePath) != null)
            {
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Gameplay";

            CreateCamera();
            CreateEventSystem();

            var scope = new GameObject("GameplayLifetimeScope");
            scope.AddComponent<JigsawVina.Presentation.Screens.GameplayLifetimeScope>();

            var canvas = CreateCanvas("GameplayCanvas");
            var playingScreen = CreateScreen(canvas.transform, "PuzzlePlayingScreen");
            var playingView = playingScreen.AddComponent<PuzzlePlayingView>();

            var titleText = AddHeader(playingScreen.transform, "Playing", new Vector2(0f, 160f));
            var cheatButton = CreateButton(playingScreen.transform, "CheatWinButton", "Cheat Win", new Vector2(0f, -20f));
            Assign(playingView, "_titleText", titleText);
            Assign(playingView, "_cheatWinButton", cheatButton);

            var rewardScreen = CreateScreen(canvas.transform, "RewardSummaryScreen");
            var rewardView = rewardScreen.AddComponent<RewardSummaryView>();

            var starsText = AddHeader(rewardScreen.transform, "Stars: 0", new Vector2(0f, 120f));
            var coinsText = AddHeader(rewardScreen.transform, "Coins Earned: 0", new Vector2(0f, 40f));
            var returnButton = CreateButton(rewardScreen.transform, "ReturnHomeButton", "Return Home", new Vector2(0f, -100f));
            Assign(rewardView, "_starsText", starsText);
            Assign(rewardView, "_coinsText", coinsText);
            Assign(rewardView, "_returnButton", returnButton);

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
            scaler.referenceResolution = new Vector2(1080f, 1920f);
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

        private static TMP_Text AddHeader(Transform parent, string text, Vector2 anchoredPosition)
        {
            var textObject = new GameObject(text.Replace(" ", string.Empty) + "Text", typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
            var rect = (RectTransform)textObject.transform;
            rect.sizeDelta = new Vector2(760f, 80f);
            rect.anchoredPosition = anchoredPosition;

            var label = textObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = 42f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            return label;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition)
        {
            var buttonObject = new GameObject(name, typeof(RectTransform));
            buttonObject.transform.SetParent(parent, false);
            var rect = (RectTransform)buttonObject.transform;
            rect.sizeDelta = new Vector2(520f, 64f);
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
            text.fontSize = 28f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.raycastTarget = false;

            return button;
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
