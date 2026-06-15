# Kế hoạch giải quyết review Dynamic Home UI (Cập nhật v1)

Ghi nhận các phản hồi review [P2] và [P3], kế hoạch này tập trung vào việc sửa đổi kích thước card để không bị cắt ngang, loại bỏ việc gọi thủ công vòng đời Unity `Awake()` trong code runtime, bổ sung kiểm thử bố cục (layout) cho cảnh Home, và cập nhật trạng thái đồng bộ của `task.md`.

## Proposed Changes

---

### [Component Name] Layout & Prefab

#### [MODIFY] [ThinVerticalSliceSceneSetup.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Editor/ThinVerticalSliceSceneSetup.cs)
* Sửa đổi phương thức `CreatePictureSelectCardPrefabForTask28()`:
  * Thay đổi kích thước bề rộng của prefab card từ `640f` thành `560f` (`rootRect.sizeDelta = new Vector2(560f, 120f);`).
  * Kích thước này sẽ khớp hoàn toàn bên trong ScrollView rộng `600f` với khoảng cách lề `20f` mỗi bên, loại bỏ hiện tượng bị Mask cắt ngang (clip).

---

### [Component Name] Presentation & Views

#### [MODIFY] [PictureSelectView.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/PictureSelectView.cs)
* Loại bỏ đoạn code gọi trực tiếp hàm `Awake()` của card:
  ```csharp
  // Xóa bỏ:
  if (!Application.isPlaying)
  {
      cardInstance.Awake();
  }
  ```

#### [MODIFY] [PictureSelectCard.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/PictureSelectCard.cs)
* Đổi phạm vi truy cập từ `internal void Awake()` trở về lại `private void Awake()` để đảm bảo tính đóng gói của vòng đời Unity.

---

### [Component Name] Automated Tests

#### [MODIFY] [PictureSelectFlowTests.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Tests/PictureSelectFlowTests.cs)
* Thêm hàm helper dùng Reflection để kích hoạt `Awake()` cho các card giả lập trong môi trường kiểm thử EditMode:
  ```csharp
  private void TriggerCardAwake(PictureSelectCard card)
  {
      var method = typeof(PictureSelectCard).GetMethod("Awake", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
      method?.Invoke(card, null);
  }
  ```
* Gọi `TriggerCardAwake(card)` trên các card được khởi tạo trong các test case kích hoạt click button (như `CardClick_InvokesOnPictureSelectedWithCorrectId`, `Presenter_OnDispose_UnsubscribesFromView`, `FlowController_OnDispose_UnsubscribesFromView`).

#### [MODIFY] [LifetimeScopeRegistrationTests.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Tests/LifetimeScopeRegistrationTests.cs)
* Mở rộng kiểm tra `HomeScene_PictureSelectView_IsWiredCorrectly()` để kiểm chứng các thông số bố cục:
  * Xác minh `VerticalLayoutGroup` có `spacing = 20`.
  * Xác minh các cờ sizing đều được đặt đúng: `childControlWidth = false`, `childControlHeight = false`, `childForceExpandWidth = false`, `childForceExpandHeight = false`.
  * Xác minh `childAlignment = TextAnchor.UpperCenter`.
  * Xác minh `ContentSizeFitter` có `verticalFit = ContentSizeFitter.FitMode.PreferredSize`.
  * Thêm Assertion so sánh chiều rộng: `cardRect.rect.width <= scrollViewRect.rect.width` để tránh việc card tràn mép ScrollView gây lỗi cắt UI trong tương lai.

---

### [Component Name] Documentation

#### [MODIFY] [task.md](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/docs/plans/task.md)
* Loại bỏ ghi chú cũ lỗi thời dưới mục `## Pending` liên quan đến việc Tasks 28-34 chưa bắt đầu.

---

## Verification Plan

### Automated Tests
* Chạy bộ EditMode tests để xác nhận mọi kiểm thử logic và giao diện đều vượt qua:
  * Menu `JigsawVina/Run EditMode Tests`
* Chạy bộ PlayMode tests để đảm bảo không xảy ra hồi quy chức năng:
  * Menu `JigsawVina/Run PlayMode Tests`

### Idempotency & Visual Verification
1. Chọn `JigsawVina/Task 28/Create Picture Select Card Prefab` để tái tạo Prefab với kích thước mới `560x120`.
2. Chọn `JigsawVina/Setup Thin Vertical Slice Scenes` để tự động dựng lại cảnh `Home.unity`.
3. Kiểm tra trực quan trong Unity Editor để đảm bảo card đã co lại `560` và nằm gọn gàng bên trong `PictureScrollView` rộng `600`, không bị clipping.
4. Chạy lại Setup lần thứ 2 để đảm bảo tính ổn định và không làm dirty file.
