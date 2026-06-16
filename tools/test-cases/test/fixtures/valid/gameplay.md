# Gameplay Test Cases

## TC-GAMEPLAY-001: Complete a puzzle

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

1. Picture 1 is selected.
2. Difficulty 0 is unlocked.

### Test Data

| Field | Value |
|---|---|
| Picture ID | 1 |
| Difficulty ID | 0 |

### Steps

| # | Action | Expected Result |
|---:|---|---|
| 1 | Open Gameplay. | The configured puzzle loads. |
| 2 | Complete every piece. | Reward Summary is shown. |

### Automation Notes

Planned PlayMode coverage.
