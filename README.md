# HoloCure.ModLoader

HoloCure Mod Loader and Modding API

---

I love modding, you love modding, everyone loves modding; what's not to love? HoloCure.ModLoader is a [free and open-source](https://en.wikipedia.org/wiki/Free_and_open-source_software) mod loader and modding API for [HoloCure](https://kay-yu.itch.io/holocure). You should check it out if you haven't already.

## Building

If you would like to build this project from source instead of grabbing a pre-built release binary, you may do so by cloning this repository, restoring submodules, and building `./src/HoloCure.ModLoader.sln`.

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