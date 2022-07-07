import { existsSync, readFileSync, writeFileSync } from "fs";
import { join } from "path";
import { cwd } from "process";
import { prompts } from "prompts";
import { wrapAwait } from "./await.js";
import { brightWhite, color, pinkish } from "./color.js";
import {
  getBranch,
  getCommit,
  getSubmoduleCommits,
  parseSubmoduleCommits,
  shortenCommit,
  SubmoduleCommitStatus,
} from "./git.js";
import { addError, error, log, logVar, success, title, writeErrors } from "./logging.js";

async function getCommitStatus() {
  const commitRes = await getCommit();
  if (!commitRes.isValid) {
    addError(commitRes.errorMessage!);
    error("Could not get current commit.");
    return { commit: undefined, commitShort: undefined };
  }

  const commit = logVar("Got commit: ", commitRes.result);

  const commitShort = logVar(
    "Got shortened commit: ",
    shortenCommit(commitRes.result!)
  );

  return { commit, commitShort };
}

async function getBranchStatus() {
  const branch = logVar("Got branch: ", await getBranch());

  return { branch };
}

async function getSubmodules() {
  const subComs = logVar(
    "Got raw submodule commit output: ",
    await getSubmoduleCommits()
  );

  const parsed = parseSubmoduleCommits(subComs);
  const subCommits: SubmoduleCommitStatus[] = [];

  if (!parsed.isValid) {
    addError(parsed.errorMessage!);
    return { subCommits: undefined };
  }

  parsed.result!.forEach((element) => {
    if (!element.isValid) {
      addError(element.errorMessage!);
    } else {
      subCommits.push(element.result!);
    }
  });

  return { subCommits };
}

function makeSubmoduleElements(subCommits: SubmoduleCommitStatus[]) {
  const data: string[] = [];
  subCommits.forEach((subCommit) => {
    data.push(`${subCommit.submoduleName},${subCommit.commit}`);
  });
  return `    <SubmoduleData>\n${data.join("\n")}\n    </SubmoduleData>`;
}

async function main() {
  log(`Welcome to the ${title()} setup.`);
  log("Gathering commit and branch info before starting...");
  log("");
  log("Determining current commit...");
  const { commit, commitShort } = await getCommitStatus();
  log("");
  log("Determining current branch...");
  const { branch } = await getBranchStatus();
  log("");
  log("Determining submodule commits...");
  const { subCommits } = await getSubmodules();
  log("Got submodule commits:");
  if (subCommits && subCommits.length != 0) {
    subCommits.forEach((subCommit) => {
      log(`    ${subCommit.submoduleName}: ${subCommit.commit}`);
    });
  } else {
    error("No submodule commits found.");
  }
  log("");

  if (writeErrors()) {
    error("Cannot continue execution due to unresolved errors.");
    return;
  }

  if (!commit) {
    error("Could not get current commit.");
    return;
  }

  if (!commitShort) {
    error("Could not get shortened commit.");
    return;
  }

  if (!branch) {
    error("Could not get branch.");
    return;
  }

  if (!subCommits) {
    error("Could not get submodule commits.");
    return;
  }

  log("Moving on to writing .targets file...");
  if (!existsSync(join(cwd(), "targets.template"))) {
    error("Could not find targets.template file.");
    return;
  }

  const template: string = readFileSync(
    join(cwd(), "targets.template"),
    "utf-8"
  );
  var targets = template
    .replace("{{ $CURRENT_COMMIT }}", commit)
    .replace("{{ $CURRENT_COMMIT_SHORT }}", commitShort)
    .replace("{{ $CURRENT_BRANCH }}", branch)
    .replace("{{ $SUBMODULE_DATA }}", makeSubmoduleElements(subCommits));

  writeFileSync(join(cwd(), "../src/git.targets"), targets);
  success("Wrote to ./src/git.targets");

  // TODO: Konata compilation.
}

wrapAwait(main)();
