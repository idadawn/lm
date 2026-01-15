type Source = {
  id: string;
  name: string;
  age: string;
}
export const props = {
  source: {
    type:Object as PropType<Source>,
    default: () => ({}),
  },
};
