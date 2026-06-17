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
            ThinVerticalSliceHomeSceneBuilder.CreateHomeScene(HomeScenePath);
            ThinVerticalSliceGameplaySceneBuilder.CreateGameplayScene(GameplayScenePath);
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

        internal static bool CheckSceneAlreadyUpdated(string scenePath, string markerName)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
            {
                return false;
            }

            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            var marker = GameObject.Find(markerName);
            return marker != null;
        }

        private static void ConfigureBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(HomeScenePath, true),
                new EditorBuildSettingsScene(GameplayScenePath, true)
            };
        }

        [MenuItem("JigsawVina/Task 38/Create Picture Select Card Prefab")]
        public static void CreatePictureSelectCardPrefabForTask38()
        {
            const string prefabPath = PrefabsFolder + "/PictureSelectCard.prefab";

            var root = new GameObject(
                "PictureSelectCard",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button),
                typeof(PictureSelectCard));

            try
            {
                var rootRect = (RectTransform)root.transform;
                rootRect.sizeDelta = new Vector2(560f, 120f);

                var background = root.GetComponent<Image>();
                background.color = new Color(0.15f, 0.38f, 0.72f, 1f);

                var button = root.GetComponent<Button>();
                button.targetGraphic = background;

                var thumbnailObject = new GameObject(
                    "Thumbnail",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image));
                thumbnailObject.transform.SetParent(root.transform, false);

                var thumbnailRect = (RectTransform)thumbnailObject.transform;
                thumbnailRect.anchorMin = new Vector2(0f, 0.5f);
                thumbnailRect.anchorMax = new Vector2(0f, 0.5f);
                thumbnailRect.pivot = new Vector2(0.5f, 0.5f);
                thumbnailRect.anchoredPosition = new Vector2(80f, 0f);
                thumbnailRect.sizeDelta = new Vector2(120f, 80f);

                var thumbnailImage = thumbnailObject.GetComponent<Image>();
                thumbnailImage.preserveAspect = true;

                var labelObject = new GameObject(
                    "Label",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(TextMeshProUGUI));
                labelObject.transform.SetParent(root.transform, false);

                var labelRect = (RectTransform)labelObject.transform;
                labelRect.anchorMin = new Vector2(0f, 0f);
                labelRect.anchorMax = new Vector2(1f, 1f);
                labelRect.offsetMin = new Vector2(160f, 20f);
                labelRect.offsetMax = new Vector2(-30f, -20f);

                var label = labelObject.GetComponent<TextMeshProUGUI>();
                label.text = "Picture Name";
                label.fontSize = 24f;
                label.alignment = TextAlignmentOptions.MidlineLeft;

                var lockOverlay = new GameObject(
                    "LockOverlay",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image));
                lockOverlay.transform.SetParent(root.transform, false);
                var overlayRect = (RectTransform)lockOverlay.transform;
                overlayRect.anchorMin = Vector2.zero;
                overlayRect.anchorMax = Vector2.one;
                overlayRect.offsetMin = Vector2.zero;
                overlayRect.offsetMax = Vector2.zero;
                var overlayImage = lockOverlay.GetComponent<Image>();
                overlayImage.color = new Color(0.04f, 0.06f, 0.1f, 0.88f);

                var lockLabelObject = new GameObject(
                    "LockIcon",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(TextMeshProUGUI));
                lockLabelObject.transform.SetParent(lockOverlay.transform, false);
                var lockLabelRect = (RectTransform)lockLabelObject.transform;
                lockLabelRect.anchorMin = new Vector2(0f, 0.5f);
                lockLabelRect.anchorMax = new Vector2(0f, 0.5f);
                lockLabelRect.anchoredPosition = new Vector2(70f, 32f);
                lockLabelRect.sizeDelta = new Vector2(110f, 36f);
                var lockLabel = lockLabelObject.GetComponent<TextMeshProUGUI>();
                lockLabel.text = "LOCK";
                lockLabel.fontSize = 20f;
                lockLabel.fontStyle = FontStyles.Bold;
                lockLabel.alignment = TextAlignmentOptions.Center;
                lockLabel.color = new Color(1f, 0.82f, 0.25f);
                lockLabel.raycastTarget = false;

                var keyItemPanel = new GameObject(
                    "KeyItemPanel",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image));
                keyItemPanel.transform.SetParent(lockOverlay.transform, false);
                var keyItemPanelRect = (RectTransform)keyItemPanel.transform;
                keyItemPanelRect.anchorMin = new Vector2(0f, 0f);
                keyItemPanelRect.anchorMax = new Vector2(1f, 1f);
                keyItemPanelRect.offsetMin = new Vector2(125f, 8f);
                keyItemPanelRect.offsetMax = new Vector2(-135f, -8f);
                var keyItemPanelImage = keyItemPanel.GetComponent<Image>();
                keyItemPanelImage.color = new Color(0.12f, 0.18f, 0.28f, 0.95f);
                keyItemPanelImage.raycastTarget = false;

                var hintObject = new GameObject(
                    "MissingItemsHint",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(TextMeshProUGUI));
                hintObject.transform.SetParent(keyItemPanel.transform, false);
                var hintRect = (RectTransform)hintObject.transform;
                hintRect.anchorMin = Vector2.zero;
                hintRect.anchorMax = Vector2.one;
                hintRect.offsetMin = new Vector2(10f, 4f);
                hintRect.offsetMax = new Vector2(-10f, -4f);
                var hintText = hintObject.GetComponent<TextMeshProUGUI>();
                hintText.text = "Missing item source hint";
                hintText.fontSize = 16f;
                hintText.alignment = TextAlignmentOptions.MidlineLeft;
                hintText.color = Color.white;
                hintText.raycastTarget = false;

                var unlockButton = ThinVerticalSliceUiFactory.CreateButton(
                    lockOverlay.transform,
                    "UnlockButton",
                    "Unlock",
                    new Vector2(215f, 0f),
                    new Vector2(120f, 52f));
                var unlockRect = (RectTransform)unlockButton.transform;
                unlockRect.anchorMin = new Vector2(0.5f, 0.5f);
                unlockRect.anchorMax = new Vector2(0.5f, 0.5f);

                var card = root.GetComponent<PictureSelectCard>();
                ThinVerticalSliceUiFactory.Assign(card, "_button", button);
                ThinVerticalSliceUiFactory.Assign(card, "_thumbnailImage", thumbnailImage);
                ThinVerticalSliceUiFactory.Assign(card, "_displayNameText", label);
                ThinVerticalSliceUiFactory.Assign(card, "_lockOverlay", lockOverlay);
                ThinVerticalSliceUiFactory.Assign(card, "_missingItemsHintText", hintText);
                ThinVerticalSliceUiFactory.Assign(card, "_unlockButton", unlockButton);

                lockOverlay.SetActive(false);

                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log($"[JigsawVina] Created PictureSelectCard prefab at {prefabPath}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }
    }
}
#endif
