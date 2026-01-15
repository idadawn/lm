type node = {
  id: string;
  label: string;
}
type Source = {
  nodes: node[];
}
type Params = {
  id: string;
  type: string;
}
export const props = {
  params: {
    type:Object as PropType<Params>,
    default: () => ({}),
  },
  source: {
    type:Object as PropType<Source>,
    default: () => ({nodes:[]}),
  },
  dragItem: {
    type:Object as PropType<node>,
    default: () => ({}),
  },
  onClick: { type: Function as PropType<(...args) => any>, default: null },
};
