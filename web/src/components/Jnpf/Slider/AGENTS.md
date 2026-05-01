<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Slider

## Purpose
`JnpfSlider` 滑块组件入口。极薄包装 `a-slider`，仅做全局插件式注册以匹配 `JnpfXxx` 命名约定，无业务逻辑增强。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | `withInstall(Slider)` 后导出 `JnpfSlider` |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | SFC（见 `src/AGENTS.md`） |

## For AI Agents

### Working in this directory
- 没有自定义 props；通过 `useAttrs` 透传所有 ant 原生属性（`min/max/step/marks/range` 等）。
- 与 `Rate` 同模式，是项目中"零包装"组件的样板——勿在此添加状态。

### Common patterns
- `withInstall` 全局注册；slot 通过 `Object.keys($slots)` 全量转发。

## Dependencies
### Internal
- `/@/utils`、`/@/hooks/core/useAttrs`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
