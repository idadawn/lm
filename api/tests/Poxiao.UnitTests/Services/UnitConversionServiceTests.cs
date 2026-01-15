using Poxiao.Lab.Interfaces;
using Xunit;
using Xunit.Abstractions;

namespace Poxiao.UnitTests.Services;

/// <summary>
/// 单位换算服务单元测试.
/// </summary>
public class UnitConversionServiceTests
{
    private readonly ITestOutputHelper _output;
    private readonly IUnitConversionService _unitConversionService;

    public UnitConversionServiceTests(ITestOutputHelper output, IUnitConversionService unitConversionService)
    {
        _output = output;
        _unitConversionService = unitConversionService;
    }

    /// <summary>
    /// 测试 15mm 转换为 15000μm 的逻辑.
    /// </summary>
    [Fact]
    public async Task Test_Convert_15mm_To_15000um()
    {
        // 根据初始化数据，UNIT_MM 是毫米，UNIT_UM 是微米
        // 基准单位是米 (m)
        // 毫米到基准单位：1mm = 0.001m，所以 ScaleToBase = 0.001
        // 微米到基准单位：1μm = 0.000001m，所以 ScaleToBase = 0.000001
        
        // 换算过程：
        // 1. 15mm 转换为基准单位（米）：15 * 0.001 = 0.015m
        // 2. 0.015m 转换为微米：0.015 / 0.000001 = 15000μm

        decimal sourceValue = 15m;
        string fromUnitId = "UNIT_MM"; // 毫米
        string toUnitId = "UNIT_UM";   // 微米

        var result = await _unitConversionService.ConvertAsync(sourceValue, fromUnitId, toUnitId);

        _output.WriteLine($"转换结果：{sourceValue}mm = {result}μm");

        // 验证结果：15mm = 15000μm
        Assert.Equal(15000m, result);
    }

    /// <summary>
    /// 测试跨维度换算应抛出异常.
    /// </summary>
    [Fact]
    public async Task Test_Convert_CrossCategory_ShouldThrowException()
    {
        decimal sourceValue = 15m;
        string fromUnitId = "UNIT_MM"; // 长度单位
        string toUnitId = "UNIT_KG";   // 质量单位

        // 应该抛出异常，因为不能跨维度换算
        await Assert.ThrowsAsync<Exception>(async () =>
        {
            await _unitConversionService.ConvertAsync(sourceValue, fromUnitId, toUnitId);
        });
    }

    /// <summary>
    /// 测试其他长度单位换算.
    /// </summary>
    [Theory]
    [InlineData(1, "UNIT_M", "UNIT_MM", 1000)]      // 1m = 1000mm
    [InlineData(1, "UNIT_CM", "UNIT_MM", 10)]       // 1cm = 10mm
    [InlineData(1000, "UNIT_MM", "UNIT_M", 1)]      // 1000mm = 1m
    [InlineData(1, "UNIT_KM", "UNIT_M", 1000)]      // 1km = 1000m
    public async Task Test_Length_Conversions(decimal value, string fromUnitId, string toUnitId, decimal expected)
    {
        var result = await _unitConversionService.ConvertAsync(value, fromUnitId, toUnitId);
        
        _output.WriteLine($"转换结果：{value} {fromUnitId} = {result} {toUnitId}，期望值：{expected}");

        Assert.Equal(expected, result, 10); // 允许10位小数精度误差
    }

    /// <summary>
    /// 测试获取单位维度列表.
    /// </summary>
    [Fact]
    public async Task Test_GetCategories()
    {
        var categories = await _unitConversionService.GetCategoriesAsync();

        Assert.NotNull(categories);
        Assert.True(categories.Count > 0);

        _output.WriteLine($"找到 {categories.Count} 个单位维度：");
        foreach (var category in categories)
        {
            _output.WriteLine($"  - {category.Name} ({category.Code})");
        }
    }

    /// <summary>
    /// 测试根据维度获取单位列表.
    /// </summary>
    [Fact]
    public async Task Test_GetUnitsByCategory()
    {
        // 先获取长度维度
        var categories = await _unitConversionService.GetCategoriesAsync();
        var lengthCategory = categories.FirstOrDefault(c => c.Code == "LENGTH");

        Assert.NotNull(lengthCategory);

        var units = await _unitConversionService.GetUnitsByCategoryAsync(lengthCategory.Id);

        Assert.NotNull(units);
        Assert.True(units.Count > 0);

        _output.WriteLine($"长度维度包含 {units.Count} 个单位：");
        foreach (var unit in units)
        {
            _output.WriteLine($"  - {unit.DisplayName} (基准单位：{unit.IsBase})");
        }

        // 验证至少有一个基准单位
        Assert.True(units.Any(u => u.IsBase));
    }
}
