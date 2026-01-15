using System.Text.RegularExpressions;
using Poxiao.DependencyInjection;
using Poxiao.Lab.Entity.Entity;
using Poxiao.Lab.Helpers;
using Poxiao.Lab.Interfaces;

namespace Poxiao.Lab.Service;

/// <summary>
/// 原始数据校验服务
/// </summary>
public class RawDataValidationService : IRawDataValidationService, ITransient
{
    public List<string> ValidateBasicFields(RawDataEntity entity)
    {
        var errors = new List<string>();
        if (entity.ProdDate == null)
            errors.Add("日期不能为空");
        if (string.IsNullOrWhiteSpace(entity.FurnaceNo))
            errors.Add("炉号不能为空");
        if (entity.Width == null)
            errors.Add("宽度不能为空");
        if (entity.CoilWeight == null)
            errors.Add("带材重量不能为空");

        // Logical ranges
        if (entity.Width.HasValue && (entity.Width < 0 || entity.Width > 3000))
            errors.Add("宽度数值异常");
        if (entity.CoilWeight.HasValue && entity.CoilWeight < 0)
            errors.Add("带材重量数值异常");

        if (entity.ProdDate.HasValue)
        {
            var minDate = new DateTime(2020, 1, 1);
            var maxDate = DateTime.Now.AddDays(1);
            if (entity.ProdDate.Value < minDate)
                errors.Add($"日期不能早于{minDate:yyyy-MM-dd}");
            if (entity.ProdDate.Value > maxDate)
                errors.Add("日期不能晚于当前日期");
        }

        return errors;
    }

    public string ValidateFurnaceNo(string furnaceNo)
    {
        if (string.IsNullOrWhiteSpace(furnaceNo))
            return "炉号为空";

        if (!FurnaceNoHelper.IsValidFurnaceNo(furnaceNo))
        {
            return "炉号格式不符合规则";
        }
        return null;
    }

    public List<string> ValidateIntegrity(RawDataEntity entity)
    {
        var errors = new List<string>();
        if (string.IsNullOrEmpty(entity.ProductSpecId))
        {
            errors.Add("未匹配产品规格");
        }

        // ValidData means successful parse and valid format.
        if (entity.IsValidData == 0)
        {
            errors.Add("数据无效（炉号解析失败或格式错误）");
        }

        // Feature check: optional? "AI Assistant" doc says "User can modify match results".
        // It doesn't strictly say every row MUST have a feature. But usually "FeatureSuffix" implies features.
        // If FeatureSuffix exists but AppearanceFeatureIds is empty, maybe a warning?
        // We'll leave it as non-blocking for now unless strict requirement.

        return errors;
    }
}
