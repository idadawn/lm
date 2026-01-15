namespace Poxiao.Kpi.Application;

/// <summary>
/// Mapper转换.
/// </summary>
public class MetricDimMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<MetricDimensionCrInput, MetricDimensionEntity>()
            .Map(dest => dest.DataModelId, src => src.DataModelId.ToJsonString())
            .Map(dest => dest.Column, src => src.Column.ToJsonString())
            ;

        config.ForType<MetricDimensionEntity, MetricDimensionInfoOutput>()
            .Map(dest => dest.Column, src => src.Column.IsNotEmptyOrNull() ? src.Column.ToObject<TableFieldOutput>() : new TableFieldOutput())
            .Map(dest => dest.DataModelId, src => src.DataModelId.IsNotEmptyOrNull() ? src.DataModelId.ToObject<DbSchemaOutput>() : new DbSchemaOutput())
            ;
    }
}

