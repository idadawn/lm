using Poxiao.Infrastructure.Const;

namespace Poxiao.VisualDev.Engine;

/// <summary>
/// 模板解析.
/// </summary>
public static class TemplateAnalysis
{
    /// <summary>
    /// 解析模板数据
    /// 移除模板内的布局类型控件.
    /// </summary>
    public static List<FieldsModel> AnalysisTemplateData(List<FieldsModel> fieldsModelList)
    {
        var template = new List<FieldsModel>();

        // 将模板内的无限children解析出来
        // 不包含子表children
        foreach (FieldsModel? item in fieldsModelList)
        {
            ConfigModel? config = item.__config__;
            switch (config.poxiaoKey)
            {
                case PoxiaoKeyConst.TABLE: // 设计子表
                    item.__config__.defaultCurrent = item.__config__.children.Any(it => it.__config__.defaultCurrent);
                    template.Add(item);
                    break;
                case PoxiaoKeyConst.ROW: // 栅格布局
                case PoxiaoKeyConst.CARD: // 卡片容器
                case PoxiaoKeyConst.TABITEM: // 标签面板Item
                case PoxiaoKeyConst.TABLEGRIDTR: // 表格容器Tr
                case PoxiaoKeyConst.TABLEGRIDTD: // 表格容器Td
                    template.AddRange(AnalysisTemplateData(config.children));
                    break;
                case PoxiaoKeyConst.COLLAPSE: // 折叠面板
                case PoxiaoKeyConst.TAB: // 标签面板
                case PoxiaoKeyConst.TABLEGRID: // 表格容器
                    config.children.ForEach(item => template.AddRange(AnalysisTemplateData(item.__config__.children)));
                    break;
                case PoxiaoKeyConst.PoxiaoTEXT: // 文本
                case PoxiaoKeyConst.DIVIDER: // 分割线
                case PoxiaoKeyConst.GROUPTITLE: // 分组标题
                case PoxiaoKeyConst.BUTTON: // 按钮
                case PoxiaoKeyConst.ALERT: // 提示
                case PoxiaoKeyConst.LINK: // 链接
                    break;
                default:
                    template.Add(item);
                    break;
            }
        }

        return template;
    }
}