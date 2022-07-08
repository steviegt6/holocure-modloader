import { brightWhite, color, pinkish } from "./color";

const errors: string[] = [];

function log(msg: string) {
  console.log(color.cyan("▶  ") + brightWhite(msg));
}

function success(msg: string) {
  console.log(color.green("✔  ") + brightWhite(msg));
}

function error(msg: string, indent: number = 0) {
  var print = "✘  ";
  for (var i = 0; i < indent; i++) print += " ";
  console.log(color.red(print + msg));
}

function logVar(msg: string, val: any): any {
  log(msg + color.cyan(val));
  return val;
}

function title() {
  return color.cyan("HoloCure.") + pinkish("ModLoader");
}

function addError(error: string) {
  errors.push(error);
}

function writeErrors() {
  if (errors.length == 0) {
    success("Setup ran without any detected errors!");
    return false;
  }

  error("Setup ran with " + errors.length + " detected errors:");

  for (var i = 0; i < errors.length; i++) {
    error(errors[i], 4);
  }

  errors.length = 0;
  return true;
}

export { log, success, error, logVar, title, addError, writeErrors };
