# Game Test Case System Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a Markdown-first Living Test Plan covering the current game flow and a manually invoked, validated Excel exporter for QA.

**Architecture:** `docs/test-cases/README.md` owns the living schema and module order; six module files own the test cases. A testable Node.js module parses and validates Markdown, builds the workbook with bundled `@oai/artifact-tool`, verifies key ranges, renders previews, and exports atomically. A PowerShell entrypoint receives the bundled runtime paths and launches the exporter without requiring Excel, global Node.js, or installed PowerShell modules.

**Tech Stack:** Markdown, PowerShell 7/Windows PowerShell, bundled Node.js, Node built-in `node:test`, `@oai/artifact-tool`, Git.

**Project Constraint:** Do not commit during implementation unless the user explicitly requests it. The commit steps normally required by the generic planning workflow are intentionally omitted.

---

## File Map

**Create**

- `docs/test-cases/README.md`: Living Test Plan, schema version, module registry, maintenance rules, and coverage summary.
- `docs/test-cases/gameplay.md`: Critical Home-to-Gameplay, puzzle interaction, completion, and reward-return cases.
- `docs/test-cases/save-load.md`: save creation, normalization, persistence, reload, and date rollover cases.
- `docs/test-cases/progression.md`: picture unlock, difficulty policy, first-clear, and reward progression cases.
- `docs/test-cases/daily-drop.md`: replay drop rate, decay, grant, exclusion, and counter cases.
- `docs/test-cases/collection.md`: owned-item display, source hints, navigation, and lifecycle cases.
- `docs/test-cases/error-handling.md`: invalid selection, invalid data, unavailable assets, duplicate actions, and recovery cases.
- `tools/test-cases/export_test_cases_to_excel.mjs`: parser, validator, workbook builder, visual verification, and atomic exporter.
- `tools/test-cases/export_test_cases_to_excel.ps1`: manual runtime-resolving entrypoint.
- `tools/test-cases/test/export_test_cases_to_excel.test.mjs`: focused parser, validation, normalization, and export tests.
- `tools/test-cases/test/fixtures/valid/README.md`: minimal valid schema/module fixture.
- `tools/test-cases/test/fixtures/valid/gameplay.md`: minimal valid test case fixture.

**Modify**

- `.gitignore`: ignore generated workbook and temporary exporter artifacts while retaining source Markdown.
- `docs/plans/task.md`: record implementation completion and subsequent NUnit-mapping work.
- `docs/plans/current-handoff.md`: record generated coverage, exporter verification, warnings, and uncommitted scope.

**Generated on explicit request only**

- `docs/test-cases/generated/jigsaw-vina-test-cases.xlsx`

---

### Task 1: Create the Living Test Plan Contract

**Files:**

- Create: `docs/test-cases/README.md`
- Modify: `.gitignore`

- [ ] **Step 1: Add generated-output ignore rules**

Add only these project-specific rules:

```gitignore
# Generated QA test case artifacts
/docs/test-cases/generated/*.xlsx
/tools/test-cases/.runtime/
/tools/test-cases/.preview/
/tools/test-cases/node_modules
```

Do not ignore `docs/test-cases/generated/` itself; source control may retain a small README later if needed.

- [ ] **Step 2: Write the Living Test Plan header and module registry**

Create `docs/test-cases/README.md` with:

```markdown
# Jigsaw ViNa Living Test Plan

- **Schema Version:** 1
- **Source of Truth:** Markdown files in this directory
- **Excel Artifact:** `generated/jigsaw-vina-test-cases.xlsx`
- **Export Policy:** Export only when explicitly requested

## Scope

This test plan covers the current player-facing flow:

- Home picture selection and unlock;
- difficulty selection;
- puzzle gameplay and completion;
- reward summary and return;
- save/load and migration behavior;
- progression;
- daily drop rewards;
- Collection;
- relevant error and recovery behavior.

Game Data Editor tooling is outside this test plan.

## Modules

| Order | Module | ID Prefix | File |
|---:|---|---|---|
| 1 | Gameplay | `TC-GAMEPLAY` | `gameplay.md` |
| 2 | Save Load | `TC-SAVE` | `save-load.md` |
| 3 | Progression | `TC-PROGRESSION` | `progression.md` |
| 4 | Daily Drop | `TC-DROP` | `daily-drop.md` |
| 5 | Collection | `TC-COLLECTION` | `collection.md` |
| 6 | Error Handling | `TC-ERROR` | `error-handling.md` |
```

- [ ] **Step 3: Add the complete canonical schema**

Document every required metadata field and section exactly:

```markdown
## Test Case Schema

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

State explicitly that empty sections use:

```markdown
### Preconditions

1. None.

### Test Data

| Field | Value |
|---|---|
| None | N/A |
```

- [ ] **Step 4: Add allowed values and lifecycle rules**

Document:

```markdown
## Allowed Values

| Field | Values |
|---|---|
| Case Status | `Active`, `Deprecated` |
| Priority | `Critical`, `High`, `Medium`, `Low` |
| Test Suite | `Smoke`, `Regression` |
| Test Level | `Unit`, `Integration`, `End-to-End` |
| Automation Status | `Automated`, `Planned`, `Manual Only`, `Not Applicable` |
| Execution Mode | `EditMode`, `PlayMode`, `Manual`, `N/A` |
```

Add these maintenance rules:

- IDs are immutable after publication.
- Deprecated cases remain in their module file with a retirement reason.
- Routine updates modify Living Test Plan files, not dated plan files.
- `Automated` requires at least one passing fully qualified NUnit mapping.
- Multiple NUnit mappings are separated by semicolons.
- The Excel workbook is generated output and is never edited as source.

- [ ] **Step 5: Add the initial coverage summary**

Add a manually maintained initial summary table whose totals match Task 2:

```markdown
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
```

- [ ] **Step 6: Record the Regression backlog without publishing test IDs**

Add a backlog section listing these future coverage topics without reserving IDs:

```markdown
## Regression Backlog

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
```

- [ ] **Step 7: Validate the documentation structure**

Run:

```powershell
rg -n "^# |^## |Schema Version|TC-GAMEPLAY|TC-SAVE|TC-PROGRESSION|TC-DROP|TC-COLLECTION|TC-ERROR" docs/test-cases/README.md
git diff --check -- .gitignore docs/test-cases/README.md
```

Expected: schema version and all six modules are present; `git diff --check` exits successfully.

---

### Task 2: Write the Initial Smoke and Critical Test Cases

**Files:**

- Create: `docs/test-cases/gameplay.md`
- Create: `docs/test-cases/save-load.md`
- Create: `docs/test-cases/progression.md`
- Create: `docs/test-cases/daily-drop.md`
- Create: `docs/test-cases/collection.md`
- Create: `docs/test-cases/error-handling.md`

- [ ] **Step 1: Create the Gameplay cases**

Write these seven cases using the canonical schema. Every step must contain a directly observable expected result.

| ID | Title | Priority | Suite | Level | Mode |
|---|---|---|---|---|---|
| TC-GAMEPLAY-001 | Mở Home và hiển thị danh sách tranh | Critical | Smoke | End-to-End | PlayMode |
| TC-GAMEPLAY-002 | Chọn tranh đã mở khóa và mở Difficulty Select | Critical | Smoke | End-to-End | PlayMode |
| TC-GAMEPLAY-003 | Chọn độ khó đã mở khóa và tải Gameplay | Critical | Smoke | End-to-End | PlayMode |
| TC-GAMEPLAY-004 | Tạo đúng số puzzle pieces theo cấu hình | Critical | Smoke | Integration | PlayMode |
| TC-GAMEPLAY-005 | Kéo và snap piece vào đúng vị trí | Critical | Smoke | End-to-End | PlayMode |
| TC-GAMEPLAY-006 | Hoàn thành puzzle và mở Reward Summary | Critical | Smoke | End-to-End | PlayMode |
| TC-GAMEPLAY-007 | Nhận reward và quay về Home | Critical | Smoke | End-to-End | PlayMode |

Use concrete initial data such as Picture ID `1` and Difficulty ID `0` where the behavior does not require a locked picture.

- [ ] **Step 2: Create the Save Load cases**

| ID | Title | Priority | Suite | Level | Mode |
|---|---|---|---|---|---|
| TC-SAVE-001 | Tạo save mặc định khi chưa có dữ liệu | Critical | Smoke | Integration | EditMode |
| TC-SAVE-002 | Lưu và tải coins, hints, inventory | Critical | Smoke | Integration | EditMode |
| TC-SAVE-003 | Lưu completion và best result | Critical | Smoke | Integration | EditMode |
| TC-SAVE-004 | Reset DailyDropCounts khi đổi local date | Critical | Smoke | Integration | EditMode |

- [ ] **Step 3: Create the Progression cases**

| ID | Title | Priority | Suite | Level | Mode |
|---|---|---|---|---|---|
| TC-PROGRESSION-001 | Tranh initially unlocked có thể được chọn | Critical | Smoke | Integration | EditMode |
| TC-PROGRESSION-002 | Tranh khóa hiển thị required Key Items | Critical | Smoke | Integration | EditMode |
| TC-PROGRESSION-003 | Đủ Key Items chuyển tranh sang ReadyToUnlock | Critical | Smoke | Integration | EditMode |
| TC-PROGRESSION-004 | Unlock tranh atomically và persist | Critical | Smoke | Integration | EditMode |
| TC-PROGRESSION-005 | Unlock không consume Key Items | Critical | Smoke | Unit | EditMode |
| TC-PROGRESSION-006 | Sequential policy mở difficulty sau khi clear | Critical | Smoke | Integration | EditMode |
| TC-PROGRESSION-007 | First-clear reward chỉ cấp một lần | Critical | Smoke | Integration | EditMode |

- [ ] **Step 4: Create the Daily Drop cases**

| ID | Title | Priority | Suite | Level | Mode |
|---|---|---|---|---|---|
| TC-DROP-001 | Mỗi active drop entry roll độc lập | Critical | Smoke | Unit | EditMode |
| TC-DROP-002 | Drop rate decay theo số lần item đã drop trong ngày | Critical | Smoke | Unit | EditMode |
| TC-DROP-003 | Replay reward cấp coins và hints đúng amount | Critical | Smoke | Integration | EditMode |
| TC-DROP-004 | Không drop Key Item đã sở hữu | Critical | Smoke | Unit | EditMode |
| TC-DROP-005 | Chỉ tăng DailyDropCounts khi grant thành công | Critical | Smoke | Integration | EditMode |

- [ ] **Step 5: Create the Collection cases**

| ID | Title | Priority | Suite | Level | Mode |
|---|---|---|---|---|---|
| TC-COLLECTION-001 | Mở Collection từ Home | Critical | Smoke | End-to-End | PlayMode |
| TC-COLLECTION-002 | Chỉ hiển thị Key Items đã sở hữu | Critical | Smoke | Integration | EditMode |
| TC-COLLECTION-003 | Điều hướng tới tranh đã mở khóa từ source | Critical | Smoke | End-to-End | PlayMode |

- [ ] **Step 6: Create the Error Handling cases**

| ID | Title | Priority | Suite | Level | Mode |
|---|---|---|---|---|---|
| TC-ERROR-001 | Chặn chọn tranh đang khóa | Critical | Smoke | Integration | EditMode |
| TC-ERROR-002 | Chặn chọn difficulty chưa mở khóa | Critical | Smoke | Integration | EditMode |
| TC-ERROR-003 | Unlock thất bại không thay đổi save | Critical | Smoke | Unit | EditMode |
| TC-ERROR-004 | Lặp thao tác complete không cấp duplicate reward | Critical | Smoke | Integration | PlayMode |
| TC-ERROR-005 | Static data không hợp lệ dừng load với lỗi rõ ràng | Critical | Smoke | Integration | EditMode |

- [ ] **Step 7: Run structural counts before exporter implementation**

Run:

```powershell
$files = Get-ChildItem -LiteralPath 'docs/test-cases' -Filter '*.md' | Where-Object Name -ne 'README.md'
$ids = $files | ForEach-Object { Select-String -LiteralPath $_.FullName -Pattern '^## TC-[A-Z]+-\d{3}:' }
"FILES=$($files.Count)"
"CASES=$($ids.Count)"
$ids.Line
```

Expected:

```text
FILES=6
CASES=31
```

- [ ] **Step 8: Check source formatting**

Run:

```powershell
git diff --check -- docs/test-cases
$moduleFiles = Get-ChildItem -LiteralPath 'docs/test-cases' -Filter '*.md' | Where-Object Name -ne 'README.md'
$requiredPatterns = @(
    '\*\*Case Status:\*\*',
    '### Preconditions',
    '### Test Data',
    '### Steps',
    '### Automation Notes'
)
foreach ($file in $moduleFiles) {
    foreach ($pattern in $requiredPatterns) {
        if (-not (Select-String -LiteralPath $file.FullName -Pattern $pattern -Quiet)) {
            throw "Missing '$pattern' in $($file.FullName)"
        }
    }
}
```

Expected: `git diff --check` succeeds and the structural loop throws no error.

---

### Task 3: Build the Markdown Parser and Validator with TDD

**Files:**

- Create: `tools/test-cases/export_test_cases_to_excel.mjs`
- Create: `tools/test-cases/test/export_test_cases_to_excel.test.mjs`
- Create: `tools/test-cases/test/fixtures/valid/README.md`
- Create: `tools/test-cases/test/fixtures/valid/gameplay.md`

- [ ] **Step 1: Create a minimal valid fixture**

The fixture README contains schema version `1` and one Gameplay module. The fixture module contains one complete `TC-GAMEPLAY-001` case with `Case Status: Active`, `Automation Status: Planned`, `Execution Mode: PlayMode`, and `NUnit Test: none`.

- [ ] **Step 2: Write failing parser tests**

Use Node built-in tests and import these named exports:

```javascript
import test from "node:test";
import assert from "node:assert/strict";
import {
  parseLivingTestPlan,
  parseModuleFile,
  validateTestPlan,
  normalizeWorksheetName,
} from "../export_test_cases_to_excel.mjs";
```

Add focused tests proving:

```javascript
test("parseLivingTestPlan reads schema version and ordered modules", async () => {});
test("parseModuleFile reads metadata, preconditions, test data, steps, and notes", async () => {});
test("validateTestPlan rejects duplicate IDs", () => {});
test("validateTestPlan rejects an ID prefix that does not match the module", () => {});
test("validateTestPlan rejects missing required metadata", () => {});
test("validateTestPlan rejects non-sequential steps", () => {});
test("validateTestPlan enforces Automated mapping rules", () => {});
test("validateTestPlan enforces Planned execution mode", () => {});
test("validateTestPlan enforces Manual Only rules", () => {});
test("validateTestPlan enforces Deprecated retirement notes", () => {});
test("normalizeWorksheetName removes invalid characters and preserves uniqueness", () => {});
```

Create malformed data in memory or under the test process temporary directory; do not add one fixture directory per error.

- [ ] **Step 3: Run tests and verify they fail**

Run with the bundled Node.js executable:

```powershell
& 'C:\Users\Hoang.H\.cache\codex-runtimes\codex-primary-runtime\dependencies\node\bin\node.exe' --test 'tools/test-cases/test/export_test_cases_to_excel.test.mjs'
```

Expected: FAIL because the exporter module or named exports do not exist.

- [ ] **Step 4: Implement parser constants and data shapes**

Define:

```javascript
export const SCHEMA_VERSION = 1;
export const REQUIRED_METADATA = [
  "Module",
  "Feature",
  "Case Status",
  "Priority",
  "Test Suite",
  "Test Level",
  "Automation Status",
  "Execution Mode",
  "NUnit Test",
];
```

Represent a parsed case as:

```javascript
{
  id,
  title,
  sourceFile,
  module,
  feature,
  caseStatus,
  priority,
  testSuite,
  testLevel,
  automationStatus,
  executionMode,
  nunitTests,
  preconditions,
  testData,
  steps,
  automationNotes,
}
```

- [ ] **Step 5: Implement deterministic Markdown parsing**

Implement:

```javascript
export async function parseLivingTestPlan(readmePath) {}
export async function parseModuleFile(moduleDefinition, baseDirectory) {}
```

Parsing rules:

- UTF-8 input only.
- `README.md` schema metadata matches `- **Schema Version:** <integer>`.
- Module registry is the first table under `## Modules`.
- Test cases begin only at headings matching `^## (TC-[A-Z]+-\d{3}): (.+)$`.
- Metadata lines match `^- \*\*(.+):\*\* (.*)$`.
- Section names must appear exactly once and in canonical order.
- Test Data and Steps use pipe tables with canonical headers.
- Semicolon-separated NUnit mappings are trimmed and empty segments are rejected.

- [ ] **Step 6: Implement validation with aggregated errors**

Implement:

```javascript
export function validateTestPlan(plan) {
  const errors = [];
  // append every deterministic validation failure
  return errors;
}
```

Each error includes `sourceFile`, test case ID when available, field/section, and message. Validate every rule in the approved design, including schema version, module filename uniqueness, allowed enums, ID prefix, case status, automation combinations, tables, and sequential steps.

- [ ] **Step 7: Implement worksheet-name normalization**

Implement:

```javascript
export function normalizeWorksheetName(rawName, usedNames) {}
```

Rules:

- replace `: \ / ? * [ ]` with spaces;
- collapse whitespace;
- trim apostrophes and spaces;
- use `Module` if the result is empty;
- limit to 31 characters;
- append ` (2)`, ` (3)`, and so on while preserving the 31-character limit;
- compare uniqueness case-insensitively.

- [ ] **Step 8: Run parser tests**

Run:

```powershell
& 'C:\Users\Hoang.H\.cache\codex-runtimes\codex-primary-runtime\dependencies\node\bin\node.exe' --test 'tools/test-cases/test/export_test_cases_to_excel.test.mjs'
```

Expected: all parser and validator tests pass.

---

### Task 4: Build and Verify the Excel Workbook

**Files:**

- Modify: `tools/test-cases/export_test_cases_to_excel.mjs`
- Modify: `tools/test-cases/test/export_test_cases_to_excel.test.mjs`

- [ ] **Step 1: Write failing workbook integration tests**

Add tests for:

```javascript
test("buildWorkbook creates Summary, All Test Cases, and module sheets", async () => {});
test("exportWorkbook does not replace an existing file when validation fails", async () => {});
test("exportWorkbook replaces the destination after successful export", async () => {});
```

Use temporary directories from `node:os` and `node:fs/promises`. The valid fixture produces exactly three sheets: `Summary`, `All Test Cases`, and `Gameplay`.

- [ ] **Step 2: Run tests and verify workbook tests fail**

Run the test command from Task 3.

Expected: parser tests pass; workbook tests fail because workbook functions do not exist.

- [ ] **Step 3: Implement workbook construction**

Import:

```javascript
import {
  FileBlob,
  SpreadsheetFile,
  Workbook,
} from "@oai/artifact-tool";
```

Export:

```javascript
export async function buildWorkbook(plan) {}
export async function verifyWorkbook(workbook, plan) {}
export async function exportWorkbook(options) {}
```

Create sheets in this order:

1. `Summary`
2. `All Test Cases`
3. module sheets in README order

The All Test Cases and module columns are:

```text
ID
Title
Module
Feature
Case Status
Priority
Test Suite
Test Level
Automation Status
Execution Mode
NUnit Test
Preconditions
Test Data
Steps
Expected Results
Automation Notes
Source File
```

Flatten lists with `\n`. Flatten steps as numbered actions and expected results into separate multiline cells while preserving matching order.

- [ ] **Step 4: Apply workbook formatting**

Apply:

- dark blue title/header fill with white bold text;
- frozen header row on data sheets;
- filters through real Excel tables;
- wrapped top-aligned long-text columns;
- bounded widths: IDs/status fields 14-22, title/feature 28-36, steps/results/notes 48-60;
- alternating table rows;
- status colors for `Active` and `Deprecated`;
- priority colors for `Critical`, `High`, `Medium`, and `Low`;
- automation colors for `Automated`, `Planned`, `Manual Only`, and `Not Applicable`;
- gridlines hidden on Summary only.

Summary contains:

- title and schema version;
- total active/deprecated cases;
- one compact count table each for module, priority, suite, level, and automation status;
- a native column chart for active cases by module.

- [ ] **Step 5: Implement workbook verification**

Use artifact-tool inspection to verify:

- sheet names and count match the parsed plan;
- `All Test Cases` row count equals parsed case count plus header;
- first and last IDs match the ordered source;
- no formula error strings occur;
- module sheets contain only their module's cases.

Render every sheet to a disposable preview directory before final export. Fail on an empty render or export exception. Preview PNGs are verification artifacts and are not committed.

- [ ] **Step 6: Implement atomic export**

The export flow is:

```text
parse all Markdown
validate all Markdown
build workbook
inspect workbook
render all sheets
export to destination-directory temporary .xlsx
rename temporary file over destination
clean temporary file on failure
```

Never remove the existing destination before the temporary workbook has been exported successfully.

- [ ] **Step 7: Run workbook tests**

Before running, create a disposable local module junction:

```powershell
$runtimeModules = 'C:\Users\Hoang.H\.cache\codex-runtimes\codex-primary-runtime\dependencies\node\node_modules'
$localModules = 'tools/test-cases/node_modules'
if (-not (Test-Path -LiteralPath $localModules)) {
    New-Item -ItemType Junction -Path $localModules -Target $runtimeModules | Out-Null
}
& 'C:\Users\Hoang.H\.cache\codex-runtimes\codex-primary-runtime\dependencies\node\bin\node.exe' --test 'tools/test-cases/test/export_test_cases_to_excel.test.mjs'
```

Expected: all parser, validator, workbook, and atomic replacement tests pass.

Remove the disposable junction after tests:

```powershell
Remove-Item -LiteralPath 'tools/test-cases/node_modules'
```

Verify the resolved target is the bundled runtime path before removal.

---

### Task 5: Add the Manual PowerShell Entrypoint

**Files:**

- Create: `tools/test-cases/export_test_cases_to_excel.ps1`
- Modify: `tools/test-cases/export_test_cases_to_excel.mjs`

- [ ] **Step 1: Add CLI handling to the Node exporter**

Support:

```text
--source <docs/test-cases>
--output <xlsx path>
--preview-dir <directory>
```

Return exit code `0` on success and nonzero on parse, validation, verification, or export failure. Print concise errors to stderr and a final success line containing case count, sheet count, and output path.

- [ ] **Step 2: Implement explicit PowerShell parameters**

Use:

```powershell
param(
    [Parameter(Mandatory = $true)]
    [string]$NodePath,

    [Parameter(Mandatory = $true)]
    [string]$NodeModulesPath,

    [string]$SourcePath = (Join-Path $PSScriptRoot '..\..\docs\test-cases'),

    [string]$OutputPath = (Join-Path $PSScriptRoot '..\..\docs\test-cases\generated\jigsaw-vina-test-cases.xlsx'),

    [switch]$ValidateOnly
)
```

Resolve all paths and fail clearly when the Node executable, module directory, source README, or exporter module is absent.

- [ ] **Step 3: Implement disposable runtime setup**

The wrapper:

1. creates a unique directory under `[System.IO.Path]::GetTempPath()`;
2. creates a `node_modules` junction to the supplied bundled module path;
3. copies the `.mjs` exporter into the temporary directory;
4. invokes the supplied Node executable with resolved source/output/preview paths;
5. propagates the Node exit code;
6. removes only the verified unique temporary directory in `finally`.

Before recursive removal, resolve the temporary path and verify it starts with the OS temp directory plus the exporter-specific prefix.

- [ ] **Step 4: Run the wrapper against an invalid temporary source**

Invoke with the current bundled paths and a temporary source whose README has `Schema Version: 999`.

Expected: nonzero exit, clear unsupported schema error, and no workbook created or replaced.

- [ ] **Step 5: Run the wrapper against the valid fixture**

Run:

```powershell
& 'tools/test-cases/export_test_cases_to_excel.ps1' `
  -NodePath 'C:\Users\Hoang.H\.cache\codex-runtimes\codex-primary-runtime\dependencies\node\bin\node.exe' `
  -NodeModulesPath 'C:\Users\Hoang.H\.cache\codex-runtimes\codex-primary-runtime\dependencies\node\node_modules' `
  -SourcePath 'tools/test-cases/test/fixtures/valid' `
  -OutputPath (Join-Path $env:TEMP 'jigsaw-vina-test-case-fixture.xlsx')
```

Expected: exit `0`, one case exported, three sheets reported, and the workbook exists.

---

### Task 6: Validate the Real Living Test Plan Without Publishing Excel

**Files:**

- No source changes expected unless validation exposes a documentation defect.

- [ ] **Step 1: Run all exporter unit and integration tests**

Use the bundled runtime junction procedure from Task 4.

Expected: all tests pass.

- [ ] **Step 2: Run parser and validation only against real Markdown**

Add and invoke a CLI `--validate-only` option:

```powershell
& 'tools/test-cases/export_test_cases_to_excel.ps1' `
  -NodePath 'C:\Users\Hoang.H\.cache\codex-runtimes\codex-primary-runtime\dependencies\node\bin\node.exe' `
  -NodeModulesPath 'C:\Users\Hoang.H\.cache\codex-runtimes\codex-primary-runtime\dependencies\node\node_modules' `
  -SourcePath 'docs/test-cases' `
  -ValidateOnly
```

Expected:

```text
VALID: schema=1 modules=6 cases=31
```

No `.xlsx` is created because the user has not requested an export.

- [ ] **Step 3: Verify coverage and IDs**

Run:

```powershell
$moduleFiles = Get-ChildItem -LiteralPath 'docs/test-cases' -Filter '*.md' | Where-Object Name -ne 'README.md'
$ids = $moduleFiles | ForEach-Object { Select-String -LiteralPath $_.FullName -Pattern '^## (TC-[A-Z]+-\d{3}):' }
$idValues = foreach ($match in $ids) {
    [regex]::Match($match.Line, '^## (TC-[A-Z]+-\d{3}):').Groups[1].Value
}
$duplicates = $idValues | Group-Object | Where-Object Count -gt 1
"MODULES=$($moduleFiles.Count)"
"CASES=$($ids.Count)"
"DUPLICATES=$($duplicates.Count)"
```

Expected:

```text
MODULES=6
CASES=31
DUPLICATES=0
```

- [ ] **Step 4: Run final source checks**

Run:

```powershell
git diff --check
git status --short
```

Expected: no whitespace errors. Review status to ensure no generated `.xlsx`, preview PNG, runtime junction, or scratch directory is present.

Unity validation: not run - test documentation and non-Unity exporter tooling do not affect Unity execution.

---

### Task 7: Update Project Tracking and Handoff

**Files:**

- Modify: `docs/plans/task.md`
- Modify: `docs/plans/current-handoff.md`

- [ ] **Step 1: Update task status**

Add a completed task row for:

```text
Task 46: Markdown Living Test Plan & Manual Excel Exporter
```

Note six modules, 31 initial Smoke/Critical-priority cases, a documented Regression backlog, validated schema, and manual exporter.

Move the next pending work to:

```text
Map approved Planned test cases to existing NUnit coverage, then add only missing targeted tests.
```

- [ ] **Step 2: Update current handoff**

Record:

- files and module coverage created;
- parser/exporter test result count;
- real Markdown validation result `schema=1 modules=6 cases=31`;
- Excel not generated because export was not requested;
- Unity validation `not run - not relevant to this change`;
- generated artifacts and runtime links absent;
- current uncommitted scope;
- recommended next task: select the first NUnit mapping batch.

- [ ] **Step 3: Final documentation check**

Run:

```powershell
git diff --check -- docs/plans/task.md docs/plans/current-handoff.md
git status --short
```

Expected: documentation passes whitespace checks and all intended files remain uncommitted.
