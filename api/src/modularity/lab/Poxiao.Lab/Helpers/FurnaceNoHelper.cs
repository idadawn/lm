using Poxiao.Lab.Entity.Models;
using System.Text.RegularExpressions;

namespace Poxiao.Lab.Helpers;

/// <summary>
/// 炉号处理辅助类.
/// </summary>
public static class FurnaceNoHelper
{
    /// <summary>
    /// 炉号解析结果.
    /// </summary>
    public class FurnaceNoParseResult
    {
        /// <summary>
        /// 是否解析成功.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 错误信息.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 产线.
        /// </summary>
        public string LineNo { get; set; }

        /// <summary>
        /// 班次.
        /// </summary>
        public string Shift { get; set; }

        /// <summary>
        /// 生产日期.
        /// </summary>
        public DateTime? ProdDate { get; set; }

        /// <summary>
        /// 炉次号（解析后的炉次号数字部分）.
        /// </summary>
        public string FurnaceNo { get; set; }

        /// <summary>
        /// 卷号.
        /// </summary>
        public string CoilNo { get; set; }

        /// <summary>
        /// 分卷号.
        /// </summary>
        public string SubcoilNo { get; set; }

        /// <summary>
        /// 特殊标记（W或者w）.
        /// </summary>
        public string SpecialMarker { get; set; }

        /// <summary>
        /// 特性描述（特性汉字）.
        /// </summary>
        public string FeatureSuffix { get; set; }

        /// <summary>
        /// 产线数字（用于排序）.
        /// </summary>
        public int? LineNoNumeric { get; set; }

        /// <summary>
        /// 班次数字（用于排序：甲=1, 乙=2, 丙=3）.
        /// </summary>
        public int? ShiftNumeric { get; set; }

        /// <summary>
        /// 炉号数字（用于排序）.
        /// </summary>
        public int? FurnaceNoNumeric { get; set; }

        /// <summary>
        /// 卷号数字（用于排序，支持小数）.
        /// </summary>
        public decimal? CoilNoNumeric { get; set; }

        /// <summary>
        /// 分卷号数字（用于排序，支持小数）.
        /// </summary>
        public decimal? SubcoilNoNumeric { get; set; }
    }

    /// <summary>
    /// 解析炉号.
    /// 格式：[产线数字][班次汉字][8位日期]-[炉次号]-[卷号]-[分卷号][可能存在W或者w][可选特性汉字]
    /// 示例：1甲20251101-1-4-1W脆
    /// 注意：炉号格式为 [产线数字][班次汉字][8位日期]-[炉次号]
    /// </summary>
    /// <param name="furnaceNo">原始炉号</param>
    /// <param name="ignoredSuffixes">需要忽略的后缀列表</param>
    /// <returns>解析结果</returns>
    public static FurnaceNoParseResult ParseFurnaceNo(string furnaceNo, IEnumerable<string> ignoredSuffixes = null)
    {
        var result = new FurnaceNoParseResult();

        if (string.IsNullOrWhiteSpace(furnaceNo))
        {
            result.ErrorMessage = "炉号为空";
            return result;
        }

        // 使用新的FurnaceNo类进行解析
        var furnaceNoObj = FurnaceNo.Parse(furnaceNo, ignoredSuffixes);

        if (!furnaceNoObj.IsValid)
        {
            result.ErrorMessage = furnaceNoObj.ErrorMessage;
            return result;
        }

        // 填充结果
        result.Success = true;
        result.LineNo = furnaceNoObj.LineNo;
        result.Shift = furnaceNoObj.Shift;
        result.ProdDate = furnaceNoObj.ProdDate;
        result.FurnaceNo = furnaceNoObj.FurnaceBatchNo; // 注意：这里存储的是炉次号
        result.CoilNo = furnaceNoObj.CoilNo;
        result.SubcoilNo = furnaceNoObj.SubcoilNo;
        result.SpecialMarker = furnaceNoObj.SpecialMarker;
        result.FeatureSuffix = furnaceNoObj.FeatureSuffix;
        result.LineNoNumeric = furnaceNoObj.LineNoNumeric;
        result.ShiftNumeric = furnaceNoObj.ShiftNumeric;
        result.FurnaceNoNumeric = furnaceNoObj.FurnaceBatchNoNumeric;
        result.CoilNoNumeric = furnaceNoObj.CoilNoNumeric;
        result.SubcoilNoNumeric = furnaceNoObj.SubcoilNoNumeric;

        return result;
    }

    /// <summary>
    /// 将班次汉字转换为数字（用于排序）.
    /// 甲=1, 乙=2, 丙=3
    /// </summary>
    /// <param name="shift">班次汉字</param>
    /// <returns>班次数字，如果无法识别则返回null</returns>
    private static int? ConvertShiftToNumeric(string shift)
    {
        if (string.IsNullOrWhiteSpace(shift))
            return null;

        // 移除所有空白字符后判断
        var shiftTrimmed = shift.Trim();

        if (shiftTrimmed.Contains("甲"))
            return 1;
        if (shiftTrimmed.Contains("乙"))
            return 2;
        if (shiftTrimmed.Contains("丙"))
            return 3;

        // 如果无法识别，返回null
        return null;
    }

    /// <summary>
    /// 从炉号中移除特性汉字.
    /// 例如：1甲20251101-1-4-1脆 -> 1甲20251101-1-4-1
    /// </summary>
    /// <param name="furnaceNo">原始炉号（包含特性汉字）</param>
    /// <param name="featureSuffix">特性汉字（如果已知，可传入以提高性能）</param>
    /// <param name="ignoredSuffixes">需要忽略的后缀列表</param>
    /// <returns>去掉特性汉字后的炉号</returns>
    public static string RemoveFeatureSuffix(string furnaceNo, string featureSuffix = null, IEnumerable<string> ignoredSuffixes = null)
    {
        if (string.IsNullOrWhiteSpace(furnaceNo))
            return furnaceNo;

        // 如果已知特性汉字，直接移除
        if (!string.IsNullOrWhiteSpace(featureSuffix))
        {
            return furnaceNo.Replace(featureSuffix, "").TrimEnd();
        }

        // 否则，先解析获取特性汉字，再移除
        var parseResult = ParseFurnaceNo(furnaceNo, ignoredSuffixes);
        if (parseResult.Success && !string.IsNullOrWhiteSpace(parseResult.FeatureSuffix))
        {
            return furnaceNo.Replace(parseResult.FeatureSuffix, "").TrimEnd();
        }

        // 如果解析失败或没有特性汉字，返回原值
        return furnaceNo;
    }

    /// <summary>
    /// 从炉号中自动移除特性汉字（自动解析）.
    /// </summary>
    /// <param name="furnaceNo">原始炉号</param>
    /// <returns>去掉特性汉字后的炉号</returns>
    public static string RemoveFeatureSuffixAuto(string furnaceNo)
    {
        return RemoveFeatureSuffix(furnaceNo, null);
    }

    /// <summary>
    /// 判断炉号是否符合解析规则（用于判断是否为有效数据）.
    /// </summary>
    /// <param name="furnaceNo">原始炉号</param>
    /// <returns>是否符合规则</returns>
    public static bool IsValidFurnaceNo(string furnaceNo, IEnumerable<string> ignoredSuffixes = null)
    {
        if (string.IsNullOrWhiteSpace(furnaceNo))
            return false;

        var parseResult = ParseFurnaceNo(furnaceNo, ignoredSuffixes);
        return parseResult.Success;
    }

    /// <summary>
    /// 构建炉号（去掉特性汉字后的标准格式）.
    /// 格式：[产线数字][班次汉字][8位日期]-[炉次号]-[卷号]-[分卷号]
    /// 注意：用于重复检查的炉号格式为 [产线数字][班次汉字][8位日期]-[炉次号]
    /// </summary>
    /// <param name="lineNo">产线</param>
    /// <param name="shift">班次</param>
    /// <param name="prodDate">生产日期</param>
    /// <param name="furnaceNo">炉次号</param>
    /// <param name="coilNo">卷号</param>
    /// <param name="subcoilNo">分卷号</param>
    /// <returns>炉号</returns>
    public static string? BuildFurnaceNo(
        string lineNo,
        string shift,
        DateTime? prodDate,
        string furnaceNo,
        string coilNo,
        string subcoilNo
    )
    {
        // 使用新的FurnaceNo类构建
        var furnaceNoObj = FurnaceNo.Build(lineNo, shift, prodDate, furnaceNo, coilNo, subcoilNo);
        return furnaceNoObj?.GetFurnaceNo();
    }
}
