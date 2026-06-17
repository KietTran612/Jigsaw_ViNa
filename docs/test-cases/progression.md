# Progression Test Cases

## TC-PROGRESSION-001: Tranh initially unlocked có thể được chọn

- **Module:** Progression
- **Feature:** Picture Unlock State
- **Case Status:** Active
- **Priority:** Critical
- **Test Suite:** Smoke
- **Test Level:** Integration
- **Automation Status:** Automated
- **Execution Mode:** EditMode
- **NUnit Test:** JigsawVina.Tests.ProgressionTests.GetPictureState_InitiallyUnlocked_ReturnsUnlockedOrCompleted

### Preconditions

1. Static data fixture có Picture ID `1` với `is_initially_unlocked = true`.
2. Save không chứa Picture ID `1` trong `UnlockedPictureIds`.
3. Picture ID `1` chưa completed toàn bộ difficulty.

### Test Data

| Field | Value |
|---|---|
| Picture ID | `1` |
| Initially unlocked | `true` |
| Difficulty policy | `sequential` |

### Steps

| # | Action | Expected Result |
|---:|---|---|
| 1 | Gọi `ProgressionService.GetPictureState(1)`. | Kết quả là `PictureCardState.Unlocked`, không phụ thuộc `UnlockedPictureIds`. |
| 2 | Build `PictureCardPresentationModel` cho Picture ID `1`. | Model không đánh dấu tranh là `Locked` hoặc `ReadyToUnlock`. |
| 3 | Bind model vào `PictureSelectCard`. | Lock overlay ẩn và nút chọn tranh có thể tương tác. |
| 4 | Phát yêu cầu chọn Picture ID `1`. | Sự kiện chọn tranh được chuyển tiếp để Home flow mở Difficulty Select. |

### Automation Notes

Automated via JigsawVina.Tests.ProgressionTests.GetPictureState_InitiallyUnlocked_ReturnsUnlockedOrCompleted.

## TC-PROGRESSION-002: Tranh khóa hiển thị required Key Items

- **Module:** Progression
- **Feature:** Locked Picture Requirements
- **Case Status:** Active
- **Priority:** Critical
- **Test Suite:** Smoke
- **Test Level:** Integration
- **Automation Status:** Automated
- **Execution Mode:** EditMode
- **NUnit Test:** JigsawVina.Tests.ProgressionTests.GetPictureState_NotInitiallyUnlocked_Locked_AndTransitionToReadyToUnlock; JigsawVina.Tests.PictureSelectFlowTests.Card_Locked_DisablesSelectionAndShowsMissingItemHint

### Preconditions

1. Static data fixture có Picture ID `2` với `is_initially_unlocked = false`.
2. Picture ID `2` yêu cầu Key Item ID `101`.
3. Save chưa sở hữu Item ID `101` và chưa unlock Picture ID `2`.

### Test Data

| Field | Value |
|---|---|
| Picture ID | `2` |
| Required Key Item | Item ID `101` |
| Item type | `key_item` |

### Steps

| # | Action | Expected Result |
|---:|---|---|
| 1 | Gọi `ProgressionService.GetPictureState(2)`. | Kết quả là `PictureCardState.Locked`. |
| 2 | Build presentation model cho Picture ID `2`. | Model chứa missing-item hint cho Item ID `101`. |
| 3 | Bind model vào `PictureSelectCard`. | Lock overlay và missing-items hint hiển thị; nút chọn tranh bị disable. |
| 4 | Kiểm tra nút Unlock. | Nút Unlock không hiển thị hoặc không thể tương tác vì requirement chưa đủ. |

### Automation Notes

Automated via JigsawVina.Tests.ProgressionTests.GetPictureState_NotInitiallyUnlocked_Locked_AndTransitionToReadyToUnlock and JigsawVina.Tests.PictureSelectFlowTests.Card_Locked_DisablesSelectionAndShowsMissingItemHint.

## TC-PROGRESSION-003: Đủ Key Items chuyển tranh sang ReadyToUnlock

- **Module:** Progression
- **Feature:** Unlock Readiness
- **Case Status:** Active
- **Priority:** Critical
- **Test Suite:** Smoke
- **Test Level:** Integration
- **Automation Status:** Automated
- **Execution Mode:** EditMode
- **NUnit Test:** JigsawVina.Tests.ProgressionTests.GetPictureState_NotInitiallyUnlocked_Locked_AndTransitionToReadyToUnlock; JigsawVina.Tests.PictureSelectFlowTests.Card_ReadyToUnlock_ShowsUnlockButtonAndRaisesRequest

### Preconditions

1. Static data fixture có Picture ID `2` yêu cầu Key Item ID `101`.
2. Save chưa có Picture ID `2` trong `UnlockedPictureIds`.
3. Save ban đầu chưa có Item ID `101`.

### Test Data

| Field | Value |
|---|---|
| Picture ID | `2` |
| Required Key Item | Item ID `101` |

### Steps

| # | Action | Expected Result |
|---:|---|---|
| 1 | Xác nhận state trước khi nhận item. | `GetPictureState(2)` trả về `Locked`. |
| 2 | Thêm Item ID `101` vào `PlayerSave.OwnedItemIds`. | Save sở hữu đầy đủ requirement nhưng chưa tự thêm Picture ID `2` vào `UnlockedPictureIds`. |
| 3 | Gọi lại `GetPictureState(2)`. | State chuyển thành `PictureCardState.ReadyToUnlock`. |
| 4 | Refresh card của Picture ID `2`. | Lock overlay vẫn hiển thị, missing requirement không còn và nút Unlock hiển thị có thể tương tác. |

### Automation Notes

Automated via JigsawVina.Tests.ProgressionTests.GetPictureState_NotInitiallyUnlocked_Locked_AndTransitionToReadyToUnlock and JigsawVina.Tests.PictureSelectFlowTests.Card_ReadyToUnlock_ShowsUnlockButtonAndRaisesRequest.

## TC-PROGRESSION-004: Unlock tranh atomically và persist

- **Module:** Progression
- **Feature:** Atomic Picture Unlock
- **Case Status:** Active
- **Priority:** Critical
- **Test Suite:** Smoke
- **Test Level:** Integration
- **Automation Status:** Automated
- **Execution Mode:** EditMode
- **NUnit Test:** JigsawVina.Tests.ProgressionTests.TryUnlockPicture_FlowAndConstraints; JigsawVina.Tests.PictureSelectFlowTests.Presenter_UnlockSuccess_RefreshesCardAsUnlocked

### Preconditions

1. Picture ID `2` không initially unlocked và yêu cầu Item ID `101`.
2. Save sở hữu Item ID `101`.
3. Save chưa có Picture ID `2` trong `UnlockedPictureIds`.
4. Mock save service ghi nhận số lần gọi `Save`.

### Test Data

| Field | Value |
|---|---|
| Picture ID | `2` |
| Required Key Item | Item ID `101` |
| Expected save calls | `1` |

### Steps

| # | Action | Expected Result |
|---:|---|---|
| 1 | Gọi `ProgressionService.TryUnlockPicture(2)`. | Kết quả là `UnlockResult.Success`. |
| 2 | Kiểm tra save trong memory ngay sau thao tác. | `UnlockedPictureIds` chứa Picture ID `2` đúng một lần. |
| 3 | Kiểm tra persistence call. | Save service được gọi đúng một lần với state đã có Picture ID `2`. |
| 4 | Load lại save đã persist và gọi `GetPictureState(2)`. | Picture ID `2` vẫn có trong `UnlockedPictureIds` và state là `Unlocked`. |

### Automation Notes

Automated via JigsawVina.Tests.ProgressionTests.TryUnlockPicture_FlowAndConstraints and JigsawVina.Tests.PictureSelectFlowTests.Presenter_UnlockSuccess_RefreshesCardAsUnlocked.

## TC-PROGRESSION-005: Unlock không consume Key Items

- **Module:** Progression
- **Feature:** Non-Consumable Unlock Requirement
- **Case Status:** Active
- **Priority:** Critical
- **Test Suite:** Smoke
- **Test Level:** Unit
- **Automation Status:** Automated
- **Execution Mode:** EditMode
- **NUnit Test:** JigsawVina.Tests.ProgressionTests.TryUnlockPicture_FlowAndConstraints

### Preconditions

1. Picture ID `2` yêu cầu active, non-consumable Key Item ID `101`.
2. Save sở hữu duy nhất Item ID `101`.
3. Picture ID `2` đang `ReadyToUnlock`.

### Test Data

| Field | Value |
|---|---|
| Picture ID | `2` |
| Owned item before unlock | `[101]` |

### Steps

| # | Action | Expected Result |
|---:|---|---|
| 1 | Ghi nhận snapshot của `OwnedItemIds`. | Snapshot chứa đúng Item ID `101`. |
| 2 | Gọi `TryUnlockPicture(2)`. | Kết quả là `Success` và Picture ID `2` được unlock. |
| 3 | Kiểm tra `OwnedItemIds` sau unlock. | Item ID `101` vẫn tồn tại, số lượng item không giảm và không có duplicate. |
| 4 | Gọi `TryUnlockPicture(2)` lần nữa. | Kết quả là `AlreadyUnlocked`; item vẫn còn và save không bị ghi thêm. |

### Automation Notes

Automated via JigsawVina.Tests.ProgressionTests.TryUnlockPicture_FlowAndConstraints.

## TC-PROGRESSION-006: Sequential policy mở difficulty sau khi clear

- **Module:** Progression
- **Feature:** Sequential Difficulty Unlock
- **Case Status:** Active
- **Priority:** Critical
- **Test Suite:** Smoke
- **Test Level:** Integration
- **Automation Status:** Automated
- **Execution Mode:** EditMode
- **NUnit Test:** JigsawVina.Tests.ProgressionTests.IsDifficultyUnlocked_SequentialPolicy; JigsawVina.Tests.DifficultySelectFlowTests.Presenter_Refresh_UpdatesUIAccordingToProgression

### Preconditions

1. Picture ID `1` đã mở khóa và có Difficulty ID `0`, `1`, `2`.
2. Picture ID `1` dùng `difficulty_unlock_policy = sequential`.
3. Save chưa có completion cho Picture ID `1`.

### Test Data

| Field | Value |
|---|---|
| Picture ID | `1` |
| Difficulty IDs | `0`, `1`, `2` |

### Steps

| # | Action | Expected Result |
|---:|---|---|
| 1 | Kiểm tra unlock state ban đầu của ba difficulty. | Difficulty `0` unlocked; Difficulty `1` và `2` locked. |
| 2 | Thêm completion Difficulty `0` với `BestStar = 1`. | Completion được nhận là clear hợp lệ. |
| 3 | Kiểm tra unlock state sau clear Difficulty `0`. | Difficulty `1` unlocked; Difficulty `2` vẫn locked. |
| 4 | Thêm completion Difficulty `1` với `BestStar = 2`. | Save có valid completion cho hai difficulty trước. |
| 5 | Kiểm tra Difficulty `2`. | Difficulty `2` được unlock. |

### Automation Notes

Automated via JigsawVina.Tests.ProgressionTests.IsDifficultyUnlocked_SequentialPolicy and JigsawVina.Tests.DifficultySelectFlowTests.Presenter_Refresh_UpdatesUIAccordingToProgression.

## TC-PROGRESSION-007: First-clear reward chỉ cấp một lần

- **Module:** Progression
- **Feature:** First-Clear Reward
- **Case Status:** Active
- **Priority:** Critical
- **Test Suite:** Smoke
- **Test Level:** Integration
- **Automation Status:** Automated
- **Execution Mode:** EditMode
- **NUnit Test:** JigsawVina.Tests.ProgressionTests.ProcessRewards_FirstClear_AwardsFirstClearCoinsHintsAndItems; JigsawVina.Tests.ProgressionTests.ProcessRewards_Replay_AwardsReplayCoinsOnly; JigsawVina.Tests.ProgressionTests.ProcessRewards_FirstClear_DoesNotRollDropTable

### Preconditions

1. Save chưa có completion cho Picture ID `1`, Difficulty ID `0`.
2. `GameSessionService` chọn Picture ID `1`, Difficulty ID `0`.
3. Drop service fixture không trả replay drop để cô lập first-clear behavior.

### Test Data

| Field | Value |
|---|---|
| First-clear coins | `30` |
| Replay coins | `10` |
| First-clear Key Item | Item ID `107` |
| Elapsed time | `20` seconds |

### Steps

| # | Action | Expected Result |
|---:|---|---|
| 1 | Process reward cho lần hoàn thành đầu tiên. | Save tăng `30` coins, nhận Item ID `107` và tạo đúng một completion. |
| 2 | Bắt đầu session replay cho cùng Picture và Difficulty. | Session reward guard được reset cho puzzle mới; completion cũ vẫn tồn tại. |
| 3 | Process reward cho lần hoàn thành replay. | Chỉ replay coins `10` được cấp; first-clear coins và Item ID `107` không được cấp lại. |
| 4 | Kiểm tra save sau hai lần hoàn thành. | Tổng coins tăng `40`, Item ID `107` xuất hiện đúng một lần và chỉ có một completion cho composite key này. |

### Automation Notes

Automated via JigsawVina.Tests.ProgressionTests.ProcessRewards_FirstClear_AwardsFirstClearCoinsHintsAndItems, JigsawVina.Tests.ProgressionTests.ProcessRewards_Replay_AwardsReplayCoinsOnly, and JigsawVina.Tests.ProgressionTests.ProcessRewards_FirstClear_DoesNotRollDropTable.
