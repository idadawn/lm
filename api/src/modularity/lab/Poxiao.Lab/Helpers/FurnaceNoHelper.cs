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
        /// 炉号（解析后的炉号数字部分）.
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
        /// 卷号数字（用于排序）.
        /// </summary>
        public int? CoilNoNumeric { get; set; }

        /// <summary>
        /// 分卷号数字（用于排序）.
        /// </summary>
        public int? SubcoilNoNumeric { get; set; }
    }

    /// <summary>
    /// 解析炉号.
    /// 格式：[产线数字][班次汉字][8位日期]-[炉号]-[卷号]-[分卷号][可选特性汉字]
    /// 示例：1甲20251101-1-4-1脆
    /// </summary>
    /// <param name="furnaceNo">原始炉号</param>
    /// <returns>解析结果</returns>
    public static FurnaceNoParseResult ParseFurnaceNo(string furnaceNo)
    {
        var result = new FurnaceNoParseResult();

        if (string.IsNullOrWhiteSpace(furnaceNo))
        {
            result.ErrorMessage = "炉号为空";
            return result;
        }

        // 正则表达式：匹配 [产线数字][班次汉字][8位日期]-[炉号]-[卷号]-[分卷号][可选特性汉字]
        // 例如：1甲20251101-1-4-1脆
        var pattern = @"^(\d+)([^\d]+)(\d{8})-(\d+)-(\d+)-(\d+)(.*)$";
        var match = Regex.Match(furnaceNo, pattern);

        if (!match.Success)
        {
            result.ErrorMessage = "炉号格式不符合规则";
            return result;
        }

        try
        {
            result.LineNo = match.Groups[1].Value; // 产线
            result.Shift = match.Groups[2].Value; // 班次
            var dateStr = match.Groups[3].Value; // 日期字符串
            result.FurnaceNo = match.Groups[4].Value; // 炉号
            result.CoilNo = match.Groups[5].Value; // 卷号
            result.SubcoilNo = match.Groups[6].Value; // 分卷号
            result.FeatureSuffix = match.Groups[7].Value?.Trim(); // 特性描述（可选）

            // 解析日期
            if (DateTime.TryParseExact(
                dateStr,
                "yyyyMMdd",
                null,
                System.Globalization.DateTimeStyles.None,
                out var date))
            {
                result.ProdDate = date;
            }
            else
            {
                result.ErrorMessage = "日期格式错误";
                return result;
            }

            // 解析数字字段（用于排序）
            if (int.TryParse(result.LineNo, out var lineNoNum))
            {
                result.LineNoNumeric = lineNoNum;
            }

            // 班次转换为数字：甲=1, 乙=2, 丙=3
            result.ShiftNumeric = ConvertShiftToNumeric(result.Shift);

            if (int.TryParse(result.FurnaceNo, out var furnaceNoNum))
            {
                result.FurnaceNoNumeric = furnaceNoNum;
            }

            if (int.TryParse(result.CoilNo, out var coilNoNum))
            {
                result.CoilNoNumeric = coilNoNum;
            }

            if (int.TryParse(result.SubcoilNo, out var subcoilNoNum))
            {
                result.SubcoilNoNumeric = subcoilNoNum;
            }

            result.Success = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"解析失败：{ex.Message}";
        }

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
    /// <returns>去掉特性汉字后的炉号</returns>
    public static string RemoveFeatureSuffix(string furnaceNo, string featureSuffix = null)
    {
        if (string.IsNullOrWhiteSpace(furnaceNo))
            return furnaceNo;

        // 如果已知特性汉字，直接移除
        if (!string.IsNullOrWhiteSpace(featureSuffix))
        {
            return furnaceNo.Replace(featureSuffix, "").TrimEnd();
        }

        // 否则，先解析获取特性汉字，再移除
        var parseResult = ParseFurnaceNo(furnaceNo);
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
    public static bool IsValidFurnaceNo(string furnaceNo)
    {
        if (string.IsNullOrWhiteSpace(furnaceNo))
            return false;

        var parseResult = ParseFurnaceNo(furnaceNo);
        return parseResult.Success;
    }

    /// <summary>
    /// 构建标准炉号（去掉特性汉字后的标准格式）.
    /// 格式：[产线数字][班次汉字][8位日期]-[炉号]-[卷号]-[分卷号]
    /// </summary>
    /// <param name="lineNo">产线</param>
    /// <param name="shift">班次</param>
    /// <param name="prodDate">生产日期</param>
    /// <param name="furnaceNo">炉号</param>
    /// <param name="coilNo">卷号</param>
    /// <param name="subcoilNo">分卷号</param>
    /// <returns>标准炉号</returns>
    public static string BuildStandardFurnaceNo(
        string lineNo,
        string shift,
        DateTime? prodDate,
        string furnaceNo,
        string coilNo,
        string subcoilNo)
    {
        if (string.IsNullOrWhiteSpace(lineNo) ||
            string.IsNullOrWhiteSpace(shift) ||
            !prodDate.HasValue ||
            string.IsNullOrWhiteSpace(furnaceNo) ||
            string.IsNullOrWhiteSpace(coilNo) ||
            string.IsNullOrWhiteSpace(subcoilNo))
        {
            return null;
        }

        var dateStr = prodDate.Value.ToString("yyyyMMdd");
        return $"{lineNo}{shift}{dateStr}-{furnaceNo}-{coilNo}-{subcoilNo}";
    }
}
