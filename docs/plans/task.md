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


## Completed

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

## Pending

- Manual full click-through from Home picture selection to Gameplay reward return can be run in the open Unity Editor if desired.
- Manual full-flow/device verification remains optional before delivery.
