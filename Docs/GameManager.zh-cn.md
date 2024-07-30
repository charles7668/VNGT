# GameManager

[English](./GameManager.md) | [繁體中文](./GameManager.zh-tw.md)

- [GameManager](#gamemanager)
  - [使用指南](#使用指南)
    - [手动添加游戏](#手动添加游戏)
    - [自动扫描游戏](#自动扫描游戏)
    - [整合 Locale Emulator](#整合-locale-emulator)
    - [整合 VNGTTranslator](#整合-vngttranslator)
    - [整合您的自定义工具](#整合您的自定义工具)
  - [开发](#开发)
    - [要求](#要求)
    - [数据库模型变更](#数据库模型变更)

## 使用指南

### 手动添加游戏

- 点击添加按钮并选择 exe 文件。

### 自动扫描游戏

- 将文件夹添加到`Library`，然后点击`扫描`按钮。
- 程序将扫描`Library`文件夹内最多 8 层子目录，直到找到 exe 文件。

### 整合 Locale Emulator

- 在`设置`页面设置 Locale Emulator 位置。
- 点击编辑游戏。
- 选择`LE Config`设置。

### 整合 [VNGTTranslator](https://github.com/charles7668/VNGTTranslator)

- 前往`工具`页面
- 点击`下载`（如果未安装）
- 前往游戏编辑页面
- 勾选`RunWithVNGTTranslator`

### 整合您的自定义工具

- 前往`工具`页面
- 点击`打开工具文件夹`
- 将您的程序放置在具有以下结构的文件夹中（执行文件名应与文件夹名相同），或者放置`conf.vngt.yaml`来指定工具名称和执行路径：

  ```shell
  tools\
      your-custom-tool\
          your-custom-tool.exe
  ```

  或

  ```yaml
  Name: your-custom-tool-name
  ExeName: path-to-your-custom-tool # 相对于 tools/your-custom-tool 文件夹
  RunAsAdmin: true
  ```

## 开发

### 要求

- 使用`dotnet tool install --global dotnet-ef`安装`dotnet-ef`

### 数据库模型变更

如果需要详细信息，使用以下命令时可以添加`--verbose`

添加迁移

```shell
dotnet ef migrations add {MigrationName} --project .\GameManager.DB --startup-project .\GameManager.DB.Migrator
```

移除迁移

```shell
dotnet ef migrations remove --project .\GameManager.DB --startup-project .\GameManager.DB.Migrator
```
