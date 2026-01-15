using Mapster;
using Microsoft.AspNetCore.Mvc;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.Lab.Entity.Config;
using Poxiao.Lab.Entity.Dto;
using Poxiao.Lab.Entity.Entity;
using Poxiao.Lab.Interfaces;
using SqlSugar;

namespace Poxiao.Lab.Service;

/// <summary>
/// Excel导入模板服务.
/// </summary>
[ApiDescriptionSettings(Tag = "Lab", Name = "excel-templates", Order = 250)]
[Route("api/lab/excel-templates")]
public class ExcelImportTemplateService : IExcelImportTemplateService, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<ExcelImportTemplateEntity> _repository;
    private readonly ISqlSugarRepository<ProductSpecEntity> _productSpecRepository;

    public ExcelImportTemplateService(
        ISqlSugarRepository<ExcelImportTemplateEntity> repository,
        ISqlSugarRepository<ProductSpecEntity> productSpecRepository)
    {
        _repository = repository;
        _productSpecRepository = productSpecRepository;
    }

    /// <inheritdoc />
    [HttpGet("")]
    public async Task<List<ExcelImportTemplateDto>> GetList()
    {
        var list = await _repository.GetListAsync(t => t.DeleteMark == 0 || t.DeleteMark == null);
        var dtos = new List<ExcelImportTemplateDto>();

        foreach (var entity in list)
        {
            var dto = entity.Adapt<ExcelImportTemplateDto>();

            // 填充产品规格名称
            if (!string.IsNullOrEmpty(entity.ProductSpecId))
            {
                var productSpec = await _productSpecRepository.GetFirstAsync(p => p.Id == entity.ProductSpecId && p.DeleteMark == null);
                if (productSpec != null)
                {
                    dto.ProductSpecName = productSpec.Name;
                }
            }

            dtos.Add(dto);
        }

        return dtos.OrderBy(t => t.SortCode ?? 0).ToList();
    }

    /// <inheritdoc />
    [HttpGet("by-product-spec/{productSpecId}")]
    public async Task<List<ExcelImportTemplateDto>> GetByProductSpecId(string productSpecId)
    {
        var list = await _repository.GetListAsync(t =>
            t.ProductSpecId == productSpecId && (t.DeleteMark == 0 || t.DeleteMark == null));

        return list
            .OrderBy(t => t.SortCode ?? 0)
            .Select(t => t.Adapt<ExcelImportTemplateDto>())
            .ToList();
    }

    /// <inheritdoc />
    [HttpGet("default")]
    public async Task<ExcelImportTemplateDto> GetDefaultTemplate()
    {
        var defaultTemplate = await _repository.GetFirstAsync(t =>
            t.IsDefault == 1 && (t.DeleteMark == 0 || t.DeleteMark == null));

        if (defaultTemplate == null)
        {
            // 如果没有默认模板，返回第一个系统模板
            var systemTemplate = await _repository.GetFirstAsync(t =>
                t.TemplateType == "system" && (t.DeleteMark == 0 || t.DeleteMark == null));

            if (systemTemplate == null)
                throw Oops.Bah("没有可用的导入模板");

            return systemTemplate.Adapt<ExcelImportTemplateDto>();
        }

        return defaultTemplate.Adapt<ExcelImportTemplateDto>();
    }

    /// <inheritdoc />
    [HttpGet("{id}")]
    public async Task<ExcelImportTemplateDto> GetInfo(string id)
    {
        var entity = await _repository.GetFirstAsync(t => t.Id == id && (t.DeleteMark == 0 || t.DeleteMark == null));
        if (entity == null)
            throw Oops.Bah("模板不存在");

        var dto = entity.Adapt<ExcelImportTemplateDto>();

        // 填充产品规格名称
        if (!string.IsNullOrEmpty(entity.ProductSpecId))
        {
            var productSpec = await _productSpecRepository.GetFirstAsync(p => p.Id == entity.ProductSpecId && p.DeleteMark == null);
            if (productSpec != null)
            {
                dto.ProductSpecName = productSpec.Name;
            }
        }

        return dto;
    }

    /// <inheritdoc />
    [HttpPost]
    public async Task Create([FromBody] ExcelImportTemplateInput input)
    {
        // 验证模板编码唯一性
        var code = input.TemplateCode?.Trim();
        if (string.IsNullOrWhiteSpace(code))
            throw Oops.Bah("模板编码不能为空");

        var exists = await _repository
            .AsQueryable()
            .Where(t => t.TemplateCode == code && (t.DeleteMark == 0 || t.DeleteMark == null))
            .AnyAsync();

        if (exists)
            throw Oops.Bah($"模板编码 '{code}' 已存在");

        // 验证模板配置JSON
        await ValidateTemplateConfig(input.ConfigJson);

        var entity = input.Adapt<ExcelImportTemplateEntity>();
        entity.TemplateCode = code;
        entity.Creator();

        // 如果是默认模板，清除其他默认模板
        if (input.IsDefault == 1)
        {
            await ClearDefaultTemplate();
        }

        var isOk = await _repository.InsertAsync(entity);
        if (!isOk)
            throw Oops.Bah("创建失败");
    }

    /// <inheritdoc />
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] ExcelImportTemplateInput input)
    {
        var entity = await _repository.GetFirstAsync(t => t.Id == id && (t.DeleteMark == 0 || t.DeleteMark == null));
        if (entity == null)
            throw Oops.Bah("模板不存在");

        // 如果模板编码改变，验证唯一性
        var code = input.TemplateCode?.Trim();
        if (string.IsNullOrWhiteSpace(code))
            throw Oops.Bah("模板编码不能为空");

        if (entity.TemplateCode != code)
        {
            var exists = await _repository
                .AsQueryable()
                .Where(t => t.TemplateCode == code && t.Id != id && (t.DeleteMark == 0 || t.DeleteMark == null))
                .AnyAsync();

            if (exists)
                throw Oops.Bah($"模板编码 '{code}' 已被其他模板使用");
        }

        // 验证模板配置JSON
        await ValidateTemplateConfig(input.ConfigJson);

        entity.TemplateName = input.TemplateName;
        entity.TemplateCode = code;
        entity.Description = input.Description;
        entity.TemplateType = input.TemplateType;
        entity.OwnerUserId = input.OwnerUserId;
        entity.ConfigJson = input.ConfigJson;
        entity.ProductSpecId = input.ProductSpecId;
        entity.SortCode = input.SortCode;
        entity.LastModify();

        // 如果是默认模板，清除其他默认模板
        if (input.IsDefault == 1 && entity.IsDefault == 0)
        {
            await ClearDefaultTemplate();
        }
        entity.IsDefault = input.IsDefault;

        var isOk = await _repository.UpdateAsync(entity);
        if (!isOk)
            throw Oops.Bah("更新失败");
    }

    /// <inheritdoc />
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var entity = await _repository.GetFirstAsync(t => t.Id == id && (t.DeleteMark == 0 || t.DeleteMark == null));
        if (entity == null)
            throw Oops.Bah("模板不存在");

        entity.Delete();
        var isOk = await _repository.UpdateAsync(entity);
        if (!isOk)
            throw Oops.Bah("删除失败");
    }

    /// <inheritdoc />
    [HttpPut("{id}/set-default")]
    public async Task SetAsDefault(string id)
    {
        var entity = await _repository.GetFirstAsync(t => t.Id == id && (t.DeleteMark == 0 || t.DeleteMark == null));
        if (entity == null)
            throw Oops.Bah("模板不存在");

        // 清除其他默认模板
        await ClearDefaultTemplate();

        entity.IsDefault = 1;
        entity.LastModify();

        var isOk = await _repository.UpdateAsync(entity);
        if (!isOk)
            throw Oops.Bah("设置默认模板失败");
    }

    /// <inheritdoc />
    [HttpPost("validate-config")]
    public async Task ValidateTemplateConfig(string configJson)
    {
        if (string.IsNullOrWhiteSpace(configJson))
            throw Oops.Bah("模板配置不能为空");

        try
        {
            // 尝试解析JSON
            var config = System.Text.Json.JsonSerializer.Deserialize<ExcelTemplateConfig>(configJson);
            if (config == null)
                throw Oops.Bah("模板配置格式错误");

            // 验证必要字段
            if (config.FieldMappings == null || config.FieldMappings.Count == 0)
                throw Oops.Bah("模板配置必须包含字段映射");

            // 验证每个字段映射
            foreach (var mapping in config.FieldMappings)
            {
                if (string.IsNullOrWhiteSpace(mapping.Field))
                    throw Oops.Bah("字段映射中的字段名不能为空");

                if (mapping.ExcelColumnNames == null || mapping.ExcelColumnNames.Count == 0)
                    throw Oops.Bah($"字段 '{mapping.Field}' 必须指定至少一个Excel列名");
            }

            // 验证检测列配置
            if (config.DetectionColumns == null)
                throw Oops.Bah("检测列配置不能为空");

            if (config.DetectionColumns.MinColumn < 1)
                throw Oops.Bah("最小列号必须大于0");

            if (config.DetectionColumns.MaxColumn < config.DetectionColumns.MinColumn)
                throw Oops.Bah("最大列号必须大于等于最小列号");
        }
        catch (System.Text.Json.JsonException ex)
        {
            throw Oops.Bah($"模板配置JSON格式错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 清除其他默认模板.
    /// </summary>
    private async Task ClearDefaultTemplate()
    {
        await _repository
            .AsUpdateable()
            .SetColumns(t => new ExcelImportTemplateEntity { IsDefault = 0 })
            .Where(t => t.IsDefault == 1 && (t.DeleteMark == 0 || t.DeleteMark == null))
            .ExecuteCommandAsync();
    }
}