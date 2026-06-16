# Implementation Plan - Daily Login Reward System (Task 47)

**Goal:** Implement a Daily Login Reward system where players can claim a reward once per local calendar day. Rewards are configured in static data (e.g. 7-day cycle) and can include coins, hints, key items, or consumables. The Home screen will display a button with a notification badge if a reward is ready to claim.

**Architecture:**
- **Static Data:** Extend `StaticDataDto` and `jigsaw_vina_game_data.json` to configure exactly 7 days of daily login rewards.
- **Player Save:** Add `LastDailyRewardClaimDateString` and `DailyRewardStreak` to `PlayerSave`.
- **Shared Reward Applier:** Extract a shared `IRewardApplier` / `RewardApplier` service (which constructor-injects `IStaticDataService`) to unify coin, hint, key item, and consumable grant logic (stack clamping, duplicate key item compensation, etc.) across both `RewardSummaryPresenter` and `DailyRewardService`.
- **Core Logic:** Implement `IDailyRewardService` / `DailyRewardService` that evaluates claimability, streak progression/reset using `ILocalDateProvider`, and mutates `PlayerSave` without calling save internally (caller owns save lifecycle).
- **Editor Tooling:** Extend `JigsawVinaGameDataEditor` to configure daily rewards.
- **UI Screen:** Implement `DailyRewardView` (popup with 7 days slots, claim button, close button), a badge on the Home screen's Daily Reward button, and `DailyRewardPresenter` (with proper event cleaning / `IDisposable` lifecycle) to drive the popup and emit success events.
- **Scene Wiring:** Update `ThinVerticalSliceSceneSetup` to construct and wire the daily reward button, badge, and popup, then regenerate `Home.unity`.

**Tech Stack:** Unity 6000.3.11f1, uGUI, VContainer DI, UniTask, NUnit (EditMode tests).

---

## User Review Required

> [!IMPORTANT]
> **Streak Reset & Wrapping Logic**:
> - We implement consecutive daily login rewards:
>   - If today is `T` and last claim date is `Last`:
>     - If `T == Last` ➔ Already claimed today.
>     - If `T == Last + 1 day` ➔ Consecutive login. The next claimable reward is `(DailyRewardStreak % 7) + 1`.
>     - If `T > Last + 1 day` ➔ Missed day. Streak resets, and the next claimable reward is `1`.
>     - If `Last` is empty ➔ First time login. The next claimable reward is `1`.
> - Max streak days is 7. Claiming Day 7 wraps the streak back to 1 on the next consecutive day.
> - **Clock Drift Protection**: `CanClaimToday` parses dates using `System.DateTime.TryParseExact` defensively and returns `true` only if `Last` is empty OR today is strictly greater than `Last` (`T > Last`). If `T < Last` (clock drift / going backward), we guard and do not allow claiming.
> - **Defensive Date Parse Failure**: Date repair is done defensively inside `PlayerSave.Normalize()`. If parsing `LastDailyRewardClaimDateString` fails, `Normalize()` resets it to `null` (representing no previous claim) to avoid locking out the player. This ensures `CanClaimToday` remains a pure query with no side effects.
> - **Streak Defensive Bounds**:
>   - Inside `PlayerSave.Normalize()`, any invalid streak values (out of range `[0, 7]`) are reset to 0.
>   - Inside `GetNextRewardDayIndex`, we defensively read `save.DailyRewardStreak` using a local sanitized variable: `int sanitizedStreak = (save.DailyRewardStreak < 0 || save.DailyRewardStreak > 7) ? 0 : save.DailyRewardStreak;` to ensure no side effects mutate `save` during query calls.
> - **Display Range Guard [1, 7]**: `GetNextRewardDayIndex` must always return a valid day index in `[1, 7]`. If the player already claimed today (`T == Last`), the method will return the day index of tomorrow's reward: `(sanitizedStreak % 7) + 1` (clamped to `[1, 7]`), never returning 0.
> - **Shared Reward Applier Result & Policies**:
>   - `IRewardApplier.Apply` takes a `RewardApplyPolicy` parameter (`Standard` or `WithCompensation`) and returns a structured `RewardApplyResult` containing success status, applied item details, and whether compensation was applied, avoiding hardcoded feedback strings in core services.
>   - **Standard Policy**: No compensation is awarded (used for Drop Tables replay rewards). If Key Item is already owned or a consumable stack is full, it returns `Success = false`, `IsCompensated = false`, `AppliedAmount = 0` (or clamps consumables to what fits and returns `Success = true` only if addedAmount > 0).
>   - **WithCompensation Policy**: Award 100 coins compensation (used for Daily Rewards and first clear rewards). If a daily reward is a Key Item that the player already owns, or a consumable item that is already at `max_stack`, we compensate the player by granting **100 Coins** instead of the duplicate/full-stack item, setting `RewardApplyResult.IsCompensated = true`.
>   - **Item Config Requirement (No Bypass)**: Enforce that all items, including Coins (ID 1) and Hints (ID 2), must have valid, active configurations in static data. The `RewardApplier` resolves display names, max stacks, and active status exclusively from static data, with no hardcoded bypass or fallback display names.
>   - Compensation coin amount is defined as a public constant `RewardApplier.DuplicateRewardCompensationCoins = 100` inside `RewardApplier.cs` with direct tests verifying the compensation logic.
> - **Claim Fail Guard**: If `RewardApplier.Apply` returns `Success = false` during daily login claim (e.g. invalid configuration or invalid item ID passed), `ClaimDailyReward` will return a failure result and will NOT update `save.DailyRewardStreak` or `save.LastDailyRewardClaimDateString` to protect the player's claim attempt.
> - **Presenter Constructor Signature**: `DailyRewardPresenter` constructor injects only `DailyRewardView`, `IDailyRewardService`, and `ISaveDataService`. It does NOT inject `ILocalDateProvider` directly to avoid redundant dependencies.
> - **HomeFlowController Save Management**: `HomeFlowController` injects `ISaveDataService` to load/reload player save data (via `Load()`) at startup and after a reward is claimed, passing the fresh save to `IDailyRewardService` to update the Home screen's daily reward notification badge.
> - **Dedicated Editor Setup Tab**: A new 5th tab "Cấu hình Daily Reward" is added to `JigsawVinaGameDataEditor.cs` to manage the 7 daily login rewards, featuring active item selection, read-only Day labels, amount validation, and a preview thumbnail box.

---

## Proposed Changes

### Shared Reward Applier Service

#### [NEW] [IRewardApplier.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/IRewardApplier.cs)
- Define a unified interface for applying any reward item to `PlayerSave` with policies:
```csharp
using JigsawVina.Core.Data;

namespace JigsawVina.Core.Services
{
    public enum RewardApplyPolicy
    {
        Standard,         // Used for Drop Table replay rewards (no duplicate/full compensation)
        WithCompensation  // Used for Daily Rewards and first clear rewards (grants coins on duplicate/full)
    }

    public struct RewardApplyResult
    {
        public bool Success;
        public int ItemId;
        public int AppliedAmount;
        public string DisplayName;
        public bool IsCompensated;
    }

    public interface IRewardApplier
    {
        RewardApplyResult Apply(PlayerSave save, int itemId, int amount, RewardApplyPolicy policy);
    }
}
```

#### [NEW] [RewardApplier.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/RewardApplier.cs)
- Implement `IRewardApplier` constructor-injecting `IStaticDataService`:
  - **Item Config Constraint**: Enforce that all items, including Coins (ID 1) and Hints (ID 2), must exist as active items in the static data configurations loaded via `IStaticDataService`.
  - **Invalid Item Guard**: If `itemId` does not exist in static data, or if `amount <= 0`, or if `item.status != "active"`, or if the item type is unsupported ➔ Log warning and return `Success = false` with no modifications to `PlayerSave`.
  - Coin (ID 1) ➔ `save.Coins += amount`, returns `RewardApplyResult` with `ItemId = 1`, `AppliedAmount = amount`, `DisplayName` resolved from static data, and `IsCompensated = false`, `Success = true`.
  - Hint (ID 2) ➔ `save.Hints += amount`, returns `RewardApplyResult` with `ItemId = 2`, `AppliedAmount = amount`, `DisplayName` resolved from static data, and `IsCompensated = false`, `Success = true`.
  - Key Item ➔ Check if `save.OwnedItemIds.Contains(itemId)`.
    - If already owned:
      - If `policy == RewardApplyPolicy.WithCompensation` ➔ Apply duplicate compensation (grants coins defined by `public const int DuplicateRewardCompensationCoins = 100;`, increments `save.Coins`, returns `RewardApplyResult` with `ItemId = 1`, `AppliedAmount = 100`, `DisplayName` resolved from static data, `IsCompensated = true`, `Success = true`).
      - If `policy == RewardApplyPolicy.Standard` ➔ Return `Success = false`, `IsCompensated = false`, `AppliedAmount = 0`.
    - If not owned ➔ Add to `save.OwnedItemIds`, returns `RewardApplyResult` with `ItemId = itemId`, `AppliedAmount = 1`, `DisplayName` resolved from static data, `IsCompensated = false`, `Success = true`.
  - Consumable ➔ Find `InventoryItem` in `save.Inventory`, clamp new amount to `max_stack`, and append the actual delta.
    - If delta is 0 (fully stack clamped):
      - If `policy == RewardApplyPolicy.WithCompensation` ➔ Apply coin compensation of `DuplicateRewardCompensationCoins = 100` coins, returns `RewardApplyResult` with `ItemId = 1`, `AppliedAmount = 100`, `DisplayName` resolved from static data, `IsCompensated = true`, `Success = true`.
      - If `policy == RewardApplyPolicy.Standard` ➔ Return `Success = false`, `IsCompensated = false`, `AppliedAmount = 0`.
    - If delta > 0 ➔ Update `InventoryItem.Amount`, returns `Success = true`, `AppliedAmount = delta`, `IsCompensated = false`.

#### [MODIFY] [RewardSummaryPresenter.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/RewardSummaryPresenter.cs)
- Constructor-inject `IRewardApplier`.
- **Maintain Constructor Compatibility**: Add/retain backward-compatible overloads delegating to the master constructor to prevent compilation errors in PlayMode tests and VContainer setup:
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
                NoOpDropRewardService.Instance,
                new RewardApplier(staticDataService))
        {
        }

        public RewardSummaryPresenter(
            RewardSummaryView view,
            GameSessionService sessionService,
            ISaveDataService saveDataService,
            IStaticDataService staticDataService,
            IDropRewardService dropRewardService) : this(
                view,
                sessionService,
                saveDataService,
                staticDataService,
                dropRewardService,
                new RewardApplier(staticDataService))
        {
        }

        [Inject]
        public RewardSummaryPresenter(
            RewardSummaryView view,
            GameSessionService sessionService,
            ISaveDataService saveDataService,
            IStaticDataService staticDataService,
            IDropRewardService dropRewardService,
            IRewardApplier rewardApplier)
        {
            _view = view;
            _sessionService = sessionService;
            _saveDataService = saveDataService;
            _staticDataService = staticDataService;
            _dropRewardService = dropRewardService ?? NoOpDropRewardService.Instance;
            _rewardApplier = rewardApplier;
        }
```
- Refactor the presenter to delegate `ApplyDropRewards` (using `RewardApplyPolicy.Standard`) and first-clear items (using `RewardApplyPolicy.WithCompensation`) to `_rewardApplier`.

---

### Static Data & Game Data Editor

#### [MODIFY] [jigsaw_vina_game_data.json](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/Resources/GameData/jigsaw_vina_game_data.json)
- Add the `daily_rewards` section containing exactly 7 default rewards:
```json
    "daily_rewards": [
        { "day_index": 1, "item_id": 1, "amount": 50 },
        { "day_index": 2, "item_id": 2, "amount": 1 },
        { "day_index": 3, "item_id": 1, "amount": 100 },
        { "day_index": 4, "item_id": 10, "amount": 1 },
        { "day_index": 5, "item_id": 1, "amount": 150 },
        { "day_index": 6, "item_id": 2, "amount": 2 },
        { "day_index": 7, "item_id": 1, "amount": 300 }
    ]
```

#### [MODIFY] [StaticDataDto.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Data/StaticDataDto.cs)
- Add `DailyRewardDto` class:
```csharp
[Serializable]
public class DailyRewardDto
{
    public int day_index;
    public int item_id;
    public int amount;
}
```
- Add `public List<DailyRewardDto> daily_rewards = new();` in `StaticDataDto`.

#### [NEW] [DailyRewardConfig.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Data/DailyRewardConfig.cs)
- Create immutable runtime model for daily rewards:
```csharp
namespace JigsawVina.Core.Data
{
    public readonly struct DailyRewardConfig
    {
        public readonly int DayIndex;
        public readonly int ItemId;
        public readonly int Amount;

        public DailyRewardConfig(int dayIndex, int itemId, int amount)
        {
            DayIndex = dayIndex;
            ItemId = itemId;
            Amount = amount;
        }
    }
}
```

#### [MODIFY] [IStaticDataService.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/IStaticDataService.cs)
- Add contract method:
```csharp
IReadOnlyList<DailyRewardConfig> GetDailyRewards();
```

#### [MODIFY] [StaticDataService.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/StaticDataService.cs)
- Load `daily_rewards` list and map to `DailyRewardConfig`.
- Implement new contract method `GetDailyRewards()`.
- Add validation in `ValidateStaticData`:
  - `daily_rewards` must contain **exactly 7 items** with day indices `1` through `7` sequentially.
  - `item_id` must reference a valid active item.
  - `amount` must be greater than 0.
  - If `item_id` is a Key Item, `amount` must be exactly 1.

#### [MODIFY] [JigsawVinaGameDataEditor.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Editor/JigsawVinaGameDataEditor.cs)
- Add a new 5th tab "Cấu hình Daily Reward" to the editor toolbar layout.
- Keep `daily_rewards` list in memory.
- In `LoadStateFromDto`, if the list is null or does not have exactly 7 elements, **auto-seed/populate exactly 7 default rewards** (using item ID 1 for coins).
- In the new Editor tab UI:
  - Draw a fixed table/list for the 7 daily rewards where the `day_index` column is read-only (labels "Day 1" to "Day 7") preventing user modification.
  - Display `item_id` as a dropdown displaying only active items (Coins, Hints, active Key Items, and active Consumables).
  - Display a small preview thumbnail box showing the sprite icon of the selected item (if available).
  - Expose a positive integer field for the `amount`.
  - Validate amounts are positive integers before saving.
  - On "Save & Generate JSON", serialize these 7 rewards into the `daily_rewards` section of `jigsaw_vina_game_data.json`.

---

### PlayerSave Schema

#### [MODIFY] [PlayerSave.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Data/PlayerSave.cs)
- Add fields:
```csharp
public string LastDailyRewardClaimDateString;
public int DailyRewardStreak;
```
- In `Normalize()`, reset invalid streak to 0 and **defensively validate and repair `LastDailyRewardClaimDateString`**:
```csharp
        public void Normalize()
        {
            if (CompletedPuzzles == null) CompletedPuzzles = new();
            if (OwnedItemIds == null) OwnedItemIds = new();
            if (UnlockedPictureIds == null) UnlockedPictureIds = new();
            if (DailyDropCounts == null) DailyDropCounts = new();
            if (Inventory == null) Inventory = new();
            
            // Reset invalid streak values (out of range [0, 7]) defensively to 0
            if (DailyRewardStreak < 0 || DailyRewardStreak > 7)
            {
                DailyRewardStreak = 0;
            }

            // Defensively validate and repair LastDailyRewardClaimDateString using fully qualified System types
            if (!string.IsNullOrEmpty(LastDailyRewardClaimDateString))
            {
                if (!System.DateTime.TryParseExact(
                    LastDailyRewardClaimDateString, 
                    "yyyy-MM-dd", 
                    System.Globalization.CultureInfo.InvariantCulture, 
                    System.Globalization.DateTimeStyles.None, 
                    out _))
                {
                    // Repair invalid formats by resetting to empty
                    LastDailyRewardClaimDateString = null;
                }
            }
        }
```

---

### Daily Login Reward Service

#### [NEW] [IDailyRewardService.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/IDailyRewardService.cs)
- Define the Daily Login Reward Service contract:
```csharp
using JigsawVina.Core.Data;

namespace JigsawVina.Core.Services
{
    public struct ClaimDailyRewardResult
    {
        public int DayIndex;
        public RewardApplyResult ApplyResult;
        public bool Success;
    }

    public interface IDailyRewardService
    {
        bool CanClaimToday(PlayerSave save);
        int GetNextRewardDayIndex(PlayerSave save);
        ClaimDailyRewardResult ClaimDailyReward(PlayerSave save);
    }
}
```

#### [NEW] [DailyRewardService.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Services/DailyRewardService.cs)
- Implement `IDailyRewardService`.
- Constructor-inject `IStaticDataService`, `IRewardApplier`, and `ILocalDateProvider`.
- **Pure Mutation Design**: It does NOT inject `ISaveDataService` nor call save internally. The caller owns saving.
- Implement `CanClaimToday`:
  - Return `true` if `save.LastDailyRewardClaimDateString` is null/empty.
  - Parse `save.LastDailyRewardClaimDateString` and `todayString` using `System.DateTime.TryParseExact`. If parsing `Last` fails ➔ return `true` (treating it defensively as no previous claim, with no side effects).
  - Return `true` only if `today > lastClaim` (preventing claims on same day or when clock drift makes `today < lastClaim`).
- Implement `GetNextRewardDayIndex`:
  - **Defensive Local Streak Guard**: Reset invalid streak values (out of range `[0, 7]`) defensively using a local variable `int sanitizedStreak = (save.DailyRewardStreak < 0 || save.DailyRewardStreak > 7) ? 0 : save.DailyRewardStreak;` to ensure no side effects mutate `save` during query calls.
  - If `LastDailyRewardClaimDateString` is null/empty ➔ return 1.
  - Parse `LastDailyRewardClaimDateString` and `todayString` using `System.DateTime.TryParseExact`. If parsing fails ➔ return 1.
  - Calculate `daysDiff` = difference in days.
  - If `daysDiff == 1` (consecutive day) ➔ Return `(sanitizedStreak % 7) + 1`.
  - If `daysDiff > 1` (missed a day) ➔ Return 1.
  - If `daysDiff < 1` (same day or clock drift) ➔ Return `(sanitizedStreak % 7) + 1` (already claimed today, show next day to claim tomorrow, bounded within `[1, 7]`).
- Implement `ClaimDailyReward`:
  - If `!CanClaimToday(save)`, return fail result.
  - Get next day index `claimDay` using `GetNextRewardDayIndex`.
  - Fetch reward config for `claimDay` from static data.
  - Grant reward item using `IRewardApplier.Apply(save, itemId, amount, RewardApplyPolicy.WithCompensation)`.
  - **Apply Success Guard**: If `Apply` returned `Success = false` ➔ return fail result, leaving `DailyRewardStreak` and `LastDailyRewardClaimDateString` unchanged.
  - Update `save.DailyRewardStreak = claimDay`.
  - Update `save.LastDailyRewardClaimDateString = todayString`.
  - Return success result with `claimDay` and `RewardApplyResult`.

---

### UI View and Presenter

#### [NEW] [DailyRewardView.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/DailyRewardView.cs)
- Panel popup showing:
  - 7 daily reward slots (Day 1 - Day 7).
  - Each slot shows Day number, reward thumbnail image, amount, and status overlay (Claimed, Next/Claimable, Locked).
  - Central Claim button at the bottom (interactable only if reward can be claimed today).
  - Close button.
- Expose events:
  - `public event Action OnClaimRequested;`
  - `public event Action OnCloseRequested;`
- Methods:
  - `public void SetDailyRewardSlots(IReadOnlyList<SlotData> configs, int nextClaimableDay, bool canClaimToday);`
  - `public void ShowRewardClaimedFeedback(string itemName, int amount, bool isCompensated);`
  - `public void SetActive(bool active);`

#### [NEW] [DailyRewardPresenter.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/DailyRewardPresenter.cs)
- Drive `DailyRewardView`.
- Implement `IDisposable` to cleanly clean up listeners:
```csharp
        public void Dispose()
        {
            _view.OnClaimRequested -= Claim;
            _view.OnCloseRequested -= ClosePopup;
        }
```
- Constructor-inject `DailyRewardView`, `IDailyRewardService`, and `ISaveDataService` (omits `ILocalDateProvider`).
- Expose event `public event Action OnRewardClaimed;`.
- Methods:
  - `public void OpenPopup()`: load save, calls `Refresh()`, sets view active.
  - `public void ClosePopup()`: sets view deactive.
  - `private void Refresh()`: query `CanClaimToday` and `GetNextRewardDayIndex`, setup slots and Claim button on view.
  - `private void Claim()`: 
    - Execute `ClaimDailyReward`.
    - **Success Guard**: If `ClaimDailyRewardResult.Success == true`:
      - Save modified `PlayerSave` via `ISaveDataService.Save(save)`.
      - Show reward claimed feedback on the view.
      - Refresh the popup slots UI.
      - Fire `OnRewardClaimed` event to notify parent flow controller.
    - If `Success == false`:
      - Leave the popup open, show a warning or error message, and do NOT save or trigger parent flow events.

#### [MODIFY] [PictureSelectView.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/PictureSelectView.cs)
- Add serialized fields:
  - `Button _dailyRewardButton`
  - `GameObject _dailyRewardNotificationBadge`
- Expose event `public event Action OnDailyRewardRequested;` triggered by the button click.
- Add method `public void SetDailyRewardNotificationBadge(bool visible)` to show/hide the red dot.

#### [MODIFY] [HomeFlowController.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/HomeFlowController.cs)
- **Save Lifecycle Injection**: Inject `ISaveDataService` to manage loading and updating save state on badge refresh.
- In `Start()`:
  - Listen to `PictureSelectView.OnDailyRewardRequested` ➔ calls `DailyRewardPresenter.OpenPopup()`.
  - Listen to `DailyRewardPresenter.OnRewardClaimed` ➔ loads the fresh `PlayerSave` via `_saveDataService.Load()`, triggers `PictureSelectPresenter.Refresh()` and calling the existing `CollectionPresenter.Refresh()` method, and updates notification badge visibility via `IDailyRewardService.CanClaimToday(save)` using the newly loaded save.
  - On start, load `PlayerSave` via `_saveDataService.Load()` and check `IDailyRewardService.CanClaimToday(save)` to update `PictureSelectView.SetDailyRewardNotificationBadge`.
- In `Dispose()`:
  - Clean up all Daily Reward related subscriptions (specifically `OnDailyRewardRequested` and `OnRewardClaimed`).

#### [MODIFY] [HomeLifetimeScope.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/HomeLifetimeScope.cs)
- Register `DailyRewardView` (in scene hierarchy) and `DailyRewardPresenter` (as singleton).

#### [MODIFY] [ProjectLifetimeScope.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/App/ProjectLifetimeScope.cs)
- Register `RewardApplier` as `IRewardApplier` (singleton).
- Register `DailyRewardService` as `IDailyRewardService` (singleton).

---

### Scene Setup & Regeneration

#### [MODIFY] [ThinVerticalSliceSceneSetup.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Editor/ThinVerticalSliceSceneSetup.cs)
- Increment setup version check to `SetupVersionMarker_v7`.
- Modify `CreateHomeScene` to build:
  - A Daily Reward button in the Picture Select view top-right section (near Collection).
  - A red notification badge on it.
  - A Daily Reward Popup Panel view (deactive by default) wired to `HomeLifetimeScope` and containing the 7 day slot structures, Claim button, and Close button.
- Wire these serialized fields using `SerializedObject`.

---

## Verification Plan

### Automated Tests
- Create `JigsawVina/Assets/JigsawVina/Tests/DailyRewardTests.cs` (EditMode):
  - **RewardApplier Unit Tests**:
    - Test `RewardApplier` applying Coin (ID 1) and Hint (ID 2).
    - Test applying Consumable Item with stack clamping under `RewardApplyPolicy.Standard` (returns actual added amount, no compensation).
    - Test that applying a Consumable Item to a full stack under `RewardApplyPolicy.WithCompensation` triggers duplicate compensation (grants 100 Coins, returns `IsCompensated = true`).
    - Test that applying a Consumable Item to a full stack under `RewardApplyPolicy.Standard` fails (returns `Success = false`, grants 0 coins).
    - Test duplicate Key Item compensation under `RewardApplyPolicy.WithCompensation` (grants 100 Coins when Key Item is already owned, and checks constant `RewardApplier.DuplicateRewardCompensationCoins`).
    - Test duplicate Key Item under `RewardApplyPolicy.Standard` fails (returns `Success = false`).
    - Test `RewardApplier` edge cases: invalid item ID, inactive item, negative amount, or unsupported item type returns `Success = false` and performs no mutations on `PlayerSave`.
    - **RewardSummaryPresenter First-Clear Duplicate Key Item Regression Test**: Test that completing a puzzle difficulty for the first time when its first-clear reward is a Key Item the player already owns correctly awards the 100 Coin duplicate compensation instead of duplicating the Key Item.
  - **DailyRewardService Unit Tests**:
    - Test `CanClaimToday` returns `true` initially, `false` after claim, and correctly handles clock drift (returns `false` when today < lastClaim).
    - Test date parsing errors defensive reset of `LastDailyRewardClaimDateString` to null during `Normalize()` (and returns `true` for claimability with no side-effects).
    - Test streak calculation `GetNextRewardDayIndex` in consecutive cases (increments streak), missed cases (resets to 1), and bounds handling (streak out of `[0, 7]` range defaults to 0).
    - Test `ClaimDailyReward` wraps streak from Day 7 back to Day 1.
    - Test that `GetNextRewardDayIndex` never returns 0 when already claimed today (returns `(streak % 7) + 1` instead).
    - Test `ClaimDailyReward` fail guard (does not increment streak or claim date when `Apply` returns `Success = false`).
  - **Static Data Validator Tests**:
    - Verify that the static data validator correctly rejects daily rewards JSON configs that do not contain exactly 7 rewards.
    - Verify that the static data validator correctly rejects daily rewards if Coin/Hint items are missing or inactive in configuration.
  - **Game Data Editor Tests**:
    - Verify that the "Cấu hình Daily Reward" tab correctly seeds default daily rewards on null config.
    - Verify that the daily rewards settings round-trip successfully in DTOs.
  - **Presenter and Controller Lifecycle Tests**:
    - Test that `DailyRewardPresenter.Dispose()` unsubscribes `OnClaimRequested` and `OnCloseRequested`.
    - Test that `HomeFlowController.Dispose()` unsubscribes `OnDailyRewardRequested` and `OnRewardClaimed`.
- Run `LifetimeScopeRegistrationTests` (especially scene wiring tests) to assert that the new Home scene has correct button, badge, and popup configurations.

### Compile & Log Check
- Wait for Unity to compile/import all script and scene changes, inspect the Console/Editor log for compiler warnings or errors, and fix any compile errors before proceeding.

### Idempotency Check
- Run `ThinVerticalSliceSceneSetup` twice in succession. Check that the second run produces no modifications to `Home.unity` (identic SHA-256 hash).

### Manual Verification
- Regenerate the Home scene and open in Unity Editor.
- Launch in Play Mode.
- Verify that a red notification badge is shown on the Daily Reward button on day start.
- Click the button to open the popup, check the slot UI representation.
- Click Claim, verify that Coins/Hints are granted, the badge disappears, and the slot switches to Claimed.
- Verify that clicking Claim again is prevented and badge remains hidden.
