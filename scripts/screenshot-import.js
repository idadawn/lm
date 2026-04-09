const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');

const dataMgmtDir = path.join(__dirname, '../docs/screenshots/数据管理');

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

    // 进入原始数据管理-导入与日志页签
    console.log('[1] 进入导入与日志...');
    await page.goto('http://47.105.59.151:8928/lab/rawData', { waitUntil: 'networkidle' });
    await delay(3000);
    
    // 切换到导入与日志页签
    const tabs = await page.locator('.ant-tabs-tab').all();
    for (const tab of tabs) {
      const text = await tab.textContent();
      if (text && text.includes('导入与日志')) {
        await tab.click();
        break;
      }
    }
    await delay(2000);

    // 点击分步导入向导
    console.log('[2] 点击分步导入向导...');
    const wizardCards = await page.locator('.import-method-card, .ant-card').all();
    for (const card of wizardCards) {
      const text = await card.textContent();
      if (text && text.includes('分步导入向导')) {
        await card.click();
        break;
      }
    }
    await delay(3000);

    // 步骤1：上传文件
    console.log('[3] 04-04-原始数据管理-导入步骤1.png');
    await screenshot(page, path.join(dataMgmtDir, '04-04-原始数据管理-导入步骤1.png'));

    console.log('\n导入步骤截图完成！');
    console.log('(由于导入向导需要实际文件上传，步骤2-5建议手动截图补充)');

  } catch (error) {
    console.error('\n✗ 错误:', error.message);
  } finally {
    await browser.close();
  }
})();
