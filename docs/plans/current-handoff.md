# Current Handoff

## Latest Completed Work

- Added an original-image opacity slider with a `0-100%` range and `20%` default; the percentage label updates continuously.
- Replaced the previous preview toggle event with continuous opacity updates from `PuzzlePlayingView` to `PuzzleBoardView`.
- Regenerated `Gameplay.unity` with `SetupVersionMarker_v4`; Home remains on `v3` and was not regenerated.
- Fixed locked-piece placement by resetting piece anchors and pivot to board center before assigning board-local target positions. Runtime inspection showed the prior offset was exactly half the board size because pieces retained top-left tray anchors.
- Added Fisher-Yates tray shuffling with a guaranteed non-identity fallback.
- Added incorrect-drop feedback: red outline plus a short shake, with the red outline retained until the next drag or successful lock.
- Changed piece dragging to preserve the pointer offset in world space after reparenting into `DragContainer`, avoiding large jumps on scaled Canvas layouts.
- Kept the win animation behavior unchanged: completion fades the original image to `100%`.
- Strengthened `AGENTS.md` verification policy: target only the current behavior by test case/class/filter, and never fall back to a whole suite without explicit user approval.
- Completed and reviewed **Tasks 7-11** from the [2026-06-11-jigsaw-gameplay.md](2026-06-11-jigsaw-gameplay.md) implementation plan.
- Implemented **Puzzle Gameplay State Model**: created [PuzzleSession](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Core/Data/PuzzleSession.cs) to handle elapsed time, snapping margins, hint indexing prioritizing the last interacted piece, and completion state.
- Implemented **UI Visual & Drag-Drop Interaction**:
  - Created [PuzzlePieceView](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/PuzzlePieceView.cs) supporting pointer-offset dragging, tray-only vertical scrolling, and horizontal drag threshold gesture classification.
  - Created [PuzzleBoardView](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/PuzzleBoardView.cs) supporting async completed image fade-in and opacity settings.
- Wired up **Gameplay loop & Presenters**: modified [PuzzlePlayingPresenter](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Presentation/Screens/PuzzlePlayingPresenter.cs) to procedurally slice picture textures, position pieces, drive hint locks, and trigger win flows.
- Implemented **Idempotent Scene Setup**: updated [ThinVerticalSliceSceneSetup](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Scripts/Editor/ThinVerticalSliceSceneSetup.cs) to use `SetupVersionMarker_v3`, keep `DragContainer` as the top render sibling, and configure puzzle textures for runtime slicing.
- Integrated **Win Lifecycle and Transitions**: rewards are persisted immediately when completion is detected, before the presentation-only animation. `Debug Win` remains available in Editor/Development builds and is hidden in release builds.
- Replaced square puzzle images with true `1440x1080` 4:3 landscape textures.
- Split tests into `JigsawVina.Tests` EditMode and `JigsawVina.PlayModeTests` PlayMode assemblies so Unity discovers both suites correctly.

## Verification

- **Current gameplay fix tests**: not run - user explicitly stopped test execution.
- **Current gameplay fix compile/log check**: Unity compile/reload completed with no C# compiler errors.
- **Opacity slider scene wiring**: confirmed through Unity MCP (`min=0`, `max=1`, `value=0.2`, fill/handle assigned, and `PuzzlePlayingView` references wired).
- **Locked-piece runtime diagnosis**: confirmed `Piece_8` was offset from its expected target by `(-400, +300)`, exactly half of the `800x600` board; anchor normalization now handles snap, hint, and Debug Win paths.
- **Tray shuffle and invalid-drop feedback**: manually verified working by the user in Play Mode.
- **EditMode Test Suite**: `17 passed, 0 failed, 0 skipped`.
- **PlayMode Test Suite**: `6 passed, 0 failed, 0 skipped`.
- **Compiler Check**: Unity finished importing/compiling with no C# compiler errors.
- **Asset Check**: both gameplay textures are `1440x1080` with a 4:3 ratio.

## Known Warnings Or Blockers

- No known blocker for the latest shuffle/invalid-feedback change.
- Manual device/full mouse or touch click-through has not been run in this review.

## Delivery Scope

- Tasks 7-11 gameplay source, tests, generated scenes, puzzle textures, and planning/handoff documentation.
- Unrelated `Assets/Textures/` and `docs/Images/Picture_2..5/` content is outside this delivery scope.

## Recommended Next Task

- Perform the remaining full-flow/device verification before packaging a release build.
