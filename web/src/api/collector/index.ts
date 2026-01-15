import { defHttp } from '/@/utils/http/axios';

const server = '/collectServer';

// 获取分集器类型接口
export function collectorDropDown() {
  return defHttp.get({ url: server + '/collector/dropDown' });
}
// 获取配置模板
export function collectorConfigTemplate(data) {
  return defHttp.get({ url: server + '/collector/configTemplate', data });
}
// 获取数据列表（分页）
export function collectorPage(data) {
  return defHttp.get({ url: server + '/collector/page', data });
}
// 添加
export function collectorAdd(data) {
  return defHttp.post({ url: server + '/collector/add', data });
}
// 编辑
export function collectorUpdate(data) {
  return defHttp.post({ url: server + '/collector/update', data });
}
// 删除
export function collectorRemove(id) {
  // return defHttp.post({ url: server + '/collector/remove?id=' + id });
  let data = {
    id: id,
  };
  return defHttp.post({ url: server + '/collector/remove', data });
}
// 详情
export function collectorDetail(data) {
  return defHttp.get({ url: server + '/collector/detail', data });
}
// 启用
export function collectorEnable(id) {
  return defHttp.get({ url: server + '/collector/enable?id=' + id });
}
// 禁用
export function collectorDisable(id) {
  return defHttp.get({ url: server + '/collector/disable?id=' + id });
}
// 保存
export function systemSaveConfig() {
  return defHttp.get({ url: server + '/system/saveConfig' });
}
// 导出
export function systemDownloadCollectorConfig() {
  return defHttp.get({ url: server + '/system/downloadCollectorConfig' });
}

// -------普通标签------

// 获取数据列表（分页）
export function tagPage(data) {
  return defHttp.get({ url: server + '/tag/page', data });
}
// 获取标签配置模板
export function tagConfigTemplate(data) {
  return defHttp.get({ url: server + '/tag/configTemplate', data });
}
// 添加
export function tagAdd(data) {
  return defHttp.post({ url: server + '/tag/add', data });
}
// 编辑
export function tagUpdate(data) {
  return defHttp.post({ url: server + '/tag/update', data });
}
// 删除
export function tagRemove(id, tagId) {
  // let data = `?id=${id}&tagId=${tagId}`;
  let data = {
    id: id,
    tagId: tagId,
  };
  return defHttp.post({ url: server + '/tag/remove', data });
}
// 详情
export function tagDetail(data) {
  return defHttp.get({ url: server + '/tag/detail', data });
}
// 查看测点值
export function tagGetValue(data) {
  return defHttp.get({ url: server + '/tag/getValue', data });
}
// 修改测点值
export function tagSetValue(data) {
  return defHttp.get({ url: server + '/tag/setValue', data });
}
// 获取标签历史值（曲线）
export function tagHistory(data) {
  return defHttp.get({ url: server + '/tag/history', data });
}
// 获取标签历史值（表格）
export function historyPage(data) {
  return defHttp.get({ url: server + '/tag/historyPage', data });
}
// -------逻辑标签------

// 获取数据列表（分页）
export function tagPageLogic(data) {
  return defHttp.get({ url: server + '/tag/pageLogic', data });
}
// 获取标签配置模板
export function tagConfigTemplateLogic(data) {
  return defHttp.get({ url: server + '/tag/configTemplateLogic', data });
}
// 添加
export function tagAddLogic(data) {
  return defHttp.post({ url: server + '/tag/addLogic', data });
}
// 编辑
export function tagUpdateLogic(data) {
  return defHttp.post({ url: server + '/tag/updateLogic', data });
}
// 删除
export function tagRemoveLogic(id, tagId) {
  let data = {
    id: id,
    tagId: tagId,
  };
  return defHttp.post({ url: server + '/tag/removeLogic', data });
}
// 详情
export function tagDetailLogic(data) {
  return defHttp.get({ url: server + '/tag/detailLogic', data });
}
// 查看测点值
export function tagGetValueLogic(data) {
  return defHttp.get({ url: server + '/tag/getValueLogic', data });
}
// 修改测点值
export function tagSetValueLogic(data) {
  return defHttp.get({ url: server + '/tag/setValueLogic', data });
}
