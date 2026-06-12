# Extended Editor Tools Implementation Plan

> **For Antigravity:** REQUIRED WORKFLOW: Use `.agent/workflows/execute-plan.md` to execute this plan in single-flow mode.

**Goal:** Extend the Unity JigsawVinaGameDataEditor window to support custom Category management, Global Item database editing, and a Player Save / Cheat Tool tab directly inside the editor.

**Architecture:** 
- Add data fields and UI tabs inside `JigsawVinaGameDataEditor.cs` for managing Categories and Global Items.
- Load and parse `categories` and custom non-scanned `items` from the static JSON file (`jigsaw_vina_game_data.json`) upon opening.
- Provide a Player Save tab that reads/writes directly to `PlayerPrefs.GetString("JigsawVina_PlayerSave")` using the existing `PlayerSave` schema.

**Tech Stack:** Unity Editor GUI (IMGUI), Unity PlayerPrefs, JSON Utility serialization.

---

### Task 1: Category Management & Main Tab Layout

**Files:**
- Modify: [JigsawVinaGameDataEditor.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Editor/JigsawVinaGameDataEditor.cs)
- Modify: [JigsawVina.Tests.asmdef](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Tests/JigsawVina.Tests.asmdef)

**Step 1: Assembly Definition Reference & Internals Visible Update**
- Add `"JigsawVina.Editor"` reference to [JigsawVina.Tests.asmdef](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Tests/JigsawVina.Tests.asmdef) to allow EditMode tests to access internal methods of the Editor.
- Add the assembly attribute to [JigsawVinaGameDataEditor.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Editor/JigsawVinaGameDataEditor.cs):
```csharp
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("JigsawVina.Tests")]
```

**Step 2: Accessibility of State Classes & Variables (Internal)**
- Make both `EditorItemState` and `EditorTabState` and their fields internal to avoid compilation error CS0052 (inconsistent accessibility) and allow test access:
```csharp
[Serializable]
internal class EditorItemState
{
    public string filename = "";
    public string displayName = "";
    public string description = "";
    public string rarity = "common";
}

[Serializable]
internal class EditorTabState
{
    public DefaultAsset folderAsset;
    public int pictureId;
    public string idString = "";
    public string displayName = "";
    public int categoryId = 1;
    public List<EditorItemState> itemStates = new();
    
    public bool easyExpanded = true;
    public bool normalExpanded = true;
    public bool hardExpanded = true;

    public int easyCols = 6, easyRows = 4;
    public int easyCoins = 30, easyReplayCoins = 10, easyHints = 0;
    public int easyKeyRewardIndex = 0;

    public int normalCols = 8, normalRows = 6;
    public int normalCoins = 60, normalReplayCoins = 20, normalHints = 0;
    public int normalKeyRewardIndex = 0;

    public int hardCols = 12, hardRows = 8;
    public int hardCoins = 120, hardReplayCoins = 40, hardHints = 0;
    public int hardKeyRewardIndex = 0;
}

[SerializeField] internal List<EditorTabState> _tabs = new();
[SerializeField] internal List<EditorCategoryState> _categories = new();
private Vector2 _categoryScroll;
private int _mainTabSelected = 0;
```
- Make `EditorCategoryState` internal:
```csharp
[Serializable]
internal class EditorCategoryState
{
    public int id;
    public string idString = "";
    public string displayName = "";
}
```

**Step 3: Test State Injector**
- Add a test helper to inject custom state inside [JigsawVinaGameDataEditor.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Editor/JigsawVinaGameDataEditor.cs):
```csharp
internal void SetStateForTesting(List<EditorTabState> tabs, List<EditorCategoryState> categories, List<ItemDto> globalItems)
{
    _tabs = tabs ?? new();
    _categories = categories ?? new();
    _globalItems = globalItems ?? new();
}
```

**Step 4: Define Main Tab State & GUI Helper Routing**
- In `OnGUI()`, draw a global Toolbar at the top:
  - Tab Labels: `{"Cấu hình Tranh", "Quản lý Danh mục", "Quản lý Vật phẩm", "Trình sửa Save (Cheat)"}`.
  - Based on `_mainTabSelected`, call helper drawing methods:
    - `0` -> `DrawPicturesTab()`
    - `1` -> `DrawCategoriesTab()`
    - `2` -> `DrawGlobalItemsTab()`
    - `3` -> `DrawSaveTab()`
  - The "Save & Generate JSON" button remains globally visible on the top toolbar.

**Step 5: Testable DTO Hydration & File Reading (Asset-Independent Hydration)**
- Refactor `LoadFromDisk()` to call `LoadStateFromDto()`:
```csharp
private void LoadFromDisk()
{
    if (File.Exists(SavePath))
    {
        try
        {
            string json = File.ReadAllText(SavePath);
            var dto = JsonUtility.FromJson<StaticDataDto>(json);
            if (dto != null)
            {
                LoadStateFromDto(dto);
                return;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[JigsawVina Editor] Could not parse config: {e.Message}");
        }
    }
    LoadStateFromDto(new StaticDataDto());
}

internal void LoadStateFromDto(StaticDataDto dto)
{
    _tabs.Clear();
    _categories.Clear();
    _globalItems.Clear();

    // 1. Hydrate Categories first
    if (dto.categories != null)
    {
        foreach (var cat in dto.categories)
        {
            _categories.Add(new EditorCategoryState
            {
                id = cat.id,
                idString = cat.id_string,
                displayName = cat.display_name
            });
        }
    }

    // 2. Hydrate Picture tabs
    if (dto.pictures != null)
    {
        var diffsByPic = new Dictionary<int, List<PictureDifficultyDto>>();
        if (dto.picture_difficulties != null)
        {
            foreach (var diff in dto.picture_difficulties)
            {
                if (!diffsByPic.ContainsKey(diff.picture_id))
                    diffsByPic[diff.picture_id] = new List<PictureDifficultyDto>();
                diffsByPic[diff.picture_id].Add(diff);
            }
        }

        foreach (var pic in dto.pictures)
        {
            var state = new EditorTabState
            {
                pictureId = pic.id,
                idString = pic.id_string,
                displayName = pic.display_name,
                categoryId = pic.category_id != 0 ? pic.category_id : 1
            };

            // Reconstruct folder asset path
            if (!string.IsNullOrEmpty(pic.asset_path))
            {
                string relativeDir = Path.GetDirectoryName(pic.asset_path).Replace("\\", "/");
                string folderPath = $"Assets/Resources/{relativeDir}";
                state.folderAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(folderPath);
            }

            // Sync item states from DTO (Filter generated key items, safety 64-bit int logic to prevent overflow)
            state.itemStates.Clear();
            if (dto.items != null)
            {
                long picId = pic.id;
                foreach (var item in dto.items)
                {
                    long itemId = item.id;
                    if (itemId > picId * 100 && itemId < (picId + 1) * 100 && item.item_type == "key_item")
                    {
                        string filename = Path.GetFileNameWithoutExtension(item.asset_path);
                        state.itemStates.Add(new EditorItemState
                        {
                            filename = filename,
                            displayName = item.display_name,
                            description = item.description,
                            rarity = string.IsNullOrEmpty(item.rarity) ? "common" : item.rarity
                        });
                    }
                }
            }

            // If folderAsset exists, do production sync to scan new folder items
            if (state.folderAsset != null)
            {
                var (_, scannedItems) = ScanFolder(state.folderAsset);
                SyncItemStates(state, scannedItems);
            }

            // 3. Hydrate Difficulty settings (Completely independent of folderAsset existence)
            if (diffsByPic.TryGetValue(pic.id, out var picDiffs))
            {
                foreach (var d in picDiffs)
                {
                    int rewardIdx = 0;
                    if (d.first_clear_reward_item_ids != null && d.first_clear_reward_item_ids.Count > 0)
                    {
                        int rewardId = d.first_clear_reward_item_ids[0];
                        int calculatedIndex = (rewardId - pic.id * 100) - 1;

                        if (state.folderAsset != null)
                        {
                            // Production scan index matching
                            var (_, scannedItems) = ScanFolder(state.folderAsset);
                            if (calculatedIndex >= 0 && calculatedIndex < scannedItems.Count)
                            {
                                rewardIdx = calculatedIndex + 1;
                            }
                        }
                        else
                        {
                            // Fallback mock mapping using itemStates sorted alphabetically using Ordinal comparer
                            state.itemStates.Sort((a, b) => string.Compare(a.filename, b.filename, StringComparison.Ordinal));
                            if (calculatedIndex >= 0 && calculatedIndex < state.itemStates.Count)
                            {
                                rewardIdx = calculatedIndex + 1;
                            }
                        }
                    }

                    if (d.difficulty_id == 0)
                    {
                        state.easyCols = d.grid_columns;
                        state.easyRows = d.grid_rows;
                        state.easyCoins = d.first_clear_coin;
                        state.easyReplayCoins = d.replay_coin;
                        state.easyHints = d.first_clear_hint;
                        state.easyKeyRewardIndex = rewardIdx;
                    }
                    else if (d.difficulty_id == 1)
                    {
                        state.normalCols = d.grid_columns;
                        state.normalRows = d.grid_rows;
                        state.normalCoins = d.first_clear_coin;
                        state.normalReplayCoins = d.replay_coin;
                        state.normalHints = d.first_clear_hint;
                        state.normalKeyRewardIndex = rewardIdx;
                    }
                    else if (d.difficulty_id == 2)
                    {
                        state.hardCols = d.grid_columns;
                        state.hardRows = d.grid_rows;
                        state.hardCoins = d.first_clear_coin;
                        state.hardReplayCoins = d.replay_coin;
                        state.hardHints = d.first_clear_hint;
                        state.hardKeyRewardIndex = rewardIdx;
                    }
                }
            }
            _tabs.Add(state);
        }
    }

    // 4. Hydrate Global Items (Exclude key items via persisted metadata: item_type == "key_item")
    if (dto.items != null)
    {
        foreach (var item in dto.items)
        {
            if (item.item_type != "key_item")
            {
                _globalItems.Add(item);
            }
        }
    }

    // 5. Ensure Default Categories and Reserved Items exist (Deduplicated order)
    if (_categories.Count == 0)
    {
        _categories.Add(new EditorCategoryState
        {
            id = 1,
            idString = "vietnam_landscapes",
            displayName = "Phong Cảnh Việt Nam"
        });
    }
    EnsureReservedItems();

    if (_tabs.Count == 0)
    {
        _tabs.Add(new EditorTabState { pictureId = 1, categoryId = _categories[0].id });
    }
}
```

**Step 6: Render the Category selection popup in Picture Tab details**
- In `DrawTabDetails` (inside Picture Tab), draw the category dropdown and update `state.categoryId`.

**Step 7: Category Deletion Safety Check**
- Implement `internal bool CanDeleteCategory(int categoryId, out string reason)`:
  - If `_categories.Count <= 1`, set `reason = "Không thể xóa danh mục cuối cùng."` and return false.
  - Scan all `_tabs` (pictures). If any picture has `categoryId == categoryId`, set `reason = $"Không thể xóa vì danh mục đang được sử dụng."` and return false.
  - Otherwise, return true.

---

### Task 2: Global Item Database Editor & Testable Validation

**Files:**
- Modify: [JigsawVinaGameDataEditor.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Editor/JigsawVinaGameDataEditor.cs)

**Step 1: Add Global Item database structure using ItemDto**
- Use the full `ItemDto` class directly for our database list:
```csharp
[SerializeField] internal List<ItemDto> _globalItems = new();
private Vector2 _itemsScroll;
```

**Step 2: Scanned Item Isolation**
- Scanned items are generated from disk and always have `item_type == "key_item"`. By excluding `key_item` from `_globalItems` in `LoadStateFromDto()`, we achieve total isolation from scan disk dependencies.

**Step 3: Ensure Reserved Items**
- Implement `internal void EnsureReservedItems()`:
  - If `coin` (ID 1) does not exist in `_globalItems` (check `!_globalItems.Exists(i => i.id == 1)`), insert/append it with full defaults (is_consumable = true, max_stack = 999999, status = "active", etc.).
  - If `hint` (ID 2) does not exist in `_globalItems` (check `!_globalItems.Exists(i => i.id == 2)`), insert/append it with full defaults (is_consumable = true, max_stack = 9999, status = "active", etc.).

**Step 4: Testable Build Config & Pure Validation (No UI dialogs)**
- Extract validation and DTO mapping logic into `internal bool TryBuildConfig(out StaticDataDto config, out string errorMessage, bool validateAssets = true)`:
  - **No UI Dialogs inside this method.** Any validation failure must set `errorMessage = [details]` and return `false`.
  - Validate Category data:
    - Ensure all Category IDs are strictly positive (`id > 0`).
    - Ensure `_categories` has no duplicate Category IDs or duplicate/empty `id_string`s.
    - Ensure all pictures reference a category ID that exists in `_categories`.
  - Validate Picture data:
    - Validate that all picture IDs are strictly positive (`id > 0`) and less than `20,000,000` (to prevent integer overflow when calculating scanned item IDs).
    - Validate that there are no duplicate picture IDs among pictures.
    - Validate that there are no empty or duplicate picture `id_string` values among pictures.
  - Validate Items data:
    - Validate that all global item IDs are strictly positive (`id > 0`).
    - Loop through `_tabs`. 
      - If `validateAssets` is true:
        - Verify `tab.folderAsset` is not null and is inside `Assets/Resources/`.
        - Scan folder. Verify main texture starting with `MAIN_` is present.
        - Sort scanned item textures alphabetically using `StringComparison.Ordinal` to ensure stable ID generation.
        - Validate that scanned key item count does not exceed 99 items (cannot have index >= 99, otherwise it overflows ID namespace). If items.Count > 99, return false with error message.
        - Calculate scanned key items using stable ID formula: `tab.pictureId * 100 + (itemIndex + 1)`.
      - If `validateAssets` is false (e.g. in EditMode tests):
        - Sort `tab.itemStates` alphabetically by `filename` using `StringComparison.Ordinal` before assigning IDs to maintain perfect order stability:
          `tab.itemStates.Sort((a, b) => string.Compare(a.filename, b.filename, StringComparison.Ordinal));`
        - Validate that `tab.itemStates.Count` does not exceed 99 items. If `tab.itemStates.Count > 99`, return false.
        - Reconstruct mock scanned key items from sorted `tab.itemStates` directly using the stable ID formula `tab.pictureId * 100 + (itemIndex + 1)` and mock asset paths.
    - Validate that there are no duplicate IDs or duplicate `id_string` values across the union of all global items and all scanned key items.
    - Validate that no global item in `_globalItems` has `item_type == "key_item"`. (Key items are reserved strictly for picture completion rewards).
    - Ensure reserved items `coin` (ID 1) and `hint` (ID 2) exist in global items, have correct types, and are not renamed in ID/id_string/item_type.
  - If validation succeeds, return `true` and populate the output `config` DTO containing the mapped categories, pictures, difficulties, global items, and scanned items.
- In `SaveConfig()` (UI button click only):
  - Call `TryBuildConfig(out var config, out var err, true)`. If false, display `EditorUtility.DisplayDialog("Lỗi Cấu Hình", err, "OK")` and abort. If true, serialize and write JSON to disk.

**Step 5: Global Items GUI layout & Reserved Items Protection**
- In `DrawGlobalItemsTab()`, draw the list of items.
- **Reserved Item Protections & Constraints**:
  - For `coin` (ID 1) and `hint` (ID 2), disable `id`, `id_string`, and `item_type` fields in the GUI. Only allow editing display name, description, max_stack, etc.
  - Disable the "Xóa" button for ID 1 and ID 2.
  - For all other global items, the `item_type` selection popup must exclude `"key_item"` from the choices (allowing only `currency`, `consumable`, `collectible`, etc.).
- **Item ID Calculation**:
  - Implement `internal List<int> GetActiveItemIds(bool scanFolders)`:
    - Add all `_globalItems` IDs.
    - For each picture tab in `_tabs`:
      - If `scanFolders` is true and `tab.folderAsset != null`:
        - Scan folder, sort item textures alphabetically using `StringComparison.Ordinal`, and add `tab.pictureId * 100 + (itemIndex + 1)`.
      - Else:
        - Sort `tab.itemStates` alphabetically by `filename` using `StringComparison.Ordinal`, and add `tab.pictureId * 100 + (itemIndex + 1)`.
  - When "Thêm Vật phẩm mới" is clicked, calculate the next safe ID using `internal int GetNextAvailableItemId()`:
    - Calculate used IDs using `GetActiveItemIds(scanFolders: true)`.
    - Start at 1. Increment ID until it is not used in the active IDs set.
  - Assign default values to the new `ItemDto` containing complete non-empty values:
    - `id` = next available ID.
    - `id_string` = `$"new_item_{id}"`
    - `display_name` = `$"Vật phẩm mới {id}"`
    - `description` = `""`
    - `display_name_key` = `$"item.new_item_{id}.name"`
    - `description_key` = `$"item.new_item_{id}.description"`
    - `item_type` = `"collectible"`
    - `rarity` = `"common"`
    - `is_consumable` = `false`
    - `is_time_limited` = `false`
    - `max_stack` = `1`
    - `status` = `"active"`
    - `sort_order` = `id`
    - `asset_path` = `""`

---

### Task 3: Player Save & Cheat Editor

**Files:**
- Modify: [JigsawVinaGameDataEditor.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Editor/JigsawVinaGameDataEditor.cs)

**Step 1: Load/Save Normalization**
- Inside `LoadPlayerSave()`:
  - After deserialization, ensure collections are initialized if null:
    ```csharp
    _cachedSave.CompletedPuzzles ??= new List<CompletedPuzzleData>();
    _cachedSave.OwnedItemIds ??= new List<int>();
    ```

**Step 2: Idempotent Unlock All & Stale Deletion**
- Implement `internal void ApplyUnlockAll(PlayerSave save)`:
  - Save guarding: ensure `save.CompletedPuzzles ??= new List<CompletedPuzzleData>();`.
  - Create a new `List<CompletedPuzzleData> newCompletions = new();`
  - Loop through all active pictures in `_tabs`:
    - For each difficulty level (0, 1, 2):
      - Find an existing completed puzzle record in the loaded `save.CompletedPuzzles` matching `PictureId == tab.pictureId` and `DifficultyId == difficultyId`.
      - If found, add it to `newCompletions` while setting `BestStar = 3` and `BestTimeSeconds = 45.0f`.
      - If not found, add a new `CompletedPuzzleData` with `PictureId = tab.pictureId`, `DifficultyId = difficultyId`, `BestStar = 3`, `BestTimeSeconds = 45.0f`.
  - Reassign `save.CompletedPuzzles = newCompletions;` (effectively purging any duplicate records or stale records representing deleted pictures or out-of-range difficulty IDs).

**Step 3: Reset Player Save**
- Implement `internal void ResetPlayerSave()`:
  - Deletes only the targeted key using `PlayerPrefs.DeleteKey(SaveDataService.SaveKey)`.
  - Wipes memory cache: `_cachedSave = new PlayerSave(); _saveLoaded = true;`.
  - Call `PlayerPrefs.Save()` to commit deletion.
  - **Do NOT call `SavePlayerSave()` inside this method** (so key remains deleted on disk until explicitly saved again).
- UI button handler: When clicked, prompt using `EditorUtility.DisplayDialog`. If yes, call `ResetPlayerSave()`.

---

### Verification Plan & Automated Tests

We will write automated tests to verify the core data logic without showing modal UI dialogs.

#### Automated Editor Tests
We will create EditMode tests in [JigsawVinaGameDataEditorTests.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Tests/JigsawVinaGameDataEditorTests.cs):

- `[SetUp]`: Create test editor window using `ScriptableObject.CreateInstance<JigsawVinaGameDataEditor>()` to ensure isolation and avoid affecting the developer's docked UI layouts.
- `[TearDown]`: Clean up window using `DestroyImmediate(window)`. Clear specific PlayerPrefs keys used in tests (the save key and dummy keys).

1. **LoadSaveRoundTripPreservesAllFieldsTest**:
   - Create editor window instance, inject mock state or load a mock DTO containing custom global items with complex fields, call `TryBuildConfig(out var config, out var err, false)` (bypassing assets), and verify all 14 fields of `ItemDto` (including `id`, `id_string`, `display_name`, `description`, `display_name_key`, `description_key`, `item_type`, `rarity`, `is_consumable`, `is_time_limited`, `max_stack`, `status`, `sort_order`, `asset_path`) are perfectly preserved.
2. **CategoryRoundTripPreservesCategoryIdTest**:
   - Verify that when a picture state is loaded with `category_id = 3`, `TryBuildConfig(..., false)` preserves that specific `category_id` in the output `PictureDto`.
3. **LoadStateFromDtoWithoutAssetsPreservesDifficultySettingsTest**:
   - Verify that loading a DTO with missing folder assets correctly populates all difficulty settings (columns, rows, coins, hints, replay coins) in memory.
   - Verify that the `first_clear_reward_item_ids` are mapped to `easy/normal/hardKeyRewardIndex` correctly based on alphabetical mock matching.
   - Verify that calling `TryBuildConfig(..., false)` writes them back to `first_clear_reward_item_ids` without loss.
4. **DuplicateIDValidationTest**:
   - Inject duplicate global item ID or duplicate `id_string` into the editor window state, call `TryBuildConfig(..., false)`, verify that validation fails (returns false) and outputs the correct duplicate error message.
5. **PositiveIDValidationTest**:
   - Inject non-positive IDs (0 or negative) for picture, category, or global item, call `TryBuildConfig(..., false)`, verify that validation fails (returns false).
6. **KeyItemCountLimitValidationTest**:
   - Inject 100 key item states under a single picture, call `TryBuildConfig(..., false)`, verify that validation fails (returns false) and blocks saving to prevent key item ID overflow.
7. **CategoryDeletionSafetyTest**:
   - Add a Category, assign it to a picture tab, call `CanDeleteCategory(categoryId, out string reason)`, verify it returns false and provides the correct reference error message.
8. **UnlockAllCheatIsIdempotentAndCleansStaleTest**:
   - Initialize `PlayerSave` with duplicate completions for active pictures, and some stale completions representing deleted picture IDs. Call `ApplyUnlockAll()`. Verify the number of completions is exactly equal to `ActivePicturesCount * 3` and contains no duplicate or stale records.
9. **ResetSaveOnlyTargetedKeyTest**:
   - Set a dummy key in PlayerPrefs, set `SaveDataService.SaveKey`, run the reset logic `ResetPlayerSave()`, verify `SaveKey` is deleted but the dummy key remains intact.
10. **EnsureReservedItemsSeededOnEmptyLoadTest**:
   - Load an empty `StaticDataDto` via `LoadStateFromDto(dto)`, verify `EnsureReservedItems()` is executed and `coin` (ID 1) and `hint` (ID 2) are automatically seeded.

#### Manual Verification
- Open the editor window, switch tabs, test category and global item additions.
- Rename category ID to a duplicate, try to save, verify the error dialog blocks it.
- Delete a category with pictures, verify it is blocked.
- Click "Unlock All" twice, click save, verify the JSON string in PlayerPrefs is clean and contains no duplicate records.
- Run EditMode tests inside Unity Test Runner to confirm everything passes cleanly.

#### Post-Implementation Task
- Update task tracker: [task.md](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/docs/plans/task.md)
- Update handoff: [current-handoff.md](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/docs/plans/current-handoff.md)
