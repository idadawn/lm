#Requires -Version 5.1

$ErrorActionPreference = "Stop"
$ScriptDir = $PSScriptRoot
. (Join-Path $ScriptDir "_dotenv.ps1")

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
[System.Windows.Forms.Application]::EnableVisualStyles()

$script:Busy = $false
$script:CurrentProcess = $null
$script:EnvMap = @{}

function Reload-Env {
    $script:EnvMap = Get-DotEnv -Path (Join-Path $ScriptDir ".env")
}

function EnvVal($key, $default = "") {
    return Coalesce-EnvValue -ParamValue "" -EnvMap $script:EnvMap -Key $key -Default $default
}

function Get-DeployRoot {
    $raw = EnvVal "DEPLOY_ROOT" "D:\deploy\lab"
    if (-not (Test-Path $raw)) { New-Item -ItemType Directory -Force -Path $raw | Out-Null }
    return (Resolve-Path $raw).Path
}

function Test-IsAdmin {
    return ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
}

function Test-TcpPort {
    param([int]$Port)
    try {
        $client = New-Object Net.Sockets.TcpClient
        $async = $client.BeginConnect("127.0.0.1", $Port, $null, $null)
        if (-not $async.AsyncWaitHandle.WaitOne(700, $false)) {
            $client.Close()
            return $false
        }
        $client.EndConnect($async)
        $client.Close()
        return $true
    }
    catch {
        return $false
    }
}

function Get-ServiceDefinitions {
    return @(
        [pscustomobject]@{ Name = "lm-web";       Label = "Web / nginx"; PortKey = "WEB_PORT";                 DefaultPort = 80;    Log = "lm-web" },
        [pscustomobject]@{ Name = "lm-api";       Label = ".NET API";    PortKey = "API_PORT";                 DefaultPort = 10089; Log = "lm-api" },
        [pscustomobject]@{ Name = "lm-nlq-agent"; Label = "NLQ Agent";   PortKey = "NLQ_PORT";                 DefaultPort = 8000;  Log = "lm-nlq-agent" },
        [pscustomobject]@{ Name = "lm-redis";     Label = "Redis";       PortKey = "REDIS_PORT";               DefaultPort = 6380;  Log = "lm-redis" },
        [pscustomobject]@{ Name = "lm-rabbitmq";  Label = "RabbitMQ";    PortKey = "RABBITMQ_PORT";            DefaultPort = 8005;  Log = "lm-rabbitmq" },
        [pscustomobject]@{ Name = "MySQL80";      Label = "MySQL";       PortKey = "DB_PORT";                  DefaultPort = 3306;  Log = "" },
        [pscustomobject]@{ Name = "rabbitmq-mgmt";Label = "RabbitMQ UI"; PortKey = "RABBITMQ_MANAGEMENT_PORT"; DefaultPort = 15672; Log = "lm-rabbitmq" }
    )
}

function Append-Log {
    param([string]$Message)
    if (-not $script:LogBox) { return }
    if ($script:LogBox.InvokeRequired) {
        $script:LogBox.BeginInvoke([Action[string]]{ param($m) Append-Log $m }, $Message) | Out-Null
        return
    }
    $script:LogBox.AppendText(("[{0}] {1}`r`n" -f (Get-Date -Format "HH:mm:ss"), $Message))
    $script:LogBox.SelectionStart = $script:LogBox.TextLength
    $script:LogBox.ScrollToCaret()
}

function Set-Busy {
    param([bool]$Busy)
    $script:Busy = $Busy
    foreach ($btn in $script:ActionButtons) { $btn.Enabled = -not $Busy }
    $script:BtnRefresh.Enabled = $true
    $script:StatusLabel.Text = if ($Busy) { "Running task..." } else { "Ready" }
}

function Get-SelectedServiceNames {
    $names = @()
    foreach ($item in $script:ServiceChecks.CheckedItems) {
        $name = ($item -split "\s+")[0]
        if ($name -ne "rabbitmq-mgmt") { $names += $name }
    }
    return $names
}

function Refresh-ServiceGrid {
    Reload-Env
    $script:Grid.Rows.Clear()
    foreach ($def in Get-ServiceDefinitions) {
        $port = [int](EnvVal $def.PortKey ([string]$def.DefaultPort))
        $svc = $null
        if ($def.Name -ne "rabbitmq-mgmt") {
            $svc = Get-CimInstance Win32_Service -Filter ("Name='{0}'" -f $def.Name) -ErrorAction SilentlyContinue
        }
        $status = if ($svc) { $svc.State } elseif ($def.Name -eq "rabbitmq-mgmt") { "PortOnly" } else { "NotInstalled" }
        $startMode = if ($svc) { $svc.StartMode } else { "" }
        $pid = if ($svc -and $svc.ProcessId -gt 0) { [string]$svc.ProcessId } else { "" }
        $portOk = if ($port -gt 0) { Test-TcpPort $port } else { $false }
        $rowIndex = $script:Grid.Rows.Add($def.Name, $def.Label, $status, $startMode, $pid, $port, $portOk)
        $row = $script:Grid.Rows[$rowIndex]
        if ($status -eq "Running" -or ($def.Name -eq "rabbitmq-mgmt" -and $portOk)) {
            $row.DefaultCellStyle.BackColor = [Drawing.Color]::FromArgb(230, 248, 235)
        } elseif ($status -eq "NotInstalled") {
            $row.DefaultCellStyle.BackColor = [Drawing.Color]::FromArgb(245, 245, 245)
        } else {
            $row.DefaultCellStyle.BackColor = [Drawing.Color]::FromArgb(255, 244, 220)
        }
    }
    $script:DeployRootBox.Text = Get-DeployRoot
    $script:LastRefreshLabel.Text = "Last refresh: " + (Get-Date -Format "HH:mm:ss")
}

function Start-PowerShellTask {
    param(
        [string]$Title,
        [string]$Arguments,
        [string]$WorkingDirectory = $ScriptDir
    )
    if ($script:Busy) {
        [Windows.Forms.MessageBox]::Show("A task is already running. Please wait.", "Info", "OK", "Information") | Out-Null
        return
    }
    Append-Log ""
    Append-Log ">>> $Title"
    Set-Busy $true

    $psi = New-Object Diagnostics.ProcessStartInfo
    $psi.FileName = "powershell.exe"
    $psi.WorkingDirectory = $WorkingDirectory
    $psi.Arguments = "-NoProfile -ExecutionPolicy Bypass $Arguments"
    $psi.UseShellExecute = $false
    $psi.RedirectStandardOutput = $true
    $psi.RedirectStandardError = $true
    $psi.CreateNoWindow = $true

    $proc = New-Object Diagnostics.Process
    $proc.StartInfo = $psi
    $proc.EnableRaisingEvents = $true
    $proc.add_OutputDataReceived({
        param($sender, $eventArgs)
        if ($eventArgs.Data) {
            $script:Form.BeginInvoke([Action[string]]{ param($m) Append-Log $m }, $eventArgs.Data) | Out-Null
        }
    })
    $proc.add_ErrorDataReceived({
        param($sender, $eventArgs)
        if ($eventArgs.Data) {
            $script:Form.BeginInvoke([Action[string]]{ param($m) Append-Log $m }, $eventArgs.Data) | Out-Null
        }
    })
    $proc.add_Exited({
        param($sender, $eventArgs)
        $code = $sender.ExitCode
        $script:Form.BeginInvoke([Action[int]]{
            param($exitCode)
            Append-Log ("<<< Task finished, exit code: {0}" -f $exitCode)
            Set-Busy $false
            Refresh-ServiceGrid
        }, $code) | Out-Null
    })
    $script:CurrentProcess = $proc
    [void]$proc.Start()
    $proc.BeginOutputReadLine()
    $proc.BeginErrorReadLine()
}

function Run-Script {
    param([string]$Title, [string]$ScriptName, [string]$ExtraArgs = "")
    $path = Join-Path $ScriptDir $ScriptName
    if (-not (Test-Path $path)) {
        [Windows.Forms.MessageBox]::Show("Script not found: $path", "Error", "OK", "Error") | Out-Null
        return
    }
    Start-PowerShellTask -Title $Title -Arguments ("-File `"{0}`" {1}" -f $path, $ExtraArgs)
}

function Run-ServiceCommand {
    param([string]$Action)
    $names = Get-SelectedServiceNames
    if ($names.Count -eq 0) {
        [Windows.Forms.MessageBox]::Show("Please select at least one service.", "Info", "OK", "Information") | Out-Null
        return
    }
    $arrayLiteral = ($names | ForEach-Object { "'$_'" }) -join ","
    $verb = switch ($Action) {
        "start" { "Start-Service -Name `$s -ErrorAction Stop" }
        "stop" { "Stop-Service -Name `$s -Force -ErrorAction Stop" }
        "restart" { "Restart-Service -Name `$s -Force -ErrorAction Stop" }
    }
    $command = @"
`$ErrorActionPreference='Continue'
foreach (`$s in @($arrayLiteral)) {
  try {
    Write-Host "[$Action] `$s"
    $verb
  } catch {
    Write-Host "FAILED `$s : `$($_.Exception.Message)"
  }
}
"@
    Start-PowerShellTask -Title "$Action services" -Arguments ("-EncodedCommand {0}" -f ([Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($command))))
}

function Run-HealthCheck {
    $webPort = [int](EnvVal "WEB_PORT" "80")
    $apiPort = [int](EnvVal "API_PORT" "10089")
    $nlqPort = [int](EnvVal "NLQ_PORT" "8000")
    $urls = @(
        "http://127.0.0.1:$webPort/",
        "http://127.0.0.1:$webPort/health",
        "http://127.0.0.1:$apiPort/health",
        "http://127.0.0.1:$nlqPort/api/v1/health",
        "http://127.0.0.1:$nlqPort/api/v1/ready"
    )
    Append-Log ">>> Health check"
    foreach ($url in $urls) {
        try {
            $res = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 5
            Append-Log ("{0} -> {1}" -f $url, [int]$res.StatusCode)
        }
        catch {
            Append-Log ("{0} -> FAILED: {1}" -f $url, $_.Exception.Message)
        }
    }
}

function Show-SelectedLog {
    $names = Get-SelectedServiceNames
    if ($names.Count -eq 0) { return }
    $deployRoot = Get-DeployRoot
    foreach ($name in $names) {
        $stderr = Join-Path $deployRoot "logs\$name\stderr.log"
        $stdout = Join-Path $deployRoot "logs\$name\stdout.log"
        $file = if (Test-Path $stderr) { $stderr } elseif (Test-Path $stdout) { $stdout } else { "" }
        if (-not $file) {
            Append-Log "Log not found: $name"
            continue
        }
        Append-Log ">>> tail $file"
        Get-Content -Path $file -Tail 80 -ErrorAction SilentlyContinue | ForEach-Object { Append-Log $_ }
    }
}

Reload-Env

$script:Form = New-Object Windows.Forms.Form
$script:Form.Text = "LM Deployment Console"
$script:Form.StartPosition = "CenterScreen"
$script:Form.Size = New-Object Drawing.Size(1180, 760)
$script:Form.MinimumSize = New-Object Drawing.Size(1020, 680)
$script:Form.Font = New-Object Drawing.Font("Microsoft YaHei UI", 9)

$root = New-Object Windows.Forms.TableLayoutPanel
$root.Dock = "Fill"
$root.ColumnCount = 2
$root.RowCount = 1
$root.ColumnStyles.Add((New-Object Windows.Forms.ColumnStyle("Absolute", 300))) | Out-Null
$root.ColumnStyles.Add((New-Object Windows.Forms.ColumnStyle("Percent", 100))) | Out-Null
$script:Form.Controls.Add($root)

$left = New-Object Windows.Forms.Panel
$left.Dock = "Fill"
$left.Padding = New-Object Windows.Forms.Padding(10)
$root.Controls.Add($left, 0, 0)

$right = New-Object Windows.Forms.TableLayoutPanel
$right.Dock = "Fill"
$right.RowCount = 4
$right.ColumnCount = 1
$right.RowStyles.Add((New-Object Windows.Forms.RowStyle("Absolute", 42))) | Out-Null
$right.RowStyles.Add((New-Object Windows.Forms.RowStyle("Percent", 48))) | Out-Null
$right.RowStyles.Add((New-Object Windows.Forms.RowStyle("Absolute", 32))) | Out-Null
$right.RowStyles.Add((New-Object Windows.Forms.RowStyle("Percent", 52))) | Out-Null
$root.Controls.Add($right, 1, 0)

$title = New-Object Windows.Forms.Label
$title.Text = "Deployment Console"
$title.Font = New-Object Drawing.Font("Microsoft YaHei UI", 15, [Drawing.FontStyle]::Bold)
$title.AutoSize = $true
$title.Location = New-Object Drawing.Point(10, 8)
$left.Controls.Add($title)

$adminLabel = New-Object Windows.Forms.Label
$adminLabel.AutoSize = $false
$adminLabel.Width = 260
$adminLabel.Height = 38
$adminLabel.Location = New-Object Drawing.Point(10, 44)
$adminLabel.Text = if (Test-IsAdmin) { "Administrator: yes" } else { "Administrator: no (install/control may fail)" }
$adminLabel.ForeColor = if (Test-IsAdmin) { [Drawing.Color]::DarkGreen } else { [Drawing.Color]::DarkRed }
$left.Controls.Add($adminLabel)

$deployLabel = New-Object Windows.Forms.Label
$deployLabel.Text = "DEPLOY_ROOT"
$deployLabel.AutoSize = $true
$deployLabel.Location = New-Object Drawing.Point(10, 88)
$left.Controls.Add($deployLabel)

$script:DeployRootBox = New-Object Windows.Forms.TextBox
$script:DeployRootBox.Location = New-Object Drawing.Point(10, 110)
$script:DeployRootBox.Size = New-Object Drawing.Size(260, 24)
$script:DeployRootBox.ReadOnly = $true
$left.Controls.Add($script:DeployRootBox)

$serviceLabel = New-Object Windows.Forms.Label
$serviceLabel.Text = "Services"
$serviceLabel.AutoSize = $true
$serviceLabel.Location = New-Object Drawing.Point(10, 146)
$left.Controls.Add($serviceLabel)

$script:ServiceChecks = New-Object Windows.Forms.CheckedListBox
$script:ServiceChecks.Location = New-Object Drawing.Point(10, 168)
$script:ServiceChecks.Size = New-Object Drawing.Size(260, 126)
$script:ServiceChecks.CheckOnClick = $true
foreach ($def in Get-ServiceDefinitions | Where-Object { $_.Name -ne "rabbitmq-mgmt" }) {
    [void]$script:ServiceChecks.Items.Add(("{0}  {1}" -f $def.Name, $def.Label), $true)
}
$left.Controls.Add($script:ServiceChecks)

function New-Button {
    param([string]$Text, [int]$X, [int]$Y, [scriptblock]$OnClick)
    $btn = New-Object Windows.Forms.Button
    $btn.Text = $Text
    $btn.Location = New-Object Drawing.Point($X, $Y)
    $btn.Size = New-Object Drawing.Size(125, 32)
    $btn.Add_Click($OnClick)
    $left.Controls.Add($btn)
    return $btn
}

$script:ActionButtons = @()
$script:ActionButtons += New-Button "Download prereq" 10 310 { Run-Script "Download prerequisites" "download-prereqs.ps1" }
$script:ActionButtons += New-Button "Render config" 145 310 { Run-Script "Render config" "render-config.ps1" }
$script:ActionButtons += New-Button "Install infra" 10 350 { Run-Script "Install infrastructure" "install-infra.ps1" }
$script:ActionButtons += New-Button "Install apps" 145 350 { Run-Script "Install application services" "install-services.ps1" "-SkipBuild" }
$script:ActionButtons += New-Button "Deploy all" 10 390 { Run-Script "Deploy all" "deploy-all.ps1" }
$script:ActionButtons += New-Button "Online upgrade" 145 390 { Run-Script "Online upgrade" "upgrade.ps1" }
$script:ActionButtons += New-Button "Start selected" 10 440 { Run-ServiceCommand "start" }
$script:ActionButtons += New-Button "Stop selected" 145 440 { Run-ServiceCommand "stop" }
$script:ActionButtons += New-Button "Restart selected" 10 480 { Run-ServiceCommand "restart" }
$script:BtnRefresh = New-Button "Refresh" 145 480 { Refresh-ServiceGrid }
$script:ActionButtons += New-Button "Health check" 10 520 { Run-HealthCheck }
$script:ActionButtons += New-Button "Tail logs" 145 520 { Show-SelectedLog }
$script:ActionButtons += New-Button "Open site" 10 560 {
    $port = [int](EnvVal "WEB_PORT" "80")
    Start-Process "http://127.0.0.1:$port/"
}
$script:ActionButtons += New-Button "Open log dir" 145 560 {
    $dir = Join-Path (Get-DeployRoot) "logs"
    New-Item -ItemType Directory -Force -Path $dir | Out-Null
    Start-Process explorer.exe $dir
}

$autoRefresh = New-Object Windows.Forms.CheckBox
$autoRefresh.Text = "Auto refresh"
$autoRefresh.Location = New-Object Drawing.Point(10, 610)
$autoRefresh.Size = New-Object Drawing.Size(140, 24)
$autoRefresh.Checked = $true
$left.Controls.Add($autoRefresh)

$script:StatusLabel = New-Object Windows.Forms.Label
$script:StatusLabel.Text = "Ready"
$script:StatusLabel.Location = New-Object Drawing.Point(10, 642)
$script:StatusLabel.Size = New-Object Drawing.Size(260, 24)
$left.Controls.Add($script:StatusLabel)

$topBar = New-Object Windows.Forms.Panel
$topBar.Dock = "Fill"
$right.Controls.Add($topBar, 0, 0)

$gridTitle = New-Object Windows.Forms.Label
$gridTitle.Text = "Service status"
$gridTitle.Font = New-Object Drawing.Font("Microsoft YaHei UI", 12, [Drawing.FontStyle]::Bold)
$gridTitle.AutoSize = $true
$gridTitle.Location = New-Object Drawing.Point(4, 10)
$topBar.Controls.Add($gridTitle)

$script:LastRefreshLabel = New-Object Windows.Forms.Label
$script:LastRefreshLabel.Text = ""
$script:LastRefreshLabel.AutoSize = $true
$script:LastRefreshLabel.Location = New-Object Drawing.Point(130, 13)
$topBar.Controls.Add($script:LastRefreshLabel)

$script:Grid = New-Object Windows.Forms.DataGridView
$script:Grid.Dock = "Fill"
$script:Grid.AllowUserToAddRows = $false
$script:Grid.AllowUserToDeleteRows = $false
$script:Grid.ReadOnly = $true
$script:Grid.RowHeadersVisible = $false
$script:Grid.SelectionMode = "FullRowSelect"
$script:Grid.AutoSizeColumnsMode = "Fill"
$script:Grid.Columns.Add("Name", "Service") | Out-Null
$script:Grid.Columns.Add("Label", "Description") | Out-Null
$script:Grid.Columns.Add("Status", "Status") | Out-Null
$script:Grid.Columns.Add("StartMode", "Start mode") | Out-Null
$script:Grid.Columns.Add("PID", "PID") | Out-Null
$script:Grid.Columns.Add("Port", "Port") | Out-Null
$script:Grid.Columns.Add("PortOpen", "Port open") | Out-Null
$right.Controls.Add($script:Grid, 0, 1)

$logTitle = New-Object Windows.Forms.Label
$logTitle.Text = "Output"
$logTitle.Font = New-Object Drawing.Font("Microsoft YaHei UI", 10, [Drawing.FontStyle]::Bold)
$logTitle.Dock = "Fill"
$right.Controls.Add($logTitle, 0, 2)

$script:LogBox = New-Object Windows.Forms.TextBox
$script:LogBox.Multiline = $true
$script:LogBox.ScrollBars = "Both"
$script:LogBox.WordWrap = $false
$script:LogBox.ReadOnly = $true
$script:LogBox.Dock = "Fill"
$script:LogBox.Font = New-Object Drawing.Font("Consolas", 9)
$right.Controls.Add($script:LogBox, 0, 3)

$timer = New-Object Windows.Forms.Timer
$timer.Interval = 5000
$timer.Add_Tick({
    if ($autoRefresh.Checked -and -not $script:Busy) { Refresh-ServiceGrid }
})
$timer.Start()

$script:Form.Add_Shown({
    Refresh-ServiceGrid
    Append-Log "GUI ready. Use lm-deploy-gui.cmd to start as Administrator."
})
$script:Form.Add_FormClosing({
    if ($script:CurrentProcess -and -not $script:CurrentProcess.HasExited) {
        $answer = [Windows.Forms.MessageBox]::Show("A task is still running. Close anyway?", "Confirm", "YesNo", "Warning")
        if ($answer -ne "Yes") { $_.Cancel = $true }
    }
})

[void][Windows.Forms.Application]::Run($script:Form)
