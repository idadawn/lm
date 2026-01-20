using System.ComponentModel;

namespace Poxiao.Lab.Entity.Enum;

/// <summary>
/// Excel导入模板编码枚举.
/// </summary>
public enum ExcelImportTemplateCode
{
    /// <summary>
    /// 检测数据导入模板.
    /// </summary>
    [Description("检测数据导入模板")]
    RawDataImport,

    /// <summary>
    /// 磁性数据导入模板.
    /// </summary>
    [Description("磁性数据导入模板")]
    MagneticDataImport,
}
