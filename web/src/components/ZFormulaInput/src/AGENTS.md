<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
ZFormulaInput 的具体实现。包含 contenteditable 输入区、变量选择浮层、HTML/字符串互转工具,以及输入合法性校验逻辑。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 主组件:监听 keydown/keyup/blur,管理 `state.innerModel.formula/vars`,弹出 `selectionRef` 选项框 |
| `props.ts` | props:options / placeholder / disabled / initValue / scrollWrapperClassName 等 |
| `utils.js` | `getHTMLList / str2dom / dom2str / isHTML / validKeys / getDiffIndex / setFocus / getParentNode` 工具 |
| `index.less` | 输入区、错误提示、变量浮层样式 |

## For AI Agents

### Working in this directory
- `validKeys` 字符串集中维护,只允许 `0-9 + - * / @ . ( )`;新增允许字符要同步键盘事件分支。
- `setFocus` 通过 Range/Selection API 重设光标,避免插入变量后光标错位;不要替换为 `focus()`。
- `scrollWrapperClassName` 用于在长表单内重定位下拉浮层,通过 `getParentNode` 向上查找;改动会影响表单滚动场景。
- `setValue` 在 blur 时触发,统一通过 `update:value / change / input` 三个事件向外同步。

### Common patterns
- DOM 操作集中在 `utils.js`(纯 JS 文件),组件逻辑保持纯净;调试 DOM 问题先看 utils。

## Dependencies
### External
- `lodash-es` (throttle),Vue 3 Composition API,Ant Design Vue (`a-input`)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
