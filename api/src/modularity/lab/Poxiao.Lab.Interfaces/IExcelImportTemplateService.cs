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
    /// 根据产品规格ID获取模板列表.
    /// </summary>
    Task<List<ExcelImportTemplateDto>> GetByProductSpecId(string productSpecId);

    /// <summary>
    /// 获取默认模板.
    /// </summary>
    Task<ExcelImportTemplateDto> GetDefaultTemplate();

    /// <summary>
    /// 获取模板详情.
    /// </summary>
    Task<ExcelImportTemplateDto> GetInfo(string id);

    /// <summary>
    /// 创建模板.
    /// </summary>
    Task Create(ExcelImportTemplateInput input);

    /// <summary>
    /// 更新模板.
    /// </summary>
    Task Update(string id, ExcelImportTemplateInput input);

    /// <summary>
    /// 删除模板.
    /// </summary>
    Task Delete(string id);

    /// <summary>
    /// 设置默认模板.
    /// </summary>
    Task SetAsDefault(string id);

    /// <summary>
    /// 验证模板配置.
    /// </summary>
    Task ValidateTemplateConfig(string configJson);
}