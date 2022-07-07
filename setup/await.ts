export function wrapAwait(func: () => void) {
  return async () => {
    func();
  };
}
