#Requires -Version 5.1
<#
.SYNOPSIS
    扫码枪模拟器（测试用）：以真实 USB 扫码枪的"键盘楔子"方式向当前焦点窗口注入扫码内容。
.DESCRIPTION
    真实 USB 扫码枪 = HID 键盘设备：扫码后逐字符高速"敲键"（每字符 1~10ms），
    最后追加后缀键（通常为回车）。本脚本用 Win32 SendInput + KEYEVENTF_UNICODE
    复现该行为，因此：
      - 支持炉号中的汉字（班次"甲/乙/丙"、特性汉字如"脆"），不依赖输入法；
      - 时序与真实扫码枪一致，可复现"焦点不在输入框时首字符丢失"等真实问题；
      - 对浏览器页面（扫码工位 web/src/views/lab/scanStation）和任意桌面程序均有效。

    用法：运行脚本 → 倒计时内把焦点切到被测页面（点击浏览器窗口）→ 自动逐条"扫描"。
.PARAMETER Code
    要扫描的一个或多个码值。省略时使用内置样例（覆盖各种炉号格式 + 未命中场景）。
.PARAMETER File
    从文本文件读取码值（每行一条，忽略空行和 # 开头的注释行）。
.PARAMETER CountdownSeconds
    倒计时秒数，留给操作者切换焦点窗口。默认 3。
.PARAMETER CharDelayMs
    逐字符击键间隔（毫秒）。真实扫码枪约 1~10ms。默认 10。
.PARAMETER ScanIntervalMs
    两次扫码之间的间隔（毫秒）。默认 1500。
.PARAMETER Suffix
    每条码值后追加的后缀键：Enter（默认，绝大多数扫码枪出厂配置）/ Tab / None。
.PARAMETER Repeat
    整组码值循环次数。默认 1。
.PARAMETER ListSamples
    仅打印内置样例码值和说明，不执行扫描。
.EXAMPLE
    .\scan-gun-simulator.ps1
    用内置样例逐条模拟扫码（先切焦点到扫码工位页面）。
.EXAMPLE
    .\scan-gun-simulator.ps1 -Code "1甲20251101-1-4-1"
    模拟扫描单个炉号。
.EXAMPLE
    .\scan-gun-simulator.ps1 -File .\codes.txt -CharDelayMs 5 -ScanIntervalMs 800 -Repeat 3
    从文件批量扫码，循环 3 轮，模拟高频作业。
#>
param(
    [Parameter(Position = 0)]
    [string[]]$Code,
    [string]$File,
    [int]$CountdownSeconds = 3,
    [ValidateRange(0, 1000)]
    [int]$CharDelayMs = 10,
    [ValidateRange(0, 60000)]
    [int]$ScanIntervalMs = 1500,
    [ValidateSet("Enter", "Tab", "None")]
    [string]$Suffix = "Enter",
    [ValidateRange(1, 1000)]
    [int]$Repeat = 1,
    [switch]$ListSamples
)

$ErrorActionPreference = "Stop"

# 内置样例：覆盖炉号各粒度格式（格式定义见 api/.../Poxiao.Lab.Entity/Models/FurnaceNo.cs）
# 注意：是否"命中"取决于数据库中实际存在的炉号，样例仅保证格式合法可被归一化。
$SampleCodes = @(
    @{ Code = "1甲20251101-1-4-1";     Note = "基准炉号（匹配原始/中间表 FurnaceNoFormatted）" },
    @{ Code = "1甲20251101-1-4-1W脆";  Note = "完整炉号（含W工艺标记+特性汉字，归一化后剥离）" },
    @{ Code = "1甲20251101-1K";        Note = "环样/单片批次级炉号（尾部K会被剥离）" },
    @{ Code = "3乙20251203-2-1-2";     Note = "另一产线/班次的基准炉号" },
    @{ Code = "TEST-NOT-EXIST-001";    Note = "非法格式：验证未命中提示与扫描历史标红" }
)

if ($ListSamples) {
    Write-Host "内置样例码值：" -ForegroundColor Cyan
    foreach ($s in $SampleCodes) {
        Write-Host ("  {0,-24} {1}" -f $s.Code, $s.Note)
    }
    exit 0
}

# ── 组装待扫码值列表 ─────────────────────────────────────────
$codes = @()
if ($File) {
    if (-not (Test-Path $File)) { throw "文件不存在：$File" }
    $codes = @(Get-Content -Path $File -Encoding UTF8 | ForEach-Object { $_.Trim() } |
        Where-Object { $_ -and -not $_.StartsWith("#") })
    if ($codes.Count -eq 0) { throw "文件中没有有效码值：$File" }
} elseif ($PSBoundParameters.ContainsKey('Code')) {
    # 用 ContainsKey 而非 `$Code -and`：单元素数组 @("") 的真值取决于该元素，
    # 会让 -Code "" 静默落入内置样例分支；显式判定后空串由下面的 throw 兜住。
    $codes = @($Code | Where-Object { $_ -and $_.Trim() } | ForEach-Object { $_.Trim() })
    if ($codes.Count -eq 0) { throw "-Code 中没有有效码值。" }
} else {
    $codes = @($SampleCodes | ForEach-Object { $_.Code })
    Write-Host "未指定 -Code / -File，使用内置样例（-ListSamples 查看说明）。" -ForegroundColor DarkGray
}

# ── SendInput P/Invoke（KEYEVENTF_UNICODE 支持汉字，不经过输入法） ──
if (-not ("ScanGunInput" -as [type])) {
    Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;

public static class ScanGunInput
{
    [StructLayout(LayoutKind.Sequential)]
    struct INPUT { public uint type; public InputUnion U; }

    // 联合体必须包含 MOUSEINPUT 以保证 x64 下结构体大小正确
    [StructLayout(LayoutKind.Explicit)]
    struct InputUnion
    {
        [FieldOffset(0)] public KEYBDINPUT ki;
        [FieldOffset(0)] public MOUSEINPUT mi;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MOUSEINPUT { public int dx; public int dy; public uint mouseData; public uint dwFlags; public uint time; public IntPtr dwExtraInfo; }

    [StructLayout(LayoutKind.Sequential)]
    struct KEYBDINPUT { public ushort wVk; public ushort wScan; public uint dwFlags; public uint time; public IntPtr dwExtraInfo; }

    const uint INPUT_KEYBOARD = 1;
    const uint KEYEVENTF_KEYUP = 0x0002;
    const uint KEYEVENTF_UNICODE = 0x0004;

    [DllImport("user32.dll", SetLastError = true)]
    static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    static INPUT KeyInput(ushort vk, ushort scan, uint flags)
    {
        var input = new INPUT { type = INPUT_KEYBOARD };
        input.U.ki = new KEYBDINPUT { wVk = vk, wScan = scan, dwFlags = flags, time = 0, dwExtraInfo = IntPtr.Zero };
        return input;
    }

    static void Send(INPUT[] inputs)
    {
        if (SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT))) != (uint)inputs.Length)
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
    }

    // 以 Unicode 方式发送单个字符（KEYEVENTF_UNICODE：目标程序收到 VK_PACKET，浏览器解析为对应字符）
    public static void SendChar(char ch)
    {
        Send(new INPUT[]
        {
            KeyInput(0, ch, KEYEVENTF_UNICODE),
            KeyInput(0, ch, KEYEVENTF_UNICODE | KEYEVENTF_KEYUP)
        });
    }

    // 发送虚拟键（回车 0x0D / Tab 0x09 等控制键必须走虚拟键，浏览器才会触发 pressEnter）
    public static void SendVirtualKey(ushort vk)
    {
        Send(new INPUT[]
        {
            KeyInput(vk, 0, 0),
            KeyInput(vk, 0, KEYEVENTF_KEYUP)
        });
    }
}
"@
}

function Send-OneScan([string]$value) {
    foreach ($ch in $value.ToCharArray()) {
        [ScanGunInput]::SendChar($ch)
        if ($CharDelayMs -gt 0) { Start-Sleep -Milliseconds $CharDelayMs }
    }
    switch ($Suffix) {
        "Enter" { [ScanGunInput]::SendVirtualKey(0x0D) }
        "Tab"   { [ScanGunInput]::SendVirtualKey(0x09) }
        "None"  { }
    }
}

# ── 执行 ─────────────────────────────────────────────────────
$totalScans = $codes.Count * $Repeat
Write-Host ""
Write-Host "扫码枪模拟器" -ForegroundColor Cyan
Write-Host ("  码值 {0} 条 x {1} 轮 = {2} 次扫描 | 字符间隔 {3}ms | 扫码间隔 {4}ms | 后缀 {5}" -f `
    $codes.Count, $Repeat, $totalScans, $CharDelayMs, $ScanIntervalMs, $Suffix)
Write-Host ""
Write-Host ("请在倒计时内把焦点切到被测页面（如浏览器的扫码工位页）…") -ForegroundColor Yellow

for ($i = $CountdownSeconds; $i -ge 1; $i--) {
    Write-Host ("  {0} ..." -f $i) -ForegroundColor Yellow
    Start-Sleep -Seconds 1
}

$n = 0
for ($round = 1; $round -le $Repeat; $round++) {
    foreach ($c in $codes) {
        $n++
        Write-Host ("[{0}/{1}] 扫描: {2}" -f $n, $totalScans, $c) -ForegroundColor Green
        Send-OneScan $c
        if ($n -lt $totalScans -and $ScanIntervalMs -gt 0) {
            Start-Sleep -Milliseconds $ScanIntervalMs
        }
    }
}

Write-Host ""
Write-Host "完成：共发送 $totalScans 次扫描。" -ForegroundColor Cyan
