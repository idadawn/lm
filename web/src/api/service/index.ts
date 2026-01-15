import { defHttp } from '/@/utils/http/axios';

const server = '/collectServer';

// 获取服务类型接口
export function serviceDropDown() {
  return defHttp.get({ url: server + '/service/dropDown' });
}
// 获取配置模板
export function serviceConfigTemplate(data) {
  return defHttp.get({ url: server + '/service/configTemplate', data });
}
// 获取数据列表（分页）
export function servicePage(data) {
  return defHttp.get({ url: server + '/service/page', data });
}
// 添加
export function serviceAdd(data) {
  return defHttp.post({ url: server + '/service/add', data });
}
// 编辑
export function serviceUpdate(data) {
  return defHttp.post({ url: server + '/service/update', data });
}
// 删除
export function serviceRemove(id) {
  // return defHttp.post({ url: server + '/service/remove?id=' + id });
  let data = {
    id: id,
  };
  return defHttp.post({ url: server + '/service/remove', data });
}
// 详情
export function serviceDetail(data) {
  return defHttp.get({ url: server + '/service/detail', data });
}
// 启用
export function serviceEnable(id) {
  return defHttp.get({ url: server + '/service/enable?id=' + id });
}
// 禁用
export function serviceDisable(id) {
  return defHttp.get({ url: server + '/service/disable?id=' + id });
}
// 保存
export function systemSaveConfig() {
  return defHttp.get({ url: server + '/system/saveConfig' });
}
// 导出
export function systemDownloadServiceConfig() {
  return defHttp.get({ url: server + '/system/downloadServiceConfig' });
}

// -------观察列表------

// 获取所有采集器及隶属标签
export function serviceAllCollectorTags(data) {
  return defHttp.get({ url: server + '/service/allCollectorTags', data });
}
// 获取服务观察列表
export function serviceWatchlist(data) {
  return defHttp.get({ url: server + '/service/watchlist', data });
}
// 设定服务观察列表
export function serviceWatch(data) {
  return defHttp.post({ url: server + '/service/watch', data });
}
// 删除服务观察列表
export function serviceUnwatch(data) {
  return defHttp.post({ url: server + '/service/unwatch', data });
}
