using Poxiao.Lab.Entity.Dto.IntermediateData;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Poxiao.Lab.Interfaces
{
    /// <summary>
    /// 中间数据颜色配置服务接口
    /// </summary>
    public interface IIntermediateDataColorService
    {
        /// <summary>
        /// 保存颜色配置
        /// </summary>
        Task<bool> SaveColors(SaveIntermediateDataColorInput input);

        /// <summary>
        /// 获取颜色配置
        /// </summary>
        Task<IntermediateDataColorDto> GetColors(GetIntermediateDataColorInput input);

        /// <summary>
        /// 删除颜色配置
        /// </summary>
        Task<bool> DeleteColors(DeleteIntermediateDataColorInput input);

        /// <summary>
        /// 获取指定中间数据的颜色配置（用于批量查询）
        /// </summary>
        Task<Dictionary<string, Dictionary<string, string>>> GetColorsByDataIds(List<string> intermediateDataIds, string productSpecId);

        /// <summary>
        /// 保存单个单元格颜色（用于实时保存）
        /// </summary>
        Task<bool> SaveCellColor(string intermediateDataId, string fieldName, string colorValue, string productSpecId);
    }
}