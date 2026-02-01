using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Extension;
using Poxiao.VisualDev.Engine.Model.CodeGen;

namespace Poxiao.VisualDev.Engine.Security;

/// <summary>
/// 代码生成表字段判断帮助类.
/// </summary>
public class CodeGenFieldJudgeHelper
{
    /// <summary>
    /// 是否查询列.
    /// </summary>
    /// <param name="searchList">模板内查询列表.</param>
    /// <param name="fieldName">字段名称.</param>
    /// <returns></returns>
    public static bool IsColumnQueryWhether(List<IndexSearchFieldModel>? searchList, string fieldName)
    {
        var column = searchList?.Any(s => s.prop == fieldName);
        return column ?? false;
    }

    /// <summary>
    /// 列查询类型.
    /// </summary>
    /// <param name="searchList">模板内查询列表.</param>
    /// <param name="fieldName">字段名称.</param>
    /// <returns></returns>
    public static int ColumnQueryType(List<IndexSearchFieldModel>? searchList, string fieldName)
    {
        var column = searchList?.Find(s => s.prop == fieldName);
        return column?.searchType ?? 0;
    }

    /// <summary>
    /// 列表查询多选.
    /// </summary>
    /// <param name="searchList">模板内查询列表.</param>
    /// <param name="fieldName">字段名称.</param>
    /// <returns></returns>
    public static bool ColumnQueryMultiple(List<IndexSearchFieldModel>? searchList, string fieldName)
    {
        var column = searchList?.Find(s => s.prop == fieldName);
        return (column?.searchMultiple).ParseToBool();
    }

    /// <summary>
    /// 是否展示列.
    /// </summary>
    /// <param name="columnList">模板内展示字段.</param>
    /// <param name="fieldName">字段名称.</param>
    /// <returns></returns>
    public static bool IsShowColumn(List<IndexGridFieldModel>? columnList, string fieldName)
    {
        bool? column = columnList?.Any(s => s.prop == fieldName);
        return column ?? false;
    }

    /// <summary>
    /// 获取是否多选.
    /// </summary>
    /// <param name="columnList">模板内控件列表.</param>
    /// <param name="fieldName">字段名称.</param>
    /// <returns></returns>
    public static bool IsMultipleColumn(List<FieldsModel> columnList, string fieldName)
    {
        bool isMultiple = false;
        var column = columnList.Find(s => s.VModel == fieldName);
        if (column != null)
        {
            switch (column?.Config.poxiaoKey)
            {
                case PoxiaoKeyConst.CASCADER:
                    isMultiple = column.props.props.multiple;
                    break;
                default:
                    isMultiple = column.multiple;
                    break;
            }
        }

        return isMultiple;
    }

    /// <summary>
    /// 获取是否多选.
    /// </summary>
    /// <param name="column">模板内控件.</param>
    /// <param name="fieldName">字段名称.</param>
    /// <returns></returns>
    public static bool IsMultipleColumn(FieldsModel column, string fieldName)
    {
        bool isMultiple = false;
        if (column != null)
        {
            switch (column?.Config.poxiaoKey)
            {
                case PoxiaoKeyConst.CASCADER:
                    isMultiple = column.props.props.multiple;
                    break;
                default:
                    isMultiple = column.multiple;
                    break;
            }
        }

        return isMultiple;
    }

    /// <summary>
    /// 控制解析.
    /// </summary>
    /// <param name="column"></param>
    /// <returns></returns>
    public static bool IsControlParsing(FieldsModel column)
    {
        bool isExist = false;
        switch (column?.Config.poxiaoKey)
        {
            case PoxiaoKeyConst.RELATIONFORM:
            case PoxiaoKeyConst.POPUPSELECT:
            case PoxiaoKeyConst.USERSSELECT:
                isExist = true;
                break;
        }
        return isExist;
    }

    /// <summary>
    /// 是否datetime.
    /// </summary>
    /// <param name="fields"></param>
    /// <returns></returns>
    public static bool IsDateTime(FieldsModel? fields)
    {
        bool isDateTime = false;
        if (fields?.Config.poxiaoKey == PoxiaoKeyConst.DATE || fields?.Config.poxiaoKey == PoxiaoKeyConst.TIME)
            isDateTime = true;
        return isDateTime;
    }

    /// <summary>
    /// 是否副表datetime.
    /// </summary>
    /// <param name="fields"></param>
    /// <returns></returns>
    public static bool IsSecondaryTableDateTime(FieldsModel? fields)
    {
        bool isDateTime = false;
        if (fields?.Config.poxiaoKey == PoxiaoKeyConst.DATE || fields?.Config.poxiaoKey == PoxiaoKeyConst.TIME || fields?.Config.poxiaoKey == PoxiaoKeyConst.CREATETIME || fields?.Config.poxiaoKey == PoxiaoKeyConst.MODIFYTIME)
            isDateTime = true;
        return isDateTime;
    }

    /// <summary>
    /// 是否子表映射.
    /// </summary>
    /// <param name="tableColumnConfig">表列.</param>
    /// <returns></returns>
    public static bool IsChildTableMapper(List<TableColumnConfigModel> tableColumnConfig)
    {
        bool isOpen = false;
        tableColumnConfig.ForEach(item =>
        {
            switch (item.poxiaoKey)
            {
                case PoxiaoKeyConst.CASCADER:
                case PoxiaoKeyConst.ADDRESS:
                case PoxiaoKeyConst.COMSELECT:
                case PoxiaoKeyConst.UPLOADIMG:
                case PoxiaoKeyConst.UPLOADFZ:
                case PoxiaoKeyConst.DATE:
                case PoxiaoKeyConst.TIME:
                    isOpen = true;
                    break;
                case PoxiaoKeyConst.SELECT:
                case PoxiaoKeyConst.USERSELECT:
                case PoxiaoKeyConst.TREESELECT:
                case PoxiaoKeyConst.DEPSELECT:
                case PoxiaoKeyConst.POSSELECT:
                    switch (item.IsMultiple)
                    {
                        case true:
                            isOpen = true;
                            break;
                    }
                    break;
            }
        });
        return isOpen;
    }
}