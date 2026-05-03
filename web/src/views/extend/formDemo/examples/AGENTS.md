<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# examples

## Purpose
综合表单示例：动态字段循环渲染、公式输入框 (`z-formula-input`)、下拉/多选/单选/树形/级联控件等组合，作为表单设计器与业务表单互通的快速参考。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 综合示例：`z-temp-field` 动态渲染 `formData.items` + `z-formula-input` 公式编辑 + 各类选择器 |

## For AI Agents

### Working in this directory
- `z-temp-field` 通过 `typeProps` / `labelProps` / `keyProps` 等映射决定字段渲染，更换数据结构时务必同步映射。
- 公式输入框 `options` 是指标列表，`initValue` 与 `change` emit 共同维护表达式 AST。
- 不要在该示例直接连接业务 API，仅作为 Schema/控件示例参考。

### Common patterns
- 模板内插值 `{{ formData.items }}` / `{{ state.formulaValue }}` 用于实时观察响应式状态。
- 大段表单使用 `ScrollContainer` 防止溢出。

## Dependencies
### Internal
- `/@/components/Container`
### External
- `ant-design-vue`（`z-temp-field` / `z-formula-input` 为项目内全局/插件级控件）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
