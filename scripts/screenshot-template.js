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

  // 确保目录存在
  const baseDir = path.join(SCREENSHOTS_DIR, '基础管理');
  if (!fs.existsSync(baseDir)) fs.mkdirSync(baseDir, { recursive: true });

  // 截图1: 主界面
  console.log('\n正在截取: Excel导入模板-主界面');
  try {
    await page.goto(`${BASE_URL}/lab/template`, { waitUntil: 'networkidle', timeout: 30000 });
    await page.waitForTimeout(2000);
    
    await page.screenshot({ 
      path: path.join(SCREENSHOTS_DIR, '基础管理', '03-26-Excel导入模板-主界面.png')
    });
    console.log('  ✓ 主界面已保存');
  } catch (err) {
    console.log(`  ✗ 主界面失败: ${err.message}`);
  }

  // 截图2: 编辑弹窗
  console.log('\n正在截取: Excel导入模板-编辑弹窗');
  try {
    // 等待卡片加载
    await page.waitForTimeout(2000);
    
    // 点击第一个卡片
    const card = await page.$('.bg-white.rounded-lg') ||
                 await page.$('[class*="rounded-lg"]');
    if (card) {
      // 悬停显示编辑按钮
      await card.hover();
      await page.waitForTimeout(500);
      
      // 查找编辑按钮
      const editBtn = await page.$('button:has-text("编辑")') || 
                      await page.$('text=编辑');
      if (editBtn) {
        await editBtn.click();
        await page.waitForTimeout(1500);
        
        await page.screenshot({ 
          path: path.join(SCREENSHOTS_DIR, '基础管理', '03-27-Excel导入模板-编辑弹窗.png')
        });
        console.log('  ✓ 编辑弹窗已保存');
        
        // 关闭弹窗
        await page.keyboard.press('Escape');
        await page.waitForTimeout(500);
      } else {
        console.log('  ✗ 未找到编辑按钮，尝试直接点击卡片');
        await card.click();
        await page.waitForTimeout(1500);
        
        await page.screenshot({ 
          path: path.join(SCREENSHOTS_DIR, '基础管理', '03-27-Excel导入模板-编辑弹窗.png')
        });
        console.log('  ✓ 编辑弹窗已保存（通过卡片点击）');
        
        await page.keyboard.press('Escape');
        await page.waitForTimeout(500);
      }
    } else {
      console.log('  ✗ 未找到卡片');
    }
  } catch (err) {
    console.log(`  ✗ 编辑弹窗失败: ${err.message}`);
  }

  // 截图3: 字段映射表格（重新打开弹窗并滚动到字段映射区域）
  console.log('\n正在截取: Excel导入模板-字段映射');
  try {
    const editBtn = await page.$('button:has-text("编辑")') || 
                    await page.$('.group button');
    if (editBtn) {
      await editBtn.click();
      await page.waitForTimeout(1500);
      
      // 滚动到字段映射表格
      await page.evaluate(() => {
        const tables = document.querySelectorAll('table');
        for (let table of tables) {
          if (table.textContent.includes('字段名称') || 
              table.textContent.includes('Excel列名')) {
            table.scrollIntoView({ behavior: 'instant', block: 'start' });
            return true;
          }
        }
        return false;
      });
      await page.waitForTimeout(500);
      
      await page.screenshot({ 
        path: path.join(SCREENSHOTS_DIR, '基础管理', '03-28-Excel导入模板-字段映射.png')
      });
      console.log('  ✓ 字段映射已保存');
    } else {
      console.log('  ✗ 未找到编辑按钮');
    }
  } catch (err) {
    console.log(`  ✗ 字段映射失败: ${err.message}`);
  }

  await browser.close();
  console.log('\n截图完成！');
}

main().catch(console.error);
