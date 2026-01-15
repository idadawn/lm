using Microsoft.AspNetCore.Authorization;

namespace Poxiao.Kpi.Web.Core;

/// <summary>
/// 指标数据导入导出接口.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2023-12-21.
/// </summary>
[ApiDescriptionSettings(Groups = new[] { "KPI" }, Tag = "MetricDataIE", Name = "MetricDataIE", Order = 200)]
[Route("api/kpi/v1/[controller]")]
public class ExcelContriller : IDynamicApiController
{
    private readonly IMetricDataIEService _excelService;
    public ExcelContriller(IMetricDataIEService excelService)
    {
        _excelService = excelService;
    }

    /// <summary>
    /// 获取创建数据表的模板.
    /// </summary>
    [HttpGet("getCreate")]
    public MetricDataIECreateTemplateOutput GetCreateTemplate()
    {
        return _excelService.GetCreateTemplate();
    }

    /// <summary>
    /// 创建数据表.
    /// </summary>
    [HttpPost("createTable")]
    public async Task CreateDBTable(MetricDataIECreateTableInput data)
    {
        (bool isOK, string msg) = await _excelService.CreateDBTable(data);
        if (!isOK) throw Oops.Bah(msg);
    }

    /// <summary>
    /// 获取添加数据的模板.
    /// </summary>
    [HttpGet("getInsert")]
    public async Task<MetricDataIEInsertTemplateOutput> GetInsertTemplate([FromQuery] string tableName)
    {
        (MetricDataIEInsertTemplateOutput output, bool isOK, string msg) = await _excelService.GetInsertTemplate(tableName);
        if (!isOK) throw Oops.Bah(msg);
        return output;
    }

    /// <summary>
    /// 添加数据.
    /// </summary>
    [HttpPost("insertData")]
    public async Task InsertDBTable(MetricDataIEInsertTableInput input)
    {
        (bool isOK, string msg) = await _excelService.InsertDBTable(input);
        if (!isOK) throw Oops.Bah(msg);
    }

    /// <summary>
    /// 导出数据.
    /// </summary>
    [HttpPost("exportData")]
    [AllowAnonymous]
    public async Task<MetricDataIEExportOutput> ExportData(MetricDataIEExportInput input)
    {
        (MetricDataIEExportOutput output, bool isOK, string msg) = await _excelService.ExportData(input);
        if (!isOK) throw Oops.Bah(msg);
        return output;
    }
}