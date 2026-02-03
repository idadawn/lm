using System;
using System.Text.RegularExpressions;

namespace Poxiao.Lab.Entity.Models;

/// <summary>
/// 炉号封装类
/// 格式：[产线数字][班次汉字][8位日期]-[炉次号]-[卷号]-[分卷号][可选特性汉字]
/// 示例：1甲20251101-1-4-1脆
/// </summary>
public class FurnaceNo
{
    /// <summary>
    /// 产线数字（如：1, 2, 3等）
    /// </summary>
    public string LineNo { get; private set; }

    /// <summary>
    /// 班次汉字（如：甲, 乙, 丙等）
    /// </summary>
    public string Shift { get; private set; }

    /// <summary>
    /// 生产日期
    /// </summary>
    public DateTime? ProdDate { get; private set; }

    /// <summary>
    /// 炉次号（如：1, 2, 3等）
    /// </summary>
    public string FurnaceBatchNo { get; private set; }

    /// <summary>
    /// 卷号（支持小数，如：1, 1.5, 2等）
    /// </summary>
    public string CoilNo { get; private set; }

    /// <summary>
    /// 分卷号（支持小数，如：1, 1.5, 2等）
    /// </summary>
    public string SubcoilNo { get; private set; }

    /// <summary>
    /// 特性描述（特性汉字，如：脆, 硬等）
    /// </summary>
    public string FeatureSuffix { get; private set; }

    /// <summary>
    /// 产线数字（用于排序）
    /// </summary>
    public int? LineNoNumeric { get; private set; }

    /// <summary>
    /// 班次数字（用于排序：甲=1, 乙=2, 丙=3）
    /// </summary>
    public int? ShiftNumeric { get; private set; }

    /// <summary>
    /// 炉次号数字（用于排序）
    /// </summary>
    public int? FurnaceBatchNoNumeric { get; private set; }

    /// <summary>
    /// 卷号数字（用于排序，支持小数）
    /// </summary>
    public decimal? CoilNoNumeric { get; private set; }

    /// <summary>
    /// 分卷号数字（用于排序，支持小数）
    /// </summary>
    public decimal? SubcoilNoNumeric { get; private set; }

    /// <summary>
    /// 是否解析成功
    /// </summary>
    public bool IsValid { get; private set; }

    /// <summary>
    /// 错误信息（如果解析失败）
    /// </summary>
    public string ErrorMessage { get; private set; }

    /// <summary>
    /// 原始炉号字符串
    /// </summary>
    public string OriginalFurnaceNo { get; private set; }

    /// <summary>
    /// 私有构造函数，用于内部创建实例
    /// </summary>
    private FurnaceNo()
    {
    }

    /// <summary>
    /// 从字符串解析炉号
    /// </summary>
    /// <param name="furnaceNo">原始炉号字符串</param>
    /// <param name="ignoredSuffixes">需要忽略的后缀列表（如“复测”）</param>
    /// <returns>炉号实例</returns>
    public static FurnaceNo Parse(string furnaceNo, IEnumerable<string> ignoredSuffixes = null)
    {
        var result = new FurnaceNo { OriginalFurnaceNo = furnaceNo, IsValid = false };

        if (string.IsNullOrWhiteSpace(furnaceNo))
        {
            result.ErrorMessage = "炉号为空";
            return result;
        }

        // 正则表达式：匹配 [产线数字][班次汉字][8位日期]-[炉次号]-[卷号]-[分卷号][可选特性汉字]
        // 例如：1甲20251101-1-4-1脆 或 1乙20251101-1-1-1
        // 卷号和分卷号支持小数：\d+(\.\d+)?
        // 允许炉号、卷号、分卷号前后有空格
        // 特性描述限制为中文汉字或括号：[\u4e00-\u9fa5\(\)\uff08\uff09]*
        var pattern =
            @"^\s*(\d+)(.*?)(\d{8})\s*-\s*(\d+)\s*-\s*(\d+(?:\.\d+)?)\s*-\s*(\d+(?:\.\d+)?)\s*([\u4e00-\u9fa5\(\)\uff08\uff09]*)\s*$";
        var match = Regex.Match(furnaceNo.Trim(), pattern);

        if (!match.Success)
        {
            result.ErrorMessage = $"炉号格式不符合规则: '{furnaceNo}'";
            return result;
        }

        try
        {
            result.LineNo = match.Groups[1].Value; // 产线
            result.Shift = match.Groups[2].Value.Trim(); // 班次（去除首尾空白）
            var dateStr = match.Groups[3].Value; // 日期字符串
            result.FurnaceBatchNo = match.Groups[4].Value; // 炉次号
            result.CoilNo = match.Groups[5].Value; // 卷号
            result.SubcoilNo = match.Groups[6].Value; // 分卷号
            result.FeatureSuffix = match.Groups[7].Value?.Trim(); // 特性描述（可选）

            // 忽略特定的非特性后缀（如复测）
            if (!string.IsNullOrEmpty(result.FeatureSuffix))
            {
                // 如果传入了忽略列表，使用传入的；否则使用默认的
                var ignores = ignoredSuffixes ?? new[] { "复测", "（复测）", "(复测)" };
                if (ignores.Any(s => s == result.FeatureSuffix))
                {
                    result.FeatureSuffix = null;
                }
            }

            // 解析日期
            if (
                DateTime.TryParseExact(
                    dateStr,
                    "yyyyMMdd",
                    null,
                    System.Globalization.DateTimeStyles.None,
                    out var date
                )
            )
            {
                result.ProdDate = date;
            }
            else
            {
                result.ErrorMessage = $"日期格式错误：无法解析日期 {dateStr}";
                return result;
            }

            // 解析数字字段（用于排序）
            if (int.TryParse(result.LineNo, out var lineNoNum))
            {
                result.LineNoNumeric = lineNoNum;
            }

            // 班次转换为数字：甲=1, 乙=2, 丙=3
            result.ShiftNumeric = ConvertShiftToNumeric(result.Shift);

            if (int.TryParse(result.FurnaceBatchNo, out var furnaceBatchNoNum))
            {
                result.FurnaceBatchNoNumeric = furnaceBatchNoNum;
            }

            // 卷号支持小数
            if (decimal.TryParse(result.CoilNo, out var coilNoNum))
            {
                result.CoilNoNumeric = coilNoNum;
            }

            // 分卷号支持小数
            if (decimal.TryParse(result.SubcoilNo, out var subcoilNoNum))
            {
                result.SubcoilNoNumeric = subcoilNoNum;
            }

            result.IsValid = true;
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"解析失败：{ex.Message}";
        }

        return result;
    }

    /// <summary>
    /// 尝试解析炉号，如果失败返回null
    /// </summary>
    /// <param name="furnaceNo">原始炉号字符串</param>
    /// <param name="ignoredSuffixes">需要忽略的后缀列表</param>
    /// <returns>炉号实例，如果解析失败返回null</returns>
    public static FurnaceNo TryParse(string furnaceNo, IEnumerable<string> ignoredSuffixes = null)
    {
        var result = Parse(furnaceNo, ignoredSuffixes);
        return result.IsValid ? result : null;
    }

    /// <summary>
    /// 从各部分构建炉号
    /// </summary>
    /// <param name="lineNo">产线</param>
    /// <param name="shift">班次</param>
    /// <param name="prodDate">生产日期</param>
    /// <param name="furnaceBatchNo">炉次号</param>
    /// <param name="coilNo">卷号</param>
    /// <param name="subcoilNo">分卷号</param>
    /// <param name="featureSuffix">特性描述（可选）</param>
    /// <returns>炉号实例</returns>
    public static FurnaceNo Build(
        string lineNo,
        string shift,
        DateTime? prodDate,
        string furnaceBatchNo,
        string coilNo,
        string subcoilNo,
        string featureSuffix = null
    )
    {
        var result = new FurnaceNo
        {
            LineNo = lineNo,
            Shift = shift,
            ProdDate = prodDate,
            FurnaceBatchNo = furnaceBatchNo,
            CoilNo = coilNo,
            SubcoilNo = subcoilNo,
            FeatureSuffix = featureSuffix,
            OriginalFurnaceNo = null,
        };

        // 验证必要字段
        if (
            string.IsNullOrWhiteSpace(lineNo)
            || string.IsNullOrWhiteSpace(shift)
            || !prodDate.HasValue
            || string.IsNullOrWhiteSpace(furnaceBatchNo)
            || string.IsNullOrWhiteSpace(coilNo)
            || string.IsNullOrWhiteSpace(subcoilNo)
        )
        {
            result.IsValid = false;
            result.ErrorMessage = "缺少必要的炉号组成部分";
            return result;
        }

        // 解析数字字段
        if (int.TryParse(lineNo, out var lineNoNum))
        {
            result.LineNoNumeric = lineNoNum;
        }

        result.ShiftNumeric = ConvertShiftToNumeric(shift);

        if (int.TryParse(furnaceBatchNo, out var furnaceBatchNoNum))
        {
            result.FurnaceBatchNoNumeric = furnaceBatchNoNum;
        }

        if (decimal.TryParse(coilNo, out var coilNoNum))
        {
            result.CoilNoNumeric = coilNoNum;
        }

        if (decimal.TryParse(subcoilNo, out var subcoilNoNum))
        {
            result.SubcoilNoNumeric = subcoilNoNum;
        }

        // 构建原始炉号字符串
        var dateStr = prodDate.Value.ToString("yyyyMMdd");
        result.OriginalFurnaceNo =
            $"{lineNo}{shift}{dateStr}-{furnaceBatchNo}-{coilNo}-{subcoilNo}";
        if (!string.IsNullOrWhiteSpace(featureSuffix))
        {
            result.OriginalFurnaceNo += featureSuffix;
        }

        result.IsValid = true;
        return result;
    }

    /// <summary>
    /// 获取完整炉号字符串（包含特性描述）
    /// </summary>
    /// <returns>完整炉号字符串</returns>
    public string GetFullFurnaceNo()
    {
        if (!IsValid || string.IsNullOrWhiteSpace(OriginalFurnaceNo))
        {
            // 如果原始炉号不存在，构建一个
            if (!ProdDate.HasValue)
                return null;

            var dateStr = ProdDate.Value.ToString("yyyyMMdd");
            var furnaceNo = $"{LineNo}{Shift}{dateStr}-{FurnaceBatchNo}-{CoilNo}-{SubcoilNo}";
            if (!string.IsNullOrWhiteSpace(FeatureSuffix))
            {
                furnaceNo += FeatureSuffix;
            }
            return furnaceNo;
        }

        return OriginalFurnaceNo;
    }

    /// <summary>
    /// 获取基础炉号（不包含特性描述）
    /// 格式：[产线数字][班次汉字][8位日期]-[炉次号]-[卷号]-[分卷号]
    /// </summary>
    /// <returns>基础炉号字符串</returns>
    public string GetFurnaceNo()
    {
        if (!IsValid || !ProdDate.HasValue)
            return null;

        var dateStr = ProdDate.Value.ToString("yyyyMMdd");
        return $"{LineNo}{Shift}{dateStr}-{FurnaceBatchNo}-{CoilNo}-{SubcoilNo}";
    }

    /// <summary>
    /// 获取标准炉号（等同于批次号，即 [产线数字][班次汉字][8位日期]-[炉次号]）
    /// </summary>
    /// <returns>标准炉号字符串</returns>
    public string GetStandardFurnaceNo() => GetBatchNo();

    /// <summary>
    /// 获取喷次
    /// 格式：[8位日期]-[炉次号]
    /// </summary>
    /// <returns>喷次字符串</returns>
    public string GetSprayNo()
    {
        if (!IsValid || !ProdDate.HasValue || string.IsNullOrEmpty(FurnaceBatchNo))
            return null;

        var dateStr = ProdDate.Value.ToString("yyyyMMdd");
        return $"{dateStr}-{FurnaceBatchNo}";
    }

    /// <summary>
    /// 获取批次
    /// 格式：[产线数字][班次汉字][8位日期]-[炉次号]
    /// </summary>
    /// <returns>批次字符串</returns>
    public string GetBatchNo()
    {
        if (!IsValid || !ProdDate.HasValue || string.IsNullOrEmpty(FurnaceBatchNo))
            return null;

        var dateStr = ProdDate.Value.ToString("yyyyMMdd");
        return $"{LineNo}{Shift}{dateStr}-{FurnaceBatchNo}";
    }

    /// <summary>
    /// 移除特性描述
    /// </summary>
    /// <returns>移除特性描述后的炉号实例</returns>
    public FurnaceNo WithoutFeatureSuffix()
    {
        if (!IsValid)
            return this;

        return Build(LineNo, Shift, ProdDate, FurnaceBatchNo, CoilNo, SubcoilNo, null);
    }

    /// <summary>
    /// 将班次汉字转换为数字（用于排序）
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
    /// 重写ToString方法，返回完整炉号
    /// </summary>
    /// <returns>完整炉号字符串</returns>
    public override string ToString()
    {
        return GetFullFurnaceNo() ?? OriginalFurnaceNo ?? base.ToString();
    }

    /// <summary>
    /// 重写Equals方法，比较标准炉号
    /// </summary>
    /// <param name="obj">比较对象</param>
    /// <returns>是否相等</returns>
    public override bool Equals(object obj)
    {
        if (obj is FurnaceNo other)
        {
            return GetFurnaceNo() == other.GetFurnaceNo();
        }
        return false;
    }

    /// <summary>
    /// 重写GetHashCode方法，基于标准炉号
    /// </summary>
    /// <returns>哈希码</returns>
    public override int GetHashCode()
    {
        return GetFurnaceNo()?.GetHashCode() ?? 0;
    }

    /// <summary>
    /// 相等运算符
    /// </summary>
    public static bool operator ==(FurnaceNo left, FurnaceNo right)
    {
        if (ReferenceEquals(left, right))
            return true;
        if (left is null || right is null)
            return false;
        return left.Equals(right);
    }

    /// <summary>
    /// 不等运算符
    /// </summary>
    public static bool operator !=(FurnaceNo left, FurnaceNo right)
    {
        return !(left == right);
    }
}
