using System;

namespace Poxiao.Lab.Entity.Dto.RawData;

/// <summary>
/// 简化导入输入
/// </summary>
public class SimpleImportInput
{
    /// <summary>
    /// 文件名
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// 文件数据（Base64）
    /// </summary>
    public string FileData { get; set; }

    /// <summary>
    /// 是否跳过已存在的炉号（true:跳过，false:提示错误）
    /// </summary>
    public bool SkipExistingFurnaceNo { get; set; } = true;
}

/// <summary>
/// 简化导入输出
/// </summary>
public class SimpleImportOutput
{
    /// <summary>
    /// 总行数
    /// </summary>
    public int TotalRows { get; set; }

    /// <summary>
    /// 成功导入行数
    /// </summary>
    public int SuccessRows { get; set; }

    /// <summary>
    /// 失败行数
    /// </summary>
    public int FailRows { get; set; }

    /// <summary>
    /// 跳过的行数（炉号已存在）
    /// </summary>
    public int SkippedRows { get; set; }

    /// <summary>
    /// 错误信息列表
    /// </summary>
    public List<string> Errors { get; set; } = new List<string>();

    /// <summary>
    /// 重复的炉号列表
    /// </summary>
    public List<string> DuplicateFurnaceNos { get; set; } = new List<string>();
}