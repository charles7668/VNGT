# GameManager

- [GameManager](#gamemanager)
  - [User Guide](#user-guide)
    - [Manual Add Game](#manual-add-game)
    - [Auto Scan Game](#auto-scan-game)
    - [Integrate Locale Emulator](#integrate-locale-emulator)
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
