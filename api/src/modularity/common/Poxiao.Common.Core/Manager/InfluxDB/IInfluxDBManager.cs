using InfluxData.Net.InfluxDb.Models.Responses;

namespace Poxiao.Infrastructure.Core.Manager;

/// <summary>
/// InfluxDB 数据库管理.
/// </summary>
public interface IInfluxDBManager
{
    /// <summary>
    /// 
    /// </summary>
    void Connect();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<List<string>> GetAllMeasurementsAsync();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="measurementName"></param>
    /// <returns></returns>
    Task<List<string>> GetMeasurementSchemaAsync(string measurementName);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="measurementName"></param>
    /// <returns></returns>
    Task<List<LinkAttribute>> GetSeriesByMeasurementAsync(string measurementName);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="measurementName"></param>
    /// <param name="key"></param>
    /// <param name="min"></param>
    /// <returns></returns>
    Task<List<List<object>>> QueryByKeyAndTimeRangeAsync(string measurementName, string key, int min);

    /// <summary>
    /// 获取最新的一条数据.
    /// </summary>
    /// <param name="measurementName"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    Task<string> QueryLastAsync(string measurementName, string key);
}