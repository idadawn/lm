<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Attributes

## Purpose
`App` 子系统的特性集合，目前承载启动模块的排序声明，让 `AppStartup` 派生类可声明加载次序（数字越小越先执行）。

## Key Files
| File | Description |
|------|-------------|
| `AppStartupAttribute.cs` | `[AppStartup(int order)]`：标注于 `AppStartup` 派生类，控制 `Configure*` 方法的注入与执行顺序 |

## For AI Agents

### Working in this directory
- 仅放与启动流程相关的 `Attribute`；其它业务用特性（鉴权、动态 API 路由）应归入对应模块（`Authorization/`、`DynamicApiController/`）。
- 新增启动相关特性时，类上保持 `[SuppressSniffer, AttributeUsage(...)]` 双标注，与 `AppStartupAttribute` 保持一致。

### Common patterns
- 简洁的 POCO Attribute：构造函数接受关键参数（如 `order`），公开属性可被框架反射读取。

## Dependencies
### Internal
- 被 `App/Startups/AppStartup.cs` 与启动扫描器消费。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
