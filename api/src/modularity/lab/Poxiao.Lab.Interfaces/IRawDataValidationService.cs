using Poxiao.Lab.Entity;

namespace Poxiao.Lab.Interfaces;

/// <summary>
/// 原始数据校验服务接口
/// </summary>
public interface IRawDataValidationService
{
    /// <summary>
    /// 校验基础字段（日期、炉号、宽度、带材重量）
    /// </summary>
    /// <param name="entity"></param>
    /// <returns>错误信息列表</returns>
    List<string> ValidateBasicFields(RawDataEntity entity);

    /// <summary>
    /// 校验炉号格式
    /// </summary>
    /// <param name="furnaceNo"></param>
    /// <returns>错误信息（无错误返回null）</returns>
    string ValidateFurnaceNo(string furnaceNo);

    /// <summary>
    /// 校验数据完整性（产品规格、特性匹配）
    /// </summary>
    /// <param name="entity"></param>
    /// <returns>错误信息列表</returns>
    List<string> ValidateIntegrity(RawDataEntity entity);
}
