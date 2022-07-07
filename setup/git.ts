import { execFile } from "./cp.js";
import { validateCommit, ValidationResult } from "./validate.js";

async function git(args: string[]) {
  const { stdout, stderr } = await execFile()("git", args);
  if (stderr) throw stderr;
  return stdout;
}

async function getCommit() {
  const commit = (await git(["rev-parse", "HEAD"])).trim();
  return validateCommit(commit);
}

// TODO: Validation for branches... maybe? Probably not.
async function getBranch() {
  return (await git(["rev-parse", "--abbrev-ref", "HEAD"])).trim();
}

async function getSubmoduleCommits() {
  const { stdout } = await execFile()("git", [
    "submodule",
    "foreach",
    "git",
    "rev-parse",
    "HEAD",
  ]);

  return stdout
    .split("\n")
    .map((s) => s.trim())
    .filter((s) => s != "");
}

interface SubmoduleCommitStatus {
  submoduleName: string;
  commit: string;
}

function parseSubmoduleCommits(
  lines: string[]
): ValidationResult<ValidationResult<SubmoduleCommitStatus>[]> {
  const newLines: ValidationResult<SubmoduleCommitStatus>[] = [];

  if (lines.length == 0) {
    return {
      isValid: false,
      errorMessage: "No submodules resolved from command.",
      result: undefined,
    };
  }

  if (lines.length % 2 != 0) {
    return {
      isValid: false,
      errorMessage: "Unexpected number of lines: " + lines.length,
      result: undefined,
    };
  }

  for (var i = 0; i < lines.length / 2; i++) {
    const enteringMessage = lines[i * 2];
    const commitMessage = lines[i * 2 + 1];

    if (!enteringMessage.startsWith("Entering")) {
      newLines.push({
        isValid: false,
        errorMessage: "Unexpected entering message: " + enteringMessage,
        result: undefined,
      });
    }

    if (!validateCommit(commitMessage).isValid) {
      newLines.push({
        isValid: false,
        errorMessage: "Unexpected commit: " + commitMessage,
        result: undefined,
      });
    }

    var trimmedEntering = enteringMessage.replace(
      /Entering '\.\.\/src\/lib\/(.*?)'/g,
      "$1"
    );

    newLines.push({
      isValid: true,
      errorMessage: undefined,
      result: {
        submoduleName: trimmedEntering,
        commit: commitMessage,
      },
    });
  }

  return {
    isValid: true,
    errorMessage: undefined,
    result: newLines,
  };
}

function shortenCommit(commit: string) {
  return commit.slice(0, 7);
}

export {
  git,
  getCommit,
  getBranch,
  getSubmoduleCommits,
  SubmoduleCommitStatus,
  parseSubmoduleCommits,
  shortenCommit,
};
