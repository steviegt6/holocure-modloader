> **Warning** |
> This project is not being actively developed, see [steviegt6/holocure-launcher](https://github.com/steviegt6/holocure-launcher).

# HoloCure.ModLoader

> [\[discord\]](https://discord.gg/KvqKGQNbhr)

A non-YYC GameMaker Studio 2 mod loader, built using [DogScepter](https://github.com/colinator27/DogScepter) and [YYToolkit](https://github.com/Archie-osu/YYToolkit). Designed for [HoloCure](https://kay-yu.itch.io/holocure) but appropriate elsewhere.

---

I love modding, you love modding, everyone loves modding; what's not to love? HoloCure.ModLoader is a [free and open-source](https://en.wikipedia.org/wiki/Free_and_open-source_software) mod loader and modding API for [HoloCure](https://kay-yu.itch.io/holocure) (you should check it out if you haven't already).

## !! ATTENTION !!

Until a stable release (1.0.0) is reached, I will not be guaranteeing [SemVer](https://semver.org/)-compatible versioning. Things will be prone to breaking as updates occur.

Furthermore, the current state of this project's code is, put simply, primitive. Primitive and ugly. Primivite, ugly, and impermanent. It will be changing as time stretches on. There is no guarantee that your mod for your game will remain stable.

Additionally, the HoloCure API (`HoloCure.API`) will _never_ be fully SemVer compliant due to the fickle nature of the game's codebase. This is an unavoidable fact. I will try to keep the API externally consistent, however.

## Licensing

Any projects making use of UndertaleModTool (in our case, specifically any projects using the UndertaleModLib project) is licensed under the GPlv3 license (see LICENSE-GPL3). Any project not using UndertaleModTool or any projects that make use of it are licensed under the MIT license (see LICENSE-MIT).

## Building

### Prequisites

You are expected to have `npm`, `npx`, `git`, and `dotnet` on your PATH, as these are dependencies for the setup and building.

---

If you would like to build this project from source instead of grabbing a pre-built release binary, you may do so by doing the following:

```bash
# Clone the repository.
git clone https://github.com/steviegt6/holocure-modloader.git

# Change your directory to whatever folder you cloned into.
cd holocure-modloader

# Run the setup script. Should be compatible with any shell. Windows users can execute `setup.bat` instead.
# This handles the intial project setup and compilation of certain projects, among other things.
# This needs to be ran whenever you pull.
sh setup.sh

# Assuming the setup executed with no errors, you can then build the solution.
# Please choose whether you want to build for Debug or Release and the operating system to use.
# Examples: Release (Windows), Debug (Linux), etc.
dotnet build ./src/HoloCure.ModLoader.sln -c "[Debug|Release| (Windows|MacOS|Linux)"
```

## Project Structure

Despite the naming, HoloCure.ModLoader projects may be used for other purposes.

| Project                        | Description                                                                                                                                                                                         |
| ------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `HoloCure.API`                 | A `HoloCure.ModLoader.API` mod that provides an API for modifying HoloCure (the actual game).                                                                                                       |
| `HoloCure.ModLoader`           | A capable mod loader that interfaces with `HoloCure.ModLoader.API`, and is the official CLI implementation.                                                                                         |
| `HoloCure.ModLoader.API`       | Very raw GameMaker modding API, designed to be universal - `HoloCure.ModLoader` interfaces with this.                                                                                               |
| `HoloCure.ModLoader.API.Tests` | Never-used unit tests.                                                                                                                                                                              |
| `HoloCure.ModLoader.Logging`   | Simple logging library.                                                                                                                                                                             |
| `HoloCure.ModLoader.Updater`   | Simple abstractions to partially simplify handling updates.                                                                                                                                         |
| `Konata.Windows`               | Windows bootstrapper for launching YYToolkit.                                                                                                                                                       |
| `DogScepter`              | Submodule clone of [colinator27/DogScepter](https://github.com/colinator27/DogScepter/tree/master), used for modifying GameMaker data.                                                          |
| `DiffPatch`                    | Submodule clone of [Chicken-Bones/DiffPatch](https://github.com/Chicken-Bones/DiffPatch/tree/master), used for handling diffing and raw text patching. Usage kept to a minimum (currently unused). |
