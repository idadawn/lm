using Poxiao.DependencyInjection;

namespace Poxiao.VisualDev.Engine.Model.CodeGen;

/// <summary>
/// 表单默认值控件模型.
/// </summary>
[SuppressSniffer]
public class DefaultFormControlModel
{
    /// <summary>
    /// 是否存在日期控件.
    /// </summary>
    public bool IsExistDate { get; set; }

    /// <summary>
    /// 是否存在时间控件.
    /// </summary>
    public bool IsExistTime { get; set; }

    /// <summary>
    /// 是否存在组织选择.
    /// </summary>
    public bool IsExistComSelect { get; set; }

    /// <summary>
    /// 是否存在部门织选择.
    /// </summary>
    public bool IsExistDepSelect { get; set; }

    /// <summary>
    /// 是否存在用户选择.
    /// </summary>
    public bool IsExistUserSelect { get; set; }

    /// <summary>
    /// 是否存在子表.
    /// </summary>
    public bool IsExistSubTable { get; set; }

    /// <summary>
    /// 日期选择字段.
    /// </summary>
    public List<DefaultTimeControl> DateField { get; set; }

    /// <summary>
    /// 日期选择字段.
    /// </summary>
    public List<DefaultTimeControl> TimeField { get; set; }

    /// <summary>
    /// 子表名称.
    /// </summary>
    public string SubTableName { get; set; }

    /// <summary>
    /// 组织选择控件列表.
    /// </summary>
    public List<DefaultComSelectControl> ComSelectList { get; set; }

    /// <summary>
    /// 部门选择控件列表.
    /// </summary>
    public List<DefaultDepSelectControl> DepSelectList { get; set; }

    /// <summary>
    /// 用户选择控件列表.
    /// </summary>
    public List<DefaultUserSelectControl> UserSelectList { get; set; }

    /// <summary>
    /// 子表默认值.
    /// </summary>
    public List<DefaultFormControlModel> SubTabelDefault { get; set; }
}

/// <summary>
/// 组织选择默认值.
/// </summary>
public class DefaultComSelectControl
{
    /// <summary>
    /// 表单多选.
    /// </summary>
    public bool IsMultiple { get; set; }

    /// <summary>
    /// 查询多选.
    /// </summary>
    public bool IsSearchMultiple { get; set; }

    /// <summary>
    /// 字段名称.
    /// </summary>
    public string Field { get; set; }
}

/// <summary>
/// 时间选择默认值.
/// </summary>
public class DefaultTimeControl
{
    /// <summary>
    /// 时间格式.
    /// </summary>
    public string Format { get; set; }

    /// <summary>
    /// 字段名称.
    /// </summary>
    public string Field { get; set; }
}

/// <summary>
/// 部门选择默认值.
/// </summary>
public class DefaultDepSelectControl
{
    /// <summary>
    /// 表单多选.
    /// </summary>
    public bool IsMultiple { get; set; }

    /// <summary>
    /// 查询多选.
    /// </summary>
    public bool IsSearchMultiple { get; set; }

    /// <summary>
    /// 可选范围
    /// custom-自定义,all-全部.
    /// </summary>
    public string selectType { get; set; }

    /// <summary>
    /// 可选部门.
    /// </summary>
    public string ableDepIds { get; set; }

    /// <summary>
    /// 字段名称.
    /// </summary>
    public string Field { get; set; }
}

/// <summary>
/// 用户选择默认值.
/// </summary>
public class DefaultUserSelectControl
{
    /// <summary>
    /// 表单多选.
    /// </summary>
    public bool IsMultiple { get; set; }

    /// <summary>
    /// 查询多选.
    /// </summary>
    public bool IsSearchMultiple { get; set; }

    /// <summary>
    /// 可选范围
    /// custom-自定义,all-全部.
    /// </summary>
    public string selectType { get; set; }

    /// <summary>
    /// 可选部门.
    /// </summary>
    public string ableDepIds { get; set; }

    /// <summary>
    /// 可选用户.
    /// </summary>
    public string ableUserIds { get; set; }

    /// <summary>
    /// 可选岗位.
    /// </summary>
    public string ablePosIds { get; set; }

    /// <summary>
    /// 可选角色.
    /// </summary>
    public string ableRoleIds { get; set; }

    /// <summary>
    /// 可选分组.
    /// </summary>
    public string ableGroupIds { get; set; }

    /// <summary>
    /// 字段名称.
    /// </summary>
    public string Field { get; set; }
}