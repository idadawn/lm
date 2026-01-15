namespace Poxiao.Kpi.Application;

/// <summary>
/// Mapper转换.
/// </summary>
public class MetricCategoryMapper : IRegister
{
    /// <summary>
    /// 注册.
    /// </summary>
    /// <param name="config">config.</param>
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<DbLinkListOutput, DataModel4DbOutput>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.fullName)
            .Map(dest => dest.DbType, src => src.dbType)
            .Map(dest => dest.Host, src => src.host)
            .Map(dest => dest.SortCode, src => src.sortCode)
            ;

        config.ForType<DataModel4DbOutput, DbSchemaOutput>()
            .Map(dest => dest.ParentId, src => "-1")

            ;

        config.ForType<DbTableInfo, DbSchemaOutput>()
            .Map(dest => dest.ParentId, src => "-1")

            ;
    }
}