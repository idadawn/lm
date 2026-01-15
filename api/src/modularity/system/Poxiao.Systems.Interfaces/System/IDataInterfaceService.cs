using Poxiao.Infrastructure.Dtos.VisualDev;
using Poxiao.Infrastructure.Models;
using Poxiao.Systems.Entitys.System;
using System.Data;

namespace Poxiao.Systems.Interfaces.System;

/// <summary>
/// 数据接口
/// 版 本：V1.0.0
/// 版 权：Poxiao
/// 作 者：Poxiao
/// 日 期：2021-06-01.
/// </summary>
public interface IDataInterfaceService
{
    /// <summary>
    /// 信息.
    /// </summary>
    /// <param name="id">主键id.</param>
    /// <returns></returns>
    Task<DataInterfaceEntity> GetInfo(string id);

    /// <summary>
    /// sql接口查询.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task<DataTable> GetData(DataInterfaceEntity entity);

    /// <summary>
    /// 根据不同类型请求接口.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="type">0 ： 分页 1 ：详情 ，其他 原始.</param>
    /// <param name="tenantId"></param>
    /// <param name="input"></param>
    /// <param name="dicParameters">字典参数.</param>
    /// <returns></returns>
    Task<object> GetResponseByType(string id, int type, string tenantId, VisualDevDataFieldDataListInput input = null, Dictionary<string, string> dicParameters = null);

    /// <summary>
    /// 替换参数默认值.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="dic"></param>
    void ReplaceParameterValue(DataInterfaceEntity entity, Dictionary<string, string> dic);

    /// <summary>
    /// 处理远端数据.
    /// </summary>
    /// <param name="propsUrl">远端数据ID.</param>
    /// <param name="value">指定选项标签为选项对象的某个属性值.</param>
    /// <param name="label">指定选项的值为选项对象的某个属性值.</param>
    /// <param name="children">指定选项的子选项为选项对象的某个属性值.</param>
    /// <param name="linkageParameters">联动参数.</param>
    /// <returns></returns>
    Task<List<StaticDataModel>> GetDynamicList(string propsUrl, string value, string label, string children, List<ControlLinkageParameterModel> linkageParameters = null);
}