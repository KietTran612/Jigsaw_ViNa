# Implementation Plan - Game Settings, Audio & Localization Systems (Task 50)

**Goal:** Triển khai đồng thời ba hệ thống nền tảng cốt lõi: Menu Cài đặt & Tạm dừng (Settings & Pause), Quản lý Âm thanh (Audio Service) và Hệ thống Đa ngôn ngữ tự viết (Custom JSON Localization Service) để hoàn thiện luồng vận hành hệ thống của game Jigsaw ViNa.

> [!NOTE]
> Dự án sẽ **không sử dụng** package I2 Localization cho runtime để tránh các rủi ro về thiếu Assembly Definition (.asmdef), xung đột Editor script trong runtime build, và trạng thái tĩnh (global state) làm khó kiểm thử Unit Test. Thay vào đó, ta sử dụng một Localization Service tự viết đọc từ file JSON cực kỳ gọn nhẹ và dễ Unit Test.

---

## Proposed Changes

### 0. Class Refactoring (Long Files Refactor Continuation)
#### [NEW] [GameplayFlowController.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/GameplayFlowController.cs)
- Trích xuất class `GameplayFlowController` ra khỏi file [GameplayLifetimeScope.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/GameplayLifetimeScope.cs) sang một file riêng biệt để đảm bảo tính modular và tách biệt trách nhiệm.
- `GameplayFlowController` sẽ kế thừa `IStartable`, `ITickable`, và **`IDisposable`** để dọn dẹp các đăng ký sự kiện sạch sẽ khi scope bị hủy.

#### [MODIFY] [GameplayLifetimeScope.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/GameplayLifetimeScope.cs)
- Xóa định nghĩa class `GameplayFlowController` trong file này và giữ nguyên cấu hình đăng ký `builder.RegisterEntryPoint<GameplayFlowController>();`.

---

### 1. Data Model & Settings Persistence
#### [MODIFY] [PlayerSave.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Data/PlayerSave.cs)
- Bổ sung các trường cài đặt kiểu số (`int`) đóng vai trò là các flag để thực hiện migration an toàn trên Unity `JsonUtility` (tránh việc gán mặc định `false` cho các trường `bool` thiếu trong JSON cũ):
  - `public int MusicEnabledState = -1;` // -1: chưa khởi tạo, 0: tắt, 1: bật
  - `public int SfxEnabledState = -1;`   // -1: chưa khởi tạo, 0: tắt, 1: bật
  - `public string Language = null;`     // null: chưa khởi tạo, mặc định sẽ gán "vi"
- Cập nhật hàm `Normalize()` thực hiện khởi tạo giá trị mặc định (Migration) nếu phát hiện các giá trị chưa khởi tạo:
  ```csharp
  if (MusicEnabledState == -1) MusicEnabledState = 1;
  if (SfxEnabledState == -1) SfxEnabledState = 1;
  if (string.IsNullOrEmpty(Language)) Language = "vi";
  ```

---

### 2. Custom Localization System
#### [NEW] [LocalizationKeys.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/LocalizationKeys.cs)
- Định nghĩa tập trung tất cả các khóa dịch UI tĩnh dưới dạng hằng số (`public const string`):
  ```csharp
  public static class LocalizationKeys
  {
      public const string GameplayBack = "ui.gameplay.back";
      public const string GameplayTimer = "ui.gameplay.timer";
      public const string GameplayHint = "ui.gameplay.hint";
      public const string SettingsTitle = "ui.settings.title";
      public const string SettingsMusic = "ui.settings.music";
      public const string SettingsSfx = "ui.settings.sfx";
      public const string SettingsLanguage = "ui.settings.language";
      public const string SettingsResume = "ui.settings.resume";
      public const string SettingsQuit = "ui.settings.quit";
  }
  ```

#### [NEW] [ILocalizationService.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/ILocalizationService.cs)
#### [NEW] [LocalizationService.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/LocalizationService.cs)
- Xây dựng `LocalizationService` kế thừa `ILocalizationService`.
- Nhận `ISaveDataService` qua Constructor Injection để load và save cài đặt ngôn ngữ.
- **Giải pháp nạp JSON Dictionary qua JsonUtility:**
  - Định nghĩa các class DTO phụ trợ tương thích với `JsonUtility`:
    ```csharp
    [Serializable]
    public class LocalizationEntry
    {
        public string Key;
        public string Value;
    }

    [Serializable]
    public class LocalizationData
    {
        public List<LocalizationEntry> Entries = new();
    }
    ```
  - Khi khởi tạo: load save data, xác định mã ngôn ngữ (mặc định `"vi"`).
  - Tải tệp JSON tương ứng từ `Resources/Localization/strings_vi.json` hoặc `strings_en.json` thông qua đường dẫn tương đối và không chứa đuôi mở rộng file:
    `Resources.Load<TextAsset>("Localization/strings_vi")` hoặc `Resources.Load<TextAsset>("Localization/strings_en")`.
  - Khai báo một `Dictionary<string, string> _translations` nội bộ.
  - Sử dụng `JsonUtility.FromJson<LocalizationData>(textAsset.text)` để deserialize danh sách và nạp vào `_translations` giúp tra cứu nhanh $O(1)$.
- Các phương thức chính:
  - `void SetLanguage(string langCode)` ("vi" hoặc "en"): ghi nhận vào save data, gọi `_saveDataService.Save()`, nạp lại từ điển JSON và kích hoạt event `OnLanguageChanged`.
  - `string Get(string key)`: trả về chuỗi đã dịch theo key. Nếu không tìm thấy key, trả về chính key đó để dễ phát hiện lỗi.
  - `string GetFormat(string key, params object[] args)`: trả về chuỗi được định dạng (Format).
  - `event Action OnLanguageChanged`: phát sự kiện khi người dùng chuyển ngôn ngữ.
- Đăng ký làm **Singleton** trong `ProjectLifetimeScope`.

#### [NEW] [strings_vi.json](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/Resources/Localization/strings_vi.json)
#### [NEW] [strings_en.json](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/Resources/Localization/strings_en.json)
- Lưu trữ các cặp key-value bản dịch tiếng Anh và tiếng Việt theo đúng cấu trúc:
  ```json
  {
      "Entries": [
          { "Key": "ui.gameplay.back", "Value": "Quay lại" },
          { "Key": "ui.settings.title", "Value": "CÀI ĐẶT" }
      ]
  }
  ```

---

### 3. Plain C# Audio Service & Safe Lifecycle Policy
#### [NEW] [IAudioService.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/IAudioService.cs)
#### [NEW] [AudioService.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/AudioService.cs)
- Xây dựng `AudioService` dưới dạng **Plain C# class** kế thừa `IAudioService` và **`IDisposable`** để tránh rò rỉ GameObject khi reload scene hoặc kết thúc test.
- Constructor nhận `ISaveDataService` để load/save cài đặt âm thanh.
- Lúc khởi tạo:
  - **Dọn dẹp GameObject cũ an toàn (Tránh trùng lặp GameObject cùng khung hình [P2]):**
    ```csharp
    var existing = GameObject.Find("AudioServiceRuntime");
    if (existing != null)
    {
        if (Application.isPlaying)
        {
            // Đổi tên trước để tránh trùng lặp GameObject "AudioServiceRuntime" trong frame này (delayed destroy)
            existing.name = "AudioServiceRuntime_Destroying";
            UnityEngine.Object.Destroy(existing);
        }
        else
        {
            UnityEngine.Object.DestroyImmediate(existing);
        }
    }
    ```
  - Tạo GameObject runtime mới: `_runtimeGo = new GameObject("AudioServiceRuntime"); Object.DontDestroyOnLoad(_runtimeGo);`
  - Thêm 2 AudioSource vào GameObject này: `_musicSource` và `_sfxSource`.
  - Load save data: `var save = _saveDataService.Load();` và gán trạng thái mute cho nguồn:
    - `_musicSource.mute = (save.MusicEnabledState == 0);`
    - `_sfxSource.mute = (save.SfxEnabledState == 0);`
- Các phương thức chính:
  - `void PlayBGM(string clipPath, bool loop = true, float fadeDuration = 0.5f)`.
  - `void StopBGM(float fadeDuration = 0.5f)`.
  - `void PlaySFX(string clipPath, float volumeScale = 1f)`.
  - `void SetMusicEnabled(bool enabled)`: Cập nhật mute của Source, ghi nhận thay đổi (`save.MusicEnabledState = enabled ? 1 : 0`), và lưu ngay lập tức xuống đĩa.
  - `void SetSfxEnabled(bool enabled)`: Tương tự đối với SFX.
- Thực thi `Dispose()`: tự động hủy `_runtimeGo` để giải phóng bộ nhớ khi VContainer giải phóng scope.
- Đăng ký làm **Singleton** trong `ProjectLifetimeScope` bằng: `builder.Register<AudioService>(Lifetime.Singleton).As<IAudioService>();`

---

### 4. Settings, Pause System & Boundary Control
#### [MODIFY] [PuzzleSession.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Data/PuzzleSession.cs)
- Bổ sung trường trạng thái tạm dừng: `public bool IsPaused { get; set; } = false;`
- Trong hàm `Tick(float deltaTime)`, nếu `IsPaused` là true thì bỏ qua không đếm thời gian.

#### [MODIFY] [PuzzlePlayingView.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/PuzzlePlayingView.cs)
- Bổ sung nút Pause: `[SerializeField] private Button _pauseButton;` và thuộc tính `public Button PauseButton => _pauseButton;`.
- Bổ sung sự kiện `public event Action OnPauseClicked;` và wire listener trong `Awake()`.
- Bổ sung popup settings: `[SerializeField] private GameSettingsPopup _settingsPopup;` và thuộc tính `public GameSettingsPopup SettingsPopup => _settingsPopup;`.

#### [MODIFY] [PuzzlePlayingPresenter.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/PuzzlePlayingPresenter.cs)
- Quản lý trạng thái Pause và kiểm soát các luồng tương tác/navigation (Presenter Boundary):
  - **Sự kiện Tạm dừng:** Lắng nghe `OnPauseClicked` trên `PuzzlePlayingView`.
    - Khi click Pause: đặt `_puzzleSession.IsPaused = true;` và gọi `_view.SettingsPopup.Show(save.MusicEnabledState == 1, save.SfxEnabledState == 1, save.Language);`.
  - **Sự kiện UI chính:** Nhận sự kiện click nút Back trên View. Nếu đang tạm dừng (`_puzzleSession.IsPaused`) thì bỏ qua; nếu không, phát sự kiện `public event Action OnBackRequested;` ra bên ngoài.
  - **Sự kiện Popup Settings:**
    - Khi Toggle thay đổi: gọi `SetMusicEnabled` / `SetSfxEnabled` trên `AudioService`.
    - Khi Dropdown Language thay đổi: gọi `SetLanguage` trên `LocalizationService`.
    - Khi bấm **Resume**: đặt `_puzzleSession.IsPaused = false;` và gọi ẩn popup.
    - Khi bấm **Quit (Single-Owner Cleanup [P2]):** đặt `_puzzleSession.IsPaused = false;`, gọi ẩn popup, và phát sự kiện `public event Action OnQuitRequested;` ra bên ngoài. **Presenter tuyệt đối không tự gọi Cleanup() tại đây để tránh lỗi double cleanup.**
  - **Pause Policy:** Chặn toàn bộ tương tác gameplay và các hot paths khi đang tạm dừng:
    - Kiểm tra `if (_puzzleSession.IsPaused) return;` trong:
      - Kéo thả: `HandlePiecePointerDown`, `HandlePieceDrag`, `HandlePieceDragEnd`.
      - Trợ giúp: `ApplyHint()`.
      - Phím chức năng: `ReturnAllFloatingToTray()`, `CheatWin()`.

#### [MODIFY] [GameplayFlowController.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/GameplayFlowController.cs)
- **Flow Controller Boundary & Single-Owner Cleanup & Named Handlers (leak prevention [P2]):**
  - Tránh sử dụng lambda/anonymous listener cho các sự kiện của View/Presenter. **Thay thế toàn bộ anonymous listeners hiện tại bằng các phương thức named handler rõ ràng** để đảm bảo việc unsubscribe hoạt động chuẩn xác lúc cleanup/dispose.
  - Đăng ký sự kiện:
    - `_puzzlePlayingPresenter.OnBackRequested += HandleBack;`
    - `_puzzlePlayingPresenter.OnQuitRequested += HandleQuit;`
    - `_puzzlePlayingPresenter.OnPuzzleCompleted += HandlePuzzleCompleted;`
    - `_rewardSummaryView.OnReturnClicked += HandleReturnClicked;` (thay thế lambda cũ `() => { ... }` bằng named handler).
  - Hàm `HandleQuit()`, `HandleBack()`, và `HandleReturnClicked()` sẽ là nơi **duy nhất** thực hiện gọi `_puzzlePlayingPresenter.Cleanup();` trước khi chuyển cảnh bằng `SceneLoader.LoadSceneAsync("Home")`.
- **Event Unsubscribe Policy (Chống rò rỉ listener [P2]):**
  - Implement **`IDisposable`** trên `GameplayFlowController`.
  - Trong hàm `Dispose()`, gỡ toàn bộ đăng ký sự kiện bằng named handlers:
    ```csharp
    _puzzlePlayingPresenter.OnPuzzleCompleted -= HandlePuzzleCompleted;
    _puzzlePlayingPresenter.OnBackRequested -= HandleBack;
    _puzzlePlayingPresenter.OnQuitRequested -= HandleQuit;
    _rewardSummaryView.OnReturnClicked -= HandleReturnClicked;
    ```

#### [NEW] [GameSettingsPopup.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/GameSettingsPopup.cs)
- Popup UI settings (MonoBehaviour, đính kèm GameObject popup).
- Lắng nghe sự kiện `OnLanguageChanged` của `LocalizationService` để cập nhật động các nhãn chữ.
- **Event Lifecycle Cleanup (Chống rò rỉ bộ nhớ [P2]):**
  - Khi Popup hoặc bất kỳ Presenter nào đăng ký lắng nghe `OnLanguageChanged`, bắt buộc phải gỡ bỏ đăng ký (`OnLanguageChanged -= HandleLanguageChanged`) khi đối tượng bị hủy (trong `OnDestroy` hoặc `Cleanup` / `Dispose`).
- UI của Popup có component `CanvasGroup` chặn tia raycast (`blocksRaycasts = true`, `interactable = true`) đè lên UI Gameplay phía sau để ngăn người dùng click các nút ẩn bên dưới.

#### [MODIFY] [ThinVerticalSliceGameplaySceneBuilder.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Editor/ThinVerticalSliceGameplaySceneBuilder.cs)
- Tạo nút `PauseButton` trên Topbar bằng `ThinVerticalSliceUiFactory.CreateButton` và wire vào `_pauseButton` của `PuzzlePlayingView`.
- Tạo GameObject `GameSettingsPopup` đính dưới Canvas với các Toggles, Dropdown, Buttons và wire vào trường `_settingsPopup` của `PuzzlePlayingView`.

---

## Verification Plan

### Automated Tests
- **Save Persistence Test:** Viết unit tests trong `SaveDataServiceTests` kiểm tra việc lưu/load trạng thái âm thanh (`MusicEnabledState`, `SfxEnabledState`) dạng int và ngôn ngữ dạng string, đảm bảo tính năng ghi xuống đĩa hoạt động đúng.
- **Settings Migration Test (Critical [P1]):** Viết unit test nạp một chuỗi JSON save cũ (thiếu hoàn toàn các trường cài đặt này), chạy `Normalize()` và assert các giá trị mặc định được gán chính xác (MusicEnabledState=1, SfxEnabledState=1, Language="vi").
- **Pause System Logic Test:** Viết unit tests kiểm tra logic `PuzzleSession.IsPaused` chặn đếm giây và Presenter chặn gọi `ApplyHint()`, `ReturnAllFloatingToTray()`, `CheatWin()`, và drag events.
- **Audio Service Action & Disposal Test:** Viết integration test giả lập cắm `IAudioService` vào hệ thống, kiểm tra việc lưu file settings khi bật/tắt âm thanh, và verify GameObject `AudioServiceRuntime` được destroy sạch sẽ sau khi dispose.
- **Flow Controller Event Cleanup Test (Critical [P2]):** Viết unit test kiểm chứng `GameplayFlowController` được dispose sạch sẽ và không để lại bất kỳ listener rò rỉ nào trên `PuzzlePlayingPresenter` và view components (nhờ gỡ bỏ các named handlers thành công).
- **Localization Key Integrity Validation (Critical [P2]):**
  - Viết một unit test chuyên biệt nạp file cấu hình static `jigsaw_vina_game_data.json` và cả hai file từ điển `strings_vi.json` / `strings_en.json`.
  - Quét qua toàn bộ danh sách `display_name_key` và `description_key` của Categories, Pictures, Items.
  - Sử dụng **Reflection** quét qua tất cả các hằng số `public const string` khai báo trong `LocalizationKeys`.
  - Assert rằng mọi key ngôn ngữ từ hai nguồn trên **bắt buộc phải có bản dịch hợp lệ** (không null, không rỗng và theo đúng cấu trúc `LocalizationData`) trong cả hai file `strings_vi.json` và `strings_en.json`.

### Manual Verification
- Chạy game ở màn hình Home và Gameplay:
  - Bấm nút Pause trong Gameplay: Game dừng, xuất hiện Popup Settings, đồng hồ ngưng chạy, mảnh ghép không thể tương tác, và nút Back trên màn chính không thể hoạt động.
  - Tích/bỏ tích Toggle Music/SFX: Kiểm tra trạng thái âm thanh có được lưu lại khi tắt game đi mở lại không.
  - Đổi ngôn ngữ trong dropdown: Xác nhận toàn bộ văn bản giao diện (từ tên tranh, đồng hồ, nút bấm...) tự động đổi từ tiếng Việt sang tiếng Anh và ngược lại.
