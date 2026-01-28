using Mapster;
using Microsoft.AspNetCore.Mvc;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Enums;
using Poxiao.Infrastructure.Manager;
using Poxiao.Infrastructure.Security;
using Poxiao.Lab.Entity;
using Poxiao.Lab.Entity.Dto.AppearanceFeature;
using Poxiao.Lab.Interfaces;
using SqlSugar;

namespace Poxiao.Lab.Service;

/// <summary>
/// 外观特性大类服务.
/// </summary>
[ApiDescriptionSettings(Tag = "Lab", Name = "appearance-feature-categories", Order = 102)]
[Route("api/lab/appearance-feature-categories")]
public class AppearanceFeatureCategoryService : IAppearanceFeatureCategoryService, IDynamicApiController, ITransient
{
    private readonly ISqlSugarRepository<AppearanceFeatureCategoryEntity> _categoryRepository;
    private readonly ISqlSugarRepository<AppearanceFeatureEntity> _featureRepository;
    private readonly ICacheManager _cacheManager;
    private readonly IUserManager _userManager;

    private const string CachePrefix = "LAB:AppearanceFeatureCategory";

    public AppearanceFeatureCategoryService(
        ISqlSugarRepository<AppearanceFeatureCategoryEntity> categoryRepository,
        ISqlSugarRepository<AppearanceFeatureEntity> featureRepository,
        ICacheManager cacheManager,
        IUserManager userManager
    )
    {
        _categoryRepository = categoryRepository;
        _featureRepository = featureRepository;
        _cacheManager = cacheManager;
        _userManager = userManager;
    }

    private string GetCacheKey(string suffix)
    {
        var tenantId = _userManager?.TenantId ?? "global";
        return $"{CachePrefix}:{tenantId}:{suffix}";
    }

    /// <inheritdoc />
    [HttpGet("")]
    public async Task<List<AppearanceFeatureCategoryListOutput>> GetList(
        [FromQuery] AppearanceFeatureCategoryListQuery input
    )
    {
        var query = _categoryRepository
            .AsQueryable()
            .WhereIF(
                !string.IsNullOrEmpty(input.Keyword),
                t => t.Name.Contains(input.Keyword)
            )
            .Where(t => t.DeleteMark == null)
            .OrderBy(t => t.SortCode);

        var data = await query.ToListAsync();

        // 获取每个大类下的特性数量并转换为输出对象
        var outputs = new List<AppearanceFeatureCategoryListOutput>();
        foreach (var item in data)
        {
            var output = new AppearanceFeatureCategoryListOutput
            {
                Id = item.Id,
                ParentId = item.ParentId ?? "-1", // 将 null 转换为 "-1"（顶级分类）
                Name = item.Name,
                Description = item.Description,
                SortCode = item.SortCode,
                CreatorTime = item.CreatorTime,
                RootId = item.RootId,
                Path = item.Path
            };
            output.FeatureCount = await _featureRepository
                .AsQueryable()
                .Where(f => f.CategoryId == item.Id && f.DeleteMark == null)
                .CountAsync();
            outputs.Add(output);
        }

        // 构建树形结构（使用 "-1" 作为根节点标识，参考 OrganizeEntity）
        return outputs.ToTree("-1");
    }

    /// <inheritdoc />
    /// 注意：此路由必须在 [HttpGet("{id}")] 之前，否则 "all" 会被当作 id 参数
    [HttpGet("all")]
    public async Task<List<AppearanceFeatureCategoryListOutput>> GetAllCategories()
    {
        var cacheKey = GetCacheKey("all:tree");
        var cached = await _cacheManager.GetAsync<List<AppearanceFeatureCategoryListOutput>>(cacheKey);
        if (cached != null && cached.Count > 0)
        {
            return cached;
        }

        var data = await _categoryRepository
            .AsQueryable()
            .Where(t => t.DeleteMark == null)
            .OrderBy(t => t.SortCode)
            .OrderBy(t => t.Name)
            .ToListAsync();

        // 转换为输出对象
        var outputs = data.Select(item => new AppearanceFeatureCategoryListOutput
        {
            Id = item.Id,
            ParentId = item.ParentId ?? "-1", // 将 null 转换为 "-1"（顶级分类）
            Name = item.Name,
            Description = item.Description,
            SortCode = item.SortCode,
            RootId = item.RootId,
            Path = item.Path
        }).ToList();

        // 构建树形结构（使用 "-1" 作为根节点标识，参考 OrganizeEntity）
        var result = outputs.ToTree("-1");
        if (result.Count > 0)
        {
            await _cacheManager.SetAsync(cacheKey, result, TimeSpan.FromHours(6));
        }
        return result;
    }

    /// <inheritdoc />
    [HttpGet("{id}")]
    public async Task<AppearanceFeatureCategoryInfoOutput> GetInfo(string id)
    {
        var entity = await _categoryRepository.GetFirstAsync(t => t.Id == id && t.DeleteMark == null);
        if (entity == null)
        {
            throw Oops.Oh(ErrorCode.COM1005);
        }

        var output = entity.Adapt<AppearanceFeatureCategoryInfoOutput>();
        output.FeatureCount = await _featureRepository
            .AsQueryable()
            .Where(f => f.CategoryId == entity.Id && f.DeleteMark == null)
            .CountAsync();

        return output;
    }

    /// <inheritdoc />
    [HttpPost("")]
    public async Task Create([FromBody] AppearanceFeatureCategoryCrInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            throw Oops.Oh("大类名称不能为空");
        }

        // 处理父级ID：如果为空或null，设置为 "-1"（顶级分类）
        if (string.IsNullOrEmpty(input.ParentId))
        {
            input.ParentId = "-1";
        }

        // 唯一性检查（同一父级下名称唯一）
        var parentId = input.ParentId == "-1" ? "-1" : input.ParentId;
        var existingCategory = await _categoryRepository.GetFirstAsync(t =>
            t.Name == input.Name && 
            (t.ParentId ?? "-1") == parentId &&
            t.DeleteMark == null
        );
        if (existingCategory != null)
        {
            throw Oops.Oh($"大类 '{input.Name}' 已存在");
        }

        // 如果指定了父级（且不是 "-1"），验证父级是否存在
        AppearanceFeatureCategoryEntity parent = null;
        if (input.ParentId != "-1")
        {
            parent = await _categoryRepository.GetFirstAsync(t => 
                t.Id == input.ParentId && t.DeleteMark == null);
            if (parent == null)
            {
                throw Oops.Oh("指定的父级分类不存在");
            }
        }

        var entity = input.Adapt<AppearanceFeatureCategoryEntity>();
        entity.Creator();
        entity.LastModifyUserId = entity.CreatorUserId;
        entity.LastModifyTime = entity.CreatorTime;
        entity.ParentId = input.ParentId; // 确保使用处理后的父级ID

        // 自动生成排序码：如果未指定，则取同父级下最大排序码+1，如果没有则从1开始
        if (!entity.SortCode.HasValue || entity.SortCode.Value == 0)
        {
            var sortParentId = entity.ParentId == "-1" ? "-1" : entity.ParentId;
            var maxSortCode = await _categoryRepository
                .AsQueryable()
                .Where(t => (t.ParentId ?? "-1") == sortParentId && t.DeleteMark == null)
                .MaxAsync(t => t.SortCode);
            entity.SortCode = (maxSortCode ?? 0) + 1;
        }

        // 计算并设置根分类ID和路径（参考 OrganizeIdTree 的实现方式）
        List<string> idList = new List<string>();
        idList.Add(entity.Id);
        
        if (entity.ParentId != "-1")
        {
            // 使用 ToParentList 向上查找所有父级ID
            List<string> parentIds = _categoryRepository
                .AsSugarClient()
                .Queryable<AppearanceFeatureCategoryEntity>()
                .ToParentList(it => it.ParentId, entity.ParentId)
                .Select(x => x.Id)
                .ToList();
            idList.AddRange(parentIds);
        }

        // 反转列表（根节点在前）
        idList.Reverse();
        entity.Path = string.Join(",", idList);
        
        // 设置根分类ID（路径的第一个ID）
        entity.RootId = idList.FirstOrDefault();

        // 保存实体
        var isOk = await _categoryRepository
            .AsInsertable(entity)
            .IgnoreColumns(ignoreNullColumn: true)
            .ExecuteCommandAsync();
        if (isOk < 1)
            throw Oops.Oh(ErrorCode.COM1000);

        await ClearCacheAsync();
    }

    /// <inheritdoc />
    [HttpPut("{id}")]
    public async Task Update(string id, [FromBody] AppearanceFeatureCategoryUpInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            throw Oops.Oh("大类名称不能为空");
        }

        // 优先使用路由参数 id，如果为空则使用 input.Id
        var categoryId = string.IsNullOrWhiteSpace(id) ? input.Id : id;
        if (string.IsNullOrWhiteSpace(categoryId))
        {
            throw Oops.Oh("分类ID不能为空");
        }

        Console.WriteLine($"[Update] 路由参数ID: {id}, Input.Id: {input.Id}, 最终使用ID: {categoryId}, Name: {input.Name}");

        // 查询记录（不限制 DeleteMark，以便获取更详细的错误信息）
        var entity = await _categoryRepository.GetFirstAsync(t => t.Id == categoryId);
        if (entity == null)
        {
            Console.WriteLine($"[Update] 未找到ID为 {categoryId} 的记录");
            throw Oops.Oh(ErrorCode.COM1005);
        }

        Console.WriteLine($"[Update] 找到记录: Name={entity.Name}, DeleteMark={entity.DeleteMark}");

        // 检查记录是否已被删除
        if (entity.DeleteMark != null)
        {
            throw Oops.Oh($"分类 '{entity.Name}' (ID: {categoryId}) 已被删除，无法更新");
        }

        // 处理父级ID：如果为空或null，设置为 "-1"（顶级分类）
        if (string.IsNullOrEmpty(input.ParentId))
        {
            input.ParentId = "-1";
        }

        // 防止循环引用：不能将分类设置为自己的子分类
        if (input.ParentId != "-1")
        {
            if (input.ParentId == categoryId)
            {
                throw Oops.Oh("不能将分类设置为自己的父级");
            }

            // 检查是否会将分类设置为自己的子分类
            var childIds = await GetChildCategoryIdsAsync(categoryId);
            if (childIds.Contains(input.ParentId))
            {
                throw Oops.Oh("不能将分类设置为自己的子分类");
            }

            // 验证父级是否存在
            var parent = await _categoryRepository.GetFirstAsync(t => 
                t.Id == input.ParentId && t.DeleteMark == null);
            if (parent == null)
            {
                throw Oops.Oh("指定的父级分类不存在");
            }
        }

        // 唯一性检查（同一父级下名称唯一，排除自身）
        var parentId = input.ParentId == "-1" ? "-1" : input.ParentId;
        var existingCategory = await _categoryRepository.GetFirstAsync(t =>
            t.Name == input.Name && 
            t.Id != categoryId &&
            (t.ParentId ?? "-1") == parentId &&
            t.DeleteMark == null
        );
        if (existingCategory != null)
        {
            throw Oops.Oh($"大类 '{input.Name}' 已存在");
        }

        // 注意：由于现在使用ID关联，修改大类名称不需要更新特性表
        // 特性表中的 CategoryId 保持不变，只是大类名称改变了

        entity.Name = input.Name;
        entity.Description = input.Description;
        entity.SortCode = input.SortCode;
        var oldParentId = entity.ParentId ?? "-1";
        entity.ParentId = input.ParentId;

        // 如果父级发生了变化，需要重新计算根分类ID和路径（参考 OrganizeIdTree 的实现方式）
        if (string.IsNullOrWhiteSpace(entity.Path) || entity.ParentId != oldParentId)
        {
            List<string> idList = new List<string>();
            idList.Add(entity.Id); // 使用 entity.Id（应该等于 categoryId）
            
            if (entity.ParentId != "-1")
            {
                // 使用 ToParentList 向上查找所有父级ID
                List<string> parentIds = _categoryRepository
                    .AsSugarClient()
                    .Queryable<AppearanceFeatureCategoryEntity>()
                    .ToParentList(it => it.ParentId, entity.ParentId)
                    .Select(x => x.Id)
                    .ToList();
                idList.AddRange(parentIds);
            }

            // 反转列表（根节点在前）
            idList.Reverse();
            string newPath = string.Join(",", idList);
            string oldPath = entity.Path;
            entity.Path = newPath;
            
            // 设置根分类ID（路径的第一个ID）
            entity.RootId = idList.FirstOrDefault();

            // 如果路径变化，需要更新所有子分类的路径（参考 OrganizeIdTree 的实现方式）
            if (newPath != oldPath)
            {
                // 查找所有包含当前分类ID的子分类（使用 Contains 方法）
                List<AppearanceFeatureCategoryEntity> childEntities = await _categoryRepository
                    .AsQueryable()
                    .Where(x => x.Path.Contains(entity.Id) && x.Id != entity.Id && x.DeleteMark == null)
                    .ToListAsync();
                
                childEntities.ForEach(item =>
                {
                    // 参考 OrganizeService 的实现：找到旧路径中当前ID之后的部分
                    string[] pathParts = item.Path.Split(new[] { entity.Id }, StringSplitOptions.None);
                    string childList = pathParts.Length > 1 ? pathParts.LastOrDefault() : string.Empty;
                    
                    // 拼接新路径：新路径 + 子分类ID之后的部分
                    item.Path = newPath + childList;
                    
                    // 更新根分类ID（路径的第一个ID）
                    var newPathParts = item.Path.Split(',');
                    item.RootId = newPathParts.FirstOrDefault();
                });

                // 批量更新所有子分类
                if (childEntities.Any())
                {
                    await _categoryRepository
                        .AsSugarClient()
                        .Updateable(childEntities)
                        .UpdateColumns(x => new { x.Path, x.RootId })
                        .ExecuteCommandAsync();
                }
            }
        }

        entity.LastModify();

        var isOk = await _categoryRepository
            .AsUpdateable(entity)
            .IgnoreColumns(ignoreAllNullColumns: true)
            .ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1001);

        await ClearCacheAsync();
    }

    /// <inheritdoc />
    [HttpDelete("{id}")]
    public async Task Delete(string id)
    {
        var entity = await _categoryRepository.GetFirstAsync(t => t.Id == id && t.DeleteMark == null);
        if (entity == null)
        {
            throw Oops.Oh(ErrorCode.COM1005);
        }

        // 检查是否有子分类
        var childCount = await _categoryRepository
            .AsQueryable()
            .Where(t => (t.ParentId ?? "-1") == id && t.DeleteMark == null)
            .CountAsync();

        if (childCount > 0)
        {
            throw Oops.Oh($"无法删除，该大类下还有 {childCount} 个子分类，请先删除或转移这些子分类");
        }

        // 检查是否有特性使用该大类
        var featureCount = await _featureRepository
            .AsQueryable()
            .Where(f => f.CategoryId == entity.Id && f.DeleteMark == null)
            .CountAsync();

        if (featureCount > 0)
        {
            throw Oops.Oh($"无法删除，该大类下还有 {featureCount} 个特性，请先删除或转移这些特性");
        }

        entity.Delete();
        var isOk = await _categoryRepository
            .AsUpdateable(entity)
            .UpdateColumns(it => new
            {
                it.DeleteMark,
                it.DeleteTime,
                it.DeleteUserId,
            })
            .ExecuteCommandHasChangeAsync();
        if (!isOk)
            throw Oops.Oh(ErrorCode.COM1002);

        await ClearCacheAsync();
    }

    /// <summary>
    /// 获取所有子分类ID（递归）
    /// </summary>
    private async Task<List<string>> GetChildCategoryIdsAsync(string parentId)
    {
        var childIds = new List<string>();
        var directChildren = await _categoryRepository
            .AsQueryable()
            .Where(t => (t.ParentId ?? "-1") == parentId && t.DeleteMark == null)
            .Select(t => t.Id)
            .ToListAsync();

        childIds.AddRange(directChildren);

        foreach (var childId in directChildren)
        {
            var grandChildren = await GetChildCategoryIdsAsync(childId);
            childIds.AddRange(grandChildren);
        }

        return childIds;
    }

    private async Task ClearCacheAsync()
    {
        await _cacheManager.DelAsync(GetCacheKey("all:tree"));
    }
}
