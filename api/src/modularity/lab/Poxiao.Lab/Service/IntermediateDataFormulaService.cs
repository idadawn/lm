using System.Reflection;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Poxiao.DatabaseAccessor;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.Infrastructure.Enums;
using Poxiao.Lab.Entity;
using Poxiao.Lab.Entity.Attributes;
using Poxiao.Lab.Entity.Dto.IntermediateDataFormula;
using Poxiao.Lab.Entity.Enums;
using Poxiao.Lab.Helpers;
using Poxiao.Lab.Interfaces;
using SqlSugar;

namespace Poxiao.Lab.Service;

/// <summary>
/// 中间数据表公式维护服务实现.
/// </summary>
[ApiDescriptionSettings(Tag = "Lab", Name = "intermediate-data-formula", Order = 210)]
[Route("api/lab/intermediate-data-formula")]
public class IntermediateDataFormulaService
    : IIntermediateDataFormulaService,
        IDynamicApiController,
        ITransient
{
    private readonly ISqlSugarRepository<IntermediateDataFormulaEntity> _repository;
    private readonly ISqlSugarRepository<PublicDimensionEntity> _publicDimensionRepository;
    private readonly ISqlSugarRepository<AppearanceFeatureEntity> _appearanceFeatureRepository;
    private readonly ISqlSugarRepository<AppearanceFeatureLevelEntity> _severityLevelRepository;
    private readonly ISqlSugarRepository<ProductSpecAttributeEntity> _productSpecAttributeRepository;
    private readonly ISqlSugarRepository<UnitDefinitionEntity> _unitRepository;
    private readonly IFormulaParser _formulaParser;

    public IntermediateDataFormulaService(
        ISqlSugarRepository<IntermediateDataFormulaEntity> repository,
        ISqlSugarRepository<PublicDimensionEntity> publicDimensionRepository,
        ISqlSugarRepository<AppearanceFeatureEntity> appearanceFeatureRepository,
        ISqlSugarRepository<AppearanceFeatureLevelEntity> severityLevelRepository,
        ISqlSugarRepository<ProductSpecAttributeEntity> productSpecAttributeRepository,
        ISqlSugarRepository<UnitDefinitionEntity> unitRepository,
        IFormulaParser formulaParser
    )
    {
        _repository = repository;
        _publicDimensionRepository = publicDimensionRepository;
        _appearanceFeatureRepository = appearanceFeatureRepository;
        _severityLevelRepository = severityLevelRepository;
        _productSpecAttributeRepository = productSpecAttributeRepository;
        _unitRepository = unitRepository;
        _formulaParser = formulaParser;
    }

    /// <summary>
    /// 将实体转换为 DTO，处理枚举到字符串的转换.
    /// </summary>
    private IntermediateDataFormulaDto ToDto(IntermediateDataFormulaEntity entity)
    {
        var dto = entity.Adapt<IntermediateDataFormulaDto>();
        dto.FormulaType = entity.FormulaType.ToString();
        
        // 根据 columnName 查找对应的 displayName
        dto.DisplayName = GetColumnDisplayName(entity.ColumnName);
        
        return dto;
    }

    /// <summary>
    /// 根据列名获取显示名称.
    /// </summary>
    private string GetColumnDisplayName(string columnName)
    {
        if (string.IsNullOrEmpty(columnName))
            return null;

        var entityType = typeof(IntermediateDataEntity);
        var prop = entityType.GetProperty(columnName, BindingFlags.Public | BindingFlags.Instance);
        if (prop == null)
            return null;

        var columnAttr = prop.GetCustomAttribute<IntermediateDataColumnAttribute>();
        if (columnAttr != null && !string.IsNullOrEmpty(columnAttr.DisplayName))
        {
            return columnAttr.DisplayName;
        }

        return GetDisplayName(prop);
    }

    /// <inheritdoc />
    [HttpGet("")]
    public async Task<List<IntermediateDataFormulaDto>> GetListAsync()
    {
        var list = await _repository
            .AsQueryable()
            .Where(t => t.DeleteMark == null)
            .OrderBy(t => t.SortOrder)
            .OrderBy(t => t.CreatorTime)
            .ToListAsync();

        return list.Select(ToDto).ToList();
    }

    /// <inheritdoc />
    [HttpGet("{id}")]
    public async Task<IntermediateDataFormulaDto> GetByIdAsync(string id)
    {
        var entity = await _repository.GetFirstAsync(t => t.Id == id && t.DeleteMark == null);
        if (entity == null)
            throw Oops.Oh(ErrorCode.COM1005);

        return ToDto(entity);
    }

    /// <inheritdoc />
    [HttpPost("")]
    public async Task<IntermediateDataFormulaDto> CreateAsync(
        [FromBody] IntermediateDataFormulaDto dto
    )
    {
        // 检查列名是否已存在
        var exists = await _repository
            .AsQueryable()
            .Where(t =>
                t.TableName == dto.TableName
                && t.ColumnName == dto.ColumnName
                && t.DeleteMark == null
            )
            .AnyAsync();

        if (exists)
            throw Oops.Oh(ErrorCode.COM1003, $"列 {dto.ColumnName} 的公式已存在");

        // 如果公式名称为空，使用列名
        if (string.IsNullOrWhiteSpace(dto.FormulaName))
        {
            dto.FormulaName = dto.ColumnName;
        }

        // 如果指定了单位ID，获取单位名称
        if (!string.IsNullOrEmpty(dto.UnitId))
        {
            var unit = await _unitRepository.GetFirstAsync(u =>
                u.Id == dto.UnitId && u.DeleteMark == null
            );
            if (unit != null)
            {
                dto.UnitName = unit.Symbol;
            }
        }

        var entity = dto.Adapt<IntermediateDataFormulaEntity>();
        // 处理 FormulaType 字符串到枚举的转换
        if (!string.IsNullOrEmpty(dto.FormulaType))
        {
            if (Enum.TryParse<IntermediateDataFormulaType>(dto.FormulaType, out var formulaType))
            {
                entity.FormulaType = formulaType;
            }
            else
            {
                entity.FormulaType = IntermediateDataFormulaType.CALC; // 默认值
            }
        }
        else
        {
            entity.FormulaType = IntermediateDataFormulaType.CALC; // 默认值
        }
        
        entity.Creator();
        entity.LastModifyUserId = entity.CreatorUserId;
        entity.LastModifyTime = entity.CreatorTime;

        var isOk = await _repository.InsertAsync(entity);
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1000);

        return ToDto(entity);
    }

    /// <inheritdoc />
    [HttpPut("{id}")]
    public async Task<IntermediateDataFormulaDto> UpdateAsync(
        string id,
        [FromBody] IntermediateDataFormulaDto dto
    )
    {
        var entity = await _repository.GetFirstAsync(t => t.Id == id && t.DeleteMark == null);
        if (entity == null)
            throw Oops.Oh(ErrorCode.COM1005);

        // 检查列名是否与其他记录冲突
        if (entity.ColumnName != dto.ColumnName || entity.TableName != dto.TableName)
        {
            var exists = await _repository
                .AsQueryable()
                .Where(t =>
                    t.Id != id
                    && t.TableName == dto.TableName
                    && t.ColumnName == dto.ColumnName
                    && t.DeleteMark == null
                )
                .AnyAsync();

            if (exists)
                throw Oops.Oh(ErrorCode.COM1003, $"列 {dto.ColumnName} 的公式已存在");
        }

        // 如果公式名称为空，使用列名
        if (string.IsNullOrWhiteSpace(dto.FormulaName))
        {
            dto.FormulaName = dto.ColumnName;
        }

        // 如果指定了单位ID，获取单位名称
        if (!string.IsNullOrEmpty(dto.UnitId))
        {
            var unit = await _unitRepository.GetFirstAsync(u =>
                u.Id == dto.UnitId && u.DeleteMark == null
            );
            if (unit != null)
            {
                dto.UnitName = unit.Symbol;
            }
        }

        // 保存关键字段，Adapt会覆盖这些
        var originalId = entity.Id;
        var originalCreatorUserId = entity.CreatorUserId;
        var originalCreatorTime = entity.CreatorTime;
        var originalTenantId = entity.TenantId;

        // 只更新允许修改的字段，不使用全量Adapt
        entity.FormulaName = dto.FormulaName ?? entity.ColumnName;
        entity.Formula = dto.Formula ?? entity.Formula;
        entity.FormulaLanguage = dto.FormulaLanguage ?? entity.FormulaLanguage;
        // 将字符串转换为枚举
        if (!string.IsNullOrEmpty(dto.FormulaType))
        {
            if (Enum.TryParse<IntermediateDataFormulaType>(dto.FormulaType, out var formulaType))
            {
                entity.FormulaType = formulaType;
            }
        }
        entity.UnitId = dto.UnitId;
        entity.UnitName = dto.UnitName;
        entity.Precision = dto.Precision;
        entity.IsEnabled = dto.IsEnabled;
        entity.SortOrder = dto.SortOrder;
        entity.DefaultValue = dto.DefaultValue;
        entity.Remark = dto.Remark;

        entity.LastModify();

        var isOk = await _repository.UpdateAsync(entity);
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1001);

        return ToDto(entity);
    }

    /// <inheritdoc />
    [HttpPut("{id}/formula")]
    public async Task<IntermediateDataFormulaDto> UpdateFormulaAsync(
        string id,
        [FromBody] FormulaUpdateInput input
    )
    {
        var entity = await _repository.GetFirstAsync(u => u.Id == id);
        if (entity == null)
            throw Oops.Oh(ErrorCode.COM1005);

        entity.Formula = input.Formula;
        entity.LastModify();

        var isOk = await _repository.UpdateAsync(entity);
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1001);

        return ToDto(entity);
    }

    /// <inheritdoc />
    [HttpDelete("{id}")]
    public async Task DeleteAsync(string id)
    {
        var isOk = await _repository.DeleteAsync(u => u.Id == id);
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1002);
    }

    /// <inheritdoc />
    [HttpPost("initialize")]
    public async Task InitializeAsync()
    {
        var availableColumns = await GetAvailableColumnsAsync();

        // 获取现有公式（无需先删除）
        var existingFormulas = await _repository
            .AsQueryable()
            .Where(t => t.DeleteMark == null && t.TableName == "INTERMEDIATE_DATA")
            .ToListAsync();

        var newEntities = new List<IntermediateDataFormulaEntity>();

        foreach (var col in availableColumns)
        {
            // 如果已存在，则跳过（增量添加）
            if (existingFormulas.Any(f => f.ColumnName == col.ColumnName))
            {
                continue;
            }

            var entity = new IntermediateDataFormulaEntity
            {
                // Id = col.ColumnName, // 保持与列名一致的ID策略 (如果之前是destructive set Id=ColumnName，这里继续保持一致较好)
                TableName = "INTERMEDIATE_DATA",
                ColumnName = col.ColumnName,
                FormulaName = col.DisplayName ?? col.ColumnName,
                Formula = "",
                FormulaLanguage = "EXCEL", // 默认EXCEL
                FormulaType = col.IsCalculable ? IntermediateDataFormulaType.CALC : IntermediateDataFormulaType.JUDGE,
                UnitName = null, // 单位ID暂为空，后续编辑
                Precision = col.DecimalDigits,
                IsEnabled = true,
                SortOrder = col.Sort,
                Remark = col.Description,
            };
            entity.Creator();
            entity.Id = col.ColumnName; // 覆盖默认生成的ID，确保ID一致性
            entity.LastModifyUserId = entity.CreatorUserId;
            entity.LastModifyTime = entity.CreatorTime;
            newEntities.Add(entity);
        }

        if (newEntities.Any())
        {
            await _repository.InsertRangeAsync(newEntities);
        }
    }

    /// <inheritdoc />
    [HttpGet("available-columns")]
    public async Task<List<IntermediateDataColumnInfo>> GetAvailableColumnsAsync(
        [FromQuery] bool includeHidden = false
    )
    {
        var columns = new List<IntermediateDataColumnInfo>();
        var entityType = typeof(IntermediateDataEntity);
        var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            // 只处理带有 IntermediateDataColumnAttribute 特性的属性
            var columnAttr = prop.GetCustomAttribute<IntermediateDataColumnAttribute>();
            if (columnAttr == null)
                continue;

            // 如果特性中指定了 IsIgnore，则跳过
            if (columnAttr.IsIgnore)
                continue;

            // 如果特性中指定不在公式维护中显示，且未指定包含隐藏列，则跳过
            if (!includeHidden && !columnAttr.ShowInFormulaMaintenance)
                continue;

            var sugarColumn = prop.GetCustomAttribute<SugarColumn>();

            // 如果 SugarColumn 中指定了 IsIgnore，则跳过
            if (sugarColumn != null && sugarColumn.IsIgnore)
                continue;

            // 使用特性中的信息
            var displayName = columnAttr.DisplayName ?? GetDisplayName(prop);
            var description = columnAttr.Description;
            var sort = columnAttr.Sort;
            var isCalculable = columnAttr.IsCalculable;
            var dataType = columnAttr.DataType ?? GetDataTypeName(prop.PropertyType);

            // 从 IntermediateDataColumnAttribute 或 SugarColumn 的 DecimalDigits 获取小数位数（精度）
            int? decimalDigits = null;
            if (columnAttr.DecimalDigits.HasValue)
            {
                decimalDigits = columnAttr.DecimalDigits.Value;
            }
            else if (sugarColumn != null && sugarColumn.DecimalDigits > 0)
            {
                decimalDigits = sugarColumn.DecimalDigits;
            }

            var columnInfo = new IntermediateDataColumnInfo
            {
                ColumnName = prop.Name,
                DisplayName = displayName,
                DataType = dataType,
                IsCalculable = isCalculable,
                DecimalDigits = decimalDigits,
                Description = description,
                Sort = sort,
            };

            columns.Add(columnInfo);
        }

        // 按 Sort 排序，如果没有则按显示名称排序
        return columns.OrderBy(c => c.Sort).ThenBy(c => c.DisplayName).ToList();
    }

    /// <inheritdoc />
    [HttpGet("variable-sources")]
    public async Task<List<FormulaVariableSource>> GetVariableSourcesAsync()
    {
        var sources = new List<FormulaVariableSource>();

        // 1. 中间数据表列
        var intermediateColumns = await GetAvailableColumnsAsync();
        sources.AddRange(
            intermediateColumns.Select(c => new FormulaVariableSource
            {
                SourceType = "INTERMEDIATE_DATA",
                TableName = "LAB_INTERMEDIATE_DATA",
                VariableKey = c.ColumnName,
                DisplayName = $"[中间数据表] {c.DisplayName}",
                DataType = c.DataType,
            })
        );

        // 2. 公共维度
        var publicDimensions = await _publicDimensionRepository
            .AsQueryable()
            .Where(t => t.DeleteMark == null)
            .ToListAsync();

        sources.AddRange(
            publicDimensions.Select(d => new FormulaVariableSource
            {
                SourceType = "PUBLIC_DIMENSION",
                TableName = "LAB_PUBLIC_DIMENSION",
                VariableKey = d.DimensionKey,
                DisplayName = $"[公共维度] {d.DimensionName}",
                DataType = d.ValueType,
            })
        );

        // 3. 外观特性（特性名称）
        var appearanceFeatures = await _appearanceFeatureRepository
            .AsQueryable()
            .Where(t => t.DeleteMark == null)
            .ToListAsync();

        sources.AddRange(
            appearanceFeatures.Select(f => new FormulaVariableSource
            {
                SourceType = "APPEARANCE_FEATURE",
                TableName = "LAB_APPEARANCE_FEATURE",
                VariableKey = $"FEATURE_{f.Id}",
                DisplayName = $"[外观特性] {f.Name}",
                DataType = "text",
            })
        );

        // 4. 严重等级（等级名称）
        var severityLevels = await _severityLevelRepository
            .AsQueryable()
            .Where(t => t.DeleteMark == null && t.Enabled)
            .ToListAsync();

        sources.AddRange(
            severityLevels.Select(l => new FormulaVariableSource
            {
                SourceType = "SEVERITY_LEVEL",
                TableName = "LAB_APPEARANCE_FEATURE_LEVEL",
                VariableKey = $"LEVEL_{l.Id}",
                DisplayName = $"[严重等级] {l.Name}",
                DataType = "text",
            })
        );

        // 5. 产品规格属性
        var productSpecAttributes = await _productSpecAttributeRepository
            .AsQueryable()
            .Where(t => t.DeleteMark == null)
            .ToListAsync();

        sources.AddRange(
            productSpecAttributes.Select(a => new FormulaVariableSource
            {
                SourceType = "PRODUCT_SPEC_ATTRIBUTE",
                TableName = "LAB_PRODUCT_SPEC_ATTRIBUTE",
                VariableKey = a.AttributeKey,
                DisplayName = $"[产品规格属性] {a.AttributeName}",
                DataType = a.ValueType,
            })
        );

        return sources.OrderBy(s => s.SourceType).ThenBy(s => s.DisplayName).ToList();
    }

    /// <inheritdoc />
    [HttpPost("validate")]
    public async Task<FormulaValidationResult> ValidateFormulaAsync(
        [FromBody] FormulaValidationRequest request
    )
    {
        var result = new FormulaValidationResult();

        try
        {
            // 获取所有可用的变量源
            var variableSources = await GetVariableSourcesAsync();

            return result;
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.ErrorMessage = $"验证过程中发生错误: {ex.Message}";
            return result;
        }
    }

    #region 辅助方法

    private string GetDisplayName(PropertyInfo prop)
    {
        // 尝试从注释或属性名生成显示名称
        var sugarColumn = prop.GetCustomAttribute<IntermediateDataColumnAttribute>();
        if (sugarColumn != null && !string.IsNullOrEmpty(sugarColumn.ColumnDescription))
        {
            return sugarColumn.ColumnDescription;
        }

        // 从属性名生成显示名称（驼峰转中文）
        return ConvertCamelCaseToChinese(prop.Name);
    }

    private string ConvertCamelCaseToChinese(string camelCase)
    {
        // 简单的转换逻辑，可以根据需要扩展
        // 这里返回属性名，实际项目中可以维护一个映射表
        return camelCase;
    }

    private string GetDataTypeName(Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            type = type.GetGenericArguments()[0];
        }

        return type.Name.ToLower() switch
        {
            "decimal" => "decimal",
            "int32" => "int",
            "int64" => "long",
            "double" => "double",
            "single" => "float",
            "string" => "string",
            "boolean" => "bool",
            "datetime" => "datetime",
            _ => "object",
        };
    }

    #endregion
}
