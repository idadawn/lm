<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Cron

## Purpose
`JnpfCron` Cron 表达式可视化编辑组件。`EasyCronInput` 提供只读输入框 + 编辑按钮，弹出 `EasyCronModal`，内部由 `EasyCronInner` 用 7 个 Tab（秒/分/时/日/月/周/年）拼装表达式，可隐藏秒/年。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | 导出 `JnpfCron`、`CronInner`、`CronModal` |
| `EasyCronInput.vue` | 入口组件：只读 `a-input-search` + 按钮触发 modal，写回 `value`、`change` |
| `EasyCronInner.vue` | 7 个 Tab 主面板，集成各 `*UI.vue` 子项与执行时间预览 |
| `EasyCronModal.vue` | 弹窗壳，宽度 800，复用 `ModalClose` |
| `easy.cron.data.ts` | 公共 `cronProps`/`cronEmits`（`value/disabled/hideSecond/hideYear/remote`） |
| `validator.ts` | `cronRule`：基于 `cron-parser` 校验 7 段表达式与年份 |
| `easy.cron.inner.less` / `easy.cron.input.less` | 样式 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `tabs/` | 各时间维度的 Tab UI 与公用 `useTabMixin`（见 `tabs/AGENTS.md`） |

## For AI Agents

### Working in this directory
- 公共 props/emits 必须从 `easy.cron.data.ts` 引入，不要在 `.vue` 内重复声明。
- `validator.ts` 限制 ≤7 段，添加扩展段时同步更新此规则及 `EasyCronInner` 的 Tab 列表。
- 中文标签（"秒/分/时/日/月/周/年"）保留中文，不要本地化键替换。

### Common patterns
- 通过 `useDesign('easy-cron-input')` 等 prefix 形成命名空间。
- Modal/Inner/Input 三层拆分以便外部独立复用 `CronInner`。

## Dependencies
### Internal
- `/@/utils/propTypes`、`/@/hooks/web/useDesign`、`/@/components/Modal/src/components/ModalClose.vue`
### External
- `ant-design-vue`、`cron-parser`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
