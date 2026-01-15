using Microsoft.Extensions.Logging;
using Minio.DataModel.Replication;
using NPOI.SS.Formula.Functions;
using NPOI.Util;
using System.Globalization;
using static System.String;

#pragma warning disable SA1507
#pragma warning disable CA1307

namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标数据服务.
/// </summary>
public class MetricDataService : IMetricDataService, ITransient
{
    /// <summary>
    /// 获取指标信息服务.
    /// </summary>
    private readonly IMetricInfoService _metricInfoService;

    /// <summary>
    /// 数据服务.
    /// </summary>
    private readonly IDbService _dbService;

    private readonly IMetricGradedService _metricGradedService;

    private ILogger<MetricDataService> _logger;

    /// <summary>
    /// 构造函数.
    /// </summary>
    /// <param name="dbService"></param>
    /// <param name="metricInfoService"></param>
    /// <param name="logger"></param>
    public MetricDataService(IDbService dbService, IMetricInfoService metricInfoService, ILogger<MetricDataService> logger, IMetricGradedService metricGradedService)
    {
        _dbService = dbService;
        _metricInfoService = metricInfoService;
        _logger = logger;
        _metricGradedService = metricGradedService;
    }

    /// <inheritdoc />
    public async Task<MetricDataOutput> GetDataAsync(string metricId)
    {
        var result = new MetricDataOutput();
        var info = await _metricInfoService.GetAsync(metricId);
        if (info == null) throw Oops.Oh(ErrorCode.K10019);
        switch (info.Type)
        {
            case MetricType.Basic:
                if (info.MetricDataType.Equals(MetricDataType.RealTime))
                    await GetRealDataAsync(info, result);
                else
                    await GetBasicDataAsync(info, result);
                break;
            case MetricType.Derive:
                break;
            case MetricType.Composite:
                await GetCompositeDataAsync(info, result);
                break;
        }
        return result;
    }

    /// <summary>
    /// 获取基础指标值.
    /// </summary>
    /// <param name="info"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    private async Task GetBasicDataAsync(MetricInfoInfoOutput info, MetricDataOutput result)
    {
        var query = new ModelDataAggQueryInput();
        query.TaskId = SnowflakeIdHelper.NextId();
        query.LinkId = info.DataModelId.ParentId;
        query.SchemaName = info.DataModelId.Id;
        query.ColumnField = info.Column;
        query.AggType = info.AggType.ToEnum<DBAggType>();
        query.Filters = info.Filters;
        // 如果时间维度存在使用时间维度.
        // 如果时间维度不存在使用第一个维度信息.
        // 如果维度信息不存在返回所有聚合数据.
        TableFieldOutput? dimension = null;
        if (info.TimeDimensions == null)
        {
            if (info.Dimensions is { Count: > 0 })
                dimension = info.Dimensions[0];
        }
        else
        {
            dimension = new TableFieldOutput()
            {
                field = info.TimeDimensions.Field,
                fieldName = info.TimeDimensions.FieldName,
                dataType = info.TimeDimensions.DataType
            };
            query.Granularity = info.TimeDimensions.Granularity;
            query.DisplayOption = info.TimeDimensions.DisplayOption;
        }

        query.Dimensions = dimension;
        var data = await _dbService.GetMetricDataAsync(query);

        // 处理data数据.
        try
        {
            if (info.Format != null)
                data.Data = DealData(info.Format, data.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"获取基础指标值异常,{data},指标信息:{info.Id}");
        }

        result.Data = data;
        result.MetricInfo = new MetricBasicInfo()
        {
            Name = info.Name,
            Code = info.Code,
            Type = info.Type
        };
    }

    /// <summary>
    /// 获取复合指标值.
    /// </summary>
    /// <param name="info"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    private async Task GetCompositeDataAsync(MetricInfoInfoOutput info, MetricDataOutput result)
    {
        result.Data = new ModelDataOutput();
        // 获取当前指标的父级指标.
        var parent = info.ParentIds;

        // 解析公式,${指标1}/${指标2},去除${}
        if (info.Expression == null) throw Oops.Oh(ErrorCode.K10020);

        info.Expression = info.Expression.Replace("${", Empty).Replace("}", "");

        var formula = new Expression(info.Expression);
        if (formula.HasErrors())
            throw Oops.Oh(ErrorCode.K10018);

        var dic = new Dictionary<string, string>();

        // 获取各个指标的数据.
        foreach (var metricId in parent)
        {
            var metricDataOutput = await GetDataAsync(metricId);
            var metricValue = metricDataOutput.Data.Data.IsNullOrEmpty() ? "0" : metricDataOutput.Data.Data;
            dic.Add(metricId, metricValue);
            info.Expression = info.Expression.ReplaceMetricValue(metricId, metricValue);
            result.Data.ExecutedSql += metricDataOutput.Data.ExecutedSql;
            result.Data.TotalTime += metricDataOutput.Data.TotalTime;
            result.Data.Metas.AddRange(metricDataOutput.Data.Metas);
        }

        formula = new Expression(info.Expression);
        result.Data.Data = formula.Evaluate().ToString()!;

        // 处理data数据.
        try
        {
            if (info.Format != null)
                result.Data.Data = DealData(info.Format, result.Data.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"获取复合指标值异常,{result.Data.Data},指标信息:{info.Id}");
        }


        result.MetricInfo = new MetricBasicInfo()
        {
            Name = info.Name,
            Code = info.Code,
            Type = info.Type
        };
        return;
    }


    /// <summary>
    /// 获取基础指标值.
    /// </summary>
    /// <param name="info"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    private async Task GetRealDataAsync(MetricInfoInfoOutput info, MetricDataOutput result)
    {
        var query = new RealDataQryInput();
        query.key = info.Column.field;
        query.Name = info.DataModelId.Id;
        var data = await _dbService.GetRealDataAsync(query);

        // 处理data数据.
        try
        {
            if (info.Format != null)
                data.Data = DealData(info.Format, data.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"获取基础指标值异常,{data},指标信息:{info.Id}");
        }

        result.Data = data;
        result.MetricInfo = new MetricBasicInfo()
        {
            Name = info.Name,
            Code = info.Code,
            Type = info.Type
        };
    }


    /// <inheritdoc />
    public async Task<MetricChartDataOutput> GetChartDataAsync(MetricDataQryInput input)
    {
        var result = new MetricChartDataOutput();
        var info = await _metricInfoService.GetAsync(input.MetricId);
        if (info == null) throw Oops.Oh(ErrorCode.K10019);
        TableFieldOutput? timeDim = null;
        if (input.TimeDimension != null)
            timeDim = info.Dimensions?.FirstOrDefault(x => x.fieldName.Contains("时间") || x.fieldName.Contains("日期"));
        if (timeDim != null)
        {
            input.Filters ??= new List<MetricFilterDto>();
            input.Filters.Add(new MetricFilterDto()
            {
                WhereType = MetricWhereType.And,
                DataType = timeDim.dataType,
                Field = timeDim.field,
                FieldName = timeDim.fieldName,
                Type = MetricFilterModel.ByRange,
                MinValue = "",
                MaxValue = ""
            });
        }

        switch (info.Type)
        {
            case MetricType.Basic:
                if (info.MetricDataType.Equals(MetricDataType.RealTime))
                    await BasicRealDataChartDataAsync(input, info, result);
                else
                    await BasicMetricChartDataAsync(input, info, result);
                break;
            case MetricType.Derive:
                await DeriveMetricChartDataAsync(input, info, result);
                break;
            case MetricType.Composite:
                await CompositeMetricChartDataAsync(input, info, result);
                break;
        }
        return result;
    }

    /// <summary>
    /// 处理基础指标数据.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="info"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    private async Task BasicMetricChartDataAsync(MetricDataQryInput input, MetricInfoInfoOutput info, MetricChartDataOutput result)
    {
        var query = new ModelDataAggQueryInput();
        query.TaskId = SnowflakeIdHelper.NextId();
        query.LinkId = info.DataModelId.ParentId;
        query.SchemaName = info.DataModelId.Id;
        query.ColumnField = info.Column;
        query.AggType = info.AggType.ToEnum<DBAggType>();
        if (info.Filters == null)
            info.Filters = input.Filters;
        else if (input.Filters != null)
            info.Filters.AddRange(input.Filters);
        query.Filters = input.Filters;
        query.Limit = input.Limit;
        query.SortBy = input.SortBy;
        result.MetricInfo = new MetricBasicInfo()
        {
            Name = info.Name,
            Code = info.Code,
            Type = info.Type
        };
        // 如果维度信息不存在返回所有聚合数据.
        if (info.TimeDimensions == null)
            return;

        query.Dimensions = input.Dimensions ?? info.Dimensions?[0];
        if (info.TimeDimensions != null)
            query.Granularity = info.TimeDimensions.Granularity;
        var data = await _dbService.GetMetricChartDataAsync(query);
        result.Data = data;
        result.MetricInfoGrade = await _metricGradedService.GetGradeExtInfoAsync(info.Id);
        //result.MetricInfoGrade = new List<MetricInfoGradeExtInput>()
        //{
        //    new MetricInfoGradeExtInput()
        //    {
        //        Id = "0",
        //        Name = "累计",
        //        Value = "7839.0",
        //        IsShow = true,
        //        MetricId = input.MetricId,
        //        StatusColor = ""
        //    },
        //    new MetricInfoGradeExtInput()
        //    {
        //        Id = "2",
        //        Name = "基准",
        //        Value = "6198",
        //        IsShow = true,
        //        MetricId = input.MetricId,
        //        StatusColor = ""
        //    },
        //    new MetricInfoGradeExtInput()
        //    {
        //        Id = "3",
        //        Name = "一级",
        //        Value = "6688",
        //        IsShow = true,
        //        MetricId = input.MetricId,
        //        StatusColor = ""
        //    },
        //    new MetricInfoGradeExtInput()
        //    {
        //        Id = "4",
        //        Name = "二级",
        //        Value = "7225",
        //        IsShow = true,
        //        MetricId = input.MetricId,
        //        StatusColor = ""
        //    },
        //    new MetricInfoGradeExtInput()
        //    {
        //        Id = "5",
        //        Name = "三级",
        //        Value = "7736",
        //        IsShow = true,
        //        MetricId = input.MetricId,
        //        StatusColor = ""
        //    },
        //};
        return;
    }

    /// <summary>
    /// 处理派生指标数据.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="info"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    private async Task DeriveMetricChartDataAsync(MetricDataQryInput input, MetricInfoInfoOutput info, MetricChartDataOutput result)
    {
        result.Data = new ModelChartDataOutput();
        // 获取当前指标的父级指标.
        var parent = info.ParentIds;

        // 解析公式,${指标1}/${指标2},去除${}
        if (info.Expression == null) throw Oops.Oh(ErrorCode.K10020);

        info.Expression = info.Expression.Replace("${", Empty).Replace("}", "");

        var formula = new Expression(info.Expression);
        if (formula.HasErrors())
            throw Oops.Oh(ErrorCode.K10018);

        var dic = new Dictionary<string, string>();
        // 获取各个指标的数据.
        foreach (var metricId in parent)
        {
            input.MetricId = metricId;
            var metricDataOutput = await GetChartDataAsync(input);
            var metricValue = metricDataOutput.Data.List;
            foreach (var value in metricValue)
            {
                dic.Add(value.Dimension, info.Expression.ReplaceMetricValue(metricId, value.Value));
            }
            result.Data.ExecutedSql += metricDataOutput.Data.ExecutedSql;
            result.Data.TotalTime += metricDataOutput.Data.TotalTime;
            result.Data.Metas.AddRange(metricDataOutput.Data.Metas);
        }

        var rlt = new List<ChartData>();
        foreach (var exp in dic)
        {
            formula = new Expression(info.Expression);
            try
            {
                rlt.Add(new ChartData()
                {
                    Dimension = exp.Key,
                    Value = formula.Evaluate().ToString()
                });
            }
            catch
            {
                // ignored.
            }
        }

        result.Data.Data = rlt.Select(data => new List<object?>() { data.Dimension, data.Value }).ToList();
        result.MetricInfoGrade = await _metricGradedService.GetGradeExtInfoAsync(info.Id);
        //result.MetricInfoGrade = new List<MetricInfoGradeExtInput>()
        //{
        //    new MetricInfoGradeExtInput()
        //    {
        //        Id = "0",
        //        Name = "累计",
        //        Value = "7839.0",
        //        IsShow = true,
        //        MetricId = input.MetricId,
        //        StatusColor = ""
        //    },
        //    new MetricInfoGradeExtInput()
        //    {
        //        Id = "2",
        //        Name = "基准",
        //        Value = "6198",
        //        IsShow = true,
        //        MetricId = input.MetricId,
        //        StatusColor = ""
        //    },
        //    new MetricInfoGradeExtInput()
        //    {
        //        Id = "3",
        //        Name = "一级",
        //        Value = "6688",
        //        IsShow = true,
        //        MetricId = input.MetricId,
        //        StatusColor = ""
        //    },
        //    new MetricInfoGradeExtInput()
        //    {
        //        Id = "4",
        //        Name = "二级",
        //        Value = "7225",
        //        IsShow = true,
        //        MetricId = input.MetricId,
        //        StatusColor = ""
        //    },
        //    new MetricInfoGradeExtInput()
        //    {
        //        Id = "5",
        //        Name = "三级",
        //        Value = "7736",
        //        IsShow = true,
        //        MetricId = input.MetricId,
        //        StatusColor = ""
        //    },
        //};
        return;
    }

    /// <summary>
    /// 处理复合指标数据.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="info"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    private async Task CompositeMetricChartDataAsync(MetricDataQryInput input, MetricInfoInfoOutput info, MetricChartDataOutput result)
    {
        result.Data = new ModelChartDataOutput();
        // 获取当前指标的父级指标.
        var parent = info.ParentIds;

        // 解析公式,${指标1}/${指标2},去除${}
        if (info.Expression == null) throw Oops.Oh(ErrorCode.K10020);

        info.Expression = info.Expression.Replace("${", Empty).Replace("}", "");

        var formula = new Expression(info.Expression);
        if (formula.HasErrors())
            throw Oops.Oh(ErrorCode.K10018);

        var dic = new Dictionary<string, string>();
        var listKey = new List<string>();
        // 获取各个指标的数据.
        foreach (var metricId in parent)
        {
            input.MetricId = metricId;
            var metricDataOutput = await GetChartDataAsync(input);
            var metricValue = metricDataOutput.Data.List;
            listKey.Add(metricId);
            foreach (var value in metricValue)
            {
                var tmp = info.Expression;
                // 如果字典存在当前维度,则修改值，否则进行添加.
                if (dic.ContainsKey(value.Dimension))
                {
                    dic[value.Dimension] = dic[value.Dimension].ReplaceMetricValue(metricId, value.Value);
                }
                else
                {
                    tmp = tmp.ReplaceMetricValue(metricId, value.Value);
                    dic.Add(value.Dimension, tmp);
                }

            }
            result.Data.ExecutedSql += metricDataOutput.Data.ExecutedSql;
            result.Data.TotalTime += metricDataOutput.Data.TotalTime;
            result.Data.Metas.AddRange(metricDataOutput.Data.Metas);
        }

        var rlt = new List<ChartData>();

        if (input.Limit > 0)
            dic = dic.Take(input.Limit.ParseToInt()).ToDictionary(x => x.Key, x => x.Value);

        foreach (var exp in dic)
        {
            var expValue = exp.Value;

            // 处理不存在的维度的数据.
            foreach (var key in listKey)
            {
                expValue = expValue.Replace(key, AggTypeConst.DefaultValue);
            }
            formula = new Expression(expValue);
            try
            {
                rlt.Add(new ChartData()
                {
                    Dimension = exp.Key,
                    Value = formula.Evaluate().ToString().Equals("∞") ? "0" : formula.Evaluate().ToString()
                });
            }
            catch
            {
                rlt.Add(new ChartData()
                {
                    Dimension = exp.Key,
                    Value = AggTypeConst.DefaultValue
                });
            }
        }

        result.Data.Data = rlt.Select(data => new List<object?>() { data.Dimension, data.Value }).ToList();
        result.MetricInfoGrade = await _metricGradedService.GetGradeExtInfoAsync(info.Id);
        //result.MetricInfoGrade = new List<MetricInfoGradeExtInput>()
        //{
        //    new MetricInfoGradeExtInput()
        //    {
        //        Id = "0",
        //        Name = "累计",
        //        Value = "7839.0",
        //        IsShow = true,
        //        MetricId = input.MetricId,
        //        StatusColor = ""
        //    },
        //    new MetricInfoGradeExtInput()
        //    {
        //        Id = "2",
        //        Name = "基准",
        //        Value = "6198",
        //        IsShow = true,
        //        MetricId = input.MetricId,
        //        StatusColor = ""
        //    },
        //    new MetricInfoGradeExtInput()
        //    {
        //        Id = "3",
        //        Name = "一级",
        //        Value = "6688",
        //        IsShow = true,
        //        MetricId = input.MetricId,
        //        StatusColor = ""
        //    },
        //    new MetricInfoGradeExtInput()
        //    {
        //        Id = "4",
        //        Name = "二级",
        //        Value = "7225",
        //        IsShow = true,
        //        MetricId = input.MetricId,
        //        StatusColor = ""
        //    },
        //    new MetricInfoGradeExtInput()
        //    {
        //        Id = "5",
        //        Name = "三级",
        //        Value = "7736",
        //        IsShow = true,
        //        MetricId = input.MetricId,
        //        StatusColor = ""
        //    },
        //};
        return;
    }


    /// <summary>
    /// 处理基础指标数据.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="info"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    private async Task BasicRealDataChartDataAsync(MetricDataQryInput input, MetricInfoInfoOutput info, MetricChartDataOutput result)
    {
        var query = new RealDataAggQueryInput();
        query.key = info.Column.field;
        query.Name = info.DataModelId.Id;
        var data = await _dbService.GetRealDataChartDataAsync(query);
        result.Data = data;
        result.MetricInfoGrade = new List<MetricInfoGradeExtInput>()
        {

        };
        return;
    }


    /// <inheritdoc />
    public async Task<MoreMetricChartDataOutput> GetMoreChartDataAsync(MoreMetricDataQryInput input)
    {
        var result = new MoreMetricChartDataOutput();
        var list = await _metricInfoService.GetListByIdAsync(input.MetricId);
        if (list == null || list.Count < 1) throw Oops.Oh(ErrorCode.K10019);
        foreach (var info in list)
        {
            //var query = new ModelDataAggQueryInput();
            //query.TaskId = SnowflakeIdHelper.NextId();
            //query.LinkId = info.DataModelId.ParentId;
            //query.SchemaName = info.DataModelId.Id;
            //query.ColumnField = info.Column;
            //query.AggType = info.AggType.ToEnum<DBAggType>();
            //query.Filters = input.Filters ?? info.Filters;
            //query.Limit = input.Limit;
            //query.SortBy = input.SortBy;
            //var metricBasicInfo = new MetricBasicInfo()
            //{
            //    Name = info.Name,
            //    Code = info.Code,
            //    Type = info.Type
            //};
            //// 如果维度信息不存在返回所有聚合数据.
            //if (info.TimeDimensions == null)
            //    return result;
            //query.Dimensions = input.Dimensions ?? info.Dimensions?[0];
            //if (info.TimeDimensions != null)
            //    query.Granularity = info.TimeDimensions.Granularity;
            //var data = await _dbService.GetMetricChartDataAsync(query);
            //result.Data = data;

        }
        return result;
    }


    /// <summary>
    /// 处理数据显示.
    /// </summary>
    /// <param name="format">数据格式.</param>
    /// <param name="data">数据.</param>
    private string DealData(DataModelFormat format, string data)
    {
        var rlt = data;
        if (format is { Type: DataModelFormatType.None }) return rlt;
        var tmp = data.ParseToDecimal();
        // 处理百分比.
        var isPercentage = false;
        if (format.Type == DataModelFormatType.Percentage)
        {
            tmp *= 100;
            isPercentage = true;
        }

        // 处理小数点.
        if (format.DecimalPlaces > 8) format.DecimalPlaces = 8;
        if (format.DecimalPlaces < 0) format.DecimalPlaces = 0;
        tmp = Math.Round(tmp, format.DecimalPlaces, MidpointRounding.AwayFromZero);

        // 处理单位、处理千分位.
        tmp.FormatNumberAsChineseUnit(8);

        if (isPercentage)
        {
            rlt = $"{tmp}%";
            return rlt;
        }

        switch (format.CurrencySymbol)
        {
            case DataModelCurrencySymbol.CNY:
                rlt = $"CNY{tmp}";
                break;
            case DataModelCurrencySymbol.CNY1:
                rlt = $"¥{tmp}";
                break;
            case DataModelCurrencySymbol.USD:
                rlt = $"USD{tmp}";
                break;
            case DataModelCurrencySymbol.USD1:
                rlt = $"${tmp}";
                break;
        }
        return rlt;
    }
}
