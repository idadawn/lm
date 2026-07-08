# 扫码枪模拟器（测试用）

无需实体扫码枪，模拟"USB 键盘楔子"式扫码枪的输入行为，用于测试扫码工位页面
（`web/src/views/lab/scanStation/index.vue`，路由：检测室 > 扫码工位 `/lab/scan-station`）
及其它需要扫码输入的场景。

## 背景：真实扫码枪如何工作

车间常用的 USB 扫码枪本质是一个 **HID 键盘设备**（键盘楔子 / keyboard wedge）：

- 扫到二维码后，把码值内容**逐字符高速"敲键"**输出（每字符约 1~10ms）；
- 输出完毕后追加**后缀键**，出厂默认通常是回车（Enter），也可配置为 Tab 或无后缀；
- 焦点在哪个输入框，内容就打进哪里——这正是页面需要"焦点拉回"逻辑的原因。

本项目二维码内容 = 炉号纯文本，格式（见 `FurnaceNo.cs`）：

```
[产线数字][班次汉字][8位日期]-[炉次号]-[卷号]-[分卷号][可选W][可选特性汉字]
示例：1甲20251101-1-4-1W脆
环样/单片批次级：1甲20251101-1K（尾部K会被后端归一化剥离）
```

> 注意：炉号含汉字。若使用实体扫码枪，需确认其支持中文输出
> （通常需设置为 UTF-8 编码 + 中文输出模式，或走串口/SPP 模式）。

## 工具一：PowerShell 版（推荐，真实性最强）

OS 级按键注入（Win32 `SendInput` + `KEYEVENTF_UNICODE`），支持汉字、不依赖输入法，
时序与真实扫码枪一致，可复现焦点相关的真实问题。仅限 Windows。

```powershell
# 用内置样例逐条扫描（倒计时内把焦点切到浏览器的扫码工位页）
.\scripts\scan-gun-simulator\scan-gun-simulator.ps1

# 扫描指定炉号
.\scripts\scan-gun-simulator\scan-gun-simulator.ps1 -Code "1甲20251101-1-4-1"

# 从文件批量扫描（每行一条，# 开头为注释），循环 3 轮，模拟高频作业
.\scripts\scan-gun-simulator\scan-gun-simulator.ps1 -File .\codes.txt -Repeat 3 -ScanIntervalMs 800

# 查看内置样例说明
.\scripts\scan-gun-simulator\scan-gun-simulator.ps1 -ListSamples
```

主要参数：

| 参数 | 默认 | 说明 |
|---|---|---|
| `-Code` | 内置样例 | 一个或多个码值 |
| `-File` | — | 从文件读码值（UTF-8，每行一条） |
| `-CountdownSeconds` | 3 | 切焦点倒计时 |
| `-CharDelayMs` | 10 | 逐字符击键间隔（真实枪约 1~10ms） |
| `-ScanIntervalMs` | 1500 | 两次扫码间隔 |
| `-Suffix` | Enter | 后缀键：Enter / Tab / None |
| `-Repeat` | 1 | 整组循环次数 |

## 工具二：浏览器控制台版（跨平台，快速验证）

在扫码工位页面打开 DevTools 控制台，把 `scan-gun-simulator.browser.js` 整段粘贴回车：

```js
scanGun.scanOnce('1甲20251101-1-4-1'); // 扫一条
scanGun.scanBatch(scanGun.SAMPLES);    // 内置样例批量扫
```

DOM 层伪造键盘事件 + 输入值，能驱动页面逻辑（焦点拉回、pressEnter 查询），
但不经过操作系统输入队列；做最终验收请用 PowerShell 版。

## 建议测试场景

| 场景 | 操作 | 预期 |
|---|---|---|
| 正常扫码命中 | 用数据库中真实存在的炉号扫描 | 展示基本信息/叠片/判定/环样/单片卡片，历史标"命中" |
| 未命中 | 扫 `TEST-NOT-EXIST-001` | 空态提示 + 历史标"未命中" |
| 焦点不在输入框 | 先点击页面空白处再扫 | 焦点被拉回；注意观察**首字符是否丢失**（真实枪高速击键时首键在焦点切换前落入旧焦点） |
| 连续高频扫码 | `-Repeat 3 -ScanIntervalMs 300` | 每条都触发查询、历史不错乱；注意查询在途时输入框会 `:disabled=traceLoading` 禁用，此窗口内到达的击键（PowerShell/真实枪）会被丢弃或截半——这是与真实枪一致的已知行为，观察是否出现残码误查（浏览器版会等待禁用解除再输入，不复现此丢码） |
| 带尾缀格式 | 扫 `1甲20251101-1K`、`...W脆` | 归一化正确（K/W/特性剥离），能命中批次级数据 |
| 后缀非回车的枪 | `-Suffix Tab` / `-Suffix None` | 页面行为符合预期（当前实现依赖回车触发，应无查询） |

> 命中场景的炉号请从数据库取真实值：扫码工位"标签打印"页签查询列表即为
> `LAB_RAW_DATA` 表数据，`furnaceNoFormatted`（列名 `F_FURNACE_NO_FORMATTED`）即二维码内容。

## 已知边界

- PowerShell 版通过 `KEYEVENTF_UNICODE` 注入，目标程序收到 `VK_PACKET`。
  Chrome/Edge 会正确产生对应字符的 keydown 与输入；个别老旧程序可能不识别。
- 仅支持基本多文种平面（BMP）字符，炉号用到的汉字均在范围内。
- 注入目标 = 当前前台焦点窗口，倒计时后请勿再动键盘鼠标，避免码值打进别的窗口。
