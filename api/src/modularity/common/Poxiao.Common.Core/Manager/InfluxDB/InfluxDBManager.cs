using InfluxData.Net.Common.Enums;
using InfluxData.Net.InfluxDb;
using InfluxData.Net.InfluxDb.Models.Responses;
using NPOI.SS.Formula.Functions;
using Poxiao.DependencyInjection;
using System.Globalization;

namespace Poxiao.Infrastructure.Core.Manager;

/// <summary>
/// InfluxDB 数据库管理.
/// </summary>
public class InfluxDBManager : IInfluxDBManager, ITransient
{
    private InfluxDbClient _client;
    private string _database;

    /// <inheritdoc />
    public void Connect()
    {
        var url = "http://39.106.150.90:48056/";
        var database = "ganwei_collect";
        var username = "influxdb";
        var password = "influxdb";
        _client = new InfluxDbClient(url, username, password, InfluxDbVersion.Latest);
        _database = database;
    }

    /// <inheritdoc />
    public async Task<List<string>> GetAllMeasurementsAsync()
    {
        var query = "SHOW MEASUREMENTS";
        var response = await _client.Client.QueryAsync(query, _database);
        // response 中获取value值.
        var enumerable = response.ToList();
        var list = enumerable.Select(p => p.Values[0][0].ToString()).ToList();
        return list;
    }

    /// <inheritdoc />
    public async Task<List<string>> GetMeasurementSchemaAsync(string measurementName)
    {
        var query = $"SHOW FIELD KEYS FROM \"{measurementName}\"";
        var response = await _client.Client.QueryAsync(query, _database);
        var enumerable = response.ToList();
        var list = enumerable.SelectMany(p => p.Values.Select(v => v[0].ToString())).ToList();
        return list;
    }

    /// <inheritdoc />
    public async Task<List<LinkAttribute>> GetSeriesByMeasurementAsync(string measurementName)
    {
        var attributes = new List<LinkAttribute>();
        var query = $"SHOW SERIES FROM \"{measurementName}\"";
        var response = await _client.Client.QueryAsync(query, _database);
        var enumerable = response.ToList();

        var list = enumerable.SelectMany(p => p.Values.Select(v => string.Join(",", v))).ToList();
        foreach (var info in list)
        {
            var linkAttribute = ParseLinkAttribute(info);
            attributes.Add(linkAttribute);
        }
        return attributes;
    }

    /// <inheritdoc />
    public async Task<List<List<object>>> QueryByKeyAndTimeRangeAsync(string measurementName, string key, int min)
    {
        var query = $"SELECT * FROM \"{measurementName}\" WHERE \"attrName\" = '{key}' AND time > now() - {min}m";
        var response = await _client.Client.QueryAsync(query, _database);
        // 处理和返回结果值.
        var enumerable = response.ToList();
        var list = enumerable.SelectMany(p => p.Values.Select(v => new List<object>() { v[0].ToString(), v[7].ToString() })).ToList();
        return list; // 根据实际情况调整返回值的格式
    }

    /// <inheritdoc />
    public async Task<string> QueryLastAsync(string measurementName, string key)
    {
        var query = $"SELECT * FROM \"{measurementName}\" WHERE \"attrName\" = '{key}' ORDER BY time DESC LIMIT 1";
        var response = await _client.Client.QueryAsync(query, _database);
        // 处理和返回结果值.
        var enumerable = response.ToList();
        var value = enumerable.Select(p => p.Values[0][7].ToString()).FirstOrDefault();
        return value; // 根据实际情况调整返回值的格式
    }

    public LinkAttribute ParseLinkAttribute(string data)
    {
        var linkAttribute = new LinkAttribute();
        var keyValuePairs = data.Split(',');

        foreach (var pair in keyValuePairs)
        {
            var keyValue = pair.Split('=');
            if (keyValue.Length != 2)
                continue; // 跳过格式不正确的键值对

            switch (keyValue[0].Trim())
            {
                case "linkAttrValues":
                    linkAttribute.LinkAttrValues = keyValue[1].Trim();
                    break;
                case "attrDescription":
                    linkAttribute.AttrDescription = keyValue[1].Trim();
                    break;
                case "attrId":
                    linkAttribute.AttrId = int.Parse(keyValue[1].Trim());
                    break;
                case "attrName":
                    linkAttribute.AttrName = keyValue[1].Trim();
                    break;
                case "linkId":
                    linkAttribute.LinkId = int.Parse(keyValue[1].Trim());
                    break;
                case "linkName":
                    linkAttribute.LinkName = keyValue[1].Trim();
                    break;
                case "protocolType":
                    linkAttribute.ProtocolType = keyValue[1].Trim();
                    break;
                case "valueType":
                    linkAttribute.ValueType = keyValue[1].Trim();
                    break;
            }
        }

        return linkAttribute;
    }

}

/// <summary>
///
/// </summary>
public class LinkAttribute
{
    public string LinkAttrValues { get; set; }
    public string AttrDescription { get; set; }
    public int AttrId { get; set; }
    public string AttrName { get; set; }
    public int LinkId { get; set; }
    public string LinkName { get; set; }
    public string ProtocolType { get; set; }
    public string ValueType { get; set; }
}
