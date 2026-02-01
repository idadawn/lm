using Newtonsoft.Json;

namespace Poxiao.Lab.Helpers;

/// <summary>
/// 检测数据转换辅助类（JSON格式处理）.
/// </summary>
public static class DetectionDataConverter
{
    /// <summary>
    /// 将检测数据字典转换为JSON字符串.
    /// </summary>
    /// <param name="detectionValues">检测数据字典，键为列序号（从1开始），值为检测数据</param>
    /// <returns>JSON字符串，如果字典为空则返回null</returns>
    public static string ToJson(Dictionary<int, decimal?> detectionValues)
    {
        if (detectionValues == null || detectionValues.Count == 0)
            return null;

        return JsonConvert.SerializeObject(detectionValues);
    }

    /// <summary>
    /// 将JSON字符串转换为检测数据字典.
    /// </summary>
    /// <param name="json">JSON字符串</param>
    /// <returns>检测数据字典，如果JSON为空或格式错误则返回空字典</returns>
    public static Dictionary<int, decimal?> FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new Dictionary<int, decimal?>();

        try
        {
            return JsonConvert.DeserializeObject<Dictionary<int, decimal?>>(json)
                ?? new Dictionary<int, decimal?>();
        }
        catch
        {
            return new Dictionary<int, decimal?>();
        }
    }

    /// <summary>
    /// 从Detection1-22字段转换为JSON格式（用于数据迁移）.
    /// </summary>
    /// <param name="detection1">检测数据列1</param>
    /// <param name="detection2">检测数据列2</param>
    /// <param name="detection3">检测数据列3</param>
    /// <param name="detection4">检测数据列4</param>
    /// <param name="detection5">检测数据列5</param>
    /// <param name="detection6">检测数据列6</param>
    /// <param name="detection7">检测数据列7</param>
    /// <param name="detection8">检测数据列8</param>
    /// <param name="detection9">检测数据列9</param>
    /// <param name="detection10">检测数据列10</param>
    /// <param name="detection11">检测数据列11</param>
    /// <param name="detection12">检测数据列12</param>
    /// <param name="detection13">检测数据列13</param>
    /// <param name="detection14">检测数据列14</param>
    /// <param name="detection15">检测数据列15</param>
    /// <param name="detection16">检测数据列16</param>
    /// <param name="detection17">检测数据列17</param>
    /// <param name="detection18">检测数据列18</param>
    /// <param name="detection19">检测数据列19</param>
    /// <param name="detection20">检测数据列20</param>
    /// <param name="detection21">检测数据列21</param>
    /// <param name="detection22">检测数据列22</param>
    /// <returns>JSON字符串</returns>
    public static string FromFixedColumns(
        decimal? detection1, decimal? detection2, decimal? detection3, decimal? detection4,
        decimal? detection5, decimal? detection6, decimal? detection7, decimal? detection8,
        decimal? detection9, decimal? detection10, decimal? detection11, decimal? detection12,
        decimal? detection13, decimal? detection14, decimal? detection15, decimal? detection16,
        decimal? detection17, decimal? detection18, decimal? detection19, decimal? detection20,
        decimal? detection21, decimal? detection22)
    {
        var dict = new Dictionary<int, decimal?>();

        if (detection1.HasValue) dict[1] = detection1.Value;
        if (detection2.HasValue) dict[2] = detection2.Value;
        if (detection3.HasValue) dict[3] = detection3.Value;
        if (detection4.HasValue) dict[4] = detection4.Value;
        if (detection5.HasValue) dict[5] = detection5.Value;
        if (detection6.HasValue) dict[6] = detection6.Value;
        if (detection7.HasValue) dict[7] = detection7.Value;
        if (detection8.HasValue) dict[8] = detection8.Value;
        if (detection9.HasValue) dict[9] = detection9.Value;
        if (detection10.HasValue) dict[10] = detection10.Value;
        if (detection11.HasValue) dict[11] = detection11.Value;
        if (detection12.HasValue) dict[12] = detection12.Value;
        if (detection13.HasValue) dict[13] = detection13.Value;
        if (detection14.HasValue) dict[14] = detection14.Value;
        if (detection15.HasValue) dict[15] = detection15.Value;
        if (detection16.HasValue) dict[16] = detection16.Value;
        if (detection17.HasValue) dict[17] = detection17.Value;
        if (detection18.HasValue) dict[18] = detection18.Value;
        if (detection19.HasValue) dict[19] = detection19.Value;
        if (detection20.HasValue) dict[20] = detection20.Value;
        if (detection21.HasValue) dict[21] = detection21.Value;
        if (detection22.HasValue) dict[22] = detection22.Value;

        return ToJson(dict);
    }

    /// <summary>
    /// 获取检测数据的值列表（按列序号排序）.
    /// </summary>
    /// <param name="json">JSON字符串</param>
    /// <returns>检测数据值列表（已过滤空值，按列序号排序）</returns>
    public static List<decimal> GetValues(string json)
    {
        var dict = FromJson(json);
        return dict
            .Where(kvp => kvp.Value.HasValue)
            .OrderBy(kvp => kvp.Key)
            .Select(kvp => kvp.Value.Value)
            .ToList();
    }

    /// <summary>
    /// 获取检测数据的值列表（按列序号排序，包含空值）.
    /// </summary>
    /// <param name="json">JSON字符串</param>
    /// <returns>检测数据值列表（包含空值，按列序号排序）</returns>
    public static List<decimal?> GetValuesWithNull(string json)
    {
        var dict = FromJson(json);
        return dict
            .OrderBy(kvp => kvp.Key)
            .Select(kvp => kvp.Value)
            .ToList();
    }

    /// <summary>
    /// 获取连续的有效检测列范围（从1开始连续到最后一个有效列）.
    /// </summary>
    /// <param name="json">JSON字符串</param>
    /// <returns>连续的有效检测列范围，例如：如果1-13有值，14开始为空，则返回13</returns>
    public static int GetContinuousColumnCount(string json)
    {
        var dict = FromJson(json);
        if (dict.Count == 0)
            return 0;

        // 找到最大的列序号
        var maxColumn = dict.Keys.Max();

        // 检查从1到maxColumn是否连续
        for (int i = 1; i <= maxColumn; i++)
        {
            if (!dict.ContainsKey(i) || !dict[i].HasValue)
            {
                // 如果中间有空值，返回前一个连续的数量
                return i - 1;
            }
        }

        return maxColumn;
    }

    /// <summary>
    /// 计算平均值.
    /// </summary>
    /// <param name="json">JSON字符串</param>
    /// <returns>平均值，如果没有有效值则返回null</returns>
    public static decimal? GetAverage(string json)
    {
        var values = GetValues(json);
        if (values.Count == 0)
            return null;

        return (decimal)values.Average();
    }

    /// <summary>
    /// 计算最大值.
    /// </summary>
    /// <param name="json">JSON字符串</param>
    /// <returns>最大值，如果没有有效值则返回null</returns>
    public static decimal? GetMax(string json)
    {
        var values = GetValues(json);
        if (values.Count == 0)
            return null;

        return values.Max();
    }

    /// <summary>
    /// 计算最小值.
    /// </summary>
    /// <param name="json">JSON字符串</param>
    /// <returns>最小值，如果没有有效值则返回null</returns>
    public static decimal? GetMin(string json)
    {
        var values = GetValues(json);
        if (values.Count == 0)
            return null;

        return values.Min();
    }
}
