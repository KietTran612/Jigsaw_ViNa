import fs from "node:fs/promises";
import path from "node:path";
import { pathToFileURL } from "node:url";

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

const ALLOWED_VALUES = {
  caseStatus: new Set(["Active", "Deprecated"]),
  priority: new Set(["Critical", "High", "Medium", "Low"]),
  testSuite: new Set(["Smoke", "Regression"]),
  testLevel: new Set(["Unit", "Integration", "End-to-End"]),
  automationStatus: new Set([
    "Automated",
    "Planned",
    "Manual Only",
    "Not Applicable",
  ]),
  executionMode: new Set(["EditMode", "PlayMode", "Manual", "N/A"]),
};

const METADATA_PROPERTIES = {
  Module: "module",
  Feature: "feature",
  "Case Status": "caseStatus",
  Priority: "priority",
  "Test Suite": "testSuite",
  "Test Level": "testLevel",
  "Automation Status": "automationStatus",
  "Execution Mode": "executionMode",
  "NUnit Test": "nunitTestValue",
};

const CASE_HEADING_PATTERN = /^## (TC-[A-Z]+-\d{3}): (.+)$/gm;
const SECTION_NAMES = [
  "Preconditions",
  "Test Data",
  "Steps",
  "Automation Notes",
];

async function readUtf8File(filePath) {
  const bytes = await fs.readFile(filePath);
  try {
    return new TextDecoder("utf-8", { fatal: true }).decode(bytes);
  } catch {
    throw new Error(`${filePath} is not valid UTF-8`);
  }
}

function splitTableRow(line) {
  const content = line.trim().slice(1, -1);
  const cells = [];
  let current = "";

  for (let index = 0; index < content.length; index += 1) {
    const character = content[index];
    if (character === "\\" && content[index + 1] === "|") {
      current += "|";
      index += 1;
    } else if (character === "|") {
      cells.push(current.trim());
      current = "";
    } else {
      current += character;
    }
  }
  cells.push(current.trim());
  return cells;
}

function parseTable(text, sectionName) {
  const lines = text
    .split(/\r?\n/)
    .map((line) => line.trim())
    .filter(Boolean);
  const malformedLines = lines.filter(
    (line) => !line.startsWith("|") || !line.endsWith("|"),
  );
  const rows = lines
    .filter((line) => line.startsWith("|") && line.endsWith("|"))
    .map(splitTableRow);

  if (rows.length < 2) {
    return {
      headers: [],
      rows: [],
      errors: malformedLines.map(
        () => `${sectionName} contains a malformed table row`,
      ),
    };
  }

  const separator = rows[1];
  if (!separator.every((cell) => /^:?-{3,}:?$/.test(cell))) {
    return {
      headers: [],
      rows: [],
      errors: [`${sectionName} table separator is invalid`],
    };
  }

  const headers = rows[0];
  const dataRows = rows.slice(2);
  const errors = malformedLines.map(
    () => `${sectionName} contains a malformed table row`,
  );
  dataRows.forEach((row, index) => {
    if (row.length !== headers.length) {
      errors.push(
        `${sectionName} row ${index + 1} must contain ${headers.length} columns; found ${row.length}`,
      );
    }
  });

  return {
    headers,
    rows: dataRows.filter((row) => row.length === headers.length),
    errors,
  };
}

function extractSection(chunk, name) {
  const pattern = new RegExp(
    `(?:^|\\n)### ${name}\\s*\\r?\\n([\\s\\S]*?)(?=\\r?\\n### |$)`,
  );
  return pattern.exec(chunk)?.[1]?.trim() ?? "";
}

function parseOrderedList(text) {
  return text
    .split(/\r?\n/)
    .map((line) => /^\s*\d+\.\s+(.+?)\s*$/.exec(line)?.[1])
    .filter(Boolean);
}

function parseMetadata(chunk) {
  const metadata = new Map();
  const errors = [];
  for (const match of chunk.matchAll(/^- \*\*(.+?):\*\*\s*(.*?)\s*$/gm)) {
    if (metadata.has(match[1])) {
      errors.push(`metadata '${match[1]}' appears more than once`);
    }
    metadata.set(match[1], match[2]);
  }
  return { metadata, errors };
}

function validateSectionOrder(chunk, sourceFile, id) {
  const errors = [];
  let previousIndex = -1;

  for (const section of SECTION_NAMES) {
    const marker = `### ${section}`;
    const firstIndex = chunk.indexOf(marker);
    const lastIndex = chunk.lastIndexOf(marker);
    if (firstIndex < 0) {
      errors.push(`${sourceFile} [${id}] section '${section}' is required`);
      continue;
    }
    if (firstIndex !== lastIndex) {
      errors.push(`${sourceFile} [${id}] section '${section}' appears more than once`);
    }
    if (firstIndex < previousIndex) {
      errors.push(`${sourceFile} [${id}] sections are not in canonical order`);
    }
    previousIndex = firstIndex;
  }

  return errors;
}

export async function parseLivingTestPlan(readmePath) {
  const source = await readUtf8File(readmePath);
  const schemaMatch = /^- \*\*Schema Version:\*\*\s*(\d+)\s*$/m.exec(source);
  const parseErrors = [];

  if (!schemaMatch) {
    parseErrors.push(`${readmePath} Schema Version is required`);
  }

  const schemaVersion = schemaMatch ? Number(schemaMatch[1]) : Number.NaN;
  const modulesSection =
    /(?:^|\n)## Modules\s*\r?\n([\s\S]*?)(?=\r?\n## |$)/.exec(source)?.[1] ?? "";
  const moduleTable = parseTable(modulesSection, "Modules");
  const expectedHeaders = ["Order", "Module", "ID Prefix", "File"];

  if (
    moduleTable.headers.length !== expectedHeaders.length ||
    !moduleTable.headers.every((header, index) => header === expectedHeaders[index])
  ) {
    parseErrors.push(
      `${readmePath} Modules table headers must be: ${expectedHeaders.join(", ")}`,
    );
  }
  parseErrors.push(
    ...moduleTable.errors.map((message) => `${readmePath} ${message}`),
  );

  const usedWorksheetNames = new Set(["summary", "all test cases"]);
  const modules = moduleTable.rows.map((row) => {
    const [order, name, rawIdPrefix, rawFile] = row;
    const idPrefix = rawIdPrefix?.replaceAll("`", "") ?? "";
    const file = rawFile?.replaceAll("`", "") ?? "";
    return {
      order: Number(order),
      name,
      idPrefix,
      file,
      worksheetName: normalizeWorksheetName(name, usedWorksheetNames),
    };
  });

  const baseDirectory = path.dirname(readmePath);
  const moduleCases = await Promise.all(
    modules.map(async (moduleDefinition) => {
      try {
        const parsedCases = await parseModuleFile(
          moduleDefinition,
          baseDirectory,
        );
        parseErrors.push(...(parsedCases.parseErrors ?? []));
        return parsedCases;
      } catch (error) {
        parseErrors.push(
          `${moduleDefinition.file || readmePath}: ${error.message}`,
        );
        return [];
      }
    }),
  );

  return {
    readmePath,
    baseDirectory,
    schemaVersion,
    modules,
    cases: moduleCases.flat(),
    parseErrors,
  };
}

export async function parseModuleFile(moduleDefinition, baseDirectory) {
  const modulePath = path.join(baseDirectory, moduleDefinition.file);
  const source = await readUtf8File(modulePath);
  const headings = [...source.matchAll(CASE_HEADING_PATTERN)];
  const cases = [];
  const fileParseErrors = [];

  for (const line of source.split(/\r?\n/)) {
    if (/^##\s+TC-/.test(line) && !/^## TC-[A-Z]+-\d{3}: .+$/.test(line)) {
      fileParseErrors.push(
        `${moduleDefinition.file} malformed test case heading: ${line}`,
      );
    }
  }

  for (let index = 0; index < headings.length; index += 1) {
    const heading = headings[index];
    const start = heading.index;
    const end = headings[index + 1]?.index ?? source.length;
    const chunk = source.slice(start, end).trim();
    const id = heading[1];
    const title = heading[2].trim();
    const metadataResult = parseMetadata(chunk);
    const metadata = metadataResult.metadata;
    const parseErrors = validateSectionOrder(
      chunk,
      moduleDefinition.file,
      id,
    );
    parseErrors.push(
      ...metadataResult.errors.map(
        (message) => `${moduleDefinition.file} [${id}] ${message}`,
      ),
    );

    const preconditionsText = extractSection(chunk, "Preconditions");
    const testDataTable = parseTable(
      extractSection(chunk, "Test Data"),
      "Test Data",
    );
    const stepsTable = parseTable(extractSection(chunk, "Steps"), "Steps");
    const automationNotes = extractSection(chunk, "Automation Notes");

    if (
      testDataTable.headers.length > 0 &&
      (testDataTable.headers.length !== 2 ||
        testDataTable.headers[0] !== "Field" ||
        testDataTable.headers[1] !== "Value")
    ) {
      parseErrors.push(
        `${moduleDefinition.file} [${id}] Test Data headers must be exactly Field and Value`,
      );
    }
    parseErrors.push(
      ...testDataTable.errors.map(
        (message) => `${moduleDefinition.file} [${id}] ${message}`,
      ),
      ...stepsTable.errors.map(
        (message) => `${moduleDefinition.file} [${id}] ${message}`,
      ),
    );
    if (
      stepsTable.headers.length > 0 &&
      (stepsTable.headers.length !== 3 ||
        stepsTable.headers[0] !== "#" ||
        stepsTable.headers[1] !== "Action" ||
        stepsTable.headers[2] !== "Expected Result")
    ) {
      parseErrors.push(
        `${moduleDefinition.file} [${id}] Steps headers must be exactly #, Action, Expected Result`,
      );
    }

    const nunitTestValue = metadata.get("NUnit Test") ?? "";
    const nunitTests =
      nunitTestValue === "none"
        ? []
        : nunitTestValue.split(";").map((value) => value.trim());

    const parsedCase = {
      id,
      title,
      sourceFile: moduleDefinition.file,
      module: metadata.get("Module") ?? "",
      feature: metadata.get("Feature") ?? "",
      caseStatus: metadata.get("Case Status") ?? "",
      priority: metadata.get("Priority") ?? "",
      testSuite: metadata.get("Test Suite") ?? "",
      testLevel: metadata.get("Test Level") ?? "",
      automationStatus: metadata.get("Automation Status") ?? "",
      executionMode: metadata.get("Execution Mode") ?? "",
      nunitTests,
      preconditions: parseOrderedList(preconditionsText),
      testData: testDataTable.rows.map(([field = "", value = ""]) => ({
        field,
        value,
      })),
      steps: stepsTable.rows.map(
        ([number = "", action = "", expectedResult = ""]) => ({
          number: Number(number),
          action,
          expectedResult,
        }),
      ),
      automationNotes,
    };

    for (const field of REQUIRED_METADATA) {
      if (!metadata.has(field)) {
        parseErrors.push(
          `${moduleDefinition.file} [${id}] metadata '${field}' is required`,
        );
      }
    }

    Object.defineProperty(parsedCase, "parseErrors", {
      value: parseErrors,
      enumerable: false,
    });
    Object.defineProperties(parsedCase, {
      registryModule: {
        value: moduleDefinition.name,
        enumerable: false,
      },
      registryIdPrefix: {
        value: moduleDefinition.idPrefix,
        enumerable: false,
      },
      registryFile: {
        value: moduleDefinition.file,
        enumerable: false,
      },
    });
    cases.push(parsedCase);
  }

  Object.defineProperty(cases, "parseErrors", {
    value: fileParseErrors,
    enumerable: false,
  });
  return cases;
}

function caseError(testCase, field, message) {
  return `${testCase.sourceFile} [${testCase.id}] ${field}: ${message}`;
}

export function validateTestPlan(plan) {
  const errors = [...(plan.parseErrors ?? [])];

  if (plan.schemaVersion !== SCHEMA_VERSION) {
    errors.push(
      `${plan.readmePath} Schema Version ${plan.schemaVersion} is unsupported; expected ${SCHEMA_VERSION}`,
    );
  }
  if (!Array.isArray(plan.modules) || plan.modules.length === 0) {
    errors.push(`${plan.readmePath} must declare at least one module`);
  }

  const moduleNames = new Set();
  const moduleFiles = new Set();
  const modulePrefixes = new Set();
  const moduleByName = new Map();

  for (const [moduleIndex, moduleDefinition] of (plan.modules ?? []).entries()) {
    if (!Number.isInteger(moduleDefinition.order) || moduleDefinition.order <= 0) {
      errors.push(`${plan.readmePath} module order must be a positive integer`);
    }
    if (moduleDefinition.order !== moduleIndex + 1) {
      errors.push(
        `${plan.readmePath} module order must be sequential from 1 to ${plan.modules.length}`,
      );
    }
    const normalizedName = moduleDefinition.name?.toLowerCase();
    const normalizedFile = moduleDefinition.file?.toLowerCase();
    const normalizedPrefix = moduleDefinition.idPrefix?.toLowerCase();
    if (!normalizedName || moduleNames.has(normalizedName)) {
      errors.push(`${plan.readmePath} module names must be non-empty and unique`);
    }
    if (!normalizedFile || moduleFiles.has(normalizedFile)) {
      errors.push(`${plan.readmePath} module files must be non-empty and unique`);
    }
    if (!normalizedPrefix || modulePrefixes.has(normalizedPrefix)) {
      errors.push(`${plan.readmePath} module ID prefixes must be non-empty and unique`);
    }
    moduleNames.add(normalizedName);
    moduleFiles.add(normalizedFile);
    modulePrefixes.add(normalizedPrefix);
    moduleByName.set(moduleDefinition.name, moduleDefinition);
  }

  const seenIds = new Set();
  const propertyLabels = {
    module: "Module",
    feature: "Feature",
    caseStatus: "Case Status",
    priority: "Priority",
    testSuite: "Test Suite",
    testLevel: "Test Level",
    automationStatus: "Automation Status",
    executionMode: "Execution Mode",
  };

  for (const testCase of plan.cases ?? []) {
    errors.push(...(testCase.parseErrors ?? []));

    if (seenIds.has(testCase.id)) {
      errors.push(caseError(testCase, "ID", "duplicate test case ID"));
    }
    seenIds.add(testCase.id);

    for (const [property, label] of Object.entries(propertyLabels)) {
      if (!testCase[property]?.trim()) {
        errors.push(caseError(testCase, label, "is required"));
      }
    }

    const moduleDefinition = moduleByName.get(testCase.module);
    if (
      testCase.registryModule &&
      testCase.module !== testCase.registryModule
    ) {
      errors.push(
        caseError(
          testCase,
          "Module",
          `${testCase.registryFile} belongs to registry module '${testCase.registryModule}', not '${testCase.module}'`,
        ),
      );
    }
    if (!moduleDefinition) {
      errors.push(caseError(testCase, "Module", "is not declared in README.md"));
    } else if (!testCase.id.startsWith(`${moduleDefinition.idPrefix}-`)) {
      errors.push(
        caseError(
          testCase,
          "ID prefix",
          `must match ${moduleDefinition.idPrefix}`,
        ),
      );
    }

    for (const [property, values] of Object.entries(ALLOWED_VALUES)) {
      if (testCase[property] && !values.has(testCase[property])) {
        errors.push(
          caseError(
            testCase,
            propertyLabels[property] ?? property,
            `invalid value '${testCase[property]}'`,
          ),
        );
      }
    }

    if (!testCase.title?.trim()) {
      errors.push(caseError(testCase, "Title", "is required"));
    }
    if (!testCase.preconditions?.length) {
      errors.push(caseError(testCase, "Preconditions", "requires at least one item"));
    }
    if (!testCase.testData?.length) {
      errors.push(caseError(testCase, "Test Data", "requires at least one row"));
    }
    if (
      testCase.testData?.some(
        ({ field, value }) => !field?.trim() || !value?.trim(),
      )
    ) {
      errors.push(caseError(testCase, "Test Data", "contains an empty Field or Value"));
    }
    if (!testCase.steps?.length) {
      errors.push(caseError(testCase, "Steps", "requires at least one step"));
    } else {
      const sequential = testCase.steps.every(
        (step, index) => step.number === index + 1,
      );
      if (!sequential) {
        errors.push(
          caseError(
            testCase,
            "Steps",
            "step numbers must be positive, unique, and sequential",
          ),
        );
      }
      if (
        testCase.steps.some(
          ({ action, expectedResult }) =>
            !action?.trim() || !expectedResult?.trim(),
        )
      ) {
        errors.push(
          caseError(testCase, "Steps", "each action requires an expected result"),
        );
      }
    }

    if (testCase.nunitTests?.some((value) => value.length === 0)) {
      errors.push(caseError(testCase, "NUnit Test", "contains an empty mapping"));
    }
    if (
      testCase.nunitTests?.some(
        (value) =>
          !/^[A-Za-z_]\w*(?:\.[A-Za-z_]\w*){2,}$/.test(value),
      )
    ) {
      errors.push(
        caseError(testCase, "NUnit Test", "must use fully qualified test names"),
      );
    }

    if (testCase.automationStatus === "Automated") {
      if (!testCase.nunitTests?.length) {
        errors.push(
          caseError(
            testCase,
            "Automation Status",
            "Automated requires at least one NUnit mapping",
          ),
        );
      }
      if (!["EditMode", "PlayMode"].includes(testCase.executionMode)) {
        errors.push(
          caseError(
            testCase,
            "Automation Status",
            "Automated requires EditMode or PlayMode",
          ),
        );
      }
    }
    if (
      testCase.automationStatus === "Planned" &&
      !["EditMode", "PlayMode"].includes(testCase.executionMode)
    ) {
      errors.push(
        caseError(
          testCase,
          "Automation Status",
          "Planned requires EditMode or PlayMode",
        ),
      );
    }
    if (testCase.automationStatus === "Manual Only") {
      if (testCase.executionMode !== "Manual") {
        errors.push(
          caseError(
            testCase,
            "Automation Status",
            "Manual Only requires Manual execution mode",
          ),
        );
      }
      if (!testCase.automationNotes?.trim()) {
        errors.push(
          caseError(
            testCase,
            "Automation Status",
            "Manual Only requires an explanation in Automation Notes",
          ),
        );
      }
    }
    if (testCase.automationStatus === "Not Applicable") {
      if (testCase.executionMode !== "N/A") {
        errors.push(
          caseError(
            testCase,
            "Automation Status",
            "Not Applicable requires N/A execution mode",
          ),
        );
      }
      if (testCase.nunitTests?.length) {
        errors.push(
          caseError(
            testCase,
            "NUnit Test",
            "Not Applicable requires NUnit Test: none",
          ),
        );
      }
    }
    if (
      testCase.caseStatus === "Deprecated" &&
      !testCase.automationNotes?.trim()
    ) {
      errors.push(
        caseError(
          testCase,
          "Case Status",
          "Deprecated requires a retirement reason in Automation Notes",
        ),
      );
    }
    if (!testCase.automationNotes?.trim()) {
      errors.push(caseError(testCase, "Automation Notes", "is required"));
    }
  }

  for (const moduleDefinition of plan.modules ?? []) {
    if (
      !(plan.cases ?? []).some(
        (testCase) => testCase.module === moduleDefinition.name,
      )
    ) {
      errors.push(
        `${moduleDefinition.file} [${moduleDefinition.name}] requires at least one test case`,
      );
    }
  }

  return errors;
}

export function normalizeWorksheetName(rawName, usedNames = new Set()) {
  const normalizedUsedNames = new Set(
    [...usedNames].map((value) => value.toLowerCase()),
  );
  let baseName = String(rawName ?? "")
    .replace(/[:\\/?*\[\]]/g, " ")
    .replace(/\s+/g, " ")
    .replace(/^['\s]+|['\s]+$/g, "");

  if (!baseName) {
    baseName = "Module";
  }
  baseName = baseName.slice(0, 31).trimEnd();

  let candidate = baseName;
  let suffixNumber = 2;
  while (normalizedUsedNames.has(candidate.toLowerCase())) {
    const suffix = ` (${suffixNumber})`;
    candidate = `${baseName.slice(0, 31 - suffix.length).trimEnd()}${suffix}`;
    suffixNumber += 1;
  }

  usedNames.add(candidate);
  return candidate;
}

const DATA_HEADERS = [
  "ID",
  "Title",
  "Module",
  "Feature",
  "Case Status",
  "Priority",
  "Test Suite",
  "Test Level",
  "Automation Status",
  "Execution Mode",
  "NUnit Test",
  "Preconditions",
  "Test Data",
  "Steps",
  "Expected Results",
  "Automation Notes",
  "Source File",
];

const HEADER_FORMAT = {
  fill: "#1F4E78",
  font: { bold: true, color: "#FFFFFF" },
  verticalAlignment: "center",
  wrapText: true,
};

const PRIORITY_COLORS = {
  Critical: "#F4CCCC",
  High: "#FCE5CD",
  Medium: "#FFF2CC",
  Low: "#D9EAD3",
};

const AUTOMATION_COLORS = {
  Automated: "#D9EAD3",
  Planned: "#D9EAF7",
  "Manual Only": "#FCE5CD",
  "Not Applicable": "#E7E6E6",
};

const CASE_STATUS_COLORS = {
  Active: "#D9EAD3",
  Deprecated: "#E7E6E6",
};

async function loadArtifactTool() {
  return import("@oai/artifact-tool");
}

function flattenNumbered(values) {
  return values.map((value, index) => `${index + 1}. ${value}`).join("\n");
}

function flattenTestData(rows) {
  return rows.map(({ field, value }) => `${field}: ${value}`).join("\n");
}

function flattenSteps(steps, property) {
  return steps
    .map((step) => `${step.number}. ${step[property]}`)
    .join("\n");
}

function caseToRow(testCase) {
  return [
    testCase.id,
    testCase.title,
    testCase.module,
    testCase.feature,
    testCase.caseStatus,
    testCase.priority,
    testCase.testSuite,
    testCase.testLevel,
    testCase.automationStatus,
    testCase.executionMode,
    testCase.nunitTests.length ? testCase.nunitTests.join("; ") : "none",
    flattenNumbered(testCase.preconditions),
    flattenTestData(testCase.testData),
    flattenSteps(testCase.steps, "action"),
    flattenSteps(testCase.steps, "expectedResult"),
    testCase.automationNotes,
    testCase.sourceFile,
  ];
}

function columnName(index) {
  let value = index + 1;
  let result = "";
  while (value > 0) {
    const remainder = (value - 1) % 26;
    result = String.fromCharCode(65 + remainder) + result;
    value = Math.floor((value - 1) / 26);
  }
  return result;
}

function applyDataSheetFormatting(sheet, rowCount, tableName) {
  const lastColumn = columnName(DATA_HEADERS.length - 1);
  const lastRow = Math.max(rowCount + 1, 2);
  const fullRange = sheet.getRange(`A1:${lastColumn}${lastRow}`);
  const headerRange = sheet.getRange(`A1:${lastColumn}1`);

  headerRange.format = HEADER_FORMAT;
  fullRange.format.verticalAlignment = "top";
  fullRange.format.wrapText = true;
  fullRange.format.borders = {
    preset: "all",
    style: "thin",
    color: "#D9E2F3",
  };
  sheet.freezePanes.freezeRows(1);

  const widths = [
    18, 34, 18, 28, 16, 14, 14, 16, 20, 16, 34, 42, 36, 56, 56, 48, 24,
  ];
  widths.forEach((width, index) => {
    sheet.getRange(`${columnName(index)}:${columnName(index)}`).format.columnWidth =
      width;
  });

  const table = sheet.tables.add(
    `A1:${lastColumn}${lastRow}`,
    true,
    tableName,
  );
  table.style = "TableStyleMedium2";
  table.showFilterButton = true;
  table.showBandedRows = true;

  if (rowCount > 0) {
    const statusRange = sheet.getRange(`E2:E${rowCount + 1}`);
    const priorityRange = sheet.getRange(`F2:F${rowCount + 1}`);
    const automationRange = sheet.getRange(`I2:I${rowCount + 1}`);

    for (const [value, color] of Object.entries(CASE_STATUS_COLORS)) {
      statusRange.conditionalFormats.add("containsText", {
        text: value,
        format: { fill: color },
      });
    }
    for (const [value, color] of Object.entries(PRIORITY_COLORS)) {
      priorityRange.conditionalFormats.add("containsText", {
        text: value,
        format: { fill: color },
      });
    }
    for (const [value, color] of Object.entries(AUTOMATION_COLORS)) {
      automationRange.conditionalFormats.add("containsText", {
        text: value,
        format: { fill: color },
      });
    }
  }
}

function countBy(cases, property) {
  const counts = new Map();
  for (const testCase of cases) {
    const key = testCase[property];
    counts.set(key, (counts.get(key) ?? 0) + 1);
  }
  return counts;
}

function writeCountTable(sheet, startRow, title, counts) {
  const entries = [...counts.entries()];
  sheet.getRange(`A${startRow}:B${startRow}`).merge();
  sheet.getRange(`A${startRow}`).values = [[title]];
  sheet.getRange(`A${startRow}:B${startRow}`).format = HEADER_FORMAT;
  sheet.getRange(`A${startRow + 1}:B${startRow + 1}`).values = [
    ["Value", "Count"],
  ];
  sheet.getRange(`A${startRow + 1}:B${startRow + 1}`).format = {
    fill: "#D9EAF7",
    font: { bold: true, color: "#1F1F1F" },
  };

  if (entries.length > 0) {
    sheet.getRange(
      `A${startRow + 2}:B${startRow + entries.length + 1}`,
    ).values = entries;
  }
  return startRow + entries.length + 3;
}

function safeTableName(value, suffix) {
  const cleaned = value.replace(/[^A-Za-z0-9_]/g, "");
  return `TestCases${cleaned || "Module"}${suffix}`;
}

export async function buildWorkbook(plan) {
  const { Workbook } = await loadArtifactTool();
  const workbook = Workbook.create();
  const activeCases = plan.cases.filter(
    (testCase) => testCase.caseStatus === "Active",
  );

  const summarySheet = workbook.worksheets.add("Summary");
  summarySheet.showGridLines = false;
  summarySheet.getRange("A1:H1").merge();
  summarySheet.getRange("A1").values = [["Jigsaw ViNa Test Case Summary"]];
  summarySheet.getRange("A1:H1").format = {
    fill: "#17365D",
    font: { bold: true, color: "#FFFFFF", size: 16 },
    horizontalAlignment: "center",
    verticalAlignment: "center",
  };
  summarySheet.getRange("A2:B4").values = [
    ["Schema Version", plan.schemaVersion],
    ["Total Cases", plan.cases.length],
    ["Active Cases", activeCases.length],
  ];
  summarySheet.getRange("D2:E2").values = [["Deprecated Cases", plan.cases.length - activeCases.length]];
  summarySheet.getRange("A2:A4").format.font = { bold: true };
  summarySheet.getRange("D2").format.font = { bold: true };
  summarySheet.getRange("A:A").format.columnWidth = 28;
  summarySheet.getRange("B:B").format.columnWidth = 14;

  let summaryRow = 6;
  summaryRow = writeCountTable(
    summarySheet,
    summaryRow,
    "Cases by Status",
    countBy(plan.cases, "caseStatus"),
  );
  const moduleCounts = new Map(
    plan.modules.map((moduleDefinition) => [
      moduleDefinition.name,
      plan.cases.filter(
        (testCase) => testCase.module === moduleDefinition.name,
      ).length,
    ]),
  );
  summaryRow = writeCountTable(
    summarySheet,
    summaryRow,
    "Cases by Module",
    moduleCounts,
  );
  const activeModuleCounts = new Map(
    plan.modules.map((moduleDefinition) => [
      moduleDefinition.name,
      activeCases.filter(
        (testCase) => testCase.module === moduleDefinition.name,
      ).length,
    ]),
  );
  const moduleTableStart = summaryRow;
  summaryRow = writeCountTable(
    summarySheet,
    summaryRow,
    "Active Cases by Module",
    activeModuleCounts,
  );
  summaryRow = writeCountTable(
    summarySheet,
    summaryRow,
    "Cases by Priority",
    countBy(plan.cases, "priority"),
  );
  summaryRow = writeCountTable(
    summarySheet,
    summaryRow,
    "Cases by Test Suite",
    countBy(plan.cases, "testSuite"),
  );
  summaryRow = writeCountTable(
    summarySheet,
    summaryRow,
    "Cases by Test Level",
    countBy(plan.cases, "testLevel"),
  );
  writeCountTable(
    summarySheet,
    summaryRow,
    "Cases by Automation Status",
    countBy(plan.cases, "automationStatus"),
  );

  if (activeModuleCounts.size > 0) {
    const chartRange = summarySheet.getRange(
      `A${moduleTableStart + 1}:B${moduleTableStart + activeModuleCounts.size + 1}`,
    );
    const chart = summarySheet.charts.add("bar", chartRange);
    chart.title = "Active Cases by Module";
    chart.hasLegend = false;
    chart.yAxis = {
      numberFormatCode: "0",
      min: 0,
      majorUnit: 1,
    };
    chart.setPosition("D6", "K22");
  }

  const allCasesSheet = workbook.worksheets.add("All Test Cases");
  const allRows = plan.cases.map(caseToRow);
  allCasesSheet.getRange("A1").write([DATA_HEADERS, ...allRows]);
  applyDataSheetFormatting(
    allCasesSheet,
    allRows.length,
    "AllTestCasesTable",
  );

  for (const [moduleIndex, moduleDefinition] of plan.modules.entries()) {
    const sheet = workbook.worksheets.add(moduleDefinition.worksheetName);
    const moduleRows = plan.cases
      .filter((testCase) => testCase.module === moduleDefinition.name)
      .map(caseToRow);
    sheet.getRange("A1").write([DATA_HEADERS, ...moduleRows]);
    applyDataSheetFormatting(
      sheet,
      moduleRows.length,
      safeTableName(moduleDefinition.name, moduleIndex + 1),
    );
  }

  return workbook;
}

async function saveRender(blob, outputPath) {
  const bytes = new Uint8Array(await blob.arrayBuffer());
  if (bytes.length === 0) {
    throw new Error(`Rendered preview is empty: ${outputPath}`);
  }
  await fs.writeFile(outputPath, bytes);
}

export async function verifyWorkbook(
  workbook,
  plan,
  { render = true, previewDirectory } = {},
) {
  const expectedSheetNames = [
    "Summary",
    "All Test Cases",
    ...plan.modules.map((moduleDefinition) => moduleDefinition.worksheetName),
  ];
  const sheetNames = workbook.worksheets.items.map((sheet) => sheet.name);

  if (
    sheetNames.length !== expectedSheetNames.length ||
    sheetNames.some((name, index) => name !== expectedSheetNames[index])
  ) {
    throw new Error(
      `Workbook sheet order mismatch. Expected ${expectedSheetNames.join(", ")}; received ${sheetNames.join(", ")}`,
    );
  }

  const allCasesSheet = workbook.worksheets.getItem("All Test Cases");
  const allValues = allCasesSheet.getUsedRange().values;
  if (allValues.length !== plan.cases.length + 1) {
    throw new Error(
      `All Test Cases row count mismatch. Expected ${plan.cases.length + 1}; received ${allValues.length}`,
    );
  }
  if (
    DATA_HEADERS.some(
      (header, index) => allValues[0]?.[index] !== header,
    )
  ) {
    throw new Error("All Test Cases header mismatch");
  }
  plan.cases.forEach((testCase, index) => {
    const row = allValues[index + 1];
    if (row?.[0] !== testCase.id) {
      throw new Error(
        `All Test Cases ID mismatch at row ${index + 2}. Expected ${testCase.id}; received ${row?.[0]}`,
      );
    }
    if (row?.[2] !== testCase.module) {
      throw new Error(
        `All Test Cases module mismatch for ${testCase.id}. Expected ${testCase.module}; received ${row?.[2]}`,
      );
    }
  });

  for (const moduleDefinition of plan.modules) {
    const expectedCases = plan.cases.filter(
      (testCase) => testCase.module === moduleDefinition.name,
    );
    const moduleValues = workbook.worksheets
      .getItem(moduleDefinition.worksheetName)
      .getUsedRange()
      .values;
    if (moduleValues.length !== expectedCases.length + 1) {
      throw new Error(
        `${moduleDefinition.worksheetName} row count mismatch. Expected ${expectedCases.length + 1}; received ${moduleValues.length}`,
      );
    }
    expectedCases.forEach((testCase, index) => {
      const row = moduleValues[index + 1];
      if (row?.[0] !== testCase.id || row?.[2] !== moduleDefinition.name) {
        throw new Error(
          `${moduleDefinition.worksheetName} content mismatch at row ${index + 2}`,
        );
      }
    });
  }

  await workbook.inspect({
    kind: "table",
    range: `All Test Cases!A1:Q${Math.max(plan.cases.length + 1, 2)}`,
    include: "values,formulas",
    tableMaxRows: Math.min(plan.cases.length + 1, 10),
    tableMaxCols: DATA_HEADERS.length,
    maxChars: 6000,
  });
  const formulaErrors = await workbook.inspect({
    kind: "match",
    searchTerm: "#REF!|#DIV/0!|#VALUE!|#NAME\\?|#N/A",
    options: { useRegex: true, maxResults: 100 },
    summary: "test case workbook formula error scan",
  });
  if (/"matchCount"\s*:\s*[1-9]/.test(formulaErrors.ndjson ?? "")) {
    throw new Error("Workbook contains formula errors");
  }

  if (render) {
    if (!previewDirectory) {
      throw new Error("previewDirectory is required when rendering");
    }
    await fs.mkdir(previewDirectory, { recursive: true });
    for (const [sheetIndex, sheetName] of sheetNames.entries()) {
      const preview = await workbook.render({
        sheetName,
        autoCrop: "all",
        scale: 1,
        format: "png",
      });
      const safeName = `${String(sheetIndex + 1).padStart(2, "0")}_${sheetName.replace(/[^A-Za-z0-9_-]+/g, "_")}`;
      await saveRender(preview, path.join(previewDirectory, `${safeName}.png`));
    }
  }

  return {
    sheetNames,
    caseCount: plan.cases.length,
  };
}

async function replaceAtomically(temporaryPath, outputPath) {
  await fs.rename(temporaryPath, outputPath);
}

export async function exportWorkbook({
  plan,
  outputPath,
  previewDirectory,
  render = true,
}) {
  if (path.extname(outputPath).toLowerCase() !== ".xlsx") {
    throw new Error(`Output path must end with .xlsx: ${outputPath}`);
  }
  const validationErrors = validateTestPlan(plan);
  if (validationErrors.length > 0) {
    throw new Error(
      `Test case validation failed:\n${validationErrors.join("\n")}`,
    );
  }

  const { SpreadsheetFile } = await loadArtifactTool();
  const workbook = await buildWorkbook(plan);
  const verification = await verifyWorkbook(workbook, plan, {
    render,
    previewDirectory,
  });
  const outputDirectory = path.dirname(outputPath);
  const temporaryPath = path.join(
    outputDirectory,
    `.${path.basename(outputPath)}.${process.pid}.${Date.now()}.tmp`,
  );

  await fs.mkdir(outputDirectory, { recursive: true });
  try {
    const output = await SpreadsheetFile.exportXlsx(workbook);
    await output.save(temporaryPath);
    await replaceAtomically(temporaryPath, outputPath);
  } finally {
    await fs.rm(temporaryPath, { force: true });
    await fs.rm(`${temporaryPath}.inspect.ndjson`, { force: true });
  }

  return verification;
}

function parseCliArguments(args) {
  const options = {
    validateOnly: false,
  };

  for (let index = 0; index < args.length; index += 1) {
    const argument = args[index];
    if (argument === "--validate-only") {
      options.validateOnly = true;
      continue;
    }

    if (["--source", "--output", "--preview-dir"].includes(argument)) {
      const value = args[index + 1];
      if (!value || value.startsWith("--")) {
        throw new Error(`${argument} requires a value`);
      }
      const property = {
        "--source": "sourcePath",
        "--output": "outputPath",
        "--preview-dir": "previewDirectory",
      }[argument];
      options[property] = value;
      index += 1;
      continue;
    }

    throw new Error(`Unknown argument: ${argument}`);
  }

  if (!options.sourcePath) {
    throw new Error("--source is required");
  }
  if (!options.validateOnly && !options.outputPath) {
    throw new Error("--output is required unless --validate-only is used");
  }

  return options;
}

export async function runCli(
  args,
  {
    stdout = (message) => console.log(message),
    stderr = (message) => console.error(message),
  } = {},
) {
  try {
    const options = parseCliArguments(args);
    const sourcePath = path.resolve(options.sourcePath);
    const readmePath = path.join(sourcePath, "README.md");
    const plan = await parseLivingTestPlan(readmePath);
    const validationErrors = validateTestPlan(plan);

    if (validationErrors.length > 0) {
      throw new Error(
        `Test case validation failed:\n${validationErrors.join("\n")}`,
      );
    }

    if (options.validateOnly) {
      stdout(
        `VALID: schema=${plan.schemaVersion} modules=${plan.modules.length} cases=${plan.cases.length}`,
      );
      return 0;
    }

    const outputPath = path.resolve(options.outputPath);
    if (path.extname(outputPath).toLowerCase() !== ".xlsx") {
      throw new Error(`Output path must end with .xlsx: ${outputPath}`);
    }
    const previewDirectory = path.resolve(
      options.previewDirectory ??
        path.join(path.dirname(outputPath), ".preview"),
    );
    const result = await exportWorkbook({
      plan,
      outputPath,
      previewDirectory,
      render: true,
    });
    stdout(
      `EXPORTED: cases=${result.caseCount} sheets=${result.sheetNames.length} output=${outputPath}`,
    );
    return 0;
  } catch (error) {
    stderr(error instanceof Error ? error.message : String(error));
    return 1;
  }
}

const isDirectExecution =
  process.argv[1] &&
  pathToFileURL(path.resolve(process.argv[1])).href === import.meta.url;

if (isDirectExecution) {
  process.exit(await runCli(process.argv.slice(2)));
}
