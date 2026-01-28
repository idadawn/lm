using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Manager;
using Poxiao.Infrastructure.Security;
using Poxiao.Lab.Entity;
using Poxiao.Lab.Entity.Dto.IntermediateData;
using Poxiao.Lab.Entity.Entity;
using Poxiao.Lab.Interfaces;
using SqlSugar;
using Yitter.IdGenerator;

namespace Poxiao.Lab.Service
{
    /// <summary>
    /// 中间数据颜色配置服务
    /// </summary>
    [ApiDescriptionSettings(Tag = "Lab", Name = "intermediate-data-color", Order = 200)]
    [Route("api/lab/intermediate-data-color")]
    public class IntermediateDataColorService
        : IIntermediateDataColorService,
            IDynamicApiController,
            ITransient
    {
        private readonly ISqlSugarClient _db;
        private readonly ICacheManager _cacheManager;
        private readonly IUserManager _userManager;

        private const string CachePrefix = "LAB:IntermediateDataColor";

        public IntermediateDataColorService(
            ISqlSugarClient db,
            ICacheManager cacheManager,
            IUserManager userManager
        )
        {
            _db = db;
            _cacheManager = cacheManager;
            _userManager = userManager;
        }

        private string GetCacheKey(string suffix)
        {
            var tenantId = _userManager?.TenantId ?? "global";
            return $"{CachePrefix}:{tenantId}:{suffix}";
        }

        /// <summary>
        /// 保存颜色配置
        /// </summary>
        /// <param name="input">颜色配置信息</param>
        /// <returns>保存结果</returns>
        [HttpPost("save-colors")]
        public async Task<bool> SaveColors(SaveIntermediateDataColorInput input)
        {
            if (input.Colors == null || !input.Colors.Any())
                return true;

            try
            {
                var currentUserId = App.User?.FindFirst(ClaimConst.CLAINMUSERID)?.Value;
                var now = DateTime.Now;

                // 按IntermediateDataId分组处理
                var groups = input.Colors.GroupBy(x => x.IntermediateDataId);

                foreach (var group in groups)
                {
                    var intermediateDataId = group.Key;
                    var fieldNames = group.Select(x => x.FieldName).ToList();

                    // 删除该数据这些字段的现有颜色配置
                    await _db.Deleteable<IntermediateDataColorEntity>()
                        .Where(x =>
                            x.IntermediateDataId == intermediateDataId
                            && fieldNames.Contains(x.FieldName)
                        )
                        .ExecuteCommandAsync();

                    // 添加新的颜色配置（只插入非空颜色值）
                    var entities = group
                        .Where(x => !string.IsNullOrEmpty(x.ColorValue)) // 过滤掉空颜色值
                        .Select(x => new IntermediateDataColorEntity
                        {
                            Id = SnowflakeIdHelper.NextId(),
                            IntermediateDataId = x.IntermediateDataId,
                            FieldName = x.FieldName,
                            ColorValue = x.ColorValue,
                            ProductSpecId = input.ProductSpecId,
                            CreatorUserId = currentUserId,
                            CreatorTime = now,
                            UpdateUserId = currentUserId,
                            UpdateTime = now,
                        })
                        .ToList();

                    // 只有有颜色数据时才插入
                    if (entities.Any())
                    {
                        await _db.Insertable(entities).ExecuteCommandAsync();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                throw Oops.Oh($"保存颜色配置失败: {ex.Message}");
            }
            finally
            {
                // 清除缓存
                await ClearCacheAsync(input.ProductSpecId);
            }
        }

        /// <summary>
        /// 获取颜色配置
        /// </summary>
        /// <param name="input">查询条件</param>
        /// <returns>颜色配置列表</returns>
        [HttpPost("get-colors")]
        public async Task<IntermediateDataColorDto> GetColors(GetIntermediateDataColorInput input)
        {
            // 尝试从缓存获取
            var cacheKey = GetCacheKey($"spec:{input.ProductSpecId}");
            var cachedColors = await _cacheManager.GetAsync<List<IntermediateDataColorEntity>>(cacheKey);

            if (cachedColors == null)
            {
                cachedColors = await _db.Queryable<IntermediateDataColorEntity>()
                    .Where(x => x.ProductSpecId == input.ProductSpecId)
                    .ToListAsync();
                
                await _cacheManager.SetAsync(cacheKey, cachedColors, TimeSpan.FromHours(6));
            }

            var query = cachedColors.AsEnumerable();

            if (input.IntermediateDataIds != null && input.IntermediateDataIds.Any())
            {
                query = query.Where(x => input.IntermediateDataIds.Contains(x.IntermediateDataId));
            }

            var colors = query
                .Select(x => new CellColorInfo
                {
                    IntermediateDataId = x.IntermediateDataId,
                    FieldName = x.FieldName,
                    ColorValue = x.ColorValue,
                })
                .ToList();

            return new IntermediateDataColorDto
            {
                Colors = colors,
                ProductSpecId = input.ProductSpecId,
            };
        }

        /// <summary>
        /// 删除颜色配置
        /// </summary>
        /// <param name="input">删除条件</param>
        /// <returns>删除结果</returns>
        [HttpPost("delete-colors")]
        public async Task<bool> DeleteColors(DeleteIntermediateDataColorInput input)
        {
            try
            {
                var query = _db.Deleteable<IntermediateDataColorEntity>();

                if (input.Ids != null && input.Ids.Any())
                {
                    query = query.Where(x => input.Ids.Contains(x.Id));
                }
                else if (!string.IsNullOrEmpty(input.IntermediateDataId))
                {
                    query = query.Where(x => x.IntermediateDataId == input.IntermediateDataId);
                }
                else if (!string.IsNullOrEmpty(input.ProductSpecId))
                {
                    query = query.Where(x => x.ProductSpecId == input.ProductSpecId);
                }

                await query.ExecuteCommandAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw Oops.Oh($"删除颜色配置失败: {ex.Message}");
            }
            finally
            {
                // 清除缓存
                await ClearCacheAsync(input.ProductSpecId);
            }
        }

        /// <summary>
        /// 获取指定中间数据的颜色配置（用于批量查询）
        /// </summary>
        /// <param name="intermediateDataIds">中间数据ID列表</param>
        /// <param name="productSpecId">产品规格ID</param>
        /// <returns>颜色配置字典</returns>
        public async Task<Dictionary<string, Dictionary<string, string>>> GetColorsByDataIds(
            List<string> intermediateDataIds,
            string productSpecId
        )
        {
            if (intermediateDataIds == null || !intermediateDataIds.Any())
                return new Dictionary<string, Dictionary<string, string>>();

            // 尝试从缓存获取
            var cacheKey = GetCacheKey($"spec:{productSpecId}");
            var cachedColors = await _cacheManager.GetAsync<List<IntermediateDataColorEntity>>(cacheKey);
            
            if (cachedColors == null)
            {
                // 获取该产品规格的所有颜色配置
                cachedColors = await _db.Queryable<IntermediateDataColorEntity>()
                    .Where(x => x.ProductSpecId == productSpecId)
                    .ToListAsync();
                
                // 缓存6小时
                await _cacheManager.SetAsync(cacheKey, cachedColors, TimeSpan.FromHours(6));
            }

            // 过滤出需要的数据
            return cachedColors
                .Where(x => intermediateDataIds.Contains(x.IntermediateDataId))
                .GroupBy(x => x.IntermediateDataId)
                .ToDictionary(g => g.Key, g => g.ToDictionary(x => x.FieldName, x => x.ColorValue));
        }

        /// <summary>
        /// 保存单个单元格颜色（接口要求的签名）
        /// </summary>
        public Task<bool> SaveCellColor(
            string intermediateDataId,
            string fieldName,
            string colorValue,
            string productSpecId
        )
        {
            var input = new SaveCellColorInput
            {
                IntermediateDataId = intermediateDataId,
                FieldName = fieldName,
                ColorValue = colorValue,
                ProductSpecId = productSpecId,
            };

            return SaveCellColor(input);
        }

        /// <summary>
        /// 保存单个单元格颜色（用于实时保存）
        /// </summary>
        /// <param name="input">保存单元格颜色请求</param>
        [HttpPost("save-cell-color")]
        public async Task<bool> SaveCellColor([FromBody] SaveCellColorInput input)
        {
            if (
                string.IsNullOrEmpty(input?.IntermediateDataId)
                || string.IsNullOrEmpty(input?.FieldName)
            )
                return false;

            try
            {
                var currentUserId = App.User?.FindFirst(ClaimConst.CLAINMUSERID)?.Value;
                var now = DateTime.Now;

                // 先删除该单元格现有的颜色
                await _db.Deleteable<IntermediateDataColorEntity>()
                    .Where(x =>
                        x.IntermediateDataId == input.IntermediateDataId
                        && x.FieldName == input.FieldName
                    )
                    .ExecuteCommandAsync();

                // 如果颜色值不为空，则添加新的颜色
                if (!string.IsNullOrEmpty(input.ColorValue))
                {
                    var entity = new IntermediateDataColorEntity
                    {
                        Id = SnowflakeIdHelper.NextId(),
                        IntermediateDataId = input.IntermediateDataId,
                        FieldName = input.FieldName,
                        ColorValue = input.ColorValue,
                        ProductSpecId = input.ProductSpecId,
                        CreatorUserId = currentUserId,
                        CreatorTime = now,
                        UpdateUserId = currentUserId,
                        UpdateTime = now,
                    };

                    await _db.Insertable(entity).ExecuteCommandAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                throw Oops.Oh($"保存单元格颜色失败: {ex.Message}");
            }
            finally
            {
                // 清除缓存
                await ClearCacheAsync(input.ProductSpecId);
            }
        }

        private async Task ClearCacheAsync(string productSpecId)
        {
            if (!string.IsNullOrEmpty(productSpecId))
            {
                await _cacheManager.DelAsync(GetCacheKey($"spec:{productSpecId}"));
            }
        }
    }
}
