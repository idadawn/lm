using System.Text;
using System.Text.RegularExpressions;

namespace LmDeployConsole;

public sealed record UpgradeAuth(
    string Endpoint,
    string Bucket,
    string AccessKeyId,
    string AccessKeySecret,
    string Prefix);

/// <summary>
/// 升级授权码 = Base64("LMAUTH1|endpoint|bucket|accessKeyId|accessKeySecret|prefix")。
/// 一串码封装在线升级/发布所需的 OSS 凭据，避免现场手工编辑 .env。
/// </summary>
public static class UpgradeAuthCode
{
    private const string Magic = "LMAUTH1";

    public static string Encode(UpgradeAuth auth)
    {
        var raw = string.Join("|", Magic, auth.Endpoint, auth.Bucket, auth.AccessKeyId, auth.AccessKeySecret, auth.Prefix);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
    }

    public static UpgradeAuth? TryDecode(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var compact = Regex.Replace(text, @"\s+", "");
        string raw;
        try
        {
            raw = Encoding.UTF8.GetString(Convert.FromBase64String(compact));
        }
        catch (FormatException)
        {
            return null;
        }

        var parts = raw.Split('|');
        if (parts.Length < 5 || parts[0] != Magic)
        {
            return null;
        }

        var auth = new UpgradeAuth(
            Endpoint: parts[1].Trim(),
            Bucket: parts[2].Trim(),
            AccessKeyId: parts[3].Trim(),
            AccessKeySecret: parts[4].Trim(),
            Prefix: parts.Length > 5 ? parts[5].Trim() : "");

        return auth.Bucket.Length == 0 || auth.AccessKeyId.Length == 0 || auth.AccessKeySecret.Length == 0
            ? null
            : auth;
    }
}
