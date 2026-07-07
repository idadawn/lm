using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Filter;
using Poxiao.Lab.Entity;
using Poxiao.Lab.Entity.Dto.DeviceData;
using Poxiao.Lab.Interfaces;
using Poxiao.Logging;
using Poxiao.Systems.Entitys.System;
using SqlSugar;

namespace Poxiao.Lab.Service;

/// <summary>
/// 设备数据采集接入服务.
/// 说明：本期只做"接收+暂存"，采集网关批量上报的原始数据落入暂存表，不流入导入管线。
/// </summary>
[ApiDescriptionSettings(Tag = "Lab", Name = "device-data", Order = 206)]
[Route("api/lab/device-data")]
public class DeviceDataIngestService : IDeviceDataIngestService, IDynamicApiController, ITransient
{
    /// <summary>
    /// 设备允许的编码集合：stacking（叠片机）/ ring-sample（环样机）/ single-sheet（单片机）.
    /// </summary>
    private static readonly HashSet<string> AllowedDeviceCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "stacking",
        "ring-sample",
        "single-sheet",
    };

    private readonly ISqlSugarRepository<DeviceDataInboxEntity> _repository;
    private readonly SqlSugarScope _sqlSugarClient;

    public DeviceDataIngestService(
        ISqlSugarRepository<DeviceDataInboxEntity> repository,
        ISqlSugarClient context
    )
    {
        _repository = repository;
        _sqlSugarClient = (SqlSugarScope)context;
    }

    /// <inheritdoc />
    [AllowAnonymous]
    [HttpPost("batch")]
    public async Task<DeviceDataBatchOutput> Batch([FromBody] DeviceDataBatchInput input)
    {
        await ValidateCollectorAsync();

        if (input == null)
            throw Oops.Oh("请求体不能为空");
        if (string.IsNullOrWhiteSpace(input.DeviceCode) || !AllowedDeviceCodes.Contains(input.DeviceCode.Trim()))
            throw Oops.Oh("设备编码不合法，允许值：stacking/ring-sample/single-sheet");
        if (input.Records == null || input.Records.Count == 0)
            throw Oops.Oh("上报数据不能为空");

        var deviceCode = input.DeviceCode.Trim();
        var output = new DeviceDataBatchOutput { Total = input.Records.Count };

        // 过滤掉源键为空的记录（无法去重）
        var validRecords = input.Records
            .Where(r => !string.IsNullOrWhiteSpace(r.SourceKey))
            .ToList();

        // 查询已存在的 SourceKey（同设备编码下），分批 IN 查询防止 SQL 过长
        var sourceKeys = validRecords.Select(r => r.SourceKey.Trim()).Distinct().ToList();
        var existingKeys = new HashSet<string>();
        if (sourceKeys.Count > 0)
        {
            var batches = sourceKeys.Chunk(1000);
            foreach (var batch in batches)
            {
                var batchList = batch.ToList();
                var found = await _repository
                    .AsQueryable()
                    .Where(t => t.DeviceCode == deviceCode && batchList.Contains(t.SourceKey))
                    .Select(t => t.SourceKey)
                    .ToListAsync();
                foreach (var key in found)
                {
                    existingKeys.Add(key);
                }
            }
        }

        var now = DateTime.Now;
        var newEntities = new List<DeviceDataInboxEntity>();
        var seenInBatch = new HashSet<string>();
        foreach (var record in validRecords)
        {
            var key = record.SourceKey.Trim();
            if (existingKeys.Contains(key) || !seenInBatch.Add(key))
                continue;

            newEntities.Add(new DeviceDataInboxEntity
            {
                Id = Guid.NewGuid().ToString(),
                DeviceCode = deviceCode,
                CollectorId = input.CollectorId,
                SourceKey = key,
                PayloadJson = record.PayloadJson,
                CollectedAt = record.CollectedAt,
                ReceivedAt = now,
                BatchId = input.BatchId,
                ProcessStatus = "pending",
                CreatorUserId = "collector",
                CreatorTime = now,
            });
        }

        output.Duplicated = output.Total - newEntities.Count;
        output.Accepted = newEntities.Count;

        if (newEntities.Count > 0)
        {
            var insertBatches = newEntities.Chunk(1000);
            foreach (var batch in insertBatches)
            {
                await _repository.AsInsertable(batch.ToList()).ExecuteCommandAsync();
            }
        }

        return output;
    }

    /// <inheritdoc />
    [AllowAnonymous]
    [HttpPost("heartbeat")]
    public async Task<dynamic> Heartbeat([FromBody] DeviceDataHeartbeatInput input)
    {
        await ValidateCollectorAsync();

        // 本期仅记录日志，为后续采集网关状态监控留位
        Log.Information($"[DeviceDataIngest] 收到心跳，collectorId={input?.CollectorId}, version={input?.Version}, message={input?.Message}");

        return await Task.FromResult(new { serverTime = DateTime.Now });
    }

    /// <inheritdoc />
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] DeviceDataInboxListQuery input)
    {
        var data = await _repository
            .AsQueryable()
            .WhereIF(!string.IsNullOrEmpty(input.DeviceCode), t => t.DeviceCode == input.DeviceCode)
            .WhereIF(!string.IsNullOrEmpty(input.CollectorId), t => t.CollectorId == input.CollectorId)
            .WhereIF(!string.IsNullOrEmpty(input.ProcessStatus), t => t.ProcessStatus == input.ProcessStatus)
            .WhereIF(input.StartDate.HasValue, t => t.ReceivedAt >= input.StartDate)
            .WhereIF(input.EndDate.HasValue, t => t.ReceivedAt <= input.EndDate)
            .OrderByDescending(t => t.ReceivedAt)
            .Select(t => new DeviceDataInboxListOutput
            {
                Id = t.Id,
                DeviceCode = t.DeviceCode,
                CollectorId = t.CollectorId,
                SourceKey = t.SourceKey,
                PayloadJson = t.PayloadJson,
                CollectedAt = t.CollectedAt,
                ReceivedAt = t.ReceivedAt,
                BatchId = t.BatchId,
                ProcessStatus = t.ProcessStatus,
                ProcessMessage = t.ProcessMessage,
                CreatorTime = t.CreatorTime,
            })
            .ToPagedListAsync(input.CurrentPage, input.PageSize);

        return PageResult<DeviceDataInboxListOutput>.SqlSugarPageResult(data);
    }

    // ================= Private Methods =================

    /// <summary>
    /// 校验采集网关身份。
    /// 说明：本接口 [AllowAnonymous]，无用户上下文（_userManager 不可用），
    /// 鉴权头沿用 DataInterface 系列约定的头名以外，此处自定义 "X-Collector-AppId" / "X-Collector-Secret"
    /// （DataInterface 系列使用的是 "Authorization"/"YmDate" 头，语义上专属于其签名验证流程，此处不复用，避免混淆）。
    /// 校验规则参照 DataInterfaceNewService.VerifyInterfaceOauth：
    /// 1. 记录不存在/未启用/已删除 或 Secret 不匹配 -> IO0003（接口未授权）
    /// 2. 配置了白名单 -> 按 IP 白名单比对，不在白名单 -> D9002
    /// 3. 开启了签名模式（VerifySignature==1）-> 本服务暂不支持，明确提示改用密钥模式
    /// </summary>
    private async Task ValidateCollectorAsync()
    {
        var httpContext = App.HttpContext;
        if (httpContext == null)
            throw Oops.Oh(ErrorCode.IO0003);

        var appId = httpContext.Request.Headers["X-Collector-AppId"].ToString();
        var appSecret = httpContext.Request.Headers["X-Collector-Secret"].ToString();
        if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(appSecret))
            throw Oops.Oh(ErrorCode.IO0001);

        var interfaceEntity = await _sqlSugarClient
            .Queryable<InterfaceOauthEntity>()
            .FirstAsync(x => x.AppId == appId && x.DeleteMark == null && x.EnabledMark == 1);
        if (interfaceEntity == null)
            throw Oops.Oh(ErrorCode.IO0003);

        if (interfaceEntity.WhiteList.IsNotEmptyOrNull())
        {
            var ipList = interfaceEntity.WhiteList.Split(",").ToList();
            if (!ipList.Contains(httpContext.GetLocalIpAddressToIPv4()))
                throw Oops.Oh(ErrorCode.D9002);
        }

        if (interfaceEntity.UsefulLife.IsNotEmptyOrNull() && interfaceEntity.UsefulLife < DateTime.Now)
            throw Oops.Oh(ErrorCode.IO0004);

        if (interfaceEntity.VerifySignature == 1)
        {
            // 采集网关场景暂不支持签名模式，请在接口认证配置中关闭"验证签名"，使用密钥模式对接
            throw Oops.Oh("该采集网关暂不支持签名验证模式，请使用密钥模式（AppId/AppSecret）对接");
        }

        if (interfaceEntity.AppSecret != appSecret)
            throw Oops.Oh(ErrorCode.IO0003);
    }
}
