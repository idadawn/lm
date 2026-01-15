# 通用单位管理与换算模块

## 模块概述

本模块提供通用的物理单位管理系统，支持单位标准化、自动化换算以及前端下拉选择功能。

## 数据库表结构

### 1. UNIT_CATEGORY（单位维度表）

定义单位所属的物理量维度，如长度、质量、密度等。

### 2. UNIT_DEFINITION（单位定义表）

定义具体的单位及其换算比例。

**重要约束**：
- 每个维度必须且只能有一个基准单位（`F_IS_BASE = 1`）
- 所有单位的换算都基于基准单位进行

## 核心功能

### 1. 单位换算服务（IUnitConversionService）

#### 换算算法

```
1. 将原始数值转换为基准单位：
   V_base = (V_source × ScaleToBase_from) + Offset_from

2. 将基准单位转换为目标单位：
   V_target = (V_base - Offset_to) / ScaleToBase_to
```

#### 使用示例

```csharp
// 注入服务
private readonly IUnitConversionService _unitConversionService;

// 换算 15mm 为微米
var result = await _unitConversionService.ConvertAsync(15m, "UNIT_MM", "UNIT_UM");
// 结果：15000
```

### 2. API 端点

#### 单位换算
```
POST /api/lab/unit/convert
Body: {
  "value": 15,
  "fromUnitId": "UNIT_MM",
  "toUnitId": "UNIT_UM"
}
```

#### 获取所有单位维度
```
GET /api/lab/unit/categories
```

#### 根据维度获取单位列表
```
GET /api/lab/unit/units/{categoryId}
```

#### 获取所有单位（按维度分组）
```
GET /api/lab/unit/units/all
```

## 前端集成

### 下拉选择框格式

单位下拉框显示格式：`单位全称 (单位符号)`

例如：
- 毫米 (mm)
- 微米 (μm)
- 千克 (kg)

### 数值显示精度

前端应根据单位的 `F_PRECISION` 字段自动截取小数位数。

## 数据库初始化

执行 SQL 脚本：
```
sql/migration_create_unit_management_tables.sql
```

该脚本包含：
- 表结构创建
- 初始数据（长度、质量、密度、压力、电感等维度及常用单位）

## 单元测试

运行单元测试验证换算逻辑：
```bash
dotnet test api/tests/Poxiao.UnitTests/Poxiao.UnitTests.csproj --filter "FullyQualifiedName~UnitConversionServiceTests"
```

测试用例包括：
- ✅ 15mm 转换为 15000μm
- ✅ 跨维度换算异常处理
- ✅ 其他长度单位换算

## 注意事项

1. **跨维度换算**：系统会阻止跨维度换算（如长度转质量），并抛出异常
2. **基准单位**：每个维度必须有一个且仅有一个基准单位
3. **字段命名**：所有字段使用 `F_` 前缀，表名不使用 `LAB_` 前缀
4. **租户隔离**：支持多租户，自动根据租户ID过滤数据
