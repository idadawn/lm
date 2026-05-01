<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Attributes

## Purpose
授权相关特性。`[AppAuthorize(...policies)]` 让方法/类/接口声明所需策略，并在内部以前缀 `<Poxiao.Authorization.AppAuthorizeRequirement>` + 逗号拼接 policies 编码进 `AuthorizeAttribute.Policy`，供 `AppAuthorizationPolicyProvider` 反向解析。`[SecurityDefine(resourceId)]` 用于和资源/菜单系统对接，标记 API 唯一资源 ID。

## Key Files
| File | Description |
|------|-------------|
| `AppAuthorizeAttribute.cs` | 继承 `AuthorizeAttribute`；`Policies` 属性 setter 自动加前缀写入 `Policy`，getter 反向截取 |
| `SecurityDefineAttribute.cs` | 资源标识特性，`ResourceId` 唯一，对接菜单/接口资源管理 |

## For AI Agents

### Working in this directory
- 命名空间使用 `Microsoft.AspNetCore.Authorization`，业务使用方只需 `using` 该 BCL 命名空间。
- `[AppAuthorize]` 是 `sealed`，**不要派生**——派生会破坏 `Policy` 编码约定。
- `[SecurityDefine]` 不强制 `ResourceId`，可用无参构造由资源扫描器自动生成；新建带 ID 的资源时确保全局唯一。

### Common patterns
- 特性内部仅做字符串编码；语义解析放在 `Providers/AppAuthorizationPolicyProvider`。

## Dependencies
### Internal
- `Authorization/Internal/Penetrates.cs`（前缀常量）

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
