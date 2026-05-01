<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# HVideo

## Purpose
门户运行时视频卡片。播放静态 URL 或动态接口返回的视频(MP4),支持自动播放、循环、静音、本地上传文件经 `apiUrl` 前缀拼接的两种 styleType。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | `<video>` 元素 + 动态 key 强制刷新;空源使用 portal-nodata 占位 |

## For AI Agents

### Working in this directory
- `styleType==1` 表示后台上传文件(`val.url` + `globSetting.apiUrl`);其他类型为外链直接使用。
- `key` 使用 `+new Date()` 在 `getValue` 中重算,触发 video 元素重新挂载;切勿移除否则切换源不刷新。

### Common patterns
- 复用 `CardHeader`,与其他 H* 组件保持卡片头一致。
- `playNumber === 2` 表示循环播放;布尔字段直接绑定到原生属性。

## Dependencies
### Internal
- `../CardHeader/index.vue`
- `/@/api/systemData/dataInterface` (`getDataInterfaceRes`)
- `/@/hooks/setting` (`useGlobSetting`)
### External
- 原生 `<video>`、Ant Design Vue (`a-card`)

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
