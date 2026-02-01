using Mapster;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Security;
using Poxiao.TaskScheduler.Entitys;
using Poxiao.TaskScheduler.Entitys.Dto.TaskScheduler;
using Poxiao.TaskScheduler.Entitys.Model;

namespace Poxiao.TaskScheduler.Entitys.Mapper;

public class Mapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<TimeTaskEntity, TimeTaskListOutput>()
            .Map(dest => dest.startTime, src => src.ExecuteContent.ToObject<ContentModel>().startTime)
            .Map(dest => dest.endTime, src => src.ExecuteContent.ToObject<ContentModel>().endTime);
    }
}
