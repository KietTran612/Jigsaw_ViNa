# Current Handoff

## Latest Completed Work

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

- **Task 27**: Documentation-only review; Unity compile and tests not run because no runtime or asset implementation changed.
- **Compiler Status**: Unity biên dịch thành công 100% không cảnh báo/lỗi.
- **Automated Tests**:
  - EditMode tests: 39/39 passed (bao gồm tất cả các kiểm tra logic, validator, cheat editor, và cấu trúc DTO).
  - PlayMode tests: 8/8 passed (bao gồm kiểm thử kéo thả, hoàn thành màn chơi, gợi ý, trả khay tranh và lưu dữ liệu).
- **Manual Verification**: Đã chạy phương thức `DebugSave()` từ xa qua MCP, tái tạo thành công file cấu hình JSON chính xác `jigsaw_vina_game_data.json` chứa đầy đủ các khóa ngôn ngữ hiển thị.

## Current Uncommitted Scope

- `JigsawVina/Assets/Scenes/Home.unity` contains a pre-existing unrelated Unity serialization change and is intentionally excluded from the Dynamic Home UI planning commit.

## Recommended Next Steps

- Wait for an explicit user request before starting Task 28. Tasks 28-34 must be implemented in order according to `docs/plans/2026-06-12-dynamic-home-ui-implementation.md`.
- Thực hiện commit toàn bộ thay đổi lên Git nhánh hiện tại nếu người dùng đồng ý.
