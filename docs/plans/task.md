| Implementation Task | Status | Notes |
| :--- | :---: | :--- |
| **Task 0: Configure Assembly Definitions** | [x] | Created JigsawVina, JigsawVina.Tests, and JigsawVina.Editor assembly definitions to define dependencies. |
| **Task 1: Core Data Models & Save System** | [x] | Created PlayerSave, immutable PictureConfig, ISaveDataService, SaveDataService, and SaveDataServiceTests. Merged PictureConfig into PlayerSave.cs to resolve compilation issues. |
| **Task 2: Global Services & Shared Session State** | [x] | Created IStaticDataService, StaticDataService, GameSessionService, SceneLoader, and GameSessionServiceTests. Removed stray unused StaticDataService field during review. |
| **Task 3: VContainer Project Scope** | [x] | Set up ProjectLifetimeScope for global VContainer dependency injection and registered save/static data/session/scene loader services. |
| **Task 4: Home Scene UI & Presenters** | [x] | Implemented PictureSelectView/Presenter, DifficultySelectView/Presenter, HomeLifetimeScope, and HomeFlowController. |
| **Task 5: Gameplay Scene UI, Progression Logic & Tests** | [x] | Implemented PuzzlePlayingView/Presenter, RewardSummaryView/Presenter, GameplayLifetimeScope, GameplayFlowController, and ProgressionTests. |
| **Task 6: Unity Scene Wire Up & Manual Run** | [x] | Set up VContainer settings, build settings, Home/Gameplay scene hierarchy, and Play Mode startup smoke checks. Full click-through remains manual. |
| **Task 7: Puzzle Session Data Model & Tests** | [x] | Create PuzzleSession and NUnit PuzzleSessionTests verifying snap, hint, and timer logic. |
| **Task 8: UI Visual Components & Asset Generation** | [x] | Added 1440x1080 gameplay textures plus tested tray/floating drag behavior. |
| **Task 9: Scene Layout & Editor Setup Update** | [x] | Added version 3 scene setup, texture importer configuration, and separate EditMode/PlayMode test assemblies. |
| **Task 10: Puzzle Playing Presenter & View wiring** | [x] | Added pointer-offset dragging, random hint fallback, dynamic board sizing, and debug-only Cheat Win. |
| **Task 11: Win and Reward Flow Integration** | [x] | Persist rewards before win animation and verify duplicate protection through PlayMode tests. |
| **Task 12: Original Image Opacity Control** | [x] | Replaced preview toggle with a 0-100% slider, defaulting to 20%, and regenerated Gameplay scene wiring. |
| **Task 13: Tray Shuffle & Invalid Drop Feedback** | [x] | Shuffled tray pieces and added red shake/outline feedback for incorrect placement; manually verified by user. |
| **Task 14: Extend Core Models & Setup DTOs** | [x] | Extend PlayerSave, PictureDifficultyConfig, GameSessionService, and create StaticDataDto. |
| **Task 15: Write Failing Unit & Integration Tests** | [x] | Create StaticDataServiceTests and extend ProgressionTests to verify JSON parsing, validations, and rewards. |
| **Task 16: Implement JSON loading, validation, and reward logic** | [x] | Implement StaticDataService, update IStaticDataService, and RewardSummaryPresenter to reward first-clear coins/hints/items. |
| **Task 17: Implement Game Data Editor Window** | [x] | Create JigsawVinaGameDataEditor window to scan folders, customize difficulty settings, pre-fill from disk, and write to JSON. |
| **Task 18: Integrate Presentation Asset Loading** | [x] | Update PuzzlePlayingPresenter to load board textures dynamically from configured static data paths. |
| **Task 19: Tab-bar selection & Collapsible difficulties in Game Data Editor** | [x] | Implement a two-tab view, collapsible foldouts, clickable images to ping files, Sprite Editor integration buttons, and bordered frame styling in JigsawVinaGameDataEditor.cs. |
| **Task 20: Bulk Add Pictures from Folder in Game Data Editor** | [x] | Support direct selection of multiple picture folders in the Project tab and Drag-and-Drop capability, auto-configuring assets and logging duplicate main texture names. |
| **Task 21: Category Management in Editor Window** | [x] | Added category management tab, category selection popup, DTO loading/saving, and category deletion safety check. |
| **Task 22: Global Item Database Editor in Editor Window** | [x] | Implemented global item tab, item creation/deletion, reserved items protection (coin/hint), and ID/id_string uniqueness validation. |
| **Task 23: Player Save & Cheat Editor in Editor Window** | [x] | Implemented Player Save loading/saving, coins/hints edit fields, idempotent Unlock All cheat, and targeted Save Reset button. |
| **Task 24: Automated Editor Tests for Extended Editor Tools** | [x] | Implemented 13 EditMode tests covering DTO field round-trips, category ID round-trips, difficulty settings hydration with missing assets, duplicate validations (IDs, id_strings, and key-item collisions), positive ID validations, key item limits, category deletion safety, unlock-all idempotency, PlayerPrefs save reset scope, and auto-repairing/seeding of reserved items. |
| **Task 25: Hiển thị hình ảnh trong dropdown Reward Key Item và ô xem trước** | [x] | Thêm ảnh vào dropdown và ô xem trước bên cạnh dropdown cấu hình độ khó. |
| **Task 26: Cấu hình Localization Keys & Sửa lỗi Review** | [x] | Thêm khóa ngôn ngữ cho Tranh/Danh mục/Key Items lên GUI, sửa validator runtime, dọn dẹp file dư thừa, và cập nhật/sửa các test case EditMode/PlayMode bị ảnh hưởng. |
| **Task 27: Final Review Dynamic Home UI Plans** | [x] | Reviewed design/implementation plans, tightened presenter disposal assertion, and expanded flow-controller disposal coverage for both picture selection and Back button events. |
| **Task 28: Create PictureSelectCard Component and Prefab** | [x] | Created PictureSelectCard.cs component and programmatically generated PictureSelectCard.prefab asset with required `.meta` files. |
| **Task 29: Refactor PictureSelectView for Dynamic Cards** | [x] | Replaced hardcoded button references with prefab/container references, added validation, implemented card instantiation and setup, and added safe Unbind plus card destruction. |
| **Task 30: Connect PictureSelectPresenter to Static Data** | [x] | Loaded picture configs through `IStaticDataService` and implemented `IDisposable` event cleanup. |
| **Task 31: Harden Home Flow and VContainer Lifecycle** | [x] | Registered lifecycle-aware components and replaced anonymous flow listeners with removable named handlers. |
| **Task 32: Regenerate Home Scene Setup v4** | [x] | Updated `ThinVerticalSliceSceneSetup`, replaced hardcoded buttons with a scroll view, wired prefab/container references, and regenerated `Home.unity`. |
| **Task 33: Add Targeted Home UI EditMode Tests** | [x] | Created `PictureSelectFlowTests` and added `HomeScene_PictureSelectView_IsWiredCorrectly` to `LifetimeScopeRegistrationTests` for full setup/wiring validation. |
| **Task 34: Verify and Update Dynamic Home UI Documentation** | [x] | Verified EditMode (45/45) and PlayMode (8/8) tests pass. Confirmed scene regeneration idempotency and updated documentation. |
| **Task 35: Data Model, Save Migration, Static Data Contract & Editor Integration** | [x] | Triển khai DTO, Config runtime, save migration, GUI Editor quản lý khóa tranh, nâng cấp cheat "Unlock All", tạo skeletons PictureCardPresentationModel.cs và ProgressionService.cs để tránh lỗi compile, sửa test fixtures cũ và cập nhật JSON Tranh 1-5. |
| **Task 36: Write Progression & Validator Unit Tests (TDD)** | [x] | Covers migration, atomic unlock persistence, difficulty policies, hints, deadlocks, active/non-consumable key items, duplicate requirements, invalid policies, and sequential gaps. |
| **Task 37: Implement Core Progression Service & Validator Logic** | [x] | Implemented progression logic and strict unlock validation; review fix now requires unlock Key Items to have `status == "active"`. |
| **Task 38: Update PictureSelect Card, Presenter & UI Prefab** | [x] | Added lock overlay, lock indicator, key item hint panel, Unlock button, locked navigation guard, source hints, atomic unlock refresh, lifecycle cleanup, and regenerated the prefab. |
| **Task 39: Update DifficultySelect View & Presenter with Leak Fix** | [x] | Khóa nút độ khó dựa trên policy, hiển thị thành tích, refresh đúng ID khi show, implement IDisposable hủy đăng ký event, và viết unit tests kiểm tra presenter/view logic và lifecycle. |
| **Task 40: Scene Wiring, Regeneration & Manual Verification** | [x] | Bổ sung Picture 6 khóa và cấu hình vào JSON, cập nhật ThinVerticalSliceSceneSetup.cs, chạy setup regenerate Home.unity, mở rộng LifetimeScopeRegistrationTests.cs để kiểm chứng scene wiring mới, chạy idempotency và test thủ công. |
| **Task 41: Editor Integration & Static Data Validation** | [x] | Preserve drop tables/items lists in editor configurations and validate drop configurations. |
| **Task 42: PlayerSave Structure, Save Migration & RNG/Service Contracts** | [x] | Add LastSaveDateString and DailyDropCounts to PlayerSave, implementing calendar daily reset and registering the new service contracts. |
| **Task 43: Daily Drop Decay Tests** | [x] | Added TDD coverage for independent rolls, decay/minimum clamp, inclusive amounts, exclusions, partial stacks, and daily counters. |
| **Task 44: Implement Daily Drop Reward Service** | [x] | Implemented drop rolling and replay reward application for coins, hints, key items, and clamped consumables. |
| **Task 45: Inventory / Collection UI** | [x] | Added Collection screen/presenter, owned key-item sources, quick navigation, lifecycle cleanup, Home scene v6 wiring, and targeted tests. |
| **Task 46: Markdown Living Test Plan & Manual Excel Exporter** | [x] | Added Living Test Plan, 31 Smoke/Critical QA test cases, validated Markdown parser, manual PowerShell/Node Excel exporter, and exporter regression tests. |
| **Task 47: Daily Login Reward System** | [x] | Configure daily rewards in static data, track login streak in PlayerSave, implement DailyRewardService, and add Popup UI to Home screen. |
| **Task 48: Long C# Files Refactor** | [x] | Extracted focused helpers from long project-owned C# files: StaticDataService validation/mapping, Game Data Editor build/validation, Thin Vertical Slice scene builders, and HomeFlowController file split. |
| **Task 49: Map Planned Test Cases to NUnit Coverage** | [x] | Mapped 25 of the 31 planned test cases to NUnit tests and updated metadata; 4 gameplay and 2 collection cases remain Planned due to lack of full PlayMode runtime E2E UI testing. Verified SaveDataServiceTests assertions pass. |
| **Task 50: Implement Game Settings, Audio & Localization Systems** | [x] | Implemented pause/settings popup flow, globally registered audio service, dictionary-based runtime localization, scene wiring, and review fixes. Verified Task 50 EditMode 17/17 and PlayMode 8/8 pass. |

## Completed

- Completed Task 50 game settings, pause flow, audio service, and custom localization system; fixed review issues around tests, Cheat Win wiring, popup CanvasGroup robustness, localization fallbacks, and I2 side effects.
- Mapped 25 of the 31 planned test cases to NUnit tests and updated metadata for Task 49, leaving 4 gameplay and 2 collection cases as Planned. Verified newly added assertions via targeted SaveDataServiceTests run.
- Designed the Markdown-first QA test case system for the full current game flow, including schema validation, manual Excel export, and later NUnit mapping.
- Implemented the Markdown-first QA test case system with six module files, 31 initial test cases, strict validation, and manually invoked Excel export tooling.
- Completed thin vertical slice Tasks 4-6: Home UI/presenters, Gameplay UI/presenters, reward progression tests, VContainer root settings, build settings, and generated Home/Gameplay scenes.
- Fixed `.gitignore` to support Unity subfolder `JigsawVina/` and ensure nested local build/temporary/MCP-tooling files are correctly ignored.
- Created the initial `docs/plans/` planning workflow files.
- Reviewed `AGENTS.md` setup after removing copied project references.
- Reviewed initial Jigsaw Việt Nam rules, data design, and sample JSON docs for project kickoff discussion.
- Captured confirmed kickoff decisions in `docs/plans/2026-06-10-jigsaw-vietnam-kickoff-decisions.md`.
- Updated rate item decay decision to count drops per item globally per local day.
- Synced `docs/jigsaw_vietnam_game_rules.md` and `docs/jigsaw_vietnam_data_design.md` with the kickoff decisions document.
- Synced sample static/save JSON files with the current schema and MVP decisions.
- Captured Unity framework decisions: Unity 6000.3.11f1, `JigsawVina/` subfolder, uGUI, one `Main` scene, UniTask, VContainer, and architecture rules.
- Completed, verified, and polished Daily Login Reward system (Task 47), including slot state overlays, thumbnail bindings, coin validations on compensation path, flow controller unsubscription test verification, fallback wrapping fixes, and empty asset path view tests.
- Created the Task 48 implementation plan for long C# file refactoring across project-owned runtime/editor files.
- Reviewed and fixed Task 48 plan after feedback: line counts are now marked as snapshot-only, namespace preservation is explicit, and stub-like plan snippets were removed.
- Final-reviewed Task 48 plan and clarified scene-builder path replacement, shared `CheckSceneAlreadyUpdated`, and prefab generator helper usage.
- Created the UI/UX design system and full-flow screen specification for mobile landscape 16:9-to-4:3, DOTween-first animation, and AI-assisted layered asset generation.
- Created the companion AI image brief explaining how an image assistant should interpret the UI/UX spec and produce mood boards, asset plans, and Unity-friendly layered image outputs.
- Added guidance for large full-screen mockup images used only for AI review/art-direction alignment, not Unity slicing or production asset extraction.

## Pending

- Review and later convert the approved UI/UX design system into scoped implementation plans when ready.
- Use the AI image brief together with the UI/UX design system when asking another AI to create image prompt strategy, mood boards, or layered asset plans.
- Use large mockups only for visual review and prompt iteration; generate separate layered assets for Unity production use.
- Manual full click-through from Home picture selection to Gameplay reward return can be run in the open Unity Editor if desired.
- Manual full-flow/device verification remains optional before delivery.
