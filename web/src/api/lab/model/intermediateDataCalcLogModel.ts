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

export interface IntermediateDataTraceValueItem {
  name?: string;
  value?: string;
}

export interface IntermediateDataCalcStepItem {
  order: number;
  formulaName?: string;
  columnName?: string;
  displayName?: string;
  formula?: string;
  unitName?: string;
  precision?: number;
  isDefaultValue?: boolean;
  success?: boolean;
  rawResult?: string;
  resultValue?: string;
  failureReason?: string;
  variableValues?: IntermediateDataTraceValueItem[];
}

export interface IntermediateDataJudgeStepItem {
  order: number;
  formulaName?: string;
  columnName?: string;
  displayName?: string;
  stepName?: string;
  priority?: number;
  isDefaultStep?: boolean;
  success?: boolean;
  resultValue?: string;
  conditionText?: string;
  failureReason?: string;
}

export interface IntermediateDataExecutionTrace {
  intermediateDataId: string;
  furnaceNo?: string;
  calcStatus?: number;
  calcStatusText?: string;
  calcErrorMessage?: string;
  judgeStatus?: number;
  judgeStatusText?: string;
  judgeErrorMessage?: string;
  calculationSteps?: IntermediateDataCalcStepItem[];
  judgmentSteps?: IntermediateDataJudgeStepItem[];
}
