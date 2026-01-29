export interface IntermediateDataCalcLogQuery {
  batchId?: string;
  intermediateDataId?: string;
  columnName?: string;
  formulaType?: string;
  errorType?: string;
  currentPage?: number;
  pageSize?: number;
}

export interface IntermediateDataCalcLogItem {
  id: string;
  batchId?: string;
  intermediateDataId?: string;
  columnName?: string;
  formulaName?: string;
  formulaType?: string;
  errorType?: string;
  errorMessage?: string;
  errorDetail?: string;
  creatorTime?: string;
}

export interface IntermediateDataCalcLogPage {
  list: IntermediateDataCalcLogItem[];
  pagination: {
    currentPage: number;
    pageSize: number;
    total: number;
  };
}
