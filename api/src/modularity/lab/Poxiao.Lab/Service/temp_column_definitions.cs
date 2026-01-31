using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Poxiao.Lab.Service
{
    /// <summary>
    /// Helper class for defining Excel column properties from JSON configuration.
    /// </summary>
    public class ColumnDefinition
    {
        public int ColumnIndex { get; set; }
        public string ColumnName { get; set; } // Property name on IntermediateDataEntity
        public string ColumnTitle { get; set; } // Display header in Excel
        public string Unit { get; set; } // Unit to append to title, if any
        public string DataType { get; set; } // C# data type name
        public bool IsDynamic { get; set; } // Indicates dynamic columns like Thickness_X
        public string DynamicColumnPrefix { get; set; } // Prefix for dynamic columns (e.g., "Thickness")
        public bool IsMerged { get; set; } // Indicates columns like MidSi/MidB that combine two properties
        public string MergeLeftProperty { get; set; } // Left property for merged column
        public string MergeRightProperty { get; set; } // Right property for merged column
    }

    public partial class IntermediateDataExportService 
    {
        private static readonly List<ColumnDefinition> _columnDefinitions = JsonConvert.DeserializeObject<List<ColumnDefinition>>(
@"
[
    {
        ""ColumnIndex"": 0,
        ""ColumnName"": ""DetectionDate"",
        ""ColumnTitle"": ""检验日期"",
        ""DataType"": ""System.DateTime""
    },
    {
        ""ColumnIndex"": 1,
        ""ColumnName"": ""SprayNo"",
        ""ColumnTitle"": ""喷次"",
        ""DataType"": ""System.String""
    },
    {
        ""ColumnIndex"": 2,
        ""ColumnName"": ""Labeling"",
        ""ColumnTitle"": ""贴标"",
        ""DataType"": ""System.String""
    },
    {
        ""ColumnIndex"": 3,
        ""ColumnName"": ""FurnaceNoFormatted"",
        ""ColumnTitle"": ""炉号"",
        ""DataType"": ""System.String""
    },
    {
        ""ColumnIndex"": 4,
        ""ColumnName"": ""PerfSsPower"",
        ""ColumnTitle"": ""Ss激磁功率"",
        ""Unit"": ""T"",
        ""DataType"": ""System.Decimal""
    },
    {
        ""ColumnIndex"": 5,
        ""ColumnName"": ""PerfPsLoss"",
        ""ColumnTitle"": ""Ps铁损"",
        ""Unit"": ""W/kg"",
        ""DataType"": ""System.Decimal""
    },
    {
        ""ColumnIndex"": 6,
        ""ColumnName"": ""PerfHc"",
        ""ColumnTitle"": ""Hc"",
        ""Unit"": ""A/m"",
        ""DataType"": ""System.Decimal""
    },
    {
        ""ColumnIndex"": 7,
        ""ColumnName"": ""PerfAfterSsPower"",
        ""ColumnTitle"": ""刻痕后Ss激磁功率"",
        ""Unit"": ""T"",
        ""DataType"": ""System.Decimal""
    },
    {
        ""ColumnIndex"": 8,
        ""ColumnName"": ""PerfAfterPsLoss"",
        ""ColumnTitle"": ""刻痕后Ps铁损"",
        ""Unit"": ""W/kg"",
        ""DataType"": ""System.Decimal""
    },
    {
        ""ColumnIndex"": 9,
        ""ColumnName"": ""PerfAfterHc"",
        ""ColumnTitle"": ""刻痕后Hc"",
        ""Unit"": ""A/m"",
        ""DataType"": ""System.Decimal""
    },
    {
        ""ColumnIndex"": 10,
        ""ColumnName"": ""PerfEditorName"",
        ""ColumnTitle"": ""性能录入员"",
        ""DataType"": ""System.String""
    },
    {
        ""ColumnIndex"": 11,
        ""ColumnName"": ""OneMeterWeight"",
        ""ColumnTitle"": ""一米带重"",
        ""Unit"": ""g"",
        ""DataType"": ""System.Decimal""
    },
    {
        ""ColumnIndex"": 12,
        ""ColumnName"": ""Width"",
        ""ColumnTitle"": ""带宽"",
        ""Unit"": ""mm"",
        ""DataType"": ""System.Decimal""
    },
    {
        ""ColumnIndex"": 13,
        ""ColumnName"": ""Thickness"",
        ""ColumnTitle"": ""带厚"",
        ""Unit"": ""mm"",
        ""IsDynamic"": true,
        ""DynamicColumnPrefix"": ""Thickness"",
        ""DataType"": ""System.Decimal""
    },
    {
        ""ColumnIndex"": 14,
        ""ColumnName"": ""ThicknessMin"",
        ""ColumnTitle"": ""带厚最小值"",
        ""Unit"": ""mm"",
        ""DataType"": ""System.Decimal""
    },
    {
        ""ColumnIndex"": 15,
        ""ColumnName"": ""ThicknessMax"",
        ""ColumnTitle"": ""带厚最大值"",
        ""Unit"": ""mm"",
        ""DataType"": ""System.Decimal""
    },
    {
        ""ColumnIndex"": 16,
        ""ColumnName"": ""ThicknessDiff"",
        ""ColumnTitle"": ""带厚极差"",
        ""Unit"": ""mm"",
        ""DataType"": ""System.Decimal""
    },
    {
        ""ColumnIndex"": 17,
        ""ColumnName"": ""Density"",
        ""ColumnTitle"": ""密度"",
        ""Unit"": ""g/cm³"",
        ""DataType"": ""System.Decimal""
    },
    {
        ""ColumnIndex"": 18,
        ""ColumnName"": ""LaminationFactor"",
        ""ColumnTitle"": ""叠片系数"",
        ""DataType"": ""System.Decimal""
    },
    {
        ""ColumnIndex"": 19,
        ""ColumnName"": ""AppearanceFeatureCategory"",
        ""ColumnTitle"": ""外观特性"",
        ""IsDynamic"": true,
        ""DynamicColumnPrefix"": ""AppearanceFeatureCategory"",
        ""DataType"": ""System.String""
    },
    {
        ""ColumnIndex"": 20,
        ""ColumnName"": ""BreakCount"",
        ""ColumnTitle"": ""断头数"",
        ""DataType"": ""System.Int32""
    },
    {
        ""ColumnIndex"": 21,
        ""ColumnName"": ""SingleCoilWeight"",
        ""ColumnTitle"": ""单卷重量"",
        ""Unit"": ""kg"",
        ""DataType"": ""System.Decimal""
    },
    {
        ""ColumnIndex"": 22,
        ""ColumnName"": ""AppearEditorName"",
        ""ColumnTitle"": ""外观检验员"",
        ""DataType"": ""System.String""
    },
    {
        ""ColumnIndex"": 23,
        ""ColumnName"": ""AvgThickness"",
        ""ColumnTitle"": ""平均厚度"",
        ""Unit"": ""mm"",
        ""DataType"": ""System.Decimal""
    },
    {
        ""ColumnIndex"": 24,
        ""ColumnName"": ""MagneticResult"",
        ""ColumnTitle"": ""磁性能判定"",
        ""DataType"": ""System.String""
    },
    {
        ""ColumnIndex"": 25,
        ""ColumnName"": ""ThicknessResult"",
        ""ColumnTitle"": ""厚度判定"",
        ""DataType"": ""System.String""
    },
    {
        ""ColumnIndex"": 26,
        ""ColumnName"": ""LaminationResult"",
        ""ColumnTitle"": ""叠片系数判定"",
        ""DataType"": ""System.String""
    },
    {
        ""ColumnIndex"": 27,
        ""ColumnName"": ""MidSi"",
        ""ColumnTitle"": ""中Si"",
        ""IsMerged"": true,
        ""MergeLeftProperty"": ""MidSiLeft"",
        ""MergeRightProperty"": ""MidSiRight"",
        ""DataType"": ""System.String""
    },
    {
        ""ColumnIndex"": 28,
        ""ColumnName"": ""MidB"",
        ""ColumnTitle"": ""中B"",
        ""IsMerged"": true,
        ""MergeLeftProperty"": ""MidBLeft"",
        ""MergeRightProperty"": ""MidBRight"",
        ""DataType"": ""System.String""
    },
    {
        ""ColumnIndex"": 29,
        ""ColumnName"": ""LeftPatternWidth"",
        ""ColumnTitle"": ""左花纹纹宽"",
        ""Unit"": ""mm"",
        ""DataType"": ""System.Decimal""
    },
    {
        ""ColumnIndex"": 30,
        ""ColumnName"": ""LeftPatternSpacing"",
        ""ColumnTitle"": ""左花纹纹距"",
        ""Unit"": ""mm"",
        ""DataType"": ""System.Decimal""
    },
    {
        ""ColumnIndex"": 31,
        ""ColumnName"": ""MidPatternWidth"",
        ""ColumnTitle"": ""中花纹纹宽"",
        ""Unit"": ""mm"",
        ""DataType"": ""System.Decimal""
    },
    {
        ""ColumnIndex"": 32,
        ""ColumnName"": ""MidPatternSpacing"",
        ""ColumnTitle"": ""中花纹纹距"",
        ""Unit"": ""mm"",
        ""DataType"": ""System.Decimal""
    },
    {
        ""ColumnIndex"": 33,
        ""ColumnName"": ""RightPatternWidth"",
        ""ColumnTitle"": ""右花纹纹宽"",
        ""Unit"": ""mm"",
        ""DataType"": ""System.Decimal""
    },
    {
        ""ColumnIndex"": 34,
        ""ColumnName"": ""RightPatternSpacing"",
        ""ColumnTitle"": ""右花纹纹距"",
        ""Unit"": ""mm"",
        ""DataType"": ""System.Decimal""
    },
    {
        ""ColumnIndex"": 35,
        ""ColumnName"": ""CoilWeight"",
        ""ColumnTitle"": ""四米带重"",
        ""Unit"": ""kg"",
        ""DataType"": ""System.Decimal""
    },
    {
        ""ColumnIndex"": 36,
        ""ColumnName"": ""Detection"",
        ""ColumnTitle"": ""叠片系数厚度分布"",
        ""IsDynamic"": true,
        ""DynamicColumnPrefix"": ""Detection"",
        ""DataType"": ""System.Decimal""
    },
    {
        ""ColumnIndex"": 37,
        ""ColumnName"": ""MaxThicknessRaw"",
        ""ColumnTitle"": ""最大厚度"",
        ""Unit"": ""mm"",
        ""DataType"": ""System.Decimal""
    },
    {
        ""ColumnIndex"": 38,
        ""ColumnName"": ""MaxAvgThickness"",
        ""ColumnTitle"": ""最大平均厚度"",
        ""Unit"": ""mm"",
        ""DataType"": ""System.Decimal""
    },
    {
        ""ColumnIndex"": 39,
        ""ColumnName"": ""CreatorUserId"",
        ""ColumnTitle"": ""录入员"",
        ""DataType"": ""System.String""
    },
    {
        ""ColumnIndex"": 40,
        ""ColumnName"": ""StripType"",
        ""ColumnTitle"": ""带型"",
        ""DataType"": ""System.String""
    },
    {
        ""ColumnIndex"": 41,
        ""ColumnName"": ""FirstInspection"",
        ""ColumnTitle"": ""一次交检"",
        ""DataType"": ""System.String""
    },
    {
        ""ColumnIndex"": 42,
        ""ColumnName"": ""ShiftNo"",
        ""ColumnTitle"": ""班次"",
        ""DataType"": ""System.String""
    },
    {
        ""ColumnIndex"": 43,
        ""ColumnName"": ""CalcStatus"",
        ""ColumnTitle"": ""计算状态"",
        ""DataType"": ""System.String""
    }
]
"
);
    }
}