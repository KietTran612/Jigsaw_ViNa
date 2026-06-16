# Game Test Case System Design

## Goal

Create a QA-readable test case system for the current player-facing game flow. Markdown is the single source of truth. A manually invoked PowerShell script exports the Markdown test cases to an Excel workbook when requested.

This dated document is a historical design decision record. It captures the initial architecture and is not the living test plan. Routine coverage and test case updates belong under `docs/test-cases/`.

The first coverage pass prioritizes the `Smoke` suite and `Critical` priority. It covers:

- Home picture selection and unlock flow;
- difficulty selection;
- puzzle gameplay and completion;
- reward summary and return flow;
- save/load behavior;
- progression rules;
- daily drop rewards;
- Collection UI and navigation;
- relevant error and invalid-data behavior.

The Game Data Editor and other editor tooling are outside this scope.

## File Layout

```text
docs/test-cases/
|-- README.md
|-- gameplay.md
|-- save-load.md
|-- progression.md
|-- daily-drop.md
|-- collection.md
|-- error-handling.md
`-- generated/
    `-- jigsaw-vina-test-cases.xlsx

tools/test-cases/
|-- export_test_cases_to_excel.ps1
|-- export_test_cases_to_excel.mjs
`-- test/
    `-- export_test_cases_to_excel.test.mjs
```

`README.md` defines the module order, schema, allowed values, ID rules, and coverage summary. Module files contain the test cases. Files under `generated/` are derived artifacts and must not be edited as source data.

The PowerShell script is the manual entrypoint. It invokes the bundled Node.js runtime and delegates Markdown parsing, validation, workbook construction, inspection, rendering, and `.xlsx` export to the JavaScript module using `@oai/artifact-tool`. The exporter does not depend on Microsoft Excel, Excel COM automation, globally installed Node.js, or third-party PowerShell modules.

The exporter is not connected to CI or file watchers. It runs only when explicitly requested.

## Document Lifecycle

- `docs/plans/2026-06-15-game-test-case-system-design.md` is the historical design decision record. Update it only to correct the recorded decision or document an approved architectural clarification.
- `docs/test-cases/README.md` is the Living Test Plan. It changes as scope, coverage, modules, conventions, and maintenance guidance evolve.
- `docs/test-cases/*.md` files are Living Test Cases. Add, revise, or deprecate cases there as game behavior changes.
- The generated `.xlsx` workbook is a disposable QA artifact. Regenerate it from Markdown when explicitly requested.
- Routine test case maintenance does not require a new dated plan.
- A new dated design or decision plan is required only for a material workflow or schema change, such as changing the source format, ID policy, validation contract, or export architecture.

Git history provides revision history for the Living Test Plan and test cases. Do not create timestamped copies for routine updates.

## Test Case Schema

Each test case uses a level-two heading followed by fixed metadata and sections:

```markdown
## TC-GAMEPLAY-001: Hoàn thành puzzle thành công

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

1. Player đã chọn Picture 1.
2. Difficulty 0 đã mở khóa.

### Test Data

| Field | Value |
|---|---|
| Picture ID | 1 |
| Difficulty ID | 0 |

### Steps

| # | Action | Expected Result |
|---:|---|---|
| 1 | Mở Gameplay scene. | Puzzle tương ứng được tải thành công. |
| 2 | Ghép đúng toàn bộ puzzle pieces. | Puzzle chuyển sang trạng thái hoàn thành. |
| 3 | Chờ win flow kết thúc. | Reward Summary hiển thị và dữ liệu được lưu. |

### Automation Notes

Có thể tự động hóa bằng PlayMode test với puzzle completion helper.
```

The documentation language is Vietnamese. Technical terms, screen names, code identifiers, and NUnit names remain in English.

All schema sections remain present even when they have no applicable content. Use `1. None.` for empty Preconditions, `| None | N/A |` for empty Test Data, and a short explicit reason for empty or non-automatable mappings. This keeps parsing deterministic.

## Allowed Values

| Field | Allowed values |
|---|---|
| Case Status | `Active`, `Deprecated` |
| Priority | `Critical`, `High`, `Medium`, `Low` |
| Test Suite | `Smoke`, `Regression` |
| Test Level | `Unit`, `Integration`, `End-to-End` |
| Automation Status | `Automated`, `Planned`, `Manual Only`, `Not Applicable` |
| Execution Mode | `EditMode`, `PlayMode`, `Manual`, `N/A` |
| NUnit Test | One or more fully qualified `Namespace.Class.Method` values separated by semicolons, or `none` |

Test case ID prefixes are:

- `TC-GAMEPLAY-###`
- `TC-SAVE-###`
- `TC-PROGRESSION-###`
- `TC-DROP-###`
- `TC-COLLECTION-###`
- `TC-ERROR-###`

IDs must be unique and immutable after publication. A retired test case remains in its module file with `Case Status: Deprecated`, preserving its ID and a short retirement reason in Automation Notes. Published test cases are not physically removed during routine maintenance.

## Validation Rules

The exporter must fail without producing a replacement workbook when any source test case is invalid. Errors identify the source file and test case ID when available.

Validation includes:

- required metadata and sections are present;
- IDs are unique and match the module prefix;
- enum fields use allowed values;
- deprecated cases include a retirement reason in Automation Notes;
- each step has a corresponding expected result;
- `Automated` requires a non-`none` NUnit test name and `EditMode` or `PlayMode`;
- `Planned` requires `EditMode` or `PlayMode`;
- `Manual Only` requires `Manual` and an explanation in Automation Notes;
- `Not Applicable` requires `N/A` and `NUnit Test: none`;
- every non-`none` NUnit mapping is a valid fully qualified name;
- test data tables have `Field` and `Value` columns;
- step numbers are positive, unique, and sequential.

The exporter validates source files but never rewrites or repairs Markdown automatically.

`README.md` declares `- **Schema Version:** 1`, using a positive integer value. The exporter supports only known schema versions and fails clearly when the Living Test Plan requires a newer parser.

## Excel Export

The generated workbook contains:

- `Summary`: counts grouped by case status, module, priority, test suite, test level, and automation status;
- `All Test Cases`: one row per test case with filters and a frozen header;
- one worksheet per module in the order declared by `README.md`.

Workbook cells use wrapped text. Preconditions, test data, steps, expected results, and automation notes retain line breaks inside their cells. Priority and automation status use consistent colors. Column widths remain readable without requiring every row to be manually resized.

Worksheet names must satisfy Excel's length and invalid-character restrictions and remain unique after normalization.

The script writes to `docs/test-cases/generated/jigsaw-vina-test-cases.xlsx` by default and may support an explicit output path. It parses and validates all sources before writing. It then writes a temporary workbook in the destination directory and replaces the previous workbook only after the new file is created successfully. A failed validation or export leaves the previous workbook unchanged.

The PowerShell entrypoint accepts explicit bundled Node.js and Node module paths so execution does not hardcode a user profile or runtime version. Codex resolves those paths through the workspace dependency loader when an export is requested. The entrypoint creates only disposable runtime-link/scratch data outside the source folders and removes it after execution.

## NUnit Workflow

The initial test case set is designed independently from existing automated tests. Existing NUnit coverage is not treated as a constraint while defining expected behavior.

When automated test implementation begins:

1. Select approved test cases by priority and automation status.
2. Inspect existing NUnit tests for equivalent coverage.
3. Reuse or map an existing test when it proves the complete test case.
4. Add the narrowest missing EditMode or PlayMode test when coverage is absent.
5. Update `Automation Status` and `NUnit Test` only after the mapped test passes.

The exporter does not generate NUnit source code.

One test case may map to multiple NUnit methods. `NUnit Test` uses a semicolon-separated list for multiple mappings; `none` remains the only empty mapping value.

## Initial Delivery Order

1. Create the schema, README, and module files.
2. Write Smoke and Critical test cases for the full scoped flow.
3. Review the cases for missing transitions, persistence boundaries, and failure behavior.
4. Implement and validate the manual Excel exporter.
5. Export the workbook only when explicitly requested.
6. In a separate task, map selected cases to existing or new NUnit tests.

## Acceptance Criteria

- QA can follow each test without reading implementation code.
- Codex and Antigravity can identify exact preconditions, actions, and expected results.
- The dated design record remains separate from the frequently updated Living Test Plan.
- Deprecated test cases preserve their published IDs and retirement reasons.
- Every step has an explicit expected result.
- Invalid Markdown causes a clear export failure and does not replace a valid workbook.
- Markdown remains the only editable source of truth.
- Excel export occurs only through an explicit manual request.
- No automated NUnit test is claimed by the documentation until its mapping has passed.
