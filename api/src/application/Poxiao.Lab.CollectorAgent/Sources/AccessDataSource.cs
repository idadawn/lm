using System.Data;
using System.Data.OleDb;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Poxiao.Lab.CollectorAgent.Options;

namespace Poxiao.Lab.CollectorAgent.Sources;

/// <summary>
/// Access（.mdb/.accdb）数据源：通过 OleDb + Microsoft.ACE.OLEDB.12.0 驱动只读采集。
/// 仅支持 Windows；驱动缺失或初始化失败时抛出带安装指引的异常，由调用方（PollingWorker）
/// 捕获后按源禁用退避重试，不影响进程内其他数据源。
/// </summary>
[SupportedOSPlatform("windows")]
public class AccessDataSource : IDeviceDataSource
{
    private readonly SourceOptions _sourceOptions;
    private readonly AccessSourceOptions _accessOptions;

    public AccessDataSource(SourceOptions sourceOptions)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            throw new PlatformNotSupportedException(
                $"数据源 '{sourceOptions.Name}' 类型为 access，但 OleDb/ACE 驱动仅支持 Windows。当前平台不受支持。");
        }

        _sourceOptions = sourceOptions;
        _accessOptions = sourceOptions.Access
            ?? throw new InvalidOperationException($"数据源 '{sourceOptions.Name}' 类型为 access，但未配置 Access 节点。");

        if (string.IsNullOrWhiteSpace(_accessOptions.FilePath))
        {
            throw new InvalidOperationException($"数据源 '{sourceOptions.Name}' 缺少 Access.FilePath 配置。");
        }

        if (string.IsNullOrWhiteSpace(_accessOptions.Table) || string.IsNullOrWhiteSpace(_accessOptions.KeyColumn))
        {
            throw new InvalidOperationException($"数据源 '{sourceOptions.Name}' 缺少 Access.Table 或 Access.KeyColumn 配置。");
        }
    }

    public async Task<List<CollectedRecord>> FetchNewAsync(string lastPosition, int batchSize, CancellationToken cancellationToken)
    {
        var connectionString = BuildConnectionString();
        var sql = BuildQuery(batchSize);

        await using var connection = new OleDbConnection(connectionString);
        try
        {
            await connection.OpenAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"数据源 '{_sourceOptions.Name}' 连接 Access 数据库失败（{_accessOptions.FilePath}）。" +
                "请确认已安装与进程位数匹配的 Access Database Engine 2016 Redistributable " +
                "(https://www.microsoft.com/download/details.aspx?id=54920)，且文件路径/密码正确。",
                ex);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.CommandType = CommandType.Text;
        AddParameter(command, "lastPosition", lastPosition);

        var records = new List<CollectedRecord>();
        var now = DateTimeOffset.Now;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var payload = new Dictionary<string, object?>();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                payload[name] = value;
            }

            var position = payload.TryGetValue(_accessOptions.KeyColumn, out var keyValue) && keyValue is not null
                ? Convert.ToString(keyValue) ?? string.Empty
                : string.Empty;

            records.Add(new CollectedRecord
            {
                SourceKey = _sourceOptions.Name,
                CollectedAt = now,
                Position = position,
                Payload = payload,
            });
        }

        return records;
    }

    private string BuildConnectionString()
    {
        var builder = new System.Text.StringBuilder();
        builder.Append("Provider=Microsoft.ACE.OLEDB.12.0;");
        builder.Append($"Data Source={_accessOptions.FilePath};");
        builder.Append("Mode=Read;");
        if (!string.IsNullOrEmpty(_accessOptions.Password))
        {
            builder.Append($"Jet OLEDB:Database Password={_accessOptions.Password};");
        }

        return builder.ToString();
    }

    private string BuildQuery(int batchSize)
    {
        var columns = _accessOptions.Columns is { Count: > 0 }
            ? string.Join(", ", _accessOptions.Columns.Select(WrapIdentifier))
            : "*";

        var keyColumn = WrapIdentifier(_accessOptions.KeyColumn);
        // OleDb 使用位置参数（?），非命名参数；只有一个参数时顺序天然对应。
        var whereClauses = new List<string> { $"{keyColumn} > ?" };

        if (!string.IsNullOrWhiteSpace(_accessOptions.CompleteFlagColumn) && !string.IsNullOrWhiteSpace(_accessOptions.CompleteFlagValue))
        {
            var flagColumn = WrapIdentifier(_accessOptions.CompleteFlagColumn);
            var flagValue = _accessOptions.CompleteFlagValue!.Replace("'", "''");
            whereClauses.Add($"{flagColumn} = '{flagValue}'");
        }

        var where = string.Join(" AND ", whereClauses);
        var table = WrapIdentifier(_accessOptions.Table);

        // Access 方言：TOP N 而非 LIMIT。
        return $"SELECT TOP {batchSize} {columns} FROM {table} WHERE {where} ORDER BY {keyColumn}";
    }

    private static string WrapIdentifier(string identifier) => $"[{identifier}]";

    private static void AddParameter(OleDbCommand command, string name, object? value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = string.IsNullOrEmpty(value as string) ? DBNull.Value : value;
        command.Parameters.Add(parameter);
    }
}
