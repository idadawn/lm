<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# components

## Purpose
`extend/saleOrder` 页面专用的产品选择子组件，提供分类树 + 产品列表的标准产品选择 Modal。

## Key Files
| File | Description |
|------|-------------|
| `productModal.vue` | 选择产品 Modal：左侧 `BasicLeftTree` 分类树 + 右侧 `BasicTable` 产品列表；`a-modal` 宽 1000，`destroy-on-close`，自定义 `ModalClose` 关闭按钮。提交时通过 emit 把所选产品回传给父表单。 |

## For AI Agents

### Working in this directory
- 仅服务于 `saleOrder/Form.vue`，不要被其它页面直接复用 —— 通用产品选择请走 `/@/components/...` 公共组件。
- 树选择回调用 `searchInfo` 触发表格刷新（响应式监听 `searchInfo` 变化）。
- 关闭按钮 `:canFullscreen="false"` 是项目约定，不要去掉。

### Common patterns
- `<BasicLeftTree :showSearch="false" :treeData :loading @select>` + `<BasicTable :searchInfo class="jnpf-sub-table">`。
- Modal 的开关由父调 `openSelectModal` 暴露的方法（`defineExpose`）。

## Dependencies
### Internal
- `/@/components/Tree`（BasicLeftTree）、`/@/components/Table`、`/@/components/Modal/src/components/ModalClose.vue`
### External
- `ant-design-vue` Modal、`vue`
