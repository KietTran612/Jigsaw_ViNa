# Jigsaw ViNa Living Test Plan

- **Schema Version:** 1
- **Source of Truth:** Markdown files in this directory
- **Excel Artifact:** `generated/jigsaw-vina-test-cases.xlsx`
- **Export Policy:** Export only when explicitly requested

## Scope

Test plan này bao phủ luồng hiện tại dành cho người chơi:

- chọn và mở khóa tranh tại Home;
- chọn difficulty;
- chơi và hoàn thành puzzle;
- xem Reward Summary và quay lại Home;
- save/load và migration;
- progression;
- daily drop rewards;
- Collection;
- các lỗi và hành vi recovery liên quan.

Game Data Editor và các công cụ Unity Editor nằm ngoài phạm vi test plan này.

## Modules

| Order | Module | ID Prefix | File |
|---:|---|---|---|
| 1 | Gameplay | `TC-GAMEPLAY` | `gameplay.md` |
| 2 | Save Load | `TC-SAVE` | `save-load.md` |
| 3 | Progression | `TC-PROGRESSION` | `progression.md` |
| 4 | Daily Drop | `TC-DROP` | `daily-drop.md` |
| 5 | Collection | `TC-COLLECTION` | `collection.md` |
| 6 | Error Handling | `TC-ERROR` | `error-handling.md` |

## Test Case Schema

Mỗi test case phải dùng đúng thứ tự metadata và section sau:

```markdown
## TC-GAMEPLAY-001: Tên test case

- **Module:** Gameplay
- **Feature:** Tên tính năng
- **Case Status:** Active
- **Priority:** Critical
- **Test Suite:** Smoke
- **Test Level:** End-to-End
- **Automation Status:** Planned
- **Execution Mode:** PlayMode
- **NUnit Test:** none

### Preconditions

1. Điều kiện cần thiết.

### Test Data

| Field | Value |
|---|---|
| Tên dữ liệu | Giá trị |

### Steps

| # | Action | Expected Result |
|---:|---|---|
| 1 | Hành động kiểm thử. | Kết quả quan sát được. |

### Automation Notes

Mô tả cách tự động hóa hoặc lý do phải kiểm thử thủ công.
```

Mỗi action trong bảng `Steps` phải có một `Expected Result` trực tiếp quan sát hoặc kiểm chứng được.

## Validation Contract

- `NUnit Test` phải là literal `none` hoặc một hay nhiều fully qualified NUnit test names.
- Mỗi NUnit test name phải có dạng `Namespace.Class.Method`, trong đó mỗi segment là một C# identifier hợp lệ: bắt đầu bằng chữ cái hoặc `_`, sau đó chỉ gồm chữ cái, chữ số hoặc `_`.
- Nhiều NUnit test names được phân cách bằng dấu chấm phẩy; mọi segment phải được trim và không được để trống.
- `NUnit Test: none` là giá trị duy nhất hợp lệ khi chưa có mapping hoặc mapping không áp dụng.
- Bảng `Test Data` phải dùng chính xác hai canonical headers `| Field | Value |` và `|---|---|`.
- Bảng `Steps` phải dùng chính xác ba canonical headers `| # | Action | Expected Result |` và `|---:|---|---|`.
- Step number phải là số nguyên dương, bắt đầu từ `1`, không trùng và liên tục tăng thêm `1` trong từng test case.
- `Action` và `Expected Result` của mỗi step không được để trống.

## Empty Sections

Nếu không có precondition hoặc test data, vẫn giữ section và dùng đúng quy ước:

```markdown
### Preconditions

1. None.

### Test Data

| Field | Value |
|---|---|
| None | N/A |
```

## Allowed Values

| Field | Values |
|---|---|
| Case Status | `Active`, `Deprecated` |
| Priority | `Critical`, `High`, `Medium`, `Low` |
| Test Suite | `Smoke`, `Regression` |
| Test Level | `Unit`, `Integration`, `End-to-End` |
| Automation Status | `Automated`, `Planned`, `Manual Only`, `Not Applicable` |
| Execution Mode | `EditMode`, `PlayMode`, `Manual`, `N/A` |

## Lifecycle And Automation Rules

- ID phải đúng prefix của module, không trùng, không đổi và không được tái sử dụng sau khi publish.
- Test case không còn áp dụng phải chuyển sang `Deprecated`, giữ trong module hiện tại và ghi lý do retirement trong `Automation Notes`.
- Cập nhật thường xuyên được thực hiện trực tiếp trên Living Test Plan và các module file, không tạo dated plan mới.
- `Automated` yêu cầu ít nhất một fully qualified NUnit mapping đang pass.
- Nhiều NUnit mappings được phân cách bằng dấu chấm phẩy.
- `Planned` phải dùng `EditMode` hoặc `PlayMode` và `NUnit Test` phải là `none`.
- `Manual Only` phải dùng `Manual` và ghi rõ lý do không thể hoặc không nên tự động hóa.
- `Not Applicable` phải dùng `N/A`, `NUnit Test: none` và giải thích lý do trong `Automation Notes`.
- File Excel là generated output, không được chỉnh sửa hoặc dùng làm source.
- Mọi thay đổi schema phải tăng `Schema Version` và cập nhật exporter trước khi áp dụng cho module files.

## Initial Coverage Target

| Module | Planned Cases |
|---|---:|
| Gameplay | 7 |
| Save Load | 4 |
| Progression | 7 |
| Daily Drop | 5 |
| Collection | 3 |
| Error Handling | 5 |
| **Total** | **31** |

## Regression Backlog

Các topic sau chưa reserve test ID:

- Invalid-drop feedback.
- Hint placement.
- Original Image opacity.
- Legacy save null-list normalization.
- Repeated valid save loading.
- Same-day daily counter retention.
- Sequential difficulty lock presentation.
- AllUnlocked difficulty policy.
- Minimum drop-rate clamp.
- Inclusive amount bounds.
- Partial consumable stacks.
- First-clear and drop-table source presentation.
- Locked-picture focus from Collection.
- Gameplay fallback without a selected session picture.
- Additional malformed static-data variants.
