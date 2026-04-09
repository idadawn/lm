const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');

const baseDir = path.join(__dirname, '../docs/screenshots');
const baseMgmtDir = path.join(baseDir, '基础管理');
const dataMgmtDir = path.join(baseDir, '数据管理');
const reportDir = path.join(baseDir, '报表分析');

[baseMgmtDir, dataMgmtDir, reportDir].forEach(dir => {
  if (!fs.existsSync(dir)) fs.mkdirSync(dir, { recursive: true });
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
  await delay(2000);
}

(async () => {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ 
    viewport: { width: 1920, height: 1080 },
    locale: 'zh-CN'
  });
  const page = await context.newPage();

  try {
    // 1. 登录页面
    console.log('\n[1/15] 登录页面...');
    await page.goto('http://47.105.59.151:8928/login', { waitUntil: 'networkidle' });
    await delay(2000);
    await screenshot(page, path.join(baseDir, '02-登录页面.png'));

    // 2. 登录 - 不等待导航，直接点击后检查URL
    console.log('[2/15] 登录中...');
    
    await page.locator('input').nth(0).fill('admin');
    await page.locator('input').nth(1).fill('Lm@2025');
    await delay(500);
    await page.locator('button').first().click();
    
    // 等待几秒让页面跳转
    await delay(8000);
    
    const currentUrl = page.url();
    console.log(`    当前URL: ${currentUrl}`);
    
    // 如果还在登录页，可能是登录失败，再试一次
    if (currentUrl.includes('/login')) {
      console.log('    重试登录...');
      await page.locator('input').nth(0).fill('admin');
      await page.locator('input').nth(1).fill('lm@2025');
      await page.locator('button').first().click();
      await delay(8000);
    }
    
    console.log(`    登录后URL: ${page.url()}`);
    
    // 无论是否跳转，只要有内容就继续
    await delay(3000);

    // 定义要截图的页面
    const pages = [
      { url: 'http://47.105.59.151:8928/lab/monthly-dashboard', file: '03-生产驾驶舱.png', dir: baseDir },
      { url: 'http://47.105.59.151:8928/lab/monthly-dashboard', file: '05-01-生产驾驶舱-完整界面.png', dir: reportDir },
      { url: 'http://47.105.59.151:8928/lab/product', file: '03-01-产品规格定义-列表页.png', dir: baseMgmtDir },
      { url: 'http://47.105.59.151:8928/lab/unit', file: '03-03-单位管理-单位维度.png', dir: baseMgmtDir },
      { url: 'http://47.105.59.151:8928/lab/severity-level', file: '03-07-特性等级管理-列表页.png', dir: baseMgmtDir },
      { url: 'http://47.105.59.151:8928/lab/appearanceCategory', file: '03-09-外观特性大类管理-列表页.png', dir: baseMgmtDir },
      { url: 'http://47.105.59.151:8928/lab/appearance', file: '03-11-外观特性管理-主界面.png', dir: baseMgmtDir },
      { url: 'http://47.105.59.151:8928/lab/publicDimension', file: '03-14-公共维度管理-列表页.png', dir: baseMgmtDir },
      { url: 'http://47.105.59.151:8928/lab/intermediate-data-formula', file: '03-17-公式维护-主界面.png', dir: baseMgmtDir },
      { url: 'http://47.105.59.151:8928/lab/intermediate-data-judgment-level', file: '03-20-判定等级-主界面.png', dir: baseMgmtDir },
      { url: 'http://47.105.59.151:8928/lab/report-config', file: '03-24-指标列表-主界面.png', dir: baseMgmtDir },
      { url: 'http://47.105.59.151:8928/lab/rawData', file: '04-01-原始数据管理-检测数据.png', dir: dataMgmtDir },
      { url: 'http://47.105.59.151:8928/lab/intermediate-data', file: '04-09-中间数据管理-主界面.png', dir: dataMgmtDir, waitExtra: 5000 },
      { url: 'http://47.105.59.151:8928/lab/monthly-report', file: '05-02-月度质量报表.png', dir: reportDir },
    ];

    // 逐个访问并截图
    for (let i = 0; i < pages.length; i++) {
      const p = pages[i];
      console.log(`[${i + 3}/15] ${p.file.replace('.png', '')}...`);
      try {
        await page.goto(p.url, { waitUntil: 'networkidle' });
        await waitForPageLoad(page);
        if (p.waitExtra) await delay(p.waitExtra);
        await screenshot(page, path.join(p.dir, p.file));
      } catch (e) {
        console.error(`    ✗ 失败: ${e.message}`);
      }
    }

    console.log('\n========================================');
    console.log('截图完成！');
    console.log('========================================');

  } catch (error) {
    console.error('\n✗ 错误:', error.message);
    try {
      await page.screenshot({ path: path.join(baseDir, 'error-state.png') });
    } catch (e) {}
  } finally {
    await browser.close();
  }
})();
