import { MetricCovStatusOptionType } from "/@/components/ZEditor/types/type";

export const props = {
  form: {
    type:Object as PropType<any>,
    default: () => ({}),
  },
  statusOptions: {
    type: Array as PropType<MetricCovStatusOptionType[]>,
    default: () => [],
  },
};
