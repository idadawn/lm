<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# NumberRange

## Purpose
`JnpfNumberRange` 数字范围组件入口目录。两个 `InputNumber` 拼接的 [min, max] 区间输入控件，常用于表单设计器中的范围筛选。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | `withInstall(NumberRange)` 后导出 `JnpfNumberRange` |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | 组件 SFC（见 `src/AGENTS.md`） |

## For AI Agents

### Working in this directory
- props 定义内联在 SFC 中（仅 `value`/`precision`），改动时直接编辑 SFC。
- 输出值始终是长度 0 或 2 的数组，空值使用 `[]` 而非 `[null, null]`。

### Common patterns
- 通过 `withInstall` 全局注册；类名前缀 `useDesign('number-range')`。

## Dependencies
### Internal
- `/@/utils`、`/@/hooks/core/useAttrs`、`/@/hooks/web/useDesign`
### External
- `ant-design-vue`（`InputNumber`、`Form.useInjectFormItemContext`）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
