<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# ScreenConfig

## Purpose
大屏配置 JSON（`VisualConfigEntity` → `BLADE_VISUAL_CONFIG`）DTO。一份大屏对应一份配置（detail + component 两段 JSON）。

## Key Files
| File | Description |
|------|-------------|
| `ScreenConfigCrInput.cs` | 创建（visualId/detail/component） |
| `ScreenConfigUpInput.cs` | 更新 |
| `ScreenConfigInfoOutput.cs` | 详情（id/visualId/detail/component） |

## For AI Agents

### Working in this directory
- `detail`/`component` 是 BladeX 大屏设计器序列化的字符串 JSON，**不要在后端解析**——以字符串透传；前端反序列化。
- 一对一关系：保存时若 `visualId` 已存在则覆盖更新而非新增。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
