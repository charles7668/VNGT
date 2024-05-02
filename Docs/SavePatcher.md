# Index

- [Index](#index)
- [User Guid](#user-guid)
- [Support Save File Format](#support-save-file-format)

# User Guid

Write your own configuration files.

Example of a configuration file in YAML format:

```yaml
ConfigName: game1 # Setting name. This setting specifies which game you will patch.
FilePath: save.zip # Save file path. This setting can use an HTTP path like: https://test.com/test.zip
ZipPassword: password # If the zip file requires a password, enter the password here. If not, leave this setting as an empty string ('').
PatchFiles: # Specify which files in the zip file will be copied to the DestinationPath. If all files should be copied, leave this setting as an empty list.
  - file1
  - file2
DestinationPath: destinationPath # Destination path. Specify the path to save the data folder. If set to an empty string (''), a window will prompt for selection. this setting can use environment path by using %variable_name%
```

Store configuration files in the "config" folder with arbitrary filenames. You can also place them in subdirectories.

# Support Save File Format

- `.zip`
- `.7z`
