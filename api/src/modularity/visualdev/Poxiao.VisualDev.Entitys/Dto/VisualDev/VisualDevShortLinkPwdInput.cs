using Poxiao.DependencyInjection;

namespace Poxiao.VisualDev.Entitys.Dto.VisualDev;

/// <summary>
/// 在线表单外链密码验证 输入.
/// </summary>
[SuppressSniffer]
public class VisualDevShortLinkPwdInput
{
    public string id { get; set; }

    public int type { get; set; }

    public string password { get; set; }
}
