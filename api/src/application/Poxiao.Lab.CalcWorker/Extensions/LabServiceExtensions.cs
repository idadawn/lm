using Poxiao.Lab.CalcWorker.Services;
using Poxiao.Lab.Entity;
using Poxiao.Lab.Helpers;
using Poxiao.Lab.Interfaces;
using Poxiao.Lab.Service;

namespace Poxiao.Lab.CalcWorker.Extensions;

/// <summary>
/// Lab 计算相关服务注册扩展。
/// 在独立 Worker 中手动注册，替代框架的 ITransient 自动扫描。
/// </summary>
public static class LabServiceExtensions
{
    public static IServiceCollection AddLabCalculationServices(this IServiceCollection services)
    {
        // 公式解析器
        services.AddTransient<IFormulaParser, FormulaParser>();

        // Worker 专用公式服务（仅实现 GetListAsync，用于批量计算）
        services.AddTransient<IIntermediateDataFormulaService, WorkerIntermediateDataFormulaService>();

        // 批量计算器
        services.AddTransient<IntermediateDataFormulaBatchCalculator>();

        // 中间数据生成器（从原始数据生成中间数据的纯计算逻辑）
        services.AddTransient<IntermediateDataGenerator>();

        return services;
    }
}

