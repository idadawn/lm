using Poxiao.Lab.Entity.Dto;

namespace Poxiao.Lab.Interfaces;

/// <summary>
/// Excel导入模板服务接口.
/// </summary>
public interface IExcelImportTemplateService
{
    /// <summary>
    /// 获取模板列表.
    /// </summary>
    Task<List<ExcelImportTemplateDto>> GetList();

    /// <summary>
    /// 获取默认模板.
    /// </summary>
    Task<ExcelImportTemplateDto> GetDefaultTemplate();

    /// <summary>
    /// 获取模板详情.
    /// </summary>
    Task<ExcelImportTemplateDto> GetInfo(string id);

    /// <summary>
    /// 更新模板.
    /// </summary>
    Task Update(string id, ExcelImportTemplateInput input);

    /// <summary>
    /// 验证模板配置.
    /// </summary>
    Task ValidateTemplateConfig(ExcelImportTemplateInput input);

    /// <summary>
    /// 解析Excel表头.
    /// </summary>
    Task<List<ExcelHeaderDto>> ParseHeaders(ExcelParseHeadersInput input);

    /// <summary>
    /// 获取系统字段定义（包含已保存的配置）.
    /// </summary>
    /// <param name="templateCode">模板编码</param>
    /// <returns></returns>
    Task<SystemFieldResult> GetSystemFields(string templateCode);

    /// <summary>
    /// 验证Excel文件与模板配置.
    /// </summary>
    /// <param name="input">验证输入参数</param>
    /// <returns>验证结果</returns>
    Task<ExcelTemplateValidationResult> ValidateExcelAgainstTemplate(ExcelTemplateValidationInput input);
}
