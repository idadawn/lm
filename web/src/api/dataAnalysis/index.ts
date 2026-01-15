import { guid } from '/@/utils/helper/toolHelper';
import { defHttp } from '/@/utils/http/axios';
const mock = import.meta.env.VITE_MOCK_SERVER;
const dev = '';
const fastmock = 'https://www.fastmock.site/mock/a69a88ff42173df1cb56573550e37842/mock';
const fastgpt = 'http://61.174.171.60:18096/api/v1';
const Url = {
  completions: `${fastgpt}/chat/completions`,
};

/**
 * 一键分析接口
 * @param params
 * @returns
 */
export function completions(params: any): Promise<any> {
  return defHttp.post(
    {
      url: Url.completions,
      params: {
        chatId: guid(),
        stream: false,
        detail: false,
        variables: {
          uid: 'asdfadsfasfd2323',
          name: 'kpi',
        },
        messages: [
          {
            content: params.prompt,
            role: 'user',
          },
        ],
      },
      headers: {
        Authorization: 'Bearer fastgpt-6LvLqQnrd2GZz8FIKrGbMVPv',
        'Content-Type': 'application/json',
      },
    },
    {
      withToken: false,
    },
  );
}
