import { withInstall } from '/@/utils';
import type { ExtractPropTypes } from 'vue';
import Component from './src/index.vue';
import { props } from './src/props';

export const ZFormulaInput = withInstall(Component);
export declare type ZFormulaInputFormProps = Partial<ExtractPropTypes<typeof props>>;
