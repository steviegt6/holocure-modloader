import util from "util";
import child_process from "child_process";

const _execFile = util.promisify(child_process.execFile);
function execFile() {
  return _execFile;
}

export { execFile };
