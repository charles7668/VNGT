# VNGT

![版本徽章](https://img.shields.io/github/v/release/charles7668/VNGT)
![下载次数](https://img.shields.io/github/downloads/charles7668/VNGT/total)

[English](../README.md) | [繁体中文](./README.zh-tw.md)

游戏管理工具：轻松管理您的游戏，并集成多种游戏辅助工具。

![主界面](./img/main.png)

- 自动扫描文件夹以查找游戏
- 自动下载游戏信息（目前支持：[vndb](https://vndb.org/)）
- 支持多种语言
- 集成 Locale Emulator 以支持非日文操作系统
- 集成[VNGTTranslator](https://github.com/charles7668/VNGTTranslator)以帮助用户翻译游戏文本
- 存档修改器可以替换存档以解锁游戏 CG 或替换存档
- 支持加入自定义的游戏工具

## 使用指南

- [游戏管理器](./Docs/GameManager.md)
- [存档修改器](./Docs/SavePatcher.md)

## 构建

### 要求

- [.NET 8](https://dotnet.microsoft.com/en-us/download)

运行`build.bat`文件以构建项目。

构建结果将在`bin`目录中。
