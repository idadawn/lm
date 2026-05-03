<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Rate

## Purpose
`JnpfRate` 评分组件入口。极薄包装 `a-rate`，仅做全局插件式注册以便表单设计器按 `JnpfXxx` 命名习惯统一调用。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | `withInstall(Rate)` 后导出 `JnpfRate` |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | SFC（见 `src/AGENTS.md`） |

## For AI Agents

### Working in this directory
- 没有自定义 props——通过 `useAttrs` 透传所有 ant 原生属性；勿引入业务字段。
- 如需扩展行为（如评分文案、统计），新建独立组件或在外层包装，避免污染该薄壳。

### Common patterns
- `withInstall` 全局注册，命名 `JnpfRate`。

## Dependencies
### Internal
- `/@/utils`、`/@/hooks/core/useAttrs`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
