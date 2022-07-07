interface ValidationResult<T> {
  isValid: boolean;
  errorMessage: string | undefined;
  result: T | undefined;
}

function validateCommit(commit: string): ValidationResult<string> {
  if (commit.length != 40) {
    return {
      isValid: false,
      errorMessage: "Unexpected commit length: " + commit,
      result: undefined,
    };
  }

  return { isValid: true, errorMessage: undefined, result: commit };
}

export { ValidationResult, validateCommit };
