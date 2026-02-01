import { t } from '/@/hooks/web/useI18n';
import { EXCEPTION_COMPONENT, LAYOUT, PAGE_NOT_FOUND_NAME, REDIRECT_NAME } from '/@/router/constant';
import type { AppRouteRecordRaw } from '/@/router/types';

// 404 on a page
export const PAGE_NOT_FOUND_ROUTE: AppRouteRecordRaw = {
  path: '/:path(.*)*',
  name: PAGE_NOT_FOUND_NAME,
  component: LAYOUT,
  meta: {
    title: 'ErrorPage',
    hideBreadcrumb: true,
    hideMenu: true,
  },
  children: [
    {
      path: '/:path(.*)*',
      name: PAGE_NOT_FOUND_NAME,
      component: EXCEPTION_COMPONENT,
      meta: {
        title: '',
        hideBreadcrumb: true,
        hideMenu: true,
      },
    },
  ],
};

export const REDIRECT_ROUTE: AppRouteRecordRaw = {
  path: '/redirect',
  component: LAYOUT,
  name: 'RedirectTo',
  meta: {
    title: REDIRECT_NAME,
    hideBreadcrumb: true,
    hideMenu: true,
  },
  children: [
    {
      path: '/redirect/:path(.*)/:_redirect_type(.*)/:_origin_params(.*)?',
      name: REDIRECT_NAME,
      component: () => import('/@/views/basic/redirect/index.vue'),
      meta: {
        title: REDIRECT_NAME,
        hideBreadcrumb: true,
      },
    },
  ],
};

export const ERROR_LOG_ROUTE: AppRouteRecordRaw = {
  path: '/error-log',
  name: 'ErrorLog',
  component: LAYOUT,
  redirect: '/error-log/list',
  meta: {
    title: 'ErrorLog',
    hideBreadcrumb: true,
    hideChildrenInMenu: true,
  },
  children: [
    {
      path: 'list',
      name: 'ErrorLogList',
      component: () => import('/@/views/basic/error-log/index.vue'),
      meta: {
        title: t('routes.basic.errorLogList'),
        hideBreadcrumb: true,
        currentActiveMenu: '/error-log',
      },
    },
  ],
};
export const COMMON_ROUTE: AppRouteRecordRaw = {
  path: '/common-route',
  name: 'commonRoute',
  component: LAYOUT,
  redirect: '/lab/dashboard',
  meta: {
    title: 'commonRoute',
    hideBreadcrumb: true,
    hideChildrenInMenu: true,
  },
  children: [
    {
      name: 'editor',
      path: '/dashboard/editor',
      component: () => import('/@/views/kpi/indicatorOverview/ChartEditor.vue'),
      meta: {
        title: 'editor',
        defaultTitle: 'editor',
        icon: 'icon-ym icon-ym-btn-preview',
        affix: false,
      },
    },
    {
      name: 'editorDemo',
      path: '/editorDemo',
      component: () => import('/@/views/kpi/createModel/editorDemo.vue'),
      meta: {
        title: 'editor',
        defaultTitle: 'editor',
        icon: 'icon-ym icon-ym-btn-preview',
        affix: false,
      },
    },
    {
      path: '/lab/dashboard',
      component: () => import('/@/views/lab/dashboard/index.vue'),
      name: 'home',
      meta: {
        title: 'routes.basic.home',
        defaultTitle: '首页',
        icon: 'icon-ym icon-ym-nav-home',
        affix: true,
      },
    },
    {
      path: '/messageRecord',
      component: () => import('/@/views/basic/messageRecord/index.vue'),
      name: 'messageRecord',
      meta: {
        title: 'routes.basic.messageRecord',
        defaultTitle: '站内消息',
        icon: 'icon-ym icon-ym-sysNotice',
      },
    },
    {
      path: '/profile',
      component: () => import('/@/views/basic/profile/index.vue'),
      name: 'profile',
      meta: {
        title: 'routes.basic.profile',
        defaultTitle: '个人信息',
        icon: 'icon-ym icon-ym-user',
      },
    },
    {
      path: '/externalLink',
      component: () => import('/@/views/common/externalLink/index.vue'),
      name: 'externalLink',
      meta: {
        title: 'routes.basic.externalLink',
        defaultTitle: '链接',
        icon: 'icon-ym icon-ym-generator-link',
      },
    },
    {
      path: '/workFlowDetail',
      component: () => import('/@/views/workFlow/workFlowDetail/index.vue'),
      name: 'workFlowDetail',
      meta: {
        title: 'routes.basic.workFlowDetail',
        defaultTitle: '流程详情',
        icon: 'icon-ym icon-ym-workFlow',
      },
    },
    {
      path: '/emailDetail',
      component: () => import('/@/views/extend/email/DetailPage.vue'),
      name: 'emailDetail',
      meta: {
        title: 'routes.basic.emailDetail',
        defaultTitle: '查看邮件',
        icon: 'icon-ym icon-ym-emailExample',
      },
    },
    {
      path: '/sso',
      component: () => import('/@/views/basic/login/sso-redirect.vue'),
      name: 'sso',
      meta: {
        title: 'sso',
        hideMenu: true,
      },
    },
    {
      path: '/previewModel',
      component: () => import('/@/views/common/dynamicModel/index.vue'),
      name: 'previewModel',
      meta: {
        title: 'routes.basic.previewModel',
        defaultTitle: '功能预览',
        icon: 'icon-ym icon-ym-btn-preview',
      },
    },
    {
      path: '/collectorManagement/tag/:id',
      component: () => import('/@/views/collectorManagement/tag/index.vue'),
      name: 'collectorManagement',
      meta: {
        title: '标签',
        defaultTitle: '标签',
        icon: 'icon-ym icon-ym-btn-preview',
        affix: false,
      },
    },
    {
      path: '/service/watch/:id',
      component: () => import('/@/views/service/watch/index.vue'),
      name: 'service',
      meta: {
        title: '观察列表',
        defaultTitle: '观察列表',
        icon: 'icon-ym icon-ym-btn-preview',
        affix: false,
      },
    },
    {
      path: '/kpi/indicatorDefine/formAtomic',
      component: () => import('/@/views/kpi/indicatorDefine/FormAtomic.vue'),
      name: 'FormAtomic',
      meta: {
        title: '原子指标',
        defaultTitle: '原子指标',
        icon: 'icon-ym icon-ym-echartsScatter',
        affix: false,
      },
    },
    {
      path: '/kpi/indicatorDefine/formDerive',
      component: () => import('/@/views/kpi/indicatorDefine/FormDerive.vue'),
      name: 'FormDerive',
      meta: {
        title: '派生指标',
        defaultTitle: '派生指标',
        icon: 'icon-ym icon-ym-echartsLineArea',
        affix: false,
      },
    },
    {
      path: '/kpi/indicatorDefine/formRecombination',
      component: () => import('/@/views/kpi/indicatorDefine/FormRecombination.vue'),
      name: 'FormRecombination',
      meta: {
        title: '复合指标',
        defaultTitle: '复合指标',
        icon: 'icon-ym icon-ym-wf-postBatchTab',
        affix: false,
      },
    },
    {
      path: '/kpi/indicatorDefine/chartsTree',
      component: () => import('/@/views/kpi/indicatorDefine/chartsTree.vue'),
      name: 'chartsTree',
      meta: {
        title: '指标数据',
        defaultTitle: '指标数据',
        icon: 'icon-ym icon-ym-wf-postBatchTab',
        affix: false,
      },
    },
    {
      path: '/lab/product',
      component: () => import('/@/views/lab/product/index.vue'),
      name: 'labProduct',
      meta: {
        title: '产品定义',
        defaultTitle: '产品定义',
        icon: 'icon-ym icon-ym-webDesign',
        affix: false,
      },
    },
    {
      path: '/lab/severity-level',
      component: () => import('/@/views/lab/severityLevel/index.vue'),
      name: 'labSeverityLevel',
      meta: {
        title: '特性等级管理',
        defaultTitle: '特性等级管理',
        icon: 'icon-ym icon-ym-webDesign',
        affix: false,
      },
    },
    {
      path: '/lab/raw-data',
      component: () => import('/@/views/lab/rawData/index.vue'),
      name: 'labRawData',
      meta: {
        title: '原始数据',
        defaultTitle: '原始数据',
        icon: 'icon-ym icon-ym-webDesign',
        affix: false,
      },
    },
    {
      path: '/lab/appearance',
      component: () => import('/@/views/lab/appearance/index.vue'),
      name: 'labAppearance',
      meta: {
        title: '外观特性管理',
        defaultTitle: '外观特性管理',
        icon: 'icon-ym icon-ym-webDesign',
        affix: false,
      },
    },
    {
      path: '/lab/appearance/correction',
      component: () => import('/@/views/lab/appearance/correction.vue'),
      name: 'labAppearanceCorrection',
      meta: {
        title: '人工修正匹配列表',
        defaultTitle: '人工修正匹配列表',
        icon: 'icon-ym icon-ym-webDesign',
        affix: false,
      },
    },
    {
      path: '/lab/appearance-category',
      component: () => import('/@/views/lab/appearanceCategory/index.vue'),
      name: 'labAppearanceCategory',
      meta: {
        title: '外观特性大类管理',
        defaultTitle: '外观特性大类管理',
        icon: 'icon-ym icon-ym-webDesign',
        affix: false,
      },
    },
    {
      path: '/lab/sub-table',
      component: () => import('/@/views/lab/intermediateData/index.vue'),
      name: 'labSubTable',
      meta: {
        title: '中间数据表',
        defaultTitle: '中间数据表',
        icon: 'icon-ym icon-ym-webDesign',
        affix: false,
      },
    },
    {
      path: '/lab/unit',
      component: () => import('/@/views/lab/unit/index.vue'),
      name: 'labUnit',
      meta: {
        title: '单位管理',
        defaultTitle: '单位管理',
        icon: 'icon-ym icon-ym-webDesign',
        affix: false,
      },
    },
    {
      path: '/lab/metric/form/:id?',
      component: () => import('/@/views/lab/metric/Form.vue'),
      name: 'MetricForm',
      meta: {
        title: '指标表单',
        defaultTitle: '指标表单',
        icon: 'icon-ym icon-ym-webDesign',
        affix: false,
        hideMenu: true,
      },
    },
    {
      path: '/lab/metric',
      component: () => import('/@/views/lab/metric/index.vue'),
      name: 'MetricList',
      meta: {
        title: '指标定义',
        defaultTitle: '指标定义',
        icon: 'icon-ym icon-ym-webDesign',
        affix: false,
      },
    },
  ],
};
