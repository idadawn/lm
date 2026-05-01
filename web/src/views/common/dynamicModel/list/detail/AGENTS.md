<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# detail

## Purpose
动态模型列表的只读详情视图。配合在线开发生成的表单 Schema (`formConf.fields`) 渲染只读字段，并支持打印按钮、跳转关联表单详情等浏览态能力，是 `list/index.vue` 行点击查看详情时打开的子组件。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 详情容器：Popup/Modal/Drawer 三态包裹 `Parser`，并集成 `PrintSelect` / `PrintBrowse` 打印组件 |
| `Parser.vue` | 表单只读解析器：根据 `formConf.fields` 循环渲染 `Item`，emit `toDetail` 用于关联跳转 |
| `Item.vue` | 单字段渲染器：按 `jnpfKey` 选择对应 `jnpf-*` 控件并以 detail 模式展示 |

## For AI Agents

### Working in this directory
- 只读模式下不要绑定提交逻辑，写操作走外层 `Form.vue`。
- 关联表单跳转使用 `relationData` 字典 + `toDetail({ modelId, id })` 上报；不要直接路由跳转。
- 打印能力依赖 `formConf.hasPrintBtn` 与 `formConf.printId`；缺失时隐藏按钮。

### Common patterns
- `formConf` 由 `getConfigData` 拉取，`formData` 由 `getInfo` 单条获取，再通过 `getDataInterfaceDataInfoByIds` 批量回填字典文本。
- 三态弹层共享同一份 `Parser` 实例，避免字段渲染发散。

## Dependencies
### Internal
- `/@/api/onlineDev/visualDev`, `/@/api/systemData/dataInterface`
- `/@/components/{Popup,Modal,Drawer,PrintDesign}`
- `/@/store/modules/{user,generator}`
### External
- `ant-design-vue`, `lodash-es`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
