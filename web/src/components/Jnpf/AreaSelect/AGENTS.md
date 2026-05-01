<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# AreaSelect

## Purpose
`JnpfAreaSelect` — 省/市/区/街道四级行政区划选择控件。以 `<a-select>` 为入口，点击展开模态：左侧 `BasicTree` 按需懒加载行政区树，右侧显示已选项；支持移除单项与清空，最终 emit 选中的行政区路径。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | 桶文件，通过 `withInstall` 导出 `JnpfAreaSelect` |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | `AreaSelect.vue` 实现 + `props.ts`（见 `src/AGENTS.md`） |

## For AI Agents

### Working in this directory
- 行政区数据通过懒加载 (`onLoadData`) 拉取，避免一次性装入完整国行政区；自定义数据源需保持 `{ id, fullName, hasChildren }` 的契约。
- 选择结果是节点 ID 数组（按层级），不是树节点对象 — 与 `BasicForm` schema 配置保持一致。
- 模态使用 `:maskClosable="false"` `:keyboard="false"`，防止误关闭丢失编辑。

### Common patterns
- 左右 `transfer-pane` 布局：左 `BasicTree` 选源，右已选列表 + 删除按钮。

## Dependencies
### Internal
- `/@/components/Tree` (`BasicTree`)、`/@/components/Modal/src/components/ModalClose`
- `/@/api/...`（行政区数据接口）
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
