<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# ZFormulaInput

## Purpose
公式输入框组件。提供 `contenteditable` 的富文本输入区,支持插入变量(从 `options` 选择)、键盘运算符校验 (`0-9 + - * / @ . ( )`)、HTML/字符串互转,常用于指标公式编辑场景。通过 `withInstall(ZFormulaInput)` 暴露。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | 入口,导出 `ZFormulaInput` 与 `ZFormulaInputFormProps` 类型 |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | 组件实现:输入区 + 变量选择浮层 + 工具方法 (see `src/AGENTS.md`) |

## For AI Agents

### Working in this directory
- 输入合法字符由 `validKeys` 限制(数字+运算符);插入变量必须走选项弹框,不要让用户直接键入变量名。
- 当前使用 `contenteditable="plaintext-only"`,iOS / 部分浏览器降级行为需要回归测试。
- 与 `ZTempField` 一并出现在指标编辑场景,样式独立 (`src/index.less`)。

### Common patterns
- 校验失败通过 `state.errorMsg` 在输入框下方提示,而非全局 message。

## Dependencies
### Internal
- `/@/utils` (`withInstall`)
### External
- Vue 3,Ant Design Vue (`a-input`),`lodash-es` (throttle)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
