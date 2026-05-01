<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# InterfaceOauth (Dto)

## Purpose
接口 OAuth 凭据 DTO。配置外部接口的 OAuth 客户端信息（client_id、secret、token endpoint），供 `DataInterface` 在调用第三方接口时复用。

## Key Files
| File | Description |
|------|-------------|
| `InterfaceOauthInput.cs` | 查询输入 |
| `InterfaceOauthSaveInput.cs` | 保存输入（创建/更新合一） |
| `InterfaceOauthOutput.cs` | 单条输出（敏感字段脱敏） |
| `InterfaceOauthListOutput.cs` | 列表 |

## For AI Agents

### Working in this directory
- `client_secret` 在 Output 中脱敏；写入时使用 `Poxiao.DataEncryption` 加密落库。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
