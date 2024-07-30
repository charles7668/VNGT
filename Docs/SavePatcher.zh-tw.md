# 存檔修改器

[English](./SavePatcher.md) | [简体中文](./SavePatcher.zh-cn.md)

- [存檔修改器](#存檔修改器)
  - [使用指南](#使用指南)
    - [簡單範例](#簡單範例)
    - [將補丁文件存至不同文件夾的範例](#將補丁文件存至不同文件夾的範例)
  - [支援的存檔文件格式](#支援的存檔文件格式)

## 使用指南

編寫您自己的配置文件。

以下是一個 YAML 格式的配置文件範例：

### 簡單範例

```yaml
ConfigName: game1 # 設定名稱。此設定指定您將修補的遊戲。
FilePath: save.zip # 存檔文件路徑。此設置可以使用 HTTP 路徑，例如：https://test.com/test.zip
ZipPassword: password # 如果 zip 文件需要密碼，請在此處輸入密碼。如果不需要，將此設置保留為空字符串 ('')。
PatchFiles: # 指定 zip 文件中將被複製到 DestinationPath 的文件。如果應該複製所有文件，請將此設置保留為空列表。
  - file1
  - file2
DestinationPath: destinationPath # 目標路徑。指定保存數據文件夾的路徑。如果設置為空字符串 ('')，將彈出窗口進行選擇。此設置可以通過使用 %variable_name% 使用環境變量路徑。
```

### 將補丁文件存至不同文件夾的範例

有時，如果遊戲有多個文件需要複製到不同的文件夾，則此範例可能會有所幫助。

```yaml
- ConfigName: # 設定名稱。此設定指定您將修補的遊戲。
  FilePath: test.7z # 存檔文件路徑。此設置可以使用 HTTP 路徑，例如：https://test.com/test.zip
  ZipPassword: "" # 如果 zip 文件需要密碼，請在此處輸入密碼。如果不需要，將此設置保留為空字符串 ('')。
  PatchFiles:
    - test1.txt
    - test2.txt
  DestinationPath: test_dest1 # 目標路徑。指定保存數據文件夾的路徑。如果設置為空字符串 ('')，將彈出窗口進行選擇。此設置可以通過使用 %variable_name% 使用環境變量路徑。
# 以上是同一配置名稱的第二個配置
- FilePath: test.7z
  ZipPassword: ""
  PatchFiles:
    - test3.txt
    - test4.txt
  DestinationPath: test_dest2
```

將配置文件存儲在 "config" 文件夾中，文件名可以隨意。您也可以將它們放在子目錄中。

## 支援的存檔文件格式

- `.zip`
- `.7z`
