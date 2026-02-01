using Mapster;
using Newtonsoft.Json.Linq;
using Poxiao.Extras.Thirdparty.JSEngine;
using Poxiao.FriendlyException;
using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Models.WorkFlow;
using Poxiao.Infrastructure.Security;
using Poxiao.Systems.Interfaces.Permission;
using Poxiao.WorkFlow.Entitys.Entity;
using Poxiao.WorkFlow.Entitys.Enum;
using Poxiao.WorkFlow.Entitys.Model;
using Poxiao.WorkFlow.Entitys.Model.Item;
using Poxiao.WorkFlow.Entitys.Model.Properties;
using Poxiao.WorkFlow.Interfaces.Repository;
using System.Text;

namespace Poxiao.WorkFlow.Manager;

public class FlowTemplateUtil
{
    private readonly IDataBaseManager _dataBaseManager;
    private readonly IUserManager _userManager;
    private readonly IFlowTaskRepository _flowTaskRepository;
    private readonly IUsersService _usersService;

    /// <summary>
    /// 可用节点.
    /// </summary>
    public List<FlowTaskNodeEntity> flowTaskNodeEntityList { get; set; } = new List<FlowTaskNodeEntity>();

    /// <summary>
    /// 所有节点.
    /// </summary>
    public List<TaskNodeModel> taskNodeList { get; set; } = new List<TaskNodeModel>();

    /// <summary>
    /// 开始节点.
    /// </summary>
    public FlowTaskNodeEntity startNode { get; set; } = new FlowTaskNodeEntity();

    /// <summary>
    /// 开始节点属性.
    /// </summary>
    public StartProperties startPro { get; set; } = new StartProperties();

    public FlowTemplateUtil(IDataBaseManager dataBaseManager, IUserManager userManager, IFlowTaskRepository flowTaskRepository,
        IUsersService usersService)
    {
        _dataBaseManager = dataBaseManager;
        _userManager = userManager;
        _flowTaskRepository = flowTaskRepository;
        _usersService = usersService;
    }

    /// <summary>
    /// 加载参数.
    /// </summary>
    /// <param name="flowEngineEntity"></param>
    /// <param name="formData"></param>
    /// <param name="taskId"></param>
    public void Load(FlowJsonModel flowJsonModel, string formData, string taskId, bool isDeleteCondition = true)
    {
        this.flowTaskNodeEntityList.Clear();
        this.taskNodeList.Clear();
        var flowTemplateJsonModel = flowJsonModel.flowTemplateJson.ToObject<FlowTemplateJsonModel>();
        #region 流程模板所有节点
        var flowTemplateJsonModelList = new List<FlowTemplateJsonModel>();
        var childNodeIdList = new List<string>();
        GetChildNodeIdList(flowTemplateJsonModel, childNodeIdList);
        GetFlowTemplateList(flowTemplateJsonModel, flowTemplateJsonModelList);
        #endregion
        GetFlowTemplateAll(flowTemplateJsonModel, this.taskNodeList, flowTemplateJsonModelList, childNodeIdList, taskId);
        if (isDeleteCondition)
        {
            DeleteConditionTaskNodeModel(taskNodeList, formData, taskId);
            var defaultFormId = this.taskNodeList.Find(m => FlowTaskNodeTypeEnum.start.ParseToString().Equals(m.type)).propertyJson.formId;
            foreach (var item in this.taskNodeList)
            {
                var flowTaskNodeEntity = new FlowTaskNodeEntity();
                flowTaskNodeEntity.Id = SnowflakeIdHelper.NextId();
                flowTaskNodeEntity.CreatorTime = DateTime.Now;
                flowTaskNodeEntity.TaskId = item.taskId;
                flowTaskNodeEntity.NodeCode = item.nodeId;
                flowTaskNodeEntity.NodeType = item.type;
                flowTaskNodeEntity.Completion = FlowTaskNodeTypeEnum.start.ParseToString().Equals(item.type) ? 1 : 0;
                flowTaskNodeEntity.NodeName = FlowTaskNodeTypeEnum.start.ParseToString().Equals(item.type) ? "开始" : item.propertyJson.title;
                flowTaskNodeEntity.NodeUp = !FlowTaskNodeTypeEnum.approver.ParseToString().Equals(item.type) ? null : item.propertyJson.rejectStep;
                flowTaskNodeEntity.NodeNext = item.nextNodeId;
                flowTaskNodeEntity.NodePropertyJson = JsonHelper.ToJsonString(item.propertyJson);
                flowTaskNodeEntity.State = "1";
                if (FlowTaskNodeTypeEnum.subFlow.ParseToString().Equals(item.type))
                {
                    string subFlowId = item.propertyJson.flowId; //子流程流程id
                    var jsonInfo = _flowTaskRepository.GetFlowTemplateJsonInfo(x => x.Id == subFlowId && x.DeleteMark == null);
                    flowTaskNodeEntity.FormId = jsonInfo.FlowTemplateJson.ToObject<FlowTemplateJsonModel>().properties.ToObject<StartProperties>().formId;
                }
                else
                {
                    if (!"timer".Equals(item.type))
                    {
                        string formId = item.propertyJson.formId;
                        flowTaskNodeEntity.FormId = formId.IsNotEmptyOrNull() ? formId : defaultFormId;
                    }
                }
                flowTaskNodeEntityList.Add(flowTaskNodeEntity);
            }
            DeleteTimerTaskNode(this.flowTaskNodeEntityList);
            this.startNode = this.flowTaskNodeEntityList.Find(m => FlowTaskNodeTypeEnum.start.ParseToString().Equals(m.NodeType));
            this.startPro = this.startNode.NodePropertyJson.ToObject<StartProperties>();
        }
    }

    /// <summary>
    /// 递归获取流程模板最外层childNode中所有nodeid.
    /// </summary>
    /// <param name="template">流程模板实例.</param>
    /// <param name="childNodeIdList">子节点id.</param>
    private void GetChildNodeIdList(FlowTemplateJsonModel template, List<string> childNodeIdList)
    {
        if (template.IsNotEmptyOrNull() && template.childNode.IsNotEmptyOrNull())
        {
            childNodeIdList.Add(template.childNode.nodeId);
            GetChildNodeIdList(template.childNode, childNodeIdList);
        }
    }

    /// <summary>
    /// 递归获取流程模板数组.
    /// </summary>
    /// <param name="template">流程模板.</param>
    /// <param name="templateList">流程模板数组.</param>
    private void GetFlowTemplateList(FlowTemplateJsonModel template, List<FlowTemplateJsonModel> templateList)
    {
        if (template.IsNotEmptyOrNull())
        {
            var haschildNode = template.childNode.IsNotEmptyOrNull();
            var hasconditionNodes = template.conditionNodes.IsNotEmptyOrNull() && template.conditionNodes.Count > 0;

            templateList.Add(template);

            if (hasconditionNodes)
            {
                foreach (var conditionNode in template.conditionNodes)
                {
                    GetFlowTemplateList(conditionNode, templateList);
                }
            }

            if (haschildNode)
            {
                GetFlowTemplateList(template.childNode, templateList);
            }
        }
    }

    /// <summary>
    /// 递归审批模板获取所有节点.
    /// </summary>
    /// <param name="template">当前审批流程json.</param>
    /// <param name="nodeList">流程节点数组.</param>
    /// <param name="templateList">流程模板数组.</param>
    private void GetFlowTemplateAll(FlowTemplateJsonModel template, List<TaskNodeModel> nodeList, List<FlowTemplateJsonModel> templateList, List<string> childNodeIdList, string taskId = "")
    {
        try
        {
            if (template.IsNotEmptyOrNull())
            {
                var taskNodeModel = template.Adapt<TaskNodeModel>();
                taskNodeModel.taskId = taskId;
                taskNodeModel.propertyJson = GetPropertyByType(template.type, template.properties);
                if (taskNodeModel.isBranchFlow)
                {
                    taskNodeModel.propertyJson.isBranchFlow = taskNodeModel.isBranchFlow;
                }
                var haschildNode = template.childNode.IsNotEmptyOrNull();
                var hasconditionNodes = template.conditionNodes.IsNotEmptyOrNull() && template.conditionNodes.Count > 0;
                List<string> nextNodeIdList = new List<string> { string.Empty };
                if (templateList.Count > 1)
                {
                    nextNodeIdList = GetNextNodeIdList(templateList, template, childNodeIdList);
                }
                taskNodeModel.nextNodeId = string.Join(',', nextNodeIdList.ToArray());
                nodeList.Add(taskNodeModel);

                if (hasconditionNodes)
                {
                    foreach (var conditionNode in template.conditionNodes)
                    {
                        GetFlowTemplateAll(conditionNode, nodeList, templateList, childNodeIdList, taskId);
                    }
                }

                if (haschildNode)
                {
                    taskNodeModel.childNodeId = template.childNode.nodeId;
                    GetFlowTemplateAll(template.childNode, nodeList, templateList, childNodeIdList, taskId);
                }
            }
        }
        catch (AppFriendlyException ex)
        {
            throw Oops.Oh(ex.ErrorCode);
        }
    }

    /// <summary>
    /// 根据类型获取不同属性对象.
    /// </summary>
    /// <param name="type">属性类型.</param>
    /// <param name="jd">数据.</param>
    /// <returns></returns>
    private dynamic GetPropertyByType(string type, JObject jd)
    {
        switch (type)
        {
            case "approver":
                return jd.ToObject<ApproversProperties>();
            case "timer":
                return jd.ToObject<TimerProperties>();
            case "start":
                return jd.ToObject<StartProperties>();
            case "condition":
                return jd.ToObject<ConditionProperties>();
            case "subFlow":
                return jd.ToObject<ChildTaskProperties>();
            default:
                return jd;
        }
    }

    /// <summary>
    /// 获取当前模板的下一节点
    /// 下一节点数据来源：conditionNodes和childnode (conditionNodes优先级大于childnode)
    /// conditionNodes非空：下一节点则为conditionNodes数组中所有nodeID
    /// conditionNodes非空childNode非空：下一节点则为childNode的nodeId
    /// conditionNodes空childNode空则为最终节点(两种情况：当前模板属于conditionNodes的最终节点或childNode的最终节点)
    /// conditionNodes的最终节点:下一节点为与conditionNodes同级的childNode的nodeid,没有则继续递归，直到最外层的childNode
    /// childNode的最终节点直接为"".
    /// </summary>
    /// <param name="templateList">模板数组</param>
    /// <param name="template">当前模板</param>
    /// <param name="childNodeIdList">最外层childnode的nodeid集合</param>
    /// <returns></returns>
    private List<string> GetNextNodeIdList(List<FlowTemplateJsonModel> templateList, FlowTemplateJsonModel template, List<string> childNodeIdList)
    {
        List<string> nextNodeIdList = new List<string>();
        if (template.conditionNodes.IsNotEmptyOrNull() && template.conditionNodes.Count > 0)
        {
            nextNodeIdList = template.conditionNodes.Select(x => x.nodeId).ToList();
        }
        else
        {
            if (template.childNode.IsNotEmptyOrNull())
            {
                nextNodeIdList.Add(template.childNode.nodeId);
            }
            else
            {
                //判断是否是最外层的节点
                if (childNodeIdList.Contains(template.nodeId))
                {
                    nextNodeIdList.Add(string.Empty);
                }
                else
                {
                    //conditionNodes中最终节点
                    nextNodeIdList.Add(GetChildId(templateList, template, childNodeIdList));
                }
            }
        }
        return nextNodeIdList;
    }

    /// <summary>
    /// 递归获取conditionNodes最终节点下一节点.
    /// </summary>
    /// <param name="templateList">流程模板数组.</param>
    /// <param name="template">当前模板.</param>
    /// <param name="childNodeIdList">最外层childNode的节点数据.</param>
    /// <returns></returns>
    private string GetChildId(List<FlowTemplateJsonModel> templateList, FlowTemplateJsonModel template, List<string> childNodeIdList)
    {
        var prevModel = new FlowTemplateJsonModel();
        if (template.prevId.IsNotEmptyOrNull())
        {
            prevModel = templateList.Find(x => x.nodeId.Equals(template.prevId));
            if (prevModel.childNode.IsNotEmptyOrNull() && prevModel.childNode.nodeId != template.nodeId)
            {
                return prevModel.childNode.nodeId;
            }
            if (childNodeIdList.Contains(prevModel.nodeId))
            {
                return prevModel.childNode.IsNullOrEmpty() ? string.Empty : prevModel.childNode.nodeId;
            }
            else
            {
                return GetChildId(templateList, prevModel, childNodeIdList);
            }
        }
        else
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// 删除条件节点
    /// 将条件的上非条件的节点的nextnode替换成当前条件的nextnode.
    /// </summary>
    /// <param name="taskNodeModelList">所有节点数据.</param>
    /// <param name="formDataJson">填写表单数据.</param>
    /// <param name="taskId">任务id.</param>
    private void DeleteConditionTaskNodeModel(List<TaskNodeModel> taskNodeModelList, string formDataJson, string taskId)
    {
        var conditionTaskNodeModelList = taskNodeModelList.FindAll(x => FlowTaskNodeTypeEnum.condition.ParseToString().Equals(x.type));
        var dic = new Dictionary<string, List<TaskNodeModel>>();
        foreach (var item in conditionTaskNodeModelList.GroupBy(x => x.upNodeId))
        {
            dic.Add(item.Key, taskNodeModelList.FindAll(x => x.upNodeId == item.Key && FlowTaskNodeTypeEnum.condition.ParseToString().Equals(x.type)));
        }
        //条件的默认情况判断（同层条件的父节点是一样的，只要非默认的匹配成功则不需要走默认的）
        var isDefault = new List<string>();
        foreach (var nodeId in dic.Keys)
        {
            var index = 0;
            foreach (var item in dic[nodeId])
            {
                ++index;
                //条件节点的父节点且为非条件的节点
                var upTaskNodeModel = taskNodeModelList.Find(x => x.nodeId == nodeId);
                if (FlowTaskNodeTypeEnum.condition.ParseToString().Equals(upTaskNodeModel.type))
                {
                    // 父级条件不满足则子级条件则不需要验证
                    if (!upTaskNodeModel.propertyJson.isDefault && !ConditionNodeJudge(formDataJson, upTaskNodeModel.propertyJson, taskId))
                    {
                        break;
                    }
                    upTaskNodeModel = GetUpTaskNodeModelIsNotCondition(taskNodeModelList, upTaskNodeModel);
                    // 如果父节点下一节点存在某个审批节点则不需要判断了
                    if (taskNodeModelList.Where(x => upTaskNodeModel.nextNodeId.Contains(x.nodeId)).Any(y => FlowTaskNodeTypeEnum.approver.ParseToString().Equals(y.type)))
                    {
                        break;
                    }
                }
                if (!item.propertyJson.isDefault && ConditionNodeJudge(formDataJson, item.propertyJson, taskId))
                {
                    upTaskNodeModel.nextNodeId = item.nextNodeId;
                    isDefault.Add(item.upNodeId);
                    break;
                }
                else
                {
                    if (!isDefault.Contains(item.upNodeId) && item.propertyJson.isDefault)
                    {
                        upTaskNodeModel.nextNodeId = item.nextNodeId;
                    }
                    else
                    {
                        if (index == dic[nodeId].Count)
                        {
                            upTaskNodeModel.nextNodeId = upTaskNodeModel.childNodeId.IsNotEmptyOrNull() ? upTaskNodeModel.childNodeId : FlowTaskNodeTypeEnum.end.ParseToString();
                        }
                    }
                }
            }
        }

        if (formDataJson.IsNotEmptyOrNull())
        {
            taskNodeModelList.RemoveAll(x => FlowTaskNodeTypeEnum.condition.ParseToString().Equals(x.type));
        }
    }

    /// <summary>
    /// 向上递获取非条件的节点.
    /// </summary>
    /// <param name="taskNodeModelList">所有节点数据.</param>
    /// <param name="taskNodeModel">当前节点.</param>
    /// <returns></returns>
    private TaskNodeModel GetUpTaskNodeModelIsNotCondition(List<TaskNodeModel> taskNodeModelList, TaskNodeModel taskNodeModel)
    {
        var preTaskNodeModel = taskNodeModelList.Find(x => x.nodeId == taskNodeModel.upNodeId);
        if (FlowTaskNodeTypeEnum.condition.ParseToString().Equals(preTaskNodeModel.type))
        {
            return GetUpTaskNodeModelIsNotCondition(taskNodeModelList, preTaskNodeModel);
        }
        else
        {
            return preTaskNodeModel;
        }
    }

    /// <summary>
    /// 条件判断.
    /// </summary>
    /// <param name="formDataJson">表单填写数据.</param>
    /// <param name="conditionPropertie">条件属性.</param>
    /// <param name="taskId">任务id.</param>
    /// <returns></returns>
    private bool ConditionNodeJudge(string formDataJson, ConditionProperties conditionPropertie, string taskId)
    {
        try
        {
            bool flag = false;
            StringBuilder expression = new StringBuilder();
            expression.AppendFormat("select * from base_user where  ");
            var formData = formDataJson.ToObject<JObject>();
            int i = 0;
            foreach (ConditionsItem flowNodeWhereModel in conditionPropertie.conditions)
            {
                var logic = flowNodeWhereModel.logic;
                var symbol = flowNodeWhereModel.symbol.Equals("==") ? "=" : flowNodeWhereModel.symbol;
                // 条件值
                var formValue = GetConditionValue(flowNodeWhereModel.fieldType.ParseToInt(), formData, flowNodeWhereModel.field, taskId, flowNodeWhereModel.poxiaoKey);
                // 匹配值
                var value = " ";
                if (flowNodeWhereModel.fieldValueType.ParseToInt() == 2)
                {
                    //数组类型控件
                    var poxiaoKeyList = new List<string>() { PoxiaoKeyConst.CASCADER, PoxiaoKeyConst.COMSELECT, PoxiaoKeyConst.ADDRESS, PoxiaoKeyConst.CURRORGANIZE };
                    if (poxiaoKeyList.Contains(flowNodeWhereModel.poxiaoKey) && flowNodeWhereModel.fieldValue.Count > 0)
                    {
                        if (flowNodeWhereModel.poxiaoKey.Equals(PoxiaoKeyConst.CURRORGANIZE))
                        {
                            value = flowNodeWhereModel.fieldValue[flowNodeWhereModel.fieldValue.Count - 1];
                        }
                        else
                        {
                            value = string.Join(",", flowNodeWhereModel.fieldValue);
                        }
                    }
                    else
                    {
                        value = flowNodeWhereModel.fieldValue.ToString();
                    }

                    if ("currentUser".Equals(value))
                    {
                        value = _userManager.UserId;
                    }

                    if (PoxiaoKeyConst.TIME.Equals(flowNodeWhereModel.poxiaoKey))
                    {
                        formValue = formValue.Replace(":", string.Empty);
                        value = value.Replace(":", string.Empty);
                    }
                }
                else
                {
                    value = GetConditionValue(flowNodeWhereModel.fieldValueType.ParseToInt(), formData, flowNodeWhereModel.fieldValue, taskId, flowNodeWhereModel.fieldValuePoxiaoKey);
                }

                if (symbol.Equals("=") || symbol.Equals("<>"))
                {
                    expression.AppendFormat("('{0}'{1}'{2}')", formValue, symbol, value);
                }
                else if (symbol.Equals("like"))
                {
                    expression.AppendFormat("('{0}' {1} '%{2}%')", formValue, symbol, value);
                }
                else if (symbol.Equals("notLike"))
                {
                    expression.AppendFormat("('{0}' {1} '%{2}%')", formValue, "not like", value);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(formValue) || string.IsNullOrWhiteSpace(value))
                    {
                        expression.Append("(1=2)");
                    }
                    else
                    {
                        expression.AppendFormat("({0}{1}{2})", formValue, symbol, value);
                    }
                }

                if (logic.IsNotEmptyOrNull() && i != conditionPropertie.conditions.Count - 1)
                {
                    expression.Append(" " + logic.Replace("&&", " and ").Replace("||", " or ") + " ");
                }

                i++;
            }
            var link = _dataBaseManager.GetTenantDbLink(_userManager.TenantId, _userManager.TenantDbName);
            flag = _dataBaseManager.WhereDynamicFilter(link, expression.ToString());
            return flag;
        }
        catch (Exception e)
        {
            return false;
        }
    }

    /// <summary>
    /// 获取条件匹配值.
    /// </summary>
    /// <param name="type">条件类型 1、字段 2、自定义 3、聚合函数.</param>
    /// <param name="formData">表单数据.</param>
    /// <param name="field">关联字段.</param>
    /// <param name="taskId">任务id.</param>
    /// <param name="poxiaoKey">控件key.</param>
    /// <returns></returns>
    private string GetConditionValue(int type, JObject formData, string field, string taskId, string poxiaoKey)
    {
        var conditionValue = " ";
        if (type == 1)
        {
            if (formData.ContainsKey(field))
            {
                if (formData[field] is JArray)
                {
                    try
                    {
                        var jar = formData[field].ToObject<List<string>>();
                        if (jar.Count > 0)
                        {
                            conditionValue = string.Join(",", jar);
                        }
                    }
                    catch (Exception e)
                    {
                        var arr = formData[field].ToObject<List<List<string>>>();
                        conditionValue = string.Join(",", arr.Select(x => string.Join(",", x)).ToList());
                    }
                }
                else
                {
                    if (formData[field].IsNotEmptyOrNull())
                    {
                        conditionValue = formData[field].ToString();
                    }
                    SysWidgetFormValue(taskId, poxiaoKey, ref conditionValue);
                }
            }
        }
        else
        {
            // 获取聚合函数要替换的参数key
            foreach (var item in field.Substring3())
            {
                if (formData.ContainsKey(item))
                {
                    field = field.Replace("{" + item + "}", "'" + formData[item] + "'");
                }
                else if (item.Contains("tableField") && item.Contains("-"))
                {
                    var fields = item.Split("-").ToList();
                    var tableField = fields[0];
                    var keyField = fields[1];
                    if (formData.ContainsKey(tableField) && formData[tableField] is JArray)
                    {
                        var jar = formData[tableField] as JArray;

                        var tableValue = jar.Where(x => x.ToObject<JObject>().ContainsKey(keyField)).Select(x => x.ToObject<JObject>()[keyField]).ToObject<List<string>>();
                        var valueStr = string.Join("','", tableValue);
                        field = field.Replace("{" + item + "}", "'" + valueStr + "'");
                    }
                }
                else
                {
                    field = field.Replace("{" + item + "}", "''");
                }
            }
            // 执行函数获取值
            conditionValue = JsEngineUtil.AggreFunction(field).ToString();
        }

        return conditionValue;
    }

    /// <summary>
    /// 系统控件条件匹配数据转换.
    /// </summary>
    /// <param name="taskId">任务id</param>
    /// <param name="poxiaoKey">条件匹配字段类型</param>
    /// <param name="formValue">条件匹配值</param>
    private void SysWidgetFormValue(string taskId, string poxiaoKey, ref string formValue)
    {
        var taskEntity = _flowTaskRepository.GetTaskFirstOrDefault(taskId);
        if (taskEntity.IsNotEmptyOrNull())
        {
            var creatorUser = _usersService.GetInfoByUserId(taskEntity.CreatorUserId);
            switch (poxiaoKey)
            {
                case PoxiaoKeyConst.CREATEUSER:
                    formValue = taskEntity.CreatorUserId;
                    break;
                case PoxiaoKeyConst.MODIFYUSER:
                    if (taskEntity.LastModifyUserId.IsNotEmptyOrNull())
                    {
                        formValue = _userManager.UserId;
                    }

                    break;
                case PoxiaoKeyConst.CURRORGANIZE:
                    if (creatorUser.OrganizeId.IsNotEmptyOrNull())
                    {
                        formValue = creatorUser.OrganizeId;
                    }

                    break;
                case PoxiaoKeyConst.CREATETIME:
                    formValue = ((DateTime)taskEntity.CreatorTime).ParseToUnixTime().ToString();
                    break;
                case PoxiaoKeyConst.MODIFYTIME:
                    if (taskEntity.LastModifyTime.IsNotEmptyOrNull())
                    {
                        formValue = DateTime.Now.ParseToUnixTime().ToString();
                    }

                    break;
                case PoxiaoKeyConst.CURRPOSITION:
                    if (creatorUser.PositionId.IsNotEmptyOrNull())
                    {
                        formValue = creatorUser.PositionId;
                    }

                    break;
            }
        }
        else
        {
            switch (poxiaoKey)
            {
                case PoxiaoKeyConst.CREATEUSER:
                    formValue = _userManager.UserId;
                    break;
                case PoxiaoKeyConst.MODIFYUSER:
                    formValue = " ";
                    break;
                case PoxiaoKeyConst.CURRORGANIZE:
                    if (_userManager.User.OrganizeId.IsNotEmptyOrNull())
                    {
                        formValue = _userManager.User.OrganizeId;
                    }

                    break;
                case PoxiaoKeyConst.CREATETIME:
                    formValue = DateTime.Now.ParseToUnixTime().ToString();
                    break;
                case PoxiaoKeyConst.MODIFYTIME:
                    formValue = "0";
                    break;
                case PoxiaoKeyConst.CURRPOSITION:
                    if (_userManager.User.PositionId.IsNotEmptyOrNull())
                    {
                        formValue = _userManager.User.PositionId;
                    }

                    break;
            }
        }
    }

    /// <summary>
    /// 删除定时器.
    /// </summary>
    /// <param name="flowTaskNodeEntityList">所有节点</param>
    private void DeleteTimerTaskNode(List<FlowTaskNodeEntity> flowTaskNodeEntityList)
    {
        foreach (var item in flowTaskNodeEntityList)
        {
            if ("timer".Equals(item.NodeType))
            {
                // 下一节点为Timer类型节点的节点集合
                var taskNodeList = flowTaskNodeEntityList.FindAll(x => x.NodeNext.Contains(item.NodeCode));

                // Timer类型节点的下节点集合
                var nextTaskNodeList = flowTaskNodeEntityList.FindAll(x => item.NodeNext.Contains(x.NodeCode));

                // 保存定时器节点的上节点编码到属性中
                var timerProperties = item.NodePropertyJson.ToObject<TimerProperties>();
                timerProperties.upNodeCode = string.Join(",", taskNodeList.Select(x => x.NodeCode).ToArray());
                item.NodePropertyJson = timerProperties.ToJsonString();

                // 上节点替换NodeNext
                foreach (var taskNode in taskNodeList)
                {
                    var flowTaskNodeEntity = flowTaskNodeEntityList.Where(x => x.NodeCode == taskNode.NodeCode).FirstOrDefault();
                    flowTaskNodeEntity.NodeNext = item.NodeNext;
                }

                // 下节点添加定时器属性
                nextTaskNodeList.ForEach(nextNode =>
                {
                    var flowTaskNodeEntity = flowTaskNodeEntityList.Where(x => x.NodeCode == nextNode.NodeCode).FirstOrDefault();
                    if (FlowTaskNodeTypeEnum.approver.ParseToString().Equals(flowTaskNodeEntity.NodeType))
                    {
                        var properties = flowTaskNodeEntity.NodePropertyJson.ToObject<ApproversProperties>();
                        properties.timerList.Add(item.NodePropertyJson.ToObject<TimerProperties>());
                        flowTaskNodeEntity.NodePropertyJson = JsonHelper.ToJsonString(properties);
                    }
                });
            }
        }

        flowTaskNodeEntityList.RemoveAll(x => FlowTaskNodeTypeEnum.timer.ParseToString().Equals(x.NodeType));
        UpdateNodeSort(flowTaskNodeEntityList);
    }

    /// <summary>
    /// 处理并保存节点.
    /// </summary>
    /// <param name="entitys">节点list.</param>
    public void UpdateNodeSort(List<FlowTaskNodeEntity> entitys)
    {
        var startNodes = entitys.FindAll(x => x.NodeType.Equals("start"));
        if (startNodes.Count > 0)
        {
            var startNode = startNodes[0].NodeCode;
            long num = 0L;
            long maxNum = 0L;
            var max = new List<long>();
            var _treeList = new List<FlowTaskNodeEntity>();
            NodeList(entitys, startNode, _treeList, num, max);
            max.Sort();
            if (max.Count > 0)
            {
                maxNum = max[max.Count - 1];
            }
            var nodeNext = "end";
            foreach (var item in entitys)
            {
                var type = item.NodeType;
                var node = _treeList.Find(x => x.NodeCode.Equals(item.NodeCode));
                if (item.NodeNext.IsEmpty())
                {
                    item.NodeNext = nodeNext;
                }
                if (node.IsNotEmptyOrNull())
                {
                    item.SortCode = node.SortCode;
                    item.State = "0";
                    if (item.NodeNext.IsEmpty())
                    {
                        item.NodeNext = nodeNext;
                    }
                }
            }
            entitys.Add(new FlowTaskNodeEntity()
            {
                Id = SnowflakeIdHelper.NextId(),
                NodeCode = nodeNext,
                NodeName = "结束",
                Completion = 0,
                CreatorTime = DateTime.Now,
                SortCode = maxNum + 1,
                TaskId = _treeList[0].TaskId,
                NodePropertyJson = startNodes[0].NodePropertyJson,
                NodeType = "endround",
                State = "0"
            });
        }
    }

    /// <summary>
    /// 递归获取经过的节点.
    /// </summary>
    /// <param name="dataAll"></param>
    /// <param name="nodeCode"></param>
    /// <param name="_treeList"></param>
    /// <param name="num"></param>
    /// <param name="max"></param>
    private void NodeList(List<FlowTaskNodeEntity> dataAll, string nodeCode, List<FlowTaskNodeEntity> _treeList, long num, List<long> max)
    {
        num++;
        max.Add(num);
        foreach (var item in dataAll)
        {
            if (item.NodeCode.Contains(nodeCode))
            {
                item.SortCode = num;
                item.State = "0";
                _treeList.Add(item);
                foreach (var nodeNext in item.NodeNext.Split(","))
                {
                    long nums = _treeList.FindAll(x => x.NodeCode.Equals(nodeNext)).Count;
                    if (nodeNext.IsNotEmptyOrNull() && nums == 0)
                    {
                        NodeList(dataAll, nodeNext, _treeList, num, max);
                    }
                }
            }
        }
    }
}
