import { defHttp } from '/@/utils/http/axios';

enum Api {
  List = '/api/lab/intermediate-data-judgment-level',
  Create = '/api/lab/intermediate-data-judgment-level',
  Update = '/api/lab/intermediate-data-judgment-level',
  Delete = '/api/lab/intermediate-data-judgment-level',
}

export const getIntermediateDataJudgmentLevelList = (params?: any) =>
  defHttp.get({ url: Api.List, params });

export const getIntermediateDataJudgmentLevelById = (id: string) =>
  defHttp.get({ url: `${Api.List}/${id}` });

export const createIntermediateDataJudgmentLevel = (params: any) =>
  defHttp.post({ url: Api.Create, params });

export const updateIntermediateDataJudgmentLevel = (params: any) =>
  defHttp.put({ url: Api.Update, params });

export const deleteIntermediateDataJudgmentLevel = (id: string) =>
  defHttp.delete({ url: `${Api.Delete}/${id}` });

export const updateIntermediateDataJudgmentLevelSort = (ids: string[]) =>
  defHttp.put({ url: `${Api.Update}/sort`, params: ids });

export const batchCopyLevels = (params: {
  sourceFormulaId: string;
  targetFormulaIds: string[];
  overwriteExisting: boolean;
  sourceProductSpecId?: string;
  targetProductSpecIds?: string[];
}) => defHttp.post({ url: `${Api.Create}/batch-copy`, params });
