<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Cache

## Purpose
统一缓存抽象与实现。`ICache` 提供 `Del/Exists/Get/Set/Keys/Pattern` 等基础操作，`MemoryCache` 与 `RedisCache`（基于 CSRedis）双实现按 `CacheOptions.CacheType` 通过命名解析切换；`CacheManager` 是业务侧默认入口，过滤 `mini-profiler` 前缀键，提供管理面板友好的 API。

## Key Files
| File | Description |
|------|-------------|
| `ICache.cs` | 基础缓存接口（同步/异步双轨：`Del`/`DelAsync`/`DelByPatternAsync`/`Exists`/...） |
| `ICacheManager.cs` | 高层缓存管理接口（含枚举键、清理） |
| `CacheManager.cs` | `IScoped` 实现：通过 `Func<string, ISingleton, object> resolveNamed` 解析具体 `ICache`，过滤 MiniProfiler 内部键 |
| `MemoryCache.cs` | 内存缓存实现（开发/单机） |
| `RedisCache.cs` | `ISingleton` 实现，使用 CSRedis 客户端，连接串模板 `{0}:{1},password={2}` |
| `CacheOptions.cs` | 缓存配置：`CacheType`（Memory/Redis）、`ip`/`port`/`password`/`RedisConnectionString` |

## For AI Agents

### Working in this directory
- 业务代码注入 `ICacheManager`，不要直接依赖 `MemoryCache`/`RedisCache`，便于按配置切换。
- 新增缓存方法时同步在 `ICache` 加签名并在两个实现中提供 Memory/Redis 版本，避免某种环境下 `NotImplementedException`。
- `CacheOptions.RedisConnectionString` 是模板字符串（`string.Format` 占位），保留 `{0}/{1}/{2}` 顺序。

### Common patterns
- DI 标记接口：`MemoryCache : ICache, ISingleton`、`RedisCache : ICache, ISingleton`，`CacheManager : ICacheManager, IScoped`。
- “Pattern 删除”是 Redis-friendly API，业务避免在 MemoryCache 上滥用。

## Dependencies
### Internal
- `Poxiao.DependencyInjection`、`Poxiao.ConfigurableOptions`
### External
- `CSRedisCore`、`Microsoft.Extensions.Caching.Memory`、`Microsoft.Extensions.Options`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
