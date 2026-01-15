import { defHttp } from '/@/utils/http/axios';

enum Api {
  Prefix = '/api/message/MessageTemplateConfig',
}

// 获取消息模板列表
export function getMsgTemplateList(data) {
  return defHttp.get({ url: Api.Prefix, data });
}
// 新建消息模板
export function create(data) {
  return defHttp.post({ url: Api.Prefix, data });
}
// 修改消息模板
export function update(data) {
  return defHttp.put({ url: Api.Prefix + '/' + data.id, data });
}
// 获取消息模板
export function getInfo(id) {
  return defHttp.get({ url: Api.Prefix + '/' + id });
}
// 删除消息模板
export function delMsgTemplate(id) {
  return defHttp.delete({ url: Api.Prefix + '/' + id });
}
// 复制消息模板
export function copy(id) {
  return defHttp.post({ url: Api.Prefix + `/copy/${id}` });
}
// 测试发送配置
export function testMsgTemplate(data) {
  return defHttp.post({ url: Api.Prefix + `/testSendMail`, data });
}
// 数据类型具体对应数据：1：消息类型，2：渠道，3：webhook类型，4：消息来源
export function getMsgTypeList(type) {
  return defHttp.get({ url: `/api/message/MessageDataType/getTypeList/${type}` });
}
