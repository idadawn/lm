<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# wxWorkConfig

## Purpose
企业微信账号配置页：维护 CorpId、AgentId、Secret 等，用于企业内部消息推送。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | BasicTable 列表 + 操作列 |
| `Form.vue` | 企业微信账号表单 |

## For AI Agents

### Working in this directory
- 与公众号区分：企业微信使用 CorpId + AgentId 双标识。
- Secret 字段密文处理。

### Common patterns
- 与同级其他 accountConfig 子目录一致。

## Dependencies
### Internal
- `/@/api/msgCenter/accountConfig`
### External
- `ant-design-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
