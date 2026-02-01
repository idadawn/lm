using Newtonsoft.Json.Linq;
using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Dtos.Message;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Security;
using Poxiao.Message.Interfaces;
using Poxiao.Systems.Interfaces.Permission;
using Poxiao.Systems.Interfaces.System;
using Poxiao.WorkFlow.Entitys.Entity;
using Poxiao.WorkFlow.Entitys.Model;
using Poxiao.WorkFlow.Entitys.Model.Conifg;
using Poxiao.WorkFlow.Interfaces.Repository;

namespace Poxiao.WorkFlow.Manager;

public class FlowTaskMsgUtil
{
    private readonly IMessageManager _messageManager;

    private readonly IDataInterfaceService _dataInterfaceService;

    public readonly IFlowTaskRepository _flowTaskRepository;

    public readonly IUserManager _userManager;

    public readonly IUsersService _usersService;

    public FlowTaskMsgUtil(IMessageManager messageManager, IFlowTaskRepository flowTaskRepository, IUserManager userManager, IUsersService usersService, IDataInterfaceService dataInterfaceService)
    {
        _messageManager = messageManager;
        _flowTaskRepository = flowTaskRepository;
        _userManager = userManager;
        _usersService = usersService;
        _dataInterfaceService = dataInterfaceService;
    }

    #region 消息推送

    /// <summary>
    /// 通过消息模板获取消息通知.
    /// </summary>
    /// <param name="msgConfig">消息配置.</param>
    /// <param name="users">通知人员.</param>
    /// <param name="flowTaskParamter">任务参数.</param>
    /// <param name="enCode">默认站内信编码.</param>
    /// <param name="bodyDic">跳转数据.</param>
    /// <returns></returns>
    public async Task Alerts(MsgConfig msgConfig, List<string> users, FlowTaskParamter flowTaskParamter, string enCode, Dictionary<string, object> bodyDic = null)
    {
        //自定义消息
        if (msgConfig.on == 1 || msgConfig.on == 2)
        {
            foreach (var item in msgConfig.templateJson)
            {
                item.toUser = users;
                GetMsgContent(item.paramJson, flowTaskParamter);
                await _messageManager.SendDefinedMsg(item, bodyDic);
            }
        }

        //默认消息
        if (msgConfig.on == 3)
        {
            var crUser = await _usersService.GetUserName(flowTaskParamter.flowTaskEntity.CreatorUserId, false);
            var paramDic = new Dictionary<string, string>();
            paramDic.Add("@Title", flowTaskParamter.flowTaskEntity.FullName);
            paramDic.Add("@CreatorUserName", crUser);
            var msgEntity = _messageManager.GetMessageEntity(enCode, paramDic, 2);
            var msgReceiveList = _messageManager.GetMessageReceiveList(users, msgEntity, bodyDic);
            await _messageManager.SendDefaultMsg(users, msgEntity, msgReceiveList);
        }
    }

    /// <summary>
    /// 获取消息模板内容.
    /// </summary>
    /// <param name="templateJsonItems">消息模板json.</param>
    /// <param name="flowTaskParamter">任务参数.</param>
    public Dictionary<string, string> GetMsgContent(List<MessageSendParam> templateJsonItems, FlowTaskParamter flowTaskParamter)
    {
        var jObj = flowTaskParamter.flowTaskEntity.FlowFormContentJson.ToObject<JObject>();
        var dic = new Dictionary<string, string>();
        foreach (var item in templateJsonItems)
        {
            var value = string.Empty;
            if (item.relationField.Equals("@flowOperatorUserId"))
            {
                value = _userManager.UserId;
            }
            else if (item.relationField.Equals("@taskId"))
            {
                value = flowTaskParamter.flowTaskEntity.Id;
            }
            else if (item.relationField.Equals("@taskNodeId"))
            {
                value = flowTaskParamter.flowTaskNodeEntity.Id;
            }
            else if (item.relationField.Equals("@taskFullName"))
            {
                value = flowTaskParamter.flowTaskEntity.FullName;
            }
            else if (item.relationField.Equals("@launchUserId"))
            {
                value = flowTaskParamter.flowTaskEntity.CreatorUserId;
            }
            else if (item.relationField.Equals("@launchUserName"))
            {
                value = _usersService.GetInfoByUserId(flowTaskParamter.flowTaskEntity.CreatorUserId).RealName;
            }
            else if (item.relationField.Equals("@flowOperatorUserName"))
            {
                value = _userManager.User.RealName;
            }
            else if (item.relationField.Equals("@flowId"))
            {
                value = flowTaskParamter.flowTaskEntity.FlowId;
            }
            else if (item.relationField.Equals("@flowFullName"))
            {
                value = flowTaskParamter.flowTaskEntity.FlowName;
            }
            else
            {
                if (item.isSubTable)
                {
                    var fields = item.relationField.Split("-").ToList();
                    // 子表键值
                    var tableField = fields[0];
                    // 子表字段键值
                    var keyField = fields[1];
                    if (jObj.ContainsKey(tableField) && jObj[tableField] is JArray)
                    {
                        var jar = jObj[tableField] as JArray;

                        value = jar.Where(x => x.ToObject<JObject>().ContainsKey(keyField)).Select(x => x.ToObject<JObject>()[keyField]).ToJsonString();
                    }
                }
                else
                {
                    value = jObj.ContainsKey(item.relationField) ? jObj[item.relationField].ToString() : string.Empty;
                }
            }
            item.value = value;
            dic.Add(item.field, value);
        }
        if (!templateJsonItems.Any(x => x.field == "@Title"))
        {
            templateJsonItems.Add(new MessageSendParam
            {
                field = "@Title",
                value = flowTaskParamter.flowTaskEntity.FullName
            });
        }
        if (!templateJsonItems.Any(x => x.field == "@CreatorUserName"))
        {
            templateJsonItems.Add(new MessageSendParam
            {
                field = "@CreatorUserName",
                value = _userManager.GetUserName(flowTaskParamter.flowTaskEntity.CreatorUserId)
            });
        }
        return dic;
    }

    /// <summary>
    /// 组装消息跳转详情参数.
    /// </summary>
    /// <param name="flowTaskParamter">流程实例.</param>
    /// <param name="userList">通知人员.</param>
    /// <param name="flowTaskOperatorEntities">经办实例.</param>
    /// <param name="type">1:发起，2：待办，3：抄送.</param>
    /// <returns></returns>
    public Dictionary<string, object> GetMesBodyText(FlowTaskParamter flowTaskParamter, List<string> userList, List<FlowTaskOperatorEntity> flowTaskOperatorEntities, int type, string remark = "")
    {
        var dic = new Dictionary<string, object>();
        if (flowTaskOperatorEntities.IsNotEmptyOrNull() && flowTaskOperatorEntities.Count > 0)
        {
            foreach (var item in flowTaskOperatorEntities)
            {
                var value = new
                {
                    enCode = flowTaskParamter.flowTaskEntity.FlowCode,
                    flowId = flowTaskParamter.flowTaskEntity.FlowId,
                    status = type == 1 ? 0 : 1,
                    processId = item.TaskId,
                    taskNodeId = item.TaskNodeId,
                    taskOperatorId = item.Id,
                    type = type,
                    remark = remark
                };
                dic.Add(item.HandleId, value);
                if (type == 2)
                {
                    var toUserId = _flowTaskRepository.GetToUserId(item.HandleId, flowTaskParamter.flowTaskEntity.TemplateId);
                    toUserId.ForEach(u => dic[u + "-delegate"] = value);
                }
            }
        }
        else
        {
            var value = new
            {
                enCode = flowTaskParamter.flowTaskEntity.FlowCode,
                flowId = flowTaskParamter.flowTaskEntity.FlowId,
                status = type == 1 ? 0 : 1,
                processId = flowTaskParamter.flowTaskEntity.Id,
                taskNodeId = flowTaskParamter.flowTaskNodeEntity?.Id,
                taskOperatorId = flowTaskParamter.flowTaskOperatorEntity?.Id,
                type = type,
                remark = remark
            };
            userList.ForEach(u => dic.Add(u, value));
        }
        return dic;
    }

    /// <summary>
    /// 事件请求.
    /// </summary>
    /// <param name="funcConfig">事件配置.</param>
    /// <param name="flowTaskParamter">表单数据.</param>
    /// <returns></returns>
    public async Task RequestEvents(FuncConfig funcConfig, FlowTaskParamter flowTaskParamter)
    {
        if (funcConfig.IsNotEmptyOrNull() && funcConfig.on && funcConfig.interfaceId.IsNotEmptyOrNull())
        {
            var parameters = GetMsgContent(funcConfig.templateJson, flowTaskParamter);
            await _dataInterfaceService.GetResponseByType(funcConfig.interfaceId, 3, _userManager.TenantId, null, parameters);
        }
    }

    /// <summary>
    /// 委托消息通知.
    /// </summary>
    /// <param name="delegateType">委托类型:发起，审批.</param>
    /// <param name="ToUserId">通知人员.</param>
    /// <param name="flowName">流程名.</param>
    /// <returns></returns>
    public async Task SendDelegateMsg(string delegateType, string ToUserId, string flowName)
    {
        var paramDic = new Dictionary<string, string>();
        paramDic.Add("delegateType", delegateType);
        paramDic.Add("flowName", flowName);
        var msgEntity = _messageManager.GetMessageEntity(string.Empty, paramDic, 2, 2);
        var bodyDic = new Dictionary<string, object>();
        bodyDic.Add(ToUserId, new { type = "1" });
        var msgReceiveList = _messageManager.GetMessageReceiveList(new List<string>() { ToUserId }, msgEntity, bodyDic);
        await _messageManager.SendDefaultMsg(new List<string>() { ToUserId }, msgEntity, msgReceiveList);
    }
    #endregion
}
