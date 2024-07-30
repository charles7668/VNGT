# GameManager

[English](./GameManager.md) | [简体中文](./GameManager.zh-tw.md)

- [GameManager](#gamemanager)
  - [使用指南](#使用指南)
    - [手動添加遊戲](#手動添加遊戲)
    - [自動掃描遊戲](#自動掃描遊戲)
    - [整合 Locale Emulator](#整合-locale-emulator)
    - [整合 VNGTTranslator](#整合-vngttranslator)
    - [整合您的自訂工具](#整合您的自訂工具)
  - [開發](#開發)
    - [要求](#要求)
    - [數據庫模型變更](#數據庫模型變更)

## 使用指南

### 手動添加遊戲

- 點擊添加按鈕並選擇 exe 文件。

### 自動掃描遊戲

- 將文件夾添加到`Library`，然後點擊`掃描`按鈕。
- 程式將掃描`Library`文件夾內最多 8 層子目錄，直到找到 exe 文件。

### 整合 Locale Emulator

- 在`設置`頁面設置 Locale Emulator 位置。
- 點擊編輯遊戲。
- 選擇`LE Config`設置。

### 整合 [VNGTTranslator](https://github.com/charles7668/VNGTTranslator)

- 前往`工具`頁面
- 點擊`下載`（如果未安裝）
- 前往遊戲編輯頁面
- 勾選`RunWithVNGTTranslator`

### 整合您的自訂工具

- 前往`工具`頁面
- 點擊`打開工具文件夾`
- 將您的程序放置在具有以下結構的文件夾中（執行文件名應與文件夾名相同），或者放置`conf.vngt.yaml`來指定工具名稱和執行路徑：

  ```shell
  tools\
      your-custom-tool\
          your-custom-tool.exe
  ```

  或

  ```yaml
  Name: your-custom-tool-name
  ExeName: path-to-your-custom-tool # 相對於 tools/your-custom-tool 文件夾
  RunAsAdmin: true
  ```

## 開發

### 要求

- 使用`dotnet tool install --global dotnet-ef`安裝`dotnet-ef`

### 數據庫模型變更

如果需要詳細訊息，使用以下命令時可以添加`--verbose`

添加遷移

```shell
dotnet ef migrations add {MigrationName} --project .\GameManager.DB --startup-project .\GameManager.DB.Migrator
```

移除遷移

```shell
dotnet ef migrations remove --project .\GameManager.DB --startup-project .\GameManager.DB.Migrator
```
