using Poxiao.Infrastructure.Extension;
using Poxiao.WorkFlow.Entitys.Dto.FlowBefore;
using Poxiao.WorkFlow.Entitys.Dto.FlowEngine;
using Poxiao.WorkFlow.Entitys.Dto.FlowLaunch;
using Poxiao.WorkFlow.Entitys.Entity;
using Poxiao.WorkFlow.Entitys.Model;
using Poxiao.WorkFlow.Entitys.Model.Properties;
using Mapster;

namespace Poxiao.WorkFlow.Entitys.Mapper;

internal class Mapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.ForType<FlowEngineEntity, FlowEngineListAllOutput>()
            .Map(dest => dest.formData, src => src.FormTemplateJson);
        config.ForType<FlowEngineEntity, FlowEngineInfoOutput>()
            .Map(dest => dest.formData, src => src.FormTemplateJson)
            .Map(dest => dest.dbLinkId, src => src.DbLinkId.IsEmpty() ? "0" : src.DbLinkId);
        config.ForType<FlowEngineCrInput, FlowEngineEntity>()
            .Map(dest => dest.FormTemplateJson, src => src.formData);
        config.ForType<FlowEngineUpInput, FlowEngineEntity>()
            .Map(dest => dest.FormTemplateJson, src => src.formData);
        config.ForType<FlowEngineEntity, FlowBeforeListOutput>()
            .Map(dest => dest.formData, src => src.FlowTemplateJson);
        config.ForType<FlowTemplateJsonModel, TaskNodeModel>()
            .Map(dest => dest.upNodeId, src => src.prevId);
        config.ForType<ChildTaskProperties, ApproversProperties>()
            .Map(dest => dest.assigneeType, src => src.initiateType)
            .Map(dest => dest.approvers, src => src.initiator)
            .Map(dest => dest.approverRole, src => src.initiateRole)
            .Map(dest => dest.approverPos, src => src.initiatePos)
            .Map(dest => dest.approverOrg, src => src.initiateOrg)
            .Map(dest => dest.approverGroup, src => src.initiateGroup)
            .Map(dest => dest.extraRule, src => "1");
        config.ForType<StartProperties, ApproversProperties>()
            .Map(dest => dest.approvers, src => src.initiator)
            .Map(dest => dest.approverRole, src => src.initiateRole)
            .Map(dest => dest.approverPos, src => src.initiatePos)
            .Map(dest => dest.approverOrg, src => src.initiateOrg)
            .Map(dest => dest.approverGroup, src => src.initiateGroup);
    }
}
