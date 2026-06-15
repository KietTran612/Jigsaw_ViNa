# Kế hoạch triển khai Hệ thống Tiến trình & Mở khóa (Meta-Progression & Unlocking)

Kế hoạch này được chia làm các giai đoạn (Phases) rõ ràng để ưu tiên thực hiện hệ thống cốt lõi **Picture Unlock Progression** trước, các hệ thống bổ trợ khác sẽ được đưa vào các Milestone tiếp theo.

---

## GIAI ĐOẠN 1: Picture Unlock Progression (Milestone 2 - Thực hiện ngay)

Đây là progression chính nhằm kết nối gameplay với việc mở khóa nội dung mới bằng Key Item và áp dụng cơ chế hướng dẫn người chơi mới (onboarding) thông qua chuỗi độ khó tuần tự.

### 1.1. Bổ sung Contract dữ liệu tĩnh (Static Data Contract)
Để hỗ trợ việc khóa/mở khóa và điều chỉnh cơ chế mở khóa độ khó tuần tự, chúng ta sẽ mở rộng các lớp DTO và runtime config như sau:

- **[MODIFY] [StaticDataDto.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Data/StaticDataDto.cs)**:
  * Thêm vào `PictureDto`:
    * `public bool is_initially_unlocked;` - Tranh có được mở khóa mặc định từ đầu không.
    * `public List<int> unlock_requirements = new();` - Danh sách các Key Item ID yêu cầu để mở khóa tranh.
    * `public string difficulty_unlock_policy;` - Quy định cách mở độ khó (ví dụ: `"sequential"` hoặc `"all_unlocked"`).

- **[MODIFY] [PlayerSave.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Data/PlayerSave.cs)**:
  * Thêm vào `PictureConfig` (readonly struct):
    * `public readonly bool IsInitiallyUnlocked;`
    * `public readonly string DifficultyUnlockPolicy;`
    * `public readonly IReadOnlyList<int> UnlockRequirements;` // Sử dụng IReadOnlyList để tránh rò rỉ mutable data
  * Cập nhật constructor của `PictureConfig` để nhận và gán các tham số mới này (sử dụng defensive copy hoặc tạo mảng/list mới để đảm bảo tính bất biến).

- **[MODIFY] [IStaticDataService.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/IStaticDataService.cs)**:
  * Bổ sung các API hỗ trợ lấy dữ liệu khó của tranh và toàn hệ thống:
    ```csharp
    IReadOnlyList<PictureDifficultyConfig> GetPictureDifficulties(int pictureId);
    IReadOnlyList<PictureDifficultyConfig> GetAllPictureDifficulties();
    ```

- **[MODIFY] [StaticDataService.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/StaticDataService.cs)**:
  * Triển khai hai phương thức `GetPictureDifficulties` và `GetAllPictureDifficulties`.
  * Cập nhật hàm `LoadFromText` để map các trường mới (`is_initially_unlocked`, `difficulty_unlock_policy`, `unlock_requirements`) từ DTO sang runtime `PictureConfig`.
  * Cập nhật hàm `LoadFallbackData` để gán giá trị mặc định cho các trường này của Tranh 1 và Tranh 2.

### 1.2. Tạo Lớp Khung (Skeletons) để Đảm bảo Biên dịch (Compile)
Để viết các failing tests ở giai đoạn TDD mà không làm gãy compile do thiếu kiểu dữ liệu, các lớp khung (Skeleton) trống sau sẽ được tạo lập trước:

- **[NEW] [PictureCardPresentationModel.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/PictureCardPresentationModel.cs)**:
  * Định nghĩa lớp dữ liệu hiển thị card với đầy đủ namespace import (chờ Unity biên dịch sinh `.meta`):
    ```csharp
    using JigsawVina.Core.Data;
    using JigsawVina.Core.Services;

    namespace JigsawVina.Presentation.Screens
    {
        public class PictureCardPresentationModel
        {
            public PictureConfig Config;
            public PictureCardState State;
            public string MissingItemsHint;
        }
    }
    ```

- **[NEW] [ProgressionService.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/ProgressionService.cs)**:
  * Định nghĩa các enum, struct và phương thức trống ném ra `NotImplementedException` (chờ Unity biên dịch sinh `.meta`):
    ```csharp
    using System;
    using System.Collections.Generic;
    using JigsawVina.Core.Services;

    namespace JigsawVina.Core.Services
    {
        public enum PictureCardState { Locked, ReadyToUnlock, Unlocked, Completed }
        public enum UnlockResult { Success, AlreadyUnlocked, MissingRequirements, PictureNotFound }
        public struct ItemSourceHint { public int PictureId; public int DifficultyId; }

        public class ProgressionService
        {
            private readonly IStaticDataService _staticData;
            private readonly ISaveDataService _saveData;

            public ProgressionService(IStaticDataService staticData, ISaveDataService saveData)
            {
                _staticData = staticData;
                _saveData = saveData;
            }

            public PictureCardState GetPictureState(int pictureId) => throw new NotImplementedException();
            public UnlockResult TryUnlockPicture(int pictureId) => throw new NotImplementedException();
            public bool IsDifficultyUnlocked(int pictureId, int difficultyId) => throw new NotImplementedException();
            public IReadOnlyList<ItemSourceHint> GetItemSourceHints(int itemId) => throw new NotImplementedException();
        }
    }
    ```

### 1.3. Nâng cấp Validator Dữ liệu tĩnh (Static Data Validation)
Bổ sung các quy tắc kiểm tra tính hợp lệ của dữ liệu tĩnh trong `StaticDataService.ValidateStaticData()`:
1. **Kiểm tra loại vật phẩm yêu cầu mở khóa**: Mọi ID trong `unlock_requirements` của tranh phải:
   * Tồn tại trong hệ thống (`items`).
   * Phải là Key Item (`item_type == "key_item"`).
   * Phải là vật phẩm không tiêu hao (`is_consumable == false`).
   * Phải đang hoạt động (`status == "active"`).
2. **Kiểm tra Trùng lặp yêu cầu**: Đảm bảo trong `unlock_requirements` của cùng một tranh không chứa phần tử trùng nhau.
3. **Kiểm tra Policy hợp lệ**: Giá trị của `difficulty_unlock_policy` phải thuộc tập hợp cho phép (`"sequential"` hoặc `"all_unlocked"`).
4. **Kiểm tra cấu hình độ khó tuần tự**: Với mỗi tranh:
   * Phải tồn tại cấu hình cho độ khó 0 (Dễ).
   * Nếu policy của tranh là `"sequential"`: Đảm bảo các độ khó được cấu hình liên tục không có khoảng trống (ví dụ: nếu có cấu hình cho độ khó `d` thì bắt buộc phải có cấu hình cho độ khó `d-1`).
5. **Thuật toán Phát hiện Deadlock Progression**:
   * Khởi tạo tập hợp các tranh có thể mở khóa được: `unlockedSet = new HashSet<int>()`.
   * Thêm các tranh có `is_initially_unlocked == true` vào `unlockedSet`.
   * Chạy vòng lặp lan truyền: Duyệt qua danh sách các tranh chưa thuộc `unlockedSet`. Đối với mỗi tranh `P`:
     * Kiểm tra xem các Key Item trong `unlock_requirements` của `P` có "reachable" không. Một Key Item `itemId` được xem là reachable nếu:
       * Tồn tại ít nhất một độ khó `d` của một bức tranh nguồn `P_source` thưởng vật phẩm `itemId` này trong First Clear.
       * Tranh nguồn `P_source` đã mở khóa (nằm trong `unlockedSet`).
       * Với tranh `P_source`, độ khó `d` phải chơi được (nếu policy của `P_source` là sequential, mọi độ khó từ `0` đến `d` đều phải được cấu hình đầy đủ).
     * Nếu tất cả các Key Items yêu cầu của tranh `P` đều reachable -> Thêm tranh `P` vào `unlockedSet`.
   * Lặp lại cho đến khi không có thêm tranh nào được thêm vào `unlockedSet`.
   * Nếu sau khi kết thúc, số lượng phần tử trong `unlockedSet` nhỏ hơn tổng số tranh hiện có -> Ném lỗi `InvalidOperationException` chỉ ra các tranh bị khóa vĩnh viễn (Deadlock do vòng lặp yêu cầu hoặc do nguồn thưởng không thể tiếp cận).

### 1.4. Cập nhật JSON & Di trú Test Fixtures hiện có
Để đảm bảo validator không báo lỗi và các test case cũ không bị gãy biên dịch (compiler errors):
- **[MODIFY] [jigsaw_vina_game_data.json](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/Resources/GameData/jigsaw_vina_game_data.json)**:
  * Cập nhật Tranh 1-5 hiện tại bổ sung các trường: `"is_initially_unlocked": true`, `"difficulty_unlock_policy": "sequential"`, `"unlock_requirements": []`. (Thực hiện ngay trong Task 35 để tránh validator ở Task 37 ném exception do thiếu thuộc tính).
- **Cập nhật Test Fixtures**:
  * Chỉnh sửa các file test:
    - [StaticDataServiceTests.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Tests/StaticDataServiceTests.cs)
    - [ProgressionTests.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Tests/ProgressionTests.cs)
    - [PictureSelectFlowTests.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Tests/PictureSelectFlowTests.cs)
    - [JigsawVinaGameDataEditorTests.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Tests/JigsawVinaGameDataEditorTests.cs)
  * Sửa tất cả các hàm khởi tạo `new PictureConfig(...)` thủ công và mock JSON hiện có trong các test này để bổ sung các tham số mới (`isInitiallyUnlocked`, `difficultyUnlockPolicy`, `unlockRequirements`) khớp với contract mới.

### 1.5. Chuẩn hóa & Migration Save cũ
Để tránh lỗi tham chiếu `NullReferenceException` khi deserialize các dữ liệu save cũ chưa có trường mới:
- **[MODIFY] [PlayerSave.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Data/PlayerSave.cs)**:
  * Thêm `public List<int> UnlockedPictureIds = new();` vào `PlayerSave`.
  * Viết phương thức chuẩn hóa:
    ```csharp
    public void Normalize()
    {
        if (CompletedPuzzles == null) CompletedPuzzles = new();
        if (OwnedItemIds == null) OwnedItemIds = new();
        if (UnlockedPictureIds == null) UnlockedPictureIds = new();
    }
    ```
- **[MODIFY] [SaveDataService.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/SaveDataService.cs)**:
  * Trong hàm `Load()`, gọi `save.Normalize()` trước khi trả về object.

### 1.6. Xây dựng dịch vụ Tiến trình & API Mở khóa nguyên tử
- **[MODIFY] [ProgressionService.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/ProgressionService.cs)**:
  * Triển khai chi tiết logic các phương thức:
    * `GetPictureState(int pictureId)`:
      * Đọc dữ liệu save hiện tại và cấu hình tranh.
      * **Kiểm tra trạng thái mở khóa trước**: Tranh được xem là đã mở khóa nếu `IsInitiallyUnlocked == true` hoặc ID có trong `UnlockedPictureIds`.
      * Nếu **chưa mở khóa**:
        * Nếu người chơi sở hữu đầy đủ các vật phẩm trong `UnlockRequirements` -> `ReadyToUnlock`.
        * Ngược lại -> `Locked`.
      * Nếu **đã mở khóa**:
        * Nếu đã đạt đủ số sao tối đa ở tất cả độ khó đã cấu hình của tranh trong save -> Trạng thái là `Completed`.
        * Ngược lại -> Trạng thái là `Unlocked`.
    * `TryUnlockPicture(int pictureId)`: Thực hiện mở khóa nguyên tử (kiểm tra tồn tại, kiểm tra yêu cầu, không ghi đè trùng, lưu save ngay lập tức và không tiêu hao item).
    * `IsDifficultyUnlocked(int pictureId, int difficultyId)`: Áp dụng sequential lock cho sequential policy và mở tự do cho all_unlocked policy.
    * `GetItemSourceHints(int itemId)`: Trả về các cấu trúc chứa `PictureId` và `DifficultyId` thưởng item.
- **[MODIFY] [ProjectLifetimeScope.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/App/ProjectLifetimeScope.cs)**:
  * Đăng ký `ProgressionService` làm Singleton.

### 1.7. Cập nhật UI & Presenter màn chọn tranh (Picture Select)
Để kết nối luồng Unlock và truyền đầy đủ thông tin trạng thái vào View một cách nhất quán:

- **[MODIFY] [PictureSelectView.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/PictureSelectView.cs)**:
  * Bổ sung sự kiện phát yêu cầu mở khóa `public event Action<int> OnPictureUnlockRequested;`.
  * Cập nhật hàm `Setup(IReadOnlyList<PictureCardPresentationModel> models)`:
    * Khi khởi tạo card, truyền model tương ứng vào.
    * Đăng ký hai callback nhất quán từ Card:
      * Sự kiện chọn tranh: `card.Bind(model, onSelected: id => OnPictureSelected?.Invoke(id), onUnlockRequested: id => OnPictureUnlockRequested?.Invoke(id));`
  * Cập nhật `ClearExistingCards()`: Gọi `card.Unbind()` trên từng card trước khi hủy object để xóa sạch tham chiếu callback, tránh rò rỉ bộ nhớ.

- **[MODIFY] [PictureSelectCard.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/PictureSelectCard.cs)**:
  * Bổ sung các tham chiếu UI: Lock Overlay, Text gợi ý nguồn kiếm, Nút Unlock.
  * Cập nhật hàm `Bind(PictureCardPresentationModel model, Action<int> onSelected, Action<int> onUnlockRequested)`:
    * Lưu `_pictureId = model.Config.Id;`
    * Lưu tham chiếu `_onSelected = onSelected;` và `_onUnlockRequested = onUnlockRequested;`.
    * Nếu trạng thái tranh là `Locked` hoặc `ReadyToUnlock`, đặt Button click chính của Card ở trạng thái `interactable = false`. (Chặn click chọn tranh từ tầng UI).
    * Hiển thị Lock Overlay. Nút "Unlock" chỉ được hiển thị và tương tác khi ở trạng thái `ReadyToUnlock`.
    * Hiển thị danh sách Key Item còn thiếu và chuỗi gợi ý nguồn kiếm `model.MissingItemsHint` nhận được.
  * Cập nhật hàm `Awake()`:
    * Button chính của Card: Kích hoạt `_onSelected?.Invoke(_pictureId)`.
    * Button Unlock: Kích hoạt `_onUnlockRequested?.Invoke(_pictureId)`.
  * Cập nhật `Unbind()`:
    * Gán cả `_onSelected = null;` và `_onUnlockRequested = null;`.
    * Dọn dẹp sprite và reset các thành phần UI.

- **[MODIFY] [PictureSelectPresenter.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/PictureSelectPresenter.cs)**:
  * Duyệt qua danh sách tranh, đóng gói toàn bộ thông tin tranh, trạng thái và chuỗi hint nguồn kiếm thành danh sách `PictureCardPresentationModel` và truyền vào `view.Setup()`.
  * Lắng nghe sự kiện `view.OnPictureUnlockRequested` -> Gọi `ProgressionService.TryUnlockPicture()` -> Làm mới UI khi mở khóa thành công.
  * Thực hiện hủy đăng ký sự kiện `view.OnPictureUnlockRequested -= HandlePictureUnlockRequested` trong phương thức `Dispose()`.

### 1.8. Cập nhật UI & Presenter màn chọn độ khó (Difficulty Select)
- **[MODIFY] [DifficultySelectView.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/DifficultySelectView.cs)**:
  * Hỗ trợ bật/tắt (interactable) các nút độ khó.
  * Thêm các serialized fields cho lock icon và text thành tích:
    * `[SerializeField] private GameObject[] _lockIcons;`
    * `[SerializeField] private TMP_Text[] _achievementTexts;`

- **[MODIFY] [DifficultySelectPresenter.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/DifficultySelectPresenter.cs)**:
  * Cung cấp phương thức `Refresh(int pictureId)` để cập nhật UI của View dựa trên tranh được chọn.
  * **Chặn lựa chọn ở tầng logic**: Trong `HandleDifficultySelected(int difficultyId)`, gọi `ProgressionService.IsDifficultyUnlocked(_selectedPictureId, difficultyId)` để kiểm tra. Chặn không đi tiếp nếu bị khóa.
  * Implement `IDisposable` và thực hiện hủy đăng ký sự kiện `view.OnDifficultySelected -= HandleDifficultySelected` tránh rò rỉ bộ nhớ.

- **[MODIFY] [HomeFlowController.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/HomeLifetimeScope.cs)**:
  * Trong `HandlePictureSelected(int pictureId)`: Gọi `_difficultySelectPresenter.Refresh(pictureId)` trước khi hiển thị `DifficultySelectView` (đặt active = true).

### 1.9. Cập nhật Editor Cheats & Config
- **[MODIFY] [JigsawVinaGameDataEditor.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Editor/JigsawVinaGameDataEditor.cs)**:
  * Cập nhật Tab cấu hình Tranh trong editor:
    * Thêm trường nhập boolean `Is Initially Unlocked`.
    * Thêm trường dropdown chọn `Difficulty Unlock Policy` (Sequential / AllUnlocked).
    * Thêm giao diện quản lý danh sách `Unlock Requirements` (thêm/xóa các Item ID).
  * Cập nhật nút Cheat "Unlock All": Tự động điền tất cả ID tranh có `IsInitiallyUnlocked == false` vào `UnlockedPictureIds` trong save.

### 1.10. Cập nhật Asset & Scene Wiring
- **[MODIFY] Cập nhật Prefab `PictureSelectCard.prefab`**:
  * Thêm Image Lock Overlay làm màn che tối và một Icon ổ khóa nhỏ góc card.
  * Thêm Text hiển thị Key Item yêu cầu còn thiếu và nguồn kiếm vật phẩm đó.
  * Thêm Button Unlock được định dạng đẹp mắt nằm trong Lock Overlay.
- **[MODIFY] [ThinVerticalSliceSceneSetup.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Editor/ThinVerticalSliceSceneSetup.cs)**:
  * Cập nhật script setup để:
    * Wire các component UI mới trên card prefab.
    * Tạo các Game Object ổ khóa và text sao/best-time dưới các nút độ khó trong scene.
    * Wire các Game Object ổ khóa và text mới này vào các serialized fields tương ứng của `DifficultySelectView` (`_lockIcons`, `_achievementTexts`) trên scene.
- **[REGENERATE] `Home.unity`**: Chạy scene setup để cập nhật và wire các thành phần UI mới, đảm bảo tính bất biến (idempotency) khi chạy lại nhiều lần.

---

## GIAI ĐOẠN 2: Rate Item & Daily Drop Decay (Deferred - Chuyển sang Milestone sau)

Phần này bổ sung chiều sâu cho nền kinh tế game nhưng có độ phức tạp cao hơn và không cần thiết trước khi hệ thống mở khóa cơ bản được hoàn thiện.

- **Mục tiêu**: Hạn chế người chơi cày cuốc quá mức (anti-farming) bằng cách giảm tỷ lệ rơi vật phẩm khi đạt giới hạn rơi hàng ngày.
- **Chi tiết**:
  * Lưu trữ dữ liệu số lần rơi vật phẩm trong ngày và mốc thời gian lưu cuối cùng (`LastSaveDateString`) trong `PlayerSave.cs`.
  * Tích hợp cơ chế quay thưởng theo tỷ lệ giảm dần (`base_rate - count * decay_rate`, tối thiểu là `min_rate`) vào `RewardSummaryPresenter.cs`.
  * Tự động reset bộ đếm khi qua ngày mới.

---

## GIAI ĐOẠN 3: Inventory / Collection UI (Deferred - Chuyển sang Milestone sau)

Giao diện giúp người chơi quản lý và ngắm nhìn các vật phẩm thu thập được.

- **Mục tiêu**: Tăng trải nghiệm thành tựu của người chơi.
- **Chi tiết**:
  * Xây dựng màn hình hiển thị toàn bộ Key Items đã nhận, số lượng sở hữu, tên gọi và mô tả chi tiết của từng vật phẩm.
  * Tích hợp lối tắt đi nhanh tới tranh yêu cầu vật phẩm đó.

---

## Kế hoạch kiểm thử (Verification Plan) cho Giai đoạn 1

### Automated Tests (Thực hiện viết Unit Tests trước/đồng thời theo quy trình TDD)
- Viết unit test trong `ProgressionTests.cs` kiểm tra:
  * **Save Migration**: Deserialize save cũ/null, đảm bảo các danh sách được Normalize() khởi tạo đầy đủ.
  * **Atomic Unlock API**: Đầy đủ các trường hợp và đảm bảo Key Item không bị tiêu hao.
  * **Correct Completed Check Order**: Tranh bị khóa phải ở trạng thái Locked/ReadyToUnlock chứ không được thành Completed cho dù save có thành tích.
  * **Sequential/AllUnlocked Difficulty Policy**: Thử nghiệm độ khó tuần tự của tranh sequential, và kiểm tra mở khóa toàn bộ của tranh all_unlocked.
  * **Hint Search**: Kiểm tra `GetItemSourceHints` định vị chính xác.
  * **Static Data Deadlock Check**: Đảm bảo validator bắt lỗi khi vật phẩm không phải Key Item, tiêu hao hoặc không hoạt động. Đưa vào mock JSON có deadlock (như chu trình khóa hoặc thiếu nguồn thưởng) và kiểm chứng validator bắt lỗi chính xác.
  * **UI & Lifecycle**: Chặn click locked card, và chặn click độ khó bị khóa ở presenter (`locked difficulty does not load Gameplay` test), unsubscribe sự kiện khi dispose presenter (Lifecycle test).

- **Mở rộng [LifetimeScopeRegistrationTests.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Tests/LifetimeScopeRegistrationTests.cs)** (Targeted assertions cho UI mới - **Thực hiện trong Task 40**):
  * Kiểm tra card prefab (được load từ asset path) chứa đầy đủ các GameObject/Image Lock Overlay, TMP_Text cho hint text và Button cho nút Unlock (không bị null).
  * Kiểm tra `DifficultySelectView` trên scene có mảng `_lockIcons` và `_achievementTexts` được gán chính xác đúng 3 phần tử mỗi mảng, và không có phần tử nào bị null.
  * Kiểm tra việc chạy scene setup nhiều lần không tạo ra các asset trùng lặp và không làm bẩn file `Home.unity`.

### Manual Verification
1. Reset save, kiểm tra các tranh có `is_initially_unlocked == false` bị khóa, hiển thị overlay và list key item còn thiếu kèm gợi ý nguồn kiếm.
2. Hoàn thành Tranh 1 Dễ để nhận Key Item 107 -> kiểm tra Tranh 6 chuyển sang `ReadyToUnlock` -> Click mở khóa, kiểm tra Key Item không bị biến mất trong Cheat Editor.
3. Kiểm tra sequential difficulty của các tranh có policy sequential trong màn chọn độ khó.
4. Kiểm tra tranh có policy all_unlocked sau khi mở khóa có thể chơi ngay độ khó Khó.
5. Chạy lại Scene Setup nhiều lần để đảm bảo không làm bẩn file `Home.unity` và không tạo asset trùng lặp.
