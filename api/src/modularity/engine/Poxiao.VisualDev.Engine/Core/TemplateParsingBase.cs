using Poxiao.Infrastructure.Const;
using Poxiao.Infrastructure.Extension;
using Poxiao.Infrastructure.Security;
using Poxiao.Systems.Entitys.System;
using Poxiao.VisualDev.Engine.Model;
using Poxiao.VisualDev.Entitys;
using SqlSugar;

namespace Poxiao.VisualDev.Engine.Core;

/// <summary>
/// 模板解析 基础类.
/// </summary>
public class TemplateParsingBase
{
    public TemplateParsingBase()
    {
    }

    /// <summary>
    /// 模板实体.
    /// </summary>
    public VisualDevEntity visualDevEntity { get; set; }

    /// <summary>
    /// 页面类型 （1、纯表单，2、表单加列表，3、表单列表工作流）.
    /// </summary>
    public int WebType { get; set; }

    /// <summary>
    /// 是否有表 (true 有表, false 无表).
    /// </summary>
    public bool IsHasTable { get; set; }

    /// <summary>
    /// 表单配置JSON模型.
    /// </summary>
    public FormDataModel? FormModel { get; set; }

    /// <summary>
    /// 列配置JSON模型.
    /// </summary>
    public ColumnDesignModel ColumnData { get; set; }

    /// <summary>
    /// App列配置JSON模型.
    /// </summary>
    public ColumnDesignModel AppColumnData { get; set; }

    /// <summary>
    /// 所有控件集合.
    /// </summary>
    public List<FieldsModel> AllFieldsModel { get; set; }

    /// <summary>
    /// 所有控件集合(已剔除布局控件).
    /// </summary>
    public List<FieldsModel> FieldsModelList { get; set; }

    /// <summary>
    /// 主表控件集合.
    /// </summary>
    public List<FieldsModel> MainTableFieldsModelList { get; set; }

    /// <summary>
    /// 副表控件集合.
    /// </summary>
    public List<FieldsModel> AuxiliaryTableFieldsModelList { get; set; }

    /// <summary>
    /// 子表控件集合.
    /// </summary>
    public List<FieldsModel> ChildTableFieldsModelList { get; set; }

    /// <summary>
    /// 主/副表控件集合(列表展示数据控件).
    /// </summary>
    public List<FieldsModel> SingleFormData { get; set; }

    /// <summary>
    /// 所有表.
    /// </summary>
    public List<TableModel> AllTable { get; set; }

    /// <summary>
    /// 主表.
    /// </summary>
    public TableModel? MainTable { get; set; }

    /// <summary>
    /// 主表 表名.
    /// </summary>
    public string? MainTableName { get; set; }

    /// <summary>
    /// 主/副表 系统生成控件集合.
    /// </summary>
    public List<FieldsModel> GenerateFields { get; set; }

    /// <summary>
    /// 主表 vModel 字段 字典.
    /// Key : vModel , Value : 主表.vModel.
    /// </summary>
    public Dictionary<string, string> MainTableFields { get; set; }

    /// <summary>
    /// 副表 vModel 字段 字典.
    /// Key : vModel , Value : 副表.vModel.
    /// </summary>
    public Dictionary<string, string> AuxiliaryTableFields { get; set; }

    /// <summary>
    /// 子表 vModel 字段 字典.
    /// Key : 设计子表-vModel , Value : 子表.vModel.
    /// </summary>
    public Dictionary<string, string> ChildTableFields { get; set; }

    /// <summary>
    /// 所有表 vModel 字段 字典.
    /// Key : 设计子表-vModel , Value : 表.vModel.
    /// </summary>
    public Dictionary<string, string> AllTableFields { get; set; }

    /// <summary>
    /// 功能名称.
    /// </summary>
    public string FullName { get; set; }

    /// <summary>
    /// 主表主键名.
    /// </summary>
    public string MainPrimary { get; set; }

    /// <summary>
    /// 数据库连接.
    /// </summary>
    public DbLinkEntity DbLink { get; set; }

    /// <summary>
    /// 导入模式.(1 仅新增，2 更新和新增数据).
    /// </summary>
    public string dataType { get; set; } = "1";

    /// <summary>
    /// 导入数据列表.
    /// </summary>
    public List<string> selectKey { get; set; }

    /// <summary>
    /// PC数据过滤 .
    /// </summary>
    public List<IConditionalModel> DataRuleListJson { get; set; }

    /// <summary>
    /// App数据过滤 .
    /// </summary>
    public List<IConditionalModel> AppDataRuleListJson { get; set; }

    /// <summary>
    /// 模板解析帮助 构造 (功能表单).
    /// </summary>
    /// <param name="formJson">表单Json.</param>
    /// <param name="tables">涉及表Json.</param>
    /// <param name="isFlowTask"></param>
    public TemplateParsingBase(string formJson, string tables, bool isFlowTask = false)
    {
        InitByFormType(formJson, tables, 2);
    }

    /// <summary>
    /// 模板解析帮助 构造.
    /// </summary>
    /// <param name="formJson">表单Json.</param>
    /// <param name="tables">涉及表Json.</param>
    /// <param name="formType">表单类型（1：系统表单 2：自定义表单）.</param>
    public TemplateParsingBase(string formJson, string tables, int formType)
    {
        InitByFormType(formJson, tables, formType);
    }

    /// <summary>
    /// 模板解析帮助 构造.
    /// </summary>
    /// <param name="entity">功能实体</param>
    /// <param name="isFlowTask"></param>
    public TemplateParsingBase(VisualDevEntity entity, bool isFlowTask = false)
    {
        visualDevEntity = entity;
        WebType = entity.WebType;
        if (entity.FlowId.IsNotEmptyOrNull() && entity.EnableFlow.Equals(1)) WebType = 3;

        // 数据视图
        if (entity.WebType.Equals(4))
        {
            FullName = entity.FullName;
            IsHasTable = false;
            InitColumnData(entity);
            AllFieldsModel = new List<FieldsModel>();
            ColumnData.columnList.ForEach(item =>
            {
                AllFieldsModel.Add(new FieldsModel() { VModel = item.VModel, Config = new ConfigModel() { label = item.label, poxiaoKey = item.poxiaoKey } });
            });
            AppColumnData.columnList.ForEach(item =>
            {
                AllFieldsModel.Add(new FieldsModel() { VModel = item.VModel, Config = new ConfigModel() { label = item.label, poxiaoKey = item.poxiaoKey } });
            });
            AllFieldsModel = AllFieldsModel.DistinctBy(x => x.VModel).ToList();
            FieldsModelList = AllFieldsModel;
            AuxiliaryTableFieldsModelList = AllFieldsModel;
            MainTableFieldsModelList = AllFieldsModel;
            SingleFormData = AllFieldsModel;
        }
        else
        {
            FormDataModel formModel = entity.FormData.ToObjectOld<FormDataModel>();
            FormModel = formModel; // 表单Json模型
            IsHasTable = !string.IsNullOrEmpty(entity.Tables) && !"[]".Equals(entity.Tables); // 是否有表
            AllFieldsModel = TemplateAnalysis.AnalysisTemplateData(formModel.fields.ToJsonString().ToObjectOld<List<FieldsModel>>()); // 所有控件集合
            FieldsModelList = TemplateAnalysis.AnalysisTemplateData(formModel.fields); // 已剔除布局控件集合
            MainTable = entity.Tables.ToList<TableModel>().Find(m => m.typeId.Equals("1")); // 主表
            MainTableName = MainTable?.table; // 主表名称
            AddChlidTableFeildsModel();

            // 处理旧控件 部分没有 tableName
            FieldsModelList.Where(x => string.IsNullOrWhiteSpace(x.Config.tableName)).ToList().ForEach(item =>
            {
                if (item.VModel.Contains("_poxiao_")) item.Config.tableName = item.VModel.ReplaceRegex(@"_poxiao_(\w+)", string.Empty).Replace("poxiao_", string.Empty); // 副表
                else item.Config.tableName = MainTableName != null ? MainTableName : string.Empty; // 主表
            });
            AllTable = entity.Tables.ToObject<List<TableModel>>(); // 所有表
            AuxiliaryTableFieldsModelList = FieldsModelList.Where(x => x.VModel.Contains("_poxiao_")).ToList(); // 单控件副表集合
            ChildTableFieldsModelList = FieldsModelList.Where(x => x.Config.poxiaoKey == PoxiaoKeyConst.TABLE).ToList(); // 子表集合
            MainTableFieldsModelList = FieldsModelList.Except(AuxiliaryTableFieldsModelList).Except(ChildTableFieldsModelList).ToList(); // 主表控件集合
            SingleFormData = FieldsModelList.Where(x => x.Config.poxiaoKey != PoxiaoKeyConst.TABLE).ToList(); // 非子表集合
            GenerateFields = GetGenerateFields(); // 系统生成控件

            MainTableFields = new Dictionary<string, string>();
            AuxiliaryTableFields = new Dictionary<string, string>();
            ChildTableFields = new Dictionary<string, string>();
            AllTableFields = new Dictionary<string, string>();
            MainTableFieldsModelList.Where(x => x.VModel.IsNotEmptyOrNull()).ToList().ForEach(x =>
            {
                MainTableFields.Add(x.VModel, x.Config.tableName + "." + x.VModel);
                AllTableFields.Add(x.VModel, x.Config.tableName + "." + x.VModel);
            });
            AuxiliaryTableFieldsModelList.Where(x => x.VModel.IsNotEmptyOrNull()).ToList().ForEach(x =>
            {
                AuxiliaryTableFields.Add(x.VModel, x.VModel.Replace("_poxiao_", ".").Replace("poxiao_", string.Empty));
                AllTableFields.Add(x.VModel, x.VModel.Replace("_poxiao_", ".").Replace("poxiao_", string.Empty));
            });
            ChildTableFieldsModelList.ForEach(item =>
            {
                item.Config.children.Where(x => x.VModel.IsNotEmptyOrNull()).ToList().ForEach(x =>
                {
                    ChildTableFields.Add(item.VModel + "-" + x.VModel, item.Config.tableName + "." + x.VModel);
                    AllTableFields.Add(item.VModel + "-" + x.VModel, item.Config.tableName + "." + x.VModel);
                });
            });
            InitColumnData(entity);
        }
    }

    /// <summary>
    /// 模板解析帮助 构造(代码生成用).
    /// </summary>
    /// <param name="dblink">数据连接.</param>
    /// <param name="fieldList">控件集合.</param>
    /// <param name="tables">主/副/子 表.</param>
    /// <param name="mainPrimary">主表主键.</param>
    /// <param name="webType">页面类型 （1、纯表单，2、表单加列表，3、表单列表工作流）.</param>
    /// <param name="primaryKeyPolicy">主键策略(1 雪花ID 2 自增长ID).</param>
    /// <param name="uploaderKey">导入导出数据列名集合.</param>
    /// <param name="DataType">导入类型 1 新增, 2 新增和修改.</param>
    /// <param name="enableFlow">是否开启流程 1 开启.</param>
    /// <param name="flowFormId">流程表单Id.</param>
    public TemplateParsingBase(
        DbLinkEntity dblink,
        List<FieldsModel> fieldList,
        List<DbTableRelationModel> tables,
        string mainPrimary,
        int webType,
        int primaryKeyPolicy,
        List<string> uploaderKey,
        string DataType,
        int enableFlow = 0,
        string flowFormId = "")
    {
        if (enableFlow.Equals(1)) visualDevEntity = new VisualDevEntity() { EnableFlow = 1, Id = flowFormId };
        DbLink = dblink;
        AllTable = tables.ToObject<List<TableModel>>(); // 所有表
        FieldsModelList = fieldList;
        AllFieldsModel = FieldsModelList.Copy();
        MainTable = AllTable.Find(m => m.typeId.Equals("1")); // 主表
        MainTableName = MainTable?.table; // 主表名称
        MainPrimary = mainPrimary;
        AddCodeGenChlidTableFeildsModel();

        // 处理旧控件 部分没有 tableName
        FieldsModelList.Where(x => string.IsNullOrWhiteSpace(x.Config.tableName)).ToList().ForEach(item =>
        {
            if (item.VModel.Contains("_poxiao_")) item.Config.tableName = item.VModel.ReplaceRegex(@"_poxiao_(\w+)", string.Empty).Replace("poxiao_", string.Empty); // 副表
            else item.Config.tableName = MainTableName != null ? MainTableName : string.Empty; // 主表
        });
        AuxiliaryTableFieldsModelList = FieldsModelList.Where(x => x.VModel.Contains("_poxiao_")).ToList(); // 单控件副表集合
        ChildTableFieldsModelList = FieldsModelList.Where(x => x.Config.poxiaoKey == PoxiaoKeyConst.TABLE).ToList(); // 子表集合
        MainTableFieldsModelList = FieldsModelList.Except(AuxiliaryTableFieldsModelList).Except(ChildTableFieldsModelList).ToList(); // 主表控件集合
        SingleFormData = FieldsModelList.Where(x => x.Config.poxiaoKey != PoxiaoKeyConst.TABLE).ToList(); // 非子表集合
        GenerateFields = GetGenerateFields(); // 系统生成控件

        MainTableFields = new Dictionary<string, string>();
        AuxiliaryTableFields = new Dictionary<string, string>();
        ChildTableFields = new Dictionary<string, string>();
        AllTableFields = new Dictionary<string, string>();
        MainTableFieldsModelList.Where(x => x.VModel.IsNotEmptyOrNull()).ToList().ForEach(x =>
        {
            MainTableFields.Add(x.VModel, x.Config.tableName + "." + x.VModel);
            AllTableFields.Add(x.VModel, x.Config.tableName + "." + x.VModel);
        });
        AuxiliaryTableFieldsModelList.Where(x => x.VModel.IsNotEmptyOrNull()).ToList().ForEach(x =>
        {
            AuxiliaryTableFields.Add(x.VModel, x.VModel.Replace("_poxiao_", ".").Replace("poxiao_", string.Empty));
            AllTableFields.Add(x.VModel, x.VModel.Replace("_poxiao_", ".").Replace("poxiao_", string.Empty));
        });
        ChildTableFieldsModelList.ForEach(item =>
        {
            item.Config.children.Where(x => x.VModel.IsNotEmptyOrNull()).ToList().ForEach(x =>
            {
                ChildTableFields.Add(item.VModel + "-" + x.VModel, item.Config.tableName + "." + x.VModel);
                AllTableFields.Add(item.VModel + "-" + x.VModel, item.Config.tableName + "." + x.VModel);
            });
        });

        WebType = webType;
        FormModel = new FormDataModel();
        FormModel.primaryKeyPolicy = primaryKeyPolicy;
        ColumnData = new ColumnDesignModel();
        ColumnData.type = 1;
        AppColumnData = new ColumnDesignModel();
        selectKey = uploaderKey;
        dataType = DataType;
    }

    /// <summary>
    /// 验证模板.
    /// </summary>
    /// <returns>true 通过.</returns>
    public bool VerifyTemplate()
    {
        if (FieldsModelList != null && FieldsModelList.Any(x => x.Config.poxiaoKey == PoxiaoKeyConst.TABLE))
        {
            foreach (FieldsModel? item in ChildTableFieldsModelList)
            {
                FieldsModel? tc = AuxiliaryTableFieldsModelList.Find(x => x.VModel.Contains(item.Config.tableName + "_poxiao_"));
                if (tc != null) return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 获取系统生成字段空格键.
    /// </summary>
    /// <returns></returns>
    public List<FieldsModel> GetGenerateFields()
    {
        // 系统生成字段 key
        var gfList = new List<string>() { PoxiaoKeyConst.BILLRULE, PoxiaoKeyConst.CREATEUSER, PoxiaoKeyConst.CREATETIME, PoxiaoKeyConst.MODIFYUSER, PoxiaoKeyConst.MODIFYTIME, PoxiaoKeyConst.CURRPOSITION, PoxiaoKeyConst.CURRORGANIZE, PoxiaoKeyConst.UPLOADFZ };

        return SingleFormData.Where(x => gfList.Contains(x.Config.poxiaoKey)).ToList();
    }

    /// <summary>
    /// 处理子表内的控件 添加到所有控件.
    /// </summary>
    private void AddChlidTableFeildsModel()
    {
        var ctList = new List<FieldsModel>();
        AllFieldsModel.Where(x => x.Config.poxiaoKey.Equals(PoxiaoKeyConst.TABLE)).ToList().ForEach(item =>
        {
            item.Config.children.Where(it => it.VModel.IsNotEmptyOrNull()).ToList().ForEach(it => it.VModel = item.VModel + "-" + it.VModel);
            ctList.AddRange(TemplateAnalysis.AnalysisTemplateData(item.Config.children));
        });
        AllFieldsModel.AddRange(ctList);
    }

    /// <summary>
    /// 处理子表内的控件 添加到所有控件.
    /// </summary>
    private void AddCodeGenChlidTableFeildsModel()
    {
        var ctList = new List<FieldsModel>();
        AllFieldsModel.Where(x => x.Config.poxiaoKey.Equals(PoxiaoKeyConst.TABLE)).ToList().ForEach(item =>
        {
            item.Config.children.Where(it => it.VModel.IsNotEmptyOrNull()).ToList().ForEach(it =>
            {
                it.Config.label = it.Config.label.Replace(it.VModel, item.VModel + "-" + it.VModel);
                it.VModel = item.VModel + "-" + it.VModel;
            });
            ctList.AddRange(item.Config.children);
        });
        AllFieldsModel.AddRange(ctList);
    }

    /// <summary>
    /// 根据表单类型初始化.
    /// </summary>
    /// <param name="formJson">表单Json.</param>
    /// <param name="tables">涉及表Json.</param>
    /// <param name="formType">表单类型（1：系统表单 2：自定义表单）.</param>
    private void InitByFormType(string formJson, string tables, int formType)
    {
        if (formType.Equals(1))
        {
            AllFieldsModel = new List<FieldsModel>();
            var fields = formJson.ToObject<List<Dictionary<string, object>>>();
            fields.ForEach(it =>
            {
                if (it.ContainsKey("filedId"))
                    AllFieldsModel.Add(new FieldsModel() { VModel = it["filedId"].ToString(), Config = new ConfigModel() { label = it["filedName"].ToString(), poxiaoKey = PoxiaoKeyConst.COMINPUT } });
            });
            FieldsModelList = AllFieldsModel;
        }
        else
        {
            FormDataModel formModel = formJson.ToObjectOld<FormDataModel>();
            FormModel = formModel; // 表单Json模型
            IsHasTable = !string.IsNullOrEmpty(tables) && !"[]".Equals(tables) && tables.IsNullOrEmpty(); // 是否有表
            AllFieldsModel = TemplateAnalysis.AnalysisTemplateData(formModel.fields.ToJsonString().ToObjectOld<List<FieldsModel>>()); // 所有控件集合
            FieldsModelList = TemplateAnalysis.AnalysisTemplateData(formModel.fields); // 已剔除布局控件集合
            MainTable = tables.ToList<TableModel>().Find(m => m.typeId.Equals("1")); // 主表
            MainTableName = MainTable?.table; // 主表名称
            AddChlidTableFeildsModel();

            // 处理旧控件 部分没有 tableName
            FieldsModelList.Where(x => string.IsNullOrWhiteSpace(x.Config.tableName)).ToList().ForEach(item =>
            {
                if (item.VModel.Contains("_poxiao_")) item.Config.tableName = item.VModel.ReplaceRegex(@"_poxiao_(\w+)", string.Empty).Replace("poxiao_", string.Empty); // 副表
                else item.Config.tableName = MainTableName != null ? MainTableName : string.Empty; // 主表
            });
            AllTable = tables.ToObject<List<TableModel>>(); // 所有表
            AuxiliaryTableFieldsModelList = FieldsModelList.Where(x => x.VModel.Contains("_poxiao_")).ToList(); // 单控件副表集合
            ChildTableFieldsModelList = FieldsModelList.Where(x => x.Config.poxiaoKey == PoxiaoKeyConst.TABLE).ToList(); // 子表集合
            MainTableFieldsModelList = FieldsModelList.Except(AuxiliaryTableFieldsModelList).Except(ChildTableFieldsModelList).ToList(); // 主表控件集合
            SingleFormData = FieldsModelList.Where(x => x.Config.poxiaoKey != PoxiaoKeyConst.TABLE).ToList(); // 非子表集合
            GenerateFields = GetGenerateFields(); // 系统生成控件

            MainTableFields = new Dictionary<string, string>();
            AuxiliaryTableFields = new Dictionary<string, string>();
            ChildTableFields = new Dictionary<string, string>();
            AllTableFields = new Dictionary<string, string>();
            MainTableFieldsModelList.Where(x => x.VModel.IsNotEmptyOrNull()).ToList().ForEach(x =>
            {
                MainTableFields.Add(x.VModel, x.Config.tableName + "." + x.VModel);
                AllTableFields.Add(x.VModel, x.Config.tableName + "." + x.VModel);
            });
            AuxiliaryTableFieldsModelList.Where(x => x.VModel.IsNotEmptyOrNull()).ToList().ForEach(x =>
            {
                AuxiliaryTableFields.Add(x.VModel, x.VModel.Replace("_poxiao_", ".").Replace("poxiao_", string.Empty));
                AllTableFields.Add(x.VModel, x.VModel.Replace("_poxiao_", ".").Replace("poxiao_", string.Empty));
            });
            ChildTableFieldsModelList.ForEach(item =>
            {
                item.Config.children.Where(x => x.VModel.IsNotEmptyOrNull()).ToList().ForEach(x =>
                {
                    ChildTableFields.Add(item.VModel + "-" + x.VModel, item.Config.tableName + "." + x.VModel);
                    AllTableFields.Add(item.VModel + "-" + x.VModel, item.Config.tableName + "." + x.VModel);
                });
            });

            ColumnData = new ColumnDesignModel();
            AppColumnData = new ColumnDesignModel();
        }
    }

    /// <summary>
    /// 初始化列配置模型.
    /// </summary>
    private void InitColumnData(VisualDevEntity entity)
    {
        if (!string.IsNullOrWhiteSpace(entity.ColumnData)) ColumnData = entity.ColumnData.ToObject<ColumnDesignModel>(); // 列配置模型
        else ColumnData = new ColumnDesignModel();

        if (!string.IsNullOrWhiteSpace(entity.AppColumnData)) AppColumnData = entity.AppColumnData.ToObject<ColumnDesignModel>(); // 列配置模型
        else AppColumnData = new ColumnDesignModel();

        if (AppColumnData.columnList != null && AppColumnData.columnList.Any())
        {
            AppColumnData.columnList.ForEach(item =>
            {
                var addColumn = ColumnData.columnList.Find(x => x.prop == item.prop);
                if (addColumn == null) ColumnData.columnList.Add(item);
            });
        }

        if (AppColumnData.searchList != null && AppColumnData.searchList.Any())
        {
            AppColumnData.searchList.ForEach(item =>
            {
                var addSearch = ColumnData.searchList.Find(x => x.Config.poxiaoKey == item.Config.poxiaoKey);
                if (addSearch == null) ColumnData.searchList.Add(item);
            });
        }

        if (ColumnData.searchList != null && ColumnData.searchList.Any())
        {
            ColumnData.searchList.Where(x => x.Config.poxiaoKey == PoxiaoKeyConst.CASCADER).ToList().ForEach(item =>
            {
                var it = SingleFormData.FirstOrDefault(x => x.VModel == item.VModel);
                if (it != null) item.multiple = it.props.props.multiple;
            });
        }

        FullName = entity.FullName;

        if (ColumnData.uploaderTemplateJson != null && ColumnData.uploaderTemplateJson.selectKey != null)
        {
            dataType = ColumnData.uploaderTemplateJson.dataType;
            selectKey = new List<string>();

            // 列顺序
            AllFieldsModel.ForEach(item =>
            {
                if (ColumnData.uploaderTemplateJson.selectKey.Any(x => x.Equals(item.VModel))) selectKey.Add(item.VModel);
            });
        }

        // 数据过滤
        if (ColumnData.ruleList != null && ColumnData.ruleList.Any())
        {
            DataRuleListJson = new List<IConditionalModel>();
            var condTree = new ConditionalTree() { ConditionalList = new List<KeyValuePair<WhereType, IConditionalModel>>() };
            ColumnData.ruleList.ForEach(item => condTree.ConditionalList.Add(new KeyValuePair<WhereType, IConditionalModel>(WhereType.And, GetItemRule(item))));
            DataRuleListJson.Add(condTree);
        }

        if (AppColumnData.ruleListApp != null && AppColumnData.ruleListApp.Any())
        {
            AppDataRuleListJson = new List<IConditionalModel>();
            var condTree = new ConditionalTree() { ConditionalList = new List<KeyValuePair<WhereType, IConditionalModel>>() };
            AppColumnData.ruleListApp.ForEach(item => condTree.ConditionalList.Add(new KeyValuePair<WhereType, IConditionalModel>(WhereType.And, GetItemRule(item))));
            AppDataRuleListJson.Add(condTree);
        }
    }

    private IConditionalModel GetItemRule(RuleFieldModel model)
    {
        var item = model.Copy();
        var conditionalType = ConditionalType.Equal;
        var between = new List<string>();
        if (item.fieldValue.IsNotEmptyOrNull())
        {
            if (item.symbol.Equals("between")) between = item.fieldValue.ToObject<List<string>>();
            switch (item.poxiaoKey)
            {
                case PoxiaoKeyConst.COMSELECT:
                    if (item.fieldValue.ToString().Replace("\r\n", "").Replace(" ", "").Contains("[[")) item.fieldValue = item.fieldValue.ToObject<List<List<string>>>().Select(x => x.Last() + "\"]").ToList();
                    else if (item.fieldValue.ToString().Replace("\r\n", "").Replace(" ", "").Contains("[")) item.fieldValue = item.fieldValue.ToObject<List<string>>().Select(x => x + "\"]").ToList();
                    break;
                case PoxiaoKeyConst.CREATETIME:
                case PoxiaoKeyConst.MODIFYTIME:
                case PoxiaoKeyConst.DATE:
                    {
                        if (item.symbol.Equals("between"))
                        {
                            var startTime = between.First().TimeStampToDateTime();
                            var endTime = between.Last().TimeStampToDateTime();
                            between[0] = startTime.ToString();
                            between[1] = endTime.ToString();
                            if (item.format == "yyyy-MM-dd")
                            {
                                between[0] = new DateTime(startTime.Year, startTime.Month, startTime.Day, 0, 0, 0, 0).ToString();
                                between[1] = new DateTime(endTime.Year, endTime.Month, endTime.Day, 23, 59, 59, 999).ToString();
                            }
                            else if (item.format == "yyyy")
                            {
                                between[0] = new DateTime(startTime.Year, 1, 1, 0, 0, 0, 0).ToString();
                                between[1] = new DateTime(endTime.Year, 1, 1, 0, 0, 0, 0).ToString();
                            }
                        }
                        else
                        {
                            if (item.format == "yyyy-MM-dd")
                            {
                                var value = item.fieldValue.ToString().TimeStampToDateTime();
                                item.fieldValue = new DateTime(value.Year, value.Month, value.Day, 0, 0, 0, 0).ToString();
                            }
                            else
                            {
                                item.fieldValue = item.fieldValue.IsNotEmptyOrNull() ? item.fieldValue.ToString().TimeStampToDateTime() : item.fieldValue;
                            }
                        }
                    }
                    break;
                case PoxiaoKeyConst.TIME:
                    {
                        if (!item.symbol.Equals("between"))
                        {
                            item.fieldValue = string.Format("{0:" + item.format + "}", Convert.ToDateTime(item.fieldValue));
                        }
                    }
                    break;
            }
        }
        switch (item.symbol)
        {
            case ">=":
                conditionalType = ConditionalType.GreaterThanOrEqual;
                break;
            case ">":
                conditionalType = ConditionalType.GreaterThan;
                break;
            case "==":
                conditionalType = ConditionalType.Equal;
                break;
            case "<=":
                conditionalType = ConditionalType.LessThanOrEqual;
                break;
            case "<":
                conditionalType = ConditionalType.LessThan;
                break;
            case "<>":
                conditionalType = ConditionalType.NoEqual;
                break;
            case "like":
                if (item.fieldValue != null && item.fieldValue.ToString().Contains("[")) item.fieldValue = item.fieldValue.ToString().Replace("[", string.Empty).Replace("]", string.Empty);
                conditionalType = ConditionalType.Like;
                break;
            case "notLike":
                if (item.fieldValue != null && item.fieldValue.ToString().Contains("[")) item.fieldValue = item.fieldValue.ToString().Replace("[", string.Empty).Replace("]", string.Empty);
                conditionalType = ConditionalType.NoLike;
                break;
            case "in":
            case "notIn":
                if (item.fieldValue != null && item.fieldValue.ToString().Contains("["))
                {
                    var isListValue = false;
                    var itemField = AllFieldsModel.Find(x => x.VModel.Equals(item.field));
                    if (itemField.multiple || item.poxiaoKey.Equals(PoxiaoKeyConst.CHECKBOX) || item.poxiaoKey.Equals(PoxiaoKeyConst.CASCADER) || item.poxiaoKey.Equals(PoxiaoKeyConst.ADDRESS))
                        isListValue = true;
                    if (item.poxiaoKey.Equals(PoxiaoKeyConst.COMSELECT)) isListValue = false;
                    var conditionalList = new ConditionalCollections() { ConditionalList = new List<KeyValuePair<WhereType, ConditionalModel>>() };
                    var ids = new List<string>();
                    if (item.fieldValue.ToString().Replace("\r\n", "").Replace(" ", "").Contains("[[")) ids = item.fieldValue.ToObject<List<List<string>>>().Select(x => x.Last()).ToList();
                    else ids = item.fieldValue.ToObject<List<string>>();

                    for (var i = 0; i < ids.Count; i++)
                    {
                        var it = ids[i];
                        var whereType = WhereType.And;
                        if (item.symbol.Equals("in")) whereType = i.Equals(0) && item.logic.Equals("&&") ? WhereType.And : WhereType.Or;
                        else whereType = i.Equals(0) && item.logic.Equals("||") ? WhereType.Or : WhereType.And;
                        conditionalList.ConditionalList.Add(new KeyValuePair<WhereType, ConditionalModel>(whereType, new ConditionalModel
                        {
                            FieldName = item.field,
                            ConditionalType = item.symbol.Equals("in") ? ConditionalType.Like : ConditionalType.NoLike,
                            FieldValue = isListValue ? it.ToJsonString() : it
                        }));
                    }

                    if (item.symbol.Equals("notIn"))
                    {
                        conditionalList.ConditionalList.Add(new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel
                        {
                            FieldName = item.field,
                            ConditionalType = ConditionalType.IsNot,
                            FieldValue = null
                        }));
                        conditionalList.ConditionalList.Add(new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel
                        {
                            FieldName = item.field,
                            ConditionalType = ConditionalType.IsNot,
                            FieldValue = string.Empty
                        }));
                    }

                    return conditionalList;
                }
                conditionalType = item.symbol.Equals("in") ? ConditionalType.In : ConditionalType.NotIn;
                break;
            case "null":
                conditionalType = (item.poxiaoKey.Equals(PoxiaoKeyConst.CALCULATE) || item.poxiaoKey.Equals(PoxiaoKeyConst.NUMINPUT)) ? ConditionalType.EqualNull : ConditionalType.IsNullOrEmpty;
                break;
            case "notNull":
                conditionalType = ConditionalType.IsNot;
                break;
            case "between":
                return new ConditionalCollections()
                {
                    ConditionalList = new List<KeyValuePair<WhereType, ConditionalModel>>()
                    {
                            new KeyValuePair<WhereType, ConditionalModel>( item.logic.Equals("&&") ? WhereType.And : WhereType.Or, new ConditionalModel
                            {
                                FieldName = item.field,
                                ConditionalType = ConditionalType.GreaterThanOrEqual,
                                FieldValue = between.First(),
                                FieldValueConvertFunc = it => Convert.ToDateTime(it)
                            }),
                            new KeyValuePair<WhereType, ConditionalModel>(WhereType.And, new ConditionalModel
                            {
                                FieldName = item.field,
                                ConditionalType = ConditionalType.LessThanOrEqual,
                                FieldValue = between.Last(),
                                FieldValueConvertFunc = it => Convert.ToDateTime(it)
                            })
                    }
                };
        }

        return new ConditionalCollections()
        {
            ConditionalList = new List<KeyValuePair<WhereType, ConditionalModel>>()
            {
                new KeyValuePair<WhereType, ConditionalModel>( item.logic.Equals("&&") ? WhereType.And : WhereType.Or, new ConditionalModel
                {
                    FieldName = item.field,
                    ConditionalType = conditionalType,
                    FieldValue = item.fieldValue == null ? null : item.fieldValue.ToString()
                })
            }
        };
    }
}