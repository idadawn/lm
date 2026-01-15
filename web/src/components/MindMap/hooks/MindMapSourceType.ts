export interface Node {
  id: string | number;
  name: string;
  children?: Node[];
  covTreeId?: string;
  covTreeIds?: Array<string | number>;
  createdTime?: string | number;
  createdUserid?: string | number;
  gotId?: string | number;
  gotParentId?: string | number;
  gotTreeId?: string;
  gotType?: string;
  hasChildren?: boolean;
  isLeaf?: boolean;
  is_root?: boolean;
  lastModifiedTime?: string | number;
  lastModifiedUserid?: string | number;
  metricId?: string | number;
  num?: string | number;
  parentId?: string | number;
  tenantId?: string | number;
  // 字段拼接
  metricName?: string;
  metricGrade?: MetricGrade[];
  currentValue: string;
  trendData?: TrendData[];
}

export interface MetricGrade {
  id: string | number;
  is_show: boolean;
  metricId: string | number;
  name: string;
  status_color: string;
  value: string | number;
}

export interface TrendData {
  item: string;
  value: number | string;
}

export interface Edge {
  source: string | number;
  target: string | number;
}

export type EdgeInit = string[];

export interface SourceInterface {
  nodes: Node[];
  edges: Edge[];
  gotId: string | number;
}
