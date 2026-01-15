using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Filter;
using Poxiao.Infrastructure.Manager;
using Poxiao.Infrastructure.Models.User;
using Poxiao.Infrastructure.Models.WorkFlow;
using Poxiao.Infrastructure.Security;
using Poxiao.FriendlyException;
using Poxiao.RemoteRequest.Extensions;
using Poxiao.Systems.Entitys.Dto.User;
using Poxiao.Systems.Entitys.Permission;
using Poxiao.Systems.Interfaces.Permission;
using Poxiao.UnifyResult;
using Poxiao.WorkFlow.Entitys.Entity;
using Poxiao.WorkFlow.Entitys.Enum;
using Poxiao.WorkFlow.Entitys.Model;
using Poxiao.WorkFlow.Entitys.Model.Properties;
using Poxiao.WorkFlow.Interfaces.Repository;
using Mapster;
using Newtonsoft.Json.Linq;
using SqlSugar;

namespace Poxiao.WorkFlow.Manager;

public class FlowTaskUserUtil
{
    private readonly IFlowTaskRepository _flowTaskRepository;
    private readonly IUsersService _usersService;
    private readonly IOrganizeService _organizeService;
    private readonly IDepartmentService _departmentService;
    private readonly IUserRelationService _userRelationService;
    private readonly IUserManager _userManager;
    private readonly ICacheManager _cacheManager;

    public FlowTaskUserUtil(IFlowTaskRepository flowTaskRepository, IUsersService usersService, IOrganizeService organizeService, IDepartmentService departmentService, IUserRelationService userRelationService, IUserManager userManager, ICacheManager cacheManager)
    {
        _flowTaskRepository = flowTaskRepository;
        _usersService = usersService;
        _organizeService = organizeService;
        _departmentService = departmentService;
        _userRelationService = userRelationService;
        _userManager = userManager;
        _cacheManager = cacheManager;
    }

    /// <summary>
    /// 获取节点审批人员id.
    /// </summary>
    /// <param name="flowTaskParamter">当前任务参数.</param>
    /// <param name="approversProperties">节点属性.</param>
    /// <param name="flowTaskNodeEntity">节点实体.</param>
    /// <returns></returns>
    public async Task<List<string>> GetFlowUserId(FlowTaskParamter flowTaskParamter, ApproversProperties approversProperties, FlowTaskNodeEntity flowTaskNodeEntity)
    {
        var userIdList = new List<string>();
        // 获取全部用户id
        var userList1 = await _usersService.GetUserListByExp(x => x.DeleteMark == null && x.EnabledMark == 1, u => new UserEntity() { Id = u.Id });
        // 发起者本人.
        var userEntity = _usersService.GetInfoByUserId(flowTaskParamter.flowTaskEntity.CreatorUserId);
        switch (approversProperties.assigneeType)
        {
            // 发起者主管
            case (int)FlowTaskOperatorEnum.LaunchCharge:
                var crDirector = await GetManagerByLevel(userEntity.ManagerId, approversProperties.managerLevel);
                if (crDirector.IsNotEmptyOrNull())
                    userIdList.Add(crDirector);
                break;

            // 发起者本人
            case (int)FlowTaskOperatorEnum.InitiatorMe:
                userIdList.Add(userEntity.Id);
                break;

            // 部门主管
            case (int)FlowTaskOperatorEnum.DepartmentCharge:
                var organizeEntity = await _organizeService.GetInfoById(userEntity.OrganizeId);
                if (organizeEntity.IsNotEmptyOrNull() && organizeEntity.OrganizeIdTree.IsNotEmptyOrNull())
                {
                    var orgTree = organizeEntity.OrganizeIdTree.Split(",").Reverse().ToList();
                    if (orgTree.Count >= approversProperties.departmentLevel)
                    {
                        var orgId = orgTree[approversProperties.departmentLevel - 1];
                        var organize = await _organizeService.GetInfoById(orgId);
                        if (organize.IsNotEmptyOrNull() && organize.ManagerId.IsNotEmptyOrNull())
                        {
                            userIdList.Add(organize.ManagerId);
                        }
                    }
                }
                break;

            // 表单变量
            case (int)FlowTaskOperatorEnum.VariableApprover:
                var jd = flowTaskParamter.formData.ToObject<JObject>();
                var fieldValueList = new List<string>();
                var formField = approversProperties.formField;
                if (approversProperties.assignList.Any() && flowTaskParamter.flowTaskNodeEntity.IsNotEmptyOrNull())
                {
                    var ruleList = approversProperties.assignList.Find(x => x.nodeId == flowTaskParamter.flowTaskNodeEntity.NodeCode)?.ruleList;
                    if (ruleList.IsNotEmptyOrNull() && ruleList.Any(x => x.childField == approversProperties.formField))
                    {
                        formField = ruleList.Find(x => x.childField == approversProperties.formField)?.parentField;
                    }
                }
                if (jd.ContainsKey(formField))
                {
                    if (jd[formField] is JArray)
                    {
                        fieldValueList = jd[formField].ToObject<List<string>>();
                    }
                    else
                    {
                        if (jd[formField].ToString().IsNotEmptyOrNull())
                        {
                            fieldValueList = jd[formField].ToString().Split(",").ToList();
                        }
                    }
                }
                userIdList = _userRelationService.GetUserId(fieldValueList, string.Empty);
                break;

            // 环节(提交时下个节点是环节就跳过，审批则看环节节点是否是当前节点的上级)
            case (int)FlowTaskOperatorEnum.LinkApprover:
                if (flowTaskParamter.flowTaskNodeEntityList.Any(x => x.NodeCode.Equals(approversProperties.nodeId)))
                {
                    // 环节节点所有经办人(过滤掉加签人)
                    userIdList = (await _flowTaskRepository.GetTaskOperatorRecordList(x =>
                        x.TaskId == flowTaskNodeEntity.TaskId && !SqlFunc.IsNullOrEmpty(x.NodeCode)
                        && x.NodeCode.Equals(approversProperties.nodeId) && x.HandleStatus == 1 && x.Status == 0))
                        .Where(x => HasFreeApprover(x.TaskOperatorId).Result).Where(x => x.HandleId.IsNotEmptyOrNull()).Select(x => x.HandleId).Distinct().ToList();
                }
                break;

            // 接口(接口结构为{"code":200,"data":{"handleId":"admin"},"msg":""})
            case (int)FlowTaskOperatorEnum.ServiceApprover:
                try
                {
                    var Token = _userManager.ToKen.IsNotEmptyOrNull() ? _userManager.ToKen : _cacheManager.Get<List<UserOnlineModel>>(string.Format("{0}:{1}", CommonConst.CACHEKEYONLINEUSER, _userManager.TenantId)).Find(x => x.userId == _userManager.UserId).token;
                    var data = await approversProperties.getUserUrl.SetHeaders(new { Authorization = Token }).SetBody(flowTaskParamter.formData).PostAsStringAsync();
                    var result = data.ToObject<RESTfulResult<object>>();
                    if (result.IsNotEmptyOrNull())
                    {
                        var resultJobj = result.data.ToObject<JObject>();
                        if (result.code == 200)
                        {
                            var handleId = resultJobj["handleId"].ToString();
                            var handleIdList = handleId.Split(",").ToList();
                            var userList2 = await _usersService.GetUserListByExp(x => x.DeleteMark == null, u => new UserEntity() { Id = u.Id });

                            // 利用list交集方法过滤非用户数据
                            userIdList = userList2.Select(x => x.Id).Intersect(handleIdList).ToList();
                        }
                    }
                }
                catch (AppFriendlyException ex)
                {
                    break;
                }

                break;

            // 候选人
            case (int)FlowTaskOperatorEnum.CandidateApprover:
                userIdList = _flowTaskRepository.GetFlowCandidates(flowTaskNodeEntity.Id);
                break;
            default:
                userIdList = (await GetUserDefined(approversProperties));
                userIdList = await GetExtraRuleUsers(userIdList, approversProperties.extraRule, flowTaskNodeEntity.TaskId);
                break;
        }
        userIdList = userList1.Select(x => x.Id).Intersect(userIdList).ToList();// 过滤掉作废人员和非用户人员
        if (userIdList.Count == 0)
        {
            userIdList = _flowTaskRepository.GetFlowCandidates(flowTaskNodeEntity.Id);
        }
        return userIdList.Distinct().ToList();

    }

    /// <summary>
    /// 附加条件过滤.
    /// </summary>
    /// <param name="userList">过滤用户.</param>
    /// <param name="extraRule">过滤规则.</param>
    /// <param name="taskId">任务id.</param>
    /// <returns></returns>
    private async Task<List<string>> GetExtraRuleUsers(List<string> userList, string extraRule, string taskId)
    {
        var flowUserEntity = _flowTaskRepository.GetFlowUserEntity(taskId);
        if (flowUserEntity.IsNullOrEmpty())
        {
            var subordinate = (await _usersService.GetUserListByExp(u => u.EnabledMark == 1 && u.DeleteMark == null && u.ManagerId == _userManager.UserId)).Select(u => u.Id).ToList().ToJsonString();
            flowUserEntity = new FlowUserEntity()
            {
                OrganizeId = _userManager.User.OrganizeId,
                PositionId = _userManager.User.PositionId,
                ManagerId = _userManager.User.ManagerId,
                Subordinate = subordinate
            };
        }
        switch (extraRule)
        {
            case "2":
                userList = _userRelationService.GetUserId("Organize", flowUserEntity.OrganizeId).Intersect(userList).ToList();
                break;
            case "3":
                userList = _userRelationService.GetUserId("Position", flowUserEntity.PositionId).Intersect(userList).ToList();
                break;
            case "4":
                userList = new List<string> { flowUserEntity.ManagerId }.Intersect(userList).ToList();
                break;
            case "5":
                userList = flowUserEntity.Subordinate.ToObject<List<string>>().Intersect(userList).ToList();
                break;
            case "6":
                // 直属公司id
                var companyId = _departmentService.GetCompanyId(flowUserEntity.OrganizeId);
                var objIdList = (await _departmentService.GetCompanyAllDep(companyId)).Select(x => x.Id).ToList();
                objIdList.Add(companyId);
                userList = _userRelationService.GetUserId(objIdList, "Organize").Intersect(userList).ToList();
                break;
        }
        return userList;
    }

    /// <summary>
    /// 根据类型获取审批人.
    /// </summary>
    /// <param name="flowTaskParamter">任务参数.</param>
    /// <param name="nextFlowTaskNodeEntity">下个审批节点数据.</param>
    /// <param name="type">操作标识（0：提交，1：审批，3:变更）.</param>
    /// <param name="isShuntNodeCompletion">是否分流合流已完成.</param>
    /// <returns></returns>
    public async Task AddFlowTaskOperatorEntityByAssigneeType(FlowTaskParamter flowTaskParamter, FlowTaskNodeEntity nextFlowTaskNodeEntity, int type = 1, bool isShuntNodeCompletion = true)
    {
        try
        {
            if (FlowTaskNodeTypeEnum.approver.ParseToString().Equals(nextFlowTaskNodeEntity.NodeType))
            {
                var approverPropertiers = nextFlowTaskNodeEntity.NodePropertyJson.ToObject<ApproversProperties>();
                var errorUserId = new List<string>();
                if (flowTaskParamter.errorRuleUserList.IsNotEmptyOrNull() && flowTaskParamter.errorRuleUserList.ContainsKey(nextFlowTaskNodeEntity.NodeCode))
                {
                    errorUserId = flowTaskParamter.errorRuleUserList[nextFlowTaskNodeEntity.NodeCode];
                }
                var startProperties = flowTaskParamter.startProperties;
                if (type == 3)
                {
                    startProperties.errorRule = "3";
                }
                var handleIds = await GetFlowUserId(flowTaskParamter, approverPropertiers, nextFlowTaskNodeEntity);
                if (handleIds.Count == 0 && isShuntNodeCompletion)
                {
                    switch (startProperties.errorRule)
                    {
                        case "1":
                            handleIds.Add(_userManager.GetAdminUserId());
                            break;
                        case "2":
                            if ((await _usersService.GetUserListByExp(x => startProperties.errorRuleUser.Contains(x.Id) && x.DeleteMark == null && x.EnabledMark == 1)).Any())
                            {
                                handleIds = startProperties.errorRuleUser;
                            }
                            else
                            {
                                handleIds.Add(_userManager.GetAdminUserId());
                            }
                            break;
                        case "3":
                            if (errorUserId.IsNotEmptyOrNull() && errorUserId.Count > 0)
                            {
                                handleIds = errorUserId;
                            }
                            else
                            {
                                if (!flowTaskParamter.errorNodeList.Select(x => x.nodeId).Contains(nextFlowTaskNodeEntity.NodeCode))
                                {
                                    flowTaskParamter.errorNodeList.Add(new FlowTaskCandidateModel { nodeId = nextFlowTaskNodeEntity.NodeCode, nodeName = nextFlowTaskNodeEntity.NodeName });
                                }
                            }
                            break;
                        case "4":
                            // 异常节点下一节点是否存在候选人节点.
                            var falag = flowTaskParamter.flowTaskNodeEntityList.
                                Any(x => nextFlowTaskNodeEntity.NodeNext.Split(",").Contains(x.NodeCode)
                                && FlowTaskNodeTypeEnum.approver.ParseToString().Equals(x.NodeType)
                                && x.NodePropertyJson.ToObject<ApproversProperties>().assigneeType == 7);
                            if (falag)
                            {
                                handleIds.Add(_userManager.GetAdminUserId());
                            }
                            else
                            {
                                handleIds.Add("poxiao");
                            }
                            break;
                        case "5":
                            throw Oops.Oh(ErrorCode.WF0035);
                    }
                }
                var index = 0;
                var isAnyOperatorUser = !_flowTaskRepository.AnyTaskOperatorUser(x => x.TaskNodeId == nextFlowTaskNodeEntity.Id && x.State == "0");// 不存在依次审批插入.
                var OperatorUserList = new List<FlowTaskOperatorUserEntity>();
                foreach (var item in handleIds)
                {
                    if (item.IsNotEmptyOrNull())
                    {
                        if (approverPropertiers.counterSign == 2 && isAnyOperatorUser)
                        {
                            FlowTaskOperatorUserEntity flowTaskOperatorUserEntity = new FlowTaskOperatorUserEntity();
                            flowTaskOperatorUserEntity.Id = SnowflakeIdHelper.NextId();
                            flowTaskOperatorUserEntity.NodeCode = nextFlowTaskNodeEntity.NodeCode;
                            flowTaskOperatorUserEntity.NodeName = nextFlowTaskNodeEntity.NodeName;
                            flowTaskOperatorUserEntity.TaskNodeId = nextFlowTaskNodeEntity.Id;
                            flowTaskOperatorUserEntity.TaskId = nextFlowTaskNodeEntity.TaskId;
                            flowTaskOperatorUserEntity.CreatorTime = GetTimerDate(approverPropertiers, flowTaskParamter.flowTaskNodeEntity.NodeCode);
                            flowTaskOperatorUserEntity.Completion = 0;
                            flowTaskOperatorUserEntity.State = "0";
                            flowTaskOperatorUserEntity.Type = approverPropertiers.assigneeType.ToString();
                            flowTaskOperatorUserEntity.HandleId = item;
                            flowTaskOperatorUserEntity.SortCode = index++;
                            OperatorUserList.Add(flowTaskOperatorUserEntity);
                            if (index == 1)
                            {
                                flowTaskParamter.flowTaskOperatorEntityList.Add(OperatorUserList.FirstOrDefault().Adapt<FlowTaskOperatorEntity>());
                            }
                        }
                        else
                        {
                            FlowTaskOperatorEntity flowTaskOperatorEntity = new FlowTaskOperatorEntity();
                            flowTaskOperatorEntity.Id = SnowflakeIdHelper.NextId();
                            flowTaskOperatorEntity.NodeCode = nextFlowTaskNodeEntity.NodeCode;
                            flowTaskOperatorEntity.NodeName = nextFlowTaskNodeEntity.NodeName;
                            flowTaskOperatorEntity.TaskNodeId = nextFlowTaskNodeEntity.Id;
                            flowTaskOperatorEntity.TaskId = nextFlowTaskNodeEntity.TaskId;
                            flowTaskOperatorEntity.CreatorTime = GetTimerDate(approverPropertiers, flowTaskParamter.flowTaskNodeEntity.NodeCode);
                            flowTaskOperatorEntity.Completion = 0;
                            flowTaskOperatorEntity.State = "0";
                            flowTaskOperatorEntity.Type = approverPropertiers.assigneeType.ToString();
                            flowTaskOperatorEntity.HandleId = item;
                            flowTaskOperatorEntity.SortCode = index++;
                            flowTaskParamter.flowTaskOperatorEntityList.Add(flowTaskOperatorEntity);
                        }
                    }
                }
                await _flowTaskRepository.CreateTaskOperatorUser(OperatorUserList);
            }
        }
        catch (AppFriendlyException ex)
        {
            throw Oops.Oh(ex.ErrorCode);
        }
    }

    /// <summary>
    /// 获取抄送人.
    /// </summary>
    /// <param name="flowTaskParamter">任务参数.</param>
    /// <param name="handleStatus">审批类型（0：拒绝，1：同意）.</param>
    public async Task GetflowTaskCirculateEntityList(FlowTaskParamter flowTaskParamter, int handleStatus)
    {
        var circulateUserList = flowTaskParamter.copyIds.IsNotEmptyOrNull() ? flowTaskParamter.copyIds.Split(",").ToList() : new List<string>();
        #region 抄送人
        if (handleStatus == 1)
        {
            var userList = await GetUserDefined(flowTaskParamter.approversProperties, 1);
            userList = await GetExtraRuleUsers(userList, flowTaskParamter.approversProperties.extraCopyRule, flowTaskParamter.flowTaskOperatorEntity.TaskId);
            circulateUserList = circulateUserList.Union(userList).ToList();
            if (flowTaskParamter.approversProperties.isInitiatorCopy)
            {
                circulateUserList.Add(flowTaskParamter.flowTaskEntity.CreatorUserId);
                //if (flowTaskParamter.flowTaskEntity.DelegateUser.IsNotEmptyOrNull())
                //{
                //    circulateUserList.Add(flowTaskParamter.flowTaskEntity.DelegateUser);
                //}
            }
        }
        foreach (var item in circulateUserList.Distinct())
        {
            flowTaskParamter.flowTaskCirculateEntityList.Add(new FlowTaskCirculateEntity()
            {
                Id = SnowflakeIdHelper.NextId(),
                ObjectType = flowTaskParamter.flowTaskOperatorEntity.Type,
                ObjectId = item,
                NodeCode = flowTaskParamter.flowTaskOperatorEntity.NodeCode,
                NodeName = flowTaskParamter.flowTaskOperatorEntity.NodeName,
                TaskNodeId = flowTaskParamter.flowTaskOperatorEntity.TaskNodeId,
                TaskId = flowTaskParamter.flowTaskOperatorEntity.TaskId,
                CreatorTime = DateTime.Now,
            });
        }
        #endregion
    }

    /// <summary>
    /// 获取自定义人员名称.
    /// </summary>
    /// <param name="approversProperties">节点属性.</param>
    /// <param name="userNameList">用户名称容器.</param>
    /// <param name="userIdList">用户id容器.</param>
    /// <returns></returns>
    public async Task GetUserNameDefined(ApproversProperties approversProperties, List<string> userNameList, List<string> userIdList = null)
    {
        if (userIdList == null)
        {
            userIdList = (await GetUserDefined(approversProperties)).Distinct().ToList();
        }
        foreach (var item in userIdList)
        {
            var name = await _usersService.GetUserName(item);
            if (name.IsNotEmptyOrNull())
                userNameList.Add(name);
        }
    }

    /// <summary>
    /// 获取候选人节点信息.
    /// </summary>
    /// <param name="flowTaskCandidateModels">返回参数.</param>
    /// <param name="nextNodeEntities">下一节点集合.</param>
    /// <param name="nodeEntities">所有节点.</param>
    /// <returns></returns>
    public async Task GetCandidates(List<FlowTaskCandidateModel> flowTaskCandidateModels, List<FlowTaskNodeEntity> nextNodeEntities, List<FlowTaskNodeEntity> nodeEntities)
    {
        foreach (var item in nextNodeEntities)
        {
            ApproversProperties approverPropertiers = null;
            var isSubFlow = false;//是否子流程节点.
            if (FlowTaskNodeTypeEnum.approver.ParseToString().Equals(item.NodeType))
                approverPropertiers = item.NodePropertyJson.ToObject<ApproversProperties>();
            if (FlowTaskNodeTypeEnum.subFlow.ParseToString().Equals(item.NodeType))
            {
                approverPropertiers = item.NodePropertyJson.ToObject<ChildTaskProperties>().Adapt<ApproversProperties>();
                isSubFlow = true;
            }

            if (approverPropertiers.IsNotEmptyOrNull())
            {
                if (approverPropertiers.assigneeType == 7 || approverPropertiers.isBranchFlow)
                {
                    var candidateItem = new FlowTaskCandidateModel();
                    candidateItem.nodeId = item.NodeCode;
                    candidateItem.nodeName = item.NodeName;
                    candidateItem.isBranchFlow = approverPropertiers.isBranchFlow;
                    candidateItem.isCandidates = approverPropertiers.assigneeType == 7;
                    var objIds = approverPropertiers.approverRole
                        .Union(approverPropertiers.approverPos)
                        .Union(approverPropertiers.approverOrg)
                        .Union(approverPropertiers.approverGroup).ToList();
                    var flag = false;//是否有数据
                    var input = new UserConditionInput()
                    {
                        departIds = objIds,
                        userIds = approverPropertiers.approvers,
                        pagination = new PageInputBase()
                    };
                    _userRelationService.GetUserPage(input, ref flag);
                    candidateItem.hasCandidates = flag;
                    flowTaskCandidateModels.Add(candidateItem);
                }
            }
            // 子流程节点则要看下一节点是否存在候选人或选择分支
            if (isSubFlow)
            {
                var subFlowNextNodes = nodeEntities.FindAll(m => item.NodeNext.Contains(m.NodeCode));
                await GetCandidates(flowTaskCandidateModels, subFlowNextNodes, nodeEntities);
            }
        }
    }

    /// <summary>
    /// 候选人员列表.
    /// </summary>
    /// <param name="nextNodeEntity">下一节点.</param>
    /// <param name="flowHandleModel">审批参数.</param>
    /// <param name="hasCandidates">是否存在候选人.</param>
    /// <returns></returns>
    public dynamic GetCandidateItems(FlowTaskNodeEntity nextNodeEntity, FlowHandleModel flowHandleModel, bool hasCandidates = true)
    {
        var approverPropertiers = nextNodeEntity.NodePropertyJson.ToObject<ApproversProperties>();
        if (FlowTaskNodeTypeEnum.subFlow.ParseToString().Equals(nextNodeEntity.NodeType))
        {
            approverPropertiers = nextNodeEntity.NodePropertyJson.ToObject<ChildTaskProperties>().Adapt<ApproversProperties>();
        }
        var objIds = approverPropertiers.approverRole
                        .Union(approverPropertiers.approverPos)
                        .Union(approverPropertiers.approverOrg)
                        .Union(approverPropertiers.approverGroup).ToList();
        var input = new UserConditionInput()
        {
            departIds = objIds,
            userIds = approverPropertiers.approvers,
            pagination = flowHandleModel
        };
        return _userRelationService.GetUserPage(input, ref hasCandidates);
    }

    /// <summary>
    /// 获取子流程下异常节点信息.
    /// </summary>
    /// <param name="flowTaskParamter">任务参数.</param>
    /// <param name="nextNodeEntities">下一节点集合.</param>
    /// <returns></returns>
    public async Task GetErrorNode(FlowTaskParamter flowTaskParamter, List<FlowTaskNodeEntity> nextNodeEntities)
    {
        try
        {
            foreach (var item in nextNodeEntities)
            {
                if (FlowTaskNodeTypeEnum.approver.ParseToString().Equals(item.NodeType))
                {
                    var approverPropertiers = item.NodePropertyJson.ToObject<ApproversProperties>();
                    var list = await GetFlowUserId(flowTaskParamter, approverPropertiers, item);
                    if (list.Count == 0)
                    {
                        if (flowTaskParamter.startProperties.errorRule == "3" && !flowTaskParamter.errorNodeList.Select(x => x.nodeId).Contains(item.NodeCode))
                        {
                            var candidateItem = new FlowTaskCandidateModel();
                            candidateItem.nodeId = item.NodeCode;
                            candidateItem.nodeName = item.NodeName;
                            flowTaskParamter.errorNodeList.Add(candidateItem);
                        }
                        if (flowTaskParamter.startProperties.errorRule == "5")
                            throw Oops.Oh(ErrorCode.WF0035);
                    }
                }
                else if (FlowTaskNodeTypeEnum.subFlow.ParseToString().Equals(item.NodeType))
                {
                    var subFlowNextNodes = flowTaskParamter.flowTaskNodeEntityList.FindAll(m => item.NodeNext.Contains(m.NodeCode));
                    await GetErrorNode(flowTaskParamter, subFlowNextNodes);
                }
            }
        }
        catch (AppFriendlyException ex)
        {
            throw Oops.Oh(ex.ErrorCode);
        }
    }

    /// <summary>
    /// 获取子流程发起人.
    /// </summary>
    /// <param name="childTaskProperties">子流程属性.</param>
    /// <param name="flowTaskParamter">当前任务参数.</param>
    /// <param name="flowTaskNodeEntity">子流程节点.</param>
    /// <returns></returns>
    public async Task<List<string>> GetSubFlowCrUser(ChildTaskProperties childTaskProperties, FlowTaskParamter flowTaskParamter, FlowTaskNodeEntity flowTaskNodeEntity)
    {
        var approverPropertiers = childTaskProperties.Adapt<ApproversProperties>();
        var childTaskCrUserList = await GetFlowUserId(flowTaskParamter, approverPropertiers, flowTaskNodeEntity);
        if (childTaskCrUserList.Count == 0)
        {
            switch (childTaskProperties.errorRule)
            {
                case "2":
                    if ((await _usersService.GetUserListByExp(x => childTaskProperties.errorRuleUser.Contains(x.Id) && x.DeleteMark == null && x.EnabledMark == 1)).Any())
                    {
                        childTaskCrUserList = childTaskProperties.errorRuleUser;
                    }
                    else
                    {
                        childTaskCrUserList.Add(_userManager.GetAdminUserId());
                    }
                    break;
                case "6":
                    childTaskCrUserList.Add(flowTaskParamter.flowTaskEntity.CreatorUserId);
                    break;
                default:
                    childTaskCrUserList.Add(_userManager.GetAdminUserId());
                    break;
            }
        }
        return childTaskCrUserList;
    }

    /// <summary>
    /// 获取审批人名称.
    /// </summary>
    /// <param name="flowTaskNodeModel">当前节点.</param>
    /// <param name="flowTaskParamter">任务参数.</param>
    /// <param name="flowJsonModel">流程实体.</param>
    /// <returns></returns>
    public async Task<string> GetApproverUserName(FlowTaskNodeModel flowTaskNodeModel, FlowTaskParamter flowTaskParamter, FlowJsonModel flowJsonModel)
    {
        var userNameList = new List<string>();
        var userName = await _usersService.GetUserName(flowTaskParamter.flowTaskEntity.CreatorUserId);
        if (flowTaskNodeModel.nodeType.Equals(FlowTaskNodeTypeEnum.start.ParseToString()))
        {
            if (flowJsonModel.visibleType == 0)
            {
                userNameList.Add(userName);
            }
            else
            {
                await GetUserNameDefined(flowTaskParamter.startProperties.Adapt<ApproversProperties>(), userNameList);
            }
        }
        else if (flowTaskNodeModel.nodeType.Equals(FlowTaskNodeTypeEnum.subFlow.ParseToString()))
        {
            var subFlowProperties = flowTaskNodeModel.nodePropertyJson.ToObject<ChildTaskProperties>();
            var userIdList = (await _flowTaskRepository.GetTaskList(x => subFlowProperties.childTaskId.Contains(x.Id))).Select(x => x.CreatorUserId).ToList();
            var approverProperties = subFlowProperties.Adapt<ApproversProperties>();
            if (userIdList.Count == 0)
            {
                userIdList = await GetFlowUserId(flowTaskParamter, approverProperties, flowTaskNodeModel.Adapt<FlowTaskNodeEntity>());
            }
            await GetUserNameDefined(approverProperties, userNameList, userIdList);
        }
        else
        {
            var approverProperties = flowTaskNodeModel.nodePropertyJson.ToObject<ApproversProperties>();
            var userIdList = (await _flowTaskRepository.GetTaskOperatorList(x => x.TaskNodeId == flowTaskNodeModel.id && SqlFunc.IsNullOrEmpty(x.ParentId) && !x.State.Equals("-1"))).Select(x => x.HandleId).Distinct().ToList();
            if (approverProperties.counterSign == 2)
            {
                var OperatorUserIdList = (await _flowTaskRepository.GetTaskOperatorUserList(x => x.TaskId == flowTaskNodeModel.taskId && x.TaskNodeId == flowTaskNodeModel.id && !x.State.Equals("-1"))).Select(x => x.HandleId).ToList();
                if (OperatorUserIdList.Any())
                {
                    userIdList = OperatorUserIdList;
                }
            }
            if (!userIdList.Any())
            {
                userIdList = await GetFlowUserId(flowTaskParamter, approverProperties, flowTaskNodeModel.Adapt<FlowTaskNodeEntity>());
            }
            await GetUserNameDefined(approverProperties, userNameList, userIdList);
        }
        return string.Join(",", userNameList.Distinct());
    }

    /// <summary>
    /// 获取级别主管.
    /// </summary>
    /// <param name="managerId">主管id.</param>
    /// <param name="level">级别.</param>
    /// <returns></returns>
    public async Task<string> GetManagerByLevel(string managerId, int level)
    {
        --level;
        if (level == 0)
        {
            return managerId;
        }
        else
        {
            var manager = await _usersService.GetInfoByUserIdAsync(managerId);
            return manager.IsNullOrEmpty() ? string.Empty : await GetManagerByLevel(manager.ManagerId, level);
        }
    }

    /// <summary>
    /// 判断经办记录人是否加签且加签是否完成.
    /// </summary>
    /// <param name="id">经办id.</param>
    /// <returns></returns>
    public async Task<bool> HasFreeApprover(string id)
    {
        var entityList = await GetOperator(id, new List<FlowTaskOperatorEntity>());
        if (entityList.Count == 0)
        {
            return true;
        }
        else
        {
            return !entityList.Any(x => x.HandleStatus.IsEmpty() || x.HandleStatus == 0);
        }
    }

    /// <summary>
    /// 递归获取加签人.
    /// </summary>
    /// <param name="id">经办id.</param>
    /// <param name="flowTaskOperatorEntities">所有经办.</param>
    /// <returns></returns>
    public async Task<List<FlowTaskOperatorEntity>> GetOperator(string id, List<FlowTaskOperatorEntity> flowTaskOperatorEntities)
    {
        var childEntity = await _flowTaskRepository.GetTaskOperatorInfo(x => x.ParentId == id && !x.State.Equals("-1"));
        if (childEntity.IsNotEmptyOrNull())
        {
            childEntity.State = "-1";
            flowTaskOperatorEntities.Add(childEntity);
            return await GetOperator(childEntity.Id, flowTaskOperatorEntities);
        }
        else
        {
            return flowTaskOperatorEntities;
        }
    }

    /// <summary>
    /// 递归获取加签人.
    /// </summary>
    /// <param name="id">经办id.</param>
    /// <param name="flowTaskOperatorEntities">所有经办.</param>
    /// <returns></returns>
    public async Task<List<FlowTaskOperatorEntity>> GetOperatorNew(string id, List<FlowTaskOperatorEntity> flowTaskOperatorEntities)
    {
        var childEntity = await _flowTaskRepository.GetTaskOperatorInfo(x => x.HandleType == id && !x.State.Equals("-1"));
        if (childEntity.IsNotEmptyOrNull())
        {
            childEntity.State = "-1";
            flowTaskOperatorEntities.Add(childEntity);
            return await GetOperatorNew(childEntity.Id, flowTaskOperatorEntities);
        }
        else
        {
            return flowTaskOperatorEntities;
        }
    }

    /// <summary>
    /// 获取自定义人员.
    /// </summary>
    /// <param name="approversProperties">节点属性.</param>
    /// <param name="userType">0：审批人员，1：抄送人员.</param>
    /// <returns></returns>
    public async Task<List<string>> GetUserDefined(ApproversProperties approversProperties, int userType = 0)
    {
        var userIdList = new List<string>();
        var objIdList = new List<string>();
        if (userType == 0)
        {
            userIdList = approversProperties.approvers;
            objIdList = approversProperties.approverRole
            .Union(approversProperties.approverPos)
            .Union(approversProperties.approverOrg)
            .Union(approversProperties.approverGroup).ToList();
        }
        else
        {
            userIdList = approversProperties.circulateUser;
            objIdList = approversProperties.circulateRole
            .Union(approversProperties.circulatePosition)
            .Union(approversProperties.circulateOrg)
            .Union(approversProperties.circulateGroup).ToList();
        }
        userIdList = userIdList.Union(_userRelationService.GetUserId(objIdList, string.Empty)).ToList();
        return userIdList;

    }

    /// <summary>
    /// 获取定时器节点定时结束时间.
    /// </summary>
    /// <param name="approverPropertiers">定时器节点属性.</param>
    /// <param name="nodeCode">定时器节点编码.</param>
    /// <returns></returns>
    public DateTime GetTimerDate(ApproversProperties approverPropertiers, string nodeCode)
    {
        var nowTime = DateTime.Now;
        if (approverPropertiers.timerList.Count > 0)
        {
            string upNodeStr = string.Join(",", approverPropertiers.timerList.Select(x => x.upNodeCode).ToArray());
            if (upNodeStr.Contains(nodeCode))
            {
                foreach (var item in approverPropertiers.timerList)
                {
                    var result = DateTime.Now.AddDays(item.day).AddHours(item.hour).AddMinutes(item.minute).AddSeconds(item.second);
                    if (result > nowTime)
                    {
                        nowTime = result;
                    }
                }
            }
        }
        return nowTime;
    }
}
