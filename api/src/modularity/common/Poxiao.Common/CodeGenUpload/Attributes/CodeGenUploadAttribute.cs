using Poxiao.Infrastructure.Models;

namespace Poxiao.Infrastructure.CodeGenUpload;

/// <summary>
/// 代码生成导入.
/// </summary>
[SuppressSniffer]
[AttributeUsage(AttributeTargets.Property)]
public class CodeGenUploadAttribute : Attribute
{
    /// <summary>
    /// 构造函数.
    /// "createUser"
    /// "modifyUser"
    /// "createTime"
    /// "modifyTime"
    /// "currOrganize"
    /// "currPosition"
    /// "currDept"
    /// "billRule"
    /// "comInput"
    /// "textarea"
    /// "colorPicker"
    /// "rate"
    /// "slider"
    /// "editor".
    /// </summary>
    public CodeGenUploadAttribute(string Model, string Config)
    {
        this.Model = Model;
        this.Config = Config.ToObject<CodeGenConfigModel>();
    }

    /// <summary>
    /// 构造函数
    /// "time"
    /// "date".
    /// </summary>
    public CodeGenUploadAttribute(string Model, string SecondParameter, string Config)
    {
        this.Model = Model;
        this.Config = Config.ToObject<CodeGenConfigModel>();
        switch (this.Config.poxiaoKey)
        {
            case PoxiaoKeyConst.CHECKBOX:
            case PoxiaoKeyConst.RADIO:
                options = SecondParameter?.ToObject<List<Dictionary<string, object>>>();
                break;
            default:
                format = SecondParameter;
                break;
        }
    }

    /// <summary>
    /// 构造函数
    /// "numInput".
    /// </summary>
    public CodeGenUploadAttribute(string Model, string Config, int Min = default, int Max = default)
    {
        this.Model = Model;
        min = Min;
        max = Max;
        this.Config = Config.ToObject<CodeGenConfigModel>();
    }

    /// <summary>
    /// 构造函数
    /// "radio"
    /// "checkbox"
    /// "switch".
    /// </summary>
    public CodeGenUploadAttribute(string Model, string ActiveTxt, string InactiveTxt, string Config)
    {
        this.Model = Model;
        this.Config = Config.ToObject<CodeGenConfigModel>();
        switch (this.Config.poxiaoKey)
        {
            case PoxiaoKeyConst.CHECKBOX:
            case PoxiaoKeyConst.RADIO:
                props = ActiveTxt?.ToObject<CodeGenPropsModel>();
                options = InactiveTxt?.ToObject<List<Dictionary<string, object>>>();
                break;
            case PoxiaoKeyConst.SWITCH:
                activeTxt = ActiveTxt;
                inactiveTxt = InactiveTxt;
                break;
        }
    }

    /// <summary>
    /// 构造函数
    /// "popupSelect".
    /// </summary>
    public CodeGenUploadAttribute(string Model, string dataConversionModel, string SecondParameter, string ThreeParameters, string FourParameters, string ShowField, string Config)
    {
        this.Model = Model;
        VModel = dataConversionModel;
        interfaceId = SecondParameter;
        propsValue = ThreeParameters;
        relationField = FourParameters;
        showField = ShowField;
        this.Config = Config.ToObject<CodeGenConfigModel>();
    }

    /// <summary>
    /// 构造函数
    /// "comSelect":
    /// "roleSelect":
    /// "groupSelect".
    /// </summary>
    public CodeGenUploadAttribute(string Model, bool Multiple, string Config)
    {
        this.Model = Model;
        multiple = Multiple;
        this.Config = Config.ToObject<CodeGenConfigModel>();
    }

    /// <summary>
    /// 构造函数
    /// "address".
    /// </summary>
    public CodeGenUploadAttribute(string Model, bool Multiple, int Level, string Config)
    {
        this.Model = Model;
        multiple = Multiple;
        level = Level;
        this.Config = Config.ToObject<CodeGenConfigModel>();
    }

    /// <summary>
    /// 构造函数
    /// "treeSelect"
    /// "depSelect":
    /// "select".
    /// </summary>
    public CodeGenUploadAttribute(string Model, bool Multiple, string ThreeParameters, string FourParameters, string Config)
    {
        this.Model = Model;
        multiple = Multiple;
        this.Config = Config.ToObject<CodeGenConfigModel>();
        switch (this.Config.poxiaoKey)
        {
            case PoxiaoKeyConst.DEPSELECT:
                selectType = ThreeParameters;
                ableDepIds = FourParameters?.ToObject<List<string>>();
                break;
            default:
                props = ThreeParameters?.ToObject<CodeGenPropsModel>();
                options = FourParameters?.ToObject<List<Dictionary<string, object>>>();
                break;
        }
    }

    /// <summary>
    /// 构造函数
    /// "usersSelect":.
    /// </summary>
    public CodeGenUploadAttribute(string Model, string dataConversionModel, bool Multiple, string ThreeParameters, string FourParameters, string Config)
    {
        this.Model = Model;
        VModel = dataConversionModel;
        multiple = Multiple;
        this.Config = Config.ToObject<CodeGenConfigModel>();
        selectType = ThreeParameters;
        ableIds = FourParameters?.ToObject<List<string>>();
    }

    /// <summary>
    /// 构造函数
    /// "cascader"
    /// "posSelect":
    /// "relationForm"
    /// "popupTableSelect".
    /// </summary>
    public CodeGenUploadAttribute(string Model, bool Multiple, string InterfaceId, string PropsValue, string RelationField, string Config)
    {
        this.Model = Model;
        multiple = Multiple;
        this.Config = Config.ToObject<CodeGenConfigModel>();
        switch (this.Config.poxiaoKey)
        {
            case PoxiaoKeyConst.CASCADER:
                separator = InterfaceId;
                props = PropsValue?.ToObject<CodeGenPropsModel>();
                options = RelationField?.ToObject<List<Dictionary<string, object>>>();
                break;
            case PoxiaoKeyConst.POPUPTABLESELECT:
                interfaceId = InterfaceId;
                propsValue = PropsValue;
                relationField = RelationField;
                break;
            case PoxiaoKeyConst.POSSELECT:
                selectType = InterfaceId;
                ableDepIds = PropsValue?.ToObject<List<string>>();
                ablePosIds = RelationField?.ToObject<List<string>>();
                break;
        }
    }

    /// <summary>
    /// 构造函数
    /// "relationForm".
    /// </summary>
    public CodeGenUploadAttribute(string Model, string dataConversionModel, bool Multiple, string InterfaceId, string PropsValue, string RelationField, string Config)
    {
        this.Model = Model;
        VModel = dataConversionModel;
        multiple = Multiple;
        this.Config = Config.ToObject<CodeGenConfigModel>();
        modelId = InterfaceId;
        relationField = PropsValue;
        showField = RelationField;
    }

    /// <summary>
    /// 构造函数
    /// "userSelect":.
    /// </summary>
    public CodeGenUploadAttribute(string Model, bool Multiple, string SelectType, string AbleDepIds, string AblePosIds, string AbleUserIds, string AbleRoleIds, string AbleGroupIds, string Config)
    {
        this.Model = Model;
        multiple = Multiple;
        selectType = SelectType;
        ableDepIds = AbleDepIds?.ToObject<List<string>>();
        ablePosIds = AblePosIds?.ToObject<List<string>>();
        ableUserIds = AbleUserIds?.ToObject<List<string>>();
        ableRoleIds = AbleRoleIds?.ToObject<List<string>>();
        ableGroupIds = AbleGroupIds?.ToObject<List<string>>();
        this.Config = Config.ToObject<CodeGenConfigModel>();
    }

    /// <summary>
    /// 设置默认值为空字符串.
    /// </summary>
    public string Model { get; set; }

    /// <summary>
    /// 数据转换.
    /// </summary>
    public string VModel { get; set; }

    /// <summary>
    /// 最小值.
    /// </summary>
    public int min { get; set; }

    /// <summary>
    /// 最大值.
    /// </summary>
    public int max { get; set; }

    /// <summary>
    /// 开关控件 属性 - 开启展示值.
    /// </summary>
    public string? activeTxt { get; set; }

    /// <summary>
    /// 开关控件 属性 - 关闭展示值.
    /// </summary>
    public string? inactiveTxt { get; set; }

    /// <summary>
    /// 显示绑定值的格式.
    /// </summary>
    public string? format { get; set; }

    /// <summary>
    /// 是否多选.
    /// </summary>
    public bool multiple { get; set; }

    /// <summary>
    /// 选项分隔符.
    /// </summary>
    public string? separator { get; set; }

    /// <summary>
    /// 配置选项.
    /// </summary>
    public CodeGenPropsModel? props { get; set; }

    /// <summary>
    /// 配置项.
    /// </summary>
    public List<Dictionary<string, object>>? options { get; set; }

    /// <summary>
    /// 弹窗选择主键.
    /// </summary>
    public string? propsValue { get; set; }

    /// <summary>
    /// 关联表单字段.
    /// </summary>
    public string? relationField { get; set; }

    /// <summary>
    /// 关联表单id.
    /// </summary>
    public string? modelId { get; set; }

    /// <summary>
    /// 数据接口ID.
    /// </summary>
    public string? interfaceId { get; set; }

    /// <summary>
    /// 层级.
    /// </summary>
    public int level { get; set; }

    /// <summary>
    /// 配置.
    /// </summary>
    public CodeGenConfigModel? Config { get; set; }

    /// <summary>
    /// 可选范围.
    /// </summary>
    public string selectType { get; set; }

    /// <summary>
    /// 可选部门.
    /// </summary>
    public List<string> ableDepIds { get; set; }

    /// <summary>
    /// 可选岗位.
    /// </summary>
    public List<string> ablePosIds { get; set; }

    /// <summary>
    /// 可选用户.
    /// </summary>
    public List<string> ableUserIds { get; set; }

    /// <summary>
    /// 可选角色.
    /// </summary>
    public List<string> ableRoleIds { get; set; }

    /// <summary>
    /// 可选分组.
    /// </summary>
    public List<string> ableGroupIds { get; set; }

    /// <summary>
    /// 新用户选择控件.
    /// </summary>
    public List<string> ableIds { get; set; }

    /// <summary>
    /// 展示字段.
    /// </summary>
    public string showField { get; set; }
}