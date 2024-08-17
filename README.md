# VNGT

![release badge](https://img.shields.io/github/v/release/charles7668/VNGT)
![Downloads](https://img.shields.io/github/downloads/charles7668/VNGT/total)

[繁體中文](./Docs/README.zh-tw.md) | [简体中文](./Docs/README.zh-cn.md)

Game management tool: easily manage your games and integrate various assistive tools for gaming.

![main](./Docs/img/main.png)

- Automatically scan folders for games
- Automatically download game information (currently supported: [vndb](https://vndb.org/))
- Support multiple languages
- Integrate Locale Emulator to support non-Japanese OS
- Integrate [VNGTTranslator](https://github.com/charles7668/VNGTTranslator) to help users translate game text
- Save patcher can replace save data to unlock game CG
- Support execution of your game tool
- Support backup and restore of save files (up to 10 backup files)

## Guide

- [Game Manager](./Docs/GameManager.md)
- [Save Patcher](./Docs/SavePatcher.md)

## Build

### Requirement

- [.NET 8](https://dotnet.microsoft.com/en-us/download)

Run the `build.bat` file to build the project.

The build result will be in the `bin` directory.
