# Error Handling Test Cases

## TC-ERROR-001: Chặn chọn tranh đang khóa

- **Module:** Error Handling
- **Feature:** Locked Picture Guard
- **Case Status:** Active
- **Priority:** Critical
- **Test Suite:** Smoke
- **Test Level:** Integration
- **Automation Status:** Planned
- **Execution Mode:** EditMode
- **NUnit Test:** none

### Preconditions

1. Static data fixture có Picture ID `2` với `is_initially_unlocked = false`.
2. Picture ID `2` yêu cầu Item ID `101`.
3. Save không sở hữu Item ID `101` và chưa unlock Picture ID `2`.

### Test Data

| Field | Value |
|---|---|
| Locked Picture ID | `2` |
| Missing Key Item | Item ID `101` |

### Steps

| # | Action | Expected Result |
|---:|---|---|
| 1 | Refresh Picture Select với fixture state. | Picture ID `2` hiển thị `Locked`, có lock overlay và nút chọn không interactable. |
| 2 | Thử phát click chọn từ card Picture ID `2`. | Card không phát selection callback cho Picture ID `2`. |
| 3 | Kiểm tra Home screen state. | Picture Select vẫn hiển thị và Difficulty Select không được mở cho Picture ID `2`. |
| 4 | Kiểm tra session và save. | `SelectedPictureId` không đổi; `UnlockedPictureIds` và owned items không bị mutate. |

### Automation Notes

Planned EditMode integration test sẽ bind locked card, simulate click và kiểm tra callback, session cùng save đều không đổi.

## TC-ERROR-002: Chặn chọn difficulty chưa mở khóa

- **Module:** Error Handling
- **Feature:** Locked Difficulty Guard
- **Case Status:** Active
- **Priority:** Critical
- **Test Suite:** Smoke
- **Test Level:** Integration
- **Automation Status:** Planned
- **Execution Mode:** EditMode
- **NUnit Test:** none

### Preconditions

1. Picture ID `1` đã mở khóa và dùng policy `sequential`.
2. Save chưa có completion cho Difficulty ID `0`.
3. Difficulty ID `1` được cấu hình nhưng đang locked.

### Test Data

| Field | Value |
|---|---|
| Picture ID | `1` |
| Locked Difficulty ID | `1` |
| Previous selected difficulty | `0` |

### Steps

| # | Action | Expected Result |
|---:|---|---|
| 1 | Refresh Difficulty Select cho Picture ID `1`. | Difficulty ID `1` hiển thị lock state và không được coi là unlocked. |
| 2 | Phát yêu cầu chọn Difficulty ID `1`. | `DifficultySelectPresenter` kiểm tra progression và return mà không load scene. |
| 3 | Kiểm tra `GameSessionService`. | `SelectedDifficultyId` vẫn bằng giá trị trước thao tác. |
| 4 | Kiểm tra scene loader. | Không có request load scene `Gameplay`. |

### Automation Notes

Planned EditMode integration test sẽ dùng recording `SceneLoader` và kiểm tra locked selection bị guard trước session mutation.

## TC-ERROR-003: Unlock thất bại không thay đổi save

- **Module:** Error Handling
- **Feature:** Failed Unlock Atomicity
- **Case Status:** Active
- **Priority:** Critical
- **Test Suite:** Smoke
- **Test Level:** Unit
- **Automation Status:** Planned
- **Execution Mode:** EditMode
- **NUnit Test:** none

### Preconditions

1. Picture ID `2` yêu cầu Item ID `101`.
2. Save không sở hữu Item ID `101` và chưa unlock Picture ID `2`.
3. Mock save service ghi nhận mọi mutation và `Save` call.

### Test Data

| Field | Value |
|---|---|
| Picture ID | `2` |
| Missing requirement | Item ID `101` |
| Expected result | `UnlockResult.MissingRequirements` |

### Steps

| # | Action | Expected Result |
|---:|---|---|
| 1 | Chụp snapshot toàn bộ save trước unlock. | Snapshot ghi nhận các list và currency trước thao tác. |
| 2 | Gọi `ProgressionService.TryUnlockPicture(2)`. | Kết quả là `MissingRequirements`. |
| 3 | Kiểm tra persistence. | Save service không được gọi. |
| 4 | So sánh save với snapshot. | `UnlockedPictureIds`, `OwnedItemIds`, completion, inventory, currency và daily counters đều không đổi. |

### Automation Notes

Planned EditMode unit test sẽ dùng recording save service và deep state assertions để chứng minh failed unlock không có partial mutation.

## TC-ERROR-004: Lặp thao tác complete không cấp duplicate reward

- **Module:** Error Handling
- **Feature:** Duplicate Completion Guard
- **Case Status:** Active
- **Priority:** Critical
- **Test Suite:** Smoke
- **Test Level:** Integration
- **Automation Status:** Planned
- **Execution Mode:** PlayMode
- **NUnit Test:** none

### Preconditions

1. Save chưa có completion cho Picture ID `1`, Difficulty ID `0`.
2. Session hiện tại đã bắt đầu và `IsRewardProcessed = false`.
3. Recording save service và Reward Summary transition probe đều bắt đầu với count `0`.
4. Completion helper có thể được gọi hai lần cho cùng puzzle session để mô phỏng duplicate complete action.

### Test Data

| Field | Value |
|---|---|
| Picture ID | `1` |
| Difficulty ID | `0` |
| First-clear coins | `30` |
| First-clear Key Item | Item ID `107` |
| Duplicate complete actions | `2` |
| Expected save count | `1` |
| Expected Reward Summary transition count | `1` |

### Steps

| # | Action | Expected Result |
|---:|---|---|
| 1 | Gọi completion helper lần thứ nhất để complete toàn bộ piece. | `OnPuzzleCompleted` phát một lần; first-clear reward được persist; `IsRewardProcessed` trở thành `true`; Reward Summary transition bắt đầu một lần. |
| 2 | Gọi completion helper lần thứ hai trong cùng session. | Presenter completion guard không phát thêm `OnPuzzleCompleted`; không bắt đầu thêm reward sequence. |
| 3 | Kiểm tra save sau cả hai complete actions. | Recording save count bằng `1`; coins chỉ tăng `30`, Item ID `107` xuất hiện đúng một lần và chỉ có một completion Picture `1` / Difficulty `0`. |
| 4 | Kiểm tra Reward Summary transition probe. | Reward Summary được chuyển sang active đúng một lần; transition count bằng `1` và không có duplicate reward display sequence. |

### Automation Notes

Planned PlayMode integration test sẽ gọi completion helper hai lần, đồng thời đếm save calls, completion events và Reward Summary activation transitions.

## TC-ERROR-005: Static data không hợp lệ dừng load với lỗi rõ ràng

- **Module:** Error Handling
- **Feature:** Static Data Validation Failure
- **Case Status:** Active
- **Priority:** Critical
- **Test Suite:** Smoke
- **Test Level:** Integration
- **Automation Status:** Planned
- **Execution Mode:** EditMode
- **NUnit Test:** none

### Preconditions

1. `StaticDataService` được tạo với auto-load tắt để load fixture trực tiếp.
2. JSON fixture đáp ứng cú pháp JSON nhưng chứa duplicate Picture ID.

### Test Data

| Field | Value |
|---|---|
| Invalid field | Duplicate Picture ID |
| Duplicate value | `1` |
| Expected exception | `InvalidOperationException` |

### Steps

| # | Action | Expected Result |
|---:|---|---|
| 1 | Gọi `StaticDataService.LoadFromText(invalidJson)`. | Load dừng và ném `InvalidOperationException`. |
| 2 | Kiểm tra exception message. | Message nêu rõ `Duplicate Picture ID found: 1`. |
| 3 | Thử truy cập dữ liệu từ fixture lỗi. | Service không công bố một static-data state bị load một phần. |
| 4 | Load lại bằng fixture hợp lệ trên service sạch. | Static data hợp lệ load thành công, chứng minh lỗi được cô lập và có thể recovery bằng dữ liệu đúng. |

### Automation Notes

Planned EditMode integration test sẽ dùng malformed in-memory JSON, assert loại/message lỗi và xác nhận valid load vẫn hoạt động trên clean service.
