<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# detail

## Purpose
列表短链的只读详情弹层。在匿名短链场景下查看单条记录的字段详情，复用动态模型详情解析器以保证字段渲染一致。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | Popup/Modal/Drawer 三态详情容器，调用 `getDataChange` 拉取联动数据后由共享 `Parser` 渲染 |

## For AI Agents

### Working in this directory
- 解析器复用：`import Parser from '../../../dynamicModel/list/detail/Parser.vue'`，避免在此目录重复实现字段渲染逻辑。
- 接口走 `/@/api/onlineDev/shortLink`（带签名），不要切换到普通后台接口。
- 仅作只读展示，不要在弹层加入编辑/提交按钮。

### Common patterns
- `state.key = +new Date()` 强制重渲染 `Parser` 防止缓存；`destroyOnClose` 兜底清理弹层。
- 三态容器 (`usePopup` / `useModal` / `useDrawer`) 注册同一份解析器，由 `formConf` 中的尺寸字段决定使用哪种。

## Dependencies
### Internal
- `/@/api/onlineDev/shortLink`
- `/@/components/{Popup,Modal,Drawer}`
- `../../../dynamicModel/list/detail/Parser.vue`
### External
- `lodash-es`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
