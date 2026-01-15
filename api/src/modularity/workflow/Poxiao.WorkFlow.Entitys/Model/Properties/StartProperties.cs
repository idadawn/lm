using Poxiao.DependencyInjection;
using Poxiao.WorkFlow.Entitys.Model.Conifg;

namespace Poxiao.WorkFlow.Entitys.Model.Properties;

[SuppressSniffer]
public class StartProperties
{
    /// <summary>
    /// 发起节点标题.
    /// </summary>
    public string? title { get; set; }

    /// <summary>
    /// 指定发起人（为空则是所有人）.
    /// </summary>
    public List<string>? initiator { get; set; } = new List<string>();

    /// <summary>
    /// 指定发起岗位（为空则是所有人）.
    /// </summary>
    public List<string>? initiatePos { get; set; } = new List<string>();

    /// <summary>
    /// 指定发起角色.
    /// </summary>
    public List<string>? initiateRole { get; set; } = new List<string>();

    /// <summary>
    /// 指定发起部门（为空则是所有人）.
    /// </summary>
    public List<string>? initiateOrg { get; set; } = new List<string>();

    /// <summary>
    /// 指定发起分组.
    /// </summary>
    public List<string>? initiateGroup { get; set; } = new List<string>();

    /// <summary>
    /// 指定抄送岗位.
    /// </summary>
    public List<string> circulatePosition { get; set; } = new List<string>();

    /// <summary>
    /// 指定抄送人.
    /// </summary>
    public List<string> circulateUser { get; set; } = new List<string>();

    /// <summary>
    /// 抄送角色.
    /// </summary>
    public List<string> circulateRole { get; set; } = new List<string>();

    /// <summary>
    /// 指定抄送部门.
    /// </summary>
    public List<string> circulateOrg { get; set; } = new List<string>();

    /// <summary>
    /// 指定抄送分组.
    /// </summary>
    public List<string> circulateGroup { get; set; } = new List<string>();

    /// <summary>
    /// 表单权限.
    /// </summary>
    public List<object>? formOperates { get; set; }

    /// <summary>
    /// 打印id.
    /// </summary>
    public List<string> printId { get; set; } = new List<string>();

    /// <summary>
    /// 是否评论.
    /// </summary>
    public bool isComment { get; set; }

    /// <summary>
    /// 是否批量.
    /// </summary>
    public bool isBatchApproval { get; set; }

    /// <summary>
    /// 自定义抄送人.
    /// </summary>
    public bool isCustomCopy { get; set; }

    /// <summary>
    /// 是否有签名.
    /// </summary>
    public bool hasSign { get; set; }

    /// <summary>
    /// 是否有审批意见.
    /// </summary>
    public bool hasOpinion { get; set; } = true;

    /// <summary>
    /// 抄送附加条件,默认无附加条件.
    /// 1:无附加条件、2:同一部门、3:同一岗位、4:发起人上级、5:发起人下属、6:同一公司.
    /// </summary>
    public string extraCopyRule { get; set; } = "1";

    /// <summary>
    /// 异常处理规则
    /// 1:超级管理员处理、2:指定人员处理、3:上一节点审批人指定处理人、4:默认审批通过、5:无法提交.
    /// </summary>
    public string errorRule { get; set; } = "1";

    /// <summary>
    /// 异常处理人.
    /// </summary>
    public List<string>? errorRuleUser { get; set; } = new List<string>();

    /// <summary>
    /// 任务名类型 0：默认 1：自定义.
    /// </summary>
    public int titleType { get; set; } = 0;

    /// <summary>
    /// 任务名格式.
    /// </summary>
    public string titleContent { get; set; }

    /// <summary>
    /// 表单id.
    /// </summary>
    public string formId { get; set; }

    #region 按钮

    /// <summary>
    /// 撤回按钮.
    /// </summary>
    public string? revokeBtnText { get; set; } = "撤 回";

    /// <summary>
    /// 是否撤回.
    /// </summary>
    public bool hasRevokeBtn { get; set; } = true;

    /// <summary>
    /// 提交按钮.
    /// </summary>
    public string? submitBtnText { get; set; } = "提 交";

    /// <summary>
    /// 是否提交.
    /// </summary>
    public bool hasSubmitBtn { get; set; } = true;

    /// <summary>
    /// 保存按钮.
    /// </summary>
    public string? saveBtnText { get; set; } = "暂 存";

    /// <summary>
    /// 是否保存.
    /// </summary>
    public bool hasSaveBtn { get; set; } = true;

    /// <summary>
    /// 催办按钮.
    /// </summary>
    public string? pressBtnText { get; set; } = "催 办";

    /// <summary>
    /// 是否催办.
    /// </summary>
    public bool hasPressBtn { get; set; } = true;

    /// <summary>
    /// 打印按钮.
    /// </summary>
    public string? printBtnText { get; set; } = "打 印";

    /// <summary>
    /// 是否打印.
    /// </summary>
    public bool hasPrintBtn { get; set; } = true;
    #endregion

    #region 节点事件

    /// <summary>
    /// 流程发起事件.
    /// </summary>
    public FuncConfig? initFuncConfig { get; set; }

    /// <summary>
    /// 流程结束事件.
    /// </summary>
    public FuncConfig? endFuncConfig { get; set; }

    /// <summary>
    /// 流程撤回事件.
    /// </summary>
    public FuncConfig? flowRecallFuncConfig { get; set; }
    #endregion

    #region 消息

    /// <summary>
    /// 审核.
    /// </summary>
    public MsgConfig? waitMsgConfig { get; set; }

    /// <summary>
    /// 结束.
    /// </summary>
    public MsgConfig? endMsgConfig { get; set; }

    /// <summary>
    /// 同意.
    /// </summary>
    public MsgConfig? approveMsgConfig { get; set; }

    /// <summary>
    /// 拒绝.
    /// </summary>
    public MsgConfig? rejectMsgConfig { get; set; }

    /// <summary>
    /// 抄送.
    /// </summary>
    public MsgConfig? copyMsgConfig { get; set; }

    /// <summary>
    /// 超时.
    /// </summary>
    public MsgConfig? overTimeMsgConfig { get; set; }

    /// <summary>
    /// 提醒.
    /// </summary>
    public MsgConfig? noticeMsgConfig { get; set; }
    #endregion

    #region 超时

    /// <summary>
    /// 限时.
    /// </summary>
    public TimeOutConfig? timeLimitConfig { get; set; } = new TimeOutConfig();

    /// <summary>
    /// 超时.
    /// </summary>
    public TimeOutConfig? overTimeConfig { get; set; } = new TimeOutConfig();

    /// <summary>
    /// 提醒.
    /// </summary>
    public TimeOutConfig? noticeConfig { get; set; } = new TimeOutConfig();
    #endregion
}
