import type { AppRouteModule } from '/@/router/types';

import { LAYOUT } from '/@/router/constant';
import { t } from '/@/hooks/web/useI18n';

const lab: AppRouteModule = {
    path: '/lab',
    name: 'Lab',
    component: LAYOUT,
    redirect: '/lab/monthly-dashboard',
    meta: {
        orderNo: 100,
        icon: 'ion:flask-outline',
        title: t('routes.lab.lab'),
    },
    children: [
        {
            path: 'metric/form/:id?',
            name: 'MetricForm',
            component: () => import('/@/views/lab/metric/Form.vue'),
            meta: {
                title: 'Metric Form',
                hideMenu: true,
            }
        },
        {
            path: 'metric',
            name: 'MetricList',
            component: () => import('/@/views/lab/metric/index.vue'),
            meta: {
                title: 'Metric Definitions',
            },
        },
        {
            path: 'intermediate-data-formula',
            name: 'IntermediateDataFormula',
            component: () => import('/@/views/lab/intermediateDataFormula/index.vue'),
            meta: {
                title: '公式维护',
            },
        },
        {
            path: 'intermediate-data-judgment-level',
            name: 'IntermediateDataJudgmentLevel',
            component: () => import('/@/views/lab/intermediateDataJudgmentLevel/index.vue'),
            meta: {
                title: '判定等级',
            },
        },
        {
            path: 'monthly-report',
            name: 'LabMonthlyReport',
            component: () => import('/@/views/lab/monthlyReport/index.vue'),
            meta: {
                title: '月度质量报表',
                icon: 'ant-design:bar-chart-outlined',
            },
        },
        {
            path: 'report-config',
            name: 'ReportConfigList',
            component: () => import('/@/views/lab/reportConfig/index.vue'),
            meta: {
                title: '指标列表',
                icon: 'ant-design:ordered-list-outlined',
            },
        },
        {
            path: 'monthly-dashboard',
            name: 'LabMonthlyDashboard',
            component: () => import('/@/views/lab/monthly-dashboard/index.vue'),
            meta: {
                title: '生产驾驶舱',
                icon: 'ion:grid-outline',
                affix: true,
            },
        },
    ],
};

export default lab;
