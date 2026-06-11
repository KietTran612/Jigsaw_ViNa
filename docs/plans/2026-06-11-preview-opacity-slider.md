# Preview Opacity Slider Implementation Plan

**Goal:** Keep the original puzzle image visible at 20% opacity and let the player adjust it from 0% to 100%.

**Architecture:** `PuzzlePlayingView` owns the uGUI slider and emits normalized opacity changes. `PuzzlePlayingPresenter` applies the value to `PuzzleBoardView`; the win animation remains authoritative and fades the completed image to 100%.

**Tech Stack:** Unity 6000.3.11f1, uGUI, VContainer.

## Tasks

- [x] Add a serialized `Slider` to `PuzzlePlayingView`, expose an opacity-change event, and initialize its value without emitting callbacks.
- [x] Replace the preview toggle behavior in `PuzzlePlayingPresenter` with continuous opacity updates and a default value of `0.2`.
- [x] Initialize `PuzzleBoardView` and generated Gameplay scene preview at `0.2` opacity.
- [x] Add and wire a `0..1` opacity slider in `ThinVerticalSliceSceneSetup`.
- [x] Update focused gameplay regression expectations and project handoff.
- [x] Wait for Unity compile/import and check compiler logs. Do not run tests unless explicitly requested.
