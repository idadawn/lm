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
  console.log(`Screenshot saved: ${filePath}`);
}

(async () => {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: { width: 1920, height: 1080 } });
  const page = await context.newPage();

  try {
    // 1. 登录页面
    console.log('Navigating to login page...');
    await page.goto('http://47.105.59.151:8928/login');
    await delay(2000);
    await screenshot(page, path.join(baseDir, '02-登录页面.png'));

    // 2. 登录
    console.log('Logging in...');
    await page.locator('input').first().fill('admin');
    await page.locator('input').nth(1).fill('lm@2025');
    await delay(500);
    await page.locator('button').first().click();
    await delay(8000);
    await page.waitForLoadState('networkidle', { timeout: 60000 });
    await delay(3000);

    // 3. 生产驾驶舱（首页）
    console.log('Screenshot: 生产驾驶舱');
    await screenshot(page, path.join(baseDir, '03-生产驾驶舱.png'));
    await screenshot(page, path.join(reportDir, '05-01-生产驾驶舱-完整界面.png'));

    // 使用直接URL访问各功能页面
    const pages = [
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
    ];

    for (const p of pages) {
      console.log(`Navigating to ${p.name}...`);
      try {
        await page.goto(`http://47.105.59.151:8928${p.url}`);
        await delay(3000);
        await page.waitForLoadState('networkidle', { timeout: 30000 });
        await delay(2000);
        await screenshot(page, path.join(p.dir, p.file));
      } catch (e) {
        console.error(`Error navigating to ${p.name}: ${e.message}`);
        // 继续下一个页面
      }
    }

    // 截取生产驾驶舱各图表的特写
    console.log('Screenshot: 生产驾驶舱图表特写...');
    await page.goto('http://47.105.59.151:8928/lab/monthly-dashboard');
    await delay(5000);
    
    // 截取KPI区域
    const kpiCard = await page.locator('.page-header').first();
    if (kpiCard) {
      try {
        await kpiCard.screenshot({ path: path.join(reportDir, '05-02-生产驾驶舱-KPI指标卡.png') });
        console.log('Saved: 05-02-生产驾驶舱-KPI指标卡.png');
      } catch (e) {}
    }

    console.log('All screenshots completed!');

  } catch (error) {
    console.error('Error:', error);
  } finally {
    await browser.close();
  }
})();
