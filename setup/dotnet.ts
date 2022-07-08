import { execFile } from "./cp";

async function dotnet(args: string[]) {
  const { stdout, stderr } = await execFile()("dotnet", args);
  if (stderr) throw stderr;
  return stdout;
}

export { dotnet };
