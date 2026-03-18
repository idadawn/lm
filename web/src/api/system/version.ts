import { defHttp } from '/@/utils/http/axios';

enum Api {
  Prefix = '/api/system/v1/Version',
}

/**
 * 版本信息输出
 */
export interface VersionOutput {
  apiVersion: string;
  webVersion: string;
  isCompatible: boolean;
  message?: string;
}

/**
 * 更新日志项
 */
export interface ChangelogItem {
  version: string;
  date: string;
  added: string[];
  fixed: string[];
  improved: string[];
}

/**
 * 获取版本信息
 */
export function getVersion(webVersion: string) {
  return defHttp.get<VersionOutput>(
    { url: Api.Prefix },
    {
      headers: {
        'X-Web-Version': webVersion,
      },
    }
  );
}

/**
 * 获取更新日志
 */
export function getChangelog() {
  return defHttp.get<ChangelogItem[]>({ url: Api.Prefix + '/Changelog' });
}
