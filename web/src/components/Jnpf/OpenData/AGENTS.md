<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# OpenData

## Purpose
`JnpfOpenData` 内置变量数据组件入口。根据 `type` 显示当前用户、当前时间、所属组织、所属岗位等系统变量；表单设计器中常用作默认值占位/只读展示。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | `withInstall(OpenData)` 后导出 `JnpfOpenData` |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | 组件 SFC + props（见 `src/AGENTS.md`） |

## For AI Agents

### Working in this directory
- 类型常量 `currUser`/`currTime`/`currOrganize`/`currPosition` 必须与表单设计器、后端默认值映射保持一致。
- 仅作只读显示，不参与 v-model 双向绑定。

### Common patterns
- 全局注册组件，名称 `JnpfOpenData`。

## Dependencies
### Internal
- `/@/store/modules/user`
### External
- `ant-design-vue`、`dayjs`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
