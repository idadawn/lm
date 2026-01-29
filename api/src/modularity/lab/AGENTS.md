# Repository Guidelines

## Project Structure & Module Organization
- Backend: `api/` (modular monolith). Feature modules live in `api/src/modularity/`, with the **lab** module under `api/src/modularity/lab/`.
- Frontend: `web/` (Vue 3). Feature views mirror backend modules under `web/src/views/` (e.g., `web/src/views/lab/`).
- Tests: `api/tests/` for .NET test projects; `web/` contains frontend tests.
- SQL and scripts: `sql/` for migrations, `scripts/` for utilities, `api/resources/` for templates.

## Build, Test, and Development Commands
Backend:
```powershell
cd api/src/application/Poxiao.API.Entry
dotnet restore
dotnet watch run --launch-profile dev
```
Frontend:
```powershell
cd web
pnpm install
pnpm dev
pnpm build
```
Tests:
```powershell
dotnet test api/tests/
pnpm test
```
Use `docker-compose up -d` to start MySQL/Redis/Qdrant, and `docker-compose --profile ai up -d` for TEI/vLLM.

## Coding Style & Naming Conventions
- C#: PascalCase for classes/methods/properties; private fields use `_camelCase`.
- Vue/TS: Components PascalCase, variables camelCase, CSS classes kebab-case; avoid inline styles.
- Entities must inherit `CLDEntityBase`; follow field naming rules in `.cursorrules` and use `[SugarColumn]` for column mapping.
- Prefer SqlSugar cross-database APIs for MySQL/SQL Server/Oracle compatibility.

## Testing Guidelines
- .NET tests run via `dotnet test`; keep tests colocated in `api/tests/*`.
- Frontend tests run via `pnpm test`. Name tests to match feature/module (e.g., `LabSampleList.spec.ts`).
- Add tests for new business logic and critical workflows.

## Commit & Pull Request Guidelines
- Use Conventional Commits: `feat:`, `fix:`, `docs:`, `style:`, `refactor:`, `perf:`, `test:`, `chore:`, `revert:`, `merge:`, `clean:`.
- PRs should include a clear description, linked issue (if any), and test commands run. Add screenshots for UI changes.

## Security & Configuration Tips
- Configure `.env.local` from `.env.example`.
- AI services are configured in `api/src/application/Poxiao.API.Entry/Configurations/AI.json`.
- Uploads go to `api/src/application/Poxiao.API.Entry/uploads/`; logs go to `api/src/application/Poxiao.API.Entry/logs/`.

## 导入到计算的整体流程（功能与设计说明）

### 1) 总体链路（RawData → IntermediateData → 公式计算 → 日志）
- 导入会话：`RawDataImportSessionService` 负责上传/解析/复核/完成导入，数据先落到导入会话/临时 JSON，再写入 `RAW_DATA` 和 `INTERMEDIATE_DATA`。
- 计算触发：`CompleteImport` 写入中间数据后，通过事件总线发布 `IntermediateData:CalcByBatch`。
- 计算执行：`IntermediateDataCalcEventSubscriber` 订阅事件并调用 `IntermediateDataFormulaBatchCalculator` 批量计算。
- 计算日志：每条计算异常写入 `LAB_INTERMEDIATE_DATA_CALC_LOG`；中间数据写入 `CalcStatus/CalcErrorMessage/CalcStatusTime`。

### 2) 从导入开始到计算结束的接口/方法顺序
1. 创建导入会话  
   - 接口：`POST /api/lab/raw-data-import-session/create`  
   - 服务方法：`RawDataImportSessionService.Create`
2. 上传并解析文件（会话进入进行中）  
   - 接口：`POST /api/lab/raw-data-import-session/step1/upload-and-parse`  
   - 方法：`RawDataImportSessionService.UploadAndParse`
3. 匹配并确认产品规格  
   - 接口：`GET /api/lab/raw-data-import-session/{sessionId}/product-specs`  
   - 方法：`RawDataImportSessionService.GetProductSpecMatches`  
   - 接口：`PUT /api/lab/raw-data-import-session/{sessionId}/product-specs`  
   - 方法：`RawDataImportSessionService.UpdateProductSpecs`
4. 重复数据选择（可选）  
   - 接口：`PUT /api/lab/raw-data-import-session/{sessionId}/duplicate-selections`  
   - 方法：`RawDataImportSessionService.UpdateDuplicateSelections`
5. 外观/缺陷特征匹配（可选）  
   - 接口：`GET /api/lab/raw-data-import-session/{sessionId}/features`  
   - 方法：`RawDataImportSessionService.GetFeatureMatches`  
   - 接口：`PUT /api/lab/raw-data-import-session/{sessionId}/features`  
   - 方法：`RawDataImportSessionService.UpdateFeatures`
6. 复核与分页查看解析数据  
   - 接口：`GET /api/lab/raw-data-import-session/{sessionId}/review`  
   - 方法：`RawDataImportSessionService.GetReviewData`  
   - 接口：`GET /api/lab/raw-data-import-session/{sessionId}/review-data`  
   - 方法：`RawDataImportSessionService.GetReviewDataPage`
7. 完成导入并触发计算  
   - 接口：`POST /api/lab/raw-data-import-session/{sessionId}/complete`  
   - 方法：`RawDataImportSessionService.CompleteImport`  
   - 关键动作：  
     - 解析模板配置，构造 `unitPrecisions`（字段 → 单位/精度）。  
     - 从 JSON 读取 `validData`，按 `ProductSpecId` 分组。  
     - 调用 `IIntermediateDataService.GenerateIntermediateDataAsync` 生成中间数据实体（含 `batchId`）。  
     - 事务写入原始数据与中间数据。  
     - 发布事件：`IntermediateData:CalcByBatch`（`IntermediateDataCalcEventSource`）。
8. 事件订阅触发批量计算  
   - 订阅者：`IntermediateDataCalcEventSubscriber.HandleCalcByBatch`  
   - 执行器：`IntermediateDataFormulaBatchCalculator.CalculateByBatchAsync`

说明：另有 `POST /api/lab/raw-data-import-session/simple-import`（`RawDataImportSessionService.SimpleImport`）用于简化导入；此路径若需要计算，需与 `CompleteImport` 保持一致的事件触发策略。

---

## 公式解析与计算逻辑

### A. 公式维护类型（CALC/JUDGE/NO）
- 枚举：`IntermediateDataFormulaType`  
  - `CALC`：计算公式  
  - `JUDGE`：判定公式  
  - `NO`：仅展示
- 取公式来源：`IIntermediateDataFormulaService.GetListAsync()` → 过滤启用、表名为 `INTERMEDIATE_DATA` 或空 → 按 `FormulaType` 分组，仅保留同一列第一条。

### B. 计算公式（CALC）解析与执行
1. 依赖解析与执行顺序  
   - 通过 `FormulaParser.ExtractVariables` 提取公式变量。  
   - 构建依赖图与拓扑序（`BuildFormulaPlans`）。  
   - 若存在循环依赖列，标记为依赖错误（`DEPENDENCY`）并跳过该列。
2. 上下文数据构建  
   - `IntermediateDataFormulaCalcHelper.ExtractContextDataFromEntity`：  
     - 将中间数据实体中可转为数值的字段纳入上下文。  
     - 补充 `Length/Layers/Density` 与 `Detection1..22/Thickness1..22/LaminationDist1..22`。  
     - 若有 `DetectionColumns` 则加入上下文供范围展开使用。
3. 公式解析与计算  
   - 使用 `FormulaParser.Calculate`：  
     - `IF` 自动替换为 `IIF`；  
     - 支持 `[Start] TO [End]` 范围展开（如 `Detection1 TO DetectionColumns`）；  
     - 支持统计函数 `SUM/AVG/MAX/MIN`；  
     - 支持 `RANGE(...)` / `DIFF_FIRST_LAST` 范围公式（由 `RangeFormulaCalculator` 处理）。
4. 默认值与异常处理  
   - 公式为空：使用 `DefaultValue`。  
   - 公式执行异常：记录错误（`FORMULA`），该列值写 `null`。
5. 单位与精度  
   - 若导入单位与公式单位不同，按 `UnitDefinitionEntity` 的 `ScaleToBase/Offset` 做换算。  
   - 单位不可换算：记录错误（`UNIT`），该列跳过（保持 `null`）。  
   - 精度优先级：公式精度 → 导入精度（`unitPrecisions`）→ 单位默认精度。  
   - 四舍五入使用 `MidpointRounding.AwayFromZero`。

### C. 判定公式（JUDGE）解析与执行
1. 规则结构  
   - `JUDGE` 公式是 JSON 数组：每条 rule 包含 `resultValue` 与 `rootGroup` 或 `groups`。  
   - 每个 group 支持 `logic`（AND/OR）、`conditions`、`subGroups`。
2. 条件解析  
   - 条件字段：`leftExpr/fieldId`、`operator`、`rightValue/value`。  
   - `leftExpr` 支持：  
     - 字段名（直接从上下文/实体取值）；  
     - 表达式（含算符或 `RANGE/DIFF_FIRST_LAST` 时自动公式计算）。  
   - 操作符支持：`=, <>, >, >=, <, <=, IS_NULL, NOT_NULL`。  
   - 列表字段（如外观缺陷 ID 列表）支持包含判断。
3. 结果写入  
   - 满足规则即写入 `resultValue`；若无匹配则写入 `DefaultValue`。  
   - 写入失败记录 `SET_VALUE` 错误并置空。  
   - 判定解析/执行异常记录 `FORMULA` 错误并置空。

### D. 计算结果落库与日志
- 中间数据：  
  - 成功：`CalcStatus = SUCCESS`，清空错误信息。  
  - 失败：`CalcStatus = FAILED`，`CalcErrorMessage` 为摘要。
- 计算日志：写入 `LAB_INTERMEDIATE_DATA_CALC_LOG`（字段：批次、列名、公式类型、错误类型、错误详情）。
