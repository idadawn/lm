using System.Windows;

namespace LmDeployConsole;

public partial class AuthDialog : Window
{
    private readonly string? currentCode;

    public UpgradeAuth? Result { get; private set; }

    public AuthDialog(string statusText, string? currentCode)
    {
        InitializeComponent();
        this.currentCode = currentCode;
        StatusText.Text = statusText;
        StatusText.Foreground = currentCode is null
            ? System.Windows.Media.Brushes.DarkRed
            : System.Windows.Media.Brushes.DarkGreen;
        CopyButton.Visibility = currentCode is null ? Visibility.Collapsed : Visibility.Visible;
        Loaded += (_, _) => CodeBox.Focus();
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var auth = UpgradeAuthCode.TryDecode(CodeBox.Text);
        if (auth is null)
        {
            ErrorText.Text = "授权码无效：请确认完整粘贴了供应商提供的授权码。";
            ErrorText.Visibility = Visibility.Visible;
            return;
        }

        Result = auth;
        DialogResult = true;
    }

    private void Copy_Click(object sender, RoutedEventArgs e)
    {
        if (currentCode is null)
        {
            return;
        }

        try
        {
            Clipboard.SetText(currentCode);
            CopyButton.Content = "已复制";
        }
        catch (Exception ex)
        {
            ErrorText.Text = "复制失败：" + ex.Message;
            ErrorText.Visibility = Visibility.Visible;
        }
    }
}
