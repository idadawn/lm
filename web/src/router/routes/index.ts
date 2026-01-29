import type { AppRouteRecordRaw } from '/@/router/types';

import { PAGE_NOT_FOUND_ROUTE, REDIRECT_ROUTE, COMMON_ROUTE } from '/@/router/routes/basic';
import { LAYOUT } from '/@/router/constant';

import { mainOutRoutes } from './mainOut';
import { PageEnum } from '/@/enums/pageEnum';
import { t } from '/@/hooks/web/useI18n';

// 根路由
export const RootRoute: AppRouteRecordRaw = {
  path: '/',
  name: 'Root',
  redirect: PageEnum.BASE_HOME,
  meta: {
    title: 'Root',
  },
};

export const LoginRoute: AppRouteRecordRaw = {
  path: '/login',
  name: 'Login',
  component: () => import('/@/views/basic/login/Login.vue'),
  meta: {
    title: t('routes.basic.login'),
  },
};
// 表单外链
export const FormShortLinkRoute: AppRouteRecordRaw = {
  path: '/formShortLink',
  name: 'FormShortLink',
  component: () => import('/@/views/common/formShortLink/index.vue'),
  meta: {
    title: '',
  },
};

// Lab Dashboard Route (for development)
export const LabDashboardRoute: AppRouteRecordRaw = {
  path: '/lab',
  name: 'Lab',
  component: LAYOUT,
  redirect: '/lab/dashboard',
  meta: {
    title: '生产驾驶舱',
    hideChildrenInMenu: true,
  },
  children: [
    {
      path: 'dashboard',
      name: 'LabDashboard',
      component: () => import('/@/views/lab/dashboard/index.vue'),
      meta: {
        title: '生产驾驶舱',
        affix: true,
      },
    },
  ],
};

// Basic routing without permission
// 未经许可的基本路由
export const basicRoutes = [
  LoginRoute,
  FormShortLinkRoute,
  RootRoute,
  LabDashboardRoute,
  ...mainOutRoutes,
  REDIRECT_ROUTE,
  PAGE_NOT_FOUND_ROUTE,
  COMMON_ROUTE,
];
