<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# zh-TW

## Purpose
繁體中文 (zh-TW) 翻譯包，鏡像 `zh-CN` 的鍵結構，提供繁體 UI 顯示。

## Key Files
| File | Description |
|------|-------------|
| `common.ts` | 通用文案（按鈕、提示）。 |
| `component.ts` | 元件級文案。 |
| `layout.ts` | 佈局：頂欄、側邊欄、多分頁、設定抽屜。 |
| `routes.ts` | 路由 / 選單標題（含 `lab` / `onlineDev` / `system` 等鍵）。 |
| `sys.ts` | 系統訊息：API 錯誤、登入、異常記錄等。 |

## For AI Agents

### Working in this directory
- 鍵必須與 `../zh-CN` 完全對齊；新增功能時請同步。
- 切勿做語意調整 — 僅作為簡繁互轉。

### Common patterns
- 與 `zh-CN` 結構同形，便於 diff 校對。

## Dependencies
### Internal
- 由 `web/src/locales/` 與 `/@/hooks/web/useI18n` 載入。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
