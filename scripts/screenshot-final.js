const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');

const SCREENSHOTS_DIR = './docs/screenshots';
const BASE_URL = 'http://47.105.59.151:8928';

const screenshots = [
  // 数据管理 - 中间数据管理
  { 
    name: '数据管理/04-10-中间数据管理-排序编辑器', 
    path: '/lab/intermediateData',
    click: '.ant-table-column-sorters', // 点击列排序
    wait: 500
  },
  { 
    name: '数据管理/04-11-中间数据管理-特性选择弹窗', 
    path: '/lab/intermediateData',
    actions: async (page) => {
      // 等待并点击"选择显示列"按钮
      await page.waitForSelector('button:has-text("显示列")', { timeout: 5000 }).catch(() => null);
      const btn = await page.$('button:has-text("显示列")') || 
                  await page.$('button:has-text("列设置")') ||
                  await page.$('[class*="column"][class*="setting"]');
      if (btn) await btn.click();
      else console.log('未找到列设置按钮，尝试其他方式');
      await page.waitForTimeout(500);
    }
  },
  { 
    name: '数据管理/04-12-中间数据管理-计算日志', 
    path: '/lab/intermediateData',
    actions: async (page) => {
      // 查找计算/执行按钮
      const btn = await page.$('button:has-text("计算")') || 
                  await page.$('button:has-text("执行")') ||
                  await page.$('.ant-btn-primary');
      if (btn) {
        await btn.click();
        await page.waitForTimeout(500);
        // 等待日志弹窗出现
        await page.waitForSelector('.ant-modal, .ant-drawer', { timeout: 3000 }).catch(() => null);
      }
    }
  },
  { 
    name: '数据管理/04-13-中间数据管理-导出弹窗', 
    path: '/lab/intermediateData',
    actions: async (page) => {
      const btn = await page.$('button:has-text("导出")') || 
                  await page.$('[class*="export"]');
      if (btn) {
        await btn.click();
        await page.waitForTimeout(500);
        await page.waitForSelector('.ant-modal', { timeout: 3000 }).catch(() => null);
      }
    }
  },
  { 
    name: '数据管理/04-14-磁性数据导入-弹窗', 
    path: '/lab/rawData',
    actions: async (page) => {
      // 在原始数据页面查找磁性数据导入按钮
      const btn = await page.$('button:has-text("磁性")') || 
                  await page.$('button:has-text("磁性能")') ||
                  await page.$('[title*="磁"]');
      if (btn) {
        await btn.click();
        await page.waitForTimeout(500);
        await page.waitForSelector('.ant-modal', { timeout: 3000 }).catch(() => null);
      }
    }
  },
  // 报表分析 - 图表详情
  { 
    name: '报表分析/05-07-生产驾驶舱-厚度相关性散点图', 
    path: '/lab/monthly-dashboard',
    actions: async (page) => {
      // 滚动到厚度相关性散点图区域并点击
      await page.evaluate(() => {
        const charts = document.querySelectorAll('.ant-card');
        for (let card of charts) {
          if (card.textContent.includes('厚度') || card.textContent.includes('相关')) {
            card.scrollIntoView({ behavior: 'instant', block: 'center' });
            return true;
          }
        }
        return false;
      });
      await page.waitForTimeout(500);
    },
    clip: { x: 0, y: 400, width: 960, height: 540 }
  },
  { 
    name: '报表分析/05-08-生产驾驶舱-班次对比柱状图', 
    path: '/lab/monthly-dashboard',
    actions: async (page) => {
      await page.evaluate(() => {
        const charts = document.querySelectorAll('.ant-card');
        for (let card of charts) {
          if (card.textContent.includes('班次') || card.textContent.includes('对比')) {
            card.scrollIntoView({ behavior: 'instant', block: 'center' });
            return true;
          }
        }
        return false;
      });
      await page.waitForTimeout(500);
    },
    clip: { x: 960, y: 400, width: 960, height: 540 }
  },
  { 
    name: '报表分析/05-09-生产驾驶舱-质量趋势图', 
    path: '/lab/monthly-dashboard',
    actions: async (page) => {
      await page.evaluate(() => {
        const charts = document.querySelectorAll('.ant-card');
        for (let card of charts) {
          if (card.textContent.includes('质量') && card.textContent.includes('趋势')) {
            card.scrollIntoView({ behavior: 'instant', block: 'center' });
            return true;
          }
        }
        return false;
      });
      await page.waitForTimeout(500);
    },
    clip: { x: 0, y: 800, width: 1920, height: 500 }
  }
];

async function main() {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: { width: 1920, height: 1080 } });
  const page = await context.newPage();

  // 登录
  console.log('正在登录...');
  await page.goto(`${BASE_URL}/login`, { waitUntil: 'networkidle' });
  await page.waitForTimeout(2000);
  
  // 尝试多种方式找到并填写登录表单
  const usernameInput = await page.$('input[type="text"]') || 
                        await page.$('input#username') || 
                        await page.$('input[placeholder*="用户名"]') ||
                        await page.$('input[name="username"]');
  const passwordInput = await page.$('input[type="password"]') || 
                        await page.$('input#password') || 
                        await page.$('input[placeholder*="密码"]') ||
                        await page.$('input[name="password"]');
  
  if (usernameInput) await usernameInput.fill('admin');
  if (passwordInput) await passwordInput.fill('Lm@2025');
  
  // 尝试多种方式点击登录按钮
  const submitBtn = await page.$('button[type="submit"]') || 
                    await page.$('button:has-text("登录")') ||
                    await page.$('.ant-btn-primary') ||
                    await page.$('button');
  if (submitBtn) await submitBtn.click();
  
  await page.waitForURL(/\/(dashboard|home|monthly-dashboard|lab)/, { timeout: 30000 });
  await page.waitForTimeout(2000);

  // 创建目录
  ['数据管理', '报表分析'].forEach(dir => {
    const fullPath = path.join(SCREENSHOTS_DIR, dir);
    if (!fs.existsSync(fullPath)) fs.mkdirSync(fullPath, { recursive: true });
  });

  for (const item of screenshots) {
    console.log(`\n正在截取: ${item.name}`);
    try {
      // 导航到页面
      await page.goto(`${BASE_URL}${item.path}`, { waitUntil: 'networkidle', timeout: 30000 });
      await page.waitForTimeout(1500);

      // 执行自定义动作
      if (item.actions) {
        await item.actions(page);
      }

      // 如果有点击操作
      if (item.click) {
        const btn = await page.$(item.click);
        if (btn) {
          await btn.click();
          await page.waitForTimeout(item.wait || 1000);
        }
      }

      // 截图
      const outputPath = path.join(SCREENSHOTS_DIR, `${item.name}.png`);
      const screenshotOptions = { path: outputPath };
      if (item.clip) screenshotOptions.clip = item.clip;
      
      await page.screenshot(screenshotOptions);
      const stats = fs.statSync(outputPath);
      console.log(`  ✓ 已保存 (${(stats.size / 1024).toFixed(1)}KB)`);

    } catch (err) {
      console.log(`  ✗ 失败: ${err.message}`);
    }
  }

  await browser.close();
  console.log('\n截图完成！');
}

main().catch(console.error);
