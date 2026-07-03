using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace LmDeployConsole;

public sealed record ServiceItem(
    string Name,
    string Title,
    string PortKey,
    int DefaultPort,
    string ScriptKey,
    string LogName,
    bool IsPortOnly = false);

public sealed class ServiceStatusRow
{
    public string Name { get; set; } = "";
    public string Title { get; set; } = "";
    public string State { get; set; } = "";
    public string StartMode { get; set; } = "";
    public string ProcessId { get; set; } = "";
    public int Port { get; set; }
    public bool PortOpen { get; set; }
    public string PortOpenText => PortOpen ? "是" : "否";
}

public sealed record ScriptResult(int ExitCode, string Output);

public partial class MainWindow : Window
{
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

    private readonly DispatcherTimer refreshTimer = new() { Interval = TimeSpan.FromSeconds(5) };
    private readonly HttpClient httpClient = new() { Timeout = TimeSpan.FromSeconds(5) };
    private readonly string scriptsDir;
    private readonly string envPath;
    private Dictionary<string, string> env = new(StringComparer.OrdinalIgnoreCase);
    private bool busy;
    private bool refreshing;

    public MainWindow()
    {
        InitializeComponent();
        scriptsDir = FindScriptsDir();
        envPath = Path.Combine(scriptsDir, ".env");

        ScriptsDirBox.Text = scriptsDir;
        AdminText.Text = IsAdministrator() ? "管理员权限：是" : "管理员权限：否，请用管理员身份启动";
        AdminText.Foreground = IsAdministrator()
            ? System.Windows.Media.Brushes.DarkGreen
            : System.Windows.Media.Brushes.DarkRed;

        foreach (var item in services.Where(x => !x.IsPortOnly))
        {
            ServiceChecks.Children.Add(new CheckBox
            {
                Content = $"{item.Name}  {item.Title}",
                IsChecked = true,
                Margin = new Thickness(0, 0, 0, 5),
                Tag = item.Name
            });
        }

        ReloadEnv();
        SetInitialStatusRows();

        Loaded += async (_, _) =>
        {
            Log("WPF 部署控制台已启动。");
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
        httpClient.Dispose();
        base.OnClosed(e);
    }

    private void DownloadPrereqs_Click(object sender, RoutedEventArgs e) => RunScript("下载基础软件", "download-prereqs.ps1");
    private void RenderConfig_Click(object sender, RoutedEventArgs e) => RunScript("渲染配置", "render-config.ps1");
    private void DeployAll_Click(object sender, RoutedEventArgs e) => RunScript("一键部署/更新", "deploy-all.ps1");
    private void PublishToolPatch_Click(object sender, RoutedEventArgs e) => RunScript("发布工具补丁到 OSS", "publish-release.ps1", "-PackageMode ToolPatch");
    private void PublishFullRelease_Click(object sender, RoutedEventArgs e) => RunScript("发布完整版本到 OSS", "publish-release.ps1", "-PackageMode Full");
    private void CheckUpgrade_Click(object sender, RoutedEventArgs e) => RunScript("检查更新", "upgrade.ps1", "-CheckOnly");
    private void RunUpgrade_Click(object sender, RoutedEventArgs e) => RunScript("立即在线升级", "upgrade.ps1");
    private void InstallAutoUpgrade_Click(object sender, RoutedEventArgs e) => RunScript("安装自动升级", "install-auto-upgrade.ps1");
    private void RestartAsAdmin_Click(object sender, RoutedEventArgs e) => RestartAsAdministrator();
    private void StartSelected_Click(object sender, RoutedEventArgs e) => RunServiceCommand("start");
    private void StopSelected_Click(object sender, RoutedEventArgs e) => RunServiceCommand("stop");
    private void RestartSelected_Click(object sender, RoutedEventArgs e) => RunServiceCommand("restart");
    private async void Refresh_Click(object sender, RoutedEventArgs e) => await RefreshStatusAsync();
    private async void HealthCheck_Click(object sender, RoutedEventArgs e) => await HealthCheck();
    private void TailLogs_Click(object sender, RoutedEventArgs e) => TailLogs();
    private void OpenSite_Click(object sender, RoutedEventArgs e) => OpenSite();
    private void OpenLogs_Click(object sender, RoutedEventArgs e) => OpenLogsDir();
    private void ClearOutput_Click(object sender, RoutedEventArgs e) => OutputBox.Clear();

    private void InstallInfra_Click(object sender, RoutedEventArgs e)
    {
        var selected = SelectedServices()
            .Where(s => s.ScriptKey is "redis" or "rabbitmq")
            .Select(s => s.ScriptKey)
            .Distinct()
            .ToArray();
        var args = selected.Length > 0 ? "-Only " + string.Join(",", selected) : "";
        RunScript("安装基础服务", "install-infra.ps1", args);
    }

    private void InstallApps_Click(object sender, RoutedEventArgs e)
    {
        var selected = SelectedServices()
            .Where(s => s.ScriptKey is "api" or "web" or "nlq-agent")
            .Select(s => s.ScriptKey)
            .Distinct()
            .ToArray();
        var args = selected.Length > 0
            ? "-Only " + string.Join(",", selected) + " -SkipBuild"
            : "-SkipBuild";
        RunScript("安装应用服务", "install-services.ps1", args);
    }

    private async void RunScript(string title, string scriptName, string extraArgs = "")
    {
        if (!CanRunTask())
        {
            return;
        }

        var scriptPath = Path.Combine(scriptsDir, scriptName);
        if (!File.Exists(scriptPath))
        {
            MessageBox.Show($"找不到脚本：{scriptPath}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        await RunTask(title, $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\" {extraArgs}".Trim());
    }

    private async void RunServiceCommand(string action)
    {
        if (!CanRunTask())
        {
            return;
        }

        var names = SelectedServices().Where(s => !s.IsPortOnly).Select(s => s.Name).ToArray();
        if (names.Length == 0)
        {
            MessageBox.Show("请先勾选服务。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
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
        await RunTask($"{action} services", $"-NoProfile -ExecutionPolicy Bypass -EncodedCommand {encoded}");
    }

    private bool CanRunTask()
    {
        if (!busy)
        {
            return true;
        }

        MessageBox.Show("已有任务正在执行，请稍后。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        return false;
    }

    private async Task RunTask(string title, string powershellArguments)
    {
        SetBusy(true);
        Log("");
        Log($">>> {title}");
        try
        {
            var result = await RunProcess("powershell.exe", powershellArguments, scriptsDir);
            foreach (var line in SplitLines(result.Output))
            {
                Log(line);
            }
            Log($"<<< 任务结束，退出码：{result.ExitCode}");
        }
        catch (Exception ex)
        {
            Log("ERROR: " + ex.Message);
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

    private void ReloadEnv()
    {
        env = File.Exists(envPath) ? ReadDotEnv(envPath) : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        DeployRootBox.Text = DeployRoot();
    }

    private void SetInitialStatusRows()
    {
        ReloadEnv();
        ServiceGrid.ItemsSource = services.Select(item => new ServiceStatusRow
        {
            Name = item.Name,
            Title = item.Title,
            State = item.IsPortOnly ? "PortOnly" : "Loading",
            StartMode = "",
            ProcessId = "",
            Port = PortFor(item),
            PortOpen = false
        }).ToList();
        LastRefreshText.Text = "等待刷新...";
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
            var serviceInfo = await QueryServicesAsync(services.Where(x => !x.IsPortOnly).Select(x => x.Name));
            var rows = await Task.Run(() =>
            {
                return services.Select(item =>
                {
                    var port = PortFor(item);
                    var info = item.IsPortOnly || !serviceInfo.TryGetValue(item.Name, out var found)
                        ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                        : found;
                    var state = item.IsPortOnly ? "PortOnly" : info.GetValueOrDefault("State", "NotInstalled");
                    return new ServiceStatusRow
                    {
                        Name = item.Name,
                        Title = item.Title,
                        State = state,
                        StartMode = item.IsPortOnly ? "" : info.GetValueOrDefault("StartMode", ""),
                        ProcessId = item.IsPortOnly ? "" : info.GetValueOrDefault("ProcessId", ""),
                        Port = port,
                        PortOpen = port > 0 && IsPortOpen(port)
                    };
                }).ToList();
            });

            ServiceGrid.ItemsSource = rows;
            LastRefreshText.Text = "最后刷新：" + DateTime.Now.ToString("HH:mm:ss");
        }
        catch (Exception ex)
        {
            Log("刷新状态失败：" + ex.Message);
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
        Log(">>> 健康检查");
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
                Log($"{url} -> {(int)response.StatusCode}");
            }
            catch (Exception ex)
            {
                Log($"{url} -> FAILED: {ex.Message}");
            }
        }
    }

    private void TailLogs()
    {
        foreach (var item in SelectedServices())
        {
            if (string.IsNullOrWhiteSpace(item.LogName))
            {
                continue;
            }

            var stderr = Path.Combine(DeployRoot(), "logs", item.LogName, "stderr.log");
            var stdout = Path.Combine(DeployRoot(), "logs", item.LogName, "stdout.log");
            var file = File.Exists(stderr) ? stderr : File.Exists(stdout) ? stdout : "";
            if (file.Length == 0)
            {
                Log($"未找到日志：{item.Name}");
                continue;
            }

            Log($">>> {file}");
            foreach (var line in Tail(file, 80))
            {
                Log(line);
            }
        }
    }

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

    private void RestartAsAdministrator()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName ?? "LmDeployConsole.exe",
                UseShellExecute = true,
                Verb = "runas"
            });
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show("管理员重启失败：" + ex.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private IEnumerable<ServiceItem> SelectedServices()
    {
        var names = ServiceChecks.Children.OfType<CheckBox>()
            .Where(x => x.IsChecked == true)
            .Select(x => x.Tag?.ToString() ?? "")
            .Where(x => x.Length > 0)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        return services.Where(s => names.Contains(s.Name));
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
        SetButtonsEnabled(this, !value);
        AutoRefreshBox.IsEnabled = true;
    }

    private static void SetButtonsEnabled(DependencyObject root, bool enabled)
    {
        for (var i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(root, i);
            if (child is Button button && button.Content?.ToString() != "刷新状态")
            {
                button.IsEnabled = enabled;
            }

            SetButtonsEnabled(child, enabled);
        }
    }

    private void Log(string message)
    {
        OutputBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        OutputBox.ScrollToEnd();
    }
}
