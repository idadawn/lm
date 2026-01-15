import { withInstall } from '/@/utils';
import type { ExtractPropTypes } from 'vue';
// import Component from './src/indexMore.vue';
import Component from './src/index.vue';
import { props } from './src/props';

export const MindMap = withInstall(Component);
export declare type MindMapProps = Partial<ExtractPropTypes<typeof props>>;
