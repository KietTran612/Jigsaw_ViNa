# Design Document: Jigsaw Vina Data Editor & Runtime Pipeline

**Date:** 2026-06-11  
**Status:** Approved  
**Topic:** Unity Editor configurations for 5 pictures, key items, and the runtime JSON static data pipeline.

---

## 1. Overview

This design outlines the Unity Editor interface and runtime architecture to replace hardcoded configurations with a dynamic, backend-friendly JSON static data pipeline.

Instead of hardcoding picture details and difficulty grids, the editor allows designers to point to folders inside `Assets/Resources/` containing a main picture (prefixed with `MAIN_`) and its associated key items. It generates a single `StaticData.json` configuration file, which the game parses and validates at startup.

---

## 2. Static Data Schema Design (`StaticData.json`)

The config file is saved at `Assets/Resources/StaticData.json`. It uses `snake_case` properties to remain directly compatible with future HTTP backend payloads.

### JSON Schema

```json
{
  "schema_version": 1,
  "data_version": 1,
  "categories": [
    {
      "id": 1,
      "id_string": "vietnam_landscapes",
      "display_name": "Phong Cảnh Việt Nam"
    }
  ],
  "pictures": [
    {
      "id": 1,
      "id_string": "house_old_village",
      "display_name": "Nhà Cổ Làng Quê",
      "category_id": 1,
      "asset_path": "Textures/Picture_1/MAIN_House_OldVillage_1",
      "difficulty_unlock_policy": "sequential"
    }
  ],
  "items": [
    {
      "id": 101,
      "id_string": "banana_tree",
      "display_name": "Cây Chuối",
      "item_type": "key_item",
      "asset_path": "Textures/Picture_1/BananaTree_1"
    }
  ],
  "picture_difficulties": [
    {
      "picture_id": 1,
      "difficulty_id": 0,
      "display_name": "Dễ",
      "grid_columns": 6,
      "grid_rows": 4,
      "piece_count": 24,
      "star_reward": 1,
      "first_clear_coin": 30,
      "first_clear_hint": 0,
      "replay_coin": 10,
      "first_clear_reward_item_ids": [101]
    }
  ]
}
```

---

## 3. Unity Editor Window Interface (`JigsawVinaGameDataEditor.cs`)

The editor window is registered in the Unity menu under `JigsawVina/Game Data Editor`.

### Layout & Features
The window displays **5 Tabs**, one for each picture folder manually placed in `Assets/Resources/Textures/Picture_X/`.

Inside each Tab, the window splits the layout horizontally into two columns:

```text
+-----------------------------------------------------------------------------------+
|  [Tab: Picture 1]  [Tab: Picture 2]  [Tab: Picture 3]  [Tab: Picture 4]  [Tab: 5] |
+---------------------------------------+-------------------------------------------+
| LEFT PANEL: Assets & Metadata         | RIGHT PANEL: Difficulties & Rewards       |
|                                       |                                           |
| Source Folder: [Assets/Resources/...] | EASY (Difficulty 0)                       |
|                                       | - Grid Columns/Rows: [6] x [4]            |
| MAIN PICTURE (Top)                    | - First Clear Coin/Hint/Replay: [30]/[0]/[10]|
| - Thumbnail Preview                   | - Reward Key Item: [Dropdown: Cây Chuối]  |
| - ID String: [house_old_village]      |                                           |
| - Display Name: [Nhà Cổ Làng Quê]     | NORMAL (Difficulty 1)                     |
|                                       | - Grid Columns/Rows: [8] x [6]            |
| KEY ITEMS (Bottom)                    | - First Clear Coin/Hint/Replay: [60]/[0]/[20]|
| - Item 1: Thumbnail, ID, Display Name | - Reward Key Item: [Dropdown: Xe Đạp]     |
| - Item 2: Thumbnail, ID, Display Name |                                           |
|                                       | HARD (Difficulty 2)                       |
|                                       | - Grid Columns/Rows: [12] x [8]           |
|                                       | - First Clear Coin/Hint/Replay:[120]/[0]/[40]|
|                                       | - Reward Key Item: [Dropdown: Chum Nước]  |
+---------------------------------------+-------------------------------------------+
| [Save & Generate StaticData.json]                                                 |
+-----------------------------------------------------------------------------------+
```

### Save Action:
1. Strips `Assets/Resources/` and file extensions from image assets to build runtime paths (e.g. `Textures/Picture_1/MAIN_House_OldVillage_1`).
2. Checks for ID and name uniqueness across all tabs (pictures and key items).
3. Automatically maps selected reward dropdown items to their respective `first_clear_reward_item_ids`.
4. Writes the serialized JSON payload to `Assets/Resources/StaticData.json`.

---

## 4. Runtime Pipeline Integration

### DTO Layer
We define direct mapping classes matching the JSON schema:
- `StaticDataDto`
- `CategoryDto`
- `PictureDto`
- `ItemDto`
- `PictureDifficultyDto`

### Player Save Extension
The `PlayerSave` model is extended with `OwnedItemIds` to track key items earned on first clear.

### Validation Layer
During initialization, the DTO is validated for integrity:
- Checks for duplicate picture IDs or item IDs.
- Checks for duplicate `id_string` values across all pictures and items to ensure backend-friendly identifiers are unique.
- Validates that every difficulty's `first_clear_reward_item_ids` refers to an existing item ID.
- Validates that grid dimensions match piece counts (`columns * rows == piece_count`).
- Asserts that all texture paths are resolvable via `Resources.Load`.

### Repository Layer
Converts clean DTOs into immutable runtime models (`PictureConfig`, `PictureDifficultyConfig`, etc.) and indexes them in dictionary lookups for fast retrieval.

---

## 5. Testing & Verification

1. **Editor Functionality**: Verify that changing paths, IDs, names, and dropdown items correctly updates `StaticData.json` on save.
2. **Validator Validation**: Test that invalid JSON (e.g. missing items, duplicate IDs) triggers clean compiler warnings and initialization failures.
3. **Gameplay Regression**: Ensure `PuzzlePlayingPresenter` load logic retrieves details via the new `StaticDataService` instead of hardcoded arrays.
4. **Progression Rewards**: Ensure that completing a difficulty level for the first time rewards the player with the configured coins, hints, and key items, while subsequent completions reward the replay coins.
