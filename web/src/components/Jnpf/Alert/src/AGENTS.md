<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
`JnpfAlert` 控件实现。`Alert.vue` 是单文件 SFC，把 `title` props 映射到 antdv `<Alert>` 的 `message`，其余 props (`type`/`showIcon`/`closable`/`closeText`) 直传，并支持转发所有具名插槽。

## Key Files
| File | Description |
|------|-------------|
| `Alert.vue` | 单组件实现：`defineProps({ title, type, showIcon, closable, closeText })` + `computed getBindValue` 合并 attrs/props 后 `omit('title')` |

## For AI Agents

### Working in this directory
- 修改 props 时务必同步父级 `Alert/index.ts` 的导出与 `Form/src/componentMap.ts` 中 `'Alert' → JnpfAlert` 注册项。
- `closeText` 仅在 `closable === true` 时透传，避免 antdv 渲染默认关闭按钮被覆盖。
- 通过 `<template #[item]="data" v-for="item in Object.keys($slots)">` 动态转发所有插槽，保留 antdv 原生扩展点。

### Common patterns
- `defineOptions({ name: 'JnpfAlert', inheritAttrs: false })` + `useAttrs({ excludeDefaultKeys: false })` 透传非 props attrs。

## Dependencies
### Internal
- `/@/hooks/core/useAttrs`
### External
- `ant-design-vue` (`Alert`)、`lodash-es` (`omit`)、`vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
