<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# DataInterFace (Model)

## Purpose
数据接口（HTTP/SQL）配置中嵌套的 JSON 解析 Model。`DataInterfaceEntity.F_PROPERJSON` 存储字符串，运行期反序列化到这些 Model 上以执行调用。

## Key Files
| File | Description |
|------|-------------|
| `DataInterfaceProperJson.cs` | 接口属性 JSON：URL、方法、Header、Body、SQL、参数列表、返回处理等 |
| `DataInterfaceReqParameter.cs` | 接口请求参数定义：名称、类型、默认值、必填、来源（query/body/path） |

## For AI Agents

### Working in this directory
- `DataInterfaceService` 调用时会先 `JsonConvert.DeserializeObject<DataInterfaceProperJson>(...)` 再驱动 HTTP/SQL 执行；新增字段必须设置默认值避免老数据为空导致空引用。
- 修改字段名相当于改 JSON 协议，所有历史数据接口配置可能失效，慎重。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
