const defaultStep = [
  { id: '0', fullName: '流程发起' },
  { id: '1', fullName: '上级审批节点' },
  { id: '2', fullName: '自选审批节点' },
];
const typeOptions = [
  { fullName: '指定成员', id: 6 },
  { fullName: '发起者本人', id: 3 },
  { fullName: '发起者主管', id: 1 },
  { fullName: '部门主管', id: 2 },
  { fullName: '表单变量', id: 4 },
  { fullName: '流程环节', id: 5 },
  { fullName: '接口服务', id: 9 },
  { fullName: '候选人员', id: 7 },
];
const overTimeOptions = [
  { id: 0, fullName: '接收时间' },
  { id: 1, fullName: '发起时间' },
  { id: 2, fullName: '表单变量' },
];
const overTimeMsgOptions = [
  { id: 1, fullName: '自定义' },
  { id: 0, fullName: '关闭' },
];
const nodeOverTimeMsgOptions = [{ id: 2, fullName: '同步发起配置' }, ...overTimeMsgOptions];
const noticeOptions = [{ id: 3, fullName: '默认' }, ...overTimeMsgOptions];
const nodeNoticeOptions = [{ id: 2, fullName: '同步发起配置' }, ...noticeOptions];
const systemFieldOptions = [
  { id: '@flowId', fullName: '流程ID' },
  { id: '@taskId', fullName: '任务ID' },
  { id: '@taskNodeId', fullName: '节点ID' },
  { id: '@flowFullName', fullName: '流程名称' },
  { id: '@taskFullName', fullName: '任务标题' },
  { id: '@launchUserId', fullName: '发起用户ID' },
  { id: '@launchUserName', fullName: '发起用户名' },
  { id: '@flowOperatorUserId', fullName: '当前操作用户ID' },
  { id: '@flowOperatorUserName', fullName: '当前操作用户名' },
];

export { defaultStep, typeOptions, overTimeOptions, overTimeMsgOptions, nodeOverTimeMsgOptions, noticeOptions, nodeNoticeOptions, systemFieldOptions };
