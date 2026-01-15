import { defHttp } from '/@/utils/http/axios';
import { useGlobSetting } from '/@/hooks/setting';

enum Api {
  Prefix = '/api/datareport/Data',
}

const { reportServer } = useGlobSetting();

// 获取报表列表
export function getDataReportList(data) {
  return defHttp.get({ url: reportServer + Api.Prefix, data });
}
// 获取报表下拉列表
export function getDataReportSelector() {
  return defHttp.get({ url: reportServer + Api.Prefix + '/Selector' });
}
// 删除报表
export function delDataReport(id) {
  return defHttp.delete({ url: reportServer + Api.Prefix + '/' + id });
}
// 复制报表
export function copy(id) {
  return defHttp.post({ url: reportServer + Api.Prefix + `/Copy/${id}` });
}
