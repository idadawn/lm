const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');

const SCREENSHOTS_DIR = './docs/screenshots';
const BASE_URL = 'http://47.105.59.151:8928';

async function main() {
  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: { width: 1920, height: 1080 } });
  const page = await context.newPage();

  // 登录
  console.log('正在登录...');
  await page.goto(`${BASE_URL}/login`, { waitUntil: 'domcontentloaded' });
  await page.waitForTimeout(2000);
  
  const usernameInput = await page.$('input[type="text"]');
  const passwordInput = await page.$('input[type="password"]');
  if (usernameInput) await usernameInput.fill('admin');
  if (passwordInput) await passwordInput.fill('Lm@2025');
  
  const submitBtn = await page.$('button[type="submit"]') || await page.$('button');
  if (submitBtn) await submitBtn.click();
  
  await page.waitForURL(/\/(dashboard|home|monthly-dashboard|lab)/, { timeout: 30000 });
  await page.waitForTimeout(2000);

  // 进入导入模板页面
  await page.goto(`${BASE_URL}/lab/template`, { waitUntil: 'networkidle', timeout: 30000 });
  await page.waitForTimeout(3000);

  // 点击第二个卡片的编辑按钮（检测数据导入模板）
  console.log('\n正在截取: Excel导入模板-编辑弹窗');
  try {
    // 先截图当前状态（主界面）
    await page.screenshot({ 
      path: path.join(SCREENSHOTS_DIR, '基础管理', '03-26-Excel导入模板-主界面.png')
    });
    console.log('  ✓ 主界面已保存');
    
    // 使用JavaScript点击编辑按钮
    await page.evaluate(() => {
      const editBtns = document.querySelectorAll('button');
      for (let btn of editBtns) {
        if (btn.textContent.includes('编辑')) {
          btn.click();
          return true;
        }
      }
      return false;
    });
    await page.waitForTimeout(2000);
    
    // 等待弹窗出现
    await page.waitForSelector('.ant-modal, .ant-modal-content, [class*="modal"]', { timeout: 10000 });
    await page.waitForTimeout(1000);
    
    // 截图编辑弹窗
    await page.screenshot({ 
      path: path.join(SCREENSHOTS_DIR, '基础管理', '03-27-Excel导入模板-编辑弹窗.png')
    });
    console.log('  ✓ 编辑弹窗已保存');
    
    // 滚动到字段映射表格并截图
    console.log('\n正在截取: Excel导入模板-字段映射');
    await page.evaluate(() => {
      const tables = document.querySelectorAll('table');
      for (let table of tables) {
        const headers = table.querySelectorAll('th');
        for (let th of headers) {
          if (th.textContent.includes('字段名称')) {
            table.scrollIntoView({ behavior: 'instant', block: 'start' });
            return true;
          }
        }
      }
      window.scrollBy(0, 350);
      return false;
    });
    await page.waitForTimeout(800);
    
    await page.screenshot({ 
      path: path.join(SCREENSHOTS_DIR, '基础管理', '03-28-Excel导入模板-字段映射.png')
    });
    console.log('  ✓ 字段映射已保存');
    
  } catch (err) {
    console.log(`  ✗ 失败: ${err.message}`);
    // 保存当前页面状态以便调试
    await page.screenshot({ 
      path: path.join(SCREENSHOTS_DIR, '基础管理', 'error-debug.png')
    });
  }

  await browser.close();
  console.log('\n截图完成！');
}

main().catch(console.error);
