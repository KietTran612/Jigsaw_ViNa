# Jigsaw Viet Nam Kickoff Decisions

Date: 2026-06-10

This document records the project decisions confirmed during kickoff discussion. It is the working memory for future planning and should be updated when decisions change.

## Source Documents

- `docs/jigsaw_vietnam_game_rules.md` is the original game concept/rules document.
- `docs/jigsaw_vietnam_data_design.md` is the source of truth for data/schema direction.
- The rules document may be edited later to align with the data design.

## Project Stage

- The workspace is currently in docs/data design stage.
- Framework/runtime choice is Unity.
- Unity version: `6000.3.11f1`.
- Unity project will live in the `JigsawVina/` subfolder, not at repository root.
- No Unity project folders are currently present in this workspace.

Target structure:

```text
Jigsaw_ViNa/
  AGENTS.md
  docs/
  JigsawVina/
    Assets/
    Packages/
    ProjectSettings/
```

## MVP Direction

- Build a Jigsaw puzzle game about Viet Nam.
- MVP should prove the puzzle loop plus basic progression/reward/save loop.
- The current docs are stronger on meta progression than on core jigsaw gameplay; core jigsaw details are being defined in this document.

## Data Direction

- Static data and player save data stay separate.
- Static data should follow `docs/jigsaw_vietnam_data_design.md`.
- Use `id` numeric references for runtime/save links.
- Keep `id_string` for editor/debug/readable references.
- Do not use `name` as a schema field. Use `display_name`.
- Editor/static data defines picture, difficulty, reward, unlock, item, drop, and asset settings.
- Runtime should generate puzzle pieces from image and grid config. Do not hand-author every piece in static data.

## Unity Framework Decisions

- Use Unity `6000.3.11f1`.
- Create the Unity project under `JigsawVina/`.
- Use one scene for MVP: `Main`.
- `Main` should contain an internal state/view controller rather than multiple scenes.
- MVP app states:
  - `PictureSelect`
  - `DifficultySelect`
  - `PuzzlePlaying`
  - `RewardSummary`
- Use Unity UI/uGUI for MVP runtime UI.
- Do not use UI Toolkit for MVP runtime UI.
- Puzzle board, slots, pieces, tray, top bar, and dialogs should be built with uGUI components such as `Canvas`, `RectTransform`, `Image`, `TextMeshProUGUI`, `Button`, `ScrollRect`, and layout groups.
- Use Unity EventSystem drag interfaces for puzzle input:
  - `IPointerDownHandler`
  - `IBeginDragHandler`
  - `IDragHandler`
  - `IEndDragHandler`
- Use hybrid architecture:
  - Core data/progression/save/reward/puzzle logic lives in services/models.
  - UI and input live in `MonoBehaviour` views/controllers.
- Use UniTask for async flows. Do not use Unity Coroutines for core logic.
- Use VContainer for dependency injection and composition root.
- Do not use global singleton services.
- Follow Dependency Inversion: core services depend on abstractions, and implementations are bound through VContainer.
- Do not add UniTask or VContainer Git URLs to `Packages/manifest.json` automatically. The user will add these packages manually through Unity Package Manager.

Service/data type rules:

- Use classes for long-lived core services.
- Use structs for small DTOs, value objects, event packets, and runtime state when they are copy-safe.
- Prefer `readonly struct` for immutable value objects.
- For structs, order fields from larger to smaller where practical.
- Do not use `[StructLayout(Pack = 1)]` by default.
- Use `StructLayout`, `Pack = 1`, or explicit layout only when binary layout is required, interop/fixed serialization needs it, or profiling proves a benefit.
- Use `byte` instead of `bool` when byte-exact packed layout is required.

## Puzzle Image And Pieces

- MVP image aspect ratio: `4:3`.
- Piece shape: rectangle grid.
- No real jigsaw knob/socket shape in MVP.
- Visual polish may include borders, shadows, spacing, snap animation, and preview overlay.
- Future data should allow other aspect ratios and shape types.

MVP difficulty grids:

| Difficulty | Grid | Piece Count |
|---|---:|---:|
| Easy | 6 x 4 | 24 |
| Normal | 8 x 6 | 48 |
| Hard | 12 x 8 | 96 |

Recommended config fields:

```json
{
  "aspect_ratio": "4:3",
  "grid_columns": 6,
  "grid_rows": 4,
  "piece_count": 24,
  "piece_shape_type": "rectangle",
  "allow_rotation": false
}
```

Validator should check `piece_count == grid_columns * grid_rows`.

## Orientation And Puzzle Layout

- MVP is landscape-first.
- Portrait support is deferred.
- Puzzle screen layout:
  - board on the left/center;
  - piece tray on the right;
  - top bar for back, picture name, timer, hint, and preview.
- Board should occupy roughly 70-75% of horizontal space.
- Tray should occupy roughly 25-30% of horizontal space.
- Tray displays pieces in a vertical scrolling grid.
- Tray should be designed so filter/sort can be added later.

## Piece Interaction

- Pieces do not rotate in MVP.
- Pieces start in the tray.
- Player drags pieces from tray into the play area.
- If released near the correct slot, the piece snaps and locks.
- If released in the wrong position, the piece remains freely placed in the play area.
- A `Return to Tray` control should move all non-locked floating pieces back to the tray.
- Locked pieces are not affected by `Return to Tray`.
- Future undo/reset puzzle can be added later.
- MVP completion condition: all pieces are locked in their correct slots.

Important runtime states:

- `tray`: piece has not been placed in the play area or was returned to tray.
- `floating`: piece is in the play area but not correctly snapped.
- `locked`: piece is correctly snapped and cannot be moved.

## In-Progress Puzzle Save

- MVP does not save in-progress puzzle sessions.
- If the player exits mid-puzzle, the session is discarded.
- Save updates only after puzzle completion.
- Architecture should keep runtime `PuzzleSessionState` separate from persistent `PlayerSave` so in-progress save can be added later.

## Timer And Stars

- MVP has a timer.
- Timer starts when the puzzle screen is ready.
- Timer stops when all pieces are locked.
- Save `best_time_seconds` per `picture_id + difficulty_id`.
- Timer does not affect star, reward, or drop rate.

Star decision:

- Star is achievement/progress score, not currency.
- Store `best_star` per `picture_id + difficulty_id`.
- Total star is computed from saved `best_star` values.
- Star is not stored in inventory.
- Star is not consumed.
- If unlock by star is needed later, use a requirement such as `total_star_at_least`; do not consume star.

MVP star reward:

| Difficulty | Star |
|---|---:|
| Easy | 1 |
| Normal | 2 |
| Hard | 3 |

## Coin And Hint

- Coin is a currency item.
- Hint is a currency/support resource item.
- Star is not a currency item.
- MVP uses one generic `hint` resource.
- Schema should be able to support multiple hint types later.

Hint behavior:

- MVP hint auto-places one piece.
- Hint prioritizes the currently selected or most recently interacted piece.
- If that piece is not valid, hint chooses a random unlocked non-locked piece.
- Hint costs 1 `hint`.
- Do not spend hint if there is no eligible piece.
- Hint does not affect star or reward in MVP.

## Difficulty Unlock Policy

MVP uses a hybrid policy:

- First 5 pictures are unlocked at game start.
- First 5 pictures use sequential difficulty unlock:
  - Easy starts unlocked.
  - Completing Easy unlocks Normal automatically.
  - Completing Normal unlocks Hard automatically.
- Pictures after the first 5 start locked.
- Later pictures unlock through key item requirements.
- When a later picture is unlocked by the player, all 3 difficulties open at once.

Suggested policy values:

```json
{
  "difficulty_unlock_policy": "sequential"
}
```

```json
{
  "difficulty_unlock_policy": "all_when_picture_unlocked"
}
```

Do not hard-code "first 5" in gameplay logic. Static data should mark those pictures by sort/order/policy.

## Rewards And Key Items

- Editor/static data sets which first-clear key items each `picture + difficulty` drops.
- A `picture + difficulty` may drop one or multiple first-clear key items.
- Use a list such as `first_clear_reward_item_ids`.
- First-clear key items drop once per `picture + difficulty`.
- Key items are permanent and are not consumed for main progression.
- If the player already owns a permanent key item, do not duplicate it.

## Unlock Requirements

- MVP unlock requirements use AND-only logic.
- A locked picture unlocks only when all active requirements are satisfied.
- Main progression requirements check permanent key item ownership.
- Later bonus/event content may support consumable requirements, but MVP main progression does not consume key items.
- Data can represent AND by multiple active `unlock_requirements` records for the same target.
- Validator must check missing item references and deadlock progression.

Unlock UX:

- Pictures after the first 5 do not unlock automatically.
- When requirements are met, the picture card shows `Ready to Unlock`.
- Player must press `Unlock`.
- Unlock re-checks requirements before applying.
- Main progression unlock does not consume key items.
- Reward summary may list pictures that are ready to unlock, but actual unlock happens in the picture select screen.

## Rate Item Decay

- MVP has no backend.
- Decay reset uses local date for now.
- This must be revisited if backend/server time is introduced.
- Drop count scope is per item globally for the current local day.
- If item `postcard_stamp` drops from any picture/drop table, it increments the same daily count for `postcard_stamp`.
- Save state should key decay count by `item_id`, not by `drop_table_id + item_id`.
- This gives stronger control over event/limited item farming across the whole game.

## Completion And Reward Flow

Completion flow:

1. All pieces become locked.
2. Puzzle input is disabled.
3. Show completed-picture animation.
4. Show reward summary.
5. Persist progress/rewards safely around summary display.
6. Update inventory, best time, best star, first-clear state, and unlock readiness.

Reward summary should show:

- rewards received this run;
- key items received;
- coin/hint/rate items received;
- suggestions for next steps, such as pictures now ready to unlock.

Reward summary actions:

- `Chon tranh`: return to picture select.
- `Choi lai`: restart the same picture and difficulty.

## Picture Select UI

- MVP picture select shows a grid of all pictures.
- Data still keeps category/chapter fields for future filtering.
- Picture card shows:
  - thumbnail;
  - display name;
  - locked/unlocked/ready-to-unlock state;
  - completed state;
  - missing requirements or unlock button when relevant.
- Clicking an unlocked picture opens a difficulty panel.

Difficulty panel:

- Use 3 difficulty cards instead of tabs for MVP.
- Each card shows:
  - Easy/Normal/Hard;
  - piece count;
  - locked/unlocked/completed state;
  - fixed star reward;
  - best time if completed;
  - first-clear key item reward;
  - replay/drop reward if needed.

## Docs Sync Status

- `docs/jigsaw_vietnam_game_rules.md` has been updated to match these kickoff decisions.
- `docs/jigsaw_vietnam_data_design.md` has been updated to match these kickoff decisions.
- If this decision document changes again, sync both docs immediately to avoid implementation conflicts.

## Open Topics

- Exact first MVP implementation scope.
- Exact list of the first 20 pictures and first 5 unlocked pictures.
- Whether picture categories are enough or if chapters should be first-class in MVP.
