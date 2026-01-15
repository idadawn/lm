import { withInstall } from '/@/utils';
import type { ExtractPropTypes } from 'vue';
import Component from './src/index.vue';
import { props } from './src/props';

export const ZEditor = withInstall(Component);
export declare type ZEditorFormProps = Partial<ExtractPropTypes<typeof props>>;
