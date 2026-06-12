# Localization Keys and Review Updates Implementation Plan

> **For Antigravity:** REQUIRED WORKFLOW: Use `.agent/workflows/execute-plan.md` to execute this plan in single-flow mode.

**Goal:** Add `display_name_key` and `description_key` to Category, Picture, and Key Item configurations, expose them in the GUI editor, and optionally address the other P1/P2/P3 review comments.

**Architecture:**
- Extend `CategoryDto` and `PictureDto` DTO classes with `display_name_key` and `description_key` fields.
- Update `PictureConfig` in `PlayerSave.cs` and `StaticDataService.cs` to handle these fields at runtime.
- Update `JigsawVinaGameDataEditor.cs` states (`EditorCategoryState`, `EditorTabState`, `EditorItemState`) and GUI rendering methods to allow editing these keys.
- Auto-populate default localization keys on scanning or directory drag-and-drop.

**Tech Stack:** Unity Editor GUI (IMGUI), Unity C# scripting, JSON serialization.

---

## Open Questions for User Review

> [!IMPORTANT]
> **Vui lòng xác nhận các nội dung sau trước khi chúng ta tiến hành lập trình:**
> 
> 1. **Bạn có muốn gộp luôn việc sửa các lỗi P1/P2 từ review trước vào đợt cập nhật này không?**
>    - **Sửa P1 (Save mẫu)**: Cập nhật file `docs/jigsaw_vietnam_player_save_sample_v0_1.json` về đúng định dạng runtime (`Coins`, `Hints`, `CompletedPuzzles`, `OwnedItemIds`).
>    - **Sửa P1 (Difficulty ID mẫu)**: Thay đổi `difficulty_id` trong 2 file mẫu `jigsaw_vietnam_static_data_sample_v0_1.json` và `jigsaw_vietnam_player_save_sample_v0_1.json` từ `1/2/3` về `0/1/2`.
>    - **Sửa P2 (Runtime Validator)**: Bổ sung kiểm tra category, trùng lặp difficulty, difficulty ID ngoài 0..2, giá trị âm và check version vào `StaticDataService.cs`.
>    - **Sửa P2 (Nguồn Static Data dư thừa)**: Xóa hoàn toàn file dư thừa `Assets/Resources/StaticData.json`.
>
> 2. **Nếu có, bạn có đồng ý với đề xuất tự động sinh (Auto-generate) mã ngôn ngữ theo định dạng sau cho tranh và danh mục để giảm thiểu thời gian cấu hình?**
>    - Đối với Tranh: `picture.<id_string>.name` và `picture.<id_string>.description`
>    - Đối với Danh mục: `category.<id_string>.name` và `category.<id_string>.description`
>    - Đối với Key Items: `item.<id_string>.name` và `item.<id_string>.description`

---

## Proposed Changes

### Component 1: Core Data DTOs & Runtime Configuration

#### [MODIFY] [StaticDataDto.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Data/StaticDataDto.cs)
- Thêm trường `display_name_key` và `description_key` vào `CategoryDto` và `PictureDto`.

```csharp
    [Serializable]
    public class CategoryDto
    {
        public int id;
        public string id_string;
        public string display_name;
        public string display_name_key;
        public string description_key;
    }

    [Serializable]
    public class PictureDto
    {
        public int id;
        public string id_string;
        public string display_name;
        public int category_id;
        public string asset_path;
        public string difficulty_unlock_policy;
        public string display_name_key;
        public string description_key;
    }
```

#### [MODIFY] [PlayerSave.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Data/PlayerSave.cs)
- Thêm trường `DisplayNameKey` và `DescriptionKey` vào struct `PictureConfig`.

```csharp
    public readonly struct PictureConfig
    {
        public readonly int Id;
        public readonly string IdString;
        public readonly string DisplayName;
        public readonly string AssetPath;
        public readonly string DisplayNameKey;
        public readonly string DescriptionKey;

        public PictureConfig(int id, string idString, string displayName, string assetPath, string displayNameKey, string descriptionKey)
        {
            Id = id;
            IdString = idString;
            DisplayName = displayName;
            AssetPath = assetPath;
            DisplayNameKey = displayNameKey;
            DescriptionKey = descriptionKey;
        }
    }
```

#### [MODIFY] [StaticDataService.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/StaticDataService.cs)
- Cập nhật hàm `LoadFallbackData` và hàm `LoadFromText` để truyền đủ tham số mới cho `PictureConfig`.

```csharp
            _pictures = dto.pictures.Select(p => new PictureConfig(
                p.id, 
                p.id_string, 
                p.display_name, 
                p.asset_path,
                p.display_name_key,
                p.description_key
            )).ToList();
```

---

### Component 2: Game Data Editor GUI & Serialization Logic

#### [MODIFY] [JigsawVinaGameDataEditor.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Editor/JigsawVinaGameDataEditor.cs)
- Thêm trường dữ liệu cấu hình ngôn ngữ vào các lớp trạng thái GUI nội bộ:
  - `EditorCategoryState`: thêm `displayNameKey`, `descriptionKey`.
  - `EditorTabState`: thêm `displayNameKey`, `descriptionKey`.
  - `EditorItemState`: thêm `displayNameKey`, `descriptionKey`.
- Cập nhật hàm `LoadStateFromDto` để gán dữ liệu từ DTO sang EditorState.
- Cập nhật hàm `TryBuildConfig` để gán dữ liệu từ EditorState ngược lại DTO trước khi ghi file JSON.
- Cập nhật hàm `DrawCategoriesTab` để hiển thị 2 ô nhập cho Danh mục:
  ```csharp
  cat.displayNameKey = EditorGUILayout.TextField("Khóa tên hiển thị", cat.displayNameKey);
  cat.descriptionKey = EditorGUILayout.TextField("Khóa mô tả", cat.descriptionKey);
  ```
- Cập nhật hàm `DrawTabDetails` ở tab phụ 1 để hiển thị 2 ô nhập cho Tranh chính:
  ```csharp
  state.displayNameKey = EditorGUILayout.TextField("Khóa tên hiển thị", state.displayNameKey);
  state.descriptionKey = EditorGUILayout.TextField("Khóa mô tả", state.descriptionKey);
  ```
- Cập nhật hàm `DrawTabDetails` ở tab phụ 1 phần Key Items để hiển thị 2 ô nhập cho từng Key Item:
  ```csharp
  itemState.displayNameKey = EditorGUILayout.TextField("Khóa tên hiển thị", itemState.displayNameKey);
  itemState.descriptionKey = EditorGUILayout.TextField("Khóa mô tả", itemState.descriptionKey);
  ```
- Cập nhật logic tự động điền (Auto-generate) trong `AutoFillFromFolder` và `SyncItemStates` cho cả tranh, category và key items nếu trường bị để trống.

---

## Verification Plan

### Manual Verification
- Mở `Game Data Editor`.
- Điền các khóa ngôn ngữ cho Danh mục, Tranh chính và Key Items.
- Bấm "Save & Generate JSON".
- Mở file `jigsaw_vina_game_data.json` kiểm tra cấu trúc lưu trữ xem các khóa ngôn ngữ đã được lưu chính xác hay chưa.
- Khởi động Play Mode xem dữ liệu cấu trúc mới có được nạp bình thường hay không.
