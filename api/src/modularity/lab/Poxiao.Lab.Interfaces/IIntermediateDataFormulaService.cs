using Poxiao.Lab.Entity;
using Poxiao.Lab.Entity.Dto.IntermediateDataFormula;

namespace Poxiao.Lab.Interfaces;

/// <summary>
/// 中间数据表公式维护服务接口.
/// </summary>
public interface IIntermediateDataFormulaService
{
    /// <summary>
    /// 获取所有公式列表.
    /// </summary>
    Task<List<IntermediateDataFormulaDto>> GetListAsync();

    /// <summary>
    /// 根据ID获取公式.
    /// </summary>
    Task<IntermediateDataFormulaDto> GetByIdAsync(string id);

    /// <summary>
    /// 创建公式.
    /// </summary>
    Task<IntermediateDataFormulaDto> CreateAsync(IntermediateDataFormulaDto dto);

    /// <summary>
    /// 更新公式.
    /// </summary>
    Task<IntermediateDataFormulaDto> UpdateAsync(string id, IntermediateDataFormulaDto dto);

    /// <summary>
    /// 删除公式.
    /// </summary>
    Task DeleteAsync(string id);

    /// <summary>
    /// 更新公式内容（仅更新公式相关字段）.
    /// </summary>
    Task<IntermediateDataFormulaDto> UpdateFormulaAsync(string id, FormulaUpdateInput input);

    /// <summary>
    /// 获取中间数据表可用列列表.
    /// </summary>
    /// <param name="includeHidden">是否包含隐藏列（ShowInFormulaMaintenance=false）</param>
    Task<List<IntermediateDataColumnInfo>> GetAvailableColumnsAsync(bool includeHidden = false);

    /// <summary>
    /// 初始化公式列表（根据可用列自动生成）.
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// 获取公式变量来源列表（用于公式编辑时选择变量）.
    /// </summary>
    Task<List<FormulaVariableSource>> GetVariableSourcesAsync();

    /// <summary>
    /// 验证公式.
    /// </summary>
    Task<FormulaValidationResult> ValidateFormulaAsync(FormulaValidationRequest request);
}

/// <summary>
/// 公式验证请求.
/// </summary>
public class FormulaValidationRequest
{
    /// <summary>
    /// 公式表达式.
    /// </summary>
    public string Formula { get; set; }

    /// <summary>
    /// 公式语言.
    /// </summary>
    public string FormulaLanguage { get; set; } = "EXCEL";

    /// <summary>
    /// 列名（可选，用于上下文验证）.
    /// </summary>
    public string ColumnName { get; set; }
}

/// <summary>
/// 公式验证结果.
/// </summary>
public class FormulaValidationResult
{
    /// <summary>
    /// 是否有效.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// 错误消息.
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// 未定义的变量列表.
    /// </summary>
    public List<string> UndefinedVariables { get; set; } = new();
}

/// <summary>
/// 公式更新输入参数.
/// </summary>
public class FormulaUpdateInput
{
    /// <summary>
    /// 公式内容.
    /// </summary>
    public string Formula { get; set; }
}
