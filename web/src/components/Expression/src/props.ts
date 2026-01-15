export const props = {
  visible: {
    type: Boolean,
    default: () => {
      return false;
    },
  },
  expressionValue: {
    type: String,
    default: () => {
      return '';
    },
  },
};
