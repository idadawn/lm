import { MetricCovStatusOptionType } from '/@/components/ZEditor/types/type';

type node = {
  id: string;
  label: string;
};

type edge = {
  source: string | number;
  target: string | number;
};
type Source = {
  nodes?: node[];
  edges?: edge[];
  [propName: string]: any;
};
export const props = {
  source: {
    type: Object as PropType<Source>,
    default: () => ({}),
  },
  dragItem: {
    type: Object as PropType<node>,
    default: () => ({}),
  },
  statusOptions: {
    type: Array as PropType<MetricCovStatusOptionType[]>,
    default: () => [],
  },
};
