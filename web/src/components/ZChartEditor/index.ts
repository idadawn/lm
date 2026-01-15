import { withInstall } from '/@/utils';
import type { ExtractPropTypes } from 'vue';
import Component from './src/index.vue';
import { props } from './src/props';

export const ZChartEditor = withInstall(Component);
export declare type ZChartEditorProps = Partial<ExtractPropTypes<typeof props>>;
