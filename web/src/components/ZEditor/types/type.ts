export type RuleType = {
  id?: string | number;
  covId?: string | number;
  level?: string | number;
  operators: string;
  type: string;
  value?: string | number;
  minValue?: string | number;
  maxValue?: string | number;
  status: string;
  statusName?: string;
  statusColor?: string;
};

export type RuleListType = Array<RuleType>;

export type MetricCovStatusOptionType = {
  id: string | number;
  name: string;
  color: string;
};
