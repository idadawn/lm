import type { AppRouteRecordRaw, Menu } from '/@/router/types';

import { defineStore } from 'pinia';
import { store } from '/@/store';
import { useUserStore } from './user';
import { toRaw } from 'vue';
import { transformObjToRoute, flatMultiLevelRoutes } from '/@/router/helper/routeHelper';
// import { transformRouteToMenu } from '/@/router/helper/menuHelper';
import { ERROR_LOG_ROUTE, PAGE_NOT_FOUND_ROUTE } from '/@/router/routes/basic';
import { PageEnum } from '/@/enums/pageEnum';

interface PermissionState {
  // 路由是否动态添加
  isDynamicAddedRoute: boolean;
  // 触发菜单更新
  lastBuildMenuTime: number;
  // 菜单列表
  menuList: Menu[];
}

export const usePermissionStore = defineStore({
  id: 'app-permission',
  state: (): PermissionState => ({
    // 路由是否动态添加
    isDynamicAddedRoute: false,
    // 触发菜单更新
    lastBuildMenuTime: 0,
    // 菜单列表
    menuList: [],
  }),
  getters: {
    getMenuList(): Menu[] {
      return this.menuList;
    },
    getLastBuildMenuTime(): number {
      return this.lastBuildMenuTime;
    },
    getIsDynamicAddedRoute(): boolean {
      return this.isDynamicAddedRoute;
    },
  },
  actions: {
    setMenuList(list: Menu[]) {
      this.menuList = list;
      list?.length > 0 && this.setLastBuildMenuTime();
    },

    setLastBuildMenuTime() {
      this.lastBuildMenuTime = new Date().getTime();
    },

    setDynamicAddedRoute(added: boolean) {
      this.isDynamicAddedRoute = added;
    },
    resetState(): void {
      this.isDynamicAddedRoute = false;
      this.menuList = [];
      this.lastBuildMenuTime = 0;
    },

    // 构建路由
    async buildRoutesAction(): Promise<AppRouteRecordRaw[]> {
      const userStore = useUserStore();
      const backMenuList = toRaw(userStore.getBackMenuList);
      const backRouterList = toRaw(userStore.getBackRouterList);
      const userInfo = toRaw(userStore.getUserInfo);

      // 动态引入组件
      let routeList = transformObjToRoute(backRouterList);
      let routes: AppRouteRecordRaw[] = [];
      routes = [PAGE_NOT_FOUND_ROUTE, ...routeList];

      // 设置菜单列表
      let menus: Menu[] = [];
      if (userInfo.systemIds && userInfo.systemIds.length && backRouterList.length) {
        for (let i = 0; i < userInfo.systemIds.length; i++) {
          const e = userInfo.systemIds[i];
          if (e.currentSystem) {
            const list = backRouterList.filter((o) => o.id === e.id);
            list.length ? (menus = (list[0].children || []) as Menu[]) : (menus = []);
            break;
          }
        }
      } else {
        menus = (!backRouterList.length ? backMenuList : backRouterList[0].children ? backRouterList[0].children : []) as Menu[];
      }
      this.setMenuList(menus);
      routeList = flatMultiLevelRoutes(routeList);

      /**
       * @description 根据设置的首页path，修正routes中的affix标记（固定首页）
       * */
      const patchHomeAffix = (routes: AppRouteRecordRaw[]) => {
        if (!routes || routes.length === 0) return;
        let homePath: string = PageEnum.BASE_HOME;

        function patcher(routes: AppRouteRecordRaw[], parentPath = '') {
          if (parentPath) parentPath = parentPath + '/';
          routes.forEach((route: AppRouteRecordRaw) => {
            const { path, children, redirect } = route;
            const currentPath = path.startsWith('/') ? path : parentPath + path;
            if (currentPath === homePath) {
              if (redirect) {
                homePath = route.redirect! as string;
              } else {
                route.meta = Object.assign({}, route.meta, { affix: true });
                throw new Error('end');
              }
            }
            children && children.length > 0 && patcher(children, currentPath);
          });
        }

        try {
          patcher(routes);
        } catch (e) {
          // 已处理完毕跳出循环
        }
        return;
      };

      routes.push(ERROR_LOG_ROUTE);
      patchHomeAffix(routes);
      return routes;
    },
  },
});

// Need to be used outside the setup
// 需要在设置之外使用
export function usePermissionStoreWithOut() {
  return usePermissionStore(store);
}
