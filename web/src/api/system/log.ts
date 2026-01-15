import { defHttp } from '/@/utils/http/axios';

enum Api {
  Prefix = '/api/system/Log',
}

// 获取系统日志信息
export function getLogList(data) {
  return defHttp.get({ url: Api.Prefix + '/' + data.type, data });
}
// 删除或批量删除日志
export function delLog(data) {
  return defHttp.delete({ url: Api.Prefix, data });
}
// 一键清空
export function batchDelLog(type) {
  return defHttp.delete({ url: Api.Prefix + '/' + type });
}
