<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
`JnpfOpenData` SFC 实现：渲染只读 `Input`（或 `<p>` 详情态），按 `type` 从 `userStore.getUserInfo` 派生显示文本，支持 `currUser`/`currTime`/`currOrganize`/`currPosition` 四种系统变量。

## Key Files
| File | Description |
|------|-------------|
| `OpenData.vue` | setValue 按 type 分支：用户名/账号拼接、`dayjs().format('YYYY-MM-DD HH:mm:ss')`、组织 `last` vs 全路径 |
| `props.ts` | `openDataProps`：`type`/`showLevel`(`last`)/`placeholder`/`detailed`/`disabled` |

## For AI Agents

### Working in this directory
- `currOrganize` 的 `showLevel === 'last'` 取 `departmentName`，否则取 `organizeName`——勿混淆字段。
- 这是只读组件：不要 emit `update:value`，写入由 setValue 内部 ref 完成。
- `detailed=true` 用于详情/打印态，渲染 `<p>` 不带边框。

### Common patterns
- 监听 `props.showLevel` 变更重新计算（用户在配置面板切换"显示完整组织"时即时反映）。

## Dependencies
### Internal
- `/@/store/modules/user`
### External
- `ant-design-vue`、`dayjs`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
