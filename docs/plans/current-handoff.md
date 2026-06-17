# Current Handoff

- **UI/UX Design System and Screen Specification**:
  - Added `docs/plans/2026-06-16-ui-ux-design-system-and-screen-spec.md`.
  - Captures the approved mobile landscape direction: 16:9 primary layout, 4:3-safe responsive behavior, two-handed play, icon-first Vietnamese-supported UI, and localization-ready text rules.
  - Defines the warm Vietnamese handmade visual system: paper/album/stamp UI for meta screens, light wood/craft treatment for Gameplay, and restrained traditional accents.
  - Establishes DOTween as the primary UI animation and feedback system, with cleanup rules and guidance for popup, card, gameplay, daily, reward, unlock, and collection animations.
  - Includes detailed screen specs and AI layered asset briefs for Daily Reward, Home, Difficulty Select, Gameplay, Reward/Win, Collection, and Pause/Settings.
  - Review pass tightened Daily Reward claim/skip policy, AI asset constraints, 16:9/4:3 reference targets, and Gameplay Pause/Back behavior to reduce implementation ambiguity.
  - Added the design doc to `docs/plans/index.md`.

- **AI Image Brief Companion**:
  - Added `docs/plans/2026-06-16-ui-ux-ai-image-brief.md`.
  - Companion file tells an AI image assistant how to interpret the UI/UX design system, classify expected outputs, avoid baked text/fake labels, respect transparent/9-slice-friendly layered asset constraints, and produce mood-board or production-oriented asset plans.
  - Added explicit guidance for large full-screen mockups used only for AI review, art-direction alignment, and prompt iteration; these mockups are not Unity slicing sources or production assets.
  - Added the companion brief to `docs/plans/index.md`.

- **Task 48: Long C# Files Refactor**:
  - Audited all `.cs` files with 200+ lines and classified them by project runtime/editor/test vs third-party plugin code.
  - Created the refactor plan at `docs/plans/2026-06-16-long-csharp-files-refactor.md`.
  - Extracted `StaticDataValidator` and `StaticDataCatalogBuilder` from `StaticDataService.cs` to handle validation and mapping logic.
  - Fixed compilation error `CS0518` in Unity Editor by changing `StaticDataCatalog` properties from `init;` to `set;` in `StaticDataCatalogBuilder.cs`.
  - Extracted `JigsawVinaGameDataBuilder` and `JigsawVinaGameDataBuildInput` from `JigsawVinaGameDataEditor.cs` to decouple game data construction and validation.
  - Extracted UI creation helpers and scene builders (`ThinVerticalSliceUiFactory`, `ThinVerticalSliceHomeSceneBuilder`, `ThinVerticalSliceGameplaySceneBuilder`) from `ThinVerticalSliceSceneSetup.cs`.
  - Moved `HomeFlowController` out of `HomeLifetimeScope.cs` to its own file (`HomeFlowController.cs`) with explicit namespace preservation.

- **Game Test Case System Design**:
  - Approved a Markdown-first test case workflow for QA, Codex, and Antigravity.
  - Scoped coverage to the full current player flow, save/load, progression, daily drops, Collection, and error cases; Game Data Editor is excluded.
  - Defined module files, strict test case schema, validation rules, manual Excel export, and the later NUnit mapping workflow.
  - Clarified document lifecycle: the dated plan is a historical design decision record, while `docs/test-cases/README.md` and module files are living documents updated through normal Git history.
  - Review fixes added case lifecycle/deprecation, schema versioning, safe workbook replacement, Excel worksheet-name validation, and support for multiple NUnit mappings.
  - Export architecture uses a manual PowerShell entrypoint plus a tested Node.js module backed by the bundled `@oai/artifact-tool`; no Excel installation or global package is required.
  - Added the implementation plan at `docs/plans/2026-06-15-game-test-case-system-implementation.md`.
  - Added the detailed design at `docs/plans/2026-06-15-game-test-case-system-design.md`.

- **Task 46: Markdown Living Test Plan & Manual Excel Exporter**:
  - Created `docs/test-cases/README.md` as the Living Test Plan and six module files: Gameplay, Save Load, Progression, Daily Drop, Collection, and Error Handling.
  - Added 31 initial `Active` / `Critical` / `Smoke` / `Planned` QA test cases plus a 15-topic Regression backlog without reserving IDs.
  - Added `.gitignore` rules for generated QA workbook artifacts and exporter runtime/preview scratch paths.
  - Implemented `tools/test-cases/export_test_cases_to_excel.mjs` with strict UTF-8 parsing, schema validation, source-module binding, escaped-pipe table parsing, duplicate metadata detection, workbook content verification, direct CLI support, and `.xlsx` output guards.
  - Implemented `tools/test-cases/export_test_cases_to_excel.ps1` as the manual entrypoint that receives bundled Node/runtime paths, creates a verified temp junction, and cleans up safely.
  - No project `.xlsx` was generated; fixture workbook exports were created only in OS temp for verification and then cleaned up.

- **Task 47: Daily Login Reward System**:
  - Configured exactly 7 days of daily login rewards in static JSON data.
  - Tracked login streak in `PlayerSave` (`LastDailyRewardClaimDateString`, `DailyRewardStreak`).
  - Implemented `IRewardApplier` / `RewardApplier` to unify reward grants, handling stack clamping, owned Key Item duplicate detection, and 100 Coin duplicate compensation. Enforced active Coin config validation on the compensation path.
  - Implemented `DailyRewardService` resolving claimability, streak progression/wrapping, clock-drift protection, and defensive date parse validation.
  - Implemented `DailyRewardView` popup panel displaying the 7 reward slots with correct status overlays (resolving claimed/locked state bug), and loading sprites dynamically from `ItemDto.asset_path`. Added clean fallback logic deactivating the image component if the sprite is null and appending the item display name to the amount text. Enabled text wrapping and best fit sizing to prevent text clipping in the 120x30 slot area.
  - Wired Home screen daily reward button, red notification badge, and Popup UI to the VContainer context and flow controller.
  - Removed the hardcoded debug file writing code in `StaticDataService.cs` (`ValidateStaticData` method) and cleaned up `debug-json.txt` / `debug-error.txt`.
  - Added new EditMode tests (`DailyRewardTests`) covering RewardApplier policy branches, streak bounds, wrapping logic, date parse failures, presenter/controller cleanups, and a direct view setup test with empty asset paths (fallback text and disabled image), with updated unsubscribe validation for the flow controller.
  - Synchronized the plan file `2026-06-16-daily-login-reward-system.md` to match the actual code signature of `SetDailyRewardSlots`.

- **Tasks 43-45: Daily Drop Rewards & Collection UI**:
  - Implemented `DropRewardService` with per-item daily decay, minimum-rate clamping, independent active-entry rolls, inclusive amount ranges, owned Key Item/full consumable exclusion, partial-stack rewards, and success counters.
  - Integrated replay drops into `RewardSummaryPresenter` while preserving its old constructor; applied coins, hints, Key Items, and consumables using the actual granted amount.
  - Added `CollectionView` and `CollectionPresenter` for owned Key Items, deterministic first-clear/drop-table source mapping, source navigation, and event cleanup.
  - Extended picture selection and Home flow with Collection open/close, unlocked-picture navigation to Difficulty Select, and locked-picture focus/highlight.
  - Updated `ThinVerticalSliceSceneSetup` to Home marker v6, regenerated `Home.unity`, and kept Gameplay marker/scene at v4.
  - Added targeted `RunTask43Tests`, `RunTask44Tests`, and `RunTask45Tests` routes.

- **Task 42: PlayerSave Structure, Save Migration & RNG/Service Contracts**:
  - Extended `PlayerSave` with `LastSaveDateString`, `DailyDropCounts`, and `Inventory` lists.
  - Added a `Normalize(string localDateString)` overload to reset daily drop counters on calendar date change.
  - Implemented mockable `ILocalDateProvider` and `LocalDateProvider`.
  - Added unit tests in `SaveDataServiceTests` asserting daily reset of counters upon date change.
  - Registered `ILocalDateProvider`, `IRandomSource` (with `UnityRandomSource`), and `IDropRewardService` (with `DropRewardService` skeleton) in VContainer's `ProjectLifetimeScope`.

- **Task 41: Editor Integration & Static Data Validation**:
  - Added `DropTableDto`, `DropTableItemDto` DTOs, and immutable `DropTableConfig`, `DropTableItemConfig` classes.
  - Implemented comprehensive static data validations for drop configurations (uniqueness of IDs/id_strings, rate bounds, amount bounds, item types, active references).
  - Updated `JigsawVinaGameDataEditor` to display and configure `drop_table_id` fields per difficulty.
  - Updated test mock assemblies and added validation tests in `StaticDataServiceTests` and round-trip tests in `JigsawVinaGameDataEditorTests`.

- **Task 40: Scene Wiring, Regeneration & Manual Verification**:
  - Appended Picture 6 (initially locked, requiring Key Item 107) and its difficulty configurations to `jigsaw_vina_game_data.json`, and created its own unique premium image asset at `Textures/Pictures/Picture_6/MAIN_old_village_north_002` to avoid duplicate content reuse.
  - Refactored `ThinVerticalSliceSceneSetup.cs` to position difficulty selection buttons, dynamically spawn lock icons and achievement text elements, and automatically wire them to the serialized fields of `DifficultySelectView` using `SerializedObject`.
  - Reverted the Gameplay scene setup check to `SetupVersionMarker_v4` and restored `Gameplay.unity` to prevent unnecessary regeneration and scene churn, while keeping `Home.unity` at `SetupVersionMarker_v5`.
  - Fixed a potential crash in `DifficultySelectPresenter` when loading valid pictures with fewer than three difficulties by checking configured difficulties first and dynamically hiding unconfigured difficulty buttons on the view.
  - Extended `LifetimeScopeRegistrationTests.cs` with tests asserting correct wiring of lock icons/achievement texts on `DifficultySelectView`, and card prefab fields (`_lockOverlay`, `_missingItemsHintText`, `_unlockButton`).
  - Added targeted `RunTask40Tests` helper to `TestRunnerHelper` and verified all 75 EditMode and 8 PlayMode tests pass cleanly.

- **Task 39: Update DifficultySelect View & Presenter with Leak Fix**:
  - Updated `DifficultySelectView` to add serialized arrays for lock icons (`_lockIcons`) and achievement texts (`_achievementTexts`), exposing `LockIcons`, `AchievementTexts`, and public helper `SetDifficultyState`.
  - Updated `DifficultySelectPresenter` to support `Refresh()`, querying progression policies/unlock states, formatting best star and time achievements, logical selection guards, and implementing `IDisposable` event cleanup.
  - Updated `HomeFlowController` in `HomeLifetimeScope` to store `DifficultySelectPresenter` and call its `Refresh()` method when a picture is selected.
  - Made `SceneLoader.LoadSceneAsync` virtual to enable mocking in tests.
  - Implemented 6 EditMode tests in `DifficultySelectFlowTests.cs` covering view properties, presenter UI refresh, allowed/locked selection loading behavior, presenter disposal event unsubscribing, and HomeFlowController integration.

- **Task 38: Update PictureSelect Card, Presenter & UI Prefab**:
  - Updated `PictureSelectCard` to bind `PictureCardPresentationModel`, block selection while locked, display lock/hint UI, and expose a separate Unlock action only for `ReadyToUnlock`.
  - Updated `PictureSelectView` with `OnPictureUnlockRequested` and model-based dynamic card setup.
  - Updated `PictureSelectPresenter` to build progression-aware models, generate missing item source hints, unlock through `ProgressionService`, refresh cards after success, and unsubscribe both view events.
  - Updated the prefab generator and regenerated `PictureSelectCard.prefab` with `LockOverlay`, `LockIcon`, `KeyItemPanel`, `MissingItemsHint`, and `UnlockButton` while preserving the existing `.meta`.
  - Added targeted card/view/presenter tests covering locked, ready, unlocked, hint, refresh, and disposal behavior.

- **Task 36-37 Review Fixes**:
  - Fixed unlock validation to require `item.status == "active"` instead of accepting missing/empty status.
  - Added regression tests for missing active status, consumable requirements, duplicate requirements, invalid difficulty policies, and sequential difficulty gaps.
  - Strengthened atomic unlock coverage to verify failed/repeated attempts do not save or create duplicate unlocked picture IDs.
  - Updated legacy test fixtures so validator tests reach the intended rule instead of failing on unrelated missing status fields.

- **Task 37: Implement Core Progression Service & Validator Logic**:
  - Implemented `ProgressionService` picture states, atomic unlock persistence without consuming key items, difficulty policies, and deterministic item source hints.
  - Extended `StaticDataService` validation for unlock requirements, policy values, difficulty continuity, and unreachable/circular progression deadlocks.
  - Registered `ProgressionService` as a VContainer singleton and added a targeted `RunTask37Tests` route.
  - Migrated affected test fixtures to satisfy the new difficulty and reachability contract.

- **Task 36: Write Progression & Validator Unit Tests (TDD)**:
  - Added test case `PlayerSave_Normalize_InitializesNullLists` for save migration / normalization.
  - Added test cases `GetPictureState_InitiallyUnlocked_ReturnsUnlockedOrCompleted`, `GetPictureState_NotInitiallyUnlocked_Locked_AndTransitionToReadyToUnlock`, `GetPictureState_AfterUnlocking_ReturnsUnlocked`, and `GetPictureState_LockedEvenWithCompletions_IfUnlockFlagMissing`.
  - Added atomic unlock lifecycle and constraints test case `TryUnlockPicture_FlowAndConstraints`.
  - Added tests `IsDifficultyUnlocked_SequentialPolicy` and `IsDifficultyUnlocked_AllUnlockedPolicy` verifying difficulty locking rules.
  - Added test case `GetItemSourceHints_ReturnsCorrectSources` for source item tracking.
  - Added static data validator tests `ValidateStaticData_DeadlockRequiredItemUnreachable_ThrowsException`, `ValidateStaticData_DeadlockCircularRequirement_ThrowsException`, and `ValidateStaticData_UnlockRequirementNotKeyItem_ThrowsException`.
  - Verified they compile cleanly and fail correctly (11 failing tests due to skeletal implementations and lack of deadlock detection).

- **Task 35: Data Model, Save Migration, Static Data Contract & Editor Integration**:
  - Expanded `EditorTabState` in `JigsawVinaGameDataEditor.cs` with fields: `isInitiallyUnlocked`, `difficultyUnlockPolicy`, and `unlockRequirements`.
  - Implemented GUI Editor fields in the Picture Configuration detail view (toggle, popup, list elements).
  - Updated mapping logic `LoadStateFromDto` and `TryBuildConfig` to support serializing and deserializing these new locking attributes.
  - Enhanced the "Unlock All" cheat editor button to automatically populate `UnlockedPictureIds` inside the save database for non-initially unlocked pictures.
  - Cleaned up compile-breaking string literal unescaped quotes in both `StaticDataServiceTests.cs` and `ProgressionTests.cs`.
  - Verified 100% test success: 45/45 EditMode tests and 8/8 PlayMode tests pass.

- **Review Fixes (P1 & P2)**:
  - Fixed VContainer DI resolution issue by registering `StaticDataService` using a factory lambda in `ProjectLifetimeScope.cs`, bypassing the boolean parameter injection error.
  - Fixed `KeyNotFoundException` in PlayMode when starting directly from the `Gameplay` scene by defaulting to the first available picture configuration and difficulty 0 if `SelectedPictureId == 0` (in `PuzzlePlayingPresenter.Initialize()`).
  - Successfully regenerated `PictureSelectCard.prefab` at the updated `560x120` dimensions on disk.
  - Regenerated the `Home.unity` scene to reference the updated prefab.
  - Verified that all 45 EditMode tests and 8 PlayMode tests pass cleanly with no compilation errors or runtime exceptions.

- **Review Fixes (P2 & P3)**:
  - Adjusted card width from `640f` to `560f` inside the `Setup` script and regenerated the prefab to prevent horizontal clipping under the `600f`-wide scroll view viewport.
  - Refactored EditMode tests to use a reflection-based private `Awake()` helper, keeping production code free of manual lifecycle calls.
  - Expanded `LifetimeScopeRegistrationTests.cs` to verify `VerticalLayoutGroup` configuration, `ContentSizeFitter` options, and assert card width fits within ScrollView width.
  - Deduplicated Completed section entries and cleaned up Pending section in [task.md](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/docs/plans/task.md).

- **Task 34: Verify and Update Dynamic Home UI Documentation**:
  - Verified compilation logs and ran all tests successfully.
  - Performed scene regeneration idempotency check twice without issues.
  - Updated [task.md](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/docs/plans/task.md) and [current-handoff.md](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/docs/plans/current-handoff.md).

- **Task 33: Add Targeted Home UI EditMode Tests**:
  - Created [PictureSelectFlowTests.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Tests/PictureSelectFlowTests.cs) validating dynamic cards setup, unbind, twice setups, and proper Disposes on presenter/flow controller.
  - Added `HomeScene_PictureSelectView_IsWiredCorrectly` test to [LifetimeScopeRegistrationTests.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Tests/LifetimeScopeRegistrationTests.cs) verifying Scroll View layout, prefab properties, and that old buttons are gone.

- **Task 32: Regenerate Home Scene Setup v4**:
  - Updated [ThinVerticalSliceSceneSetup.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Editor/ThinVerticalSliceSceneSetup.cs) to build a Scroll View layout (Viewport/Content) and wire the prefab.
  - Regenerated `Home.unity` to v4 marker with a clean Scroll View and wired prefabs.

- **Task 31: Harden Home Flow and VContainer Lifecycle**:
  - Refactored [HomeLifetimeScope.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/HomeLifetimeScope.cs).
  - Implemented `IDisposable` on `HomeFlowController`.
  - Replaced anonymous lambda listeners with named handlers (`HandlePictureSelected` and `HandleBackButtonClicked`) to prevent event listener leaks.
  - Ensured correct cleanup of all flow listeners when the Home scope is disposed.

- **Task 30: Connect PictureSelectPresenter to Static Data**:
  - Modified [PictureSelectPresenter.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/PictureSelectPresenter.cs).
  - Registered `IStaticDataService` dependency via constructor injection.
  - In `Initialize()`, loaded the list of all available picture configs using `GetAllPictures()` and verified it's not null/empty, logging clear data errors.
  - Wired data list to View dynamically by calling `Setup(pictures)`.
  - Implemented `IDisposable` to cleanly unsubscribe from `OnPictureSelected` event when the presenter is destroyed.

- **Task 29: Refactor PictureSelectView for Dynamic Cards**:
  - Modified [PictureSelectView.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/PictureSelectView.cs) to remove hardcoded button references.
  - Added fields for the `PictureSelectCard` prefab and a `RectTransform` content container.
  - Implemented the dynamic card instantiation loop in `Setup()`, performing thorough null checks, assigning card names dynamically, and binding selected ID callbacks.
  - Implemented `ClearExistingCards()`, ensuring instantiated cards are unbound cleanly via `Unbind()` before destruction to avoid memory leaks.
  - Exposed the internal `IReadOnlyList<PictureSelectCard> InstantiatedCards` list for unit testing.

- **Task 28: Create PictureSelectCard Component and Prefab**:
  - Created new C# component script [PictureSelectCard.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/PictureSelectCard.cs) to bind/unbind UI card elements dynamically.
  - Fix [P3]: Ensured `_thumbnailImage.sprite = null` is set if asset path is invalid or sprite loading fails to prevent reused cards from keeping old thumbnail.
  - Programmatically generated prefab `PictureSelectCard.prefab` under `Assets/JigsawVina/Prefabs/` using the menu item helper `JigsawVina/Task 28/Create Picture Select Card Prefab`.
  - Let Unity compile and generate the corresponding `.meta` files for both assets.

- **Review Fixes [P2 & P3]**:
  - Fix [P2]: Patched the `Assign` helper in [ThinVerticalSliceSceneSetup.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Editor/ThinVerticalSliceSceneSetup.cs) to check if the target serialized property is null before assigning value. This prevents the editor setup script from crashing on missing fields during intermediate refactoring states.
  - Fix [P3]: Handled sprite reset in `PictureSelectCard.Bind()` on null path or load failures.

- **Task 27: Final Review Dynamic Home UI Plans**:
  - Reviewed `2026-06-12-dynamic-home-ui-design.md` and `2026-06-12-dynamic-home-ui-implementation.md` for prefab setup order, VContainer disposal, dynamic card cleanup, sprite references, scene regeneration, and targeted verification scope.
  - Tightened `Presenter_OnDispose_UnsubscribesFromView` to assert the exact default session state.
  - Expanded `FlowController_OnDispose_UnsubscribesFromView` to prove both picture-selection and Back-button listeners no longer change screen state after disposal.
  - No runtime code or Unity asset was changed as part of this review.

- **Task 26: Cấu hình Localization Keys & Sửa lỗi Review**:
  - **Localization Keys GUI & Config**: Thêm trường `display_name_key` và `description_key` vào các DTO (`CategoryDto`, `PictureDto`, `ItemDto`), `PictureConfig` runtime và giao diện cấu hình của `JigsawVinaGameDataEditor.cs`.
  - **Auto-Generation Rules**: Hỗ trợ tự động điền mã ngôn ngữ theo định dạng chuẩn nếu bỏ trống:
    - Tranh: `picture.<id_string>.name` / `picture.<id_string>.description`
    - Danh mục: `category.<id_string>.name` / `category.<id_string>.description`
    - Key Items: `item.<id_string>.name` / `item.<id_string>.description`
  - **Sửa P1 (Save mẫu & Difficulty ID)**: Đồng bộ file `docs/jigsaw_vietnam_player_save_sample_v0_1.json` khớp với runtime save schema và đổi giá trị difficulty_id về định dạng `0/1/2` (thay cho `1/2/3`) trong cả save mẫu lẫn static data mẫu.
  - **Sửa P2 (Runtime Validator)**: Tích hợp bộ kiểm tra tĩnh toàn diện vào `StaticDataService.cs` (kiểm tra schema_version, Category reference, trùng lặp picture/item/difficulty ID, giới hạn key items, giá trị âm và piece count).
  - **Sửa P2 (Nguồn dữ liệu thừa)**: Xóa bỏ file `Assets/Resources/StaticData.json` và `.meta` đi kèm.
  - **Sửa lỗi Unit / PlayMode Tests**:
    - Cập nhật mock JSON trong `ProgressionTests.cs` và `StaticDataServiceTests.cs` bổ sung trường phiên bản và cấu trúc danh mục hợp lệ để vượt qua bộ lọc validator mới.
    - Sửa lỗi trong `PuzzleGameplayPlayModeTests.cs` chuyển các tham chiếu tĩnh `GetChild(0)` sang tìm kiếm `PuzzlePieceView` theo chỉ số index thực tế nhằm hỗ trợ thuật toán xáo trộn khay tranh (Tray Shuffling) đã triển khai trước đó.

## Verification

- **Task 46 targeted verification**:
  - Exporter tests: `tools/test-cases/test/export_test_cases_to_excel.test.mjs` passed 31/31.
  - Real Living Test Plan validation: `VALID: schema=1 modules=6 cases=31`.
  - In-memory workbook verification for real cases: 31 cases across 8 sheets.
  - Manual PowerShell wrapper fixture export/render succeeded: 1 fixture case, 3 sheets.
  - Project artifact cleanup check: `PROJECT_XLSX=0`, `PROJECT_PREVIEW_DIRS=0`, `LOCAL_NODE_MODULES_JUNCTION=False`.
- **Tasks 43-45 targeted verification**:
  - Unity script compilation completed with no compiler errors.
  - Task 44 route (`DropRewardTests` + `ProgressionTests`): 31/31 passed.
  - Task 45 route (`CollectionFlowTests` + `PictureSelectFlowTests` + `LifetimeScopeRegistrationTests`): 21/21 passed.
  - Home v6 scene setup idempotency passed; SHA-256 hashes for both `Home.unity` and `Gameplay.unity` were unchanged on the second setup run.
- **Task 47 targeted verification**:
  - Unity script compilation completed with no compiler errors.
  - `DailyRewardTests`: Verified 22 EditMode tests covering RewardApplier, DailyRewardService, StaticData validation, presenter lifecycle, flow controller event unsubscriptions, and empty asset path fallback behavior. All 22 tests pass.
  - Chạy toàn bộ EditMode test suite: 120/120 tests PASS.
  - Chạy toàn bộ PlayMode test suite: 8/8 tests PASS.
  - Home v7 scene setup và cấu hình daily reward button, badge, popup panel hoàn tất.
- **Task 48 targeted verification**:
  - Unity script compilation completed successfully with zero warnings/errors.
  - Verified that all 120 EditMode tests and 8 PlayMode tests pass cleanly.
  - Verified scene setup regeneration idempotency: generated scenes twice and verified SHA-256 hashes of `Home.unity` and `Gameplay.unity` remain unchanged.
- **UI/UX design verification**:
  - Docs-only change; Unity validation not run - not relevant to design-only change.
  - Spec self-review completed for placeholders, contradictory requirements, scope drift, and implementation ambiguity.
  - Plan index updated with the new UI/UX design document.
- **AI image brief verification**:
  - Docs-only change; Unity validation not run - not relevant to companion brief change.
  - Confirmed the new file is a companion instruction document rather than a prompt pack, matching the requested scope.
- **Task 42 targeted verification**:
  - Unity script compilation completed with no compiler errors.
  - `SaveDataServiceTests`: Added `Load_DailyDropCounts_ResetsOnDateChange` verifying daily drop counts are cleared and LastSaveDateString is set on new date. Passed.
- **Task 41 targeted verification**:
  - Unity script compilation completed with no compiler errors.
  - `StaticDataServiceTests`: Verified drop table configurations and strict constraints (rates, IDs, references). Passed.
  - `JigsawVinaGameDataEditorTests`: Verified that configurations round-trip correctly. Passed.
- **Compiler Status**: Unity compiled successfully with no compiler errors. All 120 EditMode and 8 PlayMode tests passed cleanly.

## Current Uncommitted Scope

- Task 48 source code changes: refactored classes and new C# scripts (`StaticDataValidator`, `StaticDataCatalogBuilder`, `JigsawVinaGameDataBuilder`, `JigsawVinaGameDataBuildInput`, `ThinVerticalSliceUiFactory`, `ThinVerticalSliceHomeSceneBuilder`, `ThinVerticalSliceGameplaySceneBuilder`, `HomeFlowController`) and updated original files.
- Task 48 planning/tracker/handoff updates.
- UI/UX planning docs: mobile landscape design system and screen specification, plan index update, task tracker update, and current handoff update.
- AI image planning docs: companion image brief, plan index update, task tracker update, and current handoff update.

## Recommended Next Steps

- Review and commit the refactored code for Task 48 (including generated `.meta` files).
- Review the UI/UX design doc, then split approved UI work into focused implementation plans when ready.
- Use `2026-06-16-ui-ux-ai-image-brief.md` with the UI/UX design spec when asking an AI image assistant to produce prompt strategy, mood boards, or layered asset plans.
- Map approved Planned test cases to existing NUnit coverage, then add only missing targeted tests.
- Run the optional manual Home Collection and daily reward claim click-through before a user-requested commit.
