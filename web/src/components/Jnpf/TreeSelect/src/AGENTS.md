<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
`JnpfTreeSelect` 组件实现。在树形选择器上做了字段映射归一（`id` → `value`、`fullName` → `label`）、`lastLevel` 末级节点限制、单选/多选切换、图标渲染等业务能力，并通过 `useTree` 复用项目的树形工具。

## Key Files
| File | Description |
|------|-------------|
| `TreeSelect.vue` | 组件实现；`getTreeData` 处理 `lastLevel` 禁用、`onChange` 同时 emit `update:modelValue` / `update:value` / `change(value, data)`。 |
| `props.ts` | 定义 `treeSelectProps`、`FieldNames` 接口，默认值含 `treeNodeFilterProp: 'fullName'`、`treeDefaultExpandAll: true`。 |

## For AI Agents

### Working in this directory
- `change` 事件第二参数会透传选中节点完整对象（多选返回数组），调用方依赖该签名，不要随意改动。
- 修改 `getFieldNames` 时确保 `key` 字段始终回退到 `value || 'id'`，否则 ant-design-vue tree 会丢失节点 key。
- `useTree` 来自 `/@/components/Tree/src/hooks/useTree`，与 `Tree` 组件共享逻辑。

### Common patterns
- 同时支持 `value` 与 `modelValue` 两条受控通道，watch 各自同步 `innerValue`。
- `Reflect.has` 判断属性是否被传入以决定 `showArrow` / `showCheckedStrategy` 的默认覆盖。

## Dependencies
### Internal
- `/@/hooks/core/useAttrs`、`/@/utils/is`、`/@/components/Tree`
### External
- `ant-design-vue`、`lodash-es`、`vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
