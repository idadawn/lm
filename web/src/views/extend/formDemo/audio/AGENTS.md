<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# audio

## Purpose
浏览器录音示例。演示麦克风录音的开始 / 暂停 / 继续 / 停止 / 销毁与录音回放（暂停 / 继续 / 停止），并显示录音和播放时长，便于业务在表单中嵌入语音留言能力。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 单文件 demo：按钮触发 `recorder` 实例方法，模板中显示 `recorder.duration` / `playTime` |

## For AI Agents

### Working in this directory
- 浏览器录音 API 依赖 HTTPS 或 localhost；生产环境需提示用户授权麦克风。
- 实例存放于 `recorder` ref，在 `onUnmounted` 中应调用 `handleDestroy` 释放 `MediaStream`。
- 业务化时需把 base64/Blob 通过 `jnpf-upload-file` 上传到后端，不要把音频留存在前端。

### Common patterns
- 录音 / 播放分离的双指针时长展示。
- 使用 `a-button` 不同 `type` 颜色区分录音状态（primary 开始 / info 暂停 / warning 停止）。

## Dependencies
### External
- `ant-design-vue`（录音 SDK 由项目级或 `recorder-core` 类库提供）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
