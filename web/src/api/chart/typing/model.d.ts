declare namespace API {
  interface BaseResult {
    version: string;
    code: string;
    msg: string;
    data: Record<string, unknown>;
  }
  interface GetNodesParams {
    userId: string;
  }
  interface GetNodesResult extends BaseResult {
    data: {
      nodes: Record<string, unknown>[];
      edges: Record<string, unknown>[];
    };
  }

  interface GetNodeElementsParams {
    userId: string;
    nodeId: string;
  }
  interface GetNodeElementsResult extends BaseResult {
    data: {
      elements: Record<string, unknown>[];
    };
  }

  interface GetOptimalNodeElementsParams {
    userId: string;
    nodeId: string;
  }
  interface GetOptimalNodeElementsResult extends BaseResult {
    data: {
      elements: Record<string, unknown>[];
    };
  }
  interface GetWarningNodeElementsParams {
    userId: string;
    nodeId: string;
  }
  interface GetWarningNodeElementsResult extends BaseResult {
    data: {
      elements: Record<string, unknown>[];
    };
  }

  // chart data

  interface GetChartDataParams {
    metrics: string[];
    filters: Filter[];
    // dimensions: string[];
    // orders: any[];
    // limit: number;
    // deDuplicInDimension: boolean;
  }

  interface Filter {
    operator: string;
    conditions: Condition[];
  }

  interface Condition {
    operator: string;
    field: string;
    values: string[];
  }

  interface GetWarningNodeElementsResult extends BaseResult {
    data: {
      data: Data;
    };
  }

  interface Data {
    data: string[][];
    dimensions: string[];
    time_dimension?: any;
    metric_names: string[];
    display_names: string[];
    query_id: string;
    total_time: number;
    fetch_data_query_ids?: any;
    fetch_data_time: number;
    executed_sqls: string[];
    metas: Meta[];
    hit_realizations: Hitrealizations;
    data_last_update_time: number;
  }

  interface Hitrealizations {}

  interface Meta {
    name: string;
    data_type: string;
  }

  // GetFilterData
  interface GetFilterDataParams {
    key: string;
  }
  interface GetFilterDataResult extends BaseResult {
    data: {
      data: GetFilterDataResultData;
    };
  }

  interface GetFilterDataResultData {
    query_id: string;
    total_time: number;
    fetch_data_query_ids?: any;
    fetch_data_time: number;
    executed_sqls: string[];
    metas: Meta[];
    tableFieldList: Datum[];
    hit_realizations: Hitrealizations;
    data_last_update_time?: any;
  }

  interface Hitrealizations {}

  interface Datum {
    id: string;
    name: string;
  }

  interface Meta {
    name: string;
    data_type: string;
  }
}
