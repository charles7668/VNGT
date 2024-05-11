# GameManager

- [GameManager](#gamemanager)
  - [Develop](#develop)
    - [Requirement](#requirement)
    - [Dabase Model Change](#dabase-model-change)

## Develop

### Requirement

- Install `dotnet-ef` by using `dotnet tool install --global dotnet-ef`

### Dabase Model Change

using following command , if need detail message , the command can add `--verbose`

for add migration

```shell
dotnet migrations add {MigrationName} --project .\GameManager.DB
```

for remove migration

```shell
dotnet migrations remove --project .\GameManage.DB
```
