using Mapster;
using Microsoft.AspNetCore.Mvc;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.Infrastructure.Filter;
using Poxiao.Lab.Entity;
using Poxiao.Lab.Entity.Dto.SingleSheet;
using Poxiao.Lab.Interfaces;
using SqlSugar;

namespace Poxiao.Lab.Service;

/// <summary>
/// 单片性能原始数据服务.
/// </summary>
[ApiDescriptionSettings(Tag = "Lab", Name = "single-sheet-raw-data", Order = 205)]
[Route("api/lab/single-sheet-raw-data")]
public class SingleSheetRawDataService : ISingleSheetRawDataService, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<SingleSheetRawDataEntity> _repository;

    public SingleSheetRawDataService(ISqlSugarRepository<SingleSheetRawDataEntity> repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    [HttpGet("")]
    public async Task<dynamic> GetList([FromQuery] SingleSheetRawDataListQuery input)
    {
        var data = await _repository
            .AsQueryable()
            .WhereIF(
                !string.IsNullOrEmpty(input.Keyword),
                t =>
                    t.FurnaceNo.Contains(input.Keyword)
                    || t.OriginalFurnaceNo.Contains(input.Keyword)
            )
            .WhereIF(input.StartDate.HasValue, t => t.DetectionTime >= input.StartDate)
            .WhereIF(input.EndDate.HasValue, t => t.DetectionTime <= input.EndDate)
            .WhereIF(
                input.IsValidData.HasValue && input.IsValidData >= 0,
                t => t.IsValid == input.IsValidData
            )
            .OrderByDescending(t => t.DetectionTime)
            .Select(t => new SingleSheetRawDataListOutput
            {
                Id = t.Id,
                OriginalFurnaceNo = t.OriginalFurnaceNo,
                FurnaceNo = t.FurnaceNo,
                PsLoss = t.PsLoss,
                SsPower = t.SsPower,
                Hc = t.Hc,
                DetectionTime = t.DetectionTime,
                IsScratched = t.IsScratched,
                IsValid = t.IsValid,
                ErrorMessage = t.ErrorMessage,
                CreatorTime = t.CreatorTime,
                SortCode = t.SortCode,
            })
            .ToPagedListAsync(input.CurrentPage, input.PageSize);

        return PageResult<SingleSheetRawDataListOutput>.SqlSugarPageResult(data);
    }

    /// <inheritdoc />
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            throw Oops.Oh("数据不存在");

        await _repository.DeleteAsync(entity);
    }
}
