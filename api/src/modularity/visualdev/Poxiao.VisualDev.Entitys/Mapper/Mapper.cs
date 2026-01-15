using Poxiao.VisualDev.Entitys.Dto.VisualDev;
using Mapster;

namespace Poxiao.VisualDev.Entitys.Mapper;

public class Mapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<VisualDevEntity, VisualDevSelectorOutput>()
            .Map(dest => dest.ParentId, src => src.Category);
        config.ForType<VisualDevReleaseEntity, VisualDevSelectorOutput>()
            .Map(dest => dest.ParentId, src => src.Category);
    }
}
