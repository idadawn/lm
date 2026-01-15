import type { AppRouteModule } from '/@/router/types';

import { LAYOUT } from '/@/router/constant';
import { t } from '/@/hooks/web/useI18n';

const ai: AppRouteModule = {
    path: '/ai',
    name: 'AI',
    component: LAYOUT,
    redirect: '/ai/config',
    meta: {
        orderNo: 2000,
        icon: 'ant-design:robot-outlined',
        title: 'AI 助手',
    },
    children: [
        {
            path: 'config',
            name: 'AiConfig',
            component: () => import('/@/views/ai/config.vue'),
            meta: {
                title: '助手配置',
            },
        },
    ],
};

export default ai;
