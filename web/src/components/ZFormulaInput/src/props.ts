export const props = {
  initValue: {
    type: Object,
    default: () => ({
      formula: '',
      vars: {},
    }),
  },
  value: {
    type: Object,
    default: () => ({
      formula: '',
      vars: {},
    }),
  },
  placeholder: String,
  disabled: {
    type: Boolean,
    default: false,
  },
  scrollWrapperClassName: String,
  options: {
    type: Array,
    default: () => [],
  },
};
