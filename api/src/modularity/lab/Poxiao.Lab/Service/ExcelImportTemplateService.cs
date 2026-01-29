using System;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Manager;
using Poxiao.Lab.Entity;
using Poxiao.Lab.Entity.Config;
using Poxiao.Lab.Entity.Dto;
using Poxiao.Lab.Entity.Enum;
using Poxiao.Lab.Interfaces;
using SqlSugar;

namespace Poxiao.Lab.Service;

/// <summary>
/// Excel导入模板服务.
/// </summary>
[ApiDescriptionSettings(Tag = "Lab", Name = "excel-templates", Order = 250)]
[Route("api/lab/excel-templates")]
public class ExcelImportTemplateService
    : IExcelImportTemplateService,
        IDynamicApiController,
        ITransient
{
    private readonly ISqlSugarRepository<ExcelImportTemplateEntity> _repository;
    private readonly IUserManager _userManager;
    private readonly ICacheManager _cacheManager;

    private const string TemplateCachePrefix = "LAB:ExcelTemplateConfig";

    public ExcelImportTemplateService(
        ISqlSugarRepository<ExcelImportTemplateEntity> repository,
        IUserManager userManager,
        ICacheManager cacheManager
    )
    {
        _repository = repository;
        _userManager = userManager;
        _cacheManager = cacheManager;
    }

    /// <inheritdoc />
    [HttpGet("")]
    public async Task<List<ExcelImportTemplateDto>> GetList()
    {
        var cacheKey = BuildTemplateCacheKey("list:all");

        // 尝试从缓存获取
        var cached = await _cacheManager.GetAsync<List<ExcelImportTemplateDto>>(cacheKey);
        if (cached != null && cached.Count > 0)
        {
            return cached;
        }

        // 确保默认模板存在
        await EnsureDefaultTemplates();

        var list = await _repository.GetListAsync(t => t.DeleteMark == 0 || t.DeleteMark == null);
        var dtos = new List<ExcelImportTemplateDto>();

        foreach (var entity in list)
        {
            var dto = entity.Adapt<ExcelImportTemplateDto>();
            dto.TemplateCode = entity.TemplateCode;
            dtos.Add(dto);
        }

        var result = dtos.OrderByDescending(t => t.Id).ToList();

        // 写入缓存（6小时过期）
        if (result.Count > 0)
        {
            await _cacheManager.SetAsync(cacheKey, result, TimeSpan.FromHours(6));
        }

        return result;
    }

    /// <inheritdoc />
    [HttpGet("default")]
    public async Task<ExcelImportTemplateDto> GetDefaultTemplate()
    {
        // 因为除去了 IsDefault 字段，所以默认逻辑可以硬编码为优先查找 Code 为 raw-data-import 的模板
        var defaultTemplate = await _repository.GetFirstAsync(t =>
            t.TemplateCode == ExcelImportTemplateCode.RawDataImport.ToString()
            && (t.DeleteMark == 0 || t.DeleteMark == null)
        );

        if (defaultTemplate == null)
        {
            // 如果没有默认模板，返回第一个有效模板（不区分类型）
            var anyTemplate = await _repository.GetFirstAsync(t =>
                t.DeleteMark == 0 || t.DeleteMark == null
            );

            if (anyTemplate == null)
                throw Oops.Bah("没有可用的导入模板");

            return anyTemplate.Adapt<ExcelImportTemplateDto>();
        }

        return defaultTemplate.Adapt<ExcelImportTemplateDto>();
    }

    /// <inheritdoc />
    [HttpGet("{id}")]
    public async Task<ExcelImportTemplateDto> GetInfo(string id)
    {
        var entity = await _repository.GetFirstAsync(t =>
            t.Id == id && (t.DeleteMark == 0 || t.DeleteMark == null)
        );
        if (entity == null)
            throw Oops.Bah("模板不存在");

        var dto = entity.Adapt<ExcelImportTemplateDto>();

        return dto;
    }

    /// <inheritdoc />
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] ExcelImportTemplateInput input)
    {
        var entity = await _repository.GetFirstAsync(t =>
            t.Id == id && (t.DeleteMark == 0 || t.DeleteMark == null)
        );
        if (entity == null)
            throw Oops.Bah("模板不存在");

        // 验证模板配置JSON
        var configJson = input.ConfigJson?.Trim();
        if (string.IsNullOrWhiteSpace(configJson))
            throw Oops.Bah("模板配置不能为空");

        await ValidateTemplateConfig(input);

        entity.TemplateName = input.TemplateName;
        entity.Description = input.Description;
        entity.OwnerUserId = input.OwnerUserId;
        entity.ConfigJson = configJson; // 使用处理后的值
        entity.LastModify();

        var isOk = await _repository.UpdateAsync(entity);
        if (!isOk)
            throw Oops.Bah("更新失败");

        var cacheKey = BuildTemplateCacheKey(entity.TemplateCode);
        var cacheConfig = TryParseTemplateConfig(configJson);
        if (cacheConfig != null)
        {
            await _cacheManager.SetAsync(cacheKey, cacheConfig, TimeSpan.FromHours(6));
        }
        else
        {
            await _cacheManager.DelAsync(cacheKey);
        }

        // 清除列表缓存和GetSystemFields缓存
        await _cacheManager.DelAsync(BuildTemplateCacheKey("list:all"));
        await _cacheManager.DelAsync(BuildTemplateCacheKey($"system-fields:{entity.TemplateCode}"));
    }

    private string BuildTemplateCacheKey(string templateCode)
    {
        var tenantId = _userManager?.TenantId ?? "global";
        return $"{TemplateCachePrefix}:{tenantId}:{templateCode}";
    }

    private ExcelTemplateConfig TryParseTemplateConfig(string configJson)
    {
        if (string.IsNullOrWhiteSpace(configJson))
            return null;

        try
        {
            var json = configJson.Trim();
            if (json.StartsWith("\"") && json.EndsWith("\""))
            {
                try
                {
                    json = System.Text.Json.JsonSerializer.Deserialize<string>(json);
                }
                catch
                {
                    // ignore
                }
            }

            return System.Text.Json.JsonSerializer.Deserialize<ExcelTemplateConfig>(json);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 确保默认模板存在.
    /// </summary>
    private async Task EnsureDefaultTemplates()
    {
        var count = await _repository.CountAsync(t => t.DeleteMark == 0 || t.DeleteMark == null);
        if (count > 0)
            return;

        var templates = new List<ExcelImportTemplateEntity>();

        // 获取当前用户ID，如果没有用户上下文则使用 "system" 表示系统创建
        var creatorUserId = !string.IsNullOrEmpty(_userManager?.UserId)
            ? _userManager.UserId
            : "system";

        // 1. 检测数据导入模板
        templates.Add(
            new ExcelImportTemplateEntity
            {
                Id = Guid.NewGuid().ToString(),
                TemplateName = "检测数据导入模板",
                TemplateCode = ExcelImportTemplateCode.RawDataImport.ToString(),
                CreatorTime = DateTime.Now,
                CreatorUserId = creatorUserId,
                ConfigJson = System.Text.Json.JsonSerializer.Serialize(
                    CreateDefaultConfig<RawDataEntity>(
                        "检测数据默认模板",
                        new DetectionColumnConfig
                        {
                            MinColumn = 1,
                            MaxColumn = 100,
                            Patterns = new List<string>
                            {
                                "{col}",
                                "检测{col}",
                                "列{col}",
                                "第{col}列",
                                "检测列{col}",
                            },
                        }
                    )
                ),
            }
        );

        // 2. 磁性数据导入模板
        templates.Add(
            new ExcelImportTemplateEntity
            {
                Id = Guid.NewGuid().ToString(),
                TemplateName = "磁性数据导入模板",
                TemplateCode = ExcelImportTemplateCode.MagneticDataImport.ToString(),
                CreatorTime = DateTime.Now,
                CreatorUserId = creatorUserId,
                ConfigJson = System.Text.Json.JsonSerializer.Serialize(
                    CreateDefaultConfig<MagneticRawDataEntity>(
                        "磁性数据默认模板",
                        new DetectionColumnConfig
                        {
                            MinColumn = 0,
                            MaxColumn = 0, // 磁性数据没有动态检测列
                        }
                    )
                ),
            }
        );

        await _repository.InsertRangeAsync(templates);
    }

    private ExcelTemplateConfig CreateDefaultConfig<T>(
        string description,
        DetectionColumnConfig detectionConfig
    )
    {
        var config = new ExcelTemplateConfig
        {
            Version = "1.0",
            Description = description,
            DetectionColumns = detectionConfig,
            FieldMappings = new List<TemplateColumnMapping>(),
        };

        var mappings = new List<(int Sort, TemplateColumnMapping Mapping)>();
        var properties = typeof(T).GetProperties();

        foreach (var prop in properties)
        {
            var importAttr =
                prop.GetCustomAttributes(
                        typeof(Poxiao.Lab.Entity.Attributes.ExcelImportColumnAttribute),
                        false
                    )
                    .FirstOrDefault() as Poxiao.Lab.Entity.Attributes.ExcelImportColumnAttribute;

            if (importAttr != null && importAttr.IsImportField)
            {
                string dataType = "string";
                var underlyingType =
                    Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                if (
                    underlyingType == typeof(decimal)
                    || underlyingType == typeof(double)
                    || underlyingType == typeof(float)
                )
                    dataType = "decimal";
                else if (underlyingType == typeof(int) || underlyingType == typeof(long))
                    dataType = "int";
                else if (underlyingType == typeof(DateTime))
                    dataType = "datetime";

                mappings.Add(
                    (
                        importAttr.Sort,
                        new TemplateColumnMapping
                        {
                            Field = prop.Name,
                            Label = importAttr.Name,
                            ExcelColumnNames = new List<string> { importAttr.Name },
                            DataType = dataType,
                            Required = false, // 默认非必填
                        }
                    )
                );
            }
        }

        config.FieldMappings = mappings.OrderBy(x => x.Sort).Select(x => x.Mapping).ToList();
        return config;
    }

    /// <inheritdoc />
    [HttpPost("validate-config")]
    public async Task ValidateTemplateConfig([FromBody] ExcelImportTemplateInput input)
    {
        var configJson = input?.ConfigJson;
        if (string.IsNullOrWhiteSpace(configJson))
            throw Oops.Bah("模板配置不能为空");

        try
        {
            // 尝试解析JSON
            var config = System.Text.Json.JsonSerializer.Deserialize<ExcelTemplateConfig>(
                configJson
            );
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

                if (
                    string.IsNullOrWhiteSpace(mapping.ExcelColumnIndex)
                    && (mapping.ExcelColumnNames == null || mapping.ExcelColumnNames.Count == 0)
                )
                    throw Oops.Bah($"字段 '{mapping.Field}' 必须指定至少一个Excel列名或列索引");
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

    // Removed ClearDefaultTemplate method as IsDefault field is gone

    /// <inheritdoc />
    [HttpPost("parse-headers")]
    public async Task<List<ExcelHeaderDto>> ParseHeaders([FromBody] ExcelParseHeadersInput input)
    {
        if (input == null || string.IsNullOrEmpty(input.FileData))
            throw Oops.Bah("请上传Excel文件");

        try
        {
            var fileBytes = Convert.FromBase64String(input.FileData);
            using var stream = new MemoryStream(fileBytes);
            var columns = MiniExcelLibs.MiniExcel.GetColumns(stream, useHeaderRow: true).ToList();

            var result = new List<ExcelHeaderDto>();
            for (int i = 0; i < columns.Count; i++)
            {
                result.Add(
                    new ExcelHeaderDto { Name = columns[i], Index = GetExcelColumnIndex(i) }
                );
            }

            return await Task.FromResult(result);
        }
        catch (FormatException)
        {
            throw Oops.Bah("文件数据格式错误");
        }
    }

    private string GetExcelColumnIndex(int columnIndex)
    {
        string columnName = "";
        while (columnIndex >= 0)
        {
            columnName = (char)('A' + (columnIndex % 26)) + columnName;
            columnIndex = (columnIndex / 26) - 1;
        }
        return columnName;
    }

    /// <inheritdoc />
    [HttpGet("system-fields")]
    public async Task<SystemFieldResult> GetSystemFields(string templateCode)
    {
        var cacheKey = BuildTemplateCacheKey($"system-fields:{templateCode}");

        // 尝试从缓存获取
        var cached = await _cacheManager.GetAsync<SystemFieldResult>(cacheKey);
        if (cached != null)
        {
            return cached;
        }

        var fields = new List<SystemFieldDto>();
        Type targetType = null;
        ExcelTemplateConfig savedConfig = null;

        // 根据模板编码确定实体类型
        if (templateCode == ExcelImportTemplateCode.RawDataImport.ToString()) // 兼容旧字符串
        {
            targetType = typeof(RawDataEntity);
        }
        else if (templateCode == ExcelImportTemplateCode.MagneticDataImport.ToString())
        {
            targetType = typeof(MagneticRawDataEntity);
        }

        if (targetType != null)
        {
            // 尝试获取该TemplateCode对应的已保存配置
            var existingTemplate = await _repository.GetFirstAsync(t =>
                t.TemplateCode == templateCode && (t.DeleteMark == 0 || t.DeleteMark == null)
            );

            if (existingTemplate != null && !string.IsNullOrWhiteSpace(existingTemplate.ConfigJson))
            {
                try
                {
                    // 尝试解析JSON (包含双重序列化处理)
                    var json = existingTemplate.ConfigJson;
                    // 初步尝试判断是否为双重序列化字符串
                    if (json.StartsWith("\"") && json.EndsWith("\""))
                    {
                        try
                        {
                            json = System.Text.Json.JsonSerializer.Deserialize<string>(json);
                        }
                        catch
                        { /* Ignore, treat as normal json */
                        }
                    }

                    savedConfig = System.Text.Json.JsonSerializer.Deserialize<ExcelTemplateConfig>(
                        json
                    );
                }
                catch
                {
                    // 解析失败则忽略，只返回系统默认字段
                }
            }
            // 构建字段映射字典 (Field -> Mapping)
            var configMap = new Dictionary<string, TemplateColumnMapping>();
            var headerMap = new Dictionary<string, string>();

            if (savedConfig != null)
            {
                if (savedConfig.FieldMappings != null)
                {
                    foreach (var mapping in savedConfig.FieldMappings)
                    {
                        if (
                            !string.IsNullOrEmpty(mapping.Field)
                            && !configMap.ContainsKey(mapping.Field)
                        )
                        {
                            configMap[mapping.Field] = mapping;
                        }
                    }
                }

                if (savedConfig.ExcelHeaders != null)
                {
                    foreach (var header in savedConfig.ExcelHeaders)
                    {
                        if (
                            !string.IsNullOrEmpty(header.Index)
                            && !headerMap.ContainsKey(header.Index)
                        )
                        {
                            headerMap[header.Index] = header.Name;
                        }
                    }
                }
            }

            var properties = targetType.GetProperties();
            foreach (var prop in properties)
            {
                var importAttr =
                    prop.GetCustomAttributes(
                            typeof(Poxiao.Lab.Entity.Attributes.ExcelImportColumnAttribute),
                            false
                        )
                        .FirstOrDefault()
                    as Poxiao.Lab.Entity.Attributes.ExcelImportColumnAttribute;

                if (importAttr != null && importAttr.IsImportField)
                {
                    string dataType = "string";
                    var underlyingType =
                        Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

                    if (
                        underlyingType == typeof(decimal)
                        || underlyingType == typeof(double)
                        || underlyingType == typeof(float)
                    )
                        dataType = "decimal";
                    else if (underlyingType == typeof(int) || underlyingType == typeof(long))
                        dataType = "int";
                    else if (underlyingType == typeof(DateTime))
                        dataType = "datetime";

                    // 默认值
                    var dto = new SystemFieldDto
                    {
                        Field = prop.Name,
                        Label = importAttr.Name,
                        DataType = dataType,
                        SortCode = importAttr.Sort,
                        ExcelColumnNames = new List<string> { importAttr.Name },
                        ExcelColumnIndex = null,
                        DefaultValue = null,
                    };

                    // 如果有保存的配置，覆盖默认值
                    if (configMap.ContainsKey(prop.Name))
                    {
                        var mapping = configMap[prop.Name];

                        // Apply UnitId and Required regardless of whether Index is set
                        if (mapping.UnitId != null)
                        {
                            dto.UnitId = mapping.UnitId;
                        }
                        dto.Required = mapping.Required;
                        dto.DecimalPlaces = mapping.DecimalPlaces;
                        dto.DefaultValue = mapping.DefaultValue;

                        if (!string.IsNullOrEmpty(mapping.ExcelColumnIndex))
                        {
                            dto.ExcelColumnIndex = mapping.ExcelColumnIndex;

                            // 优先从HeaderMap中查找对应的Excel列名，确保显示的是上次上传的文件里的列名
                            if (headerMap.ContainsKey(mapping.ExcelColumnIndex))
                            {
                                dto.ExcelColumnNames = new List<string>
                                {
                                    headerMap[mapping.ExcelColumnIndex],
                                };
                            }
                            else if (
                                mapping.ExcelColumnNames != null
                                && mapping.ExcelColumnNames.Count > 0
                            )
                            {
                                // 如果Header里没找到（可能不匹配），回退到保存的Names
                                dto.ExcelColumnNames = mapping.ExcelColumnNames;
                            }
                        }
                        else
                        {
                            // 没有Index，尝试使用保存的Names
                            if (
                                mapping.ExcelColumnNames != null
                                && mapping.ExcelColumnNames.Count > 0
                            )
                            {
                                dto.ExcelColumnNames = mapping.ExcelColumnNames;
                            }
                        }
                    }

                    fields.Add(dto);
                }
            }

            // 按SortCode排序
            fields = fields.OrderBy(f => f.SortCode).ToList();
        }

        var result = new SystemFieldResult
        {
            Fields = fields,
            ExcelHeaders = new List<ExcelHeaderDto>(),
        };

        // 填充ExcelHeaders
        if (savedConfig?.ExcelHeaders != null)
        {
            foreach (var h in savedConfig.ExcelHeaders)
            {
                result.ExcelHeaders.Add(new ExcelHeaderDto { Index = h.Index, Name = h.Name });
            }
        }

        // 写入缓存（6小时过期）
        await _cacheManager.SetAsync(cacheKey, result, TimeSpan.FromHours(6));

        return result;
    }

    /// <inheritdoc />
    [HttpPost("validate-excel")]
    public async Task<ExcelTemplateValidationResult> ValidateExcelAgainstTemplate(
        [FromBody] ExcelTemplateValidationInput input
    )
    {
        if (input == null || string.IsNullOrEmpty(input.FileData))
            throw Oops.Bah("请上传Excel文件");

        if (string.IsNullOrEmpty(input.TemplateCode))
            throw Oops.Bah("模板编码不能为空");

        var result = new ExcelTemplateValidationResult();
        var errors = new List<string>();

        try
        {
            // 1. 获取模板配置
            ExcelTemplateConfig templateConfig = null;
            var cacheKey = BuildTemplateCacheKey(input.TemplateCode);
            
            // 尝试从缓存获取
            templateConfig = await _cacheManager.GetAsync<ExcelTemplateConfig>(cacheKey);

            if (templateConfig == null)
            {
                var existingTemplate = await _repository.GetFirstAsync(t =>
                    t.TemplateCode == input.TemplateCode && (t.DeleteMark == 0 || t.DeleteMark == null)
                );

                if (existingTemplate != null && !string.IsNullOrWhiteSpace(existingTemplate.ConfigJson))
                {
                    templateConfig = TryParseTemplateConfig(existingTemplate.ConfigJson);
                    if (templateConfig == null)
                    {
                         errors.Add($"解析模板配置失败");
                    }
                    else
                    {
                        // 写入缓存（6小时过期）
                        await _cacheManager.SetAsync(cacheKey, templateConfig, TimeSpan.FromHours(6));
                    }
                }
            }

            if (templateConfig == null)
            {
                if (errors.Count == 0) errors.Add("未找到有效的模板配置");
                result.IsValid = false;
                result.Errors = errors;
                return result;
            }

            result.TemplateConfig = templateConfig;

            // 2. 解析Excel表头
            List<ExcelHeaderDto> excelHeaders;
            try
            {
                var fileBytes = Convert.FromBase64String(input.FileData);
                using var stream = new MemoryStream(fileBytes);
                var columns = MiniExcelLibs
                    .MiniExcel.GetColumns(stream, useHeaderRow: true)
                    .ToList();

                excelHeaders = new List<ExcelHeaderDto>();
                for (int i = 0; i < columns.Count; i++)
                {
                    excelHeaders.Add(
                        new ExcelHeaderDto { Name = columns[i], Index = GetExcelColumnIndex(i) }
                    );
                }

                result.ExcelHeaders = excelHeaders;
            }
            catch (Exception ex)
            {
                errors.Add($"解析Excel表头失败: {ex.Message}");
                result.IsValid = false;
                result.Errors = errors;
                return result;
            }

            if (!excelHeaders.Any())
            {
                errors.Add("Excel文件没有表头行，请检查文件格式");
                result.IsValid = false;
                result.Errors = errors;
                return result;
            }

            // 3. 对比Excel表头和模板配置
            var excelHeaderMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var header in excelHeaders)
            {
                var name = header.Name?.Trim();
                if (!string.IsNullOrEmpty(name))
                {
                    excelHeaderMap[name.ToLower()] = name;
                }
            }

            // 检查必填字段
            if (templateConfig.FieldMappings != null)
            {
                foreach (var field in templateConfig.FieldMappings)
                {
                    if (
                        field.Required
                        && field.ExcelColumnNames != null
                        && field.ExcelColumnNames.Count > 0
                    )
                    {
                        // 检查是否至少有一个列名在Excel中存在
                        var found = field.ExcelColumnNames.Any(colName =>
                        {
                            var normalizedColName = colName?.Trim()?.ToLower();
                            return !string.IsNullOrEmpty(normalizedColName)
                                && excelHeaderMap.ContainsKey(normalizedColName);
                        });

                        if (!found)
                        {
                            var label = string.Join(" 或 ", field.ExcelColumnNames);
                            var fieldLabel = field.Label ?? field.Field;
                            errors.Add($"缺少必填列: {label} ({fieldLabel})");
                        }
                    }
                }
            }

            // 检查检测列
            if (templateConfig.DetectionColumns != null)
            {
                var detectionConfig = templateConfig.DetectionColumns;
                var detectionHeaders = excelHeaders
                    .Where(h =>
                    {
                        var name = h.Name?.Trim() ?? "";
                        if (string.IsNullOrEmpty(name))
                            return false;

                        // 检查是否符合检测列模式
                        if (detectionConfig.Patterns != null && detectionConfig.Patterns.Count > 0)
                        {
                            return detectionConfig.Patterns.Any(pattern =>
                            {
                                try
                                {
                                    // 将模式中的 {col} 替换为数字匹配
                                    var regexPattern = pattern.Replace("{col}", @"\d+");
                                    // 构建正则表达式，使用忽略大小写选项
                                    var regex = new System.Text.RegularExpressions.Regex(
                                        $"^{regexPattern}$",
                                        System.Text.RegularExpressions.RegexOptions.IgnoreCase
                                    );
                                    return regex.IsMatch(name);
                                }
                                catch
                                {
                                    // 如果正则表达式构建失败，尝试简单的字符串匹配
                                    var simplePattern = pattern.Replace("{col}", "");
                                    return name.Contains(
                                        simplePattern,
                                        StringComparison.OrdinalIgnoreCase
                                    );
                                }
                            });
                        }

                        // 如果没有模式，检查是否包含"检测"关键字
                        return name.Contains("检测", StringComparison.OrdinalIgnoreCase);
                    })
                    .ToList();

                // 提取检测列号
                var detectionColumnNumbers = new List<int>();
                foreach (var header in detectionHeaders)
                {
                    var name = header.Name?.Trim() ?? "";
                    var match = System.Text.RegularExpressions.Regex.Match(name, @"\d+");
                    if (match.Success && int.TryParse(match.Value, out var colNum) && colNum > 0)
                    {
                        detectionColumnNumbers.Add(colNum);
                    }
                }

                // 去重并排序
                var uniqueColumnNumbers = detectionColumnNumbers
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                if (uniqueColumnNumbers.Count == 0)
                {
                    errors.Add("未找到检测列，请确保Excel中包含检测数据列（如：检测1、检测2等）");
                }
                else
                {
                    // 检查检测列数量范围
                    if (
                        detectionConfig.MinColumn > 0
                        && uniqueColumnNumbers.Count < detectionConfig.MinColumn
                    )
                    {
                        errors.Add(
                            $"检测列数量不足：至少需要 {detectionConfig.MinColumn} 列检测数据，当前找到 {uniqueColumnNumbers.Count} 列（{string.Join(", ", uniqueColumnNumbers)}）"
                        );
                    }

                    if (
                        detectionConfig.MaxColumn > 0
                        && uniqueColumnNumbers.Count > detectionConfig.MaxColumn
                    )
                    {
                        errors.Add(
                            $"检测列数量超出：最多允许 {detectionConfig.MaxColumn} 列检测数据，当前找到 {uniqueColumnNumbers.Count} 列（{string.Join(", ", uniqueColumnNumbers)}）"
                        );
                    }
                }
            }

            result.IsValid = !errors.Any();
            result.Errors = errors;
        }
        catch (Exception ex)
        {
            errors.Add($"模板验证过程出错: {ex.Message}");
            result.IsValid = false;
            result.Errors = errors;
        }

        return result;
    }
}
