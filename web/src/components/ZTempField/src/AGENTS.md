<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
ZTempField 的具体实现。基于 `a-form-item` + `jnpf-*` 控件按字段类型动态渲染,并通过 `<z-temp-field>` 自递归支持子字段;props 设计高度参数化,允许调用方将组件适配到任意后端字段命名约定。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 控件渲染主体,绑定 `state.result` 双向到 `item[keyProps]`;包含工具提示 (`item.des`) |
| `props.ts` | 全部命名映射 prop:typeProps / labelProps / valueProps / keyProps / listProps / listLabelProps / listValueProps / displayProps / requiredProps / relationProps 等 |

## For AI Agents

### Working in this directory
- props 数量大且默认值面向"产品因子"业务,新业务接入务必显式覆盖映射,避免污染默认默认值。
- `state.options` 由 `item[listProps]` 派生 `{ v, fullName }`;字段名固定为 `v` 与 `fullName`,Select / Radio 模板都依赖,不要更名。
- 递归渲染时 `formItemName` 用 `concat([index, keyProps])` 组合,保持 a-form 校验路径正确。
- `minDate / maxDate` 默认 1900-01-01 / 2100-12-31,日期类型若以后接入需复用这两个 prop。

### Common patterns
- 默认值优先 `item[keyProps]`,回退 `item[defaultValue]`,与表单 reset 行为对齐。

## Dependencies
### External
- Ant Design Vue,`@ant-design/icons-vue` (ExclamationCircleOutlined),全局 `jnpf-*` 组件

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
