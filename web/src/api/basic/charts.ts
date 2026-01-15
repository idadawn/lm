import { defHttp } from '/@/utils/http/axios';
const mockServer = import.meta.env.VITE_MOCK_SERVER;
const Url = {
  getChartsDataList: `${mockServer}/api/extend/getChartsDataList`,
  getHomeChartsDataList: `${mockServer}/api/extend/getHomeChartsDataList`,
  getAnalysisChartsData: `${mockServer}/api/extend/getAnalysisChartsData`,
  getChartsFormatData: `${mockServer}/api/extend/getChartsFormatData`,
};

export function getChartsDataList(data: API.GetChartsParams): Promise<API.GetChartsResult> {
  return defHttp.get({ url: Url.getChartsDataList, data });
}

export function getHomeChartsDataList(data: API.GetChartsParams): Promise<API.GetChartsResult> {
  return defHttp.get({ url: Url.getHomeChartsDataList, data });
}
export function getAnalysisChartsData(data: API.GetChartsParams): Promise<API.GetChartsResult> {
  return defHttp.get({ url: Url.getAnalysisChartsData, data });
}
export function getChartsFormatData(data: API.GetChartsParams): Promise<API.GetChartsResult> {
  return defHttp.get({ url: Url.getChartsFormatData, data });
}
