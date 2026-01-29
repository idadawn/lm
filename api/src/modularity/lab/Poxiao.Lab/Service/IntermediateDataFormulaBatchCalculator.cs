using System.Collections.Concurrent;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Poxiao.DependencyInjection;
using Poxiao.Lab.Entity;
using Poxiao.Lab.Entity.Config;
using Poxiao.Lab.Entity.Dto.IntermediateData;
using Poxiao.Lab.Entity.Dto.IntermediateDataFormula;
using Poxiao.Lab.Entity.Enums;
using Poxiao.Lab.EventBus;
using Poxiao.Lab.Helpers;
using Poxiao.Lab.Interfaces;
using SqlSugar;

namespace Poxiao.Lab.Service;

public class IntermediateDataFormulaBatchCalculator : ITransient
{
    private const string FormulaTypeCalc = "CALC";
    private const string FormulaTypeJudge = "JUDGE";
    private const string ErrorTypeFormula = "FORMULA";
    private const string ErrorTypeUnit = "UNIT";
    private const string ErrorTypeDependency = "DEPENDENCY";
    private const string ErrorTypeSetValue = "SET_VALUE";

    private static readonly Regex JudgeExpressionRegex = new(
        @"[\[\]\(\)\+\-\*/<>]|RANGE\(|DIFF_FIRST_LAST",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    );

    private readonly ISqlSugarRepository<IntermediateDataEntity> _intermediateRepository;
    private readonly ISqlSugarRepository<UnitDefinitionEntity> _unitRepository;
    private readonly ISqlSugarRepository<IntermediateDataFormulaCalcLogEntity> _calcLogRepository;
    private readonly IIntermediateDataFormulaService _formulaService;
    private readonly IFormulaParser _formulaParser;
    private readonly LabOptions _labOptions;

    public IntermediateDataFormulaBatchCalculator(
        ISqlSugarRepository<IntermediateDataEntity> intermediateRepository,
        ISqlSugarRepository<UnitDefinitionEntity> unitRepository,
        ISqlSugarRepository<IntermediateDataFormulaCalcLogEntity> calcLogRepository,
        IIntermediateDataFormulaService formulaService,
        IFormulaParser formulaParser,
        IOptions<LabOptions> labOptions
    )
    {
        _intermediateRepository = intermediateRepository;
        _unitRepository = unitRepository;
        _calcLogRepository = calcLogRepository;
        _formulaService = formulaService;
        _formulaParser = formulaParser;
        _labOptions = labOptions?.Value ?? new LabOptions();
    }

    public async Task<FormulaCalculationResult> CalculateByBatchAsync(
        string batchId,
        Dictionary<string, UnitPrecisionInfo> unitPrecisions = null
    )
    {
        if (string.IsNullOrWhiteSpace(batchId))
        {
            return new FormulaCalculationResult { Message = "批次ID为空，未执行计算。" };
        }

        var entities = await _intermediateRepository
            .AsQueryable()
            .Where(t => t.BatchId == batchId && t.DeleteMark == null)
            .ToListAsync();

        return await CalculateInternalAsync(entities, batchId, unitPrecisions);
    }

    public async Task<FormulaCalculationResult> CalculateByIdsAsync(
        List<string> intermediateDataIds,
        Dictionary<string, UnitPrecisionInfo> unitPrecisions = null
    )
    {
        if (intermediateDataIds == null || intermediateDataIds.Count == 0)
        {
            return new FormulaCalculationResult { Message = "未传入数据ID，未执行计算。" };
        }

        var ids = intermediateDataIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (ids.Count == 0)
        {
            return new FormulaCalculationResult { Message = "未传入有效数据ID，未执行计算。" };
        }

        var entities = await _intermediateRepository
            .AsQueryable()
            .Where(t => ids.Contains(t.Id) && t.DeleteMark == null)
            .ToListAsync();

        var batchId = entities.Select(t => t.BatchId).FirstOrDefault();
        return await CalculateInternalAsync(entities, batchId, unitPrecisions);
    }

    private async Task<FormulaCalculationResult> CalculateInternalAsync(
        List<IntermediateDataEntity> entities,
        string batchId,
        Dictionary<string, UnitPrecisionInfo> unitPrecisions
    )
    {
        var result = new FormulaCalculationResult { TotalCount = entities?.Count ?? 0 };

        if (entities == null || entities.Count == 0)
        {
            result.Message = "未找到待计算的数据。";
            return result;
        }

        var formulaSet = await LoadFormulasAsync();
        if (formulaSet.CalcFormulas.Count == 0 && formulaSet.JudgeFormulas.Count == 0)
        {
            result.Message = "未找到可用公式，未执行计算。";
            return result;
        }

        var unitPrecisionMap = NormalizeUnitPrecisions(unitPrecisions);
        var calcPlans = BuildFormulaPlans(formulaSet.CalcFormulas, out var cyclicColumns);

        var unitIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var formula in formulaSet.CalcFormulas)
        {
            if (!string.IsNullOrWhiteSpace(formula.UnitId))
            {
                unitIds.Add(formula.UnitId);
            }
        }
        foreach (var info in unitPrecisionMap.Values)
        {
            if (!string.IsNullOrWhiteSpace(info?.UnitId))
            {
                unitIds.Add(info.UnitId);
            }
        }

        var unitMap = await LoadUnitsAsync(unitIds);

        var errorItems = new ConcurrentBag<CalcErrorItem>();
        var resultErrors = new ConcurrentBag<FormulaCalculationError>();
        var successCount = 0;
        var failedCount = 0;

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount)
        };

        await Parallel.ForEachAsync(
            entities,
            parallelOptions,
            (entity, _) =>
            {
                var entityErrors = new List<CalcErrorItem>();
                var contextData = IntermediateDataFormulaCalcHelper.ExtractContextDataFromEntity(
                    entity
                );

                foreach (var plan in calcPlans)
                {
                    CalculateSingleFormula(
                        plan,
                        entity,
                        contextData,
                        unitPrecisionMap,
                        unitMap,
                        cyclicColumns,
                        entityErrors,
                        batchId
                    );
                }

                foreach (var judgeFormula in formulaSet.JudgeFormulas)
                {
                    ApplyJudgeFormula(
                        judgeFormula,
                        entity,
                        contextData,
                        entityErrors,
                        batchId
                    );
                }

                entity.CalcStatusTime = DateTime.Now;
                if (entityErrors.Count > 0)
                {
                    entity.CalcStatus = IntermediateDataCalcStatus.FAILED;
                    entity.CalcErrorMessage = BuildErrorSummary(entityErrors);
                    Interlocked.Increment(ref failedCount);

                    var detail = JsonConvert.SerializeObject(
                        entityErrors.Select(err => new {
                            err.ColumnName,
                            err.FormulaType,
                            err.ErrorType,
                            err.ErrorMessage,
                            err.ErrorDetail
                        })
                    );

                    resultErrors.Add(
                        new FormulaCalculationError
                        {
                            IntermediateDataId = entity.Id,
                            FurnaceNo = entity.FurnaceNo,
                            ErrorMessage = entity.CalcErrorMessage,
                            ErrorDetail = detail
                        }
                    );
                }
                else
                {
                    entity.CalcStatus = IntermediateDataCalcStatus.SUCCESS;
                    entity.CalcErrorMessage = null;
                    Interlocked.Increment(ref successCount);
                }

                foreach (var err in entityErrors)
                {
                    errorItems.Add(err);
                }

                return ValueTask.CompletedTask;
            }
        );

        await UpdateEntitiesAsync(entities);
        await InsertCalcLogsAsync(errorItems);

        result.SuccessCount = successCount;
        result.FailedCount = failedCount;
        result.Errors = resultErrors.ToList();
        result.Message =
            $"计算完成，共{result.TotalCount}条，成功{result.SuccessCount}条，失败{result.FailedCount}条。";

        return result;
    }

    private async Task<FormulaSet> LoadFormulasAsync()
    {
        var all = await _formulaService.GetListAsync();

        var enabled = all
            .Where(t => t.IsEnabled)
            .Where(t =>
                string.IsNullOrWhiteSpace(t.TableName)
                || t.TableName.Equals("INTERMEDIATE_DATA", StringComparison.OrdinalIgnoreCase)
            )
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.CreatorTime)
            .ToList();

        var calcFormulas = enabled
            .Where(t => NormalizeFormulaType(t.FormulaType) == FormulaTypeCalc)
            .GroupBy(t => t.ColumnName ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .Select(t => t.First())
            .ToList();

        var judgeFormulas = enabled
            .Where(t => NormalizeFormulaType(t.FormulaType) == FormulaTypeJudge)
            .GroupBy(t => t.ColumnName ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .Select(t => t.First())
            .ToList();

        return new FormulaSet
        {
            CalcFormulas = calcFormulas,
            JudgeFormulas = judgeFormulas
        };
    }

    private List<FormulaPlan> BuildFormulaPlans(
        List<IntermediateDataFormulaDto> formulas,
        out HashSet<string> cyclicColumns
    )
    {
        cyclicColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var plans = new List<FormulaPlan>();
        if (formulas == null || formulas.Count == 0)
        {
            return plans;
        }

        var formulaMap = formulas.ToDictionary(t => t.ColumnName, StringComparer.OrdinalIgnoreCase);
        var dependencies = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        var dependents = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var formula in formulas)
        {
            var vars = _formulaParser.ExtractVariables(formula.Formula ?? string.Empty);
            var deps = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var variable in vars)
            {
                if (
                    !string.IsNullOrWhiteSpace(variable)
                    && !string.Equals(variable, formula.ColumnName, StringComparison.OrdinalIgnoreCase)
                    && formulaMap.ContainsKey(variable)
                )
                {
                    deps.Add(variable);
                    if (!dependents.TryGetValue(variable, out var list))
                    {
                        list = new List<string>();
                        dependents[variable] = list;
                    }
                    list.Add(formula.ColumnName);
                }
            }

            dependencies[formula.ColumnName] = deps;
        }

        var indegree = dependencies.ToDictionary(
            t => t.Key,
            t => t.Value.Count,
            StringComparer.OrdinalIgnoreCase
        );

        var ready = indegree
            .Where(t => t.Value == 0)
            .Select(t => formulaMap[t.Key])
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.CreatorTime)
            .ThenBy(t => t.ColumnName)
            .ToList();

        var queue = new Queue<IntermediateDataFormulaDto>(ready);
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current == null || !visited.Add(current.ColumnName))
            {
                continue;
            }

            plans.Add(new FormulaPlan { Formula = current });

            if (!dependents.TryGetValue(current.ColumnName, out var nexts))
            {
                continue;
            }

            foreach (var next in nexts)
            {
                if (!indegree.ContainsKey(next))
                {
                    continue;
                }

                indegree[next]--;
                if (indegree[next] == 0 && formulaMap.TryGetValue(next, out var nextFormula))
                {
                    queue.Enqueue(nextFormula);
                }
            }
        }

        if (visited.Count != formulas.Count)
        {
            foreach (var formula in formulas)
            {
                if (!visited.Contains(formula.ColumnName))
                {
                    cyclicColumns.Add(formula.ColumnName);
                    plans.Add(new FormulaPlan { Formula = formula });
                }
            }
        }

        return plans;
    }

    private async Task<Dictionary<string, UnitDefinitionEntity>> LoadUnitsAsync(
        HashSet<string> unitIds
    )
    {
        if (unitIds == null || unitIds.Count == 0)
        {
            return new Dictionary<string, UnitDefinitionEntity>(StringComparer.OrdinalIgnoreCase);
        }

        var units = await _unitRepository
            .AsQueryable()
            .Where(u => unitIds.Contains(u.Id) && u.DeleteMark == null)
            .ToListAsync();

        return units.ToDictionary(u => u.Id, StringComparer.OrdinalIgnoreCase);
    }

    private void CalculateSingleFormula(
        FormulaPlan plan,
        IntermediateDataEntity entity,
        Dictionary<string, object> contextData,
        Dictionary<string, UnitPrecisionInfo> unitPrecisions,
        Dictionary<string, UnitDefinitionEntity> unitMap,
        HashSet<string> cyclicColumns,
        List<CalcErrorItem> errors,
        string batchId
    )
    {
        var formula = plan.Formula;
        if (formula == null || string.IsNullOrWhiteSpace(formula.ColumnName))
        {
            return;
        }

        if (cyclicColumns.Contains(formula.ColumnName))
        {
            AddError(
                errors,
                entity,
                batchId,
                formula,
                ErrorTypeDependency,
                "公式依赖存在循环，已跳过计算。",
                null
            );
            return;
        }

        decimal? calculatedValue = null;
        var formulaError = false;

        if (string.IsNullOrWhiteSpace(formula.Formula))
        {
            calculatedValue = ParseDecimalOrNull(formula.DefaultValue);
        }
        else
        {
            try
            {
                object calcContext = IsRangeFormula(formula.Formula)
                    ? entity
                    : contextData;
                calculatedValue = _formulaParser.Calculate(formula.Formula, calcContext);
            }
            catch (Exception ex)
            {
                formulaError = true;
                AddError(
                    errors,
                    entity,
                    batchId,
                    formula,
                    ErrorTypeFormula,
                    "公式计算异常。",
                    ex.Message
                );
            }
        }

        if (formulaError)
        {
            IntermediateDataFormulaCalcHelper.SetFormulaValueToEntity(
                entity,
                formula.ColumnName,
                null
            );
            return;
        }

        if (!calculatedValue.HasValue)
        {
            calculatedValue = ParseDecimalOrNull(formula.DefaultValue);
        }

        if (calculatedValue.HasValue)
        {
            if (
                !TryApplyUnitConversion(
                    formula,
                    unitPrecisions,
                    unitMap,
                    ref calculatedValue,
                    entity,
                    errors,
                    batchId
                )
            )
            {
                return;
            }

            var precision = GetPrecision(formula, unitPrecisions, unitMap);
            if (precision.HasValue)
            {
                calculatedValue = Math.Round(
                    calculatedValue.Value,
                    precision.Value,
                    MidpointRounding.AwayFromZero
                );
            }
        }

        IntermediateDataFormulaCalcHelper.SetFormulaValueToEntity(
            entity,
            formula.ColumnName,
            calculatedValue
        );

        if (calculatedValue.HasValue)
        {
            contextData[formula.ColumnName] = calculatedValue.Value;
        }
    }

    private void ApplyJudgeFormula(
        IntermediateDataFormulaDto formula,
        IntermediateDataEntity entity,
        Dictionary<string, object> contextData,
        List<CalcErrorItem> errors,
        string batchId
    )
    {
        if (formula == null || string.IsNullOrWhiteSpace(formula.ColumnName))
        {
            return;
        }

        var judgeResult = EvaluateJudgeFormula(formula, entity, contextData);
        if (judgeResult.HasError)
        {
            AddError(
                errors,
                entity,
                batchId,
                formula,
                ErrorTypeFormula,
                judgeResult.ErrorMessage,
                judgeResult.ErrorDetail
            );

            TrySetJudgeValueToEntity(entity, formula.ColumnName, null, out _);
            return;
        }

        if (
            !TrySetJudgeValueToEntity(
                entity,
                formula.ColumnName,
                judgeResult.ResultValue,
                out var setError
            )
        )
        {
            AddError(
                errors,
                entity,
                batchId,
                formula,
                ErrorTypeSetValue,
                "判定结果写入失败。",
                setError
            );
        }
    }

    private JudgeResult EvaluateJudgeFormula(
        IntermediateDataFormulaDto formula,
        IntermediateDataEntity entity,
        Dictionary<string, object> contextData
    )
    {
        if (string.IsNullOrWhiteSpace(formula.Formula))
        {
            return new JudgeResult
            {
                ResultValue = formula.DefaultValue,
                HasError = false
            };
        }

        JArray rules;
        try
        {
            rules = JArray.Parse(formula.Formula);
        }
        catch (Exception ex)
        {
            return new JudgeResult
            {
                HasError = true,
                ErrorMessage = "判定公式解析失败。",
                ErrorDetail = ex.Message
            };
        }

        if (rules.Count == 0)
        {
            return new JudgeResult
            {
                ResultValue = formula.DefaultValue,
                HasError = false
            };
        }

        foreach (var ruleToken in rules)
        {
            if (ruleToken is not JObject rule)
            {
                continue;
            }

            var resultValue = rule["resultValue"]?.ToString();
            try
            {
                if (rule["rootGroup"] is JObject rootGroup)
                {
                    if (EvaluateRuleGroup(rootGroup, entity, contextData))
                    {
                        return new JudgeResult { ResultValue = resultValue };
                    }
                }
                else if (rule["groups"] is JArray groups)
                {
                    foreach (var groupToken in groups)
                    {
                        if (groupToken is not JObject groupObj)
                        {
                            continue;
                        }

                        if (EvaluateRuleGroup(groupObj, entity, contextData))
                        {
                            return new JudgeResult { ResultValue = resultValue };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new JudgeResult
                {
                    HasError = true,
                    ErrorMessage = "判定规则执行失败。",
                    ErrorDetail = ex.Message
                };
            }
        }

        return new JudgeResult
        {
            ResultValue = formula.DefaultValue,
            HasError = false
        };
    }

    private bool EvaluateRuleGroup(
        JObject group,
        IntermediateDataEntity entity,
        Dictionary<string, object> contextData
    )
    {
        if (group == null)
        {
            return false;
        }

        var logic = (group["logic"]?.ToString() ?? "AND").Trim().ToUpperInvariant();
        var conditions = group["conditions"] as JArray;
        var subGroups = group["subGroups"] as JArray;

        bool? conditionResult = null;
        if (conditions != null && conditions.Count > 0)
        {
            conditionResult = logic == "OR" ? false : true;
            foreach (var condToken in conditions)
            {
                if (condToken is not JObject condition)
                {
                    continue;
                }

                var satisfied = EvaluateCondition(condition, entity, contextData);
                if (logic == "OR")
                {
                    if (satisfied)
                    {
                        conditionResult = true;
                        break;
                    }
                }
                else
                {
                    if (!satisfied)
                    {
                        conditionResult = false;
                        break;
                    }
                }
            }
        }

        bool? subGroupResult = null;
        if (subGroups != null && subGroups.Count > 0)
        {
            subGroupResult = logic == "OR" ? false : true;
            foreach (var groupToken in subGroups)
            {
                if (groupToken is not JObject subGroup)
                {
                    continue;
                }

                var satisfied = EvaluateRuleGroup(subGroup, entity, contextData);
                if (logic == "OR")
                {
                    if (satisfied)
                    {
                        subGroupResult = true;
                        break;
                    }
                }
                else
                {
                    if (!satisfied)
                    {
                        subGroupResult = false;
                        break;
                    }
                }
            }
        }

        if (conditionResult == null && subGroupResult == null)
        {
            return false;
        }

        if (conditionResult == null)
        {
            return subGroupResult == true;
        }

        if (subGroupResult == null)
        {
            return conditionResult == true;
        }

        return logic == "OR"
            ? conditionResult == true || subGroupResult == true
            : conditionResult == true && subGroupResult == true;
    }

    private bool EvaluateCondition(
        JObject condition,
        IntermediateDataEntity entity,
        Dictionary<string, object> contextData
    )
    {
        var op = condition["operator"]?.ToString();
        var leftExpr = condition["leftExpr"]?.ToString() ?? condition["fieldId"]?.ToString();
        var rightValue = condition["rightValue"]?.ToString() ?? condition["value"]?.ToString();

        if (string.IsNullOrWhiteSpace(leftExpr) || string.IsNullOrWhiteSpace(op))
        {
            throw new Exception("条件缺少字段或操作符。");
        }

        op = NormalizeOperator(op);
        var leftValue = ResolveLeftValue(leftExpr, entity, contextData);

        if (op == "IS_NULL")
        {
            return IsNullValue(leftValue);
        }

        if (op == "NOT_NULL")
        {
            return !IsNullValue(leftValue);
        }

        if (leftValue is IEnumerable<string> listValue)
        {
            var contains = listValue.Any(v =>
                string.Equals(v, rightValue, StringComparison.OrdinalIgnoreCase)
            );
            return op == "=" ? contains : op == "<>" && !contains;
        }

        if (
            TryGetDecimal(leftValue, out var leftNumber)
            && TryParseDecimal(rightValue, out var rightNumber)
        )
        {
            return CompareNumbers(leftNumber, rightNumber, op);
        }

        if (TryParseBool(leftValue, out var leftBool) && TryParseBool(rightValue, out var rightBool))
        {
            return CompareBooleans(leftBool, rightBool, op);
        }

        var leftText = leftValue?.ToString();
        return CompareStrings(leftText, rightValue, op);
    }

    private object ResolveLeftValue(
        string expression,
        IntermediateDataEntity entity,
        Dictionary<string, object> contextData
    )
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return null;
        }

        if (!IsExpression(expression))
        {
            if (contextData.TryGetValue(expression, out var val))
            {
                return val;
            }

            foreach (var item in contextData)
            {
                if (string.Equals(item.Key, expression, StringComparison.OrdinalIgnoreCase))
                {
                    return item.Value;
                }
            }

            if (
                expression.Equals(
                    nameof(IntermediateDataEntity.AppearanceFeatureCategoryIds),
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return entity.AppearanceFeatureCategoryIdsList;
            }

            if (
                expression.Equals(
                    nameof(IntermediateDataEntity.AppearanceFeatureLevelIds),
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return entity.AppearanceFeatureLevelIdsList;
            }

            if (
                expression.Equals(
                    nameof(IntermediateDataEntity.AppearanceFeatureIds),
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return entity.AppearanceFeatureIdsList;
            }

            var prop = typeof(IntermediateDataEntity).GetProperty(expression);
            return prop?.GetValue(entity);
        }

        object calcContext = IsRangeFormula(expression) ? entity : contextData;
        var computed = _formulaParser.Calculate(expression, calcContext);
        return computed;
    }

    private bool TrySetJudgeValueToEntity(
        IntermediateDataEntity entity,
        string columnName,
        string value,
        out string error
    )
    {
        error = null;
        if (string.IsNullOrWhiteSpace(columnName))
        {
            error = "列名为空。";
            return false;
        }

        var property = typeof(IntermediateDataEntity).GetProperty(columnName);
        if (property == null || !property.CanWrite)
        {
            error = $"未找到可写属性: {columnName}";
            return false;
        }

        var targetType = property.PropertyType;
        var isNullable = Nullable.GetUnderlyingType(targetType) != null;
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (string.IsNullOrWhiteSpace(value))
        {
            if (underlyingType == typeof(string) || isNullable)
            {
                property.SetValue(entity, null);
                return true;
            }

            error = "结果为空且目标类型不可为空。";
            return false;
        }

        try
        {
            object convertedValue;
            if (underlyingType == typeof(string))
            {
                convertedValue = value;
            }
            else if (underlyingType == typeof(int))
            {
                convertedValue = int.Parse(value, CultureInfo.InvariantCulture);
            }
            else if (underlyingType == typeof(decimal))
            {
                convertedValue = decimal.Parse(value, CultureInfo.InvariantCulture);
            }
            else if (underlyingType == typeof(double))
            {
                convertedValue = double.Parse(value, CultureInfo.InvariantCulture);
            }
            else if (underlyingType == typeof(bool))
            {
                if (!TryParseBool(value, out var boolVal))
                {
                    error = $"无法解析布尔值: {value}";
                    return false;
                }
                convertedValue = boolVal;
            }
            else
            {
                convertedValue = Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture);
            }

            property.SetValue(entity, convertedValue);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    private bool TryApplyUnitConversion(
        IntermediateDataFormulaDto formula,
        Dictionary<string, UnitPrecisionInfo> unitPrecisions,
        Dictionary<string, UnitDefinitionEntity> unitMap,
        ref decimal? value,
        IntermediateDataEntity entity,
        List<CalcErrorItem> errors,
        string batchId
    )
    {
        if (!value.HasValue)
        {
            return true;
        }

        if (unitPrecisions == null || unitPrecisions.Count == 0)
        {
            return true;
        }

        var targetUnitId =
            unitPrecisions.TryGetValue(formula.ColumnName, out var info) && info != null
                ? info.UnitId
                : null;

        if (string.IsNullOrWhiteSpace(formula.UnitId) || string.IsNullOrWhiteSpace(targetUnitId))
        {
            return true;
        }

        if (string.Equals(formula.UnitId, targetUnitId, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (
            !unitMap.TryGetValue(formula.UnitId, out var fromUnit)
            || !unitMap.TryGetValue(targetUnitId, out var toUnit)
        )
        {
            AddError(
                errors,
                entity,
                batchId,
                formula,
                ErrorTypeUnit,
                "单位未找到，无法换算。",
                $"from={formula.UnitId}, to={targetUnitId}"
            );
            return false;
        }

        if (!string.Equals(fromUnit.CategoryId, toUnit.CategoryId, StringComparison.OrdinalIgnoreCase))
        {
            AddError(
                errors,
                entity,
                batchId,
                formula,
                ErrorTypeUnit,
                "单位维度不一致，无法换算。",
                $"{fromUnit.CategoryId} -> {toUnit.CategoryId}"
            );
            return false;
        }

        if (fromUnit.ScaleToBase == 0 || toUnit.ScaleToBase == 0)
        {
            AddError(
                errors,
                entity,
                batchId,
                formula,
                ErrorTypeUnit,
                "单位比例系数异常，无法换算。",
                $"from={fromUnit.ScaleToBase}, to={toUnit.ScaleToBase}"
            );
            return false;
        }

        var baseValue = (value.Value * fromUnit.ScaleToBase) + fromUnit.Offset;
        value = (baseValue - toUnit.Offset) / toUnit.ScaleToBase;
        return true;
    }

    /// <summary>
    /// Calculates the effective precision to use for a formula based on configuration settings, formula properties, and
    /// unit definitions.
    /// </summary>
    /// <remarks>If precision adjustment is not enabled in the configuration, the method returns the maximum allowed
    /// precision. The method checks for precision in the following order: the formula's own precision, the precision
    /// associated with the formula's unit name, the precision defined in the global unit map, and finally the default
    /// precision from configuration if specified.</remarks>
    /// <param name="formula">The formula for which to determine the precision. May specify its own precision or reference a unit whose precision
    /// should be used.</param>
    /// <param name="unitPrecisions">A dictionary that maps unit names to their associated precision information. Used to look up precision when not
    /// specified directly by the formula.</param>
    /// <param name="unitMap">A dictionary that maps unit identifiers to their definitions, which may include precision information if not
    /// available elsewhere.</param>
    /// <returns>The effective precision as an integer, limited by the maximum allowed precision, or null if no precision can be
    /// determined.</returns>
    private int? GetPrecision(
       IntermediateDataFormulaDto formula,
       Dictionary<string, UnitPrecisionInfo> unitPrecisions,
       Dictionary<string, UnitDefinitionEntity> unitMap)
    {
        var config = _labOptions?.Formula;
        var maxPrecision = config?.MaxPrecision ?? 6;

        // 如果未启用调整，直接返回最大值
        if (config?.EnablePrecisionAdjustment != true) return maxPrecision;

        // 链式回退查找：1. 公式定义 -> 2. 这里的单位精度 -> 3. 全局单位定义 -> 4. 默认配置
        int? rawPrecision = formula.Precision
            ?? unitPrecisions?.GetValueOrDefault(formula.ColumnName)?.DecimalPlaces
            ?? (string.IsNullOrWhiteSpace(formula.UnitId) ? null : unitMap?.GetValueOrDefault(formula.UnitId)?.Precision)
            ?? (config?.DefaultPrecision > 0 ? config.DefaultPrecision : null);

        // 应用最大精度限制 (Math.Min 不支持 int?, 所以需要处理下)
        return rawPrecision.HasValue ? Math.Min(rawPrecision.Value, maxPrecision) : null;
    }

    /// <summary>
    /// Normalizes the unit precision dictionary to use case-insensitive string comparison for unit keys.
    /// </summary>
    /// <param name="unitPrecisions">A dictionary that maps unit names to their associated precision information. The dictionary's key comparison may
    /// be case-sensitive or case-insensitive.</param>
    /// <returns>A dictionary containing the same unit precision information as the input, but with case-insensitive string
    /// comparison for the keys. If the input is null or empty, returns an empty dictionary with case-insensitive
    /// comparison. If the input already uses case-insensitive comparison, returns the original dictionary.</returns>
    private static Dictionary<string, UnitPrecisionInfo> NormalizeUnitPrecisions(
        Dictionary<string, UnitPrecisionInfo> unitPrecisions
    )
    {
        if (unitPrecisions == null || unitPrecisions.Count == 0)
        {
            return new Dictionary<string, UnitPrecisionInfo>(StringComparer.OrdinalIgnoreCase);
        }

        if (unitPrecisions.Comparer == StringComparer.OrdinalIgnoreCase)
        {
            return unitPrecisions;
        }

        return new Dictionary<string, UnitPrecisionInfo>(
            unitPrecisions,
            StringComparer.OrdinalIgnoreCase
        );
    }

    private static string NormalizeFormulaType(string formulaType)
    {
        return string.IsNullOrWhiteSpace(formulaType)
            ? string.Empty
            : formulaType.Trim().ToUpperInvariant();
    }

    private static string NormalizeOperator(string op)
    {
        if (string.IsNullOrWhiteSpace(op))
        {
            return string.Empty;
        }

        op = op.Trim();
        if (op == "==")
        {
            return "=";
        }

        return op.ToUpperInvariant();
    }

    private static bool IsExpression(string expression)
    {
        return JudgeExpressionRegex.IsMatch(expression ?? string.Empty);
    }

    private static bool IsRangeFormula(string formula)
    {
        if (string.IsNullOrWhiteSpace(formula))
        {
            return false;
        }

        return formula.Contains("RANGE(", StringComparison.OrdinalIgnoreCase)
            || formula.Contains("DIFF_FIRST_LAST", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsNullValue(object value)
    {
        if (value == null)
        {
            return true;
        }

        if (value is string text)
        {
            return string.IsNullOrWhiteSpace(text);
        }

        if (value is IEnumerable<string> list)
        {
            return !list.Any();
        }

        return false;
    }

    private static bool TryGetDecimal(object value, out decimal result)
    {
        result = 0m;
        if (value == null)
        {
            return false;
        }

        if (value is decimal dec)
        {
            result = dec;
            return true;
        }

        if (value is int intVal)
        {
            result = intVal;
            return true;
        }

        if (value is long longVal)
        {
            result = longVal;
            return true;
        }

        if (value is double dblVal)
        {
            result = Convert.ToDecimal(dblVal);
            return true;
        }

        if (value is float fltVal)
        {
            result = Convert.ToDecimal(fltVal);
            return true;
        }

        if (value is string text)
        {
            return decimal.TryParse(
                text,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out result
            );
        }

        try
        {
            result = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryParseDecimal(string value, out decimal result)
    {
        result = 0m;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return decimal.TryParse(
            value,
            NumberStyles.Any,
            CultureInfo.InvariantCulture,
            out result
        );
    }

    private static bool TryParseBool(object value, out bool result)
    {
        result = false;
        if (value == null)
        {
            return false;
        }

        if (value is bool boolValue)
        {
            result = boolValue;
            return true;
        }

        var text = value.ToString();
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        if (bool.TryParse(text, out result))
        {
            return true;
        }

        if (text == "1")
        {
            result = true;
            return true;
        }

        if (text == "0")
        {
            result = false;
            return true;
        }

        return false;
    }

    private static bool CompareNumbers(decimal left, decimal right, string op)
    {
        return op switch
        {
            "=" => left == right,
            "<>" => left != right,
            ">" => left > right,
            ">=" => left >= right,
            "<" => left < right,
            "<=" => left <= right,
            _ => false
        };
    }

    private static bool CompareBooleans(bool left, bool right, string op)
    {
        return op switch
        {
            "=" => left == right,
            "<>" => left != right,
            _ => false
        };
    }

    private static bool CompareStrings(string left, string right, string op)
    {
        var comparison = StringComparison.OrdinalIgnoreCase;
        return op switch
        {
            "=" => string.Equals(left, right, comparison),
            "<>" => !string.Equals(left, right, comparison),
            ">" => string.Compare(left, right, comparison) > 0,
            ">=" => string.Compare(left, right, comparison) >= 0,
            "<" => string.Compare(left, right, comparison) < 0,
            "<=" => string.Compare(left, right, comparison) <= 0,
            _ => false
        };
    }

    private static decimal? ParseDecimalOrNull(string value)
    {
        return TryParseDecimal(value, out var result) ? result : null;
    }

    private static void AddError(
        List<CalcErrorItem> errors,
        IntermediateDataEntity entity,
        string batchId,
        IntermediateDataFormulaDto formula,
        string errorType,
        string errorMessage,
        string errorDetail
    )
    {
        errors.Add(
            new CalcErrorItem
            {
                BatchId = batchId,
                IntermediateDataId = entity?.Id,
                ColumnName = formula?.ColumnName,
                FormulaName = formula?.FormulaName,
                DisplayName = !string.IsNullOrWhiteSpace(formula?.DisplayName) ? formula.DisplayName : formula?.ColumnName,
                FormulaType = NormalizeFormulaType(formula?.FormulaType),
                ErrorType = errorType,
                ErrorMessage = errorMessage,
                ErrorDetail = errorDetail
            }
        );
    }

    private static string BuildErrorSummary(List<CalcErrorItem> errors)
    {
        if (errors == null || errors.Count == 0)
        {
            return null;
        }

        var messages = errors
            .Select(e => $"{e.DisplayName}:{e.ErrorMessage}")
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .Take(3);
        return string.Join(" | ", messages);
    }

    private async Task UpdateEntitiesAsync(List<IntermediateDataEntity> entities)
    {
        if (entities == null || entities.Count == 0)
        {
            return;
        }

        const int chunkSize = 200;
        for (var i = 0; i < entities.Count; i += chunkSize)
        {
            var chunk = entities.Skip(i).Take(chunkSize).ToList();
            await _intermediateRepository.UpdateRangeAsync(chunk);
        }
    }

    private async Task InsertCalcLogsAsync(ConcurrentBag<CalcErrorItem> errors)
    {
        if (errors == null || errors.IsEmpty)
        {
            return;
        }

        var logEntities = errors
            .Select(err =>
            {
                var log = new IntermediateDataFormulaCalcLogEntity
                {
                    BatchId = err.BatchId,
                    IntermediateDataId = err.IntermediateDataId,
                    ColumnName = err.ColumnName,
                    FormulaName = err.FormulaName,
                    FormulaType = err.FormulaType,
                    ErrorType = err.ErrorType,
                    ErrorMessage = err.ErrorMessage,
                    ErrorDetail = err.ErrorDetail
                };
                log.Creator();
                return log;
            })
            .ToList();

        const int chunkSize = 200;
        for (var i = 0; i < logEntities.Count; i += chunkSize)
        {
            var chunk = logEntities.Skip(i).Take(chunkSize).ToList();
            await _calcLogRepository.InsertRangeAsync(chunk);
        }
    }

    private sealed class FormulaSet
    {
        public List<IntermediateDataFormulaDto> CalcFormulas { get; init; } = new();
        public List<IntermediateDataFormulaDto> JudgeFormulas { get; init; } = new();
    }

    private sealed class FormulaPlan
    {
        public IntermediateDataFormulaDto Formula { get; init; }
    }

    private sealed class JudgeResult
    {
        public bool HasError { get; init; }
        public string ResultValue { get; init; }
        public string ErrorMessage { get; init; }
        public string ErrorDetail { get; init; }
    }

    private sealed class CalcErrorItem
    {
        public string BatchId { get; init; }
        public string IntermediateDataId { get; init; }
        public string ColumnName { get; init; }
        public string FormulaName { get; init; }
        public string DisplayName { get; init; }
        public string FormulaType { get; init; }
        public string ErrorType { get; init; }
        public string ErrorMessage { get; init; }
        public string ErrorDetail { get; init; }
    }
}
