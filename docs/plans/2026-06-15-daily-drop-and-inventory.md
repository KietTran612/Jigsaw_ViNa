# Daily Drop Decay & Inventory/Collection UI Implementation Plan

> **For Antigravity:** REQUIRED WORKFLOW: Use `.agent/workflows/execute-plan.md` to execute this plan in single-flow mode.

**Goal:** Implement the Daily Drop Decay rate limits, daily reset, mockable drop reward service, and the Collection/Inventory UI showing owned Key Items with source locations and quick navigation.

**Architecture:** Extend static DTOs and JigsawVinaGameDataEditor to handle Drop Tables and preserve JSON data. Migrate PlayerSave with daily reset timers using a decoupled date provider. Decouple drop/reward RNG logic into a testable `IDropRewardService` that uses `IRandomSource` for mockability. Build a Collection view on the Home screen using MVP presentation and dynamic UI binding.

**Tech Stack:** Unity 6000.3.11f1, uGUI, VContainer DI, UniTask, NUnit (EditMode/PlayMode tests).

---

## User Review Required

> [!IMPORTANT]
> **Overload Safety and Test Compatibility**:
> - Keep the parameterless `Normalize()` method signature in `PlayerSave` intact to avoid compile-time errors at existing call sites. Enforce that the parameterless `Normalize()` initializes both `DailyDropCounts` and `Inventory` to prevent `NullReferenceException` when calling the overload.
> - Introduce `Normalize(string localDateString)` as a separate overload/method which internally calls the parameterless `Normalize()` first.
> - Keep the old constructor overload of `PictureDifficultyConfig` to preserve existing mock calls in unit tests.
> - Maintain a backwards-compatible constructor overload in `RewardSummaryPresenter` that delegates to the new constructor using a no-op/default `IDropRewardService`. Do not leave the dependency as an unchecked `null`, because existing direct constructor calls can load runtime configurations with `DropTableId > 0`.
>
> **Drop Table Item Restrictions & Roll Exclusion**:
> - Support general consumable items (e.g., rate items like stamps and map fragments) by introducing an inventory list `InventoryItem` in `PlayerSave.cs`. The validator will allow coin (ID 1), hint (ID 2), Key Items (`item_type == "key_item"`), and Consumable Items (`item_type == "consumable"`).
> - **Canonical Consumables ID Configuration**: Global consumable items will use IDs below 100 to avoid conflicting with picture-specific Key Item ranges (IDs 201/202 are already picture 2 key items). We define:
>   - ID 10: `"postcard_stamp"` ("Tem Bưu Thiếp"), type `"consumable"`.
>   - ID 11: `"harvest_token"` ("Xu Mùa Gặt"), type `"consumable"`.
> - If the drop item is a Key Item, `amount_min` and `amount_max` must be exactly 1.
> - **Exclusion of Owned Key Items**: If a Key Item is already owned (`save.OwnedItemIds.Contains(itemId)`), `DropRewardService` must exclude/filter it out from the rolling pool *before* performing any RNG checks or updating decay counters.
> - **Exclusion of Full Consumables**: If a consumable already has `Amount >= max_stack`, exclude it before RNG and counter updates. If only part of a rolled amount fits, grant and display the amount actually added while counting the roll as one success.
> - **Counter Incrementation**: When a roll succeeds, `DropRewardService` must update the counter in `save.DailyDropCounts` (either find and increment the existing entry or insert a new one with count = 1).
> - **Status Consistency**: The static data validator will enforce that a difficulty configuration cannot point to a drop table that is not `"active"`. In `DropRewardService`, only active drop table items will be rolled.
> - **Consumable Stack Limit**: Applying a consumable reward must clamp its stored amount to the item's configured `max_stack`. A reward that cannot add any amount because the stack is full must not be reported as received.
> - **Item State Consistency**: Every referenced drop item must have `status == "active"`. Key Items must be non-consumable with `max_stack == 1`; consumable items must have `is_consumable == true` and `max_stack > 0`.
>
> **Quick Navigation Contract**:
> - Clicking a source in the Collection UI will navigate back to the Picture Select View and close the Collection UI.
> - The controller will check the picture's unlock state via `ProgressionService.GetPictureState(pictureId)`:
>   - If **unlocked** or **completed**: Simulate selection by calling a new public method `_pictureSelectView.RequestPictureSelection(pictureId)` (which internally invokes `OnPictureSelected` to trigger proper selected picture ID session updates and open difficulties screen).
>   - If **locked** or **ready to unlock**: Return to the `PictureSelectView` and center/highlight the card using a new API `_pictureSelectView.FocusCard(pictureId)` *without* opening the Difficulty Select View.

---

## Proposed Changes

### Static Data & Game Data Editor

#### [MODIFY] [StaticDataDto.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Data/StaticDataDto.cs)
- Add DTO classes `DropTableDto` and `DropTableItemDto`.
- Add list fields for `drop_tables` and `drop_table_items` in `StaticDataDto`.
- Add `drop_table_id` field in `PictureDifficultyDto`.

#### [NEW] [DropTableConfig.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Data/DropTableConfig.cs)
- Create immutable runtime model for drop tables:
```csharp
namespace JigsawVina.Core.Data
{
    public readonly struct DropTableConfig
    {
        public readonly int Id;
        public readonly string IdString;
        public readonly string DisplayName;
        public readonly string DisplayNameKey;
        public readonly string DescriptionKey;
        public readonly string ResetRule;
        public readonly string Status;
        public readonly int SortOrder;

        public DropTableConfig(int id, string idString, string displayName, string displayNameKey, string descriptionKey, string resetRule, string status, int sortOrder)
        {
            Id = id;
            IdString = idString;
            DisplayName = displayName;
            DisplayNameKey = displayNameKey;
            DescriptionKey = descriptionKey;
            ResetRule = resetRule;
            Status = status;
            SortOrder = sortOrder;
        }
    }
}
```

#### [NEW] [DropTableItemConfig.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Data/DropTableItemConfig.cs)
- Create immutable runtime model for drop table items:
```csharp
namespace JigsawVina.Core.Data
{
    public readonly struct DropTableItemConfig
    {
        public readonly int Id;
        public readonly string IdString;
        public readonly string DisplayName;
        public readonly int DropTableId;
        public readonly int ItemId;
        public readonly float BaseRate;
        public readonly float DecayPerSuccess;
        public readonly float MinRate;
        public readonly int AmountMin;
        public readonly int AmountMax;
        public readonly string Status;

        public DropTableItemConfig(int id, string idString, string displayName, int dropTableId, int itemId, float baseRate, float decayPerSuccess, float minRate, int amountMin, int amountMax, string status)
        {
            Id = id;
            IdString = idString;
            DisplayName = displayName;
            DropTableId = dropTableId;
            ItemId = itemId;
            BaseRate = baseRate;
            DecayPerSuccess = decayPerSuccess;
            MinRate = minRate;
            AmountMin = amountMin;
            AmountMax = amountMax;
            Status = status;
        }
    }
}
```

#### [MODIFY] [PictureDifficultyConfig.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Data/PictureDifficultyConfig.cs)
- Add `public readonly int DropTableId;` to the config struct.
- Keep the old constructor overload:
```csharp
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
            IReadOnlyList<int> firstClearRewardItemIds) : this(
                pictureId,
                difficultyId,
                displayName,
                columns,
                rows,
                starReward,
                firstClearCoin,
                firstClearHint,
                replayCoin,
                firstClearRewardItemIds,
                0)
        {
        }
```
- Define the new master constructor accepting `DropTableId`.

#### [MODIFY] [IStaticDataService.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/IStaticDataService.cs)
- Add contract methods:
```csharp
        IReadOnlyList<DropTableConfig> GetAllDropTables();
        IReadOnlyList<DropTableItemConfig> GetDropTableItems(int dropTableId);
        IReadOnlyList<DropTableItemConfig> GetAllDropTableItems();
```

#### [MODIFY] [StaticDataService.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/StaticDataService.cs)
- Load `drop_tables` and `drop_table_items` lists and map them.
- Map `drop_table_id` from difficulty DTO into `PictureDifficultyConfig`.
- Implement new contract methods from `IStaticDataService`.
- Add static data validations in `ValidateStaticData`:
  - Check drop table `id` and `id_string` are unique.
  - Check drop table item `id` and `id_string` are unique.
  - Check `0 <= min_rate <= base_rate <= 1`.
  - Check `decay_per_success >= 0`.
  - Check `amount_min > 0`, `amount_max >= amount_min`, and `amount_max < int.MaxValue` so the inclusive `max + 1` RNG call cannot overflow.
  - Check `status` is valid.
  - Check `reset_rule` is valid.
  - Check that for a single `drop_table_id`, there are no duplicate `item_id` values.
  - Check references: `drop_table_id` in difficulties and in table items must reference valid drop tables. `item_id` in table items must reference valid items.
  - **Status Consistency Check**: Ensure difficulties cannot point to a drop table that is not active (`status != "active"`).
  - **Item Restrictions**: Enforce that drop items reference active coin (ID 1), hint (ID 2), Key Items (`item_type == "key_item"`), or Consumable Items (`item_type == "consumable"`).
  - Key Items must have `is_consumable == false`, `max_stack == 1`, and drop amounts exactly 1.
  - Consumable Items must have `is_consumable == true` and `max_stack > 0`.

#### [MODIFY] [JigsawVinaGameDataEditor.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Editor/JigsawVinaGameDataEditor.cs)
- Keep fields for `drop_tables` and `drop_table_items` in memory.
- In `LoadStateFromDto(StaticDataDto dto)`, load the raw lists.
- In `TryBuildConfig(out StaticDataDto config, ...)`, populate `config.drop_tables` and `config.drop_table_items` with the loaded lists.
- On each difficulty configuration GUI foldout, display a field for `drop_table_id`.

---

### PlayerSave & Migration

#### [MODIFY] [PlayerSave.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Data/PlayerSave.cs)
- Add `public string LastSaveDateString;` to store the date of last save (format: `"yyyy-MM-dd"`).
- Add `[Serializable]` helper class `DailyDropCount` containing `public int ItemId;` and `public int Count;`.
- Add `[Serializable]` helper class `InventoryItem` containing `public int ItemId;` and `public int Amount;`.
- Add `public List<DailyDropCount> DailyDropCounts = new();`.
- Add `public List<InventoryItem> Inventory = new();`.
- Keep parameterless `Normalize()` intact, and initialize collections to prevent NullReferenceException:
```csharp
        public void Normalize()
        {
            if (CompletedPuzzles == null) CompletedPuzzles = new();
            if (OwnedItemIds == null) OwnedItemIds = new();
            if (UnlockedPictureIds == null) UnlockedPictureIds = new();
            if (DailyDropCounts == null) DailyDropCounts = new();
            if (Inventory == null) Inventory = new();
        }

        public void Normalize(string localDateString)
        {
            Normalize();
            if (LastSaveDateString != localDateString)
            {
                DailyDropCounts.Clear();
                LastSaveDateString = localDateString;
            }
        }
```

#### [NEW] [ILocalDateProvider.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/ILocalDateProvider.cs)
- Define LocalDate provider contract:
```csharp
namespace JigsawVina.Core.Services
{
    public interface ILocalDateProvider
    {
        string GetCurrentLocalDateString();
    }
}
```

#### [NEW] [LocalDateProvider.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/LocalDateProvider.cs)
- Implement `ILocalDateProvider` returning `DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)`.

#### [MODIFY] [SaveDataService.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/SaveDataService.cs)
- Inject `ILocalDateProvider` into constructor.
- Inside `Load()`, call `save.Normalize(_localDateProvider.GetCurrentLocalDateString())` to ensure dates are normalized.

---

### Drop Reward Service & RNG Contracts

#### [NEW] [IRandomSource.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/IRandomSource.cs)
- Interface declaring:
```csharp
namespace JigsawVina.Core.Services
{
    public interface IRandomSource
    {
        float NextFloat();
        int NextRange(int minInclusive, int maxExclusive);
    }
}
```

#### [NEW] [UnityRandomSource.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/UnityRandomSource.cs)
- Standard runtime random source wrapping `UnityEngine.Random`.

#### [NEW] [IDropRewardService.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/IDropRewardService.cs)
- Interface declaring:
```csharp
using System.Collections.Generic;
using JigsawVina.Core.Data;

namespace JigsawVina.Core.Services
{
    public struct DropRewardResult
    {
        public int ItemId;
        public int Amount;
    }

    public interface IDropRewardService
    {
        List<DropRewardResult> RollDropRewards(int dropTableId, PlayerSave save);
    }
}
```

#### [NEW] [DropRewardService.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/DropRewardService.cs)
- Constructor-inject `IStaticDataService` and `IRandomSource`.
- Skeleton implementation of `IDropRewardService` containing stub method throwing `System.NotImplementedException`.
- Each active drop-table item is rolled independently; this is not a weighted single-selection pool.

#### [MODIFY] [RewardSummaryPresenter.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/RewardSummaryPresenter.cs)
- Inject `IDropRewardService` into constructor.
- Maintain a backwards-compatible constructor overload using a no-op implementation:
```csharp
        public RewardSummaryPresenter(
            RewardSummaryView view,
            GameSessionService sessionService,
            ISaveDataService saveDataService,
            IStaticDataService staticDataService) : this(
                view,
                sessionService,
                saveDataService,
                staticDataService,
                NoOpDropRewardService.Instance)
        {
        }
```
- Implement `NoOpDropRewardService` as a private nested singleton inside `RewardSummaryPresenter` so the compatibility overload does not introduce another Unity-tracked file.
- During replay reward processing, if difficulty `DropTableId > 0`, call `RollDropRewards` and apply drops:
  - Coin (ID 1) -> `save.Coins += drop.Amount;`
  - Hint (ID 2) -> `save.Hints += drop.Amount;`
  - Key item -> add to `save.OwnedItemIds` (prevent duplicates).
  - Consumable item -> find matching `InventoryItem` in `save.Inventory` and increment its `Amount` (or add new entry if missing), clamped to the item's `max_stack`.
- Include applied drop rewards in the reward summary:
  - Add dropped coins to `_sessionService.LastCoinEarned` so the displayed coin total includes replay coins plus coin drops.
  - Add hint, consumable, and newly-owned Key Item names/amounts to the received-items label.
  - Do not display duplicate Key Items or zero-added consumables.

#### [MODIFY] [RewardSummaryView.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/RewardSummaryView.cs)
- Keep the current `DisplayReward` signature compatible.
- Treat its third argument as a general received-items label rather than a Key Item-only label.

#### [MODIFY] [ProjectLifetimeScope.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/App/ProjectLifetimeScope.cs)
- Register `LocalDateProvider` as `ILocalDateProvider`.
- Register `UnityRandomSource` as `IRandomSource`.
- Register `DropRewardService` as `IDropRewardService`.
- Pass `ILocalDateProvider` parameter to `SaveDataService` resolve/lambda.

---

### Collection / Inventory UI & Quick Navigation

#### [NEW] [CollectionView.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/CollectionView.cs)
- Panel view component showing:
  - Scroll view content container.
  - Key item slots.
  - Item details display (name, description, thumbnail, list of drop sources).
  - Navigation button next to each drop source to select/jump to that picture.
  - Close button.
- Use hidden scene templates for item-slot and source-row buttons, clone them under their serialized content containers, and remove all generated listeners/items before rebuilding.

#### [NEW] [CollectionPresenter.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/CollectionPresenter.cs)
- Presenter logic mapping owned items in `PlayerSave` to UI slots.
- Formulate drop sources dynamically: scan all `PictureDifficulty` configurations in `IStaticDataService` to check if they reward this key item (either via first clear rewards or drop tables).
- De-duplicate sources by `(PictureId, DifficultyId)` and sort them deterministically by picture then difficulty.
- Expose an event or delegate for quick navigation: `public event Action<int> OnNavigateToPictureRequested;`.

#### [MODIFY] [PictureSelectCard.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/PictureSelectCard.cs)
- Expose the bound ID with `public int PictureId => _pictureId;`.
- Add `public void Highlight()` to trigger a visual flash or outline effect on the card.

#### [MODIFY] [PictureSelectView.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/PictureSelectView.cs)
- Add a serialized `ScrollRect` reference used by `FocusCard`.
- Add `public event Action OnCollectionRequested;` and invoke it from the serialized Collection button.
- Add `public void RequestPictureSelection(int pictureId)` to invoke `OnPictureSelected` event internally:
```csharp
        public void RequestPictureSelection(int pictureId)
        {
            OnPictureSelected?.Invoke(pictureId);
        }
```
- Add `public void FocusCard(int pictureId)` to find the card through `card.PictureId`, scroll the serialized `ScrollRect` to center the card after layout has rebuilt, and call `card.Highlight()`.

#### [MODIFY] [HomeLifetimeScope.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/HomeLifetimeScope.cs)
- Register `CollectionView` in hierarchy and `CollectionPresenter` as singleton.
- Update `HomeFlowController` to transition to/from the Collection view.
- Subscribe to `PictureSelectView.OnCollectionRequested`, `CollectionView` close requests, and `CollectionPresenter.OnNavigateToPictureRequested` with named handlers.
- In `HomeFlowController.Start()`, subscribe to `OnNavigateToPictureRequested`. In the handler:
  - Close Collection overlay view.
  - Set Picture Select View active.
  - Check the target picture state via `_progressionService.GetPictureState(pictureId)`:
    - If **Unlocked** or **Completed**: Call `_pictureSelectView.RequestPictureSelection(pictureId)` which properly triggers selection session updates in `PictureSelectPresenter` and opens the difficulties select panel.
    - If **Locked** or **ReadyToUnlock**: Call `_pictureSelectView.FocusCard(pictureId)` to center/highlight the locked card.
- In `Dispose()`, unsubscribe all Collection-related events in addition to the existing picture/difficulty listeners.

---

## Detailed Task Breakdown

### Task 41: Editor Integration, Static Data Validation & Config Update

**Files:**
- Modify: [jigsaw_vina_game_data.json](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/Resources/GameData/jigsaw_vina_game_data.json) (configure drop tables, items, and difficulties)
- Modify: [StaticDataDto.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Data/StaticDataDto.cs)
- Modify: [PictureDifficultyConfig.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Data/PictureDifficultyConfig.cs)
- Create: [DropTableConfig.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Data/DropTableConfig.cs)
- Create: [DropTableItemConfig.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Data/DropTableItemConfig.cs)
- Modify: [IStaticDataService.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/IStaticDataService.cs)
- Modify: [StaticDataService.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/StaticDataService.cs)
- Modify: [JigsawVinaGameDataEditor.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Editor/JigsawVinaGameDataEditor.cs)
- Modify: [PictureSelectFlowTests.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Tests/PictureSelectFlowTests.cs) (update static data mocks with new interface methods)
- Modify: [DifficultySelectFlowTests.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Tests/DifficultySelectFlowTests.cs) (update static data mocks with new interface methods)
- Test: [JigsawVinaGameDataEditorTests.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Tests/JigsawVinaGameDataEditorTests.cs)
- Test: [StaticDataServiceTests.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Tests/StaticDataServiceTests.cs)

**Step 1: Write DTO fields & config properties**
- Update `StaticDataDto.cs` to add `DropTableDto`, `DropTableItemDto`, and lists.
- Update `PictureDifficultyDto` to add `drop_table_id`.
- Create `DropTableConfig.cs` and `DropTableItemConfig.cs` models.
- Update `PictureDifficultyConfig.cs` to include `DropTableId` and keep the old constructor.

**Step 2: Implement StaticDataService validations and conversion**
- In `StaticDataService.cs`, load and map drop tables and drop table items.
- Write strict validation rules inside `ValidateStaticData`:
  - Check drop table `id` and `id_string` are unique.
  - Check drop table item `id` and `id_string` are unique.
  - Check `0 <= min_rate <= base_rate <= 1`.
  - Check `decay_per_success >= 0`.
  - Check `amount_min > 0`, `amount_max >= amount_min`, and `amount_max < int.MaxValue`.
  - Check `status` is valid.
  - Check `reset_rule` is valid.
  - Check that for a single `drop_table_id`, there are no duplicate `item_id` values.
  - Check references: `drop_table_id` in difficulties and in table items must reference valid drop tables. `item_id` in table items must reference valid items.
  - **Status Consistency Check**: Ensure difficulties cannot point to a drop table that is not active (`status != "active"`).
  - **Item Restrictions**: Require referenced items to be active. Allow coin (ID 1), hint (ID 2), Key Items (`item_type == "key_item"`), and Consumable Items (`item_type == "consumable"`).
  - Require Key Items to be non-consumable with `max_stack == 1` and amounts exactly 1.
  - Require Consumable Items to have `is_consumable == true` and `max_stack > 0`.

**Step 3: Update Editor and Config File**
- Hydrate and write lists in `JigsawVinaGameDataEditor.cs`.
- Add field `drop_table_id` to difficulties GUI folds.
- Update mock classes in `PictureSelectFlowTests.cs` and `DifficultySelectFlowTests.cs` to implement the new `IStaticDataService` methods.
- Modify `jigsaw_vina_game_data.json` to define:
  - Global consumable items in `items` list:
    - ID 10: `"postcard_stamp"` ("Tem Bưu Thiếp"), type `"consumable"`, `is_consumable: true`, `max_stack: 999`, active.
    - ID 11: `"harvest_token"` ("Xu Mùa Gặt"), type `"consumable"`, `is_consumable: true`, `max_stack: 999`, active. Keep it available for a later rice-field table; do not assign it to the Old Village tables.
  - `drop_tables`: `old_village_easy_drops` (ID 1001), `old_village_normal_drops` (ID 1002), `old_village_hard_drops` (ID 1003).
  - Active `drop_table_items` for postcard stamps (item ID 10), matching the existing sample balance:
    - Entry 11001 / table 1001: base `0.30`, decay `0.10`, minimum `0.20`, amount `1`.
    - Entry 11002 / table 1002: base `0.45`, decay `0.10`, minimum `0.20`, amount `1`.
    - Entry 11003 / table 1003: base `0.60`, decay `0.10`, minimum `0.20`, amount `1`.
  - `drop_table_id` assigned to Picture 1 Easy, Medium, and Hard difficulties.

**Step 4: Verify with tests**
- Run compiler check and verify editor loading / saving preserves data.
- Run StaticDataService validation tests and ensure they pass.

---

### Task 42: PlayerSave Structure, Save Migration & RNG/Service Contracts

**Files:**
- Modify: [PlayerSave.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Data/PlayerSave.cs)
- Create: [ILocalDateProvider.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/ILocalDateProvider.cs)
- Create: [LocalDateProvider.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/LocalDateProvider.cs)
- Modify: [SaveDataService.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/SaveDataService.cs)
- Create: [IRandomSource.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/IRandomSource.cs)
- Create: [UnityRandomSource.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/UnityRandomSource.cs)
- Create: [IDropRewardService.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/IDropRewardService.cs)
- Create: [DropRewardService.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/DropRewardService.cs)
- Test: [SaveDataServiceTests.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Tests/SaveDataServiceTests.cs)

**Step 1: Write PlayerSave properties and daily counter reset logic**
- In `PlayerSave.cs`, add `[Serializable]` `DailyDropCount` and `InventoryItem` classes plus the `DailyDropCounts` and `Inventory` fields.
- Initialize `DailyDropCounts` and `Inventory` lists inside the parameterless `Normalize()` method. Add `Normalize(string localDateString)` overload resetting daily counts.

**Step 2: Create RNG/Service Interfaces and Skeletal implementations**
- Create `IRandomSource.cs`, `UnityRandomSource.cs`, `IDropRewardService.cs`, and skeletal `DropRewardService.cs` with constructor dependencies on `IStaticDataService` and `IRandomSource` (stub throws `NotImplementedException`).

**Step 3: Implement LocalDateProvider and update SaveDataService**
- Inject `ILocalDateProvider` into `SaveDataService`.
- In `SaveDataService.Load()`, call `save.Normalize(_localDateProvider.GetCurrentLocalDateString())`.

**Step 4: Add validation test in SaveDataServiceTests**
- Verify daily reset resets daily counters upon date change.

---

### Task 43: Daily Drop Decay Tests (TDD)

**Files:**
- Create: `JigsawVina/Assets/JigsawVina/Tests/DropRewardTests.cs`

**Step 1: Write EditMode unit tests**
- Test that:
  - `DropRewardService` roll returns the correct items when base probability succeeds.
  - `DropRewardService` roll excludes already owned permanent Key Items from selection *before* doing RNG checks and *before* updating decay counters.
  - `DropRewardService` excludes full-stack consumables before RNG/counter updates and supports partially fitting rewards.
  - `DropRewardService` drop rates decay correctly after multiple rolls.
  - `DropRewardService` drop rate is clamped correctly to `min_rate` after excessive successes.
  - `DropRewardService` rolls inclusive amounts up to `AmountMax` by using `_randomSource.NextRange(min, max + 1)`.
  - Active entries are rolled independently, and an RNG value succeeds only when `roll < currentRate`.
  - Successful rolls increment the global-per-item daily counter exactly once; failed and excluded rolls do not.
  - Verify stubs compile, and that tests fail on skeletal stubs.

---

### Task 44: Daily Drop Reward Service Implementation & Presenter Wiring

**Files:**
- Modify: [DropRewardService.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/DropRewardService.cs)
- Modify: [RewardSummaryPresenter.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/RewardSummaryPresenter.cs)
- Modify: [RewardSummaryView.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/RewardSummaryView.cs)
- Modify: [ProjectLifetimeScope.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/App/ProjectLifetimeScope.cs)
- Test: Add presenter drop reward test cases in `ProgressionTests.cs`

**Step 1: Implement DropRewardService**
- Constructor-inject `IStaticDataService` and `IRandomSource`.
- Roll each active table item independently.
- Exclude owned key items (`save.OwnedItemIds.Contains(itemId)`) from the list before rolling.
- Exclude consumables already at `max_stack` before rolling.
- Evaluate formula `Math.Max(item.MinRate, item.BaseRate - todayCount * item.DecayPerSuccess)`.
- Ignore items in table where `status != "active"`.
- Treat a roll as successful only when `NextFloat() < currentRate`.
- Use `IRandomSource.NextFloat()` and `IRandomSource.NextRange(item.AmountMin, item.AmountMax + 1)` (to make `AmountMax` inclusive).
- **Counter Incrementation**: When a roll succeeds, update `save.DailyDropCounts` by finding the matching `DailyDropCount` (or add new one if missing) and incrementing its `Count`.

**Step 2: Update RewardSummaryPresenter & Write Tests**
- Roll drop rewards for replay runs. Apply Coin, Hint, Key Items, and Consumables appropriately, clamping consumables to `ItemDto.max_stack`.
- Include the amounts actually applied in `RewardSummaryView`; coin display must include replay coins and dropped coins.
- Add constructor overload in `RewardSummaryPresenter` using a no-op drop service to preserve backwards compatibility without nullable runtime behavior.
- Register all new dependencies in `ProjectLifetimeScope.cs`.
- Write tests in `ProgressionTests.cs` asserting that the presenter rolls rewards on replay, clamps consumable stacks, updates `PlayerSave`, and displays only applied rewards correctly.
- Verify NUnit tests pass.

---

### Task 45: Inventory / Collection UI & Navigation

**Files:**
- Create: [CollectionView.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/CollectionView.cs)
- Create: [CollectionPresenter.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/CollectionPresenter.cs)
- Modify: [HomeLifetimeScope.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/HomeLifetimeScope.cs)
- Modify: [PictureSelectCard.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/PictureSelectCard.cs)
- Modify: [PictureSelectView.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/PictureSelectView.cs)
- Modify: [ThinVerticalSliceSceneSetup.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Editor/ThinVerticalSliceSceneSetup.cs)
- Regenerate: [Home.unity](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/Scenes/Home.unity)
- Test: [PictureSelectFlowTests.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Tests/PictureSelectFlowTests.cs)
- Test: [LifetimeScopeRegistrationTests.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Tests/LifetimeScopeRegistrationTests.cs)

**Step 1: Write UI view and presenter**
- Implement `CollectionView.cs` exposing item details, item/source content containers, hidden scene templates, and close/navigate events.
- Clone item-slot and source-row templates when binding; clear generated objects and listeners before every rebuild.
- Implement `CollectionPresenter.cs` resolving owned key items and mapping their drop sources.
- Expose `OnNavigateToPictureRequested` in `CollectionPresenter.cs`.
- Implement `IDisposable` on `CollectionPresenter` and unsubscribe every view event it owns.

**Step 2: Wire buttons and navigation flow**
- Wire a Collection button on `PictureSelectView` to show/hide the collection panel.
- On Collection navigation request, close the Collection view, focus on the selected picture ID:
  - If **Unlocked** or **Completed**: Call `_pictureSelectView.RequestPictureSelection(pictureId)` which properly triggers selection session updates in `PictureSelectPresenter` and opens the difficulties select panel.
  - If **Locked** or **ReadyToUnlock**: Call `_pictureSelectView.FocusCard(pictureId)` to center/highlight the locked card.
- Add focused tests for opening/closing Collection, deterministic/de-duplicated first-clear and drop-table source mapping, unlocked and locked navigation, `FocusCard`, and disposal of presenter/controller event subscriptions.

**Step 3: Update ThinVerticalSliceSceneSetup and regenerate Home scene**
- Change setup check marker to `SetupVersionMarker_v6` in `ThinVerticalSliceSceneSetup.cs` (for `CreateHomeScene()`).
- Rebuild dynamic canvas components with Collection panel.
- **Ensure `CreateGameplayScene()` remains at `SetupVersionMarker_v4` and is NOT regenerated.**
- Run setup to update `Home.unity`.
- Run the targeted Collection/navigation tests and manually verify the generated Home scene.

---

## Verification Plan

### Automated Tests
- After each task, wait for Unity compilation and check Console/Editor logs for compiler errors.
- Task 41: run targeted `StaticDataServiceTests` and `JigsawVinaGameDataEditorTests` cases covering drop-table validation and preservation.
- Task 42: run targeted `SaveDataServiceTests` cases covering migration, serialization round-trip, and daily reset.
- Tasks 43-44: run `DropRewardTests` plus the new reward presenter cases in `ProgressionTests`.
- Task 45: run the new Collection/navigation cases in `PictureSelectFlowTests` and the Home wiring cases in `LifetimeScopeRegistrationTests`.
- Do not run the full EditMode/PlayMode suites unless separately approved under the project verification policy.

### Manual Verification
- Launch the game from Home scene.
- Press the Collection button to view owned key items.
- Complete a puzzle on Easy to earn Key Item 107 (confirming reward mapping for picture 1 easy), then open Collection to verify it is shown with description and "Nhận từ: Làng Quê Bên Bụi Tre - Dễ".
- Click on the item to navigate directly to the selection screen of "Làng Quê Bên Bụi Tre".
