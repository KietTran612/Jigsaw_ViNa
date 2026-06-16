# Daily Drop Test Cases

## TC-DROP-001: Mỗi active drop entry roll độc lập

- **Module:** Daily Drop
- **Feature:** Independent Drop Rolls
- **Case Status:** Active
- **Priority:** Critical
- **Test Suite:** Smoke
- **Test Level:** Unit
- **Automation Status:** Planned
- **Execution Mode:** EditMode
- **NUnit Test:** none

### Preconditions

1. Drop Table ID `1001` fixture có hai active entries hợp lệ.
2. Save không sở hữu hoặc full-stack các item trong fixture.
3. Fake random source trả lần lượt `0.10` và `0.90`.

### Test Data

| Field | Value |
|---|---|
| Entry A | Item ID `10`, rate `0.50`, amount `1` |
| Entry B | Item ID `2`, rate `0.50`, amount `1` |
| Random values | `0.10`, `0.90` |

### Steps

| # | Action | Expected Result |
|---:|---|---|
| 1 | Gọi `RollDropRewards(1001, save)`. | Random source được gọi một lần cho mỗi active entry, tổng cộng hai lần. |
| 2 | Kiểm tra kết quả Entry A. | Item ID `10` được grant vì `0.10 < 0.50`. |
| 3 | Kiểm tra kết quả Entry B. | Item ID `2` không được grant vì `0.90 >= 0.50`. |
| 4 | Kiểm tra tổng reward list và counter. | Reward list chỉ có Item ID `10`; chỉ counter của Item ID `10` tăng. |

### Automation Notes

Planned EditMode unit test sẽ dùng ordered fake random values để chứng minh mỗi active entry được evaluate độc lập.

## TC-DROP-002: Drop rate decay theo số lần item đã drop trong ngày

- **Module:** Daily Drop
- **Feature:** Daily Drop Decay
- **Case Status:** Active
- **Priority:** Critical
- **Test Suite:** Smoke
- **Test Level:** Unit
- **Automation Status:** Planned
- **Execution Mode:** EditMode
- **NUnit Test:** none

### Preconditions

1. Active drop entry cho Item ID `10` có rate và decay hợp lệ.
2. Save có daily success count `2` cho Item ID `10`.
3. Fake random source trả `0.39`.

### Test Data

| Field | Value |
|---|---|
| Item ID | `10` |
| Base rate | `0.60` |
| Decay per success | `0.10` |
| Minimum rate | `0.20` |
| Existing success count | `2` |
| Expected current rate | `0.40` |

### Steps

| # | Action | Expected Result |
|---:|---|---|
| 1 | Tính current rate qua `RollDropRewards`. | Service dùng `max(0.20, 0.60 - 2 * 0.10) = 0.40`. |
| 2 | So sánh random value `0.39` với current rate. | Roll thành công vì `0.39 < 0.40`. |
| 3 | Kiểm tra reward result. | Reward cho Item ID `10` được trả với amount hợp lệ. |
| 4 | Kiểm tra daily counter. | Count của Item ID `10` tăng từ `2` lên `3`. |

### Automation Notes

Planned EditMode unit test sẽ inject deterministic random source và kiểm tra rate được suy ra qua success/failure boundary.

## TC-DROP-003: Replay reward cấp coins và hints đúng amount

- **Module:** Daily Drop
- **Feature:** Replay Drop Application
- **Case Status:** Active
- **Priority:** Critical
- **Test Suite:** Smoke
- **Test Level:** Integration
- **Automation Status:** Planned
- **Execution Mode:** EditMode
- **NUnit Test:** none

### Preconditions

1. Save đã có completion cho Picture ID `1`, Difficulty ID `0`, nên lần hoàn thành là replay.
2. Save ban đầu có `Coins = 100` và `Hints = 2`.
3. Production mapping xác định Picture ID `1`, Difficulty ID `0` dùng Drop Table ID `1001`.
4. Drop service fixture trả Coin Item ID `1` amount `4` và Hint Item ID `2` amount `2`.

### Test Data

| Field | Value |
|---|---|
| Picture / Difficulty mapping | Picture ID `1` / Difficulty ID `0` -> Drop Table ID `1001` |
| Replay coins from difficulty | `10` |
| Coin drop | Item ID `1`, Amount `4` |
| Hint drop | Item ID `2`, Amount `2` |
| Expected final coins | `114` |
| Expected final hints | `4` |

### Steps

| # | Action | Expected Result |
|---:|---|---|
| 1 | Load config Picture `1` / Difficulty `0` và process replay reward. | Config trả `DropTableId = 1001`; Reward presenter gọi drop service với đúng Drop Table ID `1001`. |
| 2 | Apply replay coin và Coin drop. | Coins tăng từ `100` lên `114`, gồm `10` replay coins và `4` drop coins. |
| 3 | Apply Hint drop. | Hints tăng từ `2` lên `4`. |
| 4 | Kiểm tra Reward Summary data. | `LastCoinEarned = 14` và rewarded-items label chứa `Hint x2`. |

### Automation Notes

Planned EditMode integration test sẽ dùng fake `IDropRewardService` và kiểm tra actual applied amounts trong save cùng summary state.

## TC-DROP-004: Không drop Key Item đã sở hữu

- **Module:** Daily Drop
- **Feature:** Owned Key Item Exclusion
- **Case Status:** Active
- **Priority:** Critical
- **Test Suite:** Smoke
- **Test Level:** Unit
- **Automation Status:** Planned
- **Execution Mode:** EditMode
- **NUnit Test:** none

### Preconditions

1. Drop table fixture có active entry cho Key Item ID `101`.
2. Save đã chứa Item ID `101` trong `OwnedItemIds`.
3. Save có existing daily count `2` cho Item ID `101`.

### Test Data

| Field | Value |
|---|---|
| Key Item ID | `101` |
| Owned state | `true` |
| Existing daily count | `2` |

### Steps

| # | Action | Expected Result |
|---:|---|---|
| 1 | Gọi `RollDropRewards` cho drop table chứa Item ID `101`. | Entry được loại trước khi thực hiện random roll. |
| 2 | Kiểm tra fake random source. | Random source không được gọi cho Item ID `101`. |
| 3 | Kiểm tra reward list. | Không có reward cho Item ID `101`. |
| 4 | Kiểm tra save state. | `OwnedItemIds` không có duplicate và daily count của Item ID `101` vẫn bằng `2`. |

### Automation Notes

Planned EditMode unit test sẽ dùng recording random source để xác nhận exclusion xảy ra trước roll và không mutate counter.

## TC-DROP-005: Chỉ tăng DailyDropCounts khi grant thành công

- **Module:** Daily Drop
- **Feature:** Daily Drop Success Counters
- **Case Status:** Active
- **Priority:** Critical
- **Test Suite:** Smoke
- **Test Level:** Integration
- **Automation Status:** Planned
- **Execution Mode:** EditMode
- **NUnit Test:** none

### Preconditions

1. Active drop entry fixture cho Consumable Item ID `10` có current rate `0.30` và configured amount `2`.
2. Production Item ID `10` có `max_stack = 999`.
3. Mỗi scenario dùng save mới, chưa có `DailyDropCount` cho Item ID `10`.
4. Random source có thể cung cấp failed roll `0.30` và successful roll `0.29`.

### Test Data

| Field | Value |
|---|---|
| Item ID | `10` |
| Item type | `consumable` |
| Configured amount | `2` |
| Max stack | `999` |
| Partial-stack inventory | `998` |
| Partial-stack capacity | `1` |
| Full-stack inventory | `999` |
| Full-stack capacity | `0` |
| Current rate | `0.30` |
| Failed random value | `0.30` |
| Successful random value | `0.29` |

### Steps

| # | Action | Expected Result |
|---:|---|---|
| 1 | Với save có Inventory Item `10` amount `998`, roll bằng random value `0.30`. | Roll thất bại vì value không nhỏ hơn current rate; reward list rỗng và inventory capacity vẫn là `1`. |
| 2 | Kiểm tra save sau failed roll. | Không tạo `DailyDropCount` cho Item ID `10`. |
| 3 | Với save mới có Inventory Item `10` amount `998`, roll bằng random value `0.29` và configured amount `2`. | Roll thành công; amount được clamp theo capacity và trả actual nonzero reward Item ID `10`, Amount `1`. |
| 4 | Kiểm tra save sau actual nonzero grant. | `DailyDropCounts` có đúng một entry Item ID `10` với Count `1`; counter chỉ tăng sau khi actual granted amount được xác định là `1`. |
| 5 | Với save mới có Inventory Item `10` amount `999`, gọi roll cho cùng entry. | Entry bị loại trước random vì capacity bằng `0`; reward list rỗng. |
| 6 | Kiểm tra save sau full-stack exclusion. | Inventory không vượt `999`, không tạo `DailyDropCount` cho Item ID `10` và random source không được gọi cho entry này. |

### Automation Notes

Planned EditMode integration test sẽ chạy ba save fixtures độc lập: failed roll, partial-stack nonzero grant và full-stack exclusion.
