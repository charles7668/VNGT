# 存档修改器

[English](./SavePatcher.md) | [繁體中文](./SavePatcher.zh-tw.md)

- [存档修改器](#存档修改器)
  - [使用指南](#使用指南)
    - [简单范例](#简单范例)
    - [将补丁文件存至不同文件夹的范例](#将补丁文件存至不同文件夹的范例)
  - [支持的存档文件格式](#支持的存档文件格式)

## 使用指南

编写您自己的配置文件。

以下是一个 YAML 格式的配置文件范例：

### 简单范例

```yaml
ConfigName: game1 # 设置名称。此设置指定您将修补的游戏。
FilePath: save.zip # 存档文件路径。此设置可以使用 HTTP 路径，例如：https://test.com/test.zip
ZipPassword: password # 如果 zip 文件需要密码，请在此处输入密码。如果不需要，将此设置保留为空字符串 ('')。
PatchFiles: # 指定 zip 文件中将被复制到 DestinationPath 的文件。如果应该复制所有文件，请将此设置保留为空列表。
  - file1
  - file2
DestinationPath: destinationPath # 目标路径。指定保存数据文件夹的路径。如果设置为空字符串 ('')，将弹出窗口进行选择。此设置可以通过使用 %variable_name% 使用环境变量路径。
```

### 将补丁文件存至不同文件夹的范例

有时，如果游戏有多个文件需要复制到不同的文件夹，则此范例可能会有所帮助。

```yaml
- ConfigName: # 设置名称。此设置指定您将修补的游戏。
  FilePath: test.7z # 存档文件路径。此设置可以使用 HTTP 路径，例如：https://test.com/test.zip
  ZipPassword: "" # 如果 zip 文件需要密码，请在此处输入密码。如果不需要，将此设置保留为空字符串 ('')。
  PatchFiles:
    - test1.txt
    - test2.txt
  DestinationPath: test_dest1 # 目标路径。指定保存数据文件夹的路径。如果设置为空字符串 ('')，将弹出窗口进行选择。此设置可以通过使用 %variable_name% 使用环境变量路径。
# 以上是同一配置名称的第二个配置
- FilePath: test.7z
  ZipPassword: ""
  PatchFiles:
    - test3.txt
    - test4.txt
  DestinationPath: test_dest2
```

将配置文件存储在 "config" 文件夹中，文件名可以随意。您也可以将它们放在子目录中。

## 支持的存档文件格式

- `.zip`
- `.7z`
