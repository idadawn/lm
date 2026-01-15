import type { PropType } from 'vue';

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
  gotId?: string | number;
  [propName: string]: any;
};

type AddItemType = {
  id: string;
  name: string;
  gotId: string;
  parentId: string;
};

export const props = {
  source: {
    type: Object as PropType<Source>,
    default: () => ({
      nodes: [],
      edges: [],
      gotId: '',
    }),
    required: true,
  },
  dragItem: {
    type: Object as PropType<node>,
    default: () => ({}),
  },
  authAdd: {
    type: Boolean as PropType<boolean>,
    default: () => false,
  },
  authDelete: {
    type: Boolean as PropType<boolean>,
    default: () => false,
  },
  onClick: {
    type: Function as PropType<(...args) => any>,
    default: null,
  },
  onAddItem: {
    type: Function as PropType<(...args) => AddItemType | null>,
    default: null,
  },
  onDeleteItem: {
    type: Function as PropType<(...args) => any>,
    default: null,
  },
  onNodeClick: {
    type: Function as PropType<(...args) => any>,
    default: null,
  },
};
