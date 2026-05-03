<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# Core

## Purpose
引擎核心解析层。负责把 `VisualDevEntity` 中存储的表单/列表 JSON 反序列化为强类型模型，并剔除布局类控件、识别子表、归类查询字段，为下游 CodeGen / 列表渲染提供干净的 `FieldsModel` 集合。

## Key Files
| File | Description |
|------|-------------|
| `TemplateAnalysis.cs` | 静态方法 `AnalysisTemplateData` —— 递归剥离 `TABLE/ROW/CARD/TAB/COLLAPSE` 等布局容器，输出扁平字段列表 |
| `TemplateParsingBase.cs` | 解析基类，承载 `VisualDevEntity`、`FormModel`、`ColumnData`、`AppColumnData`、`AllFieldsModel` 等共享上下文 |
| `FormDataParsing.cs` | 表单+列表数据解析服务（`ITransient`），整合用户/数据接口/远程请求/工作流上下文，把请求载荷转为可下发的视图模型 |

## For AI Agents

### Working in this directory
- `TemplateAnalysis` 的 `switch` 必须与 `PoxiaoKeyConst` 保持同步——新增控件类别（容器 / 文本类 / 数据类）需要在此分类。
- `FormDataParsing` 通过构造函数注入大量管理器（`IUserManager`、`IDataInterFaceManager` 等），新增依赖请保持 DI 习惯，不要 new。
- `TemplateParsingBase` 为可继承基类，不要把状态字段改为 readonly——子类会赋值。

### Common patterns
- `JObject.ToObject<T>(...)` + Mapster 双重映射处理 JSON。
- 容器控件递归 `AnalysisTemplateData(config.children)` 展开。

## Dependencies
### Internal
- `../Model/`、`Poxiao.VisualDev.Entitys`、`Poxiao.Systems.*`、`Poxiao.WorkFlow.Entitys`

### External
- Mapster、Newtonsoft.Json.Linq、SqlSugar

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
