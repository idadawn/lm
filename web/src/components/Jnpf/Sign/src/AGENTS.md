<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# src

## Purpose
手写签名功能三件套：触发器 + 弹窗 + 画板。点击 `Sign.vue` 触发器打开 `SignModal.vue`；其中嵌入自研 `esign.vue`（鼠标轨迹 -> base64）；提交时按 `submitOnConfirm` 决定是否调 `createSign` 入库并刷新 user store。

## Key Files
| File | Description |
|------|-------------|
| `Sign.vue` | 触发器：`<img>` 已签预览 + `icon-ym-signature` 图标 + 暗色主题适配；`createImgPreview` 大图预览 |
| `SignModal.vue` | `a-modal` 包装 esign 画板，`handleSubmit` 调 `esignRef.generate()` 拿 base64，按 `submitOnConfirm` 走 emit 或 `createSign` |
| `esign.vue` | 画板核心（鼠标事件 + canvas 绘制 + reset/generate 暴露） |

## For AI Agents

### Working in this directory
- `submitOnConfirm=true` 时：当 `isDefault==1` 或 `isDefault==0 且原本无 signImg` 才更新 `userStore`，否则只 emit；保留该分支以避免覆盖默认签名。
- esign 画板 height/width 固定 300×580，`lineWidth` 由父级 `Sign` 透传——勿在 SignModal 内硬编码。
- `confirmLoading` 在异步入库期间防重复点击，要在所有出口（成功/失败）置回 false。

### Common patterns
- 暗色主题：`html[data-theme='dark'] .jnpf-sign-modal .sign-main { background-color: #fff; }`，使签名区始终为白底。
- 通过 `defineExpose({ openModal })` 让父组件 imperative 调用。

## Dependencies
### Internal
- `/@/api/permission/userSetting`、`/@/store/modules/user`、`/@/components/Modal/src/components/ModalClose.vue`、`/@/components/Preview`、`/@/hooks/setting/useRootSetting`、`/@/enums/appEnum`、`/@/hooks/web/{useDesign,useMessage,useI18n}`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
