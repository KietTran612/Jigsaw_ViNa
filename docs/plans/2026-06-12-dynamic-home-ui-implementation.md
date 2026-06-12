# Kế hoạch triển khai Home UI kết nối động với Static Data (Cập nhật v6)

> **For Antigravity:** REQUIRED WORKFLOW: Use `.agent/workflows/execute-plan.md` to execute this plan in single-flow mode.

**Goal:** Chuyển đổi giao diện màn hình Home (Picture Selection) sang danh sách cuộn động (Scroll View) sử dụng Prefab `PictureSelectCard` tái sử dụng, tự động hiển thị 5 tranh từ static data, đảm bảo không rò rỉ bộ nhớ, tối ưu hóa runtime và log lỗi chính xác.

**Architecture:** `PictureSelectPresenter` sẽ lấy danh sách tranh từ `IStaticDataService` và điều phối sang `PictureSelectView` để vẽ UI động. View sẽ quản lý việc khởi tạo/tiêu hủy các Card Button con từ Prefab và quản lý vòng đời bộ nhớ sạch qua `IDisposable`.

**Tech Stack:** Unity uGUI, VContainer, TextMeshPro, C#

---

### Task 1: Tạo Component PictureSelectCard.cs và Tạo Prefab PictureSelectCard

**Files:**
* Create [NEW]: [PictureSelectCard.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/PictureSelectCard.cs)
* Create [NEW]: `Assets/JigsawVina/Prefabs/PictureSelectCard.prefab` (Thực hiện thủ công trong Editor)

> [!IMPORTANT]
> Tuyệt đối không tạo file `.meta` thủ công. Chờ Unity tự động biên dịch và sinh các file `.meta` tương ứng.

**Step 1: Viết mã nguồn cho PictureSelectCard.cs**
Tạo component quản lý trực tiếp giao diện card, sử dụng duy nhất component `Image` của uGUI cho cả thumbnail và background, gán listener an toàn và hỗ trợ `Unbind()`.

```csharp
using System;
using JigsawVina.Core.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace JigsawVina.Presentation.Screens
{
    public class PictureSelectCard : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _thumbnailImage;
        [SerializeField] private TMP_Text _displayNameText;

        private Action<int> _onClicked;
        private int _pictureId;

        private void Awake()
        {
            if (_button != null)
            {
                _button.onClick.AddListener(() => _onClicked?.Invoke(_pictureId));
            }
        }

        public void Bind(PictureConfig config, Action<int> onClicked)
        {
            _pictureId = config.Id;
            _onClicked = onClicked;

            if (_displayNameText != null)
            {
                _displayNameText.text = config.DisplayName;
            }

            if (_thumbnailImage != null)
            {
                if (string.IsNullOrEmpty(config.AssetPath))
                {
                    Debug.LogError($"[JigsawVina] Data error: Picture ID {config.Id} ({config.DisplayName}) has a null or empty AssetPath.");
                    _thumbnailImage.color = Color.gray;
                    return;
                }

                // Load ảnh chính làm thumbnail (Chấp nhận chi phí RAM ở MVP)
                var sprite = Resources.Load<Sprite>(config.AssetPath);
                if (sprite != null)
                {
                    _thumbnailImage.sprite = sprite;
                    _thumbnailImage.color = Color.white;
                }
                else
                {
                    Debug.LogError($"[JigsawVina] Resources error: Failed to load Sprite for Picture ID: {config.Id} ({config.DisplayName}) at path: '{config.AssetPath}'");
                    _thumbnailImage.color = Color.gray; // Placeholder
                }
            }
        }

        public void Unbind()
        {
            _onClicked = null;
            if (_thumbnailImage != null)
            {
                _thumbnailImage.sprite = null;
            }
        }
    }
}
```

**Step 2: Tạo Prefab PictureSelectCard.prefab thủ công bằng Editor**
1. Trong scene `Home.unity`, tạo một UI Button mới làm con của Canvas, đặt tên là `PictureSelectCard`.
2. Gán component `PictureSelectCard` vào GameObject này.
3. Tạo 2 GameObject con dưới `PictureSelectCard`:
   * `Thumbnail` có component **`Image`** (vị trí lệch trái, size 120x80).
   * `Label` có component **`TextMeshProUGUI`** (vị trí lệch phải, căn trái, size 24f).
4. Kéo thả tham chiếu của Button, Thumbnail và Label vào các trường `_button`, `_thumbnailImage`, và `_displayNameText` tương ứng trên Inspector (Thống nhất chỉ dùng `Image`).
5. Kéo thả GameObject `PictureSelectCard` từ Hierarchy vào thư mục `Assets/JigsawVina/Prefabs/` để tạo Prefab.
6. Xóa đối tượng này trong Hierarchy đi.

---

### Task 2: Cập nhật PictureSelectView.cs và validation an toàn

**Files:**
* Modify: [PictureSelectView.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/PictureSelectView.cs)

**Step 1: Thay đổi các trường từ button cũ sang tham chiếu Prefab và Container**
Thay đổi code `PictureSelectView.cs` để nhận `PictureSelectCard` prefab, validate null nghiêm ngặt, giải phóng card bằng `Unbind()` trước khi destroy và expose danh sách card `internal` cho unit test. Đảm bảo toàn bộ lệnh `using` đứng trước attribute `InternalsVisibleTo`.

```csharp
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JigsawVina.Core.Data;
using UnityEngine;

[assembly: InternalsVisibleTo("JigsawVina.Tests")]

namespace JigsawVina.Presentation.Screens
{
    public class PictureSelectView : MonoBehaviour
    {
        public event Action<int> OnPictureSelected;

        [SerializeField] private PictureSelectCard _cardPrefab;
        [SerializeField] private RectTransform _contentContainer;

        private readonly List<PictureSelectCard> _instantiatedCards = new();

        internal IReadOnlyList<PictureSelectCard> InstantiatedCards => _instantiatedCards;

        public void Setup(IReadOnlyList<PictureConfig> pictures)
        {
            ClearExistingCards();

            if (pictures == null || pictures.Count == 0)
            {
                Debug.LogError("[JigsawVina] PictureSelectView: Pictures list is null or empty.");
                return;
            }

            if (_cardPrefab == null)
            {
                Debug.LogError("[JigsawVina] UI error: Card Prefab is not assigned on PictureSelectView.");
                return;
            }

            if (_contentContainer == null)
            {
                Debug.LogError("[JigsawVina] UI error: Content Container is not assigned on PictureSelectView.");
                return;
            }

            foreach (var picture in pictures)
            {
                if (picture.Id <= 0)
                {
                    Debug.LogError($"[JigsawVina] Data error: Picture has an invalid ID ({picture.Id}).");
                    continue;
                }

                var cardInstance = Instantiate(_cardPrefab, _contentContainer, false);
                cardInstance.gameObject.name = $"PictureCard_{picture.Id}_{picture.IdString}";
                cardInstance.gameObject.SetActive(true);

                cardInstance.Bind(picture, id => OnPictureSelected?.Invoke(id));
                _instantiatedCards.Add(cardInstance);
            }
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        private void ClearExistingCards()
        {
            foreach (var card in _instantiatedCards)
            {
                if (card != null)
                {
                    card.Unbind();
                    Destroy(card.gameObject);
                }
            }
            _instantiatedCards.Clear();
        }
    }
}
```

---

### Task 3: Cập nhật PictureSelectPresenter.cs và quản lý lifecycle sạch

**Files:**
* Modify: [PictureSelectPresenter.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/PictureSelectPresenter.cs)

**Step 1: Triển khai IDisposable và nạp dữ liệu từ IStaticDataService**
Cập nhật constructor của `PictureSelectPresenter` để nạp dữ liệu tranh động từ static data, và implement `Dispose()` để hủy đăng ký sự kiện an toàn.

```csharp
using System;
using JigsawVina.Core.Services;
using UnityEngine;

namespace JigsawVina.Presentation.Screens
{
    public class PictureSelectPresenter : IDisposable
    {
        private readonly PictureSelectView _view;
        private readonly GameSessionService _sessionService;
        private readonly IStaticDataService _staticDataService;

        public PictureSelectPresenter(
            PictureSelectView view,
            GameSessionService sessionService,
            IStaticDataService staticDataService)
        {
            _view = view;
            _sessionService = sessionService;
            _staticDataService = staticDataService;

            _view.OnPictureSelected += HandlePictureSelected;

            Initialize();
        }

        private void Initialize()
        {
            var pictures = _staticDataService.GetAllPictures();
            if (pictures == null || pictures.Count == 0)
            {
                Debug.LogError("[JigsawVina] StaticData error: No pictures found in IStaticDataService.");
                return;
            }

            _view.Setup(pictures);
        }

        private void HandlePictureSelected(int pictureId)
        {
            _sessionService.SetSelectedPicture(pictureId);
        }

        public void Dispose()
        {
            if (_view != null)
            {
                _view.OnPictureSelected -= HandlePictureSelected;
            }
        }
    }
}
```

---

### Task 4: Cập nhật Home Lifetime Scope & HomeFlowController

**Files:**
* Modify: [HomeLifetimeScope.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/HomeLifetimeScope.cs)

**Step 1: Triển khai named handler và IDisposable cho FlowController**
Đảm bảo `HomeFlowController` quản lý vòng đời đăng ký sự kiện sạch sẽ, tránh dùng lambda vô danh.

```csharp
using System;
using VContainer;
using VContainer.Unity;

namespace JigsawVina.Presentation.Screens
{
    public class HomeLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<PictureSelectView>();
            builder.RegisterComponentInHierarchy<DifficultySelectView>();
            builder.Register<PictureSelectPresenter>(Lifetime.Singleton);
            builder.Register<DifficultySelectPresenter>(Lifetime.Singleton);
            builder.RegisterEntryPoint<HomeFlowController>();
        }
    }

    public class HomeFlowController : IStartable, IDisposable
    {
        private readonly PictureSelectView _pictureSelectView;
        private readonly DifficultySelectView _difficultySelectView;

        public HomeFlowController(
            PictureSelectView pictureSelectView,
            DifficultySelectView difficultySelectView,
            PictureSelectPresenter pictureSelectPresenter,
            DifficultySelectPresenter difficultySelectPresenter)
        {
            _pictureSelectView = pictureSelectView;
            _difficultySelectView = difficultySelectView;
            _ = pictureSelectPresenter;
            _ = difficultySelectPresenter;
        }

        public void Start()
        {
            _pictureSelectView.SetActive(true);
            _difficultySelectView.SetActive(false);

            _pictureSelectView.OnPictureSelected += HandlePictureSelected;

            if (_difficultySelectView.BackButton != null)
            {
                _difficultySelectView.BackButton.onClick.AddListener(HandleBackButtonClicked);
            }
        }

        private void HandlePictureSelected(int pictureId)
        {
            _pictureSelectView.SetActive(false);
            _difficultySelectView.SetActive(true);
        }

        private void HandleBackButtonClicked()
        {
            _difficultySelectView.SetActive(false);
            _pictureSelectView.SetActive(true);
        }

        public void Dispose()
        {
            if (_pictureSelectView != null)
            {
                _pictureSelectView.OnPictureSelected -= HandlePictureSelected;
            }

            if (_difficultySelectView != null && _difficultySelectView.BackButton != null)
            {
                _difficultySelectView.BackButton.onClick.RemoveListener(HandleBackButtonClicked);
            }
        }
    }
}
```

---

### Task 5: Cập nhật ThinVerticalSliceSceneSetup.cs (Regenerate Update)

**Files:**
* Modify: [ThinVerticalSliceSceneSetup.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Editor/ThinVerticalSliceSceneSetup.cs)

**Step 1: Thay đổi marker version và cập nhật logic tạo scene Home**
Đảm bảo khi regenerate scene Home, script sẽ tạo cấu trúc Scroll View và gán đúng Prefab `PictureSelectCard` được load qua AssetDatabase, không tạo lại các nút hardcode cũ.

1. Nâng cấp marker version kiểm tra lên `"SetupVersionMarker_v4"` tại dòng 135:
   * Sửa `CheckSceneAlreadyUpdated` cho màn Home kiểm tra `"SetupVersionMarker_v4"`.
   * Cập nhật `CreateHomeScene()` tạo GameObject marker `"SetupVersionMarker_v4"`.
2. Sửa đổi phương thức `CreateHomeScene()`:
   * **Không tạo lại các nút hardcode** (`Ho GuomButton` và `Ha LongButton`).
   * Tạo cấu trúc **Scroll View** dưới `PictureSelectScreen`:
     * Tạo `PictureScrollView` (Width = 600, Height = 600, Anchored Position = (0, -50)). Tắt Horizontal Scrollbar.
     * Tạo `Viewport` có `Mask` và `Image`.
     * Tạo `Content` có component `VerticalLayoutGroup` (Spacing = 20, Child Control Width/Height = false, Child Force Expand = false, Child Alignment = Upper Center).
     * Thêm component `ContentSizeFitter` (Vertical Fit = Preferred Size) vào `Content`.
   * Tải prefab `PictureSelectCard` qua `AssetDatabase.LoadAssetAtPath<PictureSelectCard>("Assets/JigsawVina/Prefabs/PictureSelectCard.prefab")`. Báo lỗi rõ ràng nếu prefab này không tồn tại.
   * Gán tham chiếu `_cardPrefab` và `_contentContainer` (trỏ đến `Content`) cho `PictureSelectView` bằng SerializedObject.

---

### Task 6: Viết EditMode Tests cho Home UI Flow & Scene Wiring

**Files:**
* Create [NEW]: [PictureSelectFlowTests.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Tests/PictureSelectFlowTests.cs)
* Modify: [LifetimeScopeRegistrationTests.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Tests/LifetimeScopeRegistrationTests.cs)

**Step 1: Tạo EditMode test kiểm thử View, Presenter & FlowController**
Viết kiểm thử tự động trong `PictureSelectFlowTests.cs`.
* **Để tránh lỗi LogError trong test**: Tại hàm `SetUp()`, ta **không gán** `_thumbnailImage` của prefab mock (giữ null). Nhờ đó logic `Bind()` sẽ bỏ qua việc load tài nguyên giả, không tạo ra LogError bất ngờ.
* Bổ sung đầy đủ các test case cho Presenter Dispose và FlowController Dispose. Test FlowController phải xác minh cả event chọn tranh và Back Button đều không còn thay đổi trạng thái màn hình sau `Dispose()`.
* Sửa `Setup_Twice_ClearsExistingCards` sang dạng `[UnityTest]` sử dụng coroutine và `yield return null` để kiểm chứng GameObject thực tế bị hủy triệt để khỏi `_container` sau khi frame kết thúc.
* Cập nhật `TearDown()` chỉ huỷ `_holder` và `_cardPrefab` để tránh nguy cơ huỷ nhầm test objects khác.

```csharp
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
```

**Step 2: Cập nhật LifetimeScopeRegistrationTests.cs để kiểm tra Scene Wiring & Scroll View duy nhất**
Thêm test EditMode load scene Home (additive) để xác nhận scene không còn nút cứng, các reference của `PictureSelectView` được wire đầy đủ và prefab có đủ component. Đồng thời xác định có đúng một Scroll View duy nhất được cấu hình đúng.

```csharp
        [Test]
        public void HomeScene_PictureSelectView_IsWiredCorrectly()
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Home.unity", UnityEditor.SceneManagement.OpenSceneMode.Additive);
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
            }
            finally
            {
                UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene, true);
            }
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
```

---

### Task 7: Cập nhật tài liệu dự án

**Files:**
* Modify: [task.md](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/docs/plans/task.md)
* Modify: [current-handoff.md](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/docs/plans/current-handoff.md)
* Modify: [index.md](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/docs/plans/index.md)

> [!IMPORTANT]
> Chờ Unity tự động sinh tất cả các file `.meta` cho các tệp mới và thay đổi, sau đó cập nhật tài liệu và kiểm tra compiler log trước khi đánh dấu hoàn thành. Không tự tạo `.meta` bằng tay.

---

## Verification Plan

### Automated Tests
Chạy bộ kiểm thử cụ thể bị ảnh hưởng để kiểm tra lỗi biên dịch/logic:
* Chạy bộ EditMode tests mới:
  * Run: `PictureSelectFlowTests`
* Chạy bộ EditMode tests cho registration & wiring:
  * Run: `LifetimeScopeRegistrationTests`

### Idempotency Verification
1. Trong Unity Editor, chọn `JigsawVina/Setup Thin Vertical Slice Scenes` để tự động regenerate lại Scene Home.
2. Kiểm tra giao diện Home hoạt động đúng (Scroll View và Prefab card gán chuẩn xác).
3. Chạy lại `JigsawVina/Setup Thin Vertical Slice Scenes` lần thứ 2.
4. Xác nhận lần chạy thứ 2 không tạo ra duplicate Scroll View, không nhân bản container, và không làm dirty file `Home.unity`.

### Manual Verification
1. Mở Scene `Home.unity` trong Unity Editor và bấm **Play**.
2. Xác nhận danh sách tranh cuộn động hiện ra đầy đủ 5 tranh từ file JSON tĩnh (House OldVillage 1, old village central 001...).
3. Kiểm tra xem mỗi card hiển thị đúng tên tranh và hình ảnh tương ứng.
4. Click thử vào tranh số 3 (*old village south 001*) hoặc tranh số 5 (*bridge 001*) (ID lớn hơn 2) để chứng minh dữ liệu động. Bắt đầu chơi và xác nhận gameplay load đúng tranh và các mảnh ghép hoạt động bình thường.
5. Kiểm tra log Console để đảm bảo không xuất hiện bất kỳ log lỗi hay cảnh báo nào.
