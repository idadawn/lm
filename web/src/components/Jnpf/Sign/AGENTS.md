<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Sign

## Purpose
`JnpfSign` 手写签名组件入口。点击触发签名 modal，签完返回 base64 图片；`submitOnConfirm=true` 时直接调用 `createSign` 入库并写回 `userStore.signImg`，常用作流程审批/盖章场景。

## Key Files
| File | Description |
|------|-------------|
| `index.ts` | `withInstall(Sign)` 后导出 `JnpfSign` |

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `src/` | 签名 SFC + 弹窗 + esign 画板（见 `src/AGENTS.md`） |

## For AI Agents

### Working in this directory
- 只在此处接入项目级签名持久化（`createSign`），勿在外层重复调用 API。
- 暗色主题自动给 `<img>` 加白底（保证笔迹可见）——保留 `--dark` 修饰类逻辑。

### Common patterns
- `withInstall` 全局注册；预览复用 `/@/components/Preview` 的 `createImgPreview`。

## Dependencies
### Internal
- `/@/api/permission/userSetting`、`/@/store/modules/user`、`/@/components/Modal`、`/@/components/Preview`、`/@/hooks/setting/useRootSetting`、`/@/enums/appEnum`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
