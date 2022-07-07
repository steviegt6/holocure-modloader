# HoloCure.ModLoader

HoloCure Mod Loader and Modding API

---


I love modding, you love modding, everyone loves modding; what's not to love? HoloCure.ModLoader is a [free and open-source](https://en.wikipedia.org/wiki/Free_and_open-source_software) mod loader and modding API for [HoloCure](https://kay-yu.itch.io/holocure). You should check it out if you haven't already.

## ATTENTION

Until a stable release (1.0.0) is reached, I will not be guaranteeing [SemVer](https://semver.org/)-compatible versioning. Things will be prone to breaking as updates occur.

Furthermore, the current state of this project's code is, put simply, primitive. Primitive and ugly. Primivite, ugly, and non-permanent. It will be changing as time stretches on. There is no guarantee that your mod for your game will remain stable.

Additionally, the HoloCure API (`HoloCure.API`) will *never* be fully SemVer compliant due to the fickle nature of the game's codebase. This is an unavoidable fact. I will try to keep the API externally consistent, however.

## Licensing

Any projects making use of UndertaleModTool (in our case, specifically any projects using the UndertaleModLib project) is licensed under the GPlv3 license (see LICENSE-GPL3). Any project not using UndertaleModTool or any projects that make use of it are licensed under the MIT license (see LICENSE-MIT).

## Building

If you would like to build this project from source instead of grabbing a pre-built release binary, you may do so by doing the following:

```bash
# Clone the repository.
git clone https://github.com/steviegt6/holocure-modloader.git

# Change your directory.
cd holocure-modloader # (or whatever folder name you chose while cloning)

# Run the setup script.
bash setup.sh # You can use dash, etc.; Windows users can just run the .bat file however they like (thought I'd recommend running ina terminal to monitor the output).

# Assuming the setup executed with no errors, you can then build the solution.
# Please choose whether you want to build for Debug or Release and the operating system to use.
# Examples: Release (Windows), Debug (Linux), etc.
dotnet build ./src/HoloCure.ModLoader.sln -c "[Debug|Release| (Windows|MacOS|Linux)"
```

## Project Structure

Despite the naming, HoloCure.ModLoader projects may be used for other purposes.

| Project | Description |
|---------|-------------|
| `HoloCure.ModLoader` | The CLI application and HoloCure mod loader. Contains code specific to HoloCure, not very reusable. |
| `HoloCure.API` | The base mod, powered by `HoloCure.ModLoader.API`. Providers various APIs and patches to the game to facilitate mod loading. |
| `HoloCure.ModLoader.API` | The GameMaker modding API, designed to be general-purpose and reusable. `HoloCure.ModLoader` interfaces with this. |
| `HoloCure.ModLoader.Updater` | Simple abstractions to partially simplify handling updates. |
| `UndertaleModLib` | Submodule clone of [krzys-h/UndertaleModTool](https://github.com/krzys-h/UndertaleModTool/tree/master), used for modifying GameMaker data. |
| `DiffPatch` | Submodule clone of [Chicken-Bones/DiffPatch](https://github.com/Chicken-Bones/DiffPatch/tree/master), used for handling diffing and raw text patching. Usage kept to a minimum. |