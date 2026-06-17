# Collection Test Cases

## TC-COLLECTION-001: Mở Collection từ Home

- **Module:** Collection
- **Feature:** Collection Navigation
- **Case Status:** Active
- **Priority:** Critical
- **Test Suite:** Smoke
- **Test Level:** End-to-End
- **Automation Status:** Planned
- **Execution Mode:** PlayMode
- **NUnit Test:** none

### Preconditions

1. Scene `Home` đã load và Picture Select đang hiển thị.
2. `CollectionView` và `CollectionPresenter` đã được đăng ký trong `HomeLifetimeScope`.
3. Nút mở Collection đã được wire với `PictureSelectView.OnCollectionRequested`.

### Test Data

| Field | Value |
|---|---|
| Start screen | Picture Select |
| Target screen | Collection |

### Steps

| # | Action | Expected Result |
|---:|---|---|
| 1 | Nhấn nút Collection tại Picture Select. | `OnCollectionRequested` được phát đúng một lần. |
| 2 | Chờ `HomeFlowController` xử lý yêu cầu. | `CollectionPresenter.Refresh()` được gọi trước khi màn hình hiển thị dữ liệu. |
| 3 | Quan sát screen state. | Picture Select và Difficulty Select được ẩn; Collection được active. |
| 4 | Nhấn nút Close của Collection. | Collection được ẩn và Picture Select hiển thị lại. |

### Automation Notes

Planned PlayMode test sẽ click các button thật trong scene `Home` và kiểm tra screen state qua toàn bộ open/close flow.

## TC-COLLECTION-002: Chỉ hiển thị Key Items đã sở hữu

- **Module:** Collection
- **Feature:** Owned Key Item Filtering
- **Case Status:** Active
- **Priority:** Critical
- **Test Suite:** Smoke
- **Test Level:** Integration
- **Automation Status:** Automated
- **Execution Mode:** EditMode
- **NUnit Test:** JigsawVina.Tests.CollectionFlowTests.CollectionPresenter_BuildsOwnedItemWithDeduplicatedSortedSources

### Preconditions

1. Static data fixture có active Key Item ID `101`, active Key Item ID `102` và Consumable Item ID `10`.
2. Save sở hữu Item ID `101`, không sở hữu Item ID `102`, và có Inventory Item ID `10`.
3. Collection presenter có view hợp lệ.

### Test Data

| Field | Value |
|---|---|
| OwnedItemIds | `[101]` |
| Inventory | Item ID `10`, Amount `5` |
| Unowned Key Item | Item ID `102` |

### Steps

| # | Action | Expected Result |
|---:|---|---|
| 1 | Gọi `CollectionPresenter.Refresh()`. | Presenter load và normalize save mà không có exception. |
| 2 | Kiểm tra `CurrentModels`. | Danh sách có đúng một model cho Item ID `101`. |
| 3 | Kiểm tra item không được hiển thị. | Item ID `102` không xuất hiện vì chưa sở hữu; Item ID `10` không xuất hiện vì không phải `key_item`. |
| 4 | Kiểm tra dữ liệu bind vào view. | Model Item ID `101` có display name, description, asset path và source list từ static data. |

### Automation Notes

Automated via JigsawVina.Tests.CollectionFlowTests.CollectionPresenter_BuildsOwnedItemWithDeduplicatedSortedSources.

## TC-COLLECTION-003: Điều hướng tới tranh đã mở khóa từ source

- **Module:** Collection
- **Feature:** Collection Source Navigation
- **Case Status:** Active
- **Priority:** Critical
- **Test Suite:** Smoke
- **Test Level:** End-to-End
- **Automation Status:** Planned
- **Execution Mode:** PlayMode
- **NUnit Test:** none

### Preconditions

1. Collection đang hiển thị Item ID `107`.
2. Source của Item ID `107` trỏ tới Picture ID `1`, Difficulty ID `0`.
3. Picture ID `1` đang `Unlocked` hoặc `Completed`.

### Test Data

| Field | Value |
|---|---|
| Key Item ID | `107` |
| Source Picture ID | `1` |
| Source Difficulty ID | `0` |

### Steps

| # | Action | Expected Result |
|---:|---|---|
| 1 | Chọn Item ID `107` trong Collection. | Item detail hiển thị và source Picture `1` / Difficulty `0` có thể tương tác. |
| 2 | Nhấn source của Picture ID `1`. | `OnNavigateToPictureRequested` được phát với Picture ID `1`. |
| 3 | Chờ Home flow xử lý navigation. | Collection được ẩn; Picture Select được active tạm thời. |
| 4 | Quan sát kết quả navigation. | Picture ID `1` được chọn tự động và Difficulty Select mở cho Picture ID `1`. |

### Automation Notes

Planned PlayMode test sẽ mở Collection, chọn source model và kiểm tra unlocked-picture route đi tới Difficulty Select.
