| Implementation Task | Status | Notes |
| :--- | :---: | :--- |
| **Task 0: Configure Assembly Definitions** | [x] | Created JigsawVina, JigsawVina.Tests, and JigsawVina.Editor assembly definitions to define dependencies. |
| **Task 1: Core Data Models & Save System** | [x] | Created PlayerSave, immutable PictureConfig, ISaveDataService, SaveDataService, and SaveDataServiceTests. Merged PictureConfig into PlayerSave.cs to resolve compilation issues. |
| **Task 2: Global Services & Shared Session State** | [x] | Created IStaticDataService, StaticDataService, GameSessionService, SceneLoader, and GameSessionServiceTests. Removed stray unused StaticDataService field during review. |
| **Task 3: VContainer Project Scope** | [x] | Set up ProjectLifetimeScope for global VContainer dependency injection and registered save/static data/session/scene loader services. |
| **Task 4: Home Scene UI & Presenters** | [ ] | Implement PictureSelectView/Presenter, DifficultySelectView/Presenter, HomeLifetimeScope, and HomeFlowController. |
| **Task 5: Gameplay Scene UI, Progression Logic & Tests** | [ ] | Implement PuzzlePlayingView/Presenter, RewardSummaryView/Presenter, GameplayLifetimeScope, GameplayFlowController, and ProgressionTests. |
| **Task 6: Unity Scene Wire Up & Manual Run** | [ ] | Setup VContainer settings, build settings, scenes hierarchy, and verify the full vertical slice flow in Play Mode. |

## Completed

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

- Add task-specific entries as new project work starts.
