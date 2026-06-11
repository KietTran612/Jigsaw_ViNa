# Jigsaw Vina Editor & JSON Static Data Pipeline Implementation Plan

> **For Antigravity:** REQUIRED WORKFLOW: Use `.agent/workflows/execute-plan.md` to execute this plan in single-flow mode.

**Goal:** Create a Unity Editor Window to scan picture folders inside `Assets/Resources/`, map the main picture and item sprites, allow editing metadata and rewards (first-clear coins, replay coins, hints, and key item rewards) for 3 difficulties, and save them to a backend-compatible single JSON static data file parsed, validated, and rewarded at runtime.

**Architecture:**
1. **DTO & Model Extension**:
   - Extend `PlayerSave.cs` to store a list of owned key item IDs (`OwnedItemIds`).
   - Define DTO classes (`StaticDataDto`, `CategoryDto`, `PictureDto`, `ItemDto`, `PictureDifficultyDto`) in `StaticDataDto.cs` matching `snake_case` JSON properties.
   - Extend `PictureDifficultyConfig.cs` to include first-clear coins, replay coins, hints, and reward key item IDs.
2. **TDD Verification**:
   - Write failing EditMode unit tests in `StaticDataServiceTests.cs` verifying duplicate ID/string detections, missing references, and reward calculations.
   - Write failing unit tests in `ProgressionTests.cs` verifying that clear rewards (first-clear and replay coins, hints, and key items) are processed and persisted correctly.
3. **Runtime Service & Presenter Integration**:
   - Implement `StaticDataService.cs` to read `StaticData.json`, validate structure defensively, parse into DTOs, convert to configurations, and support fallback data.
   - Modify `RewardSummaryPresenter.cs` to check for first clear, grant coins/hints/key-items configured in static data, track actual coins in `GameSessionService`, and save progress.
4. **Editor Window Setup**:
   - Create `JigsawVinaGameDataEditor.cs` under `Editor/` to render a 5-tab split window (Left: Assets & Metadata, Right: Difficulty & Rewards settings) and serialize settings to `Assets/Resources/StaticData.json`.
   - Implement auto-scanning (alphabetically sorted items), stable ID formula `itemId = pictureId * 100 + (itemIndex + 1)`, duplicate validation, and pre-fill deserialization.
5. **Presentation Layer**:
   - Modify `PuzzlePlayingPresenter.cs` to load board textures dynamically from `picture.AssetPath` with resource paths relative to `Resources/` and safe fallback handling.

**Tech Stack:** Unity 6000.3.11f1, Unity EditorGUI layout, JsonUtility.

---

## Proposed Changes

### [Static Data & Player Save Models]

#### [MODIFY] [PlayerSave.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Data/PlayerSave.cs)
Extend player save to include owned key item IDs.

#### [NEW] [StaticDataDto.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Data/StaticDataDto.cs)
Create serialization DTO classes for categories, pictures, items, and picture difficulties.

#### [MODIFY] [PictureDifficultyConfig.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Data/PictureDifficultyConfig.cs)
Extend difficulty configurations to store coin, hint, and key-item rewards.

---

### [Services & Presenters]

#### [MODIFY] [GameSessionService.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/GameSessionService.cs)
Add `LastCoinEarned` property to track exact coins earned this session.

#### [MODIFY] [IStaticDataService.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/IStaticDataService.cs)
Add key item lookup methods to static data service interface.

#### [MODIFY] [StaticDataService.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/StaticDataService.cs)
Implement dynamic loading, validation, indexing, and fallback data injection.

#### [MODIFY] [RewardSummaryPresenter.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/RewardSummaryPresenter.cs)
Update presenter to award first-clear coins, replay coins, hints, and key-item rewards using static data, then track coins.

#### [MODIFY] [PuzzlePlayingPresenter.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/PuzzlePlayingPresenter.cs)
Update texture loading to use the asset path configured in static data with safe fallbacks.

---

### [Editor Setup Window]

#### [NEW] [JigsawVinaGameDataEditor.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Editor/JigsawVinaGameDataEditor.cs)
Create Editor Window displaying folder scanning, metadata, difficulty rewards, pre-fill logic, and JSON saving.

---

### [Tests]

#### [NEW] [StaticDataServiceTests.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Tests/StaticDataServiceTests.cs)
Unit tests for JSON parsing, integrity validations, duplicate IDs, missing references, and fallback data.

#### [MODIFY] [ProgressionTests.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Tests/ProgressionTests.cs)
Extend progression tests to verify first-clear clear rewards, replay clear rewards, and item reward persistence.

---

## Tasks

### Task 1: Extend Core Models & Setup DTOs

**Files:**
* Modify: `JigsawVina/Assets/JigsawVina/Scripts/Core/Data/PlayerSave.cs:1-39`
* Modify: `JigsawVina/Assets/JigsawVina/Scripts/Core/Data/PictureDifficultyConfig.cs:1-31`
* Create: `JigsawVina/Assets/JigsawVina/Scripts/Core/Data/StaticDataDto.cs`
* Modify: `JigsawVina/Assets/JigsawVina/Scripts/Core/Services/GameSessionService.cs:1-31`

**Step 1: Modify `PlayerSave.cs`**
Add list of owned key item IDs:
```csharp
    [Serializable]
    public class PlayerSave
    {
        public int Coins;
        public int Hints;
        public List<CompletedPuzzleData> CompletedPuzzles = new();
        public List<int> OwnedItemIds = new();
    }
```

**Step 2: Modify `PictureDifficultyConfig.cs`**
Add coins, hints, and key item rewards:
```csharp
using System.Collections.Generic;

namespace JigsawVina.Core.Data
{
    public readonly struct PictureDifficultyConfig
    {
        public readonly int PictureId;
        public readonly int DifficultyId;
        public readonly string DisplayName;
        public readonly int Columns;
        public readonly int Rows;
        public readonly int StarReward;
        public readonly int FirstClearCoin;
        public readonly int FirstClearHint;
        public readonly int ReplayCoin;
        public readonly IReadOnlyList<int> FirstClearRewardItemIds;

        public int PieceCount => Columns * Rows;

        public PictureDifficultyConfig(
            int pictureId,
            int difficultyId,
            string displayName,
            int columns,
            int rows,
            int starReward,
            int firstClearCoin,
            int firstClearHint,
            int replayCoin,
            IReadOnlyList<int> firstClearRewardItemIds)
        {
            PictureId = pictureId;
            DifficultyId = difficultyId;
            DisplayName = displayName;
            Columns = columns;
            Rows = rows;
            StarReward = starReward;
            FirstClearCoin = firstClearCoin;
            FirstClearHint = firstClearHint;
            ReplayCoin = replayCoin;
            FirstClearRewardItemIds = firstClearRewardItemIds ?? new List<int>();
        }
    }
}
```

**Step 3: Modify `GameSessionService.cs`**
Add `LastCoinEarned` field:
```csharp
        public int LastCoinEarned { get; set; }
```

**Step 4: Create `StaticDataDto.cs`**
Define serialization DTO structures using snake_case properties:
```csharp
using System;
using System.Collections.Generic;

namespace JigsawVina.Core.Data
{
    [Serializable]
    public class CategoryDto
    {
        public int id;
        public string id_string;
        public string display_name;
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
    }

    [Serializable]
    public class ItemDto
    {
        public int id;
        public string id_string;
        public string display_name;
        public string item_type;
        public string asset_path;
    }

    [Serializable]
    public class PictureDifficultyDto
    {
        public int picture_id;
        public int difficulty_id;
        public string display_name;
        public int grid_columns;
        public int grid_rows;
        public int piece_count;
        public int star_reward;
        public int first_clear_coin;
        public int first_clear_hint;
        public int replay_coin;
        public List<int> first_clear_reward_item_ids = new();
    }

    [Serializable]
    public class StaticDataDto
    {
        public int schema_version = 1;
        public int data_version = 1;
        public List<CategoryDto> categories = new();
        public List<PictureDto> pictures = new();
        public List<ItemDto> items = new();
        public List<PictureDifficultyDto> picture_difficulties = new();
    }
}
```

**Step 5: Verify compilation**
Wait for Unity to compile and verify there are no C# syntax errors.

---

### Task 2: Write Failing Unit & Integration Tests

**Files:**
* Create: `JigsawVina/Assets/JigsawVina/Tests/StaticDataServiceTests.cs`
* Modify: `JigsawVina/Assets/JigsawVina/Tests/ProgressionTests.cs:1-98`

**Step 1: Write `StaticDataServiceTests.cs`**
Assert json parsing, validation exceptions for duplicates, missing references, and fallback data initialization:
```csharp
using NUnit.Framework;
using JigsawVina.Core.Services;
using JigsawVina.Core.Data;
using System;
using System.Collections.Generic;

namespace JigsawVina.Tests
{
    public class StaticDataServiceTests
    {
        [Test]
        public void LoadFromText_ValidJson_ParsesSuccessfully()
        {
            var json = @"{
                ""schema_version"": 1,
                ""data_version"": 1,
                ""pictures"": [
                    { ""id"": 1, ""id_string"": ""pic1"", ""display_name"": ""Pic 1"", ""category_id"": 1, ""asset_path"": ""Textures/pic1"" }
                ],
                ""items"": [
                    { ""id"": 101, ""id_string"": ""item1"", ""display_name"": ""Item 1"", ""item_type"": ""key_item"", ""asset_path"": ""Items/item1"" }
                ],
                ""picture_difficulties"": [
                    { ""picture_id"": 1, ""difficulty_id"": 0, ""display_name"": ""Easy"", ""grid_columns"": 6, ""grid_rows"": 4, ""piece_count"": 24, ""star_reward"": 1, ""first_clear_coin"": 30, ""first_clear_hint"": 5, ""replay_coin"": 10, ""first_clear_reward_item_ids"": [101] }
                ]
            }";

            var service = new StaticDataService(false);
            service.LoadFromText(json);

            var pictures = service.GetAllPictures();
            Assert.AreEqual(1, pictures.Count);
            Assert.AreEqual("Pic 1", pictures[0].DisplayName);

            var diff = service.GetPictureDifficulty(1, 0);
            Assert.AreEqual(6, diff.Columns);
            Assert.AreEqual(4, diff.Rows);
            Assert.AreEqual(30, diff.FirstClearCoin);
            Assert.AreEqual(5, diff.FirstClearHint);
            Assert.AreEqual(10, diff.ReplayCoin);
            Assert.AreEqual(1, diff.FirstClearRewardItemIds.Count);
            Assert.AreEqual(101, diff.FirstClearRewardItemIds[0]);

            var item = service.GetItemById(101);
            Assert.IsNotNull(item);
            Assert.AreEqual("Item 1", item.display_name);
        }

        [Test]
        public void LoadFromText_DuplicatePictureId_ThrowsException()
        {
            var json = @"{
                ""pictures"": [
                    { ""id"": 1, ""id_string"": ""pic1"", ""display_name"": ""Pic 1"" },
                    { ""id"": 1, ""id_string"": ""pic2"", ""display_name"": ""Pic 2"" }
                ]
            }";

            var service = new StaticDataService(false);
            Assert.Throws<InvalidOperationException>(() => service.LoadFromText(json));
        }

        [Test]
        public void LoadFromText_DuplicatePictureIdString_ThrowsException()
        {
            var json = @"{
                ""pictures"": [
                    { ""id"": 1, ""id_string"": ""pic1"", ""display_name"": ""Pic 1"" },
                    { ""id"": 2, ""id_string"": ""pic1"", ""display_name"": ""Pic 2"" }
                ]
            }";

            var service = new StaticDataService(false);
            Assert.Throws<InvalidOperationException>(() => service.LoadFromText(json));
        }

        [Test]
        public void LoadFromText_DuplicateItemId_ThrowsException()
        {
            var json = @"{
                ""items"": [
                    { ""id"": 101, ""id_string"": ""item1"", ""display_name"": ""Item 1"" },
                    { ""id"": 101, ""id_string"": ""item2"", ""display_name"": ""Item 2"" }
                ]
            }";

            var service = new StaticDataService(false);
            Assert.Throws<InvalidOperationException>(() => service.LoadFromText(json));
        }

        [Test]
        public void LoadFromText_DuplicateItemIdString_ThrowsException()
        {
            var json = @"{
                ""items"": [
                    { ""id"": 101, ""id_string"": ""item1"", ""display_name"": ""Item 1"" },
                    { ""id"": 102, ""id_string"": ""item1"", ""display_name"": ""Item 2"" }
                ]
            }";

            var service = new StaticDataService(false);
            Assert.Throws<InvalidOperationException>(() => service.LoadFromText(json));
        }

        [Test]
        public void LoadFromText_GridMismatch_ThrowsException()
        {
            var json = @"{
                ""pictures"": [
                    { ""id"": 1, ""id_string"": ""pic1"", ""display_name"": ""Pic 1"" }
                ],
                ""picture_difficulties"": [
                    { ""picture_id"": 1, ""difficulty_id"": 0, ""display_name"": ""Easy"", ""grid_columns"": 6, ""grid_rows"": 4, ""piece_count"": 10 }
                ]
            }";

            var service = new StaticDataService(false);
            Assert.Throws<InvalidOperationException>(() => service.LoadFromText(json));
        }

        [Test]
        public void LoadFromText_DifficultyRewardsMissingItem_ThrowsException()
        {
            var json = @"{
                ""pictures"": [
                    { ""id"": 1, ""id_string"": ""pic1"", ""display_name"": ""Pic 1"" }
                ],
                ""picture_difficulties"": [
                    { ""picture_id"": 1, ""difficulty_id"": 0, ""display_name"": ""Easy"", ""grid_columns"": 6, ""grid_rows"": 4, ""piece_count"": 24, ""first_clear_reward_item_ids"": [999] }
                ]
            }";

            var service = new StaticDataService(false);
            Assert.Throws<InvalidOperationException>(() => service.LoadFromText(json));
        }
    }
}
```

**Step 2: Update `ProgressionTests.cs`**
Extend tests to verify rewards (first-clear coins, replay coins, hints, key items) are applied correctly under the mock save and static data services:
```csharp
using JigsawVina.Core.Data;
using JigsawVina.Core.Services;
using JigsawVina.Presentation.Screens;
using NUnit.Framework;
using System.Collections.Generic;

namespace JigsawVina.Tests
{
    public class MockSaveDataService : ISaveDataService
    {
        public PlayerSave SaveData = new();

        public PlayerSave Load()
        {
            return SaveData;
        }

        public void Save(PlayerSave save)
        {
            SaveData = save;
        }
    }

    public class ProgressionTests
    {
        [Test]
        public void ProcessRewards_FirstClear_AwardsFirstClearCoinsHintsAndItems()
        {
            var saveService = new MockSaveDataService();
            saveService.SaveData.Coins = 100;
            saveService.SaveData.Hints = 2;

            var session = new GameSessionService();
            session.SetSelectedPicture(1);
            session.SetSelectedDifficulty(1);

            // Construct service manually with test json
            var staticDataService = new StaticDataService(false);
            var json = @"{
                ""pictures"": [
                    { ""id"": 1, ""id_string"": ""pic1"", ""display_name"": ""Pic 1"", ""asset_path"": ""Textures/pic1"" }
                ],
                ""items"": [
                    { ""id"": 101, ""id_string"": ""item1"", ""display_name"": ""Item 1"", ""item_type"": ""key_item"", ""asset_path"": ""Items/item1"" }
                ],
                ""picture_difficulties"": [
                    { ""picture_id"": 1, ""difficulty_id"": 1, ""display_name"": ""Medium"", ""grid_columns"": 8, ""grid_rows"": 6, ""piece_count"": 48, ""star_reward"": 2, ""first_clear_coin"": 60, ""first_clear_hint"": 3, ""replay_coin"": 20, ""first_clear_reward_item_ids"": [101] }
                ]
            }";
            staticDataService.LoadFromText(json);

            var presenter = new RewardSummaryPresenter(null, session, saveService, staticDataService);
            presenter.ProcessRewardsAndDisplay(12f);

            var save = saveService.Load();
            Assert.AreEqual(1, save.CompletedPuzzles.Count);
            Assert.AreEqual(100 + 60, save.Coins); // 100 base + 60 first clear
            Assert.AreEqual(2 + 3, save.Hints);   // 2 base + 3 hints
            Assert.Contains(101, save.OwnedItemIds); // Rewarded item
            Assert.AreEqual(60, session.LastCoinEarned);
        }

        [Test]
        public void ProcessRewards_Replay_AwardsReplayCoinsOnly()
        {
            var saveService = new MockSaveDataService();
            saveService.SaveData.Coins = 100;
            saveService.SaveData.Hints = 2;
            saveService.SaveData.OwnedItemIds = new List<int> { 101 };
            saveService.SaveData.CompletedPuzzles.Add(new CompletedPuzzleData
            {
                PictureId = 1,
                DifficultyId = 1,
                BestTimeSeconds = 30f,
                BestStar = 1
            });

            var session = new GameSessionService();
            session.SetSelectedPicture(1);
            session.SetSelectedDifficulty(1);

            var staticDataService = new StaticDataService(false);
            var json = @"{
                ""pictures"": [
                    { ""id"": 1, ""id_string"": ""pic1"", ""display_name"": ""Pic 1"", ""asset_path"": ""Textures/pic1"" }
                ],
                ""items"": [
                    { ""id"": 101, ""id_string"": ""item1"", ""display_name"": ""Item 1"", ""item_type"": ""key_item"", ""asset_path"": ""Items/item1"" }
                ],
                ""picture_difficulties"": [
                    { ""picture_id"": 1, ""difficulty_id"": 1, ""display_name"": ""Medium"", ""grid_columns"": 8, ""grid_rows"": 6, ""piece_count"": 48, ""star_reward"": 2, ""first_clear_coin"": 60, ""first_clear_hint"": 3, ""replay_coin"": 20, ""first_clear_reward_item_ids"": [101] }
                ]
            }";
            staticDataService.LoadFromText(json);

            var presenter = new RewardSummaryPresenter(null, session, saveService, staticDataService);
            presenter.ProcessRewardsAndDisplay(15f);

            var save = saveService.Load();
            Assert.AreEqual(1, save.CompletedPuzzles.Count);
            Assert.AreEqual(100 + 20, save.Coins); // Replay coin only
            Assert.AreEqual(2, save.Hints);        // Hints not awarded on replay
            Assert.AreEqual(20, session.LastCoinEarned);
        }
    }
}
```

**Step 3: Run the test suite and verify they fail**
Run the tests using the Unity Test Runner (or `dotnet test` equivalent command).
Expected: Compile error on missing methods/fields, or fails with failures once compiled.

---

### Task 3: Implement JSON loading, validation, and reward logic

**Files:**
* Modify: `JigsawVina/Assets/JigsawVina/Scripts/Core/Services/IStaticDataService.cs:1-13`
* Modify: `JigsawVina/Assets/JigsawVina/Scripts/Core/Services/StaticDataService.cs:1-51`
* Modify: `JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/RewardSummaryPresenter.cs:1-87`

**Step 1: Implement lookup methods in `IStaticDataService.cs`**
```csharp
using System.Collections.Generic;
using JigsawVina.Core.Data;

namespace JigsawVina.Core.Services
{
    public interface IStaticDataService
    {
        IReadOnlyList<PictureConfig> GetAllPictures();
        PictureConfig GetPictureById(int id);
        PictureDifficultyConfig GetPictureDifficulty(int pictureId, int difficultyId);
        ItemDto GetItemById(int id);
        IReadOnlyList<ItemDto> GetAllItems();
    }
}
```

**Step 2: Update `StaticDataService.cs`**
Implement the JSON loading, fallback data, lookup indexers, and validation rules:
```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using JigsawVina.Core.Data;
using UnityEngine;

namespace JigsawVina.Core.Services
{
    public class StaticDataService : IStaticDataService
    {
        private const string StaticDataResourcePath = "StaticData";
        private List<PictureConfig> _pictures = new();
        private List<ItemDto> _items = new();
        private Dictionary<int, ItemDto> _itemsById = new();
        private Dictionary<(int PictureId, int DifficultyId), PictureDifficultyConfig> _difficulties = new();

        public StaticDataService() : this(true)
        {
        }

        public StaticDataService(bool loadImmediately)
        {
            if (loadImmediately)
            {
                LoadFromJSON();
            }
        }

        public void LoadFromJSON()
        {
            var textAsset = Resources.Load<TextAsset>(StaticDataResourcePath);
            if (textAsset == null)
            {
                LoadFallbackData();
                return;
            }
            LoadFromText(textAsset.text);
            if (_pictures.Count == 0)
            {
                LoadFallbackData();
            }
        }

        private void LoadFallbackData()
        {
            _pictures = new List<PictureConfig>
            {
                new PictureConfig(1, "ho_guom", "Hồ Gươm", "Textures/ho_guom"),
                new PictureConfig(2, "ha_long", "Vịnh Hạ Long", "Textures/ha_long")
            };

            _difficulties = new Dictionary<(int, int), PictureDifficultyConfig>
            {
                [(1, 0)] = new PictureDifficultyConfig(1, 0, "Dễ", 6, 4, 1, 30, 0, 10, new List<int>()),
                [(1, 1)] = new PictureDifficultyConfig(1, 1, "Trung bình", 8, 6, 2, 60, 0, 20, new List<int>()),
                [(1, 2)] = new PictureDifficultyConfig(1, 2, "Khó", 12, 8, 3, 120, 0, 40, new List<int>()),
                [(2, 0)] = new PictureDifficultyConfig(2, 0, "Dễ", 6, 4, 1, 30, 0, 10, new List<int>()),
                [(2, 1)] = new PictureDifficultyConfig(2, 1, "Trung bình", 8, 6, 2, 60, 0, 20, new List<int>()),
                [(2, 2)] = new PictureDifficultyConfig(2, 2, "Khó", 12, 8, 3, 120, 0, 40, new List<int>())
            };

            _items = new List<ItemDto>();
            _itemsById = new Dictionary<int, ItemDto>();
        }

        public void LoadFromText(string jsonText)
        {
            var dto = JsonUtility.FromJson<StaticDataDto>(jsonText);
            if (dto == null) return;

            // Defensive null initialization for missing JSON fields
            if (dto.pictures == null) dto.pictures = new List<PictureDto>();
            if (dto.items == null) dto.items = new List<ItemDto>();
            if (dto.picture_difficulties == null) dto.picture_difficulties = new List<PictureDifficultyDto>();

            ValidateStaticData(dto);

            _items = dto.items;
            _itemsById = _items.ToDictionary(i => i.id);

            _pictures = dto.pictures.Select(p => new PictureConfig(
                p.id, 
                p.id_string, 
                p.display_name, 
                p.asset_path
            )).ToList();

            _difficulties = new Dictionary<(int, int), PictureDifficultyConfig>();
            foreach (var diff in dto.picture_difficulties)
            {
                var key = (diff.picture_id, diff.difficulty_id);
                var config = new PictureDifficultyConfig(
                    diff.picture_id,
                    diff.difficulty_id,
                    diff.display_name,
                    diff.grid_columns,
                    diff.grid_rows,
                    diff.star_reward,
                    diff.first_clear_coin,
                    diff.first_clear_hint,
                    diff.replay_coin,
                    diff.first_clear_reward_item_ids
                );
                _difficulties[key] = config;
            }
        }

        private void ValidateStaticData(StaticDataDto dto)
        {
            var picIds = new HashSet<int>();
            var picIdStrings = new HashSet<string>();
            if (dto.pictures != null)
            {
                foreach (var p in dto.pictures)
                {
                    if (string.IsNullOrEmpty(p.id_string))
                        throw new InvalidOperationException($"Picture ID {p.id} has empty or null id_string.");
                    if (!picIds.Add(p.id))
                        throw new InvalidOperationException($"Duplicate Picture ID found: {p.id}");
                    if (!picIdStrings.Add(p.id_string))
                        throw new InvalidOperationException($"Duplicate Picture ID String found: {p.id_string}");
                }
            }

            var itemIds = new HashSet<int>();
            var itemIdStrings = new HashSet<string>();
            if (dto.items != null)
            {
                foreach (var item in dto.items)
                {
                    if (string.IsNullOrEmpty(item.id_string))
                        throw new InvalidOperationException($"Item ID {item.id} has empty or null id_string.");
                    if (!itemIds.Add(item.id))
                        throw new InvalidOperationException($"Duplicate Item ID found: {item.id}");
                    if (!itemIdStrings.Add(item.id_string))
                        throw new InvalidOperationException($"Duplicate Item ID String found: {item.id_string}");
                }
            }

            if (dto.picture_difficulties != null)
            {
                foreach (var diff in dto.picture_difficulties)
                {
                    if (!picIds.Contains(diff.picture_id))
                        throw new InvalidOperationException($"Difficulty references missing picture: {diff.picture_id}");

                    if (diff.grid_columns * diff.grid_rows != diff.piece_count)
                        throw new InvalidOperationException($"Difficulty Grid size does not match piece count for picture {diff.picture_id}");

                    if (diff.first_clear_reward_item_ids != null)
                    {
                        foreach (var rewardId in diff.first_clear_reward_item_ids)
                        {
                            if (!itemIds.Contains(rewardId))
                                throw new InvalidOperationException($"Difficulty rewards missing item ID: {rewardId}");
                        }
                    }
                }
            }
        }

        public IReadOnlyList<PictureConfig> GetAllPictures() => _pictures;

        public PictureConfig GetPictureById(int id)
        {
            return _pictures.FirstOrDefault(p => p.Id == id);
        }

        public PictureDifficultyConfig GetPictureDifficulty(int pictureId, int difficultyId)
        {
            if (_difficulties.TryGetValue((pictureId, difficultyId), out var config))
            {
                return config;
            }
            throw new KeyNotFoundException($"Difficulty {difficultyId} not found for picture {pictureId}");
        }

        public ItemDto GetItemById(int id)
        {
            if (_itemsById.TryGetValue(id, out var item))
            {
                return item;
            }
            return null;
        }

        public IReadOnlyList<ItemDto> GetAllItems() => _items;
    }
}
```

**Step 3: Update `RewardSummaryPresenter.cs`**
Implement progression rewards reading config, handling owned key items, and updating `session.LastCoinEarned`:
```csharp
        public void ProcessRewards(float elapsedTimeSeconds)
        {
            var config = _staticDataService.GetPictureDifficulty(_sessionService.SelectedPictureId, _sessionService.SelectedDifficultyId);
            int stars = config.StarReward;

            _sessionService.LastStarCount = stars;
            _sessionService.LastElapsedTimeSeconds = elapsedTimeSeconds;

            if (!_sessionService.IsRewardProcessed)
            {
                var save = _saveDataService.Load();

                var existing = save.CompletedPuzzles.Find(p =>
                    p.PictureId == _sessionService.SelectedPictureId &&
                    p.DifficultyId == _sessionService.SelectedDifficultyId);

                int coins = 0;
                if (existing != null)
                {
                    // Replay reward
                    coins = config.ReplayCoin > 0 ? config.ReplayCoin : (stars * 10);
                    save.Coins += coins;

                    // Update with best records
                    if (_sessionService.LastElapsedTimeSeconds < existing.BestTimeSeconds || existing.BestTimeSeconds <= 0)
                    {
                        existing.BestTimeSeconds = _sessionService.LastElapsedTimeSeconds;
                    }
                    if (stars > existing.BestStar)
                    {
                        existing.BestStar = stars;
                    }
                }
                else
                {
                    // First Clear reward
                    coins = config.FirstClearCoin > 0 ? config.FirstClearCoin : (stars * 10);
                    save.Coins += coins;
                    save.Hints += config.FirstClearHint;

                    if (config.FirstClearRewardItemIds != null)
                    {
                        if (save.OwnedItemIds == null)
                        {
                            save.OwnedItemIds = new List<int>();
                        }
                        foreach (var itemId in config.FirstClearRewardItemIds)
                        {
                            if (!save.OwnedItemIds.Contains(itemId))
                            {
                                save.OwnedItemIds.Add(itemId);
                            }
                        }
                    }

                    save.CompletedPuzzles.Add(new CompletedPuzzleData
                    {
                        PictureId = _sessionService.SelectedPictureId,
                        DifficultyId = _sessionService.SelectedDifficultyId,
                        BestTimeSeconds = _sessionService.LastElapsedTimeSeconds,
                        BestStar = stars
                    });
                }

                _sessionService.LastCoinEarned = coins;
                _saveDataService.Save(save);
                _sessionService.IsRewardProcessed = true;
            }
        }

        public void DisplayProcessedReward()
        {
            if (_view != null)
            {
                _view.DisplayReward(_sessionService.LastStarCount, _sessionService.LastCoinEarned);
            }
        }
```

**Step 4: Run tests to verify they pass**
Verify that all unit tests (`StaticDataServiceTests` and `ProgressionTests`) pass successfully.

---

### Task 4: Implement Game Data Editor Window

**Files:**
* Create: `JigsawVina/Assets/JigsawVina/Scripts/Editor/JigsawVinaGameDataEditor.cs`

**Step 1: Write `JigsawVinaGameDataEditor.cs`**
Create the Editor GUI, pre-fill parsing, alphabetical folder scan, stable ID assignment, uniqueness dialog alerts, and JSON serialization. Ensure inputs for `first_clear_hint` and `replay_coin` are properly populated and serialized to JSON:
```csharp
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using JigsawVina.Core.Data;
using UnityEditor;
using UnityEngine;

namespace JigsawVina.Editor
{
    public class JigsawVinaGameDataEditor : EditorWindow
    {
        private const string SavePath = "Assets/Resources/StaticData.json";
        private int _selectedTab = 0;
        private string[] _tabNames = { "Tranh 1", "Tranh 2", "Tranh 3", "Tranh 4", "Tranh 5" };

        [Serializable]
        private class EditorTabState
        {
            public DefaultAsset folderAsset;
            public int pictureId;
            public string idString = "";
            public string displayName = "";
            
            // Difficulty settings
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

        [SerializeField] private List<EditorTabState> _tabs = new();

        [MenuItem("JigsawVina/Game Data Editor")]
        public static void ShowWindow()
        {
            GetWindow<JigsawVinaGameDataEditor>("Game Data Editor");
        }

        private void OnEnable()
        {
            LoadFromDisk();
        }

        private void LoadFromDisk()
        {
            _tabs.Clear();
            if (File.Exists(SavePath))
            {
                try
                {
                    string json = File.ReadAllText(SavePath);
                    var dto = JsonUtility.FromJson<StaticDataDto>(json);
                    if (dto != null && dto.pictures != null)
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

                        for (int i = 0; i < Mathf.Min(5, dto.pictures.Count); i++)
                        {
                            var pic = dto.pictures[i];
                            var state = new EditorTabState();
                            state.pictureId = pic.id;
                            state.idString = pic.id_string;
                            state.displayName = pic.display_name;

                            // Reconstruct folder asset path
                            if (!string.IsNullOrEmpty(pic.asset_path))
                            {
                                string relativeDir = Path.GetDirectoryName(pic.asset_path).Replace("\\", "/");
                                string folderPath = $"Assets/Resources/{relativeDir}";
                                state.folderAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(folderPath);
                            }

                            // Scan to match reward indices mathematically via stable ID formula
                            if (state.folderAsset != null)
                            {
                                var (_, scannedItems) = ScanFolder(state.folderAsset);
                                if (diffsByPic.TryGetValue(pic.id, out var picDiffs))
                                {
                                    foreach (var d in picDiffs)
                                    {
                                        int rewardIdx = 0;
                                        if (d.first_clear_reward_item_ids != null && d.first_clear_reward_item_ids.Count > 0)
                                        {
                                            int rewardId = d.first_clear_reward_item_ids[0];
                                            int calculatedIndex = (rewardId - pic.id * 100) - 1;
                                            if (calculatedIndex >= 0 && calculatedIndex < scannedItems.Count)
                                            {
                                                rewardIdx = calculatedIndex + 1;
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
                            }
                            _tabs.Add(state);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[JigsawVina Editor] Could not parse existing StaticData.json: {e.Message}");
                }
            }

            while (_tabs.Count < 5)
            {
                _tabs.Add(new EditorTabState { pictureId = _tabs.Count + 1 });
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames);
            EditorGUILayout.Space();

            if (_selectedTab >= 0 && _selectedTab < _tabs.Count)
            {
                DrawTab(_tabs[_selectedTab]);
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Save & Generate JSON", GUILayout.Height(40)))
            {
                SaveConfig();
            }
        }

        private void DrawTab(EditorTabState state)
        {
            GUILayout.BeginHorizontal();

            // LEFT PANEL: Assets & Metadata
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(position.width * 0.45f));
            GUILayout.Label("LEFT: Assets & Metadata", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            var prevAsset = state.folderAsset;
            state.folderAsset = (DefaultAsset)EditorGUILayout.ObjectField("Thư mục tranh", state.folderAsset, typeof(DefaultAsset), false);

            if (state.folderAsset != prevAsset && state.folderAsset != null)
            {
                AutoFillFromFolder(state);
            }

            if (state.folderAsset == null)
            {
                EditorGUILayout.HelpBox("Hãy kéo thả thư mục chứa tranh vào đây.", MessageType.Info);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                return;
            }

            var (mainTexture, itemTextures) = ScanFolder(state.folderAsset);

            // Picture Configuration
            GUILayout.Label("Tranh Chính", EditorStyles.boldLabel);
            if (mainTexture != null)
            {
                var rect = GUILayoutUtility.GetRect(120, 90, GUILayout.ExpandWidth(false));
                GUI.DrawTexture(rect, mainTexture, ScaleMode.ScaleToFit);
            }
            state.pictureId = EditorGUILayout.IntField("Picture ID", state.pictureId);
            state.idString = EditorGUILayout.TextField("ID String", state.idString);
            state.displayName = EditorGUILayout.TextField("Tên Tranh", state.displayName);

            EditorGUILayout.Space();
            GUILayout.Label("Danh Sách Key Items", EditorStyles.boldLabel);

            if (itemTextures.Count == 0)
            {
                EditorGUILayout.HelpBox("Không tìm thấy key item nào.", MessageType.None);
            }

            for (int i = 0; i < itemTextures.Count; i++)
            {
                var tex = itemTextures[i];
                GUILayout.BeginHorizontal(GUI.skin.box);
                var r = GUILayoutUtility.GetRect(40, 40, GUILayout.ExpandWidth(false));
                GUI.DrawTexture(r, tex, ScaleMode.ScaleToFit);
                
                GUILayout.BeginVertical();
                GUILayout.Label($"File: {tex.name}");
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();

            // RIGHT PANEL: Difficulties & Rewards
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true));
            GUILayout.Label("RIGHT: Difficulties & Rewards", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            string[] itemNames = new string[itemTextures.Count + 1];
            itemNames[0] = "None";
            for (int i = 0; i < itemTextures.Count; i++)
            {
                itemNames[i + 1] = itemTextures[i].name;
            }

            // EASY
            GUILayout.Label("DỄ (Easy)", EditorStyles.boldLabel);
            state.easyCols = EditorGUILayout.IntField("Columns", state.easyCols);
            state.easyRows = EditorGUILayout.IntField("Rows", state.easyRows);
            state.easyCoins = EditorGUILayout.IntField("First Clear Coin", state.easyCoins);
            state.easyReplayCoins = EditorGUILayout.IntField("Replay Coin", state.easyReplayCoins);
            state.easyHints = EditorGUILayout.IntField("First Clear Hint", state.easyHints);
            state.easyKeyRewardIndex = EditorGUILayout.Popup("Reward Key Item", state.easyKeyRewardIndex, itemNames);

            EditorGUILayout.Space();

            // NORMAL
            GUILayout.Label("TRUNG BÌNH (Normal)", EditorStyles.boldLabel);
            state.normalCols = EditorGUILayout.IntField("Columns", state.normalCols);
            state.normalRows = EditorGUILayout.IntField("Rows", state.normalRows);
            state.normalCoins = EditorGUILayout.IntField("First Clear Coin", state.normalCoins);
            state.normalReplayCoins = EditorGUILayout.IntField("Replay Coin", state.normalReplayCoins);
            state.normalHints = EditorGUILayout.IntField("First Clear Hint", state.normalHints);
            state.normalKeyRewardIndex = EditorGUILayout.Popup("Reward Key Item", state.normalKeyRewardIndex, itemNames);

            EditorGUILayout.Space();

            // HARD
            GUILayout.Label("KHÓ (Hard)", EditorStyles.boldLabel);
            state.hardCols = EditorGUILayout.IntField("Columns", state.hardCols);
            state.hardRows = EditorGUILayout.IntField("Rows", state.hardRows);
            state.hardCoins = EditorGUILayout.IntField("First Clear Coin", state.hardCoins);
            state.hardReplayCoins = EditorGUILayout.IntField("Replay Coin", state.hardReplayCoins);
            state.hardHints = EditorGUILayout.IntField("First Clear Hint", state.hardHints);
            state.hardKeyRewardIndex = EditorGUILayout.Popup("Reward Key Item", state.hardKeyRewardIndex, itemNames);

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        private void AutoFillFromFolder(EditorTabState state)
        {
            var (main, _) = ScanFolder(state.folderAsset);
            if (main != null)
            {
                state.idString = main.name.Replace("MAIN_", "").ToLower();
                state.displayName = main.name.Replace("MAIN_", "").Replace("_", " ");
            }
        }

        private (Texture2D main, List<Texture2D> items) ScanFolder(DefaultAsset folder)
        {
            var itemTexs = new List<Texture2D>();
            if (folder == null)
            {
                return (null, itemTexs);
            }

            string path = AssetDatabase.GetAssetPath(folder);
            Texture2D mainTex = null;

            // Get absolute folder path for robust IO scanning
            string fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", path));
            if (!Directory.Exists(fullPath))
            {
                return (null, itemTexs);
            }

            var filePaths = Directory.GetFiles(fullPath, "*.png");
            foreach (var filePath in filePaths)
            {
                var fileName = Path.GetFileName(filePath);
                var assetRelativePath = $"{path}/{fileName}";
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetRelativePath);
                if (tex == null) continue;

                if (tex.name.StartsWith("MAIN_"))
                {
                    mainTex = tex;
                }
                else
                {
                    itemTexs.Add(tex);
                }
            }

            // Alphabetically sort the scanned items to ensure deterministic ID assignment
            itemTexs.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
            return (mainTex, itemTexs);
        }

        private void SaveConfig()
        {
            var config = new StaticDataDto();
            config.schema_version = 1;
            config.data_version = 1;

            config.categories.Add(new CategoryDto
            {
                id = 1,
                id_string = "vietnam_landscapes",
                display_name = "Phong Cảnh Việt Nam"
            });

            var validatedPicIds = new HashSet<int>();
            var validatedPicIdStrings = new HashSet<string>();
            var validatedItemIdStrings = new HashSet<string>();

            foreach (var tab in _tabs)
            {
                if (tab.folderAsset == null) continue;

                string folderPath = AssetDatabase.GetAssetPath(tab.folderAsset);
                if (!folderPath.StartsWith("Assets/Resources/"))
                {
                    EditorUtility.DisplayDialog("Lỗi Thư Mục", $"Thư mục '{folderPath}' phải nằm bên trong thư mục 'Assets/Resources/'.", "OK");
                    return;
                }

                var (main, items) = ScanFolder(tab.folderAsset);
                if (main == null)
                {
                    EditorUtility.DisplayDialog("Thiếu Tranh Chính", $"Không tìm thấy ảnh chính có prefix 'MAIN_' trong thư mục: {tab.folderAsset.name}", "OK");
                    return;
                }

                if (!validatedPicIds.Add(tab.pictureId))
                {
                    EditorUtility.DisplayDialog("Trùng ID Tranh", $"ID Tranh '{tab.pictureId}' bị trùng giữa các tab.", "OK");
                    return;
                }

                if (string.IsNullOrEmpty(tab.idString) || !validatedPicIdStrings.Add(tab.idString))
                {
                    EditorUtility.DisplayDialog("Trùng ID String Tranh", $"ID String '{tab.idString}' bị trùng hoặc trống.", "OK");
                    return;
                }

                // Strip Assets/Resources/ safely via Substring to avoid global Replace side effects
                string resourceFolder = folderPath.Substring("Assets/Resources/".Length);
                string mainPath = $"{resourceFolder}/{main.name}";
                
                config.pictures.Add(new PictureDto
                {
                    id = tab.pictureId,
                    id_string = tab.idString,
                    display_name = tab.displayName,
                    category_id = 1,
                    asset_path = mainPath,
                    difficulty_unlock_policy = "sequential"
                });

                var localItems = new Dictionary<string, int>();
                for (int itemIndex = 0; itemIndex < items.Count; itemIndex++)
                {
                    var itTex = items[itemIndex];
                    string itemIdString = itTex.name.ToLower();
                    
                    if (!validatedItemIdStrings.Add(itemIdString))
                    {
                        EditorUtility.DisplayDialog("Trùng ID String Vật Phẩm", $"Tên file vật phẩm '{itTex.name}' bị trùng lặp trong dự án. Vui lòng sử dụng tên file duy nhất.", "OK");
                        return;
                    }

                    int itemId = tab.pictureId * 100 + (itemIndex + 1); // Stable, tab-isolated ID formula
                    string itPath = $"{resourceFolder}/{itTex.name}";
                    config.items.Add(new ItemDto
                    {
                        id = itemId,
                        id_string = itemIdString,
                        display_name = itTex.name.Replace("_", " "),
                        item_type = "key_item",
                        asset_path = itPath
                    });
                    localItems[itTex.name] = itemId;
                }

                AddDifficulty(config, tab.pictureId, 0, "Dễ", tab.easyCols, tab.easyRows, tab.easyCoins, tab.easyReplayCoins, tab.easyHints, tab.easyKeyRewardIndex, items, localItems);
                AddDifficulty(config, tab.pictureId, 1, "Trung bình", tab.normalCols, tab.normalRows, tab.normalCoins, tab.normalReplayCoins, tab.normalHints, tab.normalKeyRewardIndex, items, localItems);
                AddDifficulty(config, tab.pictureId, 2, "Khó", tab.hardCols, tab.hardRows, tab.hardCoins, tab.hardReplayCoins, tab.hardHints, tab.hardKeyRewardIndex, items, localItems);
            }

            // Sort DTOs for deterministic, clean JSON output and clean git diffs
            config.pictures.Sort((a, b) => a.id.CompareTo(b.id));
            config.items.Sort((a, b) => a.id.CompareTo(b.id));
            config.picture_difficulties.Sort((a, b) =>
            {
                int comp = a.picture_id.CompareTo(b.picture_id);
                if (comp != 0) return comp;
                return a.difficulty_id.CompareTo(b.difficulty_id);
            });

            string json = JsonUtility.ToJson(config, true);
            Directory.CreateDirectory(Path.GetDirectoryName(SavePath));
            File.WriteAllText(SavePath, json);
            AssetDatabase.Refresh();
            Debug.Log($"[JigsawVina Editor] Static data written successfully to {SavePath}.");
            EditorUtility.DisplayDialog("Hoàn Thành", $"Đã lưu và cấu hình static data tại {SavePath}!", "OK");
        }

        private void AddDifficulty(StaticDataDto config, int pictureId, int diffId, string displayName, int cols, int rows, int firstClearCoins, int replayCoins, int firstClearHints, int rewardIndex, List<Texture2D> items, Dictionary<string, int> localItems)
        {
            var listRewards = new List<int>();
            if (rewardIndex > 0 && rewardIndex <= items.Count)
            {
                var texName = items[rewardIndex - 1].name;
                if (localItems.TryGetValue(texName, out int itemId))
                {
                    listRewards.Add(itemId);
                }
            }

            config.picture_difficulties.Add(new PictureDifficultyDto
            {
                picture_id = pictureId,
                difficulty_id = diffId,
                display_name = displayName,
                grid_columns = cols,
                grid_rows = rows,
                piece_count = cols * rows,
                star_reward = diffId + 1, // Easy = 1, Normal = 2, Hard = 3
                first_clear_coin = firstClearCoins,
                first_clear_hint = firstClearHints,
                replay_coin = replayCoins,
                first_clear_reward_item_ids = listRewards
            });
        }
    }
}
#endif
```

**Step 2: Verify compilation**
Compile and open the window via Unity `JigsawVina/Game Data Editor`.

---

### Task 5: Integrate Presentation Asset Loading

**Files:**
* Modify: `JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/PuzzlePlayingPresenter.cs:50-68`

**Step 1: Update texture path lookup in `PuzzlePlayingPresenter.cs`**
Adjust resource path loader to load `picture.AssetPath` directly, and provide safe fallbacks:
```csharp
            _texture = Resources.Load<Texture2D>(picture.AssetPath);
            if (_texture == null)
            {
                // Fallback to absolute/legacy if resources fails
                _texture = Resources.Load<Texture2D>("Textures/" + picture.AssetPath);
            }
            if (_texture == null)
            {
                _texture = new Texture2D(400, 300);
            }
```

**Step 2: Run test suite to verify no regressions**
Run all tests and ensure compiling/running passes without warnings.

---

## Verification Plan

### Automated Tests
- Open Unity Editor -> Test Runner window.
- Select EditMode -> Run `StaticDataServiceTests` and `ProgressionTests`. Verify both pass successfully.

### Manual Verification
1. Move folders `Picture_1` to `Picture_5` inside `Assets/Resources/Textures/` (or any subfolder inside `Assets/Resources/`).
2. Open window from Unity top menu: `JigsawVina/Game Data Editor`.
3. In Tab "Tranh 1", assign folder `Assets/Resources/Textures/Picture_1`. Confirm that `MAIN_House_OldVillage_1` is recognized as the main picture and others are recognized as Key Items.
4. Set up names, select Key Item rewards, first-clear coins, replay coins, and first-clear hints for each difficulty.
5. Repeat for tabs "Tranh 2" to "Tranh 5".
6. Click **Save & Generate JSON**.
7. Confirm that `Assets/Resources/StaticData.json` is generated correctly and contains all fields.
8. Enter Play Mode. Confirm that pictures, item rewards, first-clear coins/hints, and replay coins load and apply dynamically from JSON config instead of hardcoded paths.
