namespace Poxiao.VisualDev.Engine.Model.CodeGen;

public class CodeGenExportPropertyJsonModel
{
    public string filedName { get; set; }

    public string poxiaoKey { get; set; }

    public string filedId { get; set; }

    /// <summary>
    /// 是否必填.
    /// </summary>
    public bool required { get; set; }

    /// <summary>
    /// 是否多选.
    /// </summary>
    public bool multiple { get; set; }
}