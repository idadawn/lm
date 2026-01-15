namespace Poxiao.Kpi.Application;

/// <summary>
/// Mapper转换.
/// </summary>
public class MetricInfoMapper : IRegister
{
    /// <summary>
    /// 注册.
    /// </summary>
    /// <param name="config">config.</param>
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<MetricInfoCrInput, MetricInfoEntity>()
            .Map(dest => dest.DataModelId, src => src.DataModelId.ToJsonString())
            .Map(dest => dest.Column, src => src.Column.ToJsonString())
            .Map(dest => dest.Format, src => src.Format == null ? "" : src.Format.ToJsonString())
            .Map(dest => dest.Dimensions, src => src.Dimensions == null ? "" : src.Dimensions.ToJsonString())
            .Map(dest => dest.Filters, src => src.Filters == null ? "" : src.Filters.ToJsonString())
            .Map(dest => dest.TimeDimensions, src => src.TimeDimensions == null ? "" : src.TimeDimensions.ToJsonString())
            .Map(dest => dest.MetricTag, src => src.MetricTag == null ? "" : src.MetricTag.Join(','))
            ;

        config.ForType<MetricInfo4DeriveCrInput, MetricInfoEntity>()
            .Map(dest => dest.DataModelId, src => src.DataModelId.ToJsonString())
            .Map(dest => dest.Column, src => src.Column.ToJsonString())
            .Map(dest => dest.Format, src => src.Format == null ? "" : src.Format.ToJsonString())
            .Map(dest => dest.Dimensions, src => src.Dimensions == null ? "" : src.Dimensions.ToJsonString())
            .Map(dest => dest.Filters, src => src.Filters == null ? "" : src.Filters.ToJsonString())
            .Map(dest => dest.TimeDimensions, src => src.TimeDimensions == null ? "" : src.TimeDimensions.ToJsonString())
            .Map(dest => dest.GranularityStr, src => src.GranularityStr == null ? "" : src.GranularityStr.ToJsonString())
            .Map(dest => dest.MetricTag, src => src.MetricTag == null ? "" : src.MetricTag.Join(','))
            ;

        config.ForType<MetricInfo4CompositeCrInput, MetricInfoEntity>()
            .Map(dest => dest.Format, src => src.Format == null ? "" : src.Format.ToJsonString())
            .Map(dest => dest.Dimensions, src => src.Dimensions == null ? "" : src.Dimensions.ToJsonString())
            .Map(dest => dest.Filters, src => src.Filters == null ? "" : src.Filters.ToJsonString())
            .Map(dest => dest.TimeDimensions, src => src.TimeDimensions == null ? "" : src.TimeDimensions.ToJsonString())
            .Map(dest => dest.MetricTag, src => src.MetricTag == null ? "" : src.MetricTag.Join(','))
            .Map(dest => dest.ParentId, src => src.ParentIds.Join(','))
            ;

        config.ForType<MetricInfoEntity, MetricInfoInfoOutput>()
            .Map(dest => dest.DataModelId, src => src.DataModelId.IsNotEmptyOrNull() ? src.DataModelId.ToObject<DbSchemaOutput>() : new DbSchemaOutput())
            .Map(dest => dest.Column, src => src.Column.IsNotEmptyOrNull() && src.Type.Equals(MetricType.Basic) ? src.Column.ToObject<TableFieldOutput>() : new TableFieldOutput())
            .Map(dest => dest.Format, src => src.Format.IsNotEmptyOrNull() ? src.Format.ToObject<DataModelFormat>() : null)
            .Map(dest => dest.Dimensions, src => src.Dimensions != null && src.Dimensions.IsNotEmptyOrNull() ? src.Dimensions.ToObject<List<TableFieldOutput>>() : null)
            .Map(dest => dest.Filters, src => src.Filters != null && src.Filters.IsNotEmptyOrNull() ? src.Filters.ToObject<List<MetricFilterDto>>() : null)
            .Map(dest => dest.TimeDimensions, src => src.TimeDimensions != null && src.TimeDimensions.IsNotEmptyOrNull() ? src.TimeDimensions.ToObject<MetricTimeDimensionDto>() : null)
            ;


        config.ForType<MetricInfoEntity, MetricInfo4DeriveOutput>()
            .Map(dest => dest.DataModelId, src => src.DataModelId.IsNotEmptyOrNull() ? src.DataModelId.ToObject<DbSchemaOutput>() : new DbSchemaOutput())
            .Map(dest => dest.Column, src => src.Column.IsNotEmptyOrNull() ? src.Column.ToObject<TableFieldOutput>() : new TableFieldOutput())
            .Map(dest => dest.Format, src => src.Format.IsNotEmptyOrNull() ? src.Format.ToObject<DataModelFormat>() : null)
            .Map(dest => dest.Dimensions, src => src.Dimensions != null && src.Dimensions.IsNotEmptyOrNull() ? src.Dimensions.ToObject<List<TableFieldOutput>>() : null)
            .Map(dest => dest.Filters, src => src.Filters != null && src.Filters.IsNotEmptyOrNull() ? src.Filters.ToObject<List<MetricFilterDto>>() : null)
            .Map(dest => dest.TimeDimensions, src => src.TimeDimensions != null && src.TimeDimensions.IsNotEmptyOrNull() ? src.TimeDimensions.ToObject<MetricTimeDimensionDto>() : null)
            .Map(dest => dest.GranularityStr, src => src.GranularityStr != null && src.GranularityStr.IsNotEmptyOrNull() ? src.GranularityStr.ToObject<CalculationGranularityModel>() : null)
            ;

        config.ForType<MetricInfoEntity, MetricInfo4CompositeOutput>()
            .Map(dest => dest.Format, src => src.Format.IsNotEmptyOrNull() ? src.Format.ToObject<DataModelFormat>() : null)
            .Map(dest => dest.Dimensions, src => src.Dimensions != null && src.Dimensions.IsNotEmptyOrNull() ? src.Dimensions.ToObject<List<TableFieldOutput>>() : null)
            .Map(dest => dest.Filters, src => src.Filters != null && src.Filters.IsNotEmptyOrNull() ? src.Filters.ToObject<List<MetricFilterDto>>() : null)
            .Map(dest => dest.TimeDimensions, src => src.TimeDimensions != null && src.TimeDimensions.IsNotEmptyOrNull() ? src.TimeDimensions.ToObject<MetricTimeDimensionDto>() : null)
            .Map(dest => dest.ParentIds, src => src.ParentId.IsNotEmptyOrNull() ? src.ParentId.Split2List(",", true) : null)
            ;
    }
}