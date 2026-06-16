# Long C# Files Refactor Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:subagent-driven-development` (recommended) or `superpowers:executing-plans` to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Improve the longest maintainable project-owned C# files in one controlled refactor pass while preserving behavior, Unity scene output, static data schema, and existing runtime contracts.

**Architecture:** Do not refactor by line count alone. Exclude third-party plugin files and keep large test files unless fixture duplication blocks new work. Prioritize project-owned production/editor files whose length reflects mixed responsibilities: split pure validation/build logic first, then split move-only controllers/factories, and keep public APIs stable with compatibility wrappers.

**Tech Stack:** Unity 6000.3.11f1, C# Editor/runtime assemblies, VContainer, uGUI, Unity `JsonUtility`, NUnit EditMode/PlayMode tests.

---

## Long File Audit

Measured from `JigsawVina/Assets/**/*.cs` on 2026-06-16 in the planning workspace. Treat these counts as a snapshot, not a refactor contract. Before implementation, Task 1 reruns the line-count command and the implementer should use the current disk state. If a file has fewer lines locally but still contains the named responsibilities/methods below, keep the same refactor decision.

| Rank | File | Snapshot Lines | Type | Decision |
| :--- | :--- | ---: | :--- | :--- |
| 1 | `Scripts/Editor/JigsawVinaGameDataEditor.cs` | 2142 | Project Editor | Refactor now. Extract DTO build/validation first; GUI tab split later only if scope allows. |
| 2 | `Scripts/Editor/ThinVerticalSliceSceneSetup.cs` | 1103 | Project Editor utility | Refactor now, but only with move-only helper extraction and idempotency verification. |
| 3 | `Tests/ProgressionTests.cs` | 816 | Project tests | Do not refactor in this pass. Large tests are acceptable unless fixture duplication becomes a blocker. |
| 4 | `Plugins/Demigiant/DOTween/Modules/DOTweenModuleUI.cs` | 662 | Third-party plugin | Do not modify. |
| 5 | `Tests/DailyRewardTests.cs` | 629 | Project tests | Do not refactor in this pass. Coverage density is acceptable. |
| 6 | `Scripts/Core/Services/StaticDataService.cs` | 623 | Project runtime service | Refactor now. Extract validator and mapping helpers while preserving `IStaticDataService`. |
| 7 | `Tests/JigsawVinaGameDataEditorTests.cs` | 601 | Project tests | Modify only to add targeted regression coverage for editor refactor. |
| 8 | `Tests/DifficultySelectFlowTests.cs` | 439 | Project tests | Leave unchanged unless refactor breaks constructor/test helpers. |
| 9 | `Tests/PlayMode/PuzzleGameplayPlayModeTests.cs` | 412 | Project tests | Leave unchanged. |
| 10 | `Plugins/Demigiant/DOTween/Modules/DOTweenModuleUnityVersion.cs` | 389 | Third-party plugin | Do not modify. |
| 11 | `Tests/PictureSelectFlowTests.cs` | 385 | Project tests | Leave unchanged unless flow-controller move requires namespace adjustments. |
| 12 | `Scripts/Presentation/Screens/PuzzlePlayingPresenter.cs` | 338 | Project runtime presenter | Leave unchanged in this pass; length is acceptable for gameplay orchestration. |
| 13 | `Tests/StaticDataServiceTests.cs` | 330 | Project tests | Modify only if validator extraction needs direct regression coverage. |
| 14 | `Tests/DropRewardTests.cs` | 276 | Project tests | Leave unchanged. |
| 15 | `Tests/CollectionFlowTests.cs` | 259 | Project tests | Leave unchanged. |
| 16 | `Tests/LifetimeScopeRegistrationTests.cs` | 238 | Project tests | Run/modify only if scene setup extraction changes setup wiring tests. |
| 17 | `Scripts/Presentation/Screens/HomeLifetimeScope.cs` | 237 | Project runtime composition + controller | Refactor now with move-only split: put `HomeFlowController` in its own file. |
| 18 | `Scripts/Presentation/Screens/RewardSummaryPresenter.cs` | 229 | Project runtime presenter | Leave unchanged in this pass; already delegates reward application reasonably. |
| 19 | `Plugins/Demigiant/DOTween/Modules/DOTweenModulePhysics.cs` | 216 | Third-party plugin | Do not modify. |
| 20 | `Scripts/Presentation/Screens/PuzzlePieceView.cs` | 210 | Project runtime view | Leave unchanged; cohesive drag/feedback component. |

## Scope

### In Scope

- Extract project-owned long-file responsibilities that are already separable.
- Preserve behavior and public/internal contracts used by tests.
- Keep Unity serialized fields, scene references, and `.meta` files stable.
- Run targeted verification for the touched areas.

### Out of Scope

- Refactoring third-party DOTween files.
- Refactoring test files just to reduce line count.
- Scene design/layout changes.
- Runtime gameplay behavior changes.
- Static data schema changes.
- Full validation suite unless explicitly approved.

## File Structure

### Create

- `JigsawVina/Assets/JigsawVina/Scripts/Core/Services/StaticDataValidator.cs`
  - Runtime validation helper for `StaticDataDto`.
  - Owns logic currently in `StaticDataService.ValidateStaticData`, `ValidateUnlockConfiguration`, `ValidateProgressionReachability`, and `IsItemReachable`.

- `JigsawVina/Assets/JigsawVina/Scripts/Core/Services/StaticDataCatalogBuilder.cs`
  - Runtime mapping helper that converts validated `StaticDataDto` into collections used by `StaticDataService`.
  - Keeps `StaticDataService` focused on loading and query methods.

- `JigsawVina/Assets/JigsawVina/Scripts/Editor/JigsawVinaGameDataBuildInput.cs`
  - Editor-only snapshot for Game Data Editor build/validation.

- `JigsawVina/Assets/JigsawVina/Scripts/Editor/JigsawVinaGameDataBuilder.cs`
  - Editor-only builder for `StaticDataDto` output from editor state.

- `JigsawVina/Assets/JigsawVina/Scripts/Editor/ThinVerticalSliceUiFactory.cs`
  - Shared scene setup UI helpers currently embedded in `ThinVerticalSliceSceneSetup`.

- `JigsawVina/Assets/JigsawVina/Scripts/Editor/ThinVerticalSliceHomeSceneBuilder.cs`
  - Home scene construction extracted from `CreateHomeScene`.

- `JigsawVina/Assets/JigsawVina/Scripts/Editor/ThinVerticalSliceGameplaySceneBuilder.cs`
  - Gameplay scene construction extracted from `CreateGameplayScene`.

- `JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/HomeFlowController.cs`
  - Move `HomeFlowController` out of `HomeLifetimeScope.cs` without behavior changes.

### Modify

- `JigsawVina/Assets/JigsawVina/Scripts/Core/Services/StaticDataService.cs`
  - Delegate validation and mapping to helper classes.
  - Keep `IStaticDataService` behavior unchanged.

- `JigsawVina/Assets/JigsawVina/Scripts/Editor/JigsawVinaGameDataEditor.cs`
  - Keep IMGUI rendering/state ownership.
  - Delegate DTO build/validation to `JigsawVinaGameDataBuilder`.

- `JigsawVina/Assets/JigsawVina/Scripts/Editor/ThinVerticalSliceSceneSetup.cs`
  - Keep the menu entries, orchestration, folder/settings setup, importer setup, build settings, prefab generation route, and scene version markers.
  - Delegate Home/Gameplay scene construction and UI object creation to helpers.

- `JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/HomeLifetimeScope.cs`
  - Keep only VContainer registrations.
  - Remove moved `HomeFlowController` class body.

- Tests:
  - `JigsawVina/Assets/JigsawVina/Tests/StaticDataServiceTests.cs`
  - `JigsawVina/Assets/JigsawVina/Tests/JigsawVinaGameDataEditorTests.cs`
  - `JigsawVina/Assets/JigsawVina/Tests/LifetimeScopeRegistrationTests.cs`
  - Modify only for targeted regression coverage or namespace/file split fallout.

## Design Rules

- Do not manually create Unity `.meta` files.
- Do not change existing `.meta` GUIDs or serialized field names.
- Prefer move-only extraction before logic edits.
- Keep compatibility wrappers where tests already call existing methods.
- Keep all editor helpers under the existing Editor assembly.
- Keep runtime helpers under runtime service namespace only when they do not depend on UnityEditor.

---

### Task 1: Baseline And Guard Tests

**Files:**
- Read: `JigsawVina/Assets/JigsawVina/Scripts/Core/Services/StaticDataService.cs`
- Read: `JigsawVina/Assets/JigsawVina/Scripts/Editor/JigsawVinaGameDataEditor.cs`
- Read: `JigsawVina/Assets/JigsawVina/Scripts/Editor/ThinVerticalSliceSceneSetup.cs`

- [ ] **Step 1: Capture line-count baseline**

Run:

```powershell
Get-ChildItem -LiteralPath 'JigsawVina/Assets' -Recurse -Filter *.cs |
  ForEach-Object {
    $count = (Get-Content -LiteralPath $_.FullName).Count
    [PSCustomObject]@{ Lines=$count; Path=$_.FullName.Substring((Resolve-Path '.').Path.Length + 1) }
  } |
  Where-Object { $_.Lines -ge 200 } |
  Sort-Object Lines -Descending |
  Format-Table -AutoSize
```

Expected:
- Confirms the audit table above or records small drift if files changed.

- [ ] **Step 2: Run narrow pre-refactor tests**

Run targeted EditMode tests:

```text
JigsawVina.Tests.StaticDataServiceTests
JigsawVina.Tests.JigsawVinaGameDataEditorTests
JigsawVina.Tests.LifetimeScopeRegistrationTests
```

Expected:
- Tests pass before refactor. If they fail before any source changes, stop and investigate baseline failures first.

---

### Task 2: Move HomeFlowController To Its Own File

**Files:**
- Create: `JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/HomeFlowController.cs`
- Modify: `JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/HomeLifetimeScope.cs`

- [ ] **Step 1: Move class body unchanged**

Move the entire `public class HomeFlowController : IStartable, IDisposable` from `HomeLifetimeScope.cs` to `HomeFlowController.cs`.

The new file must keep the same namespace as `HomeLifetimeScope.cs`:

```text
namespace JigsawVina.Presentation.Screens
class to move: public class HomeFlowController : IStartable, IDisposable
```

Do not create a new controller implementation. Move the existing class body unchanged so VContainer registration and existing tests continue to resolve the same type.

Required usings should match the moved class dependencies:

```csharp
using System;
using JigsawVina.Core.Services;
using VContainer.Unity;
```

- [ ] **Step 2: Keep lifetime scope registration unchanged**

`HomeLifetimeScope.cs` should still register:

```csharp
builder.RegisterEntryPoint<HomeFlowController>();
```

- [ ] **Step 3: Compile and run flow tests**

Run:

```text
JigsawVina.Tests.DifficultySelectFlowTests
JigsawVina.Tests.PictureSelectFlowTests
JigsawVina.Tests.CollectionFlowTests
JigsawVina.Tests.DailyRewardTests
```

Expected:
- All pass or failures are limited to missing using/namespace issues from the move.

---

### Task 3: Extract StaticDataService Validation

**Files:**
- Create: `JigsawVina/Assets/JigsawVina/Scripts/Core/Services/StaticDataValidator.cs`
- Modify: `JigsawVina/Assets/JigsawVina/Scripts/Core/Services/StaticDataService.cs`
- Test: `JigsawVina/Assets/JigsawVina/Tests/StaticDataServiceTests.cs`
- Test: `JigsawVina/Assets/JigsawVina/Tests/ProgressionTests.cs`
- Test: `JigsawVina/Assets/JigsawVina/Tests/DailyRewardTests.cs`

- [ ] **Step 1: Create validator class**

Create `StaticDataValidator.cs` by moving these exact methods out of `StaticDataService.cs`:

```text
ValidateStaticData -> public static Validate
ValidateUnlockConfiguration -> private static ValidateUnlockConfiguration
ValidateProgressionReachability -> private static ValidateProgressionReachability
IsItemReachable -> private static IsItemReachable
```

Use namespace `JigsawVina.Core.Services` and keep existing validation logic, exception types, and exception messages unchanged. Required imports are expected to include `System`, `System.Collections.Generic`, `System.Linq`, and `JigsawVina.Core.Data`; add only what the compiler requires.

- [ ] **Step 2: Replace service validation call**

In `StaticDataService.LoadFromText`, replace:

```csharp
ValidateStaticData(dto);
```

with:

```csharp
StaticDataValidator.Validate(dto);
```

- [ ] **Step 3: Remove moved methods from service**

Remove from `StaticDataService.cs`:

```text
ValidateStaticData
ValidateUnlockConfiguration
ValidateProgressionReachability
IsItemReachable
```

- [ ] **Step 4: Run validator tests**

Run:

```text
JigsawVina.Tests.StaticDataServiceTests
JigsawVina.Tests.ProgressionTests
JigsawVina.Tests.DailyRewardTests
```

Expected:
- Existing validation behavior and exception messages remain stable.

---

### Task 4: Extract StaticDataService Mapping

**Files:**
- Create: `JigsawVina/Assets/JigsawVina/Scripts/Core/Services/StaticDataCatalogBuilder.cs`
- Modify: `JigsawVina/Assets/JigsawVina/Scripts/Core/Services/StaticDataService.cs`

- [ ] **Step 1: Create catalog result and builder**

Create `StaticDataCatalogBuilder.cs` with this data holder/signature reference, then move the mapping body from `StaticDataService.LoadFromText` into `StaticDataCatalogBuilder.Build` in the same edit:

```text
using System.Collections.Generic;
using System.Linq;
using JigsawVina.Core.Data;

namespace JigsawVina.Core.Services
{
    internal sealed class StaticDataCatalog
    {
        public List<PictureConfig> Pictures { get; set; } = new();
        public List<ItemDto> Items { get; set; } = new();
        public Dictionary<int, ItemDto> ItemsById { get; set; } = new();
        public Dictionary<(int PictureId, int DifficultyId), PictureDifficultyConfig> Difficulties { get; set; } = new();
        public List<DropTableConfig> DropTables { get; set; } = new();
        public Dictionary<int, List<DropTableItemConfig>> DropTableItemsByTableId { get; set; } = new();
        public List<DropTableItemConfig> AllDropTableItems { get; set; } = new();
        public List<DailyRewardConfig> DailyRewards { get; set; } = new();
    }

    internal static class StaticDataCatalogBuilder
    {
        public static StaticDataCatalog Build(StaticDataDto dto)
    }
}
```

Implement `Build` in the same edit by moving the exact mapping logic currently inside `StaticDataService.LoadFromText` after validation. Do not move resource loading or fallback loading into this builder, and do not leave an empty/default-returning shell.

- [ ] **Step 2: Delegate mapping from service**

After validation in `LoadFromText`, call:

```csharp
var catalog = StaticDataCatalogBuilder.Build(dto);
_pictures = catalog.Pictures;
_items = catalog.Items;
_itemsById = catalog.ItemsById;
_difficulties = catalog.Difficulties;
_dropTables = catalog.DropTables;
_dropTableItemsByTableId = catalog.DropTableItemsByTableId;
_allDropTableItems = catalog.AllDropTableItems;
_dailyRewards = catalog.DailyRewards;
```

- [ ] **Step 3: Run service tests**

Run:

```text
JigsawVina.Tests.StaticDataServiceTests
JigsawVina.Tests.DropRewardTests
JigsawVina.Tests.ProgressionTests
JigsawVina.Tests.DailyRewardTests
```

Expected:
- Data loading, query, progression validation, drop tables, and daily rewards remain unchanged.

---

### Task 5: Extract Game Data Editor Build/Validation

**Files:**
- Create: `JigsawVina/Assets/JigsawVina/Scripts/Editor/JigsawVinaGameDataBuildInput.cs`
- Create: `JigsawVina/Assets/JigsawVina/Scripts/Editor/JigsawVinaGameDataBuilder.cs`
- Modify: `JigsawVina/Assets/JigsawVina/Scripts/Editor/JigsawVinaGameDataEditor.cs`
- Test: `JigsawVina/Assets/JigsawVina/Tests/JigsawVinaGameDataEditorTests.cs`

- [ ] **Step 1: Create build input**

Create `JigsawVinaGameDataBuildInput.cs` with this member/signature reference:

```text
using System.Collections.Generic;
using JigsawVina.Core.Data;

namespace JigsawVina.Editor
{
    internal sealed class JigsawVinaGameDataBuildInput
    {
        public JigsawVinaGameDataBuildInput(
            IReadOnlyList<JigsawVinaGameDataEditor.EditorTabState> tabs,
            IReadOnlyList<JigsawVinaGameDataEditor.EditorCategoryState> categories,
            IReadOnlyList<ItemDto> globalItems,
            IReadOnlyList<DropTableItemDto> dropTableItems,
            IReadOnlyList<DailyRewardDto> dailyRewards)
        {
            Tabs = tabs ?? new List<JigsawVinaGameDataEditor.EditorTabState>();
            Categories = categories ?? new List<JigsawVinaGameDataEditor.EditorCategoryState>();
            GlobalItems = globalItems ?? new List<ItemDto>();
            DropTableItems = dropTableItems ?? new List<DropTableItemDto>();
            DailyRewards = dailyRewards ?? new List<DailyRewardDto>();
        }

        public IReadOnlyList<JigsawVinaGameDataEditor.EditorTabState> Tabs { get; }
        public IReadOnlyList<JigsawVinaGameDataEditor.EditorCategoryState> Categories { get; }
        public IReadOnlyList<ItemDto> GlobalItems { get; }
        public IReadOnlyList<DropTableItemDto> DropTableItems { get; }
        public IReadOnlyList<DailyRewardDto> DailyRewards { get; }
    }
}
```

- [ ] **Step 2: Create builder and scanner delegate**

Create these members:

```text
using System.Collections.Generic;
using JigsawVina.Core.Data;
using UnityEditor;
using UnityEngine;

namespace JigsawVina.Editor
{
    internal sealed class JigsawVinaGameDataBuilder
    {
        internal delegate (Texture2D mainTexture, List<Texture2D> itemTextures) FolderScanner(DefaultAsset folderAsset);

        private readonly FolderScanner _scanFolder;

        public JigsawVinaGameDataBuilder(FolderScanner scanFolder)
        {
            _scanFolder = scanFolder;
        }

        public bool TryBuild(
            JigsawVinaGameDataBuildInput input,
            out StaticDataDto config,
            out string errorMessage,
            bool validateAssets)

        private void AddDifficulty(
            StaticDataDto config,
            int pictureId,
            int diffId,
            string displayName,
            int cols,
            int rows,
            int firstClearCoins,
            int replayCoins,
            int firstClearHints,
            int rewardIndex,
            List<string> items,
            Dictionary<string, int> localItems,
            int dropTableId)
    }
}
```

Implement `TryBuild` and `AddDifficulty` in the same edit by moving the existing `JigsawVinaGameDataEditor.TryBuildConfig` and `AddDifficulty` implementations. Only change field references to `input.*` properties and replace `ScanFolder(...)` calls with `_scanFolder(...)`. Do not leave a stub implementation.

- [ ] **Step 3: Replace editor wrapper**

Add:

```csharp
        private JigsawVinaGameDataBuildInput CreateBuildInput()
        {
            return new JigsawVinaGameDataBuildInput(
                _tabs,
                _categories,
                _globalItems,
                _dropTableItems,
                _dailyRewards);
        }
```

Replace `TryBuildConfig` body:

```csharp
        internal bool TryBuildConfig(out StaticDataDto config, out string errorMessage, bool validateAssets = true)
        {
            var builder = new JigsawVinaGameDataBuilder(ScanFolder);
            return builder.TryBuild(CreateBuildInput(), out config, out errorMessage, validateAssets);
        }
```

- [ ] **Step 4: Run editor tests**

Run:

```text
JigsawVina.Tests.JigsawVinaGameDataEditorTests
JigsawVina.Tests.DailyRewardTests
```

Expected:
- Editor DTO round-trips, validation, reserved items, unlock-all, and daily reward editor tests pass.

---

### Task 6: Extract Thin Vertical Slice Scene Builders

**Files:**
- Create: `JigsawVina/Assets/JigsawVina/Scripts/Editor/ThinVerticalSliceUiFactory.cs`
- Create: `JigsawVina/Assets/JigsawVina/Scripts/Editor/ThinVerticalSliceHomeSceneBuilder.cs`
- Create: `JigsawVina/Assets/JigsawVina/Scripts/Editor/ThinVerticalSliceGameplaySceneBuilder.cs`
- Modify: `JigsawVina/Assets/JigsawVina/Scripts/Editor/ThinVerticalSliceSceneSetup.cs`
- Test: `JigsawVina/Assets/JigsawVina/Tests/LifetimeScopeRegistrationTests.cs`

- [ ] **Step 1: Extract shared UI factory**

Move helper methods from `ThinVerticalSliceSceneSetup.cs` into `ThinVerticalSliceUiFactory`:

```text
CreateCamera
CreateEventSystem
CreateCanvas
CreateScreen
AddHeader
CreateButton
CreateSlider
Assign
CreateLockIcon
CreateAchievementText
AddText
```

Use `internal static` methods so scene builders can call them.

Also update `CreatePictureSelectCardPrefabForTask38` in `ThinVerticalSliceSceneSetup.cs` to call `ThinVerticalSliceUiFactory.CreateButton(...)` and `ThinVerticalSliceUiFactory.Assign(...)` after those helpers move. This menu route should remain in `ThinVerticalSliceSceneSetup.cs`.

- [ ] **Step 2: Extract Home scene builder**

Move `CreateHomeScene()` body into the new builder. The target signature is:

```text
namespace JigsawVina.Editor
class: internal static class ThinVerticalSliceHomeSceneBuilder
method: public static void CreateHomeScene(string homeScenePath)
```

Replace local helper calls with `ThinVerticalSliceUiFactory`.

When moving the body, replace path-specific references:

```text
CheckSceneAlreadyUpdated(HomeScenePath, "SetupVersionMarker_v7")
EditorSceneManager.SaveScene(scene, HomeScenePath)
```

with:

```text
ThinVerticalSliceSceneSetup.CheckSceneAlreadyUpdated(homeScenePath, "SetupVersionMarker_v7")
EditorSceneManager.SaveScene(scene, homeScenePath)
```

Keep `CheckSceneAlreadyUpdated` on `ThinVerticalSliceSceneSetup`, change it from `private static` to `internal static`, and call it from the builder. Do not duplicate its logic in multiple builders.

- [ ] **Step 3: Extract Gameplay scene builder**

Move `CreateGameplayScene()` body into the new builder. The target signature is:

```text
namespace JigsawVina.Editor
class: internal static class ThinVerticalSliceGameplaySceneBuilder
method: public static void CreateGameplayScene(string gameplayScenePath)
```

Replace local helper calls with `ThinVerticalSliceUiFactory`.

When moving the body, replace path-specific references:

```text
CheckSceneAlreadyUpdated(GameplayScenePath, "SetupVersionMarker_v4")
EditorSceneManager.SaveScene(scene, GameplayScenePath)
```

with:

```text
ThinVerticalSliceSceneSetup.CheckSceneAlreadyUpdated(gameplayScenePath, "SetupVersionMarker_v4")
EditorSceneManager.SaveScene(scene, gameplayScenePath)
```

Use the same shared `ThinVerticalSliceSceneSetup.CheckSceneAlreadyUpdated(...)` call as the Home builder.

- [ ] **Step 4: Keep setup orchestration in original file**

`ThinVerticalSliceSceneSetup.Setup()` should still control:

```text
EnsureFolders
CreateProjectLifetimeScopePrefab
ConfigureVContainerSettings
ConfigurePuzzleTextureImporters
CheckSceneAlreadyUpdated as an internal shared marker helper
ConfigureBuildSettings
AssetDatabase.SaveAssets
AssetDatabase.Refresh
CreatePictureSelectCardPrefabForTask38 menu route
```

Scene creation calls should become:

```csharp
ThinVerticalSliceHomeSceneBuilder.CreateHomeScene(HomeScenePath);
ThinVerticalSliceGameplaySceneBuilder.CreateGameplayScene(GameplayScenePath);
```

Expose `CheckSceneAlreadyUpdated` as `internal static` on `ThinVerticalSliceSceneSetup` and call:

```text
ThinVerticalSliceSceneSetup.CheckSceneAlreadyUpdated(scenePath, markerName)
```

from both builders.

- [ ] **Step 5: Run scene wiring tests**

Run:

```text
JigsawVina.Tests.LifetimeScopeRegistrationTests
```

Expected:
- Existing Home/Game scene wiring assertions pass.

- [ ] **Step 6: Run idempotency only because scene setup code changed**

Run the existing scene setup route twice and compare `Home.unity` and `Gameplay.unity` hashes from before and after the second run.

Expected:
- Second run does not dirty scene files.

---

### Task 7: Final Targeted Verification

**Files:**
- No source changes unless failures require fixes.

- [ ] **Step 1: Unity compile/log check**

Wait for Unity import/compile and check Console/Editor log.

Expected:
- No compiler errors.

- [ ] **Step 2: Run touched-area EditMode tests**

Run:

```text
JigsawVina.Tests.StaticDataServiceTests
JigsawVina.Tests.ProgressionTests
JigsawVina.Tests.DropRewardTests
JigsawVina.Tests.DailyRewardTests
JigsawVina.Tests.JigsawVinaGameDataEditorTests
JigsawVina.Tests.DifficultySelectFlowTests
JigsawVina.Tests.PictureSelectFlowTests
JigsawVina.Tests.CollectionFlowTests
JigsawVina.Tests.LifetimeScopeRegistrationTests
```

Expected:
- All targeted tests pass.

- [ ] **Step 3: Skip unrelated broad checks by default**

Do not run full EditMode suite, full PlayMode suite, or manual click-through unless the user explicitly asks or a targeted failure indicates wider impact.

Record skipped checks as:

```text
not run - not relevant to focused long-file refactor scope
```

---

### Task 8: Documentation Handoff

**Files:**
- Modify: `docs/plans/task.md`
- Modify: `docs/plans/current-handoff.md`

- [ ] **Step 1: Update task tracker after implementation**

Set Task 48 to complete:

```markdown
| **Task 48: Long C# Files Refactor** | [x] | Extracted focused helpers from long project-owned C# files: StaticDataService validation/mapping, Game Data Editor build/validation, Thin Vertical Slice scene builders, and HomeFlowController file split. |
```

- [ ] **Step 2: Update current handoff after implementation**

Add:

```markdown
- **Task 48: Long C# Files Refactor**:
  - Extracted `StaticDataValidator` and `StaticDataCatalogBuilder` from `StaticDataService`.
  - Extracted `JigsawVinaGameDataBuilder` from `JigsawVinaGameDataEditor`.
  - Extracted Thin Vertical Slice scene builder/factory helpers.
  - Moved `HomeFlowController` into its own file without behavior changes.
```

Add verification:

```markdown
- **Task 48 targeted verification**:
  - Unity script compilation completed with no compiler errors.
  - Touched-area EditMode tests passed.
  - Scene setup idempotency passed after scene setup extraction.
  - Full EditMode/PlayMode suites: not run - not relevant to focused long-file refactor scope.
```

## Risk Controls

- Third-party plugin files are explicitly excluded.
- Tests are not split for line-count aesthetics.
- Scene setup refactor requires idempotency verification.
- Static data refactor requires validation and progression tests.
- Editor build refactor keeps `TryBuildConfig` compatibility wrapper.
- Unity `.meta` files must be generated by Unity, not manually.
