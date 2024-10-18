# VNGT

[![最新版本](https://img.shields.io/github/v/release/charles7668/VNGT)](https://github.com/charles7668/VNGT/releases/)
[![下载次数](https://img.shields.io/github/downloads/charles7668/VNGT/total)](https://github.com/charles7668/VNGT/releases/)

[English](../README.md) | [繁体中文](./README.zh-tw.md)

下载 : [点我](https://github.com/charles7668/VNGT/releases/)

游戏管理工具：轻松管理您的游戏，并集成多种游戏辅助工具。

![主界面](./img/main.png)

- 自动扫描文件夹以查找游戏
- 自动下载游戏信息（目前支持：[vndb](https://vndb.org/) , [DLSite](https://www.dlsite.com) , [ymgal](https://www.ymgal.games/developer#%E6%90%9C%E7%B4%A2%E6%B8%B8%E6%88%8F%E5%88%97%E8%A1%A8)）
- 支持多种语言
- 集成 Locale Emulator 以支持非日文操作系统
- 集成[VNGTTranslator](https://github.com/charles7668/VNGTTranslator)以帮助用户翻译游戏文本
- 存档修改器可以替换存档以解锁游戏 CG 或替换存档
- 支持加入自定义的游戏工具
- 支持备份和还原存档文件（最多可备份 10 个文件）
- 支持追踪游戏安装
- 支持从压缩档中新增新游戏
- 支持使用 [Sandboxie-Plus](https://sandboxie-plus.com/) 打开游戏
- 支持设定备份及还原

## 使用指南

- [游戏管理器](./GameManager.zh-cn.md)
- [存档修改器](./SavePatcher.zh-cn.md)

## 构建

### 要求

- [.NET 8](https://dotnet.microsoft.com/en-us/download)

运行`build.bat`文件以构建项目。

构建结果将在`bin`目录中。
