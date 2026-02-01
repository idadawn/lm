using Mapster;
using Poxiao.Infrastructure.Models;
using Poxiao.Infrastructure.Models.VisualDev;
using Poxiao.Infrastructure.Security;

namespace Poxiao.VisualDev.Engine.Mapper;

internal class VisualDev : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<FieldsModel, ListSearchParametersModel>()
           .Map(dest => dest.poxiaoKey, src => src.Config.poxiaoKey)
           .Map(dest => dest.format, src => src.format)
           .Map(dest => dest.multiple, src => src.multiple)
           .Map(dest => dest.searchType, src => src.searchType)
           .Map(dest => dest.vModel, src => src.VModel);
        config.ForType<CodeGenFieldsModel, FieldsModel>()
           .Map(dest => dest.Config, src => src.Config.ToObject<ConfigModel>())
           .Map(dest => dest.props, src => string.IsNullOrEmpty(src.props) ? null : src.props.ToObject<CodeGenPropsModel>())
           .Map(dest => dest.options, src => string.IsNullOrEmpty(src.options) ? null : src.options.ToObject<List<object>>())
           .Map(dest => dest.ableDepIds, src => string.IsNullOrEmpty(src.ableDepIds) ? null : src.ableDepIds.ToObject<List<string>>())
           .Map(dest => dest.ablePosIds, src => string.IsNullOrEmpty(src.ablePosIds) ? null : src.ablePosIds.ToObject<List<string>>())
           .Map(dest => dest.ableUserIds, src => string.IsNullOrEmpty(src.ableUserIds) ? null : src.ableUserIds.ToObject<List<string>>())
           .Map(dest => dest.ableRoleIds, src => string.IsNullOrEmpty(src.ableRoleIds) ? null : src.ableRoleIds.ToObject<List<string>>())
           .Map(dest => dest.ableGroupIds, src => string.IsNullOrEmpty(src.ableGroupIds) ? null : src.ableGroupIds.ToObject<List<string>>())
           .Map(dest => dest.ableIds, src => string.IsNullOrEmpty(src.ableIds) ? null : src.ableIds.ToObject<List<string>>());
    }
}