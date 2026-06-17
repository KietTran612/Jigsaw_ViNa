# Map Planned Test Cases to NUnit Coverage Implementation Plan

**Goal:** Map the 31 approved planned test cases defined in `docs/test-cases/` modules to NUnit tests. Verify existing test coverage, identify which cases are already covered, and implement missing NUnit tests only when absolutely necessary (prioritizing small targeted tests). Finally, update covered passing cases to `Automated` with their fully qualified NUnit test names, and keep uncovered cases as `Planned` with gap notes.

**Rationale for Task 49:**
- This is the longest-standing pending item and does not depend on UI assets.
- The project already has the Living Test Plan + exporter tool in place from Task 46, but the NUnit mapping is still missing.
- After the Task 48 refactor, consolidating test coverage is the logical next step before moving on to major feature/UI passes.

**Architecture & Principles:** 
- Align the 6 test modules (`gameplay.md`, `save-load.md`, `progression.md`, `daily-drop.md`, `collection.md`, and `error-handling.md`) with the project's C# NUnit tests (`JigsawVina.Tests` assembly for EditMode and `JigsawVina.PlayModeTests` assembly for PlayMode).
- Only implement missing tests if truly necessary, focusing on small, isolated targeted test cases rather than large integrated blocks.
- **Strict Verification Scope**: Never run the full test suite by default. Only execute the relevant test class/case for verification.
- Update metadata fields (`Automation Status` to `Automated`, `NUnit Test` to `Namespace.Class.Method`) in the Markdown test files.
- Ensure all mapped tests pass cleanly, and the JSON-to-Excel exporter is run only upon explicit request/approval.

---

## Proposed Changes

### Test Plan Documentation

#### [MODIFY] [gameplay.md](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/docs/test-cases/gameplay.md)
#### [MODIFY] [save-load.md](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/docs/test-cases/save-load.md)
#### [MODIFY] [progression.md](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/docs/test-cases/progression.md)
#### [MODIFY] [daily-drop.md](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/docs/test-cases/daily-drop.md)
#### [MODIFY] [collection.md](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/docs/test-cases/collection.md)
#### [MODIFY] [error-handling.md](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/docs/test-cases/error-handling.md)

- Map planned test cases to existing/new C# NUnit tests.
- Update `Automation Status: Automated` only after the test runs and passes.
- Update `NUnit Test: Namespace.Class.Method`. Keep `none` and document gaps for any case not covered.

### NUnit Test Implementations (If Missing)

#### [MODIFY] [JigsawVina/Assets/JigsawVina/Tests/**/*.cs](file:///d:/soflware/Unity/Source/Jigsaw_ViNa/JigsawVina/Assets/JigsawVina/Tests/)
- **Audit Step**: Audit all existing test files under `JigsawVina/Assets/JigsawVina/Tests/` (including `PuzzleGameplayPlayModeTests.cs`, `SaveDataServiceTests.cs`, `ProgressionTests.cs`, `DropRewardTests.cs`, `CollectionFlowTests.cs`, `PictureSelectFlowTests.cs`, `DifficultySelectFlowTests.cs`, `GameSessionServiceTests.cs`, `PuzzleSessionTests.cs`, `StaticDataServiceTests.cs`, and `LifetimeScopeRegistrationTests.cs`) to find existing coverage first and prevent duplicate test code creation.
- **Implement Missing Tests**: Write new test methods only when essential to cover core logic, prioritizing small, isolated targeted test cases.

---

## Detailed Task Breakdown

### Task 49: Map Planned Test Cases to NUnit Coverage

- [ ] **Step 1: Read all 31 test case descriptions in modules**
  - Read `docs/test-cases/gameplay.md`, `docs/test-cases/save-load.md`, `docs/test-cases/progression.md`, `docs/test-cases/daily-drop.md`, `docs/test-cases/collection.md`, and `docs/test-cases/error-handling.md`.

- [ ] **Step 2: Audit existing NUnit coverage and identify gaps**
  - Audit all test files under `JigsawVina/Assets/JigsawVina/Tests/` to find matching automated tests.
  - Document the map of `TC-ID` to NUnit methods in this plan. Keep any case lacking coverage as `Planned` and record the gap.

- [ ] **Step 3: Implement missing NUnit tests (as needed)**
  - Write small, focused test methods only when essential to cover core logic.
  - Compile and run only the newly added/modified tests to verify they pass.

- [ ] **Step 4: Update Markdown files with NUnit mappings**
  - Update `Automation Status` to `Automated` and `NUnit Test` to the fully qualified test name **only after** the NUnit test runs and passes.
  - If a test case is not covered and we decide not to write a new test, keep it as `Planned` with `NUnit Test: none` and document the gap in `Automation Notes`.

- [ ] **Step 5: Run Excel Exporter (Only upon explicit request/approval)**
  - Run the exporter script only if explicitly requested by the user, respecting the manual export policy.

---

## Verification Plan

### Automated Tests
- Compile/log check: wait for compilation and verify there are zero compiler errors; record warnings if any.
- Target verification: Run only the newly added/modified tests or the targeted test classes containing the mapped tests.
- Full EditMode/PlayMode suite execution requires separate user approval.
- Exporter execution: Run the manual PowerShell exporter wrapper only after explicit user approval/request.

### Manual Verification
- If the exporter is approved and run, verify the exported Excel sheet formats and mappings.
