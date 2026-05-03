<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# wxWananchiConfig

## Purpose
微信公众号（万安池/微信公众平台）账号配置页：维护 AppId、AppSecret、Token 等，用于公众号消息推送。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | BasicTable 列表 + 操作列 |
| `Form.vue` | 公众号账号表单 |

## For AI Agents

### Working in this directory
- AppSecret 与 webhook 类似按敏感字段处理，编辑不回填明文。
- 注意目录名拼写为 `wxWananchi`（沿用原命名，勿改）。

### Common patterns
- 与同级其他 accountConfig 子目录一致。

## Dependencies
### Internal
- `/@/api/msgCenter/accountConfig`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
