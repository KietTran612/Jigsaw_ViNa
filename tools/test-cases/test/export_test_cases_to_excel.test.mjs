import assert from "node:assert/strict";
import { spawn } from "node:child_process";
import fs from "node:fs/promises";
import os from "node:os";
import path from "node:path";
import test from "node:test";
import {
  buildWorkbook,
  exportWorkbook,
  normalizeWorksheetName,
  parseLivingTestPlan,
  parseModuleFile,
  runCli,
  validateTestPlan,
  verifyWorkbook,
} from "../export_test_cases_to_excel.mjs";

const fixtureDirectory = path.resolve(
  "tools/test-cases/test/fixtures/valid",
);

async function loadValidPlan() {
  return parseLivingTestPlan(path.join(fixtureDirectory, "README.md"));
}

function clonePlan(plan) {
  return structuredClone(plan);
}

async function createTemporaryFixture() {
  const directory = await fs.mkdtemp(
    path.join(os.tmpdir(), "jigsaw-parser-fixture-"),
  );
  await fs.cp(fixtureDirectory, directory, { recursive: true });
  return directory;
}

test("parseLivingTestPlan reads schema version and ordered modules", async () => {
  const plan = await loadValidPlan();

  assert.equal(plan.schemaVersion, 1);
  assert.deepEqual(plan.modules, [
    {
      order: 1,
      name: "Gameplay",
      idPrefix: "TC-GAMEPLAY",
      file: "gameplay.md",
      worksheetName: "Gameplay",
    },
  ]);
  assert.equal(plan.cases.length, 1);
});

test("parseModuleFile reads every canonical test case section", async () => {
  const [moduleDefinition] = (await loadValidPlan()).modules;
  const cases = await parseModuleFile(moduleDefinition, fixtureDirectory);

  assert.equal(cases.length, 1);
  assert.deepEqual(cases[0], {
    id: "TC-GAMEPLAY-001",
    title: "Complete a puzzle",
    sourceFile: "gameplay.md",
    module: "Gameplay",
    feature: "Puzzle Completion",
    caseStatus: "Active",
    priority: "Critical",
    testSuite: "Smoke",
    testLevel: "End-to-End",
    automationStatus: "Planned",
    executionMode: "PlayMode",
    nunitTests: [],
    preconditions: [
      "Picture 1 is selected.",
      "Difficulty 0 is unlocked.",
    ],
    testData: [
      { field: "Picture ID", value: "1" },
      { field: "Difficulty ID", value: "0" },
    ],
    steps: [
      {
        number: 1,
        action: "Open Gameplay.",
        expectedResult: "The configured puzzle loads.",
      },
      {
        number: 2,
        action: "Complete every piece.",
        expectedResult: "Reward Summary is shown.",
      },
    ],
    automationNotes: "Planned PlayMode coverage.",
  });
});

test("parseLivingTestPlan rejects malformed test case headings", async () => {
  const directory = await createTemporaryFixture();
  await fs.appendFile(
    path.join(directory, "gameplay.md"),
    "\n## TC-GAMEPLAY-002 Missing colon\n",
    "utf8",
  );

  const plan = await parseLivingTestPlan(path.join(directory, "README.md"));
  const errors = validateTestPlan(plan);

  assert.match(errors.join("\n"), /malformed test case heading/i);
});

test("parseModuleFile preserves escaped pipes in table cells", async () => {
  const directory = await createTemporaryFixture();
  const modulePath = path.join(directory, "gameplay.md");
  const source = await fs.readFile(modulePath, "utf8");
  await fs.writeFile(
    modulePath,
    source.replace("Open Gameplay.", "Open Gameplay \\| fixture."),
    "utf8",
  );

  const [testCase] = await parseModuleFile(
    {
      order: 1,
      name: "Gameplay",
      idPrefix: "TC-GAMEPLAY",
      file: "gameplay.md",
      worksheetName: "Gameplay",
    },
    directory,
  );

  assert.equal(testCase.steps[0].action, "Open Gameplay | fixture.");
});

test("validateTestPlan rejects table rows with the wrong column count", async () => {
  const directory = await createTemporaryFixture();
  const modulePath = path.join(directory, "gameplay.md");
  const source = await fs.readFile(modulePath, "utf8");
  await fs.writeFile(
    modulePath,
    source.replace(
      "| 1 | Open Gameplay. | The configured puzzle loads. |",
      "| 1 | Open Gameplay. | The configured puzzle loads. | Unexpected |",
    ),
    "utf8",
  );

  const plan = await parseLivingTestPlan(path.join(directory, "README.md"));
  const errors = validateTestPlan(plan);

  assert.match(errors.join("\n"), /Steps row.*3 columns/i);
});

test("validateTestPlan rejects extra table headers", async () => {
  const directory = await createTemporaryFixture();
  const modulePath = path.join(directory, "gameplay.md");
  const source = await fs.readFile(modulePath, "utf8");
  await fs.writeFile(
    modulePath,
    source
      .replace(
        "| Field | Value |",
        "| Field | Value | Extra |",
      )
      .replace(
        "|---|---|",
        "|---|---|---|",
      )
      .replace(
        "| Picture ID | 1 |",
        "| Picture ID | 1 | ignored |",
      )
      .replace(
        "| Difficulty ID | 0 |",
        "| Difficulty ID | 0 | ignored |",
      ),
    "utf8",
  );

  const plan = await parseLivingTestPlan(path.join(directory, "README.md"));
  const errors = validateTestPlan(plan);

  assert.match(errors.join("\n"), /Test Data headers must be exactly Field and Value/i);
});

test("validateTestPlan rejects malformed non-pipe table rows", async () => {
  const directory = await createTemporaryFixture();
  const modulePath = path.join(directory, "gameplay.md");
  const source = await fs.readFile(modulePath, "utf8");
  await fs.writeFile(
    modulePath,
    source.replace(
      "| Difficulty ID | 0 |",
      "Difficulty ID | 0 |",
    ),
    "utf8",
  );

  const plan = await parseLivingTestPlan(path.join(directory, "README.md"));
  const errors = validateTestPlan(plan);

  assert.match(errors.join("\n"), /Test Data contains a malformed table row/i);
});

test("validateTestPlan rejects duplicate metadata fields", async () => {
  const directory = await createTemporaryFixture();
  const modulePath = path.join(directory, "gameplay.md");
  const source = await fs.readFile(modulePath, "utf8");
  await fs.writeFile(
    modulePath,
    source.replace(
      "- **Feature:** Puzzle Completion",
      "- **Feature:** Puzzle Completion\n- **Feature:** Duplicate",
    ),
    "utf8",
  );

  const plan = await parseLivingTestPlan(path.join(directory, "README.md"));
  const errors = validateTestPlan(plan);

  assert.match(errors.join("\n"), /metadata 'Feature' appears more than once/i);
});

test("parseLivingTestPlan rejects invalid UTF-8 input", async () => {
  const directory = await createTemporaryFixture();
  await fs.writeFile(
    path.join(directory, "README.md"),
    Buffer.from([0xc3, 0x28]),
  );

  await assert.rejects(
    parseLivingTestPlan(path.join(directory, "README.md")),
    /valid UTF-8/i,
  );
});

test("validateTestPlan rejects duplicate IDs", async () => {
  const plan = await loadValidPlan();
  plan.cases.push(clonePlan(plan).cases[0]);

  const errors = validateTestPlan(plan);

  assert.match(errors.join("\n"), /duplicate test case ID/i);
});

test("validateTestPlan rejects an unsupported schema version", async () => {
  const plan = await loadValidPlan();
  plan.schemaVersion = 999;

  const errors = validateTestPlan(plan);

  assert.match(errors.join("\n"), /Schema Version 999 is unsupported/i);
});

test("validateTestPlan requires sequential module order", async () => {
  const plan = await loadValidPlan();
  plan.modules[0].order = 2;

  const errors = validateTestPlan(plan);

  assert.match(errors.join("\n"), /module order.*sequential/i);
});

test("validateTestPlan requires at least one case in every module", async () => {
  const plan = await loadValidPlan();
  plan.cases = [];

  const errors = validateTestPlan(plan);

  assert.match(errors.join("\n"), /Gameplay.*at least one test case/i);
});

test("validateTestPlan rejects an ID prefix that does not match the module", async () => {
  const plan = await loadValidPlan();
  plan.cases[0].id = "TC-SAVE-001";

  const errors = validateTestPlan(plan);

  assert.match(errors.join("\n"), /ID prefix/i);
});

test("validateTestPlan ties each case to its registry module file", async () => {
  const plan = await loadValidPlan();
  plan.modules.push({
    order: 2,
    name: "Save Load",
    idPrefix: "TC-SAVE",
    file: "save-load.md",
    worksheetName: "Save Load",
  });
  plan.cases[0].id = "TC-SAVE-001";
  plan.cases[0].module = "Save Load";

  const errors = validateTestPlan(plan);

  assert.match(errors.join("\n"), /gameplay\.md.*registry module 'Gameplay'/i);
});

test("validateTestPlan rejects missing required metadata", async () => {
  const plan = await loadValidPlan();
  plan.cases[0].feature = "";

  const errors = validateTestPlan(plan);

  assert.match(errors.join("\n"), /Feature.*required/i);
});

test("validateTestPlan rejects non-sequential steps", async () => {
  const plan = await loadValidPlan();
  plan.cases[0].steps[1].number = 3;

  const errors = validateTestPlan(plan);

  assert.match(errors.join("\n"), /step numbers.*sequential/i);
});

test("validateTestPlan enforces Automated mapping rules", async () => {
  const plan = await loadValidPlan();
  plan.cases[0].automationStatus = "Automated";

  const errors = validateTestPlan(plan);

  assert.match(errors.join("\n"), /Automated.*NUnit/i);
});

test("validateTestPlan enforces Planned execution mode", async () => {
  const plan = await loadValidPlan();
  plan.cases[0].executionMode = "Manual";

  const errors = validateTestPlan(plan);

  assert.match(errors.join("\n"), /Planned.*EditMode.*PlayMode/i);
});

test("validateTestPlan enforces Manual Only rules", async () => {
  const plan = await loadValidPlan();
  plan.cases[0].automationStatus = "Manual Only";
  plan.cases[0].executionMode = "PlayMode";
  plan.cases[0].automationNotes = "";

  const errors = validateTestPlan(plan);

  assert.match(errors.join("\n"), /Manual Only.*Manual/i);
  assert.match(errors.join("\n"), /Manual Only.*Automation Notes/i);
});

test("validateTestPlan enforces Deprecated retirement notes", async () => {
  const plan = await loadValidPlan();
  plan.cases[0].caseStatus = "Deprecated";
  plan.cases[0].automationNotes = "";

  const errors = validateTestPlan(plan);

  assert.match(errors.join("\n"), /Deprecated.*retirement reason/i);
});

test("normalizeWorksheetName removes invalid characters and preserves uniqueness", () => {
  const usedNames = new Set();

  assert.equal(normalizeWorksheetName("Save/Load:*?[]", usedNames), "Save Load");
  assert.equal(normalizeWorksheetName("save load", usedNames), "save load (2)");
  assert.equal(
    normalizeWorksheetName("A".repeat(40), usedNames),
    "A".repeat(31),
  );
});

test("buildWorkbook creates Summary, All Test Cases, and module sheets", async () => {
  const plan = await loadValidPlan();
  assert.deepEqual(validateTestPlan(plan), []);

  const workbook = await buildWorkbook(plan);
  const verification = await verifyWorkbook(workbook, plan, {
    render: false,
  });

  assert.deepEqual(verification.sheetNames, [
    "Summary",
    "All Test Cases",
    "Gameplay",
  ]);
  assert.equal(verification.caseCount, 1);
});

test("verifyWorkbook rejects corrupted workbook content", async () => {
  const plan = await loadValidPlan();
  const workbook = await buildWorkbook(plan);
  workbook.worksheets
    .getItem("All Test Cases")
    .getRange("A2")
    .values = [["BROKEN-ID"]];

  await assert.rejects(
    verifyWorkbook(workbook, plan, { render: false }),
    /All Test Cases.*ID mismatch/i,
  );
});

test("verifyWorkbook rejects appended workbook rows", async () => {
  const plan = await loadValidPlan();
  const workbook = await buildWorkbook(plan);
  workbook.worksheets
    .getItem("All Test Cases")
    .getRange("A3")
    .values = [["EXTRA-ID"]];

  await assert.rejects(
    verifyWorkbook(workbook, plan, { render: false }),
    /All Test Cases row count mismatch/i,
  );
});

test("exportWorkbook does not replace an existing file when validation fails", async () => {
  const directory = await fs.mkdtemp(path.join(os.tmpdir(), "jigsaw-export-"));
  const outputPath = path.join(directory, "cases.xlsx");
  await fs.writeFile(outputPath, "existing", "utf8");

  const plan = await loadValidPlan();
  plan.cases[0].feature = "";

  await assert.rejects(
    exportWorkbook({
      plan,
      outputPath,
      previewDirectory: path.join(directory, "preview"),
      render: false,
    }),
    /validation failed/i,
  );
  assert.equal(await fs.readFile(outputPath, "utf8"), "existing");
});

test("exportWorkbook replaces the destination after successful export", async () => {
  const directory = await fs.mkdtemp(path.join(os.tmpdir(), "jigsaw-export-"));
  const outputPath = path.join(directory, "cases.xlsx");
  await fs.writeFile(outputPath, "existing", "utf8");

  const result = await exportWorkbook({
    plan: await loadValidPlan(),
    outputPath,
    previewDirectory: path.join(directory, "preview"),
    render: false,
  });

  const output = await fs.readFile(outputPath);
  assert.equal(result.caseCount, 1);
  assert.deepEqual(result.sheetNames, [
    "Summary",
    "All Test Cases",
    "Gameplay",
  ]);
  assert.ok(output.length > 100);
  assert.notEqual(output.toString("utf8"), "existing");
  assert.deepEqual(
    (await fs.readdir(directory)).filter((name) => name.endsWith(".ndjson")),
    [],
  );
});

test("runCli validates source without creating a workbook", async () => {
  const output = [];
  const errors = [];

  const exitCode = await runCli(
    ["--source", fixtureDirectory, "--validate-only"],
    {
      stdout: (message) => output.push(message),
      stderr: (message) => errors.push(message),
    },
  );

  assert.equal(exitCode, 0);
  assert.deepEqual(errors, []);
  assert.match(output.join("\n"), /VALID: schema=1 modules=1 cases=1/);
});

test("runCli reports missing required export output", async () => {
  const errors = [];

  const exitCode = await runCli(["--source", fixtureDirectory], {
    stdout: () => {},
    stderr: (message) => errors.push(message),
  });

  assert.equal(exitCode, 1);
  assert.match(errors.join("\n"), /--output is required/i);
});

test("runCli refuses non-xlsx output paths", async () => {
  const errors = [];

  const exitCode = await runCli(
    [
      "--source",
      fixtureDirectory,
      "--output",
      path.join(fixtureDirectory, "gameplay.md"),
    ],
    {
      stdout: () => {},
      stderr: (message) => errors.push(message),
    },
  );

  assert.equal(exitCode, 1);
  assert.match(errors.join("\n"), /output path must end with \.xlsx/i);
});

test(
  "direct CLI rendering exits successfully",
  { timeout: 60_000 },
  async () => {
    const directory = await fs.mkdtemp(
      path.join(os.tmpdir(), "jigsaw-cli-render-"),
    );
    const outputPath = path.join(directory, "cases.xlsx");
    const previewDirectory = path.join(directory, "preview");
    const exporterPath = path.resolve(
      "tools/test-cases/export_test_cases_to_excel.mjs",
    );

    const result = await new Promise((resolve, reject) => {
      const child = spawn(
        process.execPath,
        [
          exporterPath,
          "--source",
          fixtureDirectory,
          "--output",
          outputPath,
          "--preview-dir",
          previewDirectory,
        ],
        { windowsHide: true },
      );
      let stdout = "";
      let stderr = "";
      child.stdout.on("data", (chunk) => {
        stdout += chunk;
      });
      child.stderr.on("data", (chunk) => {
        stderr += chunk;
      });
      child.once("error", reject);
      child.once("close", (code) => resolve({ code, stdout, stderr }));
    });

    assert.equal(result.code, 0, result.stderr);
    assert.match(result.stdout, /EXPORTED: cases=1 sheets=3/);
    assert.ok((await fs.stat(outputPath)).size > 100);
    assert.ok(
      (await fs.stat(path.join(previewDirectory, "01_Summary.png"))).size > 0,
    );
  },
);
