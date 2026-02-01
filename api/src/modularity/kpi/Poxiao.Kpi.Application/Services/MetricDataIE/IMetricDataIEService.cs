namespace Poxiao.Kpi.Application;

/// <summary>
/// 指标数据导入导出接口服务.
/// 版 本：V1.0.0.
/// 版 权：Poxiao.
/// 作 者：Poxiao.
/// 日 期：2024-01-05.
/// </summary>
public interface IMetricDataIEService
{
    /// <summary>
    /// 创建数据表.
    /// </summary>
    /// <returns>IsOK是否发生错误,Msg错误消息.</returns>
    Task<(bool IsOK, string Msg)> CreateDBTable(MetricDataIECreateTableInput input);

    /// <summary>
    /// 向数据表添加数据.
    /// </summary>
    /// <returns>IsOK是否发生错误,Msg错误消息,Data 插入的数据.</returns>
    Task<(bool IsOK, string Msg)> InsertDBTable(MetricDataIEInsertTableInput input);

    /// <summary>
    /// 获取创建数据表的模板.
    /// </summary>
    /// <returns></returns>
    MetricDataIECreateTemplateOutput GetCreateTemplate();

    /// <summary>
    /// 获取添加数据的模板.
    /// </summary>
    /// <param name="tableName">表名.</param>
    /// <returns></returns>
    Task<(MetricDataIEInsertTemplateOutput Output, bool IsOK, string Msg)> GetInsertTemplate(string tableName);

    /// <summary>
    /// 导出指标数据.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    Task<(MetricDataIEExportOutput Output, bool IsOK, string Msg)> ExportData(MetricDataIEExportInput input);
}