export const props = {
  layout: {
    type: String,
    default: () => {
      return 'horizontal';
    },
  },
  items: {
    type: Array,
    default: (): any[] => [],
  },

  formItemName: {
    type: Array,
    default: (): any[] => [],
  },

  childFormItemName: {
    type: Array,
    default: (): any[] => [],
  },
  
  itemsProps: {
    type: String,
    default: () => {
      return '';
    },
  },
  // 数据字典类型
  codeType: {
    type: String,
    default: () => {
      return '';
    },
  },
  // 数据类型
  fieldType: {
    type: String,
    default: () => {
      return '';
    },
  },
  // 输入框类型
  typeProps: {
    type: String,
    default: () => {
      return 'dataType';
    },
  },
  // 显示名称属性
  labelProps: {
    type: String,
    default: () => {
      return 'factorTypeName';
    },
  },
  // model绑定值
  valueProps: {
    type: String,
    default: () => {
      return 'factorType';
    },
  },
  // model name绑定值
  nameProps: {
    type: String,
    default: () => {
      return 'factorName';
    },
  },
  // model绑定值
  defaultValue: {
    type: String,
    default: () => {
      return 'defaultValue';
    },
  },
  // 会将model的值赋值到对象的字段上
  keyProps: {
    type: String,
    default: () => {
      return 'factorValue';
    },
  },
  // 列表选项
  listProps: {
    type: String,
    default: () => {
      return 'productFactorList';
    },
  },
  // 列表项的名称
  listLabelProps: {
    type: String,
    default: () => {
      return 'factorDisplayValue';
    },
  },
  // 列表项的值
  listValueProps: {
    type: String,
    default: () => {
      return 'factorValue';
    },
  },
  // 列表项的值类型
  listValueType: {
    type: String,
    default: () => {
      return '';
    },
  },
  // placeholder
  placeholderProps: {
    type: String,
    default: () => {
      return 'hint';
    },
  },
  // 数据源
  item: {
    type: Object,
    default: () => {
      return {};
    },
  },
  // 显示名称属性
  readonly: {
    type: Boolean,
    default: () => {
      return false;
    },
  },
  // 排序属性
  listSortProps: {
    type: String,
    default: () => {
      return '';
    },
  },
  // 设置选项默认值，Y/N
  listSelectProps: {
    type: String,
    default: () => {
      return '';
    },
  },
  // 控制是否显示属性
  displayProps: {
    type: String,
    default: () => {
      return 'isDisplay';
    },
  },
  // 显示名称属性
  relationProps: {
    type: String,
    default: () => {
      return 'relationId';
    },
  },

  // 显示名称属性
  isShowEditIcon: {
    type: Boolean,
    default: () => {
      return false;
    },
  },
  // 必填字段属性
  requiredProps: {
    type: String,
    default: () => {
      return 'isMustInput';
    },
  },

  // 其他参数
  params: {
    type: [Number, Object, Array],
    required: false,
    default: () => {
      return null;
    },
  },

  minDate: {
    type: Date,
    default: () => {
      return new Date(1900, 0, 1);
    },
  },
  maxDate: {
    type: Date,
    default: () => {
      return new Date(2100, 12, 31);
    },
  },
};
