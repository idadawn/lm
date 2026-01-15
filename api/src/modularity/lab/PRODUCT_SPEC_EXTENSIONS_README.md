# 产品规格扩展属性使用指南

## 概述

产品规格模块已经升级，支持可扩展的计算属性系统。除了基本的长度（4m）、层数（20）、密度（7.25）属性外，现在可以轻松添加新的属性用于后续计算。

## 后端使用

### 1. 获取扩展属性管理器

```csharp
var productSpec = new ProductSpecEntity();
var attrManager = productSpec.GetCalculationAttributesManager();
```

### 2. 设置基本属性

```csharp
// 基本属性（带默认值）
attrManager.Length = 4.0m;   // 长度，默认4米
attrManager.Layers = 20;     // 层数，默认20层
attrManager.Density = 7.25m; // 密度，默认7.25
```

### 3. 使用预定义的扩展属性

```csharp
// 扩展属性
attrManager.Width = 1.5m;              // 宽度（米）
attrManager.Thickness = 12.5m;         // 厚度（毫米）
attrManager.Weight = 850.5m;           // 重量（kg）
attrManager.TensileStrength = 450m;    // 抗拉强度（MPa）
attrManager.YieldStrength = 320m;      // 屈服强度（MPa）
attrManager.Elongation = 25m;          // 延伸率（%）
```

### 4. 添加自定义属性

```csharp
// 添加任意自定义属性
attrManager.AddAttribute("surfaceArea", 125.5m);     // 表面积
attrManager.AddAttribute("coatingThickness", 0.05m); // 涂层厚度
attrManager.AddAttribute("hardness", "HRC 65");      // 硬度
attrManager.AddAttribute("temperatureResistance", 800); // 耐温性
```

### 5. 保存更改

```csharp
// 保存所有更改到实体
attrManager.SaveChanges();

// 然后保存到数据库
db.Updateable(productSpec).ExecuteCommand();
```

### 6. 使用计算功能

```csharp
// 计算总重量（需要宽度和厚度）
var totalWeight = productSpec.CalculateTotalWeight();

// 计算每米重量
var weightPerMeter = productSpec.CalculateWeightPerMeter();

// 计算体积（需要宽度和厚度）
var volume = productSpec.CalculateVolume();

// 获取理论重量
var theoreticalWeight = productSpec.GetTheoreticalWeight();
```

### 7. 获取所有属性

```csharp
// 获取所有属性字典
var allAttributes = attrManager.GetAllAttributes();

// 遍历所有属性
foreach (var attr in allAttributes)
{
    Console.WriteLine($"{attr.Key}: {attr.Value}");
}
```

## 前端使用

### 1. 基本表单

前端表单已经集成了扩展属性组件，会自动显示以下预定义属性：
- 宽度（m）
- 厚度（mm）
- 重量（kg）
- 抗拉强度（MPa）
- 屈服强度（MPa）
- 延伸率（%）

### 2. 添加自定义属性

用户可以通过表单动态添加新的属性：

1. 点击"添加扩展属性"按钮
2. 输入属性名称和键名
3. 选择属性类型（数字、文本、下拉选择）
4. 设置相应的约束（最小值、最大值、精度等）
5. 保存属性

### 3. 属性值管理

- 属性值为空时不会保存到数据库
- 修改属性值会实时更新
- 可以删除不需要的属性
- 支持批量清空所有属性

## 扩展性说明

### 添加新的预定义属性

1. 在 `ProductSpecCalculationAttributes` 类中添加新属性：

```csharp
public decimal? NewProperty
{
    get => GetExtendedValue<decimal?>(nameof(NewProperty));
    set => SetExtendedValue(nameof(NewProperty), value);
}
```

2. 在前端 `ExtendedAttributesForm.vue` 的 `predefinedAttributes` 中添加对应配置：

```javascript
{
    key: 'newProperty',
    label: '新属性(单位)',
    type: 'number',
    min: 0,
    precision: 2
}
```

### 添加新的计算方法

在 `ProductSpecExtendedUsageExamples` 类中添加扩展方法：

```csharp
public static decimal? CalculateNewMetric(this ProductSpecEntity productSpec)
{
    var attrManager = productSpec.GetCalculationAttributesManager();
    // 实现计算逻辑
    return result;
}
```

## 默认值说明

| 属性 | 默认值 | 单位 |
|------|--------|------|
| 长度 | 4 | 米 |
| 层数 | 20 | 层 |
| 密度 | 7.25 | - |

## 注意事项

1. 基本属性（长度、层数、密度）保存在数据库的独立字段中
2. 扩展属性保存在 `PropertyJson` 字段中，格式为 JSON
3. 属性值在保存时会进行验证，确保数据类型正确
4. 计算方法的返回值可能为 null，如果缺少必要的参数
5. 扩展属性的键名建议使用英文，便于程序处理

## 示例代码

详见 `ProductSpecServiceExtendedDemo.cs` 文件，包含了完整的使用示例。```markdown

## 总结

通过这个扩展属性系统，产品规格模块现在具备了强大的可扩展性。你可以：

1. ✅ 使用默认的长度（4m）、层数（20）、密度（7.25）属性
2. ✅ 添加预定义的扩展属性（宽度、厚度、重量等）
3. ✅ 动态添加任意自定义属性
4. ✅ 基于这些属性进行各种计算
5. ✅ 前后端完美集成，用户体验良好

系统已经为未来的扩展做好了准备，可以轻松添加新的属性和计算方法。