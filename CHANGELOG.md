# 變更紀錄

## 1.0.0.1 — 2026-10-01

- 首次將既有 `MCC_Communication_Host.sln`、C# 原始碼、WinForms 資源與專案設定納入 Git。
- 新增 `.gitignore`，排除編譯產物、壓縮包所在的 `bin` 目錄、執行日誌、機台執行設定及個人 IDE 設定。
- 新增 `README.md`，記錄 Host 入口、.NET Framework 4.5、建置方式及外部 `PaeLibGeneral.dll` 相依需求。
- `Properties/AssemblyInfo.cs` 的 AssemblyVersion 與 AssemblyFileVersion 由 1.0.0.0 遞增為 1.0.0.1。
- 保留既有通訊與 UI 邏輯；外部 DLL 不納入公開儲存庫。
- 驗證：已確認方案／專案參考檔存在；候選公開文字檔的憑證關鍵字檢查僅命中資源檔的 .NET PublicKeyToken。
- 驗證：使用本機外部 DLL 在隔離副本執行 Release 建置，MSBuild 結束碼為 0，輸出組件版本為 1.0.0.1。
- 已知限制：既有 AnyCPU 專案與 x86 的 PaeLibGeneral 參考產生 MSB3270 架構警告，另有未使用變數警告；未變更平台設定或通訊邏輯。
- 待驗證：GitHub Public 狀態及遠端 commit 對照；未執行通訊程式或現場設備測試。
