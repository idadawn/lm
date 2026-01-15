import { defHttp } from '/@/utils/http/axios';

enum Api {
  Prefix = '/api/extend/Schedule',
}

// 获取日程安排列表
export function getSchedule(data) {
  return defHttp.get({ url: Api.Prefix, data });
}
// 新建日程安排
export function createSchedule(data) {
  return defHttp.post({ url: Api.Prefix, data });
}
// 删除日程安排
export function delSchedule(id) {
  return defHttp.delete({ url: Api.Prefix + `/${id}` });
}
// 获取日程安排信息
export function getInfo(id) {
  return defHttp.get({ url: Api.Prefix + `/${id}` });
}
// 更新日程安排
export function updateSchedule(data) {
  return defHttp.put({ url: Api.Prefix + `/${data.id}`, data });
}
