declare namespace API {
  interface BaseResult {
    version: string;
    code: string;
    msg: string;
    data: Record<string, unknown>;
  }
  interface GetNodesParams {
    userId: string;
    nodeId: string;
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
  interface GetNoticeElementsParams {
    userId: string;
    nodeId: string;
  }
  interface GetNoticeElementsResult extends BaseResult {
    data: {
      elements: Record<string, unknown>[];
    };
  }

  interface GetChartsParams {
    userId: string;
    nodeId: string;
  }
  interface GetChartsResult extends BaseResult {
    data: {
      elements: Record<string, unknown>[];
    };
  }
}
