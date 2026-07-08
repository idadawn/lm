using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Win32;
using Drawing = System.Drawing;
using Forms = System.Windows.Forms;
using WpfCheckBox = System.Windows.Controls.CheckBox;
using WpfMessageBox = System.Windows.MessageBox;
using WpfTextBox = System.Windows.Controls.TextBox;

namespace LmDeployConsole;

public sealed record ServiceItem(
    string Name,
    string Title,
    string PortKey,
    int DefaultPort,
    string ScriptKey,
    string LogName,
    bool IsPortOnly = false);

public sealed record StoreItem(string Key, string Title);

public sealed class ServiceStatusRow : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public string Name { get; init; } = "";
    public string Title { get; init; } = "";
    public bool CanSelect { get; init; }

    private bool selected;
    public bool Selected { get => selected; set => Set(ref selected, value); }

    private string state = "";
    public string State { get => state; set => Set(ref state, value); }

    private string startMode = "";
    public string StartMode { get => startMode; set => Set(ref startMode, value); }

    private string processId = "";
    public string ProcessId { get => processId; set => Set(ref processId, value); }

    private int port;
    public int Port { get => port; set => Set(ref port, value); }

    private bool portOpen;
    public bool PortOpen
    {
        get => portOpen;
        set
        {
            if (Set(ref portOpen, value))
            {
                OnPropertyChanged(nameof(PortOpenText));
            }
        }
    }

    public string PortOpenText => PortOpen ? "是" : "否";

    private bool Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(name);
        return true;
    }

    private void OnPropertyChanged(string? name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public sealed record ScriptResult(int ExitCode, string Output);

public enum UserRole
{
    None,
    Maintainer,
    Manager
}

public partial class MainWindow : Window
{
    private const string AppSettingsKeyPath = @"Software\LmDeployConsole";
    private const string AutoStartTaskName = "lm-deploy-console-autostart";

    private readonly List<ServiceItem> services =
    [
        new("lm-web", "Web / nginx", "WEB_PORT", 80, "web", "lm-web"),
        new("lm-api", ".NET API", "API_PORT", 10089, "api", "lm-api"),
        new("lm-nlq-agent", "NLQ Agent", "NLQ_PORT", 8000, "nlq-agent", "lm-nlq-agent"),
        new("lm-redis", "Redis", "REDIS_PORT", 6380, "redis", "lm-redis"),
        new("lm-rabbitmq", "RabbitMQ", "RABBITMQ_PORT", 8005, "rabbitmq", "lm-rabbitmq"),
        new("MySQL80", "MySQL", "DB_PORT", 3306, "mysql", ""),
        new("rabbitmq-mgmt", "RabbitMQ UI", "RABBITMQ_MANAGEMENT_PORT", 15672, "rabbitmq-mgmt", "lm-rabbitmq", true)
    ];

    private readonly List<StoreItem> prereqItems =
    [
        new("nssm", "NSSM"),
        new("nginx", "nginx"),
        new("redis", "Redis"),
        new("erlang", "Erlang"),
        new("rabbitmq", "RabbitMQ")
    ];

    private readonly List<StoreItem> infraItems =
    [
        new("redis", "Redis 服务"),
        new("rabbitmq", "RabbitMQ 服务")
    ];

    private readonly List<StoreItem> appItems =
    [
        new("web", "Web 网关"),
        new("api", "后端 API"),
        new("nlq-agent", "智能体")
    ];

    private readonly ObservableCollection<ServiceStatusRow> statusRows = [];
    private readonly Dictionary<string, ServiceStatusRow> rowsByName = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, WpfTextBox> logBoxes = new(StringComparer.OrdinalIgnoreCase);
    private readonly DispatcherTimer refreshTimer = new() { Interval = TimeSpan.FromSeconds(5) };
    private readonly HttpClient httpClient = new() { Timeout = TimeSpan.FromSeconds(5) };
    private readonly List<string> startupNotices = [];
    private readonly string scriptsDir;
    private readonly string envPath;
    private Dictionary<string, string> env = new(StringComparer.OrdinalIgnoreCase);
    private Forms.NotifyIcon? trayIcon;
    private UserRole role = UserRole.None;
    private bool busy;
    private bool refreshing;
    private bool reallyExit;

    public MainWindow()
    {
        InitializeComponent();
        scriptsDir = FindScriptsDir();
        envPath = Path.Combine(scriptsDir, ".env");

        EnsureEnvFile();
        EnsureAuthCodesInEnv();
        AddChoiceBoxes(PrereqChecks, prereqItems, true);
        AddChoiceBoxes(InfraChecks, infraItems, true);
        AddChoiceBoxes(AppChecks, appItems, true);

        foreach (var item in services)
        {
            var row = new ServiceStatusRow
            {
                Name = item.Name,
                Title = item.Title,
                CanSelect = !item.IsPortOnly,
                Selected = !item.IsPortOnly,
                State = item.IsPortOnly ? "PortOnly" : "Loading",
                Port = item.DefaultPort
            };
            statusRows.Add(row);
            rowsByName[item.Name] = row;
        }
        ServiceGrid.ItemsSource = statusRows;
        BuildLogTabs();

        InitializeTrayIcon();
        LoadLocalSettings();
        ReloadEnv();
        FillSettingsFields();
        UpdateRowPorts();
        SetAdminText();
        UpdateOssStatus();
        ApplyRolePermissions();

        Loaded += async (_, _) =>
        {
            Log("部署控制台已启动。");
            foreach (var notice in startupNotices)
            {
                Log(notice);
            }

            if (role == UserRole.None)
            {
                ShowRoleDialog(startupNotices.Count > 0 ? string.Join(Environment.NewLine, startupNotices) : null);
            }

            await RefreshStatusAsync();
        };

        refreshTimer.Tick += async (_, _) =>
        {
            if (AutoRefreshBox.IsChecked == true && !busy)
            {
                await RefreshStatusAsync();
            }
        };
        refreshTimer.Start();
    }

    protected override void OnClosed(EventArgs e)
    {
        refreshTimer.Stop();
        trayIcon?.Dispose();
        httpClient.Dispose();
        base.OnClosed(e);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (!reallyExit && CloseToTrayBox.IsChecked == true)
        {
            e.Cancel = true;
            HideToTray(showTip: true);
            return;
        }

        base.OnClosing(e);
    }

    protected override void OnStateChanged(EventArgs e)
    {
        base.OnStateChanged(e);
        if (WindowState == WindowState.Minimized && TrayOnMinimizeBox.IsChecked == true)
        {
            HideToTray(showTip: false);
        }
    }

    // ──── 身份授权（管理人员/维护人员）────

    private void Authorize_Click(object sender, RoutedEventArgs e) => ShowRoleDialog();

    private void Logout_Click(object sender, RoutedEventArgs e)
    {
        role = UserRole.None;
        ApplyRolePermissions();
        Log("已退出授权。");
    }

    private void ShowRoleDialog(string? hint = null)
    {
        var dialog = new RoleAuthDialog(hint, TryAuthorizeCode);
        if (IsLoaded)
        {
            dialog.Owner = this;
        }
        else
        {
            dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        dialog.ShowDialog();
        ApplyRolePermissions();
    }

    private bool TryAuthorizeCode(string code)
    {
        ReloadEnv();
        var managerCode = EnvValue("MANAGER_AUTH_CODE", "");
        var maintainerCode = EnvValue("MAINTAINER_AUTH_CODE", "");

        if (FixedEquals(code, managerCode))
        {
            role = UserRole.Manager;
        }
        else if (FixedEquals(code, maintainerCode))
        {
            role = UserRole.Maintainer;
        }
        else
        {
            return false;
        }

        ApplyRolePermissions();
        Log($"已切换授权：{RoleName(role)}。");
        return true;
    }

    private bool RequireMaintainer()
    {
        if (role is UserRole.Maintainer or UserRole.Manager)
        {
            return true;
        }

        var choice = WpfMessageBox.Show(
            "此操作需要维护人员或管理人员授权。现在输入授权码吗？",
            "需要授权",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        if (choice == MessageBoxResult.Yes)
        {
            ShowRoleDialog();
        }

        return role is UserRole.Maintainer or UserRole.Manager;
    }

    private bool RequireManager()
    {
        if (role == UserRole.Manager)
        {
            return true;
        }

        var choice = WpfMessageBox.Show(
            "发布程序需要管理人员授权。现在输入授权码吗？",
            "需要管理人员授权",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        if (choice == MessageBoxResult.Yes)
        {
            ShowRoleDialog();
        }

        return role == UserRole.Manager;
    }

    private void ApplyRolePermissions()
    {
        var canUse = !busy && role is UserRole.Maintainer or UserRole.Manager;
        var canPublish = !busy && role == UserRole.Manager;

        InstallPanel.IsEnabled = canUse;
        UpdatePanel.IsEnabled = canUse;
        ServicePanel.IsEnabled = canUse;
        BackupPanel.IsEnabled = canUse;
        PublishPanel.Visibility = canPublish ? Visibility.Visible : Visibility.Collapsed;
        PublishPanel.IsEnabled = canPublish;

        RoleText.Text = "当前身份：" + RoleName(role);
        RoleText.Foreground = role == UserRole.None
            ? System.Windows.Media.Brushes.DarkRed
            : System.Windows.Media.Brushes.DarkGreen;
        AuthorizeButton.Content = role == UserRole.None ? "输入授权码" : "切换授权";
    }

    private static string RoleName(UserRole currentRole) => currentRole switch
    {
        UserRole.Manager => "管理人员",
        UserRole.Maintainer => "维护人员",
        _ => "未授权"
    };

    private static bool FixedEquals(string left, string right)
    {
        if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
        {
            return false;
        }

        return string.Equals(left.Trim(), right.Trim(), StringComparison.Ordinal);
    }

    private void EnsureEnvFile()
    {
        if (File.Exists(envPath))
        {
            return;
        }

        var example = Path.Combine(scriptsDir, ".env.example");
        if (File.Exists(example))
        {
            File.Copy(example, envPath, overwrite: false);
        }
    }

    private void EnsureAuthCodesInEnv()
    {
        var current = File.Exists(envPath)
            ? ReadDotEnv(envPath)
            : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var updates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(current.GetValueOrDefault("MANAGER_AUTH_CODE", "")))
        {
            updates["MANAGER_AUTH_CODE"] = GenerateAuthorizationCode("MGR");
            startupNotices.Add("首次启动已生成管理人员授权码：" + updates["MANAGER_AUTH_CODE"]);
        }

        if (string.IsNullOrWhiteSpace(current.GetValueOrDefault("MAINTAINER_AUTH_CODE", "")))
        {
            updates["MAINTAINER_AUTH_CODE"] = GenerateAuthorizationCode("MNT");
            startupNotices.Add("首次启动已生成维护人员授权码：" + updates["MAINTAINER_AUTH_CODE"]);
        }

        if (updates.Count > 0)
        {
            WriteEnvUpdates(updates);
            startupNotices.Add("授权码已写入 ops\\.env，请妥善保存。");
        }
    }

    private static string GenerateAuthorizationCode(string roleCode)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var builder = new StringBuilder("LM-");
        builder.Append(roleCode);

        for (var group = 0; group < 4; group++)
        {
            builder.Append('-');
            for (var index = 0; index < 5; index++)
            {
                builder.Append(chars[RandomNumberGenerator.GetInt32(chars.Length)]);
            }
        }

        return builder.ToString();
    }

    // ──── OSS 凭据码（在线升级/发布所需）────

    private void OssAuth_Click(object sender, RoutedEventArgs e) => ShowOssDialog();

    private bool IsOssConfigured()
    {
        return EnvValue("OSS_BUCKET", "").Length > 0
            && EnvValue("OSS_ACCESS_KEY_ID", "").Length > 0
            && EnvValue("OSS_ACCESS_KEY_SECRET", "").Length > 0;
    }

    private void UpdateOssStatus()
    {
        OssStatusText.Text = IsOssConfigured()
            ? $"OSS 凭据：已配置（{EnvValue("OSS_BUCKET", "")}）"
            : "OSS 凭据：未配置。在线更新与发布前请先配置凭据码（由供应商提供）。";
    }

    private bool EnsureOssConfigured(string action)
    {
        ReloadEnv();
        if (IsOssConfigured())
        {
            return true;
        }

        var choice = WpfMessageBox.Show(
            $"{action}需要 OSS 凭据码（由供应商提供），当前尚未配置。现在输入吗？",
            "需要 OSS 凭据",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
        if (choice == MessageBoxResult.Yes)
        {
            ShowOssDialog();
        }

        ReloadEnv();
        return IsOssConfigured();
    }

    private void ShowOssDialog()
    {
        ReloadEnv();
        var configured = IsOssConfigured();
        var status = configured
            ? $"当前状态：已配置（Bucket：{EnvValue("OSS_BUCKET", "")}）"
            : "当前状态：未配置";
        var currentCode = configured
            ? UpgradeAuthCode.Encode(new UpgradeAuth(
                EnvValue("OSS_ENDPOINT", ""),
                EnvValue("OSS_BUCKET", ""),
                EnvValue("OSS_ACCESS_KEY_ID", ""),
                EnvValue("OSS_ACCESS_KEY_SECRET", ""),
                EnvValue("OSS_PREFIX", "")))
            : null;

        var dialog = new AuthDialog(status, currentCode);
        if (IsLoaded)
        {
            dialog.Owner = this;
        }
        else
        {
            dialog.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        if (dialog.ShowDialog() == true && dialog.Result is { } auth)
        {
            try
            {
                var updates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["OSS_BUCKET"] = auth.Bucket,
                    ["OSS_ACCESS_KEY_ID"] = auth.AccessKeyId,
                    ["OSS_ACCESS_KEY_SECRET"] = auth.AccessKeySecret
                };
                if (auth.Endpoint.Length > 0)
                {
                    updates["OSS_ENDPOINT"] = auth.Endpoint;
                }
                if (auth.Prefix.Length > 0)
                {
                    updates["OSS_PREFIX"] = auth.Prefix;
                }
                WriteEnvUpdates(updates);
                Log($"OSS 凭据码已保存（Bucket：{auth.Bucket}）。");
            }
            catch (Exception ex)
            {
                WpfMessageBox.Show("保存 OSS 凭据码失败：" + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        ReloadEnv();
        UpdateOssStatus();
    }

    // ──── 安装 ────

    private async void SaveSettings_Click(object sender, RoutedEventArgs e)
    {
        if (!RequireMaintainer())
        {
            return;
        }

        if (!SaveSettingsToEnv())
        {
            return;
        }

        Log("部署设置已保存。");
        if (FirewallBox.IsChecked == true)
        {
            await RunScriptAsync("放行 Web 端口", "configure-firewall.ps1");
        }
    }

    private async void DownloadSelectedPrereqs_Click(object sender, RoutedEventArgs e)
    {
        if (!RequireMaintainer() || !SaveSettingsToEnv())
        {
            return;
        }

        var selected = SelectedTags(PrereqChecks);
        if (selected.Length == 0)
        {
            WpfMessageBox.Show("请先勾选要下载的基础软件。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (string.Equals(EnvValue("PREREQ_SOURCE", "OSS"), "OSS", StringComparison.OrdinalIgnoreCase) && !IsOssConfigured())
        {
            Log("提示：未配置 OSS 凭据，基础软件将回退到公网下载地址。");
        }

        await RunScriptAsync("下载基础软件", "download-prereqs.ps1", "-Only " + string.Join(",", selected));
    }

    private async void InstallSelected_Click(object sender, RoutedEventArgs e)
    {
        if (!RequireMaintainer() || !SaveSettingsToEnv())
        {
            return;
        }

        var infra = SelectedTags(InfraChecks);
        var apps = SelectedTags(AppChecks);
        if (infra.Length == 0 && apps.Length == 0)
        {
            WpfMessageBox.Show("请先勾选要安装的服务。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (FirewallBox.IsChecked == true)
        {
            await RunScriptAsync("放行 Web 端口", "configure-firewall.ps1");
        }

        if (infra.Length > 0)
        {
            await RunScriptAsync("安装基础服务", "install-infra.ps1", "-Only " + string.Join(",", infra));
        }

        if (apps.Length > 0)
        {
            await RunScriptAsync("安装应用服务", "install-services.ps1", "-Only " + string.Join(",", apps) + " -SkipBuild");
        }
    }

    // ──── 更新与发布 ────

    private async void RunUpgrade_Click(object sender, RoutedEventArgs e)
    {
        if (!RequireMaintainer() || !SaveSettingsToEnv() || !EnsureOssConfigured("在线更新"))
        {
            return;
        }

        await RunScriptAsync("检查并更新", "upgrade.ps1");
    }

    private async void InstallAutoUpgrade_Click(object sender, RoutedEventArgs e)
    {
        if (!RequireMaintainer() || !SaveSettingsToEnv() || !EnsureOssConfigured("自动更新"))
        {
            return;
        }

        await RunScriptAsync("安装每日自动更新任务", "install-auto-upgrade.ps1");
    }

    private async void PublishToolPatch_Click(object sender, RoutedEventArgs e)
    {
        if (RequireManager() && SaveSettingsToEnv() && EnsureOssConfigured("发布"))
        {
            await RunScriptAsync("发布工具补丁到 OSS", "publish-release.ps1", "-PackageMode ToolPatch");
        }
    }

    private async void PublishFullRelease_Click(object sender, RoutedEventArgs e)
    {
        if (RequireManager() && SaveSettingsToEnv() && EnsureOssConfigured("发布"))
        {
            await RunScriptAsync("发布完整部署资源到 OSS", "publish-all-assets.ps1");
        }
    }

    // ──── 服务控制 ────

    private void StartSelected_Click(object sender, RoutedEventArgs e) => RunServiceCommand("start");
    private void StopSelected_Click(object sender, RoutedEventArgs e) => RunServiceCommand("stop");
    private void RestartSelected_Click(object sender, RoutedEventArgs e) => RunServiceCommand("restart");
    private async void Refresh_Click(object sender, RoutedEventArgs e) => await RefreshStatusAsync();

    private async void HealthCheck_Click(object sender, RoutedEventArgs e)
    {
        if (RequireMaintainer())
        {
            await HealthCheck();
        }
    }

    private void OpenSite_Click(object sender, RoutedEventArgs e) => OpenSite();

    private void OpenLogs_Click(object sender, RoutedEventArgs e)
    {
        if (RequireMaintainer())
        {
            OpenLogsDir();
        }
    }

    private async void RefreshLog_Click(object sender, RoutedEventArgs e) => await LoadCurrentLogTabAsync();

    private async void LogTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!ReferenceEquals(e.OriginalSource, LogTabs))
        {
            return;
        }
        await LoadCurrentLogTabAsync();
    }

    private void ClearServiceOutput_Click(object sender, RoutedEventArgs e) => ServiceOutputBox.Clear();

    // ──── 备份 ────

    private async void BackupNow_Click(object sender, RoutedEventArgs e)
    {
        if (RequireMaintainer() && SaveSettingsToEnv())
        {
            await RunScriptAsync("立即备份数据库", "backup-database.ps1", "-RunNow");
        }
    }

    private async void InstallBackupSchedule_Click(object sender, RoutedEventArgs e)
    {
        if (RequireMaintainer() && SaveSettingsToEnv())
        {
            await RunScriptAsync("启用每日备份", "backup-database.ps1", "-InstallSchedule");
        }
    }

    private async void RemoveBackupSchedule_Click(object sender, RoutedEventArgs e)
    {
        if (RequireMaintainer())
        {
            await RunScriptAsync("关闭每日备份", "backup-database.ps1", "-RemoveSchedule");
        }
    }

    // ──── 运行设置 ────

    private void BrowseDeployRoot_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new Forms.FolderBrowserDialog
        {
            Description = "选择部署目录",
            UseDescriptionForTitle = true,
            SelectedPath = Directory.Exists(DeployRootBox.Text) ? DeployRootBox.Text : @"D:\deploy\lab"
        };

        if (dialog.ShowDialog() == Forms.DialogResult.OK)
        {
            DeployRootBox.Text = dialog.SelectedPath;
        }
    }

    private async void AutoStart_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await SetAutoStartAsync(AutoStartBox.IsChecked == true);
            Log(AutoStartBox.IsChecked == true ? "已开启开机自启动。" : "已关闭开机自启动。");
        }
        catch (Exception ex)
        {
            AutoStartBox.IsChecked = IsAutoStartEnabled();
            WpfMessageBox.Show("设置开机自启动失败：" + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void TraySetting_Click(object sender, RoutedEventArgs e)
    {
        SaveBoolSetting("TrayOnMinimize", TrayOnMinimizeBox.IsChecked == true);
        SaveBoolSetting("CloseToTray", CloseToTrayBox.IsChecked == true);
    }

    private void HideToTray_Click(object sender, RoutedEventArgs e) => HideToTray(showTip: true);
    private void ClearOutput_Click(object sender, RoutedEventArgs e) => OutputBox.Clear();

    // ──── 任务执行 ────

    private async void RunServiceCommand(string action)
    {
        if (!RequireMaintainer() || !CanRunTask())
        {
            return;
        }

        var names = SelectedServices().Where(s => !s.IsPortOnly).Select(s => s.Name).ToArray();
        if (names.Length == 0)
        {
            WpfMessageBox.Show("请先在服务列表勾选服务。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var command = new StringBuilder();
        command.AppendLine("$ErrorActionPreference='Continue'");
        command.AppendLine("$services=@(" + string.Join(",", names.Select(n => $"'{n}'")) + ")");
        command.AppendLine("foreach($s in $services){");
        command.AppendLine($"  Write-Host '[{action}]' $s");
        command.AppendLine(action switch
        {
            "start" => "  Start-Service -Name $s -ErrorAction Stop",
            "stop" => "  Stop-Service -Name $s -Force -ErrorAction Stop",
            _ => "  Restart-Service -Name $s -Force -ErrorAction Stop"
        });
        command.AppendLine("}");
        var encoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(command.ToString()));
        LogTabs.SelectedIndex = 0;
        await RunTask($"{action} services", $"-NoProfile -ExecutionPolicy Bypass -EncodedCommand {encoded}", ServiceOutputBox);
    }

    private async Task RunScriptAsync(string title, string scriptName, string extraArgs = "")
    {
        if (!CanRunTask())
        {
            return;
        }

        var scriptPath = Path.Combine(scriptsDir, scriptName);
        if (!File.Exists(scriptPath))
        {
            WpfMessageBox.Show($"找不到脚本：{scriptPath}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        await RunTask(title, $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\" {extraArgs}".Trim(), OutputBox);
    }

    private bool CanRunTask()
    {
        if (!busy)
        {
            return true;
        }

        WpfMessageBox.Show("已有任务正在执行，请稍后。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        return false;
    }

    private async Task RunTask(string title, string powershellArguments, WpfTextBox target)
    {
        SetBusy(true);
        Log(target, "");
        Log(target, $">>> {title}");
        try
        {
            var result = await RunProcess("powershell.exe", powershellArguments, scriptsDir);
            foreach (var line in SplitLines(result.Output))
            {
                Log(target, line);
            }
            Log(target, $"<<< 任务结束，退出码：{result.ExitCode}");
        }
        catch (Exception ex)
        {
            Log(target, "ERROR: " + ex.Message);
        }
        finally
        {
            SetBusy(false);
            await RefreshStatusAsync();
        }
    }

    private static async Task<ScriptResult> RunProcess(string fileName, string arguments, string workingDirectory)
    {
        var output = new StringBuilder();
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            },
            EnableRaisingEvents = true
        };

        process.OutputDataReceived += (_, e) => { if (e.Data != null) output.AppendLine(e.Data); };
        process.ErrorDataReceived += (_, e) => { if (e.Data != null) output.AppendLine(e.Data); };
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();
        return new ScriptResult(process.ExitCode, output.ToString());
    }

    // ──── 配置读写 ────

    private void ReloadEnv()
    {
        env = File.Exists(envPath) ? ReadDotEnv(envPath) : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    private void FillSettingsFields()
    {
        DeployRootBox.Text = EnvValue("DEPLOY_ROOT", @"D:\deploy\lab");
        WebPortBox.Text = PortFor("WEB_PORT", 80).ToString();
        ApiPortBox.Text = PortFor("API_PORT", 10089).ToString();
        NlqPortBox.Text = PortFor("NLQ_PORT", 8000).ToString();
        RedisPortBox.Text = PortFor("REDIS_PORT", 6380).ToString();
        RabbitPortBox.Text = PortFor("RABBITMQ_PORT", 8005).ToString();
        RabbitMgmtPortBox.Text = PortFor("RABBITMQ_MANAGEMENT_PORT", 15672).ToString();
        DbPortBox.Text = PortFor("DB_PORT", 3306).ToString();
        BackupTimeBox.Text = EnvValue("DB_BACKUP_TIME", "02:30");
        BackupKeepDaysBox.Text = EnvValue("DB_BACKUP_KEEP_DAYS", "14");
    }

    private bool SaveSettingsToEnv()
    {
        if (!TryReadPort(WebPortBox, "Web 入口", out var webPort) ||
            !TryReadPort(ApiPortBox, "API", out var apiPort) ||
            !TryReadPort(NlqPortBox, "智能体", out var nlqPort) ||
            !TryReadPort(RedisPortBox, "Redis", out var redisPort) ||
            !TryReadPort(RabbitPortBox, "RabbitMQ", out var rabbitPort) ||
            !TryReadPort(RabbitMgmtPortBox, "RabbitMQ 管理台", out var rabbitMgmtPort) ||
            !TryReadPort(DbPortBox, "MySQL", out var dbPort))
        {
            return false;
        }

        var ports = new[] { webPort, apiPort, nlqPort, redisPort, rabbitPort, rabbitMgmtPort, dbPort };
        if (ports.Distinct().Count() != ports.Length)
        {
            WpfMessageBox.Show("各服务端口不能重复，请检查后再保存。", "端口冲突", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        var backupTime = BackupTimeBox.Text.Trim();
        if (!Regex.IsMatch(backupTime, @"^\d{2}:\d{2}$") || !TimeSpan.TryParse(backupTime, out _))
        {
            WpfMessageBox.Show("每日备份时间请填写 HH:mm，例如 02:30。", "端口/时间格式错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (!int.TryParse(BackupKeepDaysBox.Text.Trim(), out var keepDays) || keepDays < 1 || keepDays > 3650)
        {
            WpfMessageBox.Show("备份保留天数请填写 1-3650 的数字。", "备份设置错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        var root = DeployRootBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(root))
        {
            WpfMessageBox.Show("请选择部署目录。", "部署设置错误", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        root = Path.GetFullPath(root);
        Directory.CreateDirectory(root);
        DeployRootBox.Text = root;

        var updates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["DEPLOY_ROOT"] = root,
            ["NLQ_DIR"] = Path.Combine(root, "nlq-agent"),
            ["NSSM_EXE"] = Path.Combine(root, "nssm.exe"),
            ["NGINX_EXE"] = Path.Combine(root, "nginx", "nginx.exe"),
            ["WEB_PORT"] = webPort.ToString(),
            ["API_PORT"] = apiPort.ToString(),
            ["NLQ_PORT"] = nlqPort.ToString(),
            ["REDIS_PORT"] = redisPort.ToString(),
            ["RABBITMQ_PORT"] = rabbitPort.ToString(),
            ["RABBITMQ_MANAGEMENT_PORT"] = rabbitMgmtPort.ToString(),
            ["DB_PORT"] = dbPort.ToString(),
            ["DB_BACKUP_TIME"] = backupTime,
            ["DB_BACKUP_KEEP_DAYS"] = keepDays.ToString()
        };

        WriteEnvUpdates(updates);
        ReloadEnv();
        UpdateRowPorts();
        return true;
    }

    private static bool TryReadPort(WpfTextBox box, string name, out int port)
    {
        if (int.TryParse(box.Text.Trim(), out port) && port is >= 1 and <= 65535)
        {
            return true;
        }

        WpfMessageBox.Show($"{name} 端口必须是 1-65535 的数字。", "端口格式错误", MessageBoxButton.OK, MessageBoxImage.Warning);
        return false;
    }

    private void WriteEnvUpdates(Dictionary<string, string> updates)
    {
        EnsureEnvFile();
        var lines = File.Exists(envPath)
            ? File.ReadAllLines(envPath, Encoding.UTF8).ToList()
            : new List<string>();

        foreach (var (key, value) in updates)
        {
            var found = false;
            var pattern = @"^\s*" + Regex.Escape(key) + @"\s*=";
            for (var i = 0; i < lines.Count; i++)
            {
                if (!Regex.IsMatch(lines[i], pattern, RegexOptions.IgnoreCase))
                {
                    continue;
                }

                lines[i] = $"{key}={value}";
                found = true;
                break;
            }

            if (!found)
            {
                lines.Add($"{key}={value}");
            }
        }

        File.WriteAllLines(envPath, lines, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    }

    // ──── 服务状态 ────

    private void UpdateRowPorts()
    {
        foreach (var item in services)
        {
            rowsByName[item.Name].Port = PortFor(item);
        }
    }

    private async Task RefreshStatusAsync()
    {
        if (refreshing)
        {
            return;
        }

        refreshing = true;
        try
        {
            ReloadEnv();
            var ports = services.ToDictionary(x => x.Name, PortFor, StringComparer.OrdinalIgnoreCase);
            var serviceInfo = await QueryServicesAsync(services.Where(x => !x.IsPortOnly).Select(x => x.Name));
            var portOpen = await Task.Run(() => ports.ToDictionary(
                pair => pair.Key,
                pair => pair.Value > 0 && IsPortOpen(pair.Value),
                StringComparer.OrdinalIgnoreCase));

            foreach (var item in services)
            {
                var row = rowsByName[item.Name];
                var info = item.IsPortOnly || !serviceInfo.TryGetValue(item.Name, out var found)
                    ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    : found;
                row.State = item.IsPortOnly ? "PortOnly" : info.GetValueOrDefault("State", "NotInstalled");
                row.StartMode = item.IsPortOnly ? "" : info.GetValueOrDefault("StartMode", "");
                row.ProcessId = item.IsPortOnly ? "" : info.GetValueOrDefault("ProcessId", "");
                row.Port = ports[item.Name];
                row.PortOpen = portOpen.GetValueOrDefault(item.Name);
            }

            LastRefreshText.Text = "最后刷新：" + DateTime.Now.ToString("HH:mm:ss");
        }
        catch (Exception ex)
        {
            LogService("刷新状态失败：" + ex.Message);
        }
        finally
        {
            refreshing = false;
        }
    }

    private async Task<Dictionary<string, Dictionary<string, string>>> QueryServicesAsync(IEnumerable<string> serviceNames)
    {
        var names = serviceNames.ToArray();
        if (names.Length == 0)
        {
            return new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        }

        var filters = string.Join(" OR ", names.Select(name => $"Name='{name.Replace("'", "''")}'"));
        var query = "$ProgressPreference='SilentlyContinue';$ErrorActionPreference='SilentlyContinue';" +
            $"Get-CimInstance Win32_Service -Filter \"{filters}\" | Select-Object Name,State,StartMode,ProcessId | ConvertTo-Json -Compress";
        var encoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(query));
        var result = await RunProcess("powershell.exe", $"-NoProfile -EncodedCommand {encoded}", scriptsDir);
        var map = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        var json = ExtractJson(result.Output);
        if (result.ExitCode != 0 || string.IsNullOrWhiteSpace(json) || json.Trim() == "null")
        {
            return map;
        }

        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var element in doc.RootElement.EnumerateArray())
            {
                AddServiceJson(map, element);
            }
        }
        else if (doc.RootElement.ValueKind == JsonValueKind.Object)
        {
            AddServiceJson(map, doc.RootElement);
        }

        return map;
    }

    private static string? ExtractJson(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        for (var start = 0; start < text.Length; start++)
        {
            var first = text[start];
            if (first is not ('{' or '['))
            {
                continue;
            }

            var close = first == '{' ? '}' : ']';
            var depth = 0;
            var inString = false;
            var escaped = false;

            for (var i = start; i < text.Length; i++)
            {
                var c = text[i];
                if (inString)
                {
                    if (escaped)
                    {
                        escaped = false;
                    }
                    else if (c == '\\')
                    {
                        escaped = true;
                    }
                    else if (c == '"')
                    {
                        inString = false;
                    }

                    continue;
                }

                if (c == '"')
                {
                    inString = true;
                    continue;
                }

                if (c == first)
                {
                    depth++;
                }
                else if (c == close)
                {
                    depth--;
                    if (depth == 0)
                    {
                        return text.Substring(start, i - start + 1);
                    }
                }
            }
        }

        return null;
    }

    private static void AddServiceJson(Dictionary<string, Dictionary<string, string>> map, JsonElement element)
    {
        if (!element.TryGetProperty("Name", out var nameProperty))
        {
            return;
        }

        var name = nameProperty.GetString();
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        map[name] = element.EnumerateObject()
            .ToDictionary(property => property.Name, property => property.Value.ToString(), StringComparer.OrdinalIgnoreCase);
    }

    private async Task HealthCheck()
    {
        LogTabs.SelectedIndex = 0;
        LogService(">>> 健康检查");
        var urls = new[]
        {
            $"http://127.0.0.1:{PortFor("WEB_PORT", 80)}/",
            $"http://127.0.0.1:{PortFor("WEB_PORT", 80)}/health",
            $"http://127.0.0.1:{PortFor("API_PORT", 10089)}/health",
            $"http://127.0.0.1:{PortFor("NLQ_PORT", 8000)}/api/v1/health",
            $"http://127.0.0.1:{PortFor("NLQ_PORT", 8000)}/api/v1/ready"
        };

        foreach (var url in urls)
        {
            try
            {
                using var response = await httpClient.GetAsync(url);
                LogService($"{url} -> {(int)response.StatusCode}");
            }
            catch (Exception ex)
            {
                LogService($"{url} -> FAILED: {ex.Message}");
            }
        }
    }

    // ──── 服务日志（按服务分类展示）────

    private void BuildLogTabs()
    {
        foreach (var item in services.Where(s => s.LogName.Length > 0).DistinctBy(s => s.LogName))
        {
            var box = new WpfTextBox
            {
                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                FontSize = 12,
                IsReadOnly = true,
                AcceptsReturn = true,
                AcceptsTab = true,
                TextWrapping = TextWrapping.NoWrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            logBoxes[item.LogName] = box;
            LogTabs.Items.Add(new TabItem { Header = item.LogName, Content = box, Tag = item.LogName });
        }
    }

    private async Task LoadCurrentLogTabAsync()
    {
        if (LogTabs.SelectedItem is not TabItem tab || tab.Tag is not string logName)
        {
            return;
        }

        var box = logBoxes[logName];
        var dir = Path.Combine(DeployRoot(), "logs", logName);
        box.Text = $"正在读取 {dir} ...";
        var text = await Task.Run(() => ReadServiceLog(dir));
        box.Text = text;
        box.ScrollToEnd();
    }

    private static string ReadServiceLog(string dir)
    {
        if (!Directory.Exists(dir))
        {
            return $"日志目录不存在：{dir}";
        }

        var sb = new StringBuilder();
        foreach (var fileName in new[] { "stdout.log", "stderr.log" })
        {
            var path = Path.Combine(dir, fileName);
            sb.AppendLine($"===== {fileName} =====");
            if (File.Exists(path))
            {
                foreach (var line in Tail(path, 200))
                {
                    sb.AppendLine(line);
                }
            }
            else
            {
                sb.AppendLine("（文件不存在）");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    // ──── 其他操作 ────

    private void OpenSite()
    {
        Process.Start(new ProcessStartInfo($"http://127.0.0.1:{PortFor("WEB_PORT", 80)}/") { UseShellExecute = true });
    }

    private void OpenLogsDir()
    {
        var dir = Path.Combine(DeployRoot(), "logs");
        Directory.CreateDirectory(dir);
        Process.Start(new ProcessStartInfo("explorer.exe", $"\"{dir}\"") { UseShellExecute = true });
    }

    private IEnumerable<ServiceItem> SelectedServices()
    {
        var names = statusRows
            .Where(row => row.CanSelect && row.Selected)
            .Select(row => row.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        return services.Where(s => names.Contains(s.Name));
    }

    private static string[] SelectedTags(System.Windows.Controls.Panel panel)
    {
        return panel.Children.OfType<WpfCheckBox>()
            .Where(x => x.IsChecked == true)
            .Select(x => x.Tag?.ToString() ?? "")
            .Where(x => x.Length > 0)
            .ToArray();
    }

    private int PortFor(ServiceItem item) => PortFor(item.PortKey, item.DefaultPort);

    private int PortFor(string key, int defaultPort)
    {
        return int.TryParse(EnvValue(key, defaultPort.ToString()), out var port) ? port : defaultPort;
    }

    private string DeployRoot()
    {
        var root = EnvValue("DEPLOY_ROOT", @"D:\deploy\lab");
        Directory.CreateDirectory(root);
        return Path.GetFullPath(root);
    }

    private string EnvValue(string key, string fallback)
    {
        return env.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : fallback;
    }

    private static Dictionary<string, string> ReadDotEnv(string path)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var raw in File.ReadLines(path, Encoding.UTF8))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            var idx = line.IndexOf('=');
            if (idx <= 0)
            {
                continue;
            }

            var key = line[..idx].Trim();
            var value = line[(idx + 1)..].Trim();
            if (value.Length >= 2 && ((value[0] == '"' && value[^1] == '"') || (value[0] == '\'' && value[^1] == '\'')))
            {
                value = value[1..^1];
            }
            result[key] = value;
        }

        return result;
    }

    private void AddChoiceBoxes(System.Windows.Controls.Panel panel, IEnumerable<StoreItem> items, bool isChecked)
    {
        foreach (var item in items)
        {
            panel.Children.Add(new WpfCheckBox
            {
                Content = item.Title,
                IsChecked = isChecked,
                Margin = new Thickness(0, 0, 8, 8),
                Tag = item.Key
            });
        }
    }

    private void SetAdminText()
    {
        AdminText.Text = IsAdministrator() ? "管理员权限：已获取" : "管理员权限：未获取";
        AdminText.Foreground = IsAdministrator()
            ? System.Windows.Media.Brushes.DarkGreen
            : System.Windows.Media.Brushes.DarkRed;
    }

    // ──── 托盘与自启动 ────

    private void InitializeTrayIcon()
    {
        trayIcon = new Forms.NotifyIcon
        {
            Icon = Drawing.SystemIcons.Application,
            Text = "检测室数据分析系统 - 部署控制台",
            Visible = true,
            ContextMenuStrip = new Forms.ContextMenuStrip()
        };

        trayIcon.ContextMenuStrip.Items.Add("打开控制台", null, (_, _) => Dispatcher.Invoke(ShowFromTray));
        trayIcon.ContextMenuStrip.Items.Add("立即更新", null, (_, _) => Dispatcher.Invoke(() => RunUpgrade_Click(this, new RoutedEventArgs())));
        trayIcon.ContextMenuStrip.Items.Add("退出", null, (_, _) => Dispatcher.Invoke(() =>
        {
            reallyExit = true;
            Close();
        }));
        trayIcon.DoubleClick += (_, _) => Dispatcher.Invoke(ShowFromTray);
    }

    private void HideToTray(bool showTip)
    {
        Hide();
        if (showTip)
        {
            trayIcon?.ShowBalloonTip(2500, "部署控制台仍在运行", "双击托盘图标可重新打开。", Forms.ToolTipIcon.Info);
        }
    }

    private void ShowFromTray()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    private void LoadLocalSettings()
    {
        AutoStartBox.IsChecked = IsAutoStartEnabled();
        TrayOnMinimizeBox.IsChecked = ReadBoolSetting("TrayOnMinimize", true);
        CloseToTrayBox.IsChecked = ReadBoolSetting("CloseToTray", true);
    }

    private static bool ReadBoolSetting(string name, bool defaultValue)
    {
        using var key = Registry.CurrentUser.OpenSubKey(AppSettingsKeyPath);
        var value = key?.GetValue(name)?.ToString();
        return value switch
        {
            "0" => false,
            "1" => true,
            _ => defaultValue
        };
    }

    private static void SaveBoolSetting(string name, bool value)
    {
        using var key = Registry.CurrentUser.CreateSubKey(AppSettingsKeyPath);
        key?.SetValue(name, value ? "1" : "0", RegistryValueKind.String);
    }

    private static bool IsAutoStartEnabled()
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "schtasks.exe",
                Arguments = $"/Query /TN \"{AutoStartTaskName}\"",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });
            if (process == null)
            {
                return false;
            }

            if (!process.WaitForExit(2000))
            {
                try
                {
                    process.Kill();
                }
                catch
                {
                }
                return false;
            }
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private async Task SetAutoStartAsync(bool enabled)
    {
        string script;
        if (enabled)
        {
            var exe = ExecutablePath().Replace("'", "''");
            script = $@"
$action = New-ScheduledTaskAction -Execute '{exe}'
$trigger = New-ScheduledTaskTrigger -AtLogOn
$principal = New-ScheduledTaskPrincipal -UserId ([Security.Principal.WindowsIdentity]::GetCurrent().Name) -RunLevel Highest
$settings = New-ScheduledTaskSettingsSet -StartWhenAvailable -MultipleInstances IgnoreNew
Register-ScheduledTask -TaskName '{AutoStartTaskName}' -Action $action -Trigger $trigger -Principal $principal -Settings $settings -Force | Out-Null
";
        }
        else
        {
            script = $"Unregister-ScheduledTask -TaskName '{AutoStartTaskName}' -Confirm:$false -ErrorAction SilentlyContinue";
        }

        var encoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(script));
        var result = await RunProcess("powershell.exe", $"-NoProfile -ExecutionPolicy Bypass -EncodedCommand {encoded}", scriptsDir);
        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException(result.Output.Trim());
        }
    }

    private static string ExecutablePath()
    {
        return Environment.ProcessPath
            ?? Process.GetCurrentProcess().MainModule?.FileName
            ?? Path.Combine(AppContext.BaseDirectory, "LmDeployConsole.exe");
    }

    // ──── 工具方法 ────

    private static bool IsPortOpen(int port)
    {
        try
        {
            using var client = new TcpClient();
            var task = client.ConnectAsync("127.0.0.1", port);
            return task.Wait(TimeSpan.FromMilliseconds(700)) && client.Connected;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsAdministrator()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    private static string FindScriptsDir()
    {
        var baseDir = AppContext.BaseDirectory;
        var candidates = new[]
        {
            baseDir,
            Path.Combine(baseDir, "ops"),
            Path.GetFullPath(Path.Combine(baseDir, "..")),
            Path.GetFullPath(Path.Combine(baseDir, "..", "ops")),
            Path.GetFullPath(Path.Combine(baseDir, "..", "..", "windows")),
            @"D:\deploy\lab\ops"
        };

        foreach (var dir in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (File.Exists(Path.Combine(dir, "deploy-all.ps1")) && File.Exists(Path.Combine(dir, "_dotenv.ps1")))
            {
                return dir;
            }
        }

        return baseDir;
    }

    private static IEnumerable<string> Tail(string file, int count)
    {
        try
        {
            return File.ReadLines(file, Encoding.UTF8).TakeLast(count).ToArray();
        }
        catch
        {
            try
            {
                return File.ReadLines(file, Encoding.Default).TakeLast(count).ToArray();
            }
            catch (Exception ex)
            {
                return [$"读取日志失败：{ex.Message}"];
            }
        }
    }

    private static IEnumerable<string> SplitLines(string text)
    {
        return Regex.Split(text.Replace("\r\n", "\n"), "\n").Where(line => line.Length > 0);
    }

    private void SetBusy(bool value)
    {
        busy = value;
        BusyText.Text = value ? "正在执行任务..." : "就绪";
        AuthorizeButton.IsEnabled = !value;
        LogoutButton.IsEnabled = !value;
        BrowseButton.IsEnabled = !value;
        ApplyRolePermissions();
    }

    private void Log(string message) => Log(OutputBox, message);

    private void LogService(string message) => Log(ServiceOutputBox, message);

    private static void Log(WpfTextBox target, string message)
    {
        target.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        target.ScrollToEnd();
    }
}
