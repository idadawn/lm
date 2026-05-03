<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# DatePicker

## Purpose
日期/时间选择控件包装目录，导出四个组件：`JnpfDatePicker`、`JnpfDateRange`、`JnpfTimePicker`、`JnpfTimeRange`，统一支持 `format` + `startTime/endTime` 上下界限制。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | barrel：注册并导出四个 `withInstall` 组件 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | 各组件的 SFC 实现与 props 定义（见 `src/AGENTS.md`） |

## For AI Agents

### Working in this directory
- 始终保持 4 个组件成对存在（点选/区间 × 日期/时间），FormGenerator 配置依赖此命名。
- `format` 默认值（`YYYY-MM-DD` / `HH:mm:ss`）请勿在 barrel 层覆盖。

## Dependencies
### Internal
- `/@/utils` — `withInstall`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
