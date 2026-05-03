<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
`JnpfDivider` 的 SFC 实现。极薄封装：透传 `$attrs` 到 `a-divider`，把 `content` 作为默认插槽，`contentPosition` 映射到 antd 的 `orientation`（`left/center/right`）。

## Key Files
| File | Description |
|------|-------------|
| `Divider.vue` | 组件实现（仅 props：`content`、`contentPosition`） |

## For AI Agents

### Working in this directory
- 不要扩展过多 prop；如需更多视觉自定义，应直接使用 `a-divider`。
- `contentPosition` 类型限定为 `'left' | 'center' | 'right'`。

## Dependencies
### Internal
- `/@/hooks/core/useAttrs`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
