const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');

// 创建截图目录
const baseDir = path.join(__dirname, '../docs/screenshots');
const baseMgmtDir = path.join(baseDir, '基础管理');
const dataMgmtDir = path.join(baseDir, '数据管理');
const reportDir = path.join(baseDir, '报表分析');

[baseMgmtDir, dataMgmtDir, reportDir].forEach(dir => {
  if (!fs.existsSync(dir)) {
    fs.mkdirSync(dir, { recursive: true });
  }
});

async function delay(ms) {
  return new Promise(resolve => setTimeout(resolve, ms));
}

async function screenshot(page, filePath) {
  await page.screenshot({ path: filePath, fullPage: false });
  console.log(`✓ ${path.basename(filePath)}`);
}

async function waitForPageLoad(page) {
  await page.waitForLoadState('networkidle', { timeout: 60000 });
  await page.waitForSelector('body', { timeout: 10000 });
  await delay(2000);
}

(async () => {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ 
    viewport: { width: 1920, height: 1080 },
    locale: 'zh-CN'
  });
  const page = await context.newPage();

  const pages = [
    { url: '/login', file: '02-登录页面.png', dir: baseDir, name: '登录页面' },
    { url: '/lab/monthly-dashboard', file: '03-生产驾驶舱.png', dir: baseDir, name: '生产驾驶舱' },
    { url: '/lab/monthly-dashboard', file: '05-01-生产驾驶舱-完整界面.png', dir: reportDir, name: '生产驾驶舱(报表)' },
    { url: '/lab/product', file: '03-01-产品规格定义-列表页.png', dir: baseMgmtDir, name: '产品规格定义' },
    { url: '/lab/unit', file: '03-03-单位管理-单位维度.png', dir: baseMgmtDir, name: '单位管理' },
    { url: '/lab/severity-level', file: '03-07-特性等级管理-列表页.png', dir: baseMgmtDir, name: '特性等级管理' },
    { url: '/lab/appearanceCategory', file: '03-09-外观特性大类管理-列表页.png', dir: baseMgmtDir, name: '外观特性大类管理' },
    { url: '/lab/appearance', file: '03-11-外观特性管理-主界面.png', dir: baseMgmtDir, name: '外观特性管理' },
    { url: '/lab/publicDimension', file: '03-14-公共维度管理-列表页.png', dir: baseMgmtDir, name: '公共维度管理' },
    { url: '/lab/intermediate-data-formula', file: '03-17-公式维护-主界面.png', dir: baseMgmtDir, name: '公式维护' },
    { url: '/lab/intermediate-data-judgment-level', file: '03-20-判定等级-主界面.png', dir: baseMgmtDir, name: '判定等级' },
    { url: '/lab/report-config', file: '03-24-指标列表-主界面.png', dir: baseMgmtDir, name: '指标列表' },
    { url: '/lab/rawData', file: '04-01-原始数据管理-检测数据.png', dir: dataMgmtDir, name: '原始数据管理' },
    { url: '/lab/intermediate-data', file: '04-09-中间数据管理-主界面.png', dir: dataMgmtDir, name: '中间数据管理' },
    { url: '/lab/monthly-report', file: '05-02-月度质量报表.png', dir: reportDir, name: '月度质量报表' },
  ];

  try {
    // 1. 先访问登录页面
    console.log('\n[1/15] 登录页面...');
    await page.goto('http://47.105.59.151:8928/login', { waitUntil: 'networkidle' });
    await delay(2000);
    await screenshot(page, path.join(baseDir, '02-登录页面.png'));

    // 2. 输入账号密码
    console.log('[2/15] 登录中...');
    await page.fill('input[type="text"]', 'admin');
    await page.fill('input[type="password"]', 'lm@2025');
    
    // 点击登录按钮（使用第一个按钮）
    const buttons = await page.locator('button').all();
    for (const btn of buttons) {
      const text = await btn.textContent();
      if (text && text.includes('登录')) {
        await btn.click();
        break;
      }
    }
    
    // 等待跳转
    await delay(5000);
    await page.waitForLoadState('networkidle', { timeout: 60000 });
    
    // 检查是否登录成功
    const currentUrl = page.url();
    console.log(`    当前URL: ${currentUrl}`);
    
    if (currentUrl.includes('/login')) {
      throw new Error('登录失败');
    }

    console.log('    登录成功！');

    // 3. 从第2个开始截取所有页面
    for (let i = 2; i < pages.length; i++) {
      const p = pages[i];
      console.log(`[${i + 1}/15] ${p.name}...`);
      try {
        await page.goto(`http://47.105.59.151:8928${p.url}`, { waitUntil: 'networkidle' });
        await waitForPageLoad(page);
        
        // 中间数据页面需要额外等待
        if (p.url.includes('intermediate-data')) {
          await delay(5000);
        }
        
        await screenshot(page, path.join(p.dir, p.file));
      } catch (e) {
        console.error(`    ✗ ${p.name} 失败: ${e.message}`);
      }
    }

    console.log('\n========================================');
    console.log('所有截图已完成！');
    console.log('========================================');

  } catch (error) {
    console.error('\n✗ 错误:', error.message);
    await page.screenshot({ path: path.join(baseDir, 'error-state.png') });
  } finally {
    await browser.close();
  }
})();
