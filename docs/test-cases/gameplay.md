# Gameplay Test Cases

## TC-GAMEPLAY-001: Mở Home và hiển thị danh sách tranh

- **Module:** Gameplay
- **Feature:** Home Picture Selection
- **Case Status:** Active
- **Priority:** Critical
- **Test Suite:** Smoke
- **Test Level:** End-to-End
- **Automation Status:** Planned
- **Execution Mode:** PlayMode
- **NUnit Test:** none

### Preconditions

1. Production static data được load trực tiếp từ `JigsawVina/Assets/Resources/GameData/jigsaw_vina_game_data.json`.
2. Scene `Home` có trong Build Settings và là scene khởi đầu của luồng kiểm thử.
3. Save hợp lệ hoặc save mặc định có thể được load.

### Test Data

| Field | Value |
|---|---|
| Production static data source | `JigsawVina/Assets/Resources/GameData/jigsaw_vina_game_data.json` |
| Configured Picture IDs | `1`, `2`, `3`, `4`, `5`, `6`, `7`, `8` |
| Expected picture count | `8` |

### Steps

| # | Action | Expected Result |
|---:|---|---|
| 1 | Khởi chạy game tại scene `Home`. | Scene `Home` load thành công, không có exception và màn hình Picture Select được active. |
| 2 | Chờ `HomeLifetimeScope` hoàn tất khởi tạo. | Difficulty Select và Collection không hiển thị; Picture Select vẫn hiển thị. |
| 3 | Quan sát danh sách card tranh. | Có đúng một card cho mỗi Picture ID `1-8`; không có card trùng ID. |
| 4 | Kiểm tra `display_name` và thumbnail của toàn bộ 8 card. | Mỗi card hiển thị đúng `display_name`; `Resources.Load<Sprite>(asset_path)` thành công cho mọi production `asset_path`, thumbnail sprite được gán và không card nào dùng placeholder/fallback. |

### Automation Notes

Planned PlayMode test sẽ load `Home`, đọc production static data source, chờ VContainer khởi tạo và fail nếu bất kỳ configured thumbnail nào không load thành `Sprite`.

## TC-GAMEPLAY-002: Chọn tranh đã mở khóa và mở Difficulty Select

- **Module:** Gameplay
- **Feature:** Home Navigation
- **Case Status:** Active
- **Priority:** Critical
- **Test Suite:** Smoke
- **Test Level:** End-to-End
- **Automation Status:** Planned
- **Execution Mode:** PlayMode
- **NUnit Test:** none

### Preconditions

1. Đang ở Picture Select trong scene `Home`.
2. Picture ID `1` có `is_initially_unlocked = true`.
3. Save chưa có completion cho Picture ID `1`.

### Test Data

| Field | Value |
|---|---|
| Picture ID | `1` |
| Difficulty unlock policy | `sequential` |

### Steps

| # | Action | Expected Result |
|---:|---|---|
| 1 | Tìm card của Picture ID `1`. | Card tồn tại, không có lock overlay và nút chọn có thể tương tác. |
| 2 | Nhấn card Picture ID `1`. | Picture ID `1` được ghi vào `GameSessionService.SelectedPictureId`. |
| 3 | Chờ Home flow xử lý sự kiện chọn tranh. | Picture Select được ẩn và Difficulty Select được hiển thị. |
| 4 | Quan sát danh sách difficulty. | Difficulty ID `0` hiển thị ở trạng thái mở khóa; các difficulty được cấu hình khác hiển thị đúng trạng thái theo save. |

### Automation Notes

Planned PlayMode test sẽ click card thật và kiểm tra screen state, selected session picture cùng trạng thái difficulty.

## TC-GAMEPLAY-003: Chọn độ khó đã mở khóa và tải Gameplay

- **Module:** Gameplay
- **Feature:** Difficulty Selection
- **Case Status:** Active
- **Priority:** Critical
- **Test Suite:** Smoke
- **Test Level:** End-to-End
- **Automation Status:** Planned
- **Execution Mode:** PlayMode
- **NUnit Test:** none

### Preconditions

1. Picture ID `1` đã được chọn và Difficulty Select đang hiển thị.
2. Picture ID `1` dùng policy `sequential`.
3. Difficulty ID `0` được cấu hình và đang mở khóa.

### Test Data

| Field | Value |
|---|---|
| Picture ID | `1` |
| Difficulty ID | `0` |
| Target scene | `Gameplay` |

### Steps

| # | Action | Expected Result |
|---:|---|---|
| 1 | Kiểm tra nút Difficulty ID `0`. | Nút tồn tại, hiển thị difficulty đã cấu hình và có thể tương tác. |
| 2 | Nhấn Difficulty ID `0`. | `GameSessionService.SelectedDifficultyId` trở thành `0`. |
| 3 | Chờ scene transition hoàn tất. | Scene `Gameplay` được load đúng một lần và không có exception. |
| 4 | Quan sát màn hình đầu Gameplay. | Puzzle Playing hiển thị; Reward Summary chưa hiển thị. |

### Automation Notes

Planned PlayMode test sẽ điều khiển Difficulty Select, chờ scene `Gameplay` và xác nhận session state trước khi puzzle khởi tạo.

## TC-GAMEPLAY-004: Tạo đúng số puzzle pieces theo cấu hình

- **Module:** Gameplay
- **Feature:** Puzzle Initialization
- **Case Status:** Active
- **Priority:** Critical
- **Test Suite:** Smoke
- **Test Level:** Integration
- **Automation Status:** Planned
- **Execution Mode:** PlayMode
- **NUnit Test:** none

### Preconditions

1. `GameSessionService` đã chọn Picture ID `1`, Difficulty ID `0`.
2. Scene `Gameplay` và các view đã được wire hợp lệ.

### Test Data

| Field | Value |
|---|---|
| Picture ID | `1` |
| Difficulty ID | `0` |
| Grid | `6 x 4` |
| Expected piece count | `24` |

### Steps

| # | Action | Expected Result |
|---:|---|---|
| 1 | Khởi tạo `PuzzlePlayingPresenter`. | `PuzzleSession` được tạo với `Columns = 6` và `Rows = 4`. |
| 2 | Chờ quá trình tạo piece hoàn tất. | Có đúng `24` `PuzzlePieceView` được tạo. |
| 3 | Kiểm tra index của các piece. | Các index liên tục từ `0` đến `23`, không trùng và không thiếu. |
| 4 | Kiểm tra trạng thái ban đầu. | Tất cả piece ở trạng thái `Tray`, puzzle chưa completed và timer bắt đầu từ `0`. |

### Automation Notes

Planned PlayMode integration test sẽ dùng cấu hình production của Picture `1` / Difficulty `0` và kiểm tra session cùng hierarchy được tạo.

## TC-GAMEPLAY-005: Kéo và snap piece vào đúng vị trí

- **Module:** Gameplay
- **Feature:** Puzzle Piece Interaction
- **Case Status:** Active
- **Priority:** Critical
- **Test Suite:** Smoke
- **Test Level:** End-to-End
- **Automation Status:** Planned
- **Execution Mode:** PlayMode
- **NUnit Test:** none

### Preconditions

1. Puzzle Picture ID `1`, Difficulty ID `0` đang được chơi.
2. Piece index `0` chưa locked và đang ở Tray.
3. Board đã có kích thước hợp lệ.

### Test Data

| Field | Value |
|---|---|
| Piece index | `0` |
| Snap threshold | `50` local units |

### Steps

| # | Action | Expected Result |
|---:|---|---|
| 1 | Pointer down trên piece index `0`. | Piece index `0` trở thành last interacted piece. |
| 2 | Bắt đầu kéo piece khỏi Tray. | Piece chuyển sang trạng thái `Floating`, nằm trong drag container và có kích thước bằng board cell. |
| 3 | Kéo tâm piece vào vị trí target của index `0`, trong snap threshold. | Piece di chuyển theo pointer và vẫn giữ đúng pointer offset. |
| 4 | Thả pointer. | Snap trả về thành công; piece chuyển sang `Locked`, được đặt đúng target trong locked-pieces container. |
| 5 | Thử kéo lại piece đã locked. | Piece không rời vị trí đã snap và trạng thái vẫn là `Locked`. |

### Automation Notes

Planned PlayMode test sẽ phát pointer events hoặc gọi interaction boundary tương đương, rồi kiểm tra state và transform sau snap.

## TC-GAMEPLAY-006: Hoàn thành puzzle và mở Reward Summary

- **Module:** Gameplay
- **Feature:** Puzzle Completion
- **Case Status:** Active
- **Priority:** Critical
- **Test Suite:** Smoke
- **Test Level:** End-to-End
- **Automation Status:** Planned
- **Execution Mode:** PlayMode
- **NUnit Test:** none

### Preconditions

1. Puzzle Picture ID `1`, Difficulty ID `0` đang active.
2. Reward cho session hiện tại chưa được process.
3. Có thể hoàn thành tất cả `24` piece bằng thao tác hợp lệ hoặc test helper.

### Test Data

| Field | Value |
|---|---|
| Picture ID | `1` |
| Difficulty ID | `0` |
| Piece count | `24` |

### Steps

| # | Action | Expected Result |
|---:|---|---|
| 1 | Snap lần lượt `23` piece đầu vào đúng vị trí. | Mỗi piece được locked; puzzle vẫn chưa completed khi còn một piece chưa locked. |
| 2 | Snap piece cuối cùng vào đúng vị trí. | `PuzzleSession.IsCompleted` trở thành `true` và completion event phát đúng một lần. |
| 3 | Chờ win animation và delay kết thúc. | Input puzzle bị disable; Puzzle Playing được ẩn sau sequence. |
| 4 | Quan sát màn hình kế tiếp. | Reward Summary được active và hiển thị stars, coins cùng item label nếu có. |

### Automation Notes

Planned PlayMode test sẽ dùng deterministic completion helper, đếm completion event và chờ `ShowRewardSequence` hoàn tất.

## TC-GAMEPLAY-007: Nhận reward và quay về Home

- **Module:** Gameplay
- **Feature:** Reward Return Flow
- **Case Status:** Active
- **Priority:** Critical
- **Test Suite:** Smoke
- **Test Level:** End-to-End
- **Automation Status:** Planned
- **Execution Mode:** PlayMode
- **NUnit Test:** none

### Preconditions

1. Save chưa có completion cho Picture ID `1`, Difficulty ID `0`.
2. Puzzle Picture ID `1`, Difficulty ID `0` vừa hoàn thành.
3. Reward Summary đang hiển thị.

### Test Data

| Field | Value |
|---|---|
| Picture ID | `1` |
| Difficulty ID | `0` |
| First-clear coins | `30` |
| First-clear Key Item | Item ID `107` |
| Star reward | `1` |

### Steps

| # | Action | Expected Result |
|---:|---|---|
| 1 | Kiểm tra Reward Summary sau first clear. | Summary hiển thị `Stars: 1`, `Coins Earned: 30` và tên Key Item ID `107`. |
| 2 | Load save đã được reward flow ghi. | Coins tăng đúng `30`; Item ID `107` có trong `OwnedItemIds`; có đúng một completion cho Picture `1` / Difficulty `0`. |
| 3 | Nhấn nút Return trên Reward Summary. | `PuzzlePlayingPresenter.Cleanup()` gỡ đúng các subscription `OnHintClicked -> ApplyHint`, `OnReturnToTrayClicked -> ReturnAllFloatingToTray`, và `OnPreviewOpacityChanged -> SetPreviewOpacity`; scene loader nhận đúng một request load `Home`. |
| 4 | Chờ scene transition hoàn tất rồi phát lại ba view events trên presenter đã cleanup bằng test probe. | Scene `Home` hiển thị lại Picture Select, progression vừa lưu vẫn còn; các event probe không consume hint, không di chuyển piece và không thay đổi preview opacity qua presenter cũ. |

### Automation Notes

Planned PlayMode end-to-end test sẽ dùng save sạch, hoàn thành puzzle, xác nhận first-clear reward và click Return về `Home`.
