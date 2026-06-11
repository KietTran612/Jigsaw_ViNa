# Piece Shuffle And Invalid Feedback Implementation Plan

**Goal:** Randomize tray piece order and clearly mark pieces released at an incorrect board position.

**Architecture:** The presenter shuffles tray sibling order after creating all pieces while preserving the index-to-view mapping. `PuzzlePieceView` owns its visual error state and shake animation; the presenter triggers that feedback only after a failed snap.

**Tech Stack:** Unity 6000.3.11f1, uGUI, coroutines.

## Tasks

- [x] Shuffle tray sibling order with Fisher-Yates and force a non-identity result.
- [x] Add a disabled red `Outline` to each piece.
- [x] On failed snap, enable the outline and run a short horizontal shake.
- [x] Keep the outline enabled after shaking; clear it when the next piece drag begins or the piece locks.
- [x] Update focused PlayMode regression coverage without running tests.
- [x] Wait for Unity compile/import and check compiler logs only.
