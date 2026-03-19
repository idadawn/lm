import { defHttp } from '/@/utils/http/axios';

enum Api {
  Version = '/api/system/v1/Version/Version',
  Changelog = '/api/system/v1/Version/Changelog',
}

export interface VersionOutput {
  apiVersion: string;
  webVersion: string;
  isCompatible: boolean;
  message?: string;
}

export interface ChangelogItem {
  version: string;
  date: string;
  added: string[];
  fixed: string[];
  improved: string[];
}

export function getVersion(): Promise<VersionOutput> {
  return defHttp.get({ url: Api.Version });
}

export function getChangelog(): Promise<ChangelogItem[]> {
  return defHttp.get({ url: Api.Changelog });
}
