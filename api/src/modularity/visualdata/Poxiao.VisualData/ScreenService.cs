using Poxiao.Infrastructure.Configuration;
using Poxiao.Infrastructure.Core.Manager.Files;
using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Security;
using Poxiao.DatabaseAccessor;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.Logging.Attributes;
using Poxiao.VisualData.Entity;
using Poxiao.VisualData.Entitys.Dto.Screen;
using Poxiao.VisualData.Entitys.Dto.ScreenCategory;
using Poxiao.VisualData.Entitys.Dto.ScreenConfig;
using Poxiao.VisualData.Entitys.Enum;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SqlSugar;

namespace Poxiao.VisualData;

/// <summary>
/// 业务实现：大屏.
/// </summary>
[ApiDescriptionSettings(Tag = "BladeVisual", Name = "Visual", Order = 160)]
[Route("api/blade-visual/[controller]")]
public class ScreenService : IDynamicApiController, ITransient
{
    /// <summary>
    /// 服务基础仓储.
    /// </summary>
    private readonly ISqlSugarRepository<VisualEntity> _visualRepository;

    /// <summary>
    /// 文件服务.
    /// </summary>
    private readonly IFileManager _fileManager;

    /// <summary>
    /// 多租户事务.
    /// </summary>
    private readonly ITenant _db;

    /// <summary>
    /// 初始化一个<see cref="ScreenService"/>类型的新实例.
    /// </summary>
    public ScreenService(
        ISqlSugarRepository<VisualEntity> visualRepository,
        IFileManager fileManager,
        ISqlSugarClient context)
    {
        _visualRepository = visualRepository;
        _fileManager = fileManager;
        _db = context.AsTenant();
    }

    #region Get

    /// <summary>
    /// 分页.
    /// </summary>
    /// <returns></returns>
    [HttpGet("list")]
    public async Task<dynamic> GetList([FromQuery] ScreenListQueryInput input)
    {
        input.category = input.category == null ? 1 : input.category;
        SqlSugarPagedList<ScreenListOutput>? data = await _visualRepository.AsQueryable().Where(v => v.IsDeleted == 0).Where(v => v.Category == input.category)
            .Select(v => new ScreenListOutput
            {
                id = v.Id,
                backgroundUrl = v.BackgroundUrl,
                category = v.Category,
                createDept = v.CreateDept,
                createTime = SqlFunc.ToString(v.CreateTime),
                createUser = v.CreateUser,
                isDeleted = v.IsDeleted,
                password = v.Password,
                status = v.Status,
                title = v.Title,
                updateTime = SqlFunc.ToString(v.UpdateTime),
                updateUser = v.UpdateUser
            }).MergeTable().OrderByDescending(v => v.createTime).OrderByDescending(v => v.updateTime).ToPagedListAsync(input.current, input.size);
        return new { current = data.pagination.CurrentPage, pages = data.pagination.Total / data.pagination.PageSize, records = data.list, size = data.pagination.PageSize, total = data.pagination.Total };
    }

    /// <summary>
    /// 详情.
    /// </summary>
    /// <returns></returns>
    [HttpGet("detail")]
    public async Task<dynamic> GetInfo([FromQuery] string id)
    {
        VisualEntity? entity = await _visualRepository.AsQueryable().FirstAsync(v => v.Id == id);
        VisualConfigEntity? configEntity = await _visualRepository.AsSugarClient().Queryable<VisualConfigEntity>().FirstAsync(v => v.VisualId == id);
        return new { config = configEntity.Adapt<ScreenConfigInfoOutput>(), visual = entity.Adapt<ScreenInfoOutput>() };
    }

    /// <summary>
    /// 获取类型.
    /// </summary>
    /// <returns></returns>
    [HttpGet("category")]
    public async Task<dynamic> GetCategoryList()
    {
        VisualCategoryEntity? entity = await _visualRepository.AsSugarClient().Queryable<VisualCategoryEntity>().FirstAsync(v => v.IsDeleted == 0);
        return entity.Adapt<ScreenCategoryListOutput>();
    }

    /// <summary>
    /// 获取图片列表.
    /// </summary>
    /// <returns></returns>
    [HttpGet("{type}")]
    public dynamic GetImgFileList(string type)
    {
        List<ScreenImgFileOutput>? list = new List<ScreenImgFileOutput>();
        var typeEnum = EnumExtensions.GetEnumDescDictionary(typeof(ScreenImgEnum));
        var imgEnum = typeEnum.Where(t => t.Value.Equals(type)).FirstOrDefault();
        if (imgEnum.Value != null)
        {
            string? path = Path.Combine(FileVariable.BiVisualPath, imgEnum.Value);
            foreach (FileInfo? item in FileHelper.GetAllFiles(path))
            {
                list.Add(new ScreenImgFileOutput()
                {
                    link = string.Format("/api/file/VisusalImg/BiVisualPath/{0}/{1}", type, item.Name),
                    originalName = item.Name
                });
            }
        }

        return list;
    }

    /// <summary>
    /// 查看图片.
    /// </summary>
    /// <returns></returns>
    [HttpGet("{type}/{fileName}"), AllowAnonymous, IgnoreLog]
    public async Task<dynamic> GetImgFile(string type, string fileName)
    {
        var typeEnum = EnumExtensions.GetEnumDescDictionary(typeof(ScreenImgEnum));
        var imgEnum = typeEnum.Where(t => t.Value.Equals(type)).FirstOrDefault();
        if (imgEnum.Value != null)
        {
            string? savePath = Path.Combine(FileVariable.BiVisualPath, imgEnum.Value, fileName);

            // 得到数据
            var sr = await _fileManager.GetFileStream(savePath);
            return new FileStreamResult(sr, "application/octet-stream") { FileDownloadName = fileName };
        }

        return Task.FromResult(false);
    }

    /// <summary>
    /// 大屏下拉框.
    /// </summary>
    /// <returns></returns>
    [HttpGet("selector")]
    public async Task<dynamic> GetSelector()
    {
        List<ScreenSelectorOuput>? screenList = await _visualRepository.AsQueryable().Where(v => v.IsDeleted == 0)
            .Select(v => new ScreenSelectorOuput
            {
                Id = v.Id,
                ParentId = SqlFunc.ToString(v.Category),
                fullName = v.Title,
                isDeleted = v.IsDeleted
            }).ToListAsync();
        List<ScreenSelectorOuput>? categortList = await _visualRepository.AsSugarClient().Queryable<VisualCategoryEntity>().Where(v => v.IsDeleted == 0)
            .Select(v => new ScreenSelectorOuput
            {
                Id = v.CategoryValue,
                ParentId = "0",
                fullName = v.CategoryKey,
                isDeleted = v.IsDeleted
            }).ToListAsync();
        return new { list = categortList.Union(screenList).ToList().ToTree("0") };
    }

    #endregion

    #region Post

    /// <summary>
    /// 新增.
    /// </summary>
    /// <returns></returns>
    [HttpPost("save")]
    public async Task<dynamic> Save([FromBody] ScreenCrInput input)
    {
        VisualEntity? entity = input.visual.Adapt<VisualEntity>();
        VisualConfigEntity? configEntity = input.config.Adapt<VisualConfigEntity>();

        try
        {
            _db.BeginTran(); // 开启事务
            VisualEntity? newEntity = await _visualRepository.AsInsertable(entity).CallEntityMethod(m => m.Create()).ExecuteReturnEntityAsync();
            configEntity.VisualId = newEntity.Id;
            await _visualRepository.AsSugarClient().Insertable(configEntity).CallEntityMethod(m => m.Create()).ExecuteCommandAsync();

            _db.CommitTran();

            return new { id = newEntity.Id };
        }
        catch (Exception)
        {
            _db.RollbackTran();
            throw Oops.Oh(ErrorCode.COM1000);
        }
    }

    /// <summary>
    /// 修改.
    /// </summary>
    /// <returns></returns>
    [HttpPost("update")]
    public async Task Update([FromBody] ScreenUpInput input)
    {
        VisualEntity? entity = new VisualEntity();
        if (input.visual.backgroundUrl != null)
        {
            entity = await _visualRepository.AsQueryable().FirstAsync(v => v.Id == input.visual.id);
            entity.BackgroundUrl = input.visual.backgroundUrl;
        }
        else
        {
            entity = input.visual.Adapt<VisualEntity>();
        }

        VisualConfigEntity? configEntity = input.config.Adapt<VisualConfigEntity>();
        try
        {
            _db.BeginTran(); // 开启事务

            await _visualRepository.AsUpdateable(entity).IgnoreColumns(ignoreAllNullColumns: true).CallEntityMethod(m => m.LastModify()).ExecuteCommandAsync();
            if (configEntity != null)
                await _visualRepository.AsSugarClient().Updateable(configEntity).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();

            _db.CommitTran();
        }
        catch (Exception)
        {
            _db.RollbackTran();
            throw Oops.Oh(ErrorCode.COM1000);
        }
    }

    /// <summary>
    /// 逻辑删除.
    /// </summary>
    /// <returns></returns>
    [HttpPost("remove")]
    public async Task Remove(string ids)
    {
        VisualEntity? entity = await _visualRepository.AsQueryable().FirstAsync(v => v.Id == ids);
        await _visualRepository.AsUpdateable(entity).CallEntityMethod(m => m.Delete()).IgnoreColumns(ignoreAllNullColumns: true).ExecuteCommandAsync();
    }

    /// <summary>
    /// 复制.
    /// </summary>
    /// <returns></returns>
    [HttpPost("copy")]
    [NonUnify]
    [UnitOfWork]
    public async Task<dynamic> Copy(string id)
    {
        VisualEntity? entity = await _visualRepository.AsQueryable().FirstAsync(v => v.Id == id);
        VisualConfigEntity? configEntity = await _visualRepository.AsSugarClient().Queryable<VisualConfigEntity>().FirstAsync(v => v.VisualId == id);

        VisualEntity? newEntity = await _visualRepository.AsInsertable(entity).CallEntityMethod(m => m.Create()).ExecuteReturnEntityAsync();
        configEntity.VisualId = newEntity.Id;
        await _visualRepository.AsSugarClient().Insertable(configEntity).CallEntityMethod(m => m.Create()).ExecuteReturnEntityAsync();

        return new { code = 200, msg = newEntity.Id };
    }

    /// <summary>
    /// 上传文件.
    /// </summary>
    /// <returns></returns>
    [HttpPost("put-file/{type}"), AllowAnonymous, IgnoreLog]
    public async Task<dynamic> SaveFile(string type, IFormFile file)
    {
        var typeEnum = EnumExtensions.GetEnumDescDictionary(typeof(ScreenImgEnum));
        var imgEnum = typeEnum.Where(t => t.Value.Equals(type)).FirstOrDefault();
        if (imgEnum.Value != null)
        {
            string? ImgType = Path.GetExtension(file.FileName).Replace(".", string.Empty);
            if (!this.AllowImageType(ImgType))
                throw Oops.Oh(ErrorCode.D5013);
            var path = imgEnum.Value;
            string? filePath = Path.Combine(FileVariable.BiVisualPath, path);
            if (!Directory.Exists(filePath))
                Directory.CreateDirectory(filePath);
            string? fileName = SnowflakeIdHelper.NextId() + "." + ImgType;
            var stream = file.OpenReadStream();
            await _fileManager.UploadFileByType(stream, filePath, fileName);
            var fielNameUrl = string.Format("/{0}/{1}/{2}/{3}/{4}/{5}", "api", "file", "VisusalImg", "BiVisualPath", path, fileName);
            return new { name = fielNameUrl, link = fielNameUrl, originalName = file.FileName };
        }

        return Task.FromResult(false);
    }

    #endregion

    #region PrivateMethod

    /// <summary>
    /// 允许文件类型.
    /// </summary>
    /// <param name="fileExtension">文件后缀名.</param>
    /// <returns></returns>
    private bool AllowImageType(string fileExtension)
    {
        List<string>? allowExtension = KeyVariable.AllowImageType;
        string? isExist = allowExtension.Find(a => a == fileExtension.ToLower());
        if (!string.IsNullOrEmpty(isExist))
            return true;
        else
            return false;
    }

    #endregion
}