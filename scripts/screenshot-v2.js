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
  console.log(`✓ Screenshot saved: ${path.basename(filePath)}`);
}

async function waitForPageLoad(page) {
  // 等待网络请求完成
  await page.waitForLoadState('networkidle', { timeout: 60000 });
  // 等待页面可见内容
  await page.waitForSelector('body', { timeout: 10000 });
  // 额外等待确保页面渲染完成
  await delay(3000);
}

(async () => {
  const browser = await chromium.launch({ headless: false }); // 先使用有头模式方便调试
  const context = await browser.newContext({ 
    viewport: { width: 1920, height: 1080 },
    locale: 'zh-CN'
  });
  const page = await context.newPage();

  try {
    // 1. 登录页面
    console.log('\n[1/16] 登录页面...');
    await page.goto('http://47.105.59.151:8928/login', { waitUntil: 'networkidle' });
    await delay(2000);
    await screenshot(page, path.join(baseDir, '02-登录页面.png'));

    // 2. 输入账号密码登录
    console.log('[2/16] 正在登录...');
    await page.fill('input[type="text"]', 'admin');
    await page.fill('input[type="password"]', 'lm@2025');
    await page.click('button[type="submit"]');
    
    // 等待跳转到首页
    console.log('    等待页面跳转...');
    await page.waitForURL('**/lab/monthly-dashboard', { timeout: 30000 });
    await waitForPageLoad(page);
    
    // 验证是否登录成功（检查页面标题或特征元素）
    const pageTitle = await page.title();
    console.log(`    当前页面标题: ${pageTitle}`);
    
    if (pageTitle.includes('登录') || page.url().includes('/login')) {
      throw new Error('登录失败，仍在登录页面');
    }

    // 3. 生产驾驶舱（首页）
    console.log('[3/16] 生产驾驶舱...');
    await screenshot(page, path.join(baseDir, '03-生产驾驶舱.png'));
    await screenshot(page, path.join(reportDir, '05-01-生产驾驶舱-完整界面.png'));

    // 4. 基础管理 - 产品规格定义
    console.log('[4/16] 产品规格定义...');
    await page.goto('http://47.105.59.151:8928/lab/product', { waitUntil: 'networkidle' });
    await waitForPageLoad(page);
    await screenshot(page, path.join(baseMgmtDir, '03-01-产品规格定义-列表页.png'));

    // 5. 基础管理 - 单位管理
    console.log('[5/16] 单位管理...');
    await page.goto('http://47.105.59.151:8928/lab/unit', { waitUntil: 'networkidle' });
    await waitForPageLoad(page);
    await screenshot(page, path.join(baseMgmtDir, '03-03-单位管理-单位维度.png'));

    // 6. 基础管理 - 特性等级管理
    console.log('[6/16] 特性等级管理...');
    await page.goto('http://47.105.59.151:8928/lab/severity-level', { waitUntil: 'networkidle' });
    await waitForPageLoad(page);
    await screenshot(page, path.join(baseMgmtDir, '03-07-特性等级管理-列表页.png'));

    // 7. 基础管理 - 外观特性大类管理
    console.log('[7/16] 外观特性大类管理...');
    await page.goto('http://47.105.59.151:8928/lab/appearanceCategory', { waitUntil: 'networkidle' });
    await waitForPageLoad(page);
    await screenshot(page, path.join(baseMgmtDir, '03-09-外观特性大类管理-列表页.png'));

    // 8. 基础管理 - 外观特性管理
    console.log('[8/16] 外观特性管理...');
    await page.goto('http://47.105.59.151:8928/lab/appearance', { waitUntil: 'networkidle' });
    await waitForPageLoad(page);
    await screenshot(page, path.join(baseMgmtDir, '03-11-外观特性管理-主界面.png'));

    // 9. 基础管理 - 公共维度管理
    console.log('[9/16] 公共维度管理...');
    await page.goto('http://47.105.59.151:8928/lab/publicDimension', { waitUntil: 'networkidle' });
    await waitForPageLoad(page);
    await screenshot(page, path.join(baseMgmtDir, '03-14-公共维度管理-列表页.png'));

    // 10. 基础管理 - 公式维护
    console.log('[10/16] 公式维护...');
    await page.goto('http://47.105.59.151:8928/lab/intermediate-data-formula', { waitUntil: 'networkidle' });
    await waitForPageLoad(page);
    await screenshot(page, path.join(baseMgmtDir, '03-17-公式维护-主界面.png'));

    // 11. 基础管理 - 判定等级
    console.log('[11/16] 判定等级...');
    await page.goto('http://47.105.59.151:8928/lab/intermediate-data-judgment-level', { waitUntil: 'networkidle' });
    await waitForPageLoad(page);
    await screenshot(page, path.join(baseMgmtDir, '03-20-判定等级-主界面.png'));

    // 12. 基础管理 - 指标列表
    console.log('[12/16] 指标列表...');
    await page.goto('http://47.105.59.151:8928/lab/report-config', { waitUntil: 'networkidle' });
    await waitForPageLoad(page);
    await screenshot(page, path.join(baseMgmtDir, '03-24-指标列表-主界面.png'));

    // 13. 数据管理 - 原始数据管理
    console.log('[13/16] 原始数据管理...');
    await page.goto('http://47.105.59.151:8928/lab/rawData', { waitUntil: 'networkidle' });
    await waitForPageLoad(page);
    await screenshot(page, path.join(dataMgmtDir, '04-01-原始数据管理-检测数据.png'));

    // 14. 数据管理 - 中间数据管理
    console.log('[14/16] 中间数据管理...');
    await page.goto('http://47.105.59.151:8928/lab/intermediate-data', { waitUntil: 'networkidle' });
    await waitForPageLoad(page);
    await delay(5000); // 中间数据页面加载较慢
    await screenshot(page, path.join(dataMgmtDir, '04-09-中间数据管理-主界面.png'));

    // 15. 月度质量报表
    console.log('[15/16] 月度质量报表...');
    await page.goto('http://47.105.59.151:8928/lab/monthly-report', { waitUntil: 'networkidle' });
    await waitForPageLoad(page);
    await screenshot(page, path.join(reportDir, '05-02-月度质量报表.png'));

    // 16. 返回生产驾驶舱截图特写
    console.log('[16/16] 生产驾驶舱特写...');
    await page.goto('http://47.105.59.151:8928/lab/monthly-dashboard', { waitUntil: 'networkidle' });
    await waitForPageLoad(page);
    await delay(3000);
    
    // 尝试截取KPI区域
    try {
      const kpiArea = await page.locator('.page-header, .kpi-cards, [class*="Kpi"]').first();
      if (kpiArea) {
        await kpiArea.screenshot({ path: path.join(reportDir, '05-02-生产驾驶舱-KPI指标卡.png') });
        console.log('✓ 保存: 05-02-生产驾驶舱-KPI指标卡.png');
      }
    } catch (e) {
      console.log('    KPI区域截图失败:', e.message);
    }

    console.log('\n========================================');
    console.log('所有截图已完成！');
    console.log('========================================');

  } catch (error) {
    console.error('\n✗ 错误:', error.message);
    // 保存错误时的页面状态
    try {
      await page.screenshot({ path: path.join(baseDir, 'error-state.png') });
      console.log('已保存错误状态截图: error-state.png');
    } catch (e) {}
  } finally {
    await browser.close();
  }
})();
