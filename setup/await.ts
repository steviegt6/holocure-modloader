function wrapAwait(func: () => void) {
  return async () => {
    func();
  };
}

export { wrapAwait };
