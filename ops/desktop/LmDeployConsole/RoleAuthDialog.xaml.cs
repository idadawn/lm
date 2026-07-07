using System.Windows;

namespace LmDeployConsole;

public partial class RoleAuthDialog : Window
{
    private readonly Func<string, bool> tryAuthorize;

    public RoleAuthDialog(string? hint, Func<string, bool> tryAuthorize)
    {
        InitializeComponent();
        this.tryAuthorize = tryAuthorize;
        if (!string.IsNullOrWhiteSpace(hint))
        {
            HintText.Text = hint;
            HintText.Visibility = Visibility.Visible;
        }
        Loaded += (_, _) => CodeBox.Focus();
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        if (tryAuthorize(CodeBox.Password.Trim()))
        {
            DialogResult = true;
            return;
        }

        ErrorText.Text = "授权码不正确，请重新输入。";
        ErrorText.Visibility = Visibility.Visible;
        CodeBox.Clear();
        CodeBox.Focus();
    }
}
