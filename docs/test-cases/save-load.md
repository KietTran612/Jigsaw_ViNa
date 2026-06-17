# Save Load Test Cases

## TC-SAVE-001: Tạo save mặc định khi chưa có dữ liệu

- **Module:** Save Load
- **Feature:** Default Save Creation
- **Case Status:** Active
- **Priority:** Critical
- **Test Suite:** Smoke
- **Test Level:** Integration
- **Automation Status:** Automated
- **Execution Mode:** EditMode
- **NUnit Test:** JigsawVina.Tests.SaveDataServiceTests.Load_WhenNoSaveExists_ReturnsDefaultSave

### Preconditions

1. `PlayerPrefs` không có key `JigsawVina_PlayerSave`.
2. `ILocalDateProvider` trả về ngày cố định `2026-06-15`.

### Test Data

| Field | Value |
|---|---|
| Save key | `JigsawVina_PlayerSave` |
| Local date | `2026-06-15` |

### Steps

| # | Action | Expected Result |
|---:|---|---|
| 1 | Tạo `SaveDataService` với date provider cố định. | Service được tạo mà không cần dữ liệu save có sẵn. |
| 2 | Gọi `Load()`. | Một `PlayerSave` mới được trả về, không null và không có exception. |
| 3 | Kiểm tra currency và collection. | `Coins = 0`, `Hints = 0`; `CompletedPuzzles`, `OwnedItemIds`, `UnlockedPictureIds`, `DailyDropCounts` và `Inventory` đều là list rỗng không null. |
| 4 | Kiểm tra ngày save. | `LastSaveDateString` bằng `2026-06-15`. |

### Automation Notes

Automated via JigsawVina.Tests.SaveDataServiceTests.Load_WhenNoSaveExists_ReturnsDefaultSave.

## TC-SAVE-002: Lưu và tải coins, hints, inventory

- **Module:** Save Load
- **Feature:** Resource Persistence
- **Case Status:** Active
- **Priority:** Critical
- **Test Suite:** Smoke
- **Test Level:** Integration
- **Automation Status:** Automated
- **Execution Mode:** EditMode
- **NUnit Test:** JigsawVina.Tests.SaveDataServiceTests.SaveAndLoad_SavesCorrectData

### Preconditions

1. `PlayerPrefs` key `JigsawVina_PlayerSave` đã được xóa.
2. Local date không đổi trong suốt test.

### Test Data

| Field | Value |
|---|---|
| Coins | `125` |
| Hints | `4` |
| Owned Key Item | Item ID `101` |
| Inventory item | Item ID `10`, Amount `5` |

### Steps

| # | Action | Expected Result |
|---:|---|---|
| 1 | Tạo `PlayerSave` với coins, hints, `OwnedItemIds` và `Inventory` theo Test Data. | Object save chứa đúng các giá trị trước khi ghi. |
| 2 | Gọi `Save(save)`. | JSON được ghi vào `PlayerPrefs` tại key `JigsawVina_PlayerSave`. |
| 3 | Tạo instance `SaveDataService` mới và gọi `Load()`. | Save được deserialize thành công, không phụ thuộc instance service cũ. |
| 4 | Kiểm tra dữ liệu đã load. | `Coins = 125`, `Hints = 4`, `OwnedItemIds` chứa `101`, và Inventory Item `10` có `Amount = 5`. |

### Automation Notes

Automated via JigsawVina.Tests.SaveDataServiceTests.SaveAndLoad_SavesCorrectData.

## TC-SAVE-003: Lưu completion và best result

- **Module:** Save Load
- **Feature:** Completion Persistence
- **Case Status:** Active
- **Priority:** Critical
- **Test Suite:** Smoke
- **Test Level:** Integration
- **Automation Status:** Automated
- **Execution Mode:** EditMode
- **NUnit Test:** JigsawVina.Tests.SaveDataServiceTests.SaveAndLoad_SavesCorrectData

### Preconditions

1. `PlayerPrefs` không có save cũ.
2. Một completion hợp lệ đã được tạo cho Picture ID `1`, Difficulty ID `0`.

### Test Data

| Field | Value |
|---|---|
| Picture ID | `1` |
| Difficulty ID | `0` |
| Best time | `42.5` seconds |
| Best star | `1` |

### Steps

| # | Action | Expected Result |
|---:|---|---|
| 1 | Thêm `CompletedPuzzleData` theo Test Data vào `PlayerSave.CompletedPuzzles`. | Save có đúng một completion với composite key Picture `1` / Difficulty `0`. |
| 2 | Gọi `Save(save)`. | Save được persist mà không làm thay đổi best time hoặc best star trong object. |
| 3 | Gọi `Load()` bằng service mới. | Save được load thành công và `CompletedPuzzles` không null. |
| 4 | Tìm completion Picture `1` / Difficulty `0`. | Có đúng một record; `BestTimeSeconds = 42.5` và `BestStar = 1`. |

### Automation Notes

Automated via JigsawVina.Tests.SaveDataServiceTests.SaveAndLoad_SavesCorrectData.

## TC-SAVE-004: Reset DailyDropCounts khi đổi local date

- **Module:** Save Load
- **Feature:** Daily Save Normalization
- **Case Status:** Active
- **Priority:** Critical
- **Test Suite:** Smoke
- **Test Level:** Integration
- **Automation Status:** Automated
- **Execution Mode:** EditMode
- **NUnit Test:** JigsawVina.Tests.SaveDataServiceTests.Load_DailyDropCounts_ResetsOnDateChange

### Preconditions

1. Save đã persist với `LastSaveDateString = 2026-06-15`.
2. Save có daily count cho Item ID `10`.
3. Fake `ILocalDateProvider` có thể chuyển sang ngày kế tiếp.

### Test Data

| Field | Value |
|---|---|
| Previous local date | `2026-06-15` |
| Current local date | `2026-06-16` |
| Daily count before load | Item ID `10`, Count `3` |

### Steps

| # | Action | Expected Result |
|---:|---|---|
| 1 | Persist save với previous date và daily count theo Test Data. | JSON lưu chứa `LastSaveDateString = 2026-06-15` và Count `3`. |
| 2 | Đổi fake local date sang `2026-06-16`. | Date provider trả về ngày mới cho lần load kế tiếp. |
| 3 | Gọi `SaveDataService.Load()`. | Save được normalize theo ngày mới mà không mất các field không liên quan. |
| 4 | Kiểm tra daily state sau load. | `DailyDropCounts` rỗng và `LastSaveDateString = 2026-06-16`. |

### Automation Notes

Automated via JigsawVina.Tests.SaveDataServiceTests.Load_DailyDropCounts_ResetsOnDateChange.
