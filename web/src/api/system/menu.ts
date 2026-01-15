import { defHttp } from '/@/utils/http/axios';

enum Api {
  Prefix = '/api/system/Menu',
}

// 获取菜单列表
export function getMenuList(data) {
  return defHttp.get({ url: Api.Prefix + `/ModuleBySystem/${data.systemId}`, data });
}
// 获取上级菜单下拉框
export function getMenuSelector(data, id, systemId = '0') {
  return defHttp.get({ url: Api.Prefix + `/Selector/${(!!id ? id : '0') + '/' + systemId}`, data });
}
// 获取菜单列表（下拉框）
export function getSelectorAll(data) {
  return defHttp.get({ url: Api.Prefix + `/Selector/All`, data });
}
// 新建菜单
export function create(data) {
  return defHttp.post({ url: Api.Prefix, data });
}
// 修改菜单
export function update(data) {
  return defHttp.put({ url: Api.Prefix + '/' + data.id, data });
}
// 获取菜单详情
export function getInfo(id) {
  return defHttp.get({ url: Api.Prefix + '/' + id });
}
// 删除菜单
export function delMenu(id) {
  return defHttp.delete({ url: Api.Prefix + '/' + id });
}
// 导出系统菜单数据
export function exportMenu(id) {
  return defHttp.get({ url: Api.Prefix + `/${id}/Action/Export` });
}
