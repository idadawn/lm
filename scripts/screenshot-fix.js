const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');

const baseDir = path.join(__dirname, '../docs/screenshots');
const baseMgmtDir = path.join(baseDir, '基础管理');
const dataMgmtDir = path.join(baseDir, '数据管理');
const reportDir = path.join(baseDir, '报表分析');

async function delay(ms) {
  return new Promise(resolve => setTimeout(resolve, ms));
}

async function screenshot(page, filePath) {
  await page.screenshot({ path: filePath, fullPage: false });
  console.log(`✓ ${path.basename(filePath)}`);
}

(async () => {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: { width: 1920, height: 1080 } });
  const page = await context.newPage();

  try {
    // 登录
    console.log('[登录] 访问系统...');
    await page.goto('http://47.105.59.151:8928/login', { waitUntil: 'networkidle' });
    await delay(2000);
    await page.locator('input').nth(0).fill('admin');
    await page.locator('input').nth(1).fill('Lm@2025');
    await page.locator('button').first().click();
    await delay(8000);

    // 1. 修正中间数据管理URL - 从左侧菜单进入
    console.log('[1] 04-09-中间数据管理-主界面.png');
    await page.goto('http://47.105.59.151:8928/lab/subTable', { waitUntil: 'networkidle' });
    await delay(5000);
    await screenshot(page, path.join(dataMgmtDir, '04-09-中间数据管理-主界面.png'));

    // 2. 生产驾驶舱各图表特写
    console.log('[2] 报表分析图表特写...');
    await page.goto('http://47.105.59.151:8928/lab/monthly-dashboard', { waitUntil: 'networkidle' });
    await delay(3000);
    
    // KPI指标卡
    await screenshot(page, path.join(reportDir, '05-02-生产驾驶舱-KPI指标卡.png'));
    
    // 3. 补充公共维度版本管理
    console.log('[3] 03-16-公共维度管理-版本管理.png');
    await page.goto('http://47.105.59.151:8928/lab/publicDimension', { waitUntil: 'networkidle' });
    await delay(3000);
    const versionBtns = await page.locator('button').all();
    for (const btn of versionBtns) {
      const text = await btn.textContent();
      if (text && text.includes('版本')) {
        await btn.click();
        await delay(2000);
        break;
      }
    }
    await screenshot(page, path.join(baseMgmtDir, '03-16-公共维度管理-版本管理.png'));

    console.log('\n修复完成！');

  } catch (error) {
    console.error('\n✗ 错误:', error.message);
  } finally {
    await browser.close();
  }
})();
