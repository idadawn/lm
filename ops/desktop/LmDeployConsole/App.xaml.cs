using System.IO;
using System.Windows;

namespace LmDeployConsole;

public partial class App : System.Windows.Application
{
    static App()
    {
        EnsureWindowsEnvironment();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        EnsureWindowsEnvironment();

        DispatcherUnhandledException += (_, args) =>
        {
            WriteCrashLog(args.Exception);
            System.Windows.MessageBox.Show(args.Exception.ToString(), "LmDeployConsole 启动失败", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            if (args.ExceptionObject is Exception ex)
            {
                WriteCrashLog(ex);
            }
        };

        base.OnStartup(e);
    }

    private static void EnsureWindowsEnvironment()
    {
        var systemRoot = Environment.GetEnvironmentVariable("SystemRoot");
        if (string.IsNullOrWhiteSpace(systemRoot))
        {
            systemRoot = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Windows).TrimEnd(Path.DirectorySeparatorChar),
                "");
        }

        if (string.IsNullOrWhiteSpace(systemRoot))
        {
            systemRoot = @"C:\Windows";
        }

        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WINDIR")))
        {
            Environment.SetEnvironmentVariable("WINDIR", systemRoot);
        }

        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("windir")))
        {
            Environment.SetEnvironmentVariable("windir", systemRoot);
        }

        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SystemRoot")))
        {
            Environment.SetEnvironmentVariable("SystemRoot", systemRoot);
        }
    }

    private static void WriteCrashLog(Exception ex)
    {
        try
        {
            var logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "LmDeployConsole");
            Directory.CreateDirectory(logDir);
            File.AppendAllText(Path.Combine(logDir, "crash.log"),
                $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex}{Environment.NewLine}{Environment.NewLine}");
        }
        catch
        {
            // Best-effort crash logging only.
        }
    }
}
