/**
 * 扫码枪模拟器（浏览器控制台版，测试用）
 *
 * 适用场景：不方便运行 PowerShell 版（非 Windows / 无桌面焦点控制）时，
 * 在扫码工位页面（web/src/views/lab/scanStation）打开 DevTools 控制台，
 * 整段粘贴本文件后回车，即可用 scanGun.* 系列命令模拟扫码。
 *
 * 与 PowerShell 版的差异：本脚本在 DOM 层伪造键盘事件与输入值，
 * 能驱动页面逻辑（焦点拉回、pressEnter 查询），但不经过操作系统输入队列，
 * 不能复现 OS 级时序问题；做真实性最强的验证请用 PowerShell 版。
 *
 * 用法：
 *   scanGun.scanOnce('1甲20251101-1-4-1')          // 扫一条
 *   scanGun.scanBatch(['码1', '码2'], 1500)         // 批量扫，间隔 1500ms
 *   scanGun.scanBatch(scanGun.SAMPLES)              // 用内置样例批量扫
 */
(() => {
  // 内置样例：与 scan-gun-simulator.ps1 保持一致
  const SAMPLES = [
    '1甲20251101-1-4-1', // 基准炉号（匹配原始/中间表 FurnaceNoFormatted）
    '1甲20251101-1-4-1W脆', // 完整炉号（含W工艺标记+特性汉字）
    '1甲20251101-1K', // 环样/单片批次级炉号（尾部K会被剥离）
    '3乙20251203-2-1-2', // 另一产线/班次
    'TEST-NOT-EXIST-001', // 非法格式：验证未命中提示
  ];

  const sleep = (ms) => new Promise((resolve) => setTimeout(resolve, ms));

  // Vue 受控输入必须走原生 setter + input 事件，直接赋值 input.value 不会更新 v-model
  function setNativeValue(input, value) {
    const setter = Object.getOwnPropertyDescriptor(window.HTMLInputElement.prototype, 'value').set;
    setter.call(input, value);
    input.dispatchEvent(new Event('input', { bubbles: true }));
  }

  function dispatchKeydown(target, key) {
    // ant-design-vue 3.2 的 Input 以 e.keyCode===13 判定 pressEnter（不看 e.key），
    // 合成 KeyboardEvent 的 keyCode 默认为 0，必须显式补 legacy keyCode/which 才能触发查询。
    const keyCode = key === 'Enter' ? 13 : key.charCodeAt(0);
    target.dispatchEvent(
      new KeyboardEvent('keydown', { key, keyCode, which: keyCode, bubbles: true, cancelable: true })
    );
  }

  function findScanInput() {
    return document.querySelector('.scan-input input');
  }

  // 等待输入框从禁用态（查询在途 :disabled=traceLoading）恢复可用，最多 maxWaitMs。
  // 真实扫码枪在输入框禁用时击键会被丢弃；这里改为"等操作员般"等待恢复，避免批量扫码丢码。
  async function waitInputEnabled(input, maxWaitMs = 8000) {
    const step = 50;
    let waited = 0;
    while (input.disabled && waited < maxWaitMs) {
      await sleep(step);
      waited += step;
    }
    return !input.disabled;
  }

  /**
   * 模拟一次扫码：
   * 1. 先向 document.body 派发首字符 keydown —— 驱动页面的"焦点拉回"逻辑；
   * 2. 逐字符 keydown + 更新输入值（模拟键盘楔子高速击键）；
   * 3. 最后派发 Enter keydown 触发 ant-design-vue 的 pressEnter。
   */
  async function scanOnce(code, { charDelay = 10 } = {}) {
    if (!code) return;

    dispatchKeydown(document.body, code[0]);
    await sleep(30); // 页面 nextTick 拉回焦点

    const input = findScanInput();
    if (!input) {
      console.error('[scanGun] 未找到扫码输入框（.scan-input input），请确认当前在"扫码追溯"页签。');
      return;
    }
    if (input.disabled && !(await waitInputEnabled(input))) {
      console.warn('[scanGun] 输入框长时间处于禁用态（上一次查询未返回？），已跳过本条:', code);
      return;
    }
    input.focus();

    let acc = '';
    for (const ch of code) {
      dispatchKeydown(input, ch);
      acc += ch;
      setNativeValue(input, acc);
      if (charDelay > 0) await sleep(charDelay);
    }
    dispatchKeydown(input, 'Enter');
    console.log(`[scanGun] 已扫描: ${code}`);
  }

  /** 批量扫码，interval 为两次扫码间隔（毫秒） */
  async function scanBatch(codes, interval = 1500, options = {}) {
    for (let i = 0; i < codes.length; i++) {
      await scanOnce(codes[i], options);
      if (i < codes.length - 1) await sleep(interval);
    }
    console.log(`[scanGun] 批量扫描完成，共 ${codes.length} 条。`);
  }

  window.scanGun = { scanOnce, scanBatch, SAMPLES };
  console.log(
    '[scanGun] 扫码枪模拟器已加载：\n' +
      "  scanGun.scanOnce('码值')            扫一条\n" +
      '  scanGun.scanBatch(scanGun.SAMPLES)  内置样例批量扫\n' +
      '  scanGun.SAMPLES                     查看内置样例'
  );
})();
