#if UNITY_EDITOR
using System;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JigsawVina.Editor
{
    internal static class ThinVerticalSliceUiFactory
    {
        public static void CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.09f, 0.1f);
            cameraObject.tag = "MainCamera";
        }

        public static void CreateEventSystem()
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

        public static Canvas CreateCanvas(string name)
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

        public static GameObject CreateScreen(Transform parent, string name)
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

        public static TMP_Text AddHeader(Transform parent, string text, Vector2 anchoredPosition, Vector2 sizeDelta)
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

        public static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition, Vector2 sizeDelta)
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

        public static Slider CreateSlider(Transform parent, string name, Vector2 anchoredPosition, Vector2 sizeDelta, float value)
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

        public static void Assign(UnityEngine.Object target, string fieldName, UnityEngine.Object value)
        {
            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(fieldName);
            if (property != null)
            {
                property.objectReferenceValue = value;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
            else
            {
                Debug.LogWarning($"[JigsawVina] Field '{fieldName}' not found on '{target.GetType().Name}'. Skipping assignment.");
            }
        }

        public static GameObject CreateLockIcon(Transform buttonTransform)
        {
            var lockIcon = new GameObject("LockIcon", typeof(RectTransform));
            lockIcon.transform.SetParent(buttonTransform, false);
            var rect = (RectTransform)lockIcon.transform;
            rect.anchorMin = new Vector2(1f, 0.5f);
            rect.anchorMax = new Vector2(1f, 0.5f);
            rect.pivot = new Vector2(1f, 0.5f);
            rect.anchoredPosition = new Vector2(-15f, 0f);
            rect.sizeDelta = new Vector2(24f, 24f);

            var image = lockIcon.AddComponent<Image>();
            image.color = new Color(0.9f, 0.2f, 0.2f);
            image.raycastTarget = false;
            return lockIcon;
        }

        public static TextMeshProUGUI CreateAchievementText(Transform parent, string name, Vector2 anchoredPosition)
        {
            var textObject = new GameObject(name, typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
            var rect = (RectTransform)textObject.transform;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(520f, 20f);

            var text = textObject.AddComponent<TextMeshProUGUI>();
            text.text = "";
            text.fontSize = 14f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = new Color(0.8f, 0.8f, 0.8f);
            text.raycastTarget = false;
            return text;
        }

        public static Text AddText(Transform parent, string text, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            var textObject = new GameObject("Text_" + text.Replace(" ", string.Empty), typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
            var rect = (RectTransform)textObject.transform;
            rect.sizeDelta = sizeDelta;
            rect.anchoredPosition = anchoredPosition;

            var label = textObject.AddComponent<Text>();
            label.text = text;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (label.font == null)
            {
                label.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            label.fontSize = 24;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            return label;
        }

        public static Toggle CreateToggle(Transform parent, string name, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            var toggleGo = new GameObject(name, typeof(RectTransform), typeof(Toggle));
            toggleGo.transform.SetParent(parent, false);
            var rect = (RectTransform)toggleGo.transform;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;

            var toggle = toggleGo.GetComponent<Toggle>();

            // Background
            var bgGo = new GameObject("Background", typeof(RectTransform), typeof(Image));
            bgGo.transform.SetParent(toggleGo.transform, false);
            var bgRect = (RectTransform)bgGo.transform;
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImage = bgGo.GetComponent<Image>();
            bgImage.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            toggle.targetGraphic = bgImage;

            // Checkmark
            var checkGo = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
            checkGo.transform.SetParent(bgGo.transform, false);
            var checkRect = (RectTransform)checkGo.transform;
            checkRect.anchorMin = new Vector2(0.5f, 0.5f);
            checkRect.anchorMax = new Vector2(0.5f, 0.5f);
            checkRect.pivot = new Vector2(0.5f, 0.5f);
            checkRect.anchoredPosition = Vector2.zero;
            checkRect.sizeDelta = new Vector2(sizeDelta.x - 10f, sizeDelta.y - 10f);
            var checkImage = checkGo.GetComponent<Image>();
            checkImage.color = new Color(0.18f, 0.65f, 0.95f);
            toggle.graphic = checkImage;

            return toggle;
        }

        public static TMP_Dropdown CreateDropdown(Transform parent, string name, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            var dropdownGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(TMP_Dropdown));
            dropdownGo.transform.SetParent(parent, false);
            var rect = (RectTransform)dropdownGo.transform;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;

            var dropdown = dropdownGo.GetComponent<TMP_Dropdown>();
            var bgImage = dropdownGo.GetComponent<Image>();
            bgImage.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            dropdown.targetGraphic = bgImage;

            // Caption Text
            var captionGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            captionGo.transform.SetParent(dropdownGo.transform, false);
            var captionRect = (RectTransform)captionGo.transform;
            captionRect.anchorMin = Vector2.zero;
            captionRect.anchorMax = Vector2.one;
            captionRect.offsetMin = new Vector2(10f, 0f);
            captionRect.offsetMax = new Vector2(-30f, 0f);
            var captionText = captionGo.GetComponent<TextMeshProUGUI>();
            captionText.color = Color.white;
            captionText.fontSize = 20f;
            captionText.alignment = TextAlignmentOptions.Left;
            dropdown.captionText = captionText;

            // Arrow
            var arrowGo = new GameObject("Arrow", typeof(RectTransform), typeof(Image));
            arrowGo.transform.SetParent(dropdownGo.transform, false);
            var arrowRect = (RectTransform)arrowGo.transform;
            arrowRect.anchorMin = new Vector2(1f, 0.5f);
            arrowRect.anchorMax = new Vector2(1f, 0.5f);
            arrowRect.pivot = new Vector2(1f, 0.5f);
            arrowRect.anchoredPosition = new Vector2(-10f, 0f);
            arrowRect.sizeDelta = new Vector2(20f, 20f);
            arrowGo.GetComponent<Image>().color = Color.white;

            // Template
            var templateGo = new GameObject("Template", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            templateGo.transform.SetParent(dropdownGo.transform, false);
            var templateRect = (RectTransform)templateGo.transform;
            templateRect.anchorMin = new Vector2(0f, 0f);
            templateRect.anchorMax = new Vector2(1f, 0f);
            templateRect.pivot = new Vector2(0.5f, 1f);
            templateRect.anchoredPosition = new Vector2(0f, -2f);
            templateRect.sizeDelta = new Vector2(0f, 150f);
            templateGo.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 1f);
            templateGo.SetActive(false);

            var scrollRect = templateGo.GetComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            dropdown.template = templateRect;

            // Viewport
            var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(Mask), typeof(Image));
            viewportGo.transform.SetParent(templateGo.transform, false);
            var viewportRect = (RectTransform)viewportGo.transform;
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            viewportGo.GetComponent<Mask>().showMaskGraphic = false;
            viewportGo.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.1f);

            scrollRect.viewport = viewportRect;

            // Content
            var contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(viewportGo.transform, false);
            var contentRect = (RectTransform)contentGo.transform;
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0f, 80f);

            scrollRect.content = contentRect;

            // Item
            var itemGo = new GameObject("Item", typeof(RectTransform), typeof(Toggle));
            itemGo.transform.SetParent(contentRect.transform, false);
            var itemRect = (RectTransform)itemGo.transform;
            itemRect.anchorMin = new Vector2(0f, 0.5f);
            itemRect.anchorMax = new Vector2(1f, 0.5f);
            itemRect.sizeDelta = new Vector2(0f, 40f);

            var itemToggle = itemGo.GetComponent<Toggle>();

            // Item Background
            var itemBgGo = new GameObject("Item Background", typeof(RectTransform), typeof(Image));
            itemBgGo.transform.SetParent(itemGo.transform, false);
            var itemBgRect = (RectTransform)itemBgGo.transform;
            itemBgRect.anchorMin = Vector2.zero;
            itemBgRect.anchorMax = Vector2.one;
            itemBgRect.offsetMin = Vector2.zero;
            itemBgRect.offsetMax = Vector2.zero;
            var itemBgImage = itemBgGo.GetComponent<Image>();
            itemBgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            itemToggle.targetGraphic = itemBgImage;

            // Item Checkmark
            var itemCheckGo = new GameObject("Item Checkmark", typeof(RectTransform), typeof(Image));
            itemCheckGo.transform.SetParent(itemGo.transform, false);
            var itemCheckRect = (RectTransform)itemCheckGo.transform;
            itemCheckRect.anchorMin = new Vector2(0f, 0.5f);
            itemCheckRect.anchorMax = new Vector2(0f, 0.5f);
            itemCheckRect.pivot = new Vector2(0f, 0.5f);
            itemCheckRect.anchoredPosition = new Vector2(10f, 0f);
            itemCheckRect.sizeDelta = new Vector2(20f, 20f);
            var itemCheckImage = itemCheckGo.GetComponent<Image>();
            itemCheckImage.color = Color.green;
            itemToggle.graphic = itemCheckImage;

            // Item Label
            var itemLabelGo = new GameObject("Item Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            itemLabelGo.transform.SetParent(itemGo.transform, false);
            var itemLabelRect = (RectTransform)itemLabelGo.transform;
            itemLabelRect.anchorMin = Vector2.zero;
            itemLabelRect.anchorMax = Vector2.one;
            itemLabelRect.offsetMin = new Vector2(40f, 0f);
            itemLabelRect.offsetMax = new Vector2(-10f, 0f);
            var itemLabelText = itemLabelGo.GetComponent<TextMeshProUGUI>();
            itemLabelText.color = Color.white;
            itemLabelText.fontSize = 18f;
            itemLabelText.alignment = TextAlignmentOptions.Left;
            dropdown.itemText = itemLabelText;

            dropdown.options.Clear();
            dropdown.options.Add(new TMP_Dropdown.OptionData("Tiếng Việt"));
            dropdown.options.Add(new TMP_Dropdown.OptionData("English"));

            return dropdown;
        }
    }
}
#endif
