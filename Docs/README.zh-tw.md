# VNGT

![版本徽章](https://img.shields.io/github/v/release/charles7668/VNGT)
![下載次數](https://img.shields.io/github/downloads/charles7668/VNGT/total)

[English](../README.md) | [简体中文](./README.zh-cn.md)

遊戲管理工具：輕鬆管理您的遊戲，並集成多種遊戲輔助工具。

![主界面](./img/main.png)

- 自動掃描資料夾以查找遊戲
- 自動下載遊戲資訊（目前支援：[vndb](https://vndb.org/)）
- 支援多種語言
- 整合 Locale Emulator 以支援非日文操作系統
- 整合[VNGTTranslator](https://github.com/charles7668/VNGTTranslator)以幫助用戶翻譯遊戲文本
- 存檔修改器可以替換存檔以解鎖遊戲 CG 或替換存檔
- 支援加入自訂的遊戲工具

## 使用指南

- [遊戲管理器](./Docs/GameManager.md)
- [存檔修改器](./Docs/SavePatcher.md)

## 構建

### 要求

- [.NET 8](https://dotnet.microsoft.com/en-us/download)

運行`build.bat`文件以構建專案。

構建結果將在`bin`目錄中。
