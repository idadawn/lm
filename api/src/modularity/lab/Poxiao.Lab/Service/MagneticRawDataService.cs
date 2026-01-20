using Mapster;
using Microsoft.AspNetCore.Mvc;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.Infrastructure.Filter;
using Poxiao.Lab.Entity;
using Poxiao.Lab.Entity.Dto.MagneticData;
using Poxiao.Lab.Interfaces;
using SqlSugar;

namespace Poxiao.Lab.Service;

/// <summary>
/// 磁性原始数据服务.
/// </summary>
[ApiDescriptionSettings(Tag = "Lab", Name = "magnetic-raw-data", Order = 203)]
[Route("api/lab/magnetic-raw-data")]
public class MagneticRawDataService : IMagneticRawDataService, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<MagneticRawDataEntity> _repository;

    public MagneticRawDataService(ISqlSugarRepository<MagneticRawDataEntity> repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    [HttpGet("")]
    public async Task<List<MagneticRawDataListOutput>> GetList(
        [FromQuery] MagneticRawDataListQuery input
    )
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
            .Select(t => new MagneticRawDataListOutput
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

        return data.list.ToList();
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
