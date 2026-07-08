import { defHttp } from '/@/utils/http/axios';

enum Api {
  Prefix = '/api/lab/trace',
}

// 按炉号（或扫码枪扫描内容）查询全链路追溯数据
export function getTraceByCode(code: string) {
  return defHttp.get({ url: Api.Prefix, params: { code } });
}
