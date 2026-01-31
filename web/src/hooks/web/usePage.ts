import type { RouteLocationRaw, Router } from 'vue-router';

import { PageEnum } from '/@/enums/pageEnum';
import { unref } from 'vue';

import { useRouter } from 'vue-router';
import { REDIRECT_NAME } from '/@/router/constant';

export type PathAsPageEnum<T> = T extends { path: string } ? T & { path: PageEnum } : T;
export type RouteLocationRawEx = PathAsPageEnum<RouteLocationRaw>;

function handleError(e: Error) {
  console.error(e);
}

/**
 * page switch
 */
export function useGo(_router?: Router) {
  const { push, replace } = _router || useRouter();
  function go(opt: RouteLocationRawEx = PageEnum.BASE_HOME, isReplace = false) {
    if (!opt) {
      return;
    }
    isReplace ? replace(opt).catch(handleError) : push(opt).catch(handleError);
  }
  return go;
}

/**
 * @description: redo current page
 */
export const useRedo = (_router?: Router) => {
  const { replace, currentRoute } = _router || useRouter();
  const { query, params = {}, name, fullPath } = unref(currentRoute.value);
  function redo(): Promise<boolean> {
    return new Promise(resolve => {
      if (name === REDIRECT_NAME) {
        resolve(false);
        return;
      }
      // Clone params to avoid mutating the original route object
      const currentParams = { ...params };
      if (name && Object.keys(currentParams).length > 0) {
        currentParams['_origin_params'] = JSON.stringify(currentParams ?? {});
        currentParams['_redirect_type'] = 'name';
        currentParams['path'] = String(name);
      } else {
        currentParams['_redirect_type'] = 'path';
        currentParams['path'] = fullPath;
      }
      replace({ name: REDIRECT_NAME, params: currentParams, query }).then(() => resolve(true));
    });
  }
  return redo;
};
