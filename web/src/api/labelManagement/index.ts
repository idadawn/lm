import { defHttp } from '/@/utils/http/axios';

// const server = import.meta.env.VITE_KPI_V1_SERVER + '/api/kpi/v1';
const server = '/api/kpi/v1';
// 新建标签
export function postMetrictag(data) {
  return defHttp.post({ url: server + '/metrictag', data });
}
// 获取标签列表
export function postMetrictagList(data) {
  return defHttp.post({ url: server + '/metrictag/list', data });
}
// 获取标签信息
export function getMetrictag(id) {
  return defHttp.get({ url: server + '/metrictag/' + id });
}
// 更新标签信息
export function putMetrictag(data) {
  let obj = {
    name: data.name,
    description: data.description,
  };
  return defHttp.put({ url: server + '/metrictag/' + data.id, data: obj });
}
// 删除标签信息
export function deleteMetrictag(id) {
  return defHttp.delete({ url: server + '/metrictag/' + id });
}
