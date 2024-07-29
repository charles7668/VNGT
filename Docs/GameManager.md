# GameManager

- [GameManager](#gamemanager)
  - [User Guide](#user-guide)
    - [Manual Add Game](#manual-add-game)
    - [Auto Scan Game](#auto-scan-game)
    - [Integrate Locale Emulator](#integrate-locale-emulator)
    - [Integrate VNGTTranslator](#integrate-vngttranslator)
    - [Integrate Your Custom Tools](#integrate-your-custom-tools)
  - [Develop](#develop)
    - [Requirement](#requirement)
    - [Database Model Change](#database-model-change)

## User Guide

### Manual Add Game

- Click the Add button and select the exe file.

### Auto Scan Game

- Add a folder to the `Library`, then click the `Scan` button.
- The program will scan up to 8 levels of subdirectories inside the `Library` folders until it finds an exe file.

### Integrate Locale Emulator

- Set up Locale Emulator location in the `Settings` page.
- Click Edit Game.
- Select the `LE Config` setting.

### Integrate [VNGTTranslator](https://github.com/charles7668/VNGTTranslator)

- Go to the `tools` page
- Click `download` (if not installed)
- Go to the game edit page
- Check `RunWithVNGTTranslator`

### Integrate Your Custom Tools

- Go to the `tools` page
- Click `Open Tools Folder`
- Put your program in the folder with the following structure (the execution file name should be the same as the folder name) or put `conf.vngt.yaml` to specify the tool name and execution path:

  ```shell
  tools\
      your-custom-tool\
          your-custom-tool.exe
  ```

  or

  ```yaml
  Name: your-custom-tool-name
  ExeName: path-to-your-custom-tool # relative to tools/your-custom-tool folder
  RunAsAdmin: true
  ```

## Develop

### Requirement

- Install `dotnet-ef` by using `dotnet tool install --global dotnet-ef`

### Database Model Change

using following command , if need detail message , the command can add `--verbose`

for add migration

```shell
dotnet ef migrations add {MigrationName} --project .\GameManager.DB
```

for remove migration

```shell
dotnet ef migrations remove --project .\GameManager.DB
```
