<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
`JnpfQrcode` SFC 实现：`computed qrcode` 按 `dataType` 三分支返回字符串——`static` 取 `staticText`，`relation` 监听 `formData[relationField]`，其它分支组装 `dynamicModelExtra` 的 JSON（流程定位元数据）渲染为 `<QrCode tag="img">`。

## Key Files
| File | Description |
|------|-------------|
| `Qrcode.vue` | inline props（`dataType`/`colorDark`/`colorLight`/`width`/`relationField`/`staticText`/`formData`），调用 `useGeneratorStore.getDynamicModelExtra` |

## For AI Agents

### Working in this directory
- 动态模型 JSON 字段缩写（`t='DFD'`, `mid`, `mt`, `fid`, `pid`, `ftid`, `opt`）必须与扫码端解析逻辑一致——勿重命名。
- 渲染前置条件：`formData && dynamicModelExtra.id && dynamicModelExtra.modelId` 全部齐备，缺一返回空串。
- 颜色 props 默认黑底白字（`#000`/`#fff`），样式只在外层 `.@{prefix-cls}` 控制布局，二维码本体由 `QrCode` 组件 options 控制。

### Common patterns
- `watch(formData, ..., { deep: true, immediate: true })` 处理 relation 模式实时更新。

## Dependencies
### Internal
- `/@/components/Qrcode`、`/@/store/modules/generator`、`/@/hooks/web/useDesign`
### External
- 无

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
