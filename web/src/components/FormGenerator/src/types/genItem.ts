export interface ItemCfg {
  jnpfKey: string;
  label: string;
  labelWidth: undefined | number;
  showLabel: boolean;
  tag: string;
  tagIcon: string;
  className: string[];
  defaultValue?: any;
  required?: false;
  layout: string;
  span: number;
  dragDisabled: boolean;
  visibility: string[];
  tableName?: Nullable<string>;
  noShow: false;
  regList?: [];
  trigger?: string | string[];
  isStorage?: number;
  [prop: string]: any;
}

export interface GenItem {
  __config__: ItemCfg;
  on?: {
    change?: string;
    blur?: string;
    click?: string;
    tabClick?: string;
  };
  [prop: string]: any;
}
