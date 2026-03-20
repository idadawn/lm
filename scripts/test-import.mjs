import fs from 'fs';
import http from 'http';
import crypto from 'crypto';
import path from 'path';

const API_BASE = 'http://localhost:9530';
const EXCEL_PATH = '/mnt/c/Users/deep/Desktop/导入.xlsx';

function md5(str) {
  return crypto.createHash('md5').update(str).digest('hex');
}

function request(method, urlPath, opts) {
  return new Promise((resolve, reject) => {
    const headers = Object.assign({}, opts.headers || {});
    let body = null;

    if (opts.form) {
      headers['Content-Type'] = 'application/x-www-form-urlencoded';
      body = opts.form;
    } else if (opts.json) {
      headers['Content-Type'] = 'application/json';
      body = JSON.stringify(opts.json);
    }

    if (body) headers['Content-Length'] = Buffer.byteLength(body);

    if (!headers['User-Agent']) headers['User-Agent'] = 'test-script/1.0';
    if (!headers['Accept']) headers['Accept'] = '*/*';

    const req = http.request({
      hostname: 'localhost',
      port: 9530,
      path: urlPath,
      method: method,
      headers: headers,
    }, (res) => {
      const chunks = [];
      res.on('data', (c) => chunks.push(c));
      res.on('end', () => {
        const raw = Buffer.concat(chunks).toString();
        try {
          resolve({ status: res.statusCode, data: JSON.parse(raw) });
        } catch (e) {
          resolve({ status: res.statusCode, data: null, raw: raw });
        }
      });
    });

    req.on('error', reject);
    req.setTimeout(120000, () => { req.destroy(); reject(new Error('Timeout')); });
    if (body) req.write(body);
    req.end();
  });
}

function sleep(ms) { return new Promise((r) => setTimeout(r, ms)); }

async function main() {
  console.log('=== 原始数据导入 API 自动化测试 ===\n');

  // 1. Login
  console.log('[1/8] 登录...');
  const loginResp = await request('POST', '/api/oauth/Login', {
    form: 'account=admin&password=' + md5('Zc@2023'),
  });
  if (loginResp.data.code !== 200) {
    console.error('  登录失败:', loginResp.data.msg);
    process.exit(1);
  }
  const token = loginResp.data.data.token;
  console.log('  ✓ 登录成功\n');
  const auth = { Authorization: token };

  // 2. Read file
  console.log('[2/8] 读取Excel文件...');
  const fileBuffer = fs.readFileSync(EXCEL_PATH);
  const fileName = path.basename(EXCEL_PATH);
  const fileData = fileBuffer.toString('base64');
  console.log('  文件:', fileName, '大小:', Math.round(fileBuffer.length / 1024) + 'KB');
  console.log('  ✓ 文件读取完成\n');

  // 3a. Create session with file data
  console.log('[3/8] 创建导入会话并上传文件...');
  const createResp = await request('POST', '/api/lab/raw-data-import-session/create', {
    json: { fileName: fileName, fileData: fileData, forceUpload: true },
    headers: auth,
  });
  if (createResp.data.code !== 200) {
    console.error('  创建会话失败:', createResp.data.msg || JSON.stringify(createResp.data).substring(0, 300));
    process.exit(1);
  }
  const sessionId = createResp.data.data;
  console.log('  会话ID:', sessionId);

  // 3b. Upload and parse
  console.log('  解析文件中...');
  const uploadResp = await request('POST', '/api/lab/raw-data-import-session/step1/upload-and-parse', {
    json: { fileName: fileName, fileData: fileData, importSessionId: sessionId, forceUpload: true },
    headers: auth,
  });
  if (uploadResp.data.code !== 200) {
    console.error('  上传解析失败:', uploadResp.data.msg || JSON.stringify(uploadResp.data).substring(0, 300));
    process.exit(1);
  }
  const ud = uploadResp.data.data;
  console.log('  总行数:', ud.totalRows, '有效行数:', ud.validRows);
  console.log('  ✓ 上传解析完成\n');

  // 4. Get product spec matches
  console.log('[4/8] 获取产品规格匹配...');
  const specResp = await request('GET', '/api/lab/raw-data-import-session/' + sessionId + '/product-specs', {
    headers: auth,
  });
  if (specResp.data.code !== 200) {
    console.error('  获取匹配失败:', specResp.data.msg);
    process.exit(1);
  }
  const specData = specResp.data.data || [];
  const matched = specData.filter((s) => s.matchStatus === 'matched');
  const unmatched = specData.filter((s) => s.matchStatus !== 'matched');
  const specNames = [...new Set(matched.map((m) => m.productSpecName))];
  console.log('  总数据:', specData.length);
  console.log('  已匹配:', matched.length);
  console.log('  未匹配:', unmatched.length);
  if (specNames.length > 0) console.log('  匹配到的规格:', specNames.join(', '));
  console.log('  ✓ 产品规格匹配完成\n');

  // 5. Save product spec matches
  console.log('[5/8] 保存产品规格匹配...');
  const updateItems = specData.map((item) => ({
    rawDataId: item.rawDataId,
    productSpecId: item.productSpecId || null,
  }));
  const updateResp = await request('PUT', '/api/lab/raw-data-import-session/' + sessionId + '/product-specs', {
    json: { sessionId: sessionId, items: updateItems },
    headers: auth,
  });
  if (updateResp.data.code !== 200) {
    console.error('  保存匹配失败:', updateResp.data.msg || JSON.stringify(updateResp.data).substring(0, 500));
    process.exit(1);
  }
  console.log('  ✓ 产品规格匹配已保存\n');

  // 6. Features (optional)
  console.log('[6/8] 获取特征匹配...');
  try {
    const featResp = await request('GET', '/api/lab/raw-data-import-session/' + sessionId + '/features', {
      headers: auth,
    });
    console.log('  特征匹配响应码:', featResp.data.code);
  } catch (e) {
    console.log('  特征匹配跳过:', e.message);
  }
  console.log('  ✓ 特征匹配步骤完成\n');

  // 7. Complete import
  console.log('[7/8] 完成导入...');
  const completeResp = await request('POST', '/api/lab/raw-data-import-session/' + sessionId + '/complete', {
    headers: auth,
    json: {},
  });
  if (completeResp.data.code !== 200) {
    console.error('  完成导入失败:', completeResp.data.msg || JSON.stringify(completeResp.data).substring(0, 500));
    process.exit(1);
  }
  console.log('  ✓ 导入完成!\n');

  // 8. Verify
  console.log('[8/8] 验证结果...');
  await sleep(2000);

  const logResp = await request('GET', '/api/lab/raw-data/import-log?currentPage=1&pageSize=5', {
    headers: auth,
  });
  if (logResp.data.code === 200) {
    const logData = logResp.data.data;
    const logs = logData.list || logData || [];
    console.log('  导入日志总条数:', logData.pagination ? logData.pagination.total : logs.length);
    if (Array.isArray(logs) && logs.length > 0) {
      logs.forEach((l, i) => {
        console.log('  [' + (i + 1) + ']', l.fileName, '状态:' + l.status, '成功:' + l.successCount, '有效:' + (l.validDataCount || 'N/A'));
      });
    }
  } else {
    console.log('  导入日志API异常:', logResp.data.msg);
  }

  console.log('\n=== 测试全部完成 ===');
  console.log('\n摘要:');
  console.log('  - 文件:', fileName);
  console.log('  - 会话ID:', sessionId);
  console.log('  - 解析行数:', ud.totalRows, '(有效:', ud.validRows + ')');
  console.log('  - 产品规格: 匹配', matched.length + '/' + specData.length);
  console.log('  - 导入结果: 成功');
}

main().catch((err) => {
  console.error('\n!!! 脚本异常:', err.message);
  process.exit(1);
});
