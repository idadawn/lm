<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Switch

## Purpose
`JnpfSwitch` 开关组件入口。包装 `a-switch` 并通过 `withInstall` 全局注册，导出 `SwitchProps` 类型方便 TS 调用方按 `Partial<ExtractPropTypes<typeof switchProps>>` 复用 props 类型。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | `withInstall(Switch)` 后导出 `JnpfSwitch` 与 `SwitchProps` 类型 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | SFC + props（未在本次 deepinit 列表内，仅记录路径） |

## For AI Agents

### Working in this directory
- 同时导出运行时组件与 `SwitchProps` 类型——对接表单 schema 时优先复用该类型而非手写。
- 与 Rate/Slider 不同，本入口显式声明 `switchProps`，意味着可能存在自定义字段（如 active/inactive 文案、值映射）；改 props 时同时维护 `src/props.ts`。

### Common patterns
- `withInstall` 全局注册；类型导出走 `ExtractPropTypes` 保持 props 一致性。

## Dependencies
### Internal
- `/@/utils`
### External
- `vue`、`ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
