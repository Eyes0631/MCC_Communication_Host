# MCC_Communication_Host

MCC 通訊 Host 測試程式，使用 C# Windows Forms 與 TCP Listener／Socket。

## 方案與版本

- 方案：`MCC_Communication_Host.sln`
- 專案：`MCC_Communication/MCC_Communication_Host.csproj`
- 目標框架：.NET Framework 4.5
- 組件版本：1.0.0.1
- 程式入口：`Host`，輸出檔名為 `Host.exe`。

## 建置需求

使用 Windows、Visual Studio 與 .NET Framework 4.5 targeting pack。
專案依賴外部 `PaeLibGeneral.dll`，現有相對參考位置為
`../TBPP14900/PaeLib/DLL/PaeLibGeneral.dll`（相對於本儲存庫根目錄）。
此 DLL 未包含於儲存庫；請自行準備合法可用的相依套件，或在本機調整參考位置。

在 Visual Studio 開啟方案，或於 Visual Studio Developer Command Prompt 執行：

```bat
msbuild MCC_Communication_Host.sln /p:Configuration=Release
```

## 設定與上傳範圍

程式使用執行目錄下的 `System/Setup.ini`；原始碼的預設監聽位置為 `127.0.0.1:5555`。
個別機台設定、既有執行檔、壓縮包、日誌、`bin`、`obj` 與個人 IDE 設定未納入版本控制。
初次匯入保留既有通訊邏輯；版本與驗證紀錄請見 `CHANGELOG.md`。
