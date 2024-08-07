# SavePatcher

[繁体中文](./SavePatcher.zh-tw.md.md) | [简体中文](./SavePatcher.zh-cn.md)

- [SavePatcher](#savepatcher)
  - [User Guide](#user-guide)
    - [Simple Example](#simple-example)
    - [Patch File To Difference Folder Example](#patch-file-to-difference-folder-example)
  - [Support Save File Format](#support-save-file-format)

## User Guide

Write your own configuration files.

Example of a configuration file in YAML format:

### Simple Example

```yaml
ConfigName: game1 # Setting name. This setting specifies which game you will patch.
FilePath: save.zip # Save file path. This setting can use an HTTP path like: https://test.com/test.zip
ZipPassword: password # If the zip file requires a password, enter the password here. If not, leave this setting as an empty string ('').
PatchFiles: # Specify which files in the zip file will be copied to the DestinationPath. If all files should be copied, leave this setting as an empty list.
  - file1
  - file2
DestinationPath: destinationPath # Destination path. Specify the path to save the data folder. If set to an empty string (''), a window will prompt for selection. this setting can use environment path by using %variable_name%
```

### Patch File To Difference Folder Example

Sometimes, if a game has multiple files that need to be copied to different folders, then this example can be helpful.

```yaml
- ConfigName: # Setting name. This setting specifies which game you will patch.
  FilePath: test.7z # Save file path. This setting can use an HTTP path like: https://test.com/test.zip
  ZipPassword: "" # If the zip file requires a password, enter the password here. If not, leave this setting as an empty string ('').
  PatchFiles:
    - test1.txt
    - test2.txt
  DestinationPath: test_dest1 # Destination path. Specify the path to save the data folder. If set to an empty string (''), a window will prompt for selection. this setting can use environment path by using %variable_name%
# above is second config in same config name
- FilePath: test.7z
  ZipPassword: ""
  PatchFiles:
    - test3.txt
    - test4.txt
  DestinationPath: test_dest2
```

Store configuration files in the "config" folder with arbitrary filenames. You can also place them in subdirectories.

## Support Save File Format

- `.zip`
- `.7z`
