<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Button

## Purpose
`JnpfButton` 按钮组件包装目录。封装 `a-button` 提供表单生成器中 `align` (`left/center/right`) 与 `buttonText` 等扩展能力，便于在 LIMS 表单中以配置化方式插入按钮。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | barrel：导出 `JnpfButton` 与类型 `ButtonProps = Partial<ExtractPropTypes<typeof buttonProps>>` |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | 组件实现与 props 定义（见 `src/AGENTS.md`） |

## For AI Agents

### Working in this directory
- 类型导出（`ButtonProps`）必须随 `props.ts` 同步，避免外部消费者类型推断失效。
- 不要在 `index.ts` 中加入业务逻辑。

### Common patterns
- 统一 `withInstall` 注册；类型通过 `ExtractPropTypes` 自动从 props 推导。

## Dependencies
### Internal
- `/@/utils` — `withInstall`
### External
- `vue`（类型）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
