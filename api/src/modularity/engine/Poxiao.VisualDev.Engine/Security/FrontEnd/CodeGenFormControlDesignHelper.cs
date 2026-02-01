using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Security;
using Poxiao.VisualDev.Engine.Model.CodeGen;
using System.Text;
using System.Text.RegularExpressions;

namespace Poxiao.VisualDev.Engine.Security;

/// <summary>
/// 代码生成表单控件设计帮助类.
/// </summary>
public class CodeGenFormControlDesignHelper
{
    private static int active = 1;

    /// <summary>
    /// 表单控件设计.
    /// </summary>
    /// <param name="fieldList">组件列表.</param>
    /// <param name="realisticControls">真实控件.</param>
    /// <param name="gutter">间隔.</param>
    /// <param name="labelWidth">标签宽度.</param>
    /// <param name="columnDesignModel">列表显示列列表.</param>
    /// <param name="dataType">数据类型
    /// 1-普通列表,2-左侧树形+普通表格,3-分组表格,4-编辑表格.</param>
    /// <param name="logic">4-PC,5-App.</param>
    /// <param name="isMain">是否主循环.</param>
    /// <returns></returns>
    public static List<FormControlDesignModel> FormControlDesign(List<FieldsModel> fieldList, List<FieldsModel> realisticControls, int gutter, int labelWidth, List<IndexGridFieldModel> columnDesignModel, int dataType, int logic, bool isMain = false)
    {
        if (isMain) active = 1;
        List<FormControlDesignModel> list = new List<FormControlDesignModel>();
        foreach (var item in fieldList)
        {
            var config = item.Config;
            var needTemplateJson = new List<string>() { PoxiaoKeyConst.POPUPTABLESELECT, PoxiaoKeyConst.POPUPSELECT, PoxiaoKeyConst.AUTOCOMPLETE };
            var specialDateAttribute = new List<string>() { PoxiaoKeyConst.DATE, PoxiaoKeyConst.TIME };
            switch (config.poxiaoKey)
            {
                case PoxiaoKeyConst.ROW:
                    {
                        list.Add(new FormControlDesignModel()
                        {
                            poxiaoKey = config.poxiaoKey,
                            Span = config.span,
                            Gutter = gutter,
                            Children = FormControlDesign(config.children, realisticControls, gutter, labelWidth, columnDesignModel, dataType, logic)
                        });
                    }

                    break;
                case PoxiaoKeyConst.TABLEGRIDTR:
                    {
                        list.Add(new FormControlDesignModel()
                        {
                            poxiaoKey = config.poxiaoKey,
                            Children = FormControlDesign(config.children, realisticControls, gutter, labelWidth, columnDesignModel, dataType, logic)
                        });
                    }

                    break;
                case PoxiaoKeyConst.TABLEGRIDTD:
                    {
                        if (!config.merged)
                        {
                            list.Add(new FormControlDesignModel()
                            {
                                poxiaoKey = config.poxiaoKey,
                                Colspan = config.colspan,
                                Rowspan = config.rowspan,
                                Children = FormControlDesign(config.children, realisticControls, gutter, labelWidth, columnDesignModel, dataType, logic)
                            });
                        }
                    }

                    break;
                case PoxiaoKeyConst.TABLE:
                    {
                        List<FormControlDesignModel> childrenTableList = new List<FormControlDesignModel>();
                        var childrenRealisticControls = realisticControls.Find(it => it.VModel.Equals(item.VModel) && it.Config.poxiaoKey.Equals(PoxiaoKeyConst.TABLE)).Config.children;
                        foreach (var children in config.children)
                        {
                            var childrenConfig = children.Config;
                            switch (childrenConfig.poxiaoKey)
                            {
                                case PoxiaoKeyConst.RELATIONFORMATTR:
                                case PoxiaoKeyConst.POPUPATTR:
                                    {
                                        var relationField = Regex.Match(children.relationField, @"^(.+)_poxiaoTable_").Groups[1].Value;
                                        relationField = relationField.Replace(string.Format("poxiao_{0}_poxiao_", childrenConfig.relationTable), "");
                                        var relationControl = config.children.Find(it => it.VModel == relationField);
                                        childrenTableList.Add(new FormControlDesignModel()
                                        {
                                            vModel = children.VModel.IsNotEmptyOrNull() ? string.Format("v-model=\"scope.row.{0}\"", children.VModel) : string.Empty,
                                            Style = children.style != null && !children.style.ToString().Equals("{}") ? $":style='{children.style.ToJsonString()}' " : string.Empty,
                                            poxiaoKey = childrenConfig.poxiaoKey,
                                            OriginalName = childrenConfig.isStorage == 2 ? children.VModel : relationField,
                                            Name = childrenConfig.isStorage == 2 ? children.VModel : relationField,
                                            RelationField = relationField,
                                            ShowField = children.showField,
                                            NoShow = relationControl.Config.noShow ? "v-if='false' " : string.Empty,
                                            Tag = childrenConfig.tag,
                                            Label = string.IsNullOrEmpty(childrenConfig.label) ? null : childrenConfig.label,
                                            TipLabel = string.IsNullOrEmpty(childrenConfig.tipLabel) ? null : childrenConfig.tipLabel,
                                            Span = childrenConfig.span,
                                            IsStorage = childrenConfig.isStorage,
                                            LabelWidth = childrenConfig?.labelWidth ?? labelWidth,
                                            ColumnWidth = childrenConfig?.columnWidth != null ? $"width='{childrenConfig.columnWidth}' " : null,
                                            required = childrenConfig.required,
                                        });
                                    }

                                    break;
                                default:
                                    {
                                        var realisticControl = childrenRealisticControls.Find(it => it.VModel.Equals(children.VModel) && it.Config.poxiaoKey.Equals(childrenConfig.poxiaoKey));
                                        childrenTableList.Add(new FormControlDesignModel()
                                        {
                                            poxiaoKey = childrenConfig.poxiaoKey,
                                            Name = children.VModel,
                                            OriginalName = children.VModel,
                                            Style = children.style != null && !children.style.ToString().Equals("{}") ? $":style='{children.style.ToJsonString()}' " : string.Empty,
                                            Span = childrenConfig.span,
                                            Border = children.border ? "border " : string.Empty,
                                            Placeholder = children.placeholder != null ? $"placeholder='{children.placeholder}' " : string.Empty,
                                            Clearable = children.clearable ? "clearable " : string.Empty,
                                            Readonly = children.@readonly ? "readonly " : string.Empty,
                                            Disabled = children.disabled ? "disabled " : string.Empty,
                                            IsDisabled = item.disabled ? "disabled " : string.Format(":disabled=\"judgeWrite('{0}') || judgeWrite('{0}-{1}')\" ", item.VModel, children.VModel),
                                            ShowWordLimit = children.showWordlimit ? "show-word-limit " : string.Empty,
                                            Type = children.type != null ? $"type='{children.type}' " : string.Empty,
                                            Format = children.format != null ? $"format='{children.format}' " : string.Empty,
                                            ValueFormat = children.valueformat != null ? $"value-format='{children.valueformat}' " : string.Empty,
                                            AutoSize = children.autosize != null ? $":autosize='{children.autosize.ToJsonString()}' " : string.Empty,
                                            Multiple = (childrenConfig.poxiaoKey.Equals(PoxiaoKeyConst.CASCADER) ? children.props.props.multiple : children.multiple) ? $"multiple " : string.Empty,
                                            OptionType = children.optionType != null ? string.Format("optionType=\"{0}\" ", children.optionType) : string.Empty,
                                            PrefixIcon = !string.IsNullOrEmpty(children.prefixicon) ? $"prefix-icon='{children.prefixicon}' " : string.Empty,
                                            SuffixIcon = !string.IsNullOrEmpty(children.suffixicon) ? $"suffix-icon='{children.suffixicon}' " : string.Empty,
                                            MaxLength = !string.IsNullOrEmpty(children.maxlength) ? $"maxlength='{children.maxlength}' " : string.Empty,
                                            ShowPassword = children.showPassword ? "show-password " : string.Empty,
                                            Filterable = children.filterable ? "filterable " : string.Empty,
                                            Label = string.IsNullOrEmpty(childrenConfig.label) ? null : childrenConfig.label,
                                            TipLabel = string.IsNullOrEmpty(childrenConfig.tipLabel) ? null : childrenConfig.tipLabel,
                                            Props = children.props?.props,
                                            MainProps = children.props != null ? $":props='{string.Format("{0}_{1}", item.VModel, children.VModel)}Props' " : string.Empty,
                                            Tag = childrenConfig.tag,
                                            Options = children.options != null ? (realisticControl.IsLinkage ? $":options='{string.Format("scope.row.{0}", children.VModel)}Options' " : $":options='{string.Format("{0}_{1}", item.VModel, children.VModel)}Options' ") : string.Empty,
                                            ShowAllLevels = children.showalllevels ? "show-all-levels " : string.Empty,
                                            Separator = !string.IsNullOrEmpty(children.separator) ? $"separator='{children.separator}' " : string.Empty,
                                            RangeSeparator = !string.IsNullOrEmpty(children.rangeseparator) ? $"range-separator='{children.rangeseparator}' " : string.Empty,
                                            StartPlaceholder = !string.IsNullOrEmpty(children.startplaceholder) ? $"start-placeholder='{children.startplaceholder}' " : string.Empty,
                                            EndPlaceholder = !string.IsNullOrEmpty(children.endplaceholder) ? $"end-placeholder='{children.endplaceholder}' " : string.Empty,
                                            PickerOptions = children.pickeroptions != null && children.pickeroptions.ToJsonString() != "null" ? $":picker-options='{children.pickeroptions.ToJsonString()}' " : string.Empty,
                                            Required = childrenConfig.required ? "required " : string.Empty,
                                            Step = children.step != null ? $":step='{children.step}' " : string.Empty,
                                            StepStrictly = children.stepstrictly ? "step-strictly " : string.Empty,
                                            Max = children.max != null && children.max != 0 ? $":max='{children.max}' " : string.Empty,
                                            Min = children.min != null ? $":min='{children.min}' " : string.Empty,
                                            ColumnWidth = childrenConfig.columnWidth != null ? $"width='{childrenConfig.columnWidth}' " : null,
                                            ModelId = children.modelId != null ? children.modelId : string.Empty,
                                            RelationField = children.relationField != null ? $"relationField='{children.relationField}' " : string.Empty,
                                            ColumnOptions = children.columnOptions != null ? $":columnOptions='{string.Format("{0}_{1}", item.VModel, children.VModel)}Options' " : string.Empty,
                                            TemplateJson = needTemplateJson.Contains(childrenConfig.poxiaoKey) ? string.Format(":templateJson='{0}_{1}TemplateJson' ", item.VModel, children.VModel) : string.Empty,
                                            HasPage = children.hasPage ? "hasPage " : string.Empty,
                                            PageSize = children.pageSize != null ? $":pageSize='{children.pageSize}' " : string.Empty,
                                            PropsValue = children.propsValue != null ? $"propsValue='{children.propsValue}' " : string.Empty,
                                            InterfaceId = children.interfaceId != null ? $"interfaceId='{children.interfaceId}' " : string.Empty,
                                            Precision = children.precision != null ? $":precision='{children.precision}' " : string.Empty,
                                            ActiveText = !string.IsNullOrEmpty(children.activetext) ? $"active-text='{children.activetext}' " : string.Empty,
                                            InactiveText = !string.IsNullOrEmpty(children.inactivetext) ? $"inactive-text='{children.inactivetext}' " : string.Empty,
                                            ActiveColor = !string.IsNullOrEmpty(children.activecolor) ? $"active-color='{children.activecolor}' " : string.Empty,
                                            InactiveColor = !string.IsNullOrEmpty(children.inactivecolor) ? $"inactive-color='{children.inactivecolor}' " : string.Empty,
                                            IsSwitch = childrenConfig.poxiaoKey == PoxiaoKeyConst.SWITCH ? $":active-value='{children.activevalue}' :inactive-value='{children.inactivevalue}' " : string.Empty,
                                            ShowStops = children.showstops ? $"show-stops " : string.Empty,
                                            Accept = !string.IsNullOrEmpty(children.accept) ? $"accept='{children.accept}' " : string.Empty,
                                            ShowTip = children.showTip ? $"showTip " : string.Empty,
                                            FileSize = children.fileSize != null && !string.IsNullOrEmpty(children.fileSize.ToString()) ? $":fileSize='{children.fileSize}' " : string.Empty,
                                            SizeUnit = !string.IsNullOrEmpty(children.sizeUnit) ? $"sizeUnit='{children.sizeUnit}' " : string.Empty,
                                            Limit = children.limit != null ? $":limit='{children.limit}' " : string.Empty,
                                            ButtonText = !string.IsNullOrEmpty(children.buttonText) ? $"buttonText='{children.buttonText}' " : string.Empty,
                                            Level = childrenConfig.poxiaoKey == PoxiaoKeyConst.ADDRESS ? $":level='{children.level}' " : string.Empty,
                                            NoShow = childrenConfig.noShow ? "v-if='false' " : string.Empty,
                                            Prepend = children.Slot != null && !string.IsNullOrEmpty(children.Slot.prepend) ? children.Slot.prepend : null,
                                            Append = children.Slot != null && !string.IsNullOrEmpty(children.Slot.append) ? children.Slot.append : null,
                                            ShowLevel = !string.IsNullOrEmpty(children.showLevel) ? string.Empty : string.Empty,
                                            LabelWidth = childrenConfig?.labelWidth ?? labelWidth,
                                            IsStorage = config.isStorage,
                                            PopupType = !string.IsNullOrEmpty(children.popupType) ? $"popupType='{children.popupType}' " : string.Empty,
                                            PopupTitle = !string.IsNullOrEmpty(children.popupTitle) ? $"popupTitle='{children.popupTitle}' " : string.Empty,
                                            PopupWidth = !string.IsNullOrEmpty(children.popupWidth) ? $"popupWidth='{children.popupWidth}' " : string.Empty,
                                            Field = childrenConfig.poxiaoKey.Equals(PoxiaoKeyConst.RELATIONFORM) || childrenConfig.poxiaoKey.Equals(PoxiaoKeyConst.POPUPSELECT) ? $":field=\"'{children.VModel}'+scope.$index\" " : string.Empty,
                                            required = childrenConfig.required,
                                            SelectType = children.selectType != null ? children.selectType : string.Empty,
                                            AbleDepIds = children.selectType != null && children.selectType == "custom" && (childrenConfig.poxiaoKey.Equals(PoxiaoKeyConst.USERSELECT) || childrenConfig.poxiaoKey.Equals(PoxiaoKeyConst.DEPSELECT)) ? string.Format(":ableDepIds='{0}_{1}_AbleDepIds' ", item.VModel, children.VModel) : string.Empty,
                                            AblePosIds = children.selectType != null && children.selectType == "custom" && (childrenConfig.poxiaoKey.Equals(PoxiaoKeyConst.USERSELECT) || childrenConfig.poxiaoKey.Equals(PoxiaoKeyConst.POSSELECT)) ? string.Format(":ablePosIds='{0}_{1}_AblePosIds' ", item.VModel, children.VModel) : string.Empty,
                                            AbleUserIds = children.selectType != null && children.selectType == "custom" && childrenConfig.poxiaoKey.Equals(PoxiaoKeyConst.USERSELECT) ? string.Format(":ableUserIds='{0}_{1}_AbleUserIds' ", item.VModel, children.VModel) : string.Empty,
                                            AbleRoleIds = children.selectType != null && children.selectType == "custom" && childrenConfig.poxiaoKey.Equals(PoxiaoKeyConst.USERSELECT) ? string.Format(":ableRoleIds='{0}_{1}_AbleRoleIds' ", item.VModel, children.VModel) : string.Empty,
                                            AbleGroupIds = children.selectType != null && children.selectType == "custom" && childrenConfig.poxiaoKey.Equals(PoxiaoKeyConst.USERSELECT) ? string.Format(":ableGroupIds='{0}_{1}_AbleGroupIds' ", item.VModel, children.VModel) : string.Empty,
                                            AbleIds = children.selectType != null && children.selectType == "custom" && childrenConfig.poxiaoKey.Equals(PoxiaoKeyConst.USERSSELECT) ? string.Format(":ableIds='{0}_{1}_AbleIds' ", item.VModel, children.VModel) : string.Empty,
                                            UserRelationAttr = GetUserRelationAttr(children, realisticControls, logic),
                                            IsLinked = realisticControl.IsLinked,
                                            IsLinkage = realisticControl.IsLinkage,
                                            IsRelationForm = childrenConfig.poxiaoKey.Equals(PoxiaoKeyConst.RELATIONFORM),
                                            PathType = !string.IsNullOrEmpty(children.pathType) ? string.Format("pathType=\"{0}\" ", children.pathType) : string.Empty,
                                            IsAccount = children.isAccount != -1 ? string.Format(":isAccount=\"{0}\" ", children.isAccount) : string.Empty,
                                            Folder = !string.IsNullOrEmpty(children.folder) ? string.Format("folder=\"{0}\" ", children.folder) : string.Empty,
                                            DefaultCurrent = childrenConfig.defaultCurrent,
                                            Direction = !string.IsNullOrEmpty(children.direction) ? string.Format("direction=\"{0}\" ", children.direction) : string.Empty,
                                            Total = children.total > 0 ? string.Format(":total=\"{0}\" ", children.total) : string.Empty,
                                            AddonBefore = !string.IsNullOrEmpty(children.addonBefore) ? string.Format("addonBefore=\"{0}\" ", children.addonBefore) : string.Empty,
                                            AddonAfter = !string.IsNullOrEmpty(children.addonAfter) ? string.Format("addonAfter=\"{0}\" ", children.addonAfter) : string.Empty,
                                            Thousands = children.thousands ? string.Format("thousands ", children.addonBefore) : string.Empty,
                                            AmountChinese = children.isAmountChinese ? string.Format("isAmountChinese ", children.addonBefore) : string.Empty,
                                            ControlsPosition = !string.IsNullOrEmpty(children.controlsposition) ? $"controlsPosition='{children.controlsposition}' " : string.Empty,
                                            StartTime = specialDateAttribute.Contains(childrenConfig.poxiaoKey) ? SpecialTimeAttributeAssembly(children, realisticControls, 1, false) : string.Empty,
                                            EndTime = specialDateAttribute.Contains(childrenConfig.poxiaoKey) ? SpecialTimeAttributeAssembly(children, realisticControls, 2, false) : string.Empty,
                                            TipText = !string.IsNullOrEmpty(children.tipText) ? string.Format("tipText=\"{0}\" ", children.tipText) : string.Empty,
                                        });
                                    }

                                    break;
                            }
                        }

                        list.Add(new FormControlDesignModel()
                        {
                            poxiaoKey = config.poxiaoKey,
                            Name = item.VModel,
                            OriginalName = config.tableName,
                            TipLabel = string.IsNullOrEmpty(config.tipLabel) ? null : config.tipLabel,
                            Span = config.span,
                            ShowTitle = config.showTitle,
                            Label = string.IsNullOrEmpty(config.label) ? null : config.label,
                            ChildTableName = config.tableName.ParseToPascalCase(),
                            Children = childrenTableList,
                            LabelWidth = config?.labelWidth ?? labelWidth,
                            ShowSummary = item.showSummary,
                            required = childrenTableList.Any(it => it.required.Equals(true)),
                            AddType = item.addType,
                            IsRelationForm = childrenTableList.Any(it => it.IsRelationForm.Equals(true)),
                            DefaultCurrent = childrenTableList.Any(it => it.DefaultCurrent.Equals(true)),
                        });
                    }

                    break;
                case PoxiaoKeyConst.CARD:
                    {
                        list.Add(new FormControlDesignModel()
                        {
                            poxiaoKey = config.poxiaoKey,
                            OriginalName = item.VModel,
                            Shadow = item.shadow,
                            Children = FormControlDesign(config.children, realisticControls, gutter, labelWidth, columnDesignModel, dataType, logic),
                            Span = config.span,
                            Content = item.header,
                            LabelWidth = config?.labelWidth ?? labelWidth,
                            TipLabel = string.IsNullOrEmpty(config.tipLabel) ? null : config.tipLabel,
                        });
                    }

                    break;
                case PoxiaoKeyConst.DIVIDER:
                    {
                        list.Add(new FormControlDesignModel()
                        {
                            poxiaoKey = config.poxiaoKey,
                            OriginalName = item.VModel,
                            Span = config.span,
                            Contentposition = item.contentposition,
                            Default = item.Slot.@default,
                            LabelWidth = config?.labelWidth ?? labelWidth,
                        });
                    }

                    break;
                case PoxiaoKeyConst.COLLAPSE:
                    {
                        // 先加为了防止 children下 还有折叠面板
                        List<FormControlDesignModel> childrenCollapseList = new List<FormControlDesignModel>();
                        foreach (var children in config.children)
                        {
                            var child = FormControlDesign(children.Config.children, realisticControls, gutter, labelWidth, columnDesignModel, dataType, logic);
                            childrenCollapseList.Add(new FormControlDesignModel()
                            {
                                Title = children.title,
                                Name = children.name,
                                Gutter = gutter,
                                Children = child,
                                IsRelationForm = child.Any(x => x.IsRelationForm),
                            });
                        }

                        list.Add(new FormControlDesignModel()
                        {
                            poxiaoKey = config.poxiaoKey,
                            Accordion = item.accordion ? "true" : "false",
                            Name = "active" + active++,
                            Active = childrenCollapseList.Select(it => it.Name).ToJsonString(),
                            Children = childrenCollapseList,
                            Span = config.span,
                            LabelWidth = config?.labelWidth ?? labelWidth,
                            IsRelationForm = childrenCollapseList.Any(x => x.IsRelationForm),
                        });
                    }

                    break;
                case PoxiaoKeyConst.TAB:
                    {
                        // 先加为了防止 children下 还有折叠面板
                        List<FormControlDesignModel> childrenCollapseList = new List<FormControlDesignModel>();
                        foreach (var children in config.children)
                        {
                            var child = FormControlDesign(children.Config.children, realisticControls, gutter, labelWidth, columnDesignModel, dataType, logic);
                            childrenCollapseList.Add(new FormControlDesignModel()
                            {
                                Title = children.title,
                                Gutter = gutter,
                                Children = child,
                                IsRelationForm = child.Any(x => x.IsRelationForm),
                            });
                        }

                        list.Add(new FormControlDesignModel()
                        {
                            poxiaoKey = config.poxiaoKey,
                            Type = item.type,
                            TabPosition = item.tabPosition,
                            Name = "active" + active++,
                            Active = config.active.ToString(),
                            Children = childrenCollapseList,
                            Span = config.span,
                            LabelWidth = config?.labelWidth ?? labelWidth,
                            IsRelationForm = childrenCollapseList.Any(x => x.IsRelationForm),
                        });
                    }

                    break;
                case PoxiaoKeyConst.TABLEGRID:
                    {
                        List<FormControlDesignModel> childrenCollapseList = FormControlDesign(config.children, realisticControls, gutter, labelWidth, columnDesignModel, dataType, logic);

                        list.Add(new FormControlDesignModel()
                        {
                            poxiaoKey = config.poxiaoKey,
                            Style = "{'--borderType':'" + config.borderType + "','--borderColor':'" + config.borderColor + "','--borderWidth':'" + config.borderWidth + "px'}",
                            Children = childrenCollapseList,
                            IsRelationForm = childrenCollapseList.Any(x => x.IsRelationForm),
                        });
                    }

                    break;
                case PoxiaoKeyConst.GROUPTITLE:
                    {
                        list.Add(new FormControlDesignModel()
                        {
                            poxiaoKey = config.poxiaoKey,
                            Span = config.span,
                            Contentposition = item.contentposition,
                            Content = item.content,
                            LabelWidth = config?.labelWidth ?? labelWidth,
                            TipLabel = string.IsNullOrEmpty(item.tipLabel) ? null : item.tipLabel,
                        });
                    }

                    break;
                case PoxiaoKeyConst.PoxiaoTEXT:
                    {
                        list.Add(new FormControlDesignModel()
                        {
                            poxiaoKey = config.poxiaoKey,
                            Span = config.span,
                            DefaultValue = config.defaultValue,
                            TextStyle = item.textStyle != null ? item.textStyle.ToJsonString() : string.Empty,
                            Style = item.style.ToJsonString(),
                            LabelWidth = config?.labelWidth ?? labelWidth,
                        });
                    }

                    break;
                case PoxiaoKeyConst.BUTTON:
                    {
                        list.Add(new FormControlDesignModel()
                        {
                            poxiaoKey = config.poxiaoKey,
                            Span = config.span,
                            Align = item.align,
                            ButtonText = item.buttonText,
                            Type = item.type,
                            Disabled = item.disabled ? "disabled " : string.Empty,
                            LabelWidth = config?.labelWidth ?? labelWidth,
                        });
                    }

                    break;
                case PoxiaoKeyConst.LINK:
                    {
                        list.Add(new FormControlDesignModel()
                        {
                            poxiaoKey = config.poxiaoKey,
                            Span = config.span,
                            Content = item.content,
                            Href = item.href,
                            Target = item.target,
                            TextStyle = item.textStyle != null ? item.textStyle.ToJsonString() : string.Empty,
                        });
                    }

                    break;
                case PoxiaoKeyConst.ALERT:
                    {
                        list.Add(new FormControlDesignModel()
                        {
                            poxiaoKey = config.poxiaoKey,
                            Span = config.span,
                            Title = item.title,
                            Type = item.type,
                            ShowIcon = item.showIcon ? "true" : "false",
                            Description = item.description,
                            CloseText = item.closeText,
                            Closable = item.closable,
                        });
                    }

                    break;
                case PoxiaoKeyConst.RELATIONFORMATTR:
                case PoxiaoKeyConst.POPUPATTR:
                    {
                        var relationField = Regex.Match(item.relationField, @"^(.+)_poxiaoTable_").Groups[1].Value;
                        var relationControl = realisticControls.Find(it => it.VModel == relationField);
                        var columnDesign = columnDesignModel?.Find(it => it.VModel == item.VModel);
                        list.Add(new FormControlDesignModel()
                        {
                            vModel = item.VModel.IsNotEmptyOrNull() ? string.Format("v-model=\"dataForm.{0}\"", item.VModel) : string.Empty,
                            IsInlineEditor = columnDesignModel != null ? columnDesignModel.Any(it => it.VModel == item.VModel) : false,
                            Style = item.style != null && !item.style.ToString().Equals("{}") ? $":style='{item.style.ToJsonString()}' " : string.Empty,
                            poxiaoKey = config.poxiaoKey,
                            OriginalName = config.isStorage == 2 ? item.VModel : relationField,
                            Name = config.isStorage == 2 ? item.VModel : relationField,
                            RelationField = relationField,
                            ShowField = item.showField,
                            NoShow = config.isStorage == 2 ? config.noShow ? "v-if='false' " : string.Empty : relationControl.Config.noShow ? "v-if='false' " : string.Empty,
                            Tag = config.tag,
                            Label = string.IsNullOrEmpty(config.label) ? null : config.label,
                            Span = config.span,
                            IsStorage = config.isStorage,
                            IndexWidth = columnDesign?.width,
                            LabelWidth = config?.labelWidth ?? labelWidth,
                            IndexAlign = columnDesign?.align,
                            TipLabel = string.IsNullOrEmpty(config.tipLabel) ? null : config.tipLabel,
                        });
                    }

                    break;
                default:
                    {
                        var realisticControl = realisticControls.Find(it => it.VModel.Equals(item.VModel) && it.Config.poxiaoKey.Equals(config.poxiaoKey));
                        var columnDesign = columnDesignModel?.Find(it => it.VModel == item.VModel);
                        string vModel = string.Empty;
                        var model = item.VModel;
                        vModel = dataType != 4 ? $"v-model='dataForm.{model}' " : $"v-model='scope.row.{model}' ";
                        list.Add(new FormControlDesignModel()
                        {
                            IsSort = columnDesign != null ? columnDesign.sortable : false,
                            IsInlineEditor = columnDesignModel != null ? columnDesignModel.Any(it => it.VModel == item.VModel) : false,
                            IndexAlign = columnDesign?.align,
                            IndexWidth = columnDesign?.width,
                            Name = item.VModel,
                            OriginalName = item.VModel,
                            poxiaoKey = config.poxiaoKey,
                            Border = item.border ? "border " : string.Empty,
                            Style = item.style != null && !item.style.ToString().Equals("{}") ? $":style='{item.style.ToJsonString()}' " : string.Empty,
                            Type = !string.IsNullOrEmpty(item.type) ? $"type='{item.type}' " : string.Empty,
                            Span = config.span,
                            Clearable = item.clearable ? "clearable " : string.Empty,
                            Readonly = item.@readonly ? "readonly " : string.Empty,
                            Required = config.required ? "required " : string.Empty,
                            Placeholder = !string.IsNullOrEmpty(item.placeholder) ? $"placeholder='{item.placeholder}' " : string.Empty,
                            Disabled = item.disabled ? "disabled " : string.Empty,
                            IsDisabled = item.disabled ? "disabled " : $":disabled='judgeWrite(\"{item.VModel}\")' ",
                            ShowWordLimit = item.showWordlimit ? "show-word-limit " : string.Empty,
                            Format = !string.IsNullOrEmpty(item.format) ? $"format='{item.format}' " : string.Empty,
                            ValueFormat = !string.IsNullOrEmpty(item.valueformat) ? $"value-format='{item.valueformat}' " : string.Empty,
                            AutoSize = item.autosize != null && item.autosize.ToJsonString() != "null" ? $":autosize='{item.autosize.ToJsonString()}' " : string.Empty,
                            Multiple = (config.poxiaoKey.Equals(PoxiaoKeyConst.CASCADER) ? item.props.props.multiple : item.multiple) ? $"multiple " : string.Empty,
                            IsRange = item.isrange ? "is-range " : string.Empty,
                            Props = item.props?.props,
                            MainProps = item.props?.props != null ? $":props='{model}Props' " : string.Empty,
                            OptionType = item.optionType != null ? string.Format("optionType=\"{0}\" ", item.optionType) : string.Empty,
                            Size = !string.IsNullOrEmpty(item.optionType) ? (item.optionType == "default" ? string.Empty : $"size='{item.size}' ") : string.Empty,
                            PrefixIcon = !string.IsNullOrEmpty(item.prefixicon) ? $"prefix-icon='{item.prefixicon}' " : string.Empty,
                            SuffixIcon = !string.IsNullOrEmpty(item.suffixicon) ? $"suffix-icon='{item.suffixicon}' " : string.Empty,
                            MaxLength = !string.IsNullOrEmpty(item.maxlength) ? $"maxlength='{item.maxlength}' " : string.Empty,
                            Step = item.step != null ? $":step='{item.step}' " : string.Empty,
                            StepStrictly = item.stepstrictly ? "step-strictly " : string.Empty,
                            ControlsPosition = !string.IsNullOrEmpty(item.controlsposition) ? $"controlsPosition='{item.controlsposition}' " : string.Empty,
                            ShowChinese = item.showChinese ? "showChinese " : string.Empty,
                            ShowPassword = item.showPassword ? "show-password " : string.Empty,
                            Filterable = item.filterable ? "filterable " : string.Empty,
                            ShowAllLevels = item.showalllevels ? "show-all-levels " : string.Empty,
                            Separator = !string.IsNullOrEmpty(item.separator) ? $"separator='{item.separator}' " : string.Empty,
                            RangeSeparator = !string.IsNullOrEmpty(item.rangeseparator) ? $"range-separator='{item.rangeseparator}' " : string.Empty,
                            StartPlaceholder = !string.IsNullOrEmpty(item.startplaceholder) ? $"start-placeholder='{item.startplaceholder}' " : string.Empty,
                            EndPlaceholder = !string.IsNullOrEmpty(item.endplaceholder) ? $"end-placeholder='{item.endplaceholder}' " : string.Empty,
                            PickerOptions = item.pickeroptions != null && item.pickeroptions.ToJsonString() != "null" ? $":picker-options='{item.pickeroptions.ToJsonString()}' " : string.Empty,
                            Options = item.options != null ? $":options='{item.VModel}Options' " : string.Empty,
                            Max = item.max != null && item.max != 0 ? $":max='{item.max}' " : string.Empty,
                            AllowHalf = item.allowhalf ? "allow-half " : string.Empty,
                            ShowTexts = item.showtext ? $"show-text " : string.Empty,
                            ShowScore = item.showScore ? $"show-score " : string.Empty,
                            ShowAlpha = item.showalpha ? $"show-alpha " : string.Empty,
                            ColorFormat = !string.IsNullOrEmpty(item.colorformat) ? $"color-format='{item.colorformat}' " : string.Empty,
                            ActiveText = !string.IsNullOrEmpty(item.activetext) ? $"active-text='{item.activetext}' " : string.Empty,
                            InactiveText = !string.IsNullOrEmpty(item.inactivetext) ? $"inactive-text='{item.inactivetext}' " : string.Empty,
                            ActiveColor = !string.IsNullOrEmpty(item.activecolor) ? $"active-color='{item.activecolor}' " : string.Empty,
                            InactiveColor = !string.IsNullOrEmpty(item.inactivecolor) ? $"inactive-color='{item.inactivecolor}' " : string.Empty,
                            IsSwitch = config.poxiaoKey == PoxiaoKeyConst.SWITCH ? $":active-value='{item.activevalue}' :inactive-value='{item.inactivevalue}' " : string.Empty,
                            Min = item.min != null ? $":min='{item.min}' " : string.Empty,
                            ShowStops = item.showstops ? $"show-stops " : string.Empty,
                            Range = item.range ? $"range " : string.Empty,
                            Accept = !string.IsNullOrEmpty(item.accept) ? $"accept='{item.accept}' " : string.Empty,
                            ShowTip = item.showTip ? $"showTip " : string.Empty,
                            FileSize = item.fileSize != null && !string.IsNullOrEmpty(item.fileSize.ToString()) ? $":fileSize='{item.fileSize}' " : string.Empty,
                            SizeUnit = !string.IsNullOrEmpty(item.sizeUnit) ? $"sizeUnit='{item.sizeUnit}' " : string.Empty,
                            Limit = item.limit != null ? $":limit='{item.limit}' " : string.Empty,
                            Contentposition = !string.IsNullOrEmpty(item.contentposition) ? $"content-position='{item.contentposition}' " : string.Empty,
                            ButtonText = !string.IsNullOrEmpty(item.buttonText) ? $"buttonText='{item.buttonText}' " : string.Empty,
                            Level = config.poxiaoKey == PoxiaoKeyConst.ADDRESS ? $":level='{item.level}' " : string.Empty,
                            ActionText = !string.IsNullOrEmpty(item.actionText) ? $"actionText='{item.actionText}' " : string.Empty,
                            Shadow = !string.IsNullOrEmpty(item.shadow) ? $"shadow='{item.shadow}' " : string.Empty,
                            Content = !string.IsNullOrEmpty(item.content) ? $"content='{item.content}' " : string.Empty,
                            NoShow = config.noShow ? "v-if='false' " : string.Empty,
                            Label = string.IsNullOrEmpty(config.label) ? null : config.label,
                            TipLabel = string.IsNullOrEmpty(config.tipLabel) ? null : config.tipLabel,
                            vModel = vModel,
                            Prepend = item.Slot != null && !string.IsNullOrEmpty(item.Slot.prepend) ? item.Slot.prepend : null,
                            Append = item.Slot != null && !string.IsNullOrEmpty(item.Slot.append) ? item.Slot.append : null,
                            Tag = config.tag,
                            Count = item.max.ParseToInt(),
                            ModelId = item.modelId != null ? item.modelId : string.Empty,
                            RelationField = item.relationField != null ? $"relationField='{item.relationField}' " : string.Empty,
                            ColumnOptions = item.columnOptions != null ? $":columnOptions='{item.VModel}Options' " : string.Empty,
                            TemplateJson = needTemplateJson.Contains(config.poxiaoKey) ? string.Format(":templateJson='{0}TemplateJson' ", item.VModel) : string.Empty,
                            HasPage = item.hasPage ? "hasPage " : string.Empty,
                            PageSize = item.pageSize != null ? $":pageSize='{item.pageSize}' " : string.Empty,
                            PropsValue = item.propsValue != null ? $"propsValue='{item.propsValue}' " : string.Empty,
                            InterfaceId = item.interfaceId != null ? $"interfaceId='{item.interfaceId}' " : string.Empty,
                            Precision = item.precision != null ? $":precision='{item.precision}' " : string.Empty,
                            ShowLevel = !string.IsNullOrEmpty(item.showLevel) ? string.Empty : string.Empty,
                            LabelWidth = config?.labelWidth ?? labelWidth,
                            IsStorage = config.isStorage,
                            PopupType = !string.IsNullOrEmpty(item.popupType) ? $"popupType='{item.popupType}' " : string.Empty,
                            PopupTitle = !string.IsNullOrEmpty(item.popupTitle) ? $"popupTitle='{item.popupTitle}' " : string.Empty,
                            PopupWidth = !string.IsNullOrEmpty(item.popupWidth) ? $"popupWidth='{item.popupWidth}' " : string.Empty,
                            Field = config.poxiaoKey.Equals(PoxiaoKeyConst.RELATIONFORM) || config.poxiaoKey.Equals(PoxiaoKeyConst.POPUPSELECT) ? $"field='{item.VModel}' " : string.Empty,
                            SelectType = item.selectType != null ? item.selectType : string.Empty,
                            AbleDepIds = item.selectType != null && item.selectType == "custom" && (config.poxiaoKey.Equals(PoxiaoKeyConst.USERSELECT) || config.poxiaoKey.Equals(PoxiaoKeyConst.DEPSELECT)) ? string.Format(":ableDepIds='{0}_AbleDepIds' ", item.VModel) : string.Empty,
                            AblePosIds = item.selectType != null && item.selectType == "custom" && (config.poxiaoKey.Equals(PoxiaoKeyConst.USERSELECT) || config.poxiaoKey.Equals(PoxiaoKeyConst.POSSELECT)) ? string.Format(":ablePosIds='{0}_AblePosIds' ", item.VModel) : string.Empty,
                            AbleUserIds = item.selectType != null && item.selectType == "custom" && config.poxiaoKey.Equals(PoxiaoKeyConst.USERSELECT) ? string.Format(":ableUserIds='{0}_AbleUserIds' ", item.VModel) : string.Empty,
                            AbleRoleIds = item.selectType != null && item.selectType == "custom" && config.poxiaoKey.Equals(PoxiaoKeyConst.USERSELECT) ? string.Format(":ableRoleIds='{0}_AbleRoleIds' ", item.VModel) : string.Empty,
                            AbleGroupIds = item.selectType != null && item.selectType == "custom" && config.poxiaoKey.Equals(PoxiaoKeyConst.USERSELECT) ? string.Format(":ableGroupIds='{0}_AbleGroupIds' ", item.VModel) : string.Empty,
                            AbleIds = item.selectType != null && item.selectType == "custom" && config.poxiaoKey.Equals(PoxiaoKeyConst.USERSSELECT) ? string.Format(":ableIds='{0}_AbleIds' ", item.VModel) : string.Empty,
                            UserRelationAttr = GetUserRelationAttr(item, realisticControls, logic),
                            IsLinked = realisticControl.IsLinked,
                            IsLinkage = realisticControl.IsLinkage,
                            IsRelationForm = config.poxiaoKey.Equals(PoxiaoKeyConst.RELATIONFORM),
                            PathType = !string.IsNullOrEmpty(item.pathType) ? string.Format("pathType=\"{0}\" ", item.pathType) : string.Empty,
                            IsAccount = item.isAccount != -1 ? string.Format(":isAccount=\"{0}\" ", item.isAccount) : string.Empty,
                            Folder = !string.IsNullOrEmpty(item.folder) ? string.Format("folder=\"{0}\" ", item.folder) : string.Empty,
                            DefaultCurrent = config.defaultCurrent,
                            Direction = !string.IsNullOrEmpty(item.direction) ? string.Format("direction=\"{0}\" ", item.direction) : string.Empty,
                            Total = item.total > 0 ? string.Format(":total=\"{0}\" ", item.total) : string.Empty,
                            AddonBefore = !string.IsNullOrEmpty(item.addonBefore) ? string.Format("addonBefore=\"{0}\" ", item.addonBefore) : string.Empty,
                            AddonAfter = !string.IsNullOrEmpty(item.addonAfter) ? string.Format("addonAfter=\"{0}\" ", item.addonAfter) : string.Empty,
                            Thousands = item.thousands ? string.Format("thousands ", item.addonBefore) : string.Empty,
                            AmountChinese = item.isAmountChinese ? string.Format("isAmountChinese ", item.addonBefore) : string.Empty,
                            TipText = !string.IsNullOrEmpty(item.tipText) ? string.Format("tipText=\"{0}\" ", item.tipText) : string.Empty,
                            StartTime = specialDateAttribute.Contains(config.poxiaoKey) ? SpecialTimeAttributeAssembly(item, fieldList, 1, true) : string.Empty,
                            EndTime = specialDateAttribute.Contains(config.poxiaoKey) ? SpecialTimeAttributeAssembly(item, fieldList, 2, true) : string.Empty,
                        });
                    }

                    break;
            }
        }

        return list;
    }

    /// <summary>
    /// 特殊时间属性组装.
    /// </summary>
    /// <param name="field">配置模型.</param>
    /// <param name="fieldList">全控件列表.</param>
    /// <param name="type">类型(1-startTime,2-endTime).</param>
    /// <param name="isMainTable">是否主副表.</param>
    /// <returns></returns>
    private static string SpecialTimeAttributeAssembly(FieldsModel field, List<FieldsModel> fieldList, int type, bool isMainTable)
    {
        var time = string.Empty;
        var config = field.Config;
        switch (config.poxiaoKey)
        {
            case PoxiaoKeyConst.DATE:
                switch (type)
                {
                    case 1:
                        switch (config.startTimeRule)
                        {
                            case true:
                                var relationField = SpecialAttributeAssociatedFields(config.startRelationField, fieldList);
                                time = string.Format(":startTime=\"dateTime(true,{0},{1},'{2}','{3}')\" ", config.startTimeType, config.startTimeTarget, config.startTimeValue, relationField);
                                break;
                        }
                        break;
                    case 2:
                        switch (config.endTimeRule)
                        {
                            case true:
                                var relationField = SpecialAttributeAssociatedFields(config.endRelationField, fieldList);
                                time = string.Format(":endTime=\"dateTime(true,{0},{1},'{2}','{3}')\" ", config.endTimeType, config.endTimeTarget, config.endTimeValue, relationField);
                                break;
                        }
                        break;
                }
                break;
            case PoxiaoKeyConst.TIME:
                switch (type)
                {
                    case 1:
                        switch (config.startTimeRule)
                        {
                            case true:
                                var relationField = SpecialAttributeAssociatedFields(config.startRelationField, fieldList);
                                time = string.Format(":startTime=\"time(true,{0},{1},'{2}','{3}','{4}')\" ", config.startTimeType, config.startTimeTarget, config.startTimeValue, field.format, relationField);
                                break;
                        }
                        break;
                    case 2:
                        switch (config.endTimeRule)
                        {
                            case true:
                                var relationField = SpecialAttributeAssociatedFields(config.endRelationField, fieldList);
                                time = string.Format(":endTime=\"time(true,{0},{1},'{2}','{3}','{4}')\" ", config.endTimeType, config.endTimeTarget, config.endTimeValue, field.format, relationField);
                                break;
                        }
                        break;
                }
                break;
        }
        return time;
    }

    /// <summary>
    /// 特殊属性关联字段.
    /// </summary>
    /// <param name="relationField"></param>
    /// <param name="fieldList"></param>
    /// <returns></returns>
    private static string SpecialAttributeAssociatedFields(string relationField, List<FieldsModel> fieldList)
    {
        var completeResults = string.Empty;
        switch (fieldList.Any(it => it.VModel.Equals(relationField)))
        {
            case true:
                completeResults = string.Format("dataForm.{0}", relationField);
                break;
            case false:
                if (relationField.IsMatch(@"tableField\d{3}-"))
                {
                    var subTable = relationField.Matches(@"tableField\d{3}-").FirstOrDefault().Replace("-", "");
                    relationField = relationField.ReplaceRegex("tableField\\d{3}-", "");
                    switch (fieldList.Any(it => it.Config.poxiaoKey.Equals(PoxiaoKeyConst.TABLE) && it.Config.children.Any(x => x.VModel.Equals(relationField))))
                    {
                        case true:
                            completeResults = string.Format("dataForm.{0}[scope.$index].{1}", subTable, relationField);
                            break;
                    }
                }
                break;
        }
        return completeResults;
    }

    /// <summary>
    /// 表单默认值控件列表.
    /// </summary>
    /// <param name="fieldList">组件列表.</param>
    /// <param name="searchField">查询字段.</param>
    /// <param name="subTableName">子表名称.</param>
    /// <param name="isMain">是否主表.</param>
    /// <returns></returns>
    public static DefaultFormControlModel DefaultFormControlList(List<FieldsModel> fieldList, List<IndexSearchFieldModel> searchField, string subTableName = null, bool isMain = true)
    {
        DefaultFormControlModel model = new DefaultFormControlModel();
        model.DateField = new List<DefaultTimeControl>();
        model.TimeField = new List<DefaultTimeControl>();
        model.ComSelectList = new List<DefaultComSelectControl>();
        model.DepSelectList = new List<DefaultDepSelectControl>();
        model.UserSelectList = new List<DefaultUserSelectControl>();
        model.SubTabelDefault = new List<DefaultFormControlModel>();

        // 获取表单内存在默认值控件
        foreach (var item in fieldList)
        {
            var config = item.Config;
            var search = new IndexSearchFieldModel();
            switch (isMain && !config.poxiaoKey.Equals(PoxiaoKeyConst.TABLE))
            {
                case false:
                    search = searchField?.Find(it => it.VModel.Equals(string.Format("{0}-{1}", subTableName, item.VModel)));
                    break;
                default:
                    search = searchField?.Find(it => it.VModel.Equals(string.Format("{0}", item.VModel)));
                    break;
            }

            // 未作为查询条件
            if (search == null)
            {
                search = new IndexSearchFieldModel();
                search.searchMultiple = false;
            }
            switch (config.defaultCurrent)
            {
                case true:
                    switch (config.poxiaoKey)
                    {
                        case PoxiaoKeyConst.TABLE:
                            model.SubTabelDefault.Add(DefaultFormControlList(item.Config.children, searchField, item.VModel, false));
                            break;
                        case PoxiaoKeyConst.TIME:
                            model.TimeField.Add(new DefaultTimeControl
                            {
                                Field = item.VModel,
                                Format = item.format
                            });
                            break;
                        case PoxiaoKeyConst.DATE:
                            model.DateField.Add(new DefaultTimeControl
                            {
                                Field = item.VModel,
                                Format = item.format
                            });
                            break;
                        case PoxiaoKeyConst.COMSELECT:
                            model.ComSelectList.Add(new DefaultComSelectControl()
                            {
                                IsMultiple = item.multiple,
                                IsSearchMultiple = (bool)search?.searchMultiple,
                                Field = item.VModel
                            });
                            break;
                        case PoxiaoKeyConst.DEPSELECT:
                            model.DepSelectList.Add(new DefaultDepSelectControl()
                            {
                                IsMultiple = item.multiple,
                                selectType = item.selectType,
                                IsSearchMultiple = (bool)search?.searchMultiple,
                                Field = item.VModel,
                                ableDepIds = item.ableDepIds.ToJsonString(),
                            });
                            break;
                        case PoxiaoKeyConst.USERSELECT:
                            model.UserSelectList.Add(new DefaultUserSelectControl()
                            {
                                IsMultiple = item.multiple,
                                selectType = item.selectType,
                                IsSearchMultiple = (bool)search?.searchMultiple,
                                Field = item.VModel,
                                ableDepIds = item.ableDepIds.ToJsonString(),
                                ableGroupIds = item.ableGroupIds.ToJsonString(),
                                ablePosIds = item.ablePosIds.ToJsonString(),
                                ableRoleIds = item.ableRoleIds.ToJsonString(),
                                ableUserIds = item.ableUserIds.ToJsonString(),
                            });
                            break;
                    }
                    break;
            }
        }

        switch (isMain)
        {
            case false:
                model.SubTableName = subTableName;
                model.IsExistTime = model.TimeField.Any();
                model.IsExistDate = model.DateField.Any();
                model.IsExistComSelect = model.ComSelectList.Any();
                model.IsExistDepSelect = model.DepSelectList.Any();
                model.IsExistUserSelect = model.UserSelectList.Any();
                break;
            default:
                model.IsExistTime = model.TimeField.Any() || model.SubTabelDefault.Any(it => it.TimeField.Any());
                model.IsExistDate = model.DateField.Any() || model.SubTabelDefault.Any(it => it.DateField.Any());
                model.IsExistComSelect = model.ComSelectList.Any() || model.SubTabelDefault.Any(it => it.ComSelectList.Any());
                model.IsExistDepSelect = model.DepSelectList.Any() || model.SubTabelDefault.Any(it => it.DepSelectList.Any());
                model.IsExistUserSelect = model.UserSelectList.Any() || model.SubTabelDefault.Any(it => it.UserSelectList.Any());
                model.IsExistSubTable = model.SubTabelDefault.Count > 0 ? true : false;
                break;
        }

        return model;
    }

    /// <summary>
    /// 判断控件开启特殊属性.
    /// </summary>
    /// <param name="fieldList">控件列表.</param>
    /// <param name="poxiaoKey">控件Key.</param>
    /// <returns></returns>
    public static bool DetermineWhetherTheControlHasEnabledSpecialAttributes(List<FieldsModel> fieldList, string poxiaoKey)
    {
        var numberOfControls = 0;
        foreach (var item in fieldList)
        {
            var config = item.Config;
            switch (config.poxiaoKey)
            {
                case PoxiaoKeyConst.TABLE:
                    numberOfControls += DetermineWhetherSpecialAttributesAreEnabledForControlsWithinASubtable(config.children, poxiaoKey);
                    break;
                case PoxiaoKeyConst.DATE:
                case PoxiaoKeyConst.TIME:
                    if (config.poxiaoKey.Equals(poxiaoKey) & (config.startTimeRule || config.endTimeRule))
                    {
                        numberOfControls++;
                    }
                    break;
            }
        }
        return numberOfControls > 0 ? true : false;
    }

    /// <summary>
    /// 子表指定日期格式集合.
    /// </summary>
    /// <param name="fieldList">子表控件.</param>
    /// <returns></returns>
    public static CodeGenSpecifyDateFormatSetModel CodeGenSpecifyDateFormatSetModel(FieldsModel fieldList)
    {
        var config = fieldList.Config;
        var result = new CodeGenSpecifyDateFormatSetModel
        {
            Field = fieldList.VModel,
            Children = new List<CodeGenSpecifyDateFormatSetModel>(),
        };
        foreach (var item in config.children)
        {
            var childrenConfig = item.Config;
            switch (childrenConfig.poxiaoKey)
            {
                case PoxiaoKeyConst.DATE:
                    switch (item.format)
                    {
                        case "yyyy":
                        case "yyyy-MM":
                        case "yyyy-MM-dd":
                        case "yyyy-MM-dd HH:mm":
                            result.Children.Add(new CodeGenSpecifyDateFormatSetModel
                            {
                                Field = item.VModel,
                                Format = item.format,
                            });
                            break;
                    }
                    break;
            }
        }
        if (result.Children.Count == 0) return null;
        return result;
    }

    private static int DetermineWhetherSpecialAttributesAreEnabledForControlsWithinASubtable(List<FieldsModel> fieldList, string poxiaoKey)
    {
        var numberOfControls = 0;
        foreach (var item in fieldList)
        {
            var config = item.Config;
            switch (config.poxiaoKey)
            {
                case PoxiaoKeyConst.DATE:
                case PoxiaoKeyConst.TIME:
                    if (config.poxiaoKey.Equals(poxiaoKey) & (config.startTimeRule || config.endTimeRule))
                    {
                        numberOfControls++;
                    }
                    break;
            }
        }
        return numberOfControls;
    }

    /// <summary>
    /// 表单控件选项配置.
    /// </summary>
    /// <param name="fieldList">组件列表.</param>
    /// <param name="realisticControls">真实控件.</param>
    /// <param name="columnDesignModel">列表设计.</param>
    /// <param name="type">1-Web设计,2-App设计,3-流程表单,4-Web表单,5-App表单.</param>
    /// <param name="isMain">是否主循环.</param>
    /// <returns></returns>
    public static List<CodeGenConvIndexListControlOptionDesign> FormControlProps(List<FieldsModel> fieldList, List<FieldsModel> realisticControls, ColumnDesignModel columnDesignModel, int type, bool isMain = false)
    {
        if (isMain) active = 1;
        List<CodeGenConvIndexListControlOptionDesign> list = new List<CodeGenConvIndexListControlOptionDesign>();
        foreach (var item in fieldList)
        {
            var config = item.Config;
            switch (config.poxiaoKey)
            {
                case PoxiaoKeyConst.CARD:
                case PoxiaoKeyConst.ROW:
                case PoxiaoKeyConst.TABLEGRID:
                case PoxiaoKeyConst.TABLEGRIDTR:
                case PoxiaoKeyConst.TABLEGRIDTD:
                    {
                        list.AddRange(FormControlProps(config.children, realisticControls, columnDesignModel, type));
                    }

                    break;
                case PoxiaoKeyConst.TABLE:
                    {
                        var childrenRealisticControls = realisticControls.Find(it => it.VModel.Equals(item.VModel) && it.Config.poxiaoKey.Equals(PoxiaoKeyConst.TABLE)).Config.children;
                        foreach (var children in config.children)
                        {
                            var columnDesign = columnDesignModel.searchList?.Find(it => it.VModel.Equals(string.Format("{0}-{1}", item.VModel, children.VModel)));
                            var childrenConfig = children.Config;
                            switch (childrenConfig.poxiaoKey)
                            {
                                case PoxiaoKeyConst.DEPSELECT:
                                    if (children.selectType != null && children.selectType == "custom")
                                    {
                                        list.Add(new CodeGenConvIndexListControlOptionDesign()
                                        {
                                            poxiaoKey = childrenConfig.poxiaoKey,
                                            IsStatic = true,
                                            IsIndex = false,
                                            IsProps = false,
                                            Content = string.Format("{0}_{1}_AbleDepIds:{2},", item.VModel, children.VModel, children.ableDepIds.ToJsonString()),
                                        });
                                    }
                                    break;
                                case PoxiaoKeyConst.POSSELECT:
                                    if (children.selectType != null && children.selectType == "custom")
                                    {
                                        list.Add(new CodeGenConvIndexListControlOptionDesign()
                                        {
                                            poxiaoKey = childrenConfig.poxiaoKey,
                                            IsStatic = true,
                                            IsIndex = false,
                                            IsProps = false,
                                            Content = string.Format("{0}_{1}_AblePosIds:{2},", item.VModel, children.VModel, children.ablePosIds.ToJsonString()),
                                        });
                                    }
                                    break;
                                case PoxiaoKeyConst.USERSELECT:
                                    if (children.selectType != null && children.selectType == "custom")
                                    {
                                        list.Add(new CodeGenConvIndexListControlOptionDesign()
                                        {
                                            poxiaoKey = childrenConfig.poxiaoKey,
                                            IsStatic = true,
                                            IsIndex = false,
                                            IsProps = false,
                                            Content = string.Format("{0}_{1}_AbleDepIds:{2},", item.VModel, children.VModel, children.ableDepIds.ToJsonString()),
                                        });
                                    }

                                    if (children.selectType != null && children.selectType == "custom")
                                    {
                                        list.Add(new CodeGenConvIndexListControlOptionDesign()
                                        {
                                            poxiaoKey = childrenConfig.poxiaoKey,
                                            IsStatic = true,
                                            IsIndex = false,
                                            IsProps = false,
                                            Content = string.Format("{0}_{1}_AblePosIds:{2},", item.VModel, children.VModel, children.ablePosIds.ToJsonString()),
                                        });
                                    }

                                    if (children.selectType != null && children.selectType == "custom")
                                    {
                                        list.Add(new CodeGenConvIndexListControlOptionDesign()
                                        {
                                            poxiaoKey = childrenConfig.poxiaoKey,
                                            IsStatic = true,
                                            IsIndex = false,
                                            IsProps = false,
                                            Content = string.Format("{0}_{1}_AbleUserIds:{2},", item.VModel, children.VModel, children.ableUserIds.ToJsonString()),
                                        });
                                    }

                                    if (children.selectType != null && children.selectType == "custom")
                                    {
                                        list.Add(new CodeGenConvIndexListControlOptionDesign()
                                        {
                                            poxiaoKey = childrenConfig.poxiaoKey,
                                            IsStatic = true,
                                            IsIndex = false,
                                            IsProps = false,
                                            Content = string.Format("{0}_{1}_AbleRoleIds:{2},", item.VModel, children.VModel, children.ableRoleIds.ToJsonString()),
                                        });
                                    }

                                    if (children.selectType != null && children.selectType == "custom")
                                    {
                                        list.Add(new CodeGenConvIndexListControlOptionDesign()
                                        {
                                            poxiaoKey = childrenConfig.poxiaoKey,
                                            IsStatic = true,
                                            IsIndex = false,
                                            IsProps = false,
                                            Content = string.Format("{0}_{1}_AbleGroupIds:{2},", item.VModel, children.VModel, children.ableGroupIds.ToJsonString()),
                                        });
                                    }
                                    break;
                                case PoxiaoKeyConst.USERSSELECT:
                                    if (children.selectType != null && children.selectType == "custom")
                                    {
                                        list.Add(new CodeGenConvIndexListControlOptionDesign()
                                        {
                                            poxiaoKey = childrenConfig.poxiaoKey,
                                            IsStatic = true,
                                            IsIndex = false,
                                            IsProps = false,
                                            Content = string.Format("{0}_{1}_AbleIds:{2},", item.VModel, children.VModel, children.ableIds.ToJsonString()),
                                        });
                                    }
                                    break;
                                case PoxiaoKeyConst.SELECT:
                                    {
                                        var realisticControl = childrenRealisticControls.Find(it => it.VModel.Equals(children.VModel) && it.Config.poxiaoKey.Equals(childrenConfig.poxiaoKey));
                                        switch (childrenConfig.dataType)
                                        {
                                            // 静态数据
                                            case "static":
                                                list.Add(new CodeGenConvIndexListControlOptionDesign()
                                                {
                                                    poxiaoKey = childrenConfig.poxiaoKey,
                                                    Name = string.Format("{0}_{1}", item.VModel, children.VModel),
                                                    DictionaryType = childrenConfig.dataType == "dictionary" ? childrenConfig.dictionaryType : (childrenConfig.dataType == "dynamic" ? childrenConfig.propsUrl : null),
                                                    DataType = childrenConfig.dataType,
                                                    IsStatic = true,
                                                    IsIndex = true,
                                                    IsProps = true,
                                                    Props = string.Format("{{'label':'{0}','value':'{1}'}}", children.props?.props?.label, children.props?.props?.value),
                                                    IsChildren = true,
                                                    Content = GetCodeGenConvIndexListControlOption(string.Format("{0}_{1}", item.VModel, children.VModel), children.options),
                                                    QueryProps = GetQueryPropsModel(children.props?.props).ToJsonString(CommonConst.options),
                                                });
                                                break;
                                            default:
                                                list.Add(new CodeGenConvIndexListControlOptionDesign()
                                                {
                                                    poxiaoKey = childrenConfig.poxiaoKey,
                                                    Name = string.Format("{0}_{1}", item.VModel, children.VModel),
                                                    OptionsName = string.Format("dataForm.{0}[i].{1}", item.VModel, children.VModel),
                                                    DictionaryType = childrenConfig.dataType == "dictionary" ? childrenConfig.dictionaryType : (childrenConfig.dataType == "dynamic" ? childrenConfig.propsUrl : null),
                                                    DataType = childrenConfig.dataType,
                                                    IsStatic = false,
                                                    IsIndex = true,
                                                    IsProps = true,
                                                    Props = string.Format("{{'label':'{0}','value':'{1}'}}", children.props?.props?.label, children.props?.props?.value),
                                                    IsChildren = true,
                                                    Content = string.Format("{0}Options : [],", string.Format("{0}_{1}", item.VModel, children.VModel)),
                                                    QueryProps = GetQueryPropsModel(children.props?.props).ToJsonString(CommonConst.options),
                                                    IsLinkage = realisticControl.IsLinkage,
                                                    TemplateJson = childrenConfig.dataType == "dynamic" ? childrenConfig.templateJson.ToJsonString() : "[]"
                                                });
                                                break;
                                        }
                                    }

                                    break;
                                case PoxiaoKeyConst.TREESELECT:
                                case PoxiaoKeyConst.CASCADER:
                                    {
                                        var realisticControl = childrenRealisticControls.Find(it => it.VModel.Equals(children.VModel) && it.Config.poxiaoKey.Equals(childrenConfig.poxiaoKey));
                                        switch (childrenConfig.dataType)
                                        {
                                            case "static":
                                                list.Add(new CodeGenConvIndexListControlOptionDesign()
                                                {
                                                    poxiaoKey = childrenConfig.poxiaoKey,
                                                    Name = string.Format("{0}_{1}", item.VModel, children.VModel),
                                                    DictionaryType = childrenConfig.dataType == "dictionary" ? childrenConfig.dictionaryType : (childrenConfig.dataType == "dynamic" ? childrenConfig.propsUrl : null),
                                                    DataType = childrenConfig.dataType,
                                                    IsStatic = true,
                                                    IsIndex = columnDesign != null ? true : false,
                                                    IsProps = true,
                                                    IsChildren = true,
                                                    Props = children.props?.props?.ToJsonString(CommonConst.options),
                                                    QueryProps = GetQueryPropsModel(children.props?.props).ToJsonString(CommonConst.options),
                                                    Content = GetCodeGenConvIndexListControlOption(string.Format("{0}_{1}", item.VModel, children.VModel), children.options.ToObject<List<Dictionary<string, object>>>())
                                                });
                                                break;
                                            default:
                                                list.Add(new CodeGenConvIndexListControlOptionDesign()
                                                {
                                                    poxiaoKey = childrenConfig.poxiaoKey,
                                                    Name = string.Format("{0}_{1}", item.VModel, children.VModel),
                                                    OptionsName = string.Format("dataForm.{0}[i].{1}", item.VModel, children.VModel),
                                                    DictionaryType = childrenConfig.dataType == "dictionary" ? childrenConfig.dictionaryType : (childrenConfig.dataType == "dynamic" ? childrenConfig.propsUrl : null),
                                                    DataType = childrenConfig.dataType,
                                                    IsStatic = false,
                                                    IsIndex = columnDesign != null ? true : false,
                                                    IsProps = true,
                                                    IsChildren = true,
                                                    Props = children.props?.props?.ToJsonString(CommonConst.options),
                                                    QueryProps = GetQueryPropsModel(children.props?.props).ToJsonString(CommonConst.options),
                                                    Content = string.Format("{0}Options: [],", string.Format("{0}_{1}", item.VModel, children.VModel)),
                                                    IsLinkage = realisticControl.IsLinkage,
                                                    TemplateJson = childrenConfig.dataType == "dynamic" ? childrenConfig.templateJson.ToJsonString() : "[]"
                                                });
                                                break;
                                        }
                                    }

                                    break;
                                case PoxiaoKeyConst.POPUPTABLESELECT:
                                case PoxiaoKeyConst.POPUPSELECT:
                                case PoxiaoKeyConst.AUTOCOMPLETE:
                                    {
                                        var realisticControl = childrenRealisticControls.Find(it => it.VModel.Equals(children.VModel) && it.Config.poxiaoKey.Equals(childrenConfig.poxiaoKey));
                                        list.Add(new CodeGenConvIndexListControlOptionDesign()
                                        {
                                            poxiaoKey = childrenConfig.poxiaoKey,
                                            Name = string.Format("{0}_{1}", item.VModel, children.VModel),
                                            OptionsName = string.Format("dataForm.{0}[i].{1}", item.VModel, children.VModel),
                                            DictionaryType = null,
                                            DataType = null,
                                            IsStatic = true,
                                            IsIndex = columnDesign != null ? true : false,
                                            IsProps = false,
                                            Props = null,
                                            IsChildren = true,
                                            Content = $"{string.Format("{0}_{1}", item.VModel, children.VModel)}Options: {children.columnOptions.ToJsonString(CommonConst.options)},",
                                            IsLinkage = realisticControl.IsLinkage,
                                            TemplateJson = children.templateJson.ToJsonString()
                                        });
                                    }

                                    break;
                                case PoxiaoKeyConst.RELATIONFORM:
                                    {
                                        list.Add(new CodeGenConvIndexListControlOptionDesign()
                                        {
                                            poxiaoKey = childrenConfig.poxiaoKey,
                                            Name = string.Format("{0}_{1}", item.VModel, children.VModel),
                                            DictionaryType = null,
                                            DataType = null,
                                            IsStatic = true,
                                            IsIndex = columnDesign != null ? true : false,
                                            IsProps = false,
                                            Props = null,
                                            IsChildren = true,
                                            Content = $"{string.Format("{0}_{1}", item.VModel, children.VModel)}Options: {children.columnOptions.ToJsonString(CommonConst.options)},"
                                        });
                                    }

                                    break;
                            }
                        }
                    }

                    break;
                case PoxiaoKeyConst.COLLAPSE:
                    {
                        StringBuilder title = new StringBuilder("[");
                        StringBuilder activeList = new StringBuilder("[");
                        foreach (var children in config.children)
                        {
                            title.AppendFormat("{{title:'{0}'}},", children.title);
                            activeList.AppendFormat("'{0}',", children.name);
                            list.AddRange(FormControlProps(children.Config.children, realisticControls, columnDesignModel, type));
                        }

                        title.Remove(title.Length - 1, 1);
                        activeList.Remove(activeList.Length - 1, 1);
                        title.Append("]");
                        activeList.Append("]");
                        list.Add(new CodeGenConvIndexListControlOptionDesign()
                        {
                            poxiaoKey = config.poxiaoKey,
                            Name = "active" + active++,
                            IsStatic = true,
                            IsIndex = false,
                            IsProps = false,
                            IsChildren = false,
                            Content = activeList.ToString(),
                            Title = title.ToString()
                        });
                    }

                    break;
                case PoxiaoKeyConst.TAB:
                    {
                        StringBuilder title = new StringBuilder("[");
                        foreach (var children in config.children)
                        {
                            title.AppendFormat("{{title:'{0}'}},", children.title);
                            list.AddRange(FormControlProps(children.Config.children, realisticControls, columnDesignModel, type));
                        }

                        title.Remove(title.Length - 1, 1);
                        title.Append("]");
                        list.Add(new CodeGenConvIndexListControlOptionDesign()
                        {
                            poxiaoKey = config.poxiaoKey,
                            Name = "active" + active++,
                            IsStatic = true,
                            IsIndex = false,
                            IsProps = false,
                            IsChildren = false,
                            Content = config.active.ToString(),
                            Title = title.ToString()
                        });
                    }

                    break;
                case PoxiaoKeyConst.GROUPTITLE:
                case PoxiaoKeyConst.DIVIDER:
                case PoxiaoKeyConst.PoxiaoTEXT:
                    break;
                case PoxiaoKeyConst.DEPSELECT:
                    if (item.selectType != null && item.selectType == "custom")
                    {
                        list.Add(new CodeGenConvIndexListControlOptionDesign()
                        {
                            poxiaoKey = config.poxiaoKey,
                            IsStatic = true,
                            IsIndex = false,
                            IsProps = false,
                            Content = string.Format("{0}_AbleDepIds:{1},", item.VModel, item.ableDepIds.ToJsonString()),
                        });
                    }
                    break;
                case PoxiaoKeyConst.POSSELECT:
                    if (item.selectType != null && item.selectType == "custom")
                    {
                        list.Add(new CodeGenConvIndexListControlOptionDesign()
                        {
                            poxiaoKey = config.poxiaoKey,
                            IsStatic = true,
                            IsIndex = false,
                            IsProps = false,
                            Content = string.Format("{0}_AblePosIds:{1},", item.VModel, item.ablePosIds.ToJsonString()),
                        });
                    }
                    break;
                case PoxiaoKeyConst.USERSELECT:
                    if (item.selectType != null && item.selectType == "custom")
                    {
                        list.Add(new CodeGenConvIndexListControlOptionDesign()
                        {
                            poxiaoKey = config.poxiaoKey,
                            IsStatic = true,
                            IsIndex = false,
                            IsProps = false,
                            Content = string.Format("{0}_AbleDepIds:{1},", item.VModel, item.ableDepIds.ToJsonString()),
                        });
                    }

                    if (item.selectType != null && item.selectType == "custom")
                    {
                        list.Add(new CodeGenConvIndexListControlOptionDesign()
                        {
                            poxiaoKey = config.poxiaoKey,
                            IsStatic = true,
                            IsIndex = false,
                            IsProps = false,
                            Content = string.Format("{0}_AblePosIds:{1},", item.VModel, item.ablePosIds.ToJsonString()),
                        });
                    }

                    if (item.selectType != null && item.selectType == "custom")
                    {
                        list.Add(new CodeGenConvIndexListControlOptionDesign()
                        {
                            poxiaoKey = config.poxiaoKey,
                            IsStatic = true,
                            IsIndex = false,
                            IsProps = false,
                            Content = string.Format("{0}_AbleUserIds:{1},", item.VModel, item.ableUserIds.ToJsonString()),
                        });
                    }

                    if (item.selectType != null && item.selectType == "custom")
                    {
                        list.Add(new CodeGenConvIndexListControlOptionDesign()
                        {
                            poxiaoKey = config.poxiaoKey,
                            IsStatic = true,
                            IsIndex = false,
                            IsProps = false,
                            Content = string.Format("{0}_AbleRoleIds:{1},", item.VModel, item.ableRoleIds.ToJsonString()),
                        });
                    }

                    if (item.selectType != null && item.selectType == "custom")
                    {
                        list.Add(new CodeGenConvIndexListControlOptionDesign()
                        {
                            poxiaoKey = config.poxiaoKey,
                            IsStatic = true,
                            IsIndex = false,
                            IsProps = false,
                            Content = string.Format("{0}_AbleGroupIds:{1},", item.VModel, item.ableGroupIds.ToJsonString()),
                        });
                    }
                    break;
                case PoxiaoKeyConst.USERSSELECT:
                    if (item.selectType != null && item.selectType == "custom")
                    {
                        list.Add(new CodeGenConvIndexListControlOptionDesign()
                        {
                            poxiaoKey = config.poxiaoKey,
                            IsStatic = true,
                            IsIndex = false,
                            IsProps = false,
                            Content = string.Format("{0}_AbleIds:{1},", item.VModel, item.ableIds.ToJsonString()),
                        });
                    }

                    break;
                default:
                    {
                        switch (config.poxiaoKey)
                        {
                            case PoxiaoKeyConst.POPUPTABLESELECT:
                            case PoxiaoKeyConst.POPUPSELECT:
                            case PoxiaoKeyConst.AUTOCOMPLETE:
                                {
                                    list.Add(new CodeGenConvIndexListControlOptionDesign()
                                    {
                                        poxiaoKey = config.poxiaoKey,
                                        Name = item.VModel,
                                        DictionaryType = null,
                                        DataType = null,
                                        IsStatic = true,
                                        IsIndex = false,
                                        IsProps = false,
                                        Props = null,
                                        IsChildren = false,
                                        Content = string.Format("{0}Options: {1},", item.VModel, item.columnOptions.ToJsonString(CommonConst.options)),
                                        TemplateJson = item.templateJson.ToJsonString()
                                    });
                                }

                                break;
                            case PoxiaoKeyConst.RELATIONFORM:
                                {
                                    list.Add(new CodeGenConvIndexListControlOptionDesign()
                                    {
                                        poxiaoKey = config.poxiaoKey,
                                        Name = item.VModel,
                                        DictionaryType = null,
                                        DataType = null,
                                        IsStatic = true,
                                        IsIndex = false,
                                        IsProps = false,
                                        Props = null,
                                        IsChildren = false,
                                        Content = string.Format("{0}Options: {1},", item.VModel, item.columnOptions.ToJsonString(CommonConst.options))
                                    });
                                }

                                break;
                            case PoxiaoKeyConst.CHECKBOX:
                            case PoxiaoKeyConst.RADIO:
                            case PoxiaoKeyConst.SELECT:
                                {
                                    switch (config.dataType)
                                    {
                                        case "static":
                                            list.Add(new CodeGenConvIndexListControlOptionDesign()
                                            {
                                                poxiaoKey = config.poxiaoKey,
                                                Name = item.VModel,
                                                DictionaryType = config.dataType == "dictionary" ? config.dictionaryType : (config.dataType == "dynamic" ? config.propsUrl : null),
                                                DataType = config.dataType,
                                                IsStatic = true,
                                                IsIndex = true,
                                                IsProps = true,
                                                Props = string.Format("{{'label':'{0}','value':'{1}'}}", item.props?.props?.label, item.props?.props?.value),
                                                QueryProps = GetQueryPropsModel(item.props?.props).ToJsonString(CommonConst.options),
                                                IsChildren = false,
                                                Content = GetCodeGenConvIndexListControlOption(item.VModel, item.options)
                                            });
                                            break;
                                        default:
                                            list.Add(new CodeGenConvIndexListControlOptionDesign()
                                            {
                                                poxiaoKey = config.poxiaoKey,
                                                Name = item.VModel,
                                                DictionaryType = config.dataType == "dictionary" ? config.dictionaryType : (config.dataType == "dynamic" ? config.propsUrl : null),
                                                DataType = config.dataType,
                                                IsStatic = false,
                                                IsIndex = true,
                                                IsProps = true,
                                                QueryProps = GetQueryPropsModel(item.props?.props).ToJsonString(CommonConst.options),
                                                Props = $"{{'label':'{item.props?.props?.label}','value':'{item.props?.props?.value}'}}",
                                                IsChildren = false,
                                                Content = string.Format("{0}Options: [],", item.VModel),
                                                TemplateJson = config.dataType == "dynamic" ? config.templateJson.ToJsonString() : "[]"
                                            });
                                            break;
                                    }
                                }

                                break;
                            case PoxiaoKeyConst.TREESELECT:
                            case PoxiaoKeyConst.CASCADER:
                                {
                                    switch (config.dataType)
                                    {
                                        case "static":
                                            list.Add(new CodeGenConvIndexListControlOptionDesign()
                                            {
                                                poxiaoKey = config.poxiaoKey,
                                                Name = item.VModel,
                                                DictionaryType = config.dataType == "dictionary" ? config.dictionaryType : (config.dataType == "dynamic" ? config.propsUrl : null),
                                                DataType = config.dataType,
                                                IsStatic = true,
                                                IsIndex = true,
                                                IsProps = true,
                                                IsChildren = false,
                                                Props = item.props?.props?.ToJsonString(CommonConst.options),
                                                QueryProps = GetQueryPropsModel(item.props?.props).ToJsonString(CommonConst.options),
                                                Content = GetCodeGenConvIndexListControlOption(item.VModel, item.options.ToObject<List<Dictionary<string, object>>>())
                                            });
                                            break;
                                        default:
                                            list.Add(new CodeGenConvIndexListControlOptionDesign()
                                            {
                                                poxiaoKey = config.poxiaoKey,
                                                Name = item.VModel,
                                                DictionaryType = config.dataType == "dictionary" ? config.dictionaryType : (config.dataType == "dynamic" ? config.propsUrl : null),
                                                DataType = config.dataType,
                                                IsStatic = false,
                                                IsIndex = true,
                                                IsProps = true,
                                                IsChildren = false,
                                                Props = item.props?.props?.ToJsonString(CommonConst.options),
                                                QueryProps = GetQueryPropsModel(item.props.props).ToJsonString(CommonConst.options),
                                                Content = string.Format("{0}Options: [],", item.VModel),
                                                TemplateJson = config.dataType == "dynamic" ? config.templateJson.ToJsonString() : "[]"
                                            });
                                            break;
                                    }
                                }

                                break;
                        }
                    }

                    break;
            }
        }

        return list;
    }

    /// <summary>
    /// 表单真实控件-剔除布局控件后.
    /// </summary>
    /// <param name="fieldList">组件列表</param>
    /// <returns></returns>
    public static List<CodeGenFormRealControlModel> FormRealControl(List<FieldsModel> fieldList)
    {
        var list = new List<CodeGenFormRealControlModel>();
        foreach (var item in fieldList)
        {
            var config = item.Config;
            switch (config.poxiaoKey)
            {
                case PoxiaoKeyConst.TABLE:
                    list.Add(new CodeGenFormRealControlModel
                    {
                        poxiaoKey = config.poxiaoKey,
                        vModel = item.VModel,
                        children = FormRealControl(config.children)
                    });
                    break;
                default:
                    list.Add(new CodeGenFormRealControlModel
                    {
                        poxiaoKey = config.poxiaoKey,
                        vModel = item.VModel,
                        multiple = config.poxiaoKey == PoxiaoKeyConst.CASCADER ? item.props.props.multiple : item.multiple
                    });
                    break;
            }
        }
        return list;
    }

    /// <summary>
    /// 表单脚本设计.
    /// </summary>
    /// <param name="genModel">生成模式.</param>
    /// <param name="fieldList">组件列表.</param>
    /// <param name="tableColumns">表真实字段.</param>
    /// <returns></returns>
    public static List<FormScriptDesignModel> FormScriptDesign(string genModel, List<FieldsModel> fieldList, List<TableColumnConfigModel> tableColumns, List<IndexGridFieldModel> columnDesignModel)
    {
        var formScript = new List<FormScriptDesignModel>();
        foreach (FieldsModel item in fieldList)
        {
            var config = item.Config;
            switch (config.poxiaoKey)
            {
                case PoxiaoKeyConst.TABLE:
                    {
                        var childrenFormScript = new List<FormScriptDesignModel>();
                        foreach (var children in config.children)
                        {
                            var childrenConfig = children.Config;
                            switch (childrenConfig.poxiaoKey)
                            {
                                case PoxiaoKeyConst.RELATIONFORMATTR:
                                case PoxiaoKeyConst.POPUPATTR:
                                    {
                                        if (childrenConfig.isStorage == 2)
                                        {
                                            childrenFormScript.Add(new FormScriptDesignModel()
                                            {
                                                Name = children.VModel,
                                                OriginalName = children.VModel,
                                                poxiaoKey = childrenConfig.poxiaoKey,
                                                DataType = childrenConfig.dataType,
                                                DictionaryType = childrenConfig.dataType == "dictionary" ? childrenConfig.dictionaryType : (childrenConfig.dataType == "dynamic" ? childrenConfig.propsUrl : null),
                                                Format = children.format,
                                                Multiple = childrenConfig.poxiaoKey == PoxiaoKeyConst.CASCADER ? children.props.props.multiple : children.multiple,
                                                BillRule = childrenConfig.rule,
                                                Required = childrenConfig.required,
                                                Placeholder = childrenConfig.label,
                                                Range = children.range,
                                                RegList = childrenConfig.regList,
                                                DefaultValue = childrenConfig.defaultValue?.ToString(),
                                                Trigger = string.IsNullOrEmpty(childrenConfig.trigger?.ToString()) ? "blur" : (childrenConfig.trigger is Array ? childrenConfig.trigger.ToJsonString() : childrenConfig.trigger.ToString()),
                                                ChildrenList = null,
                                                IsSummary = item.showSummary && item.summaryField.Any(it => it.Equals(children.VModel)) ? true : false,
                                                IsLinked = children.IsLinked,
                                                LinkageRelationship = children.linkageReverseRelationship,
                                                IsLinkage = children.IsLinkage
                                            });
                                        }
                                    }
                                    break;
                                case PoxiaoKeyConst.SWITCH:
                                    {
                                        childrenFormScript.Add(new FormScriptDesignModel()
                                        {
                                            Name = children.VModel,
                                            OriginalName = children.VModel,
                                            poxiaoKey = childrenConfig.poxiaoKey,
                                            DataType = childrenConfig.dataType,
                                            DictionaryType = childrenConfig.dataType == "dictionary" ? childrenConfig.dictionaryType : (childrenConfig.dataType == "dynamic" ? childrenConfig.propsUrl : null),
                                            Format = children.format,
                                            Multiple = childrenConfig.poxiaoKey == PoxiaoKeyConst.CASCADER ? children.props.props.multiple : children.multiple,
                                            BillRule = childrenConfig.rule,
                                            Required = childrenConfig.required,
                                            Placeholder = childrenConfig.label,
                                            Range = children.range,
                                            RegList = childrenConfig.regList,
                                            DefaultValue = childrenConfig.defaultValue.ParseToBool(),
                                            Trigger = string.IsNullOrEmpty(childrenConfig.trigger?.ToString()) ? "blur" : (childrenConfig.trigger is Array ? childrenConfig.trigger.ToJsonString() : childrenConfig.trigger.ToString()),
                                            ChildrenList = null,
                                            IsSummary = item.showSummary && item.summaryField.Find(it => it.Equals(children.VModel)) != null ? true : false,
                                            IsLinked = item.IsLinked,
                                            LinkageRelationship = item.linkageReverseRelationship
                                        });
                                    }

                                    break;
                                default:
                                    {
                                        childrenFormScript.Add(new FormScriptDesignModel()
                                        {
                                            Name = children.VModel,
                                            OriginalName = children.VModel,
                                            poxiaoKey = childrenConfig.poxiaoKey,
                                            DataType = childrenConfig.dataType,
                                            DictionaryType = childrenConfig.dataType == "dictionary" ? childrenConfig.dictionaryType : (childrenConfig.dataType == "dynamic" ? childrenConfig.propsUrl : null),
                                            Format = children.format,
                                            Multiple = childrenConfig.poxiaoKey == PoxiaoKeyConst.CASCADER ? children.props.props.multiple : children.multiple,
                                            BillRule = childrenConfig.rule,
                                            Required = childrenConfig.required,
                                            Placeholder = childrenConfig.label,
                                            Range = children.range,
                                            RegList = childrenConfig.regList,
                                            DefaultValue = childrenConfig.defaultValue?.ToString(),
                                            Trigger = string.IsNullOrEmpty(childrenConfig.trigger?.ToString()) ? "blur" : (childrenConfig.trigger is Array ? childrenConfig.trigger.ToJsonString() : childrenConfig.trigger.ToString()),
                                            ChildrenList = null,
                                            IsSummary = item.showSummary && item.summaryField.Any(it => it.Equals(children.VModel)) ? true : false,
                                            IsLinked = children.IsLinked,
                                            LinkageRelationship = children.linkageReverseRelationship,
                                            IsLinkage = children.IsLinkage,
                                            Thousands = children.thousands,
                                        });
                                    }

                                    break;
                            }
                        }
                        List<RegListModel> childrenRegList = new List<RegListModel>();

                        foreach (var reg in childrenFormScript.FindAll(it => it.RegList != null && it.RegList.Count > 0).Select(it => it.RegList))
                        {
                            childrenRegList.AddRange(reg);
                        }

                        formScript.Add(new FormScriptDesignModel()
                        {
                            Name = config.tableName.ParseToPascalCase(),
                            Placeholder = config.label,
                            OriginalName = item.VModel,
                            poxiaoKey = config.poxiaoKey,
                            ChildrenList = childrenFormScript,
                            Required = childrenFormScript.Any(it => it.Required.Equals(true)),
                            RegList = childrenRegList,
                            ShowSummary = item.showSummary,
                            SummaryField = item.summaryField.ToJsonString(),
                            IsDataTransfer = item.addType == 1 ? true : false,
                            AddTableConf = item.addTableConf.ToJsonString(),
                            AddType = item.addType,
                            IsLinked = childrenFormScript.Any(it => it.IsLinked.Equals(true)),
                            Thousands = childrenFormScript.Any(it => it.Thousands.Equals(true)),
                            ChildrenThousandsField = childrenFormScript.FindAll(it => it.Thousands.Equals(true)).Select(it => it.Name).ToList().ToJsonString(),
                        });
                    }

                    break;
                case PoxiaoKeyConst.RELATIONFORMATTR:
                case PoxiaoKeyConst.POPUPATTR:
                    {
                        if (config.isStorage == 2)
                        {
                            var originalName = string.Empty;
                            if (item.VModel.Contains("_poxiao_"))
                            {
                                var auxiliaryTableName = item.VModel.Matches(@"poxiao_(?<table>[\s\S]*?)_poxiao_", "table").Last();
                                var column = item.VModel.Replace(item.VModel.Matches(@"poxiao_(?<table>[\s\S]*?)_poxiao_").Last(), string.Empty);
                                var columns = tableColumns.Find(it => it.LowerColumnName.Equals(column) && it.IsAuxiliary.Equals(true) && (bool)it.TableName?.Equals(auxiliaryTableName));
                                if (columns != null)
                                    originalName = columns.OriginalColumnName;
                            }
                            else
                            {
                                var columns = tableColumns.Find(it => it.LowerColumnName.Equals(item.VModel));
                                if (columns != null)
                                    originalName = columns.OriginalColumnName;
                            }

                            formScript.Add(new FormScriptDesignModel()
                            {
                                IsInlineEditor = columnDesignModel != null ? columnDesignModel.Any(it => it.VModel == item.VModel) : false,
                                Name = item.VModel,
                                OriginalName = originalName,
                                poxiaoKey = config.poxiaoKey,
                                DataType = config.dataType,
                                DictionaryType = config.dataType == "dictionary" ? config.dictionaryType : (config.dataType == "dynamic" ? config.propsUrl : null),
                                Format = item.format,
                                Multiple = config.poxiaoKey == PoxiaoKeyConst.CASCADER ? item.props.props.multiple : item.multiple,
                                BillRule = config.rule,
                                Required = config.required,
                                Placeholder = config.label,
                                Range = item.range,
                                RegList = config.regList,
                                DefaultValue = config.defaultValue?.ToString(),
                                Trigger = !string.IsNullOrEmpty(config.trigger?.ToString()) ? (config?.trigger is Array ? config?.trigger?.ToJsonString() : config?.trigger?.ToString()) : "blur",
                                ChildrenList = null,
                                IsLinked = item.IsLinked,
                                LinkageRelationship = item.linkageReverseRelationship,
                                IsLinkage = item.IsLinkage,
                            });
                        }
                    }
                    break;
                case PoxiaoKeyConst.SWITCH:
                    {
                        var originalName = string.Empty;
                        if (item.VModel.Contains("_poxiao_"))
                        {
                            var auxiliaryTableName = item.VModel.Matches(@"poxiao_(?<table>[\s\S]*?)_poxiao_", "table").Last();
                            var column = item.VModel.Replace(item.VModel.Matches(@"poxiao_(?<table>[\s\S]*?)_poxiao_").Last(), string.Empty);
                            var columns = tableColumns.Find(it => it.LowerColumnName.Equals(column) && (bool)it.TableName?.Equals(auxiliaryTableName) && it.IsAuxiliary.Equals(true));
                            if (columns != null)
                                originalName = columns.OriginalColumnName;
                        }
                        else
                        {
                            var columns = tableColumns.Find(it => it.LowerColumnName.Equals(item.VModel));
                            if (columns != null)
                                originalName = columns.OriginalColumnName;
                        }

                        formScript.Add(new FormScriptDesignModel()
                        {
                            IsInlineEditor = columnDesignModel != null ? columnDesignModel.Any(it => it.VModel == item.VModel) : false,
                            Name = item.VModel,
                            OriginalName = originalName,
                            poxiaoKey = config.poxiaoKey,
                            DataType = config.dataType,
                            DictionaryType = config.dataType == "dictionary" ? config.dictionaryType : (config.dataType == "dynamic" ? config.propsUrl : null),
                            Format = item.format,
                            Multiple = item.multiple,
                            BillRule = config.rule,
                            Required = config.required,
                            Placeholder = config.label,
                            Range = item.range,
                            RegList = config.regList,
                            DefaultValue = config.defaultValue.ParseToBool(),
                            Trigger = string.IsNullOrEmpty(config.trigger?.ToString()) ? "blur" : (config.trigger is Array ? config.trigger.ToJsonString() : config.trigger.ToString()),
                            ChildrenList = null,
                            IsLinked = item.IsLinked,
                            LinkageRelationship = item.linkageReverseRelationship
                        });
                    }

                    break;
                default:
                    {
                        string originalName = string.Empty;
                        if (item.VModel.Contains("_poxiao_"))
                        {
                            var auxiliaryTableName = item.VModel.Matches(@"poxiao_(?<table>[\s\S]*?)_poxiao_", "table").Last();
                            var column = item.VModel.Replace(item.VModel.Matches(@"poxiao_(?<table>[\s\S]*?)_poxiao_").Last(), string.Empty);
                            var columns = tableColumns.Find(it => it.LowerColumnName.Equals(column) && it.IsAuxiliary.Equals(true) && (bool)it.TableName?.Equals(auxiliaryTableName));
                            if (columns != null)
                                originalName = columns.OriginalColumnName;
                        }
                        else
                        {
                            var columns = tableColumns.Find(it => it.LowerColumnName.Equals(item.VModel));
                            if (columns != null)
                                originalName = columns.OriginalColumnName;
                        }

                        formScript.Add(new FormScriptDesignModel()
                        {
                            IsInlineEditor = columnDesignModel != null ? columnDesignModel.Any(it => it.VModel == item.VModel) : false,
                            Name = item.VModel,
                            OriginalName = originalName,
                            poxiaoKey = config.poxiaoKey,
                            DataType = config.dataType,
                            DictionaryType = config.dataType == "dictionary" ? config.dictionaryType : (config.dataType == "dynamic" ? config.propsUrl : null),
                            Format = item.format,
                            Multiple = config.poxiaoKey == PoxiaoKeyConst.CASCADER ? item.props.props.multiple : item.multiple,
                            BillRule = config.rule,
                            Required = config.required,
                            Placeholder = config.label,
                            Range = item.range,
                            RegList = config.regList,
                            DefaultValue = config.defaultValue?.ToString(),
                            Trigger = !string.IsNullOrEmpty(config.trigger?.ToString()) ? (config?.trigger is Array ? config?.trigger?.ToJsonString() : config?.trigger?.ToString()) : "blur",
                            ChildrenList = null,
                            IsLinked = item.IsLinked,
                            LinkageRelationship = item.linkageReverseRelationship,
                            IsLinkage = item.IsLinkage,
                            Thousands = item.thousands,
                        });
                    }

                    break;
            }
        }

        return formScript;
    }

    /// <summary>
    /// 获取常规index列表控件Option.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    private static string GetCodeGenConvIndexListControlOption(string name, List<Dictionary<string, object>> options)
    {
        StringBuilder sb = new StringBuilder();
        if (options != null)
        {
            sb.AppendFormat("{0}Options:", name);
            sb.Append("[");
            foreach (var valueItem in options?.ToObject<List<Dictionary<string, object>>>())
            {
                sb.Append("{");
                foreach (var items in valueItem)
                {
                    sb.AppendFormat("'{0}':{1},", items.Key, items.Value.ToJsonString());
                }

                sb = new StringBuilder(sb.ToString().TrimEnd(','));
                sb.Append("},");
            }

            sb = new StringBuilder(sb.ToString().TrimEnd(','));
            sb.Append("],");
        }

        return sb.ToString();
    }

    /// <summary>
    /// 查询时将多选关闭.
    /// </summary>
    /// <param name="propsModel"></param>
    /// <returns></returns>
    private static PropsBeanModel GetQueryPropsModel(PropsBeanModel propsModel)
    {
        var model = new PropsBeanModel();
        if (propsModel != null && propsModel.multiple)
        {
            model = propsModel;
            model.multiple = false;
        }
        else if (propsModel != null)
        {
            model = propsModel;
        }

        return model;
    }

    /// <summary>
    /// 获取用户控件联动属性.
    /// </summary>
    /// <param name="field">联动控件.</param>
    /// <param name="fieldList">当前控件集合.</param>
    /// <param name="logic">4-PC,5-App.</param>
    /// <returns></returns>
    private static string GetUserRelationAttr(FieldsModel field, List<FieldsModel> fieldList, int logic)
    {
        var res = string.Empty;

        // 用户控件联动
        if (field.Config.poxiaoKey.Equals(PoxiaoKeyConst.USERSELECT) && field.relationField.IsNotEmptyOrNull())
        {
            var relationField = fieldList.Find(x => x.VModel.Equals(field.relationField));
            if (relationField == null && field.relationField.ToLower().Contains("tablefield") && fieldList.Any(x => x.Config.poxiaoKey.Equals(PoxiaoKeyConst.TABLE)))
            {
                var ctFieldList = fieldList.Find(x => x.VModel.Equals(field.relationField.Split("-").FirstOrDefault()));
                if (ctFieldList != null && ctFieldList.Config.children != null)
                {
                    relationField = ctFieldList.Config.children.Find(x => x.VModel.Equals(field.relationField.Split("-").LastOrDefault()));
                    field.relationField = logic == 4 ? field.relationField.Replace("-", "[scope.$index].") : field.relationField.Replace("-", "[i].");
                }
            }

            if (relationField != null) res = string.Format(" :ableRelationIds=\"dataForm.{0}\" ", field.relationField);
        }

        return res;
    }

    /// <summary>
    /// 表单json.
    /// </summary>
    /// <param name="formScriptDesignModels"></param>
    /// <returns></returns>
    public static string GetPropertyJson(List<FormScriptDesignModel> formScriptDesignModels)
    {
        List<CodeGenExportPropertyJsonModel>? list = new List<CodeGenExportPropertyJsonModel>();
        foreach (var item in formScriptDesignModels)
        {
            switch (item.poxiaoKey)
            {
                case PoxiaoKeyConst.TABLE:
                    list.Add(new CodeGenExportPropertyJsonModel
                    {
                        filedName = item.Placeholder,
                        poxiaoKey = item.poxiaoKey,
                        filedId = item.OriginalName,
                        required = item.Required,
                        multiple = item.Multiple,
                    });
                    foreach (var subtable in item.ChildrenList)
                    {
                        list.Add(new CodeGenExportPropertyJsonModel
                        {
                            filedName = string.Format("{0}-{1}", item.Placeholder, subtable.Placeholder),
                            poxiaoKey = subtable.poxiaoKey,
                            filedId = string.Format("{0}-{1}", item.OriginalName, subtable.LowerName),
                            required = subtable.Required,
                            multiple = subtable.Multiple,
                        });
                    }
                    break;
                default:
                    list.Add(new CodeGenExportPropertyJsonModel
                    {
                        filedName = item.Placeholder,
                        poxiaoKey = item.poxiaoKey,
                        filedId = item.LowerName,
                        required = item.Required,
                        multiple = item.Multiple,
                    });
                    break;
            }
        }
        return list.ToJsonString();
    }
}