using Microsoft.AspNetCore.Mvc;
using MiniExcelLibs;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Poxiao.DependencyInjection;
using Poxiao.DynamicApiController;
using Poxiao.FriendlyException;
using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Lab.Entity;
using Poxiao.Lab.Entity.Dto.IntermediateData;
using Poxiao.Lab.Entity.Enums;
using Poxiao.Lab.Interfaces;
using SqlSugar;
using System.Data;
using System.Globalization;
using System.IO.Compression;
using System.Text.Json;
namespace Poxiao.Lab.Service;

/// <summary>
/// 中间数据导出服务.
/// </summary>
[ApiDescriptionSettings(Tag = "Lab", Name = "intermediate-data-export", Order = 200)]
[Route("api/lab/intermediate-data")]
public partial class IntermediateDataExportService : IDynamicApiController, ITransient
{
    private readonly IIntermediateDataService _intermediateDataService;
    private readonly IIntermediateDataColorService _colorService;
    private readonly ISqlSugarRepository<IntermediateDataEntity> _repository;
    private readonly ISqlSugarRepository<ProductSpecEntity> _productSpecRepository;
    private readonly ISqlSugarRepository<AppearanceFeatureCategoryEntity> _categoryRepository;
    private readonly ISqlSugarRepository<AppearanceFeatureEntity> _appearanceFeatureRepository;
    private readonly ISqlSugarRepository<AppearanceFeatureLevelEntity> _appearanceLevelRepository;
    private readonly IUserManager _userManager;

    public IntermediateDataExportService(
        ISqlSugarRepository<IntermediateDataEntity> repository,
        ISqlSugarRepository<ProductSpecEntity> productSpecRepository,
        ISqlSugarRepository<AppearanceFeatureCategoryEntity> categoryRepository,
        ISqlSugarRepository<AppearanceFeatureEntity> appearanceFeatureRepository,
        ISqlSugarRepository<AppearanceFeatureLevelEntity> appearanceLevelRepository,
        IUserManager userManager,
        IIntermediateDataService intermediateDataService,
        IIntermediateDataColorService colorService)
    {
        _repository = repository;
        _productSpecRepository = productSpecRepository;
        _categoryRepository = categoryRepository;
        _appearanceFeatureRepository = appearanceFeatureRepository;
        _appearanceLevelRepository = appearanceLevelRepository;
        _userManager = userManager;
        _intermediateDataService = intermediateDataService;
        _colorService = colorService;
    }

    /// <summary>
    /// 导出中间数据Excel（按月+产品规格分组）.
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportIntermediateData([FromQuery] IntermediateDataExportInput input)
    {
        // 1. 验证日期范围
        if (!input.StartDate.HasValue || !input.EndDate.HasValue)
        {
            throw Oops.Bah("请选择生产日期范围");
        }

        var startDate = input.StartDate.Value;
        var endDate = input.EndDate.Value;

        // 验证最多一年
        if ((endDate - startDate).TotalDays > 366)
        {
            throw Oops.Bah("导出时间范围不能超过一年");
        }

        // 2. 调用 GetList 获取数据 (PageSize设为最大值以获取所有数据)
        var listQuery = new IntermediateDataListQuery
        {
            StartDate = startDate,
            EndDate = endDate,
            PageSize = 999999, // 获取所有数据
            CurrentPage = 1,
            // FeatureSuffix = input.FeatureSuffix, // 移除筛选
            // Keyword = input.Keyword, // 移除筛选
            // ProductSpecId = input.ProductSpecId // 移除筛选
        };

        // GetList 返回 dynamic { list, pagination }
        // list 是 List<Dictionary<string, object>> (经过 Mapster 和 格式化处理)
        dynamic result = await _intermediateDataService.GetList(listQuery);
        var dataList = result.list as List<Dictionary<string, object>>;

        if (dataList == null || dataList.Count == 0)
        {
            throw Oops.Bah("指定时间范围内没有数据");
        }

        // 2.5 获取所有特性大类（用于外观特性列）
        var categories = await _categoryRepository.GetListAsync(
            c => (c.DeleteMark == null || c.DeleteMark == 0) &&
                 (c.ParentId == null || c.ParentId == "-1")
        );
        var categoryMap = categories.ToDictionary(c => c.Id, c => c.Name);
        var sortedCategories = categoryMap.OrderBy(k => k.Value).ToList();

        // 2.6 获取特性与等级映射（用于外观列导出特性等级名称）
        var allFeatures = await _appearanceFeatureRepository.GetListAsync(
            f => f.DeleteMark == null || f.DeleteMark == 0);
        var levelIds = allFeatures
            .Select(f => f.SeverityLevelId)
            .Where(id => !string.IsNullOrEmpty(id))
            .Distinct()
            .ToList();
        var allLevels = levelIds.Count > 0
            ? await _appearanceLevelRepository.GetListAsync(l => levelIds.Contains(l.Id) && (l.DeleteMark == null || l.DeleteMark == 0))
            : new List<AppearanceFeatureLevelEntity>();
        var levelIdToName = allLevels.ToDictionary(l => l.Id, l => l.Name ?? "");
        var featureIdToCategoryAndLevelName = allFeatures.ToDictionary(
            f => f.Id,
            f => (CategoryId: f.CategoryId ?? "", LevelName: !string.IsNullOrEmpty(f.SeverityLevelId) && levelIdToName.TryGetValue(f.SeverityLevelId, out var n) ? n : ""));

        // 2.7 加载导出范围内的单元格颜色（按产品规格分组查询后合并）
        var colorMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var specGroups = dataList
            .GroupBy(d => d.ContainsKey("productSpecId") ? d["productSpecId"]?.ToString() ?? "" : "")
            .Where(g => !string.IsNullOrEmpty(g.Key));
        foreach (var specGroup in specGroups)
        {
            var productSpecId = specGroup.Key;
            var ids = specGroup.Where(d => d.ContainsKey("id") && d["id"] != null).Select(d => d["id"].ToString()).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();
            if (ids.Count == 0) continue;
            var colorDto = await _colorService.GetColors(new GetIntermediateDataColorInput { ProductSpecId = productSpecId, IntermediateDataIds = ids });
            if (colorDto?.Colors != null)
            {
                foreach (var cell in colorDto.Colors)
                {
                    if (string.IsNullOrEmpty(cell.IntermediateDataId) || string.IsNullOrEmpty(cell.FieldName)) continue;
                    var key = $"{cell.IntermediateDataId}::{cell.FieldName}";
                    colorMap[key] = cell.ColorValue ?? "";
                }
            }
        }

        // 3. 按月份+产品规格分组
        // 字典中的 key 是 camelCase: prodDate, productSpecId, productSpecCode, productSpecName
        var groupedData = dataList
            .GroupBy(d => new
            {
                // 解析 ProdDate
                Year = d.ContainsKey("prodDate") && DateTime.TryParse(d["prodDate"]?.ToString(), out var dt) ? dt.Year : 0,
                Month = d.ContainsKey("prodDate") && DateTime.TryParse(d["prodDate"]?.ToString(), out var dt2) ? dt2.Month : 0,

                ProductSpecId = d.ContainsKey("productSpecId") ? d["productSpecId"]?.ToString() : "",
                ProductSpecCode = d.ContainsKey("productSpecCode") ? d["productSpecCode"]?.ToString() ?? "未知" : "未知",
                ProductSpecName = d.ContainsKey("productSpecName") ? d["productSpecName"]?.ToString() ?? "未知规格" : "未知规格"
            })
            .Select(g =>
            {
                // 获取检测列数 (从第一条数据获取 detectionColumns 属性，如果没有则默认15)
                var first = g.FirstOrDefault();
                int detectionColumns = 15;
                if (first != null && first.ContainsKey("detectionColumns") && first["detectionColumns"] != null)
                {
                    int.TryParse(first["detectionColumns"].ToString(), out detectionColumns);
                }

                return new
                {
                    SheetName = $"{g.Key.Month}月{g.Key.ProductSpecName}",
                    MonthOrder = g.Key.Month,
                    ProductSpecCode = g.Key.ProductSpecCode,
                    DetectionColumns = detectionColumns,
                    DataList = g.ToList()
                };
            })
            .OrderBy(r => r.MonthOrder)
            .ThenBy(r => r.ProductSpecCode)
            .ToList();

        // 4. 生成Excel
        var memoryStream = new MemoryStream();
        var sheets = new Dictionary<string, object>();
        var sheetRowData = new Dictionary<string, List<(string Id, string ProductSpecId)>>();
        var firstDetectionColumns = groupedData.Count > 0 ? groupedData[0].DetectionColumns : 15;
        var columnFieldNames = BuildColumnFieldNames(firstDetectionColumns, sortedCategories);

        foreach (var group in groupedData)
        {
            var dataTable = BuildDataTableFromDict(
                group.DataList,
                group.DetectionColumns,
                categoryMap,
                sortedCategories,
                featureIdToCategoryAndLevelName);

            // Sheet名称不能超过31个字符，且不能包含特殊字符
            // 移除导入文件名称可能包含的前缀 "_______"
            var sheetName = group.SheetName.Replace("_______", "");

            var safeSheetName = sheetName.Replace(":", "").Replace("\\", "").Replace("/", "").Replace("?", "").Replace("*", "").Replace("[", "").Replace("]", "");
            if (safeSheetName.Length > 30) safeSheetName = safeSheetName.Substring(0, 30);

            // 如果名称重复，添加后缀
            int suffix = 1;
            var originalName = safeSheetName;
            while (sheets.ContainsKey(safeSheetName))
            {
                safeSheetName = $"{originalName}_{suffix++}";
            }

            sheets[safeSheetName] = dataTable;
            var rowData = group.DataList
                .Select(d => (
                    Id: d.ContainsKey("id") ? d["id"]?.ToString() ?? "" : "",
                    ProductSpecId: d.ContainsKey("productSpecId") ? d["productSpecId"]?.ToString() ?? "" : ""))
                .ToList();
            sheetRowData[safeSheetName] = rowData;
        }

        // 使用配置禁用自动筛选
        var config = new MiniExcelLibs.OpenXml.OpenXmlConfiguration
        {
            AutoFilter = false
        };
        memoryStream.SaveAs(sheets, printHeader: false, configuration: config);
        memoryStream.Position = 0;

        // Post-process with NPOI merged cells and cell fill colors
        var finalStream = PostProcessExcel(memoryStream, sheetRowData, columnFieldNames, colorMap);

        var fileName = $"{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        return new FileStreamResult(finalStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        {
            FileDownloadName = fileName
        };
    }

    /// <summary>
    /// 判断导出列是否为数值列（用于将文本型数字转为数值，避免 Excel 提示“该数字是文本类型”）.
    /// </summary>
    private static bool IsNumericExportColumn(string fieldName)
    {
        if (string.IsNullOrEmpty(fieldName)) return false;
        if (fieldName.StartsWith("perf", StringComparison.OrdinalIgnoreCase)) return true;
        if (fieldName == "oneMeterWeight" || fieldName == "width" || fieldName == "thicknessDiff"
            || fieldName == "density" || fieldName == "laminationFactor" || fieldName == "breakCount"
            || fieldName == "singleCoilWeight" || fieldName == "avgThickness" || fieldName == "coilWeight"
            || fieldName == "maxThicknessRaw" || fieldName == "maxAvgThickness" || fieldName == "stripType")
            return true;
        if (fieldName.StartsWith("thickness", StringComparison.OrdinalIgnoreCase) && fieldName != "thicknessRange") return true;
        if (fieldName.StartsWith("detection", StringComparison.OrdinalIgnoreCase)) return true;
        if (fieldName == "midSiLeft" || fieldName == "midSiRight" || fieldName == "midBLeft" || fieldName == "midBRight") return true;
        if (fieldName == "leftPatternWidth" || fieldName == "leftPatternSpacing" || fieldName == "midPatternWidth"
            || fieldName == "midPatternSpacing" || fieldName == "rightPatternWidth" || fieldName == "rightPatternSpacing")
            return true;
        return false;
    }

    /// <summary>
    /// 构建列索引对应的前端字段名列表（用于颜色映射）.
    /// </summary>
    private static List<string> BuildColumnFieldNames(int detectionColumns, List<KeyValuePair<string, string>> sortedCategories)
    {
        var list = new List<string>
        {
            "detectionDateStr", "sprayNo", "labeling", "furnaceNoFormatted",
            "perfSsPower", "perfPsLoss", "perfHc", "perfAfterSsPower", "perfAfterPsLoss", "perfAfterHc", "perfEditorName",
            "oneMeterWeight", "width"
        };
        for (int i = 1; i <= detectionColumns; i++)
            list.Add($"thickness{i}");
        list.Add("thicknessMin");
        list.Add(null); // 分隔符 ~ 无字段
        list.Add("thicknessMax");
        list.Add("thicknessDiff");
        list.Add("density");
        list.Add("laminationFactor");
        for (int i = 0; i < sortedCategories.Count; i++)
            list.Add("appearanceFeatureList");
        list.Add("breakCount");
        list.Add("singleCoilWeight");
        list.Add("appearEditorName");
        list.Add("avgThickness");
        list.Add("magneticResult");
        list.Add("thicknessResult");
        list.Add("laminationResult");
        list.Add("midSiLeft");
        list.Add("midSiRight");
        list.Add("midBLeft");
        list.Add("midBRight");
        list.Add("leftPatternWidth");
        list.Add("leftPatternSpacing");
        list.Add("midPatternWidth");
        list.Add("midPatternSpacing");
        list.Add("rightPatternWidth");
        list.Add("rightPatternSpacing");
        list.Add("coilWeight");
        for (int i = 1; i <= detectionColumns; i++)
            list.Add($"detection{i}");
        list.Add("maxThicknessRaw");
        list.Add("maxAvgThickness");
        list.Add("creatorUserName");
        list.Add("stripType");
        list.Add("firstInspection");
        list.Add("shiftNo");
        return list;
    }

    /// <summary>
    /// 使用 NPOI 后处理 Excel，应用合并单元格、样式和单元格填充颜色.
    /// </summary>
    private MemoryStream PostProcessExcel(
        MemoryStream sourceStream,
        Dictionary<string, List<(string Id, string ProductSpecId)>> sheetRowData,
        List<string> columnFieldNames,
        Dictionary<string, string> colorMap)
    {
        var workbook = new XSSFWorkbook(sourceStream);
        var style = workbook.CreateCellStyle();
        style.Alignment = HorizontalAlignment.Center;
        style.VerticalAlignment = VerticalAlignment.Center;
        style.BorderBottom = BorderStyle.Thin;
        style.BorderLeft = BorderStyle.Thin;
        style.BorderRight = BorderStyle.Thin;
        style.BorderTop = BorderStyle.Thin;

        // 定义需要垂直合并（Row 0 & Row 1）的列名
        var verticalMergedColumns = new HashSet<string>
        {
            "检测日期", "喷次", "贴标", "炉号", "性能录入员",
            "一米带重(g)", "带宽 (mm)", "带厚极差",
            "密度 (g/cm³)", "叠片系数", "Hc (A/m)",
            "韧性", "脆边", "麻点", "划痕", "网眼", "毛边", "亮线", "劈裂", "棱", "龟裂纹",
            "断头数(个)", "单卷重量(kg)", "外观检验员",
            "平均厚度", "磁性能判定", "厚度判定", "叠片系数判定",
            "四米带重(g)", "最大厚度", "最大平均厚度",
            "录入人", "带型", "一次交检", "班次"
        };

        for (int i = 0; i < workbook.NumberOfSheets; i++)
        {
            var sheet = workbook.GetSheetAt(i);
            if (sheet == null) continue;

            // 取消冻结窗格
            sheet.CreateFreezePane(0, 0);

            // 0. 检查是否存在自动生成的表头 (C1, C2...)
            var firstRow = sheet.GetRow(0);
            if (firstRow != null)
            {
                var c0 = firstRow.GetCell(0)?.StringCellValue;
                if (c0 == "C1")
                {
                    // MiniExcel 生成了内部表头，删除它
                    sheet.ShiftRows(1, sheet.LastRowNum, -1);
                }
            }

            var row0 = sheet.GetRow(0); // 自定义的表头行1
            var row1 = sheet.GetRow(1); // 自定义的表头行2
            if (row0 == null) continue;

            // 注意: NPOI 的 SetAutoFilter 不支持 null 参数
            // 如果需要移除筛选，可以不调用此方法（MiniExcel默认不添加筛选）

            // 1. 处理所有行的样式 (内容垂直居中, 自动换行)
            var headerStyle = workbook.CreateCellStyle();
            headerStyle.Alignment = HorizontalAlignment.Center;
            headerStyle.VerticalAlignment = VerticalAlignment.Center;
            headerStyle.BorderBottom = BorderStyle.Thin;
            headerStyle.BorderLeft = BorderStyle.Thin;
            headerStyle.BorderRight = BorderStyle.Thin;
            headerStyle.BorderTop = BorderStyle.Thin;
            headerStyle.WrapText = true; // 自动换行

            var fillStyleCache = new Dictionary<string, ICellStyle>();
            ICellStyle GetOrCreateFillStyle(string hexColor)
            {
                if (string.IsNullOrEmpty(hexColor) || hexColor.Length < 6) return style;
                if (fillStyleCache.TryGetValue(hexColor, out var cached)) return cached;
                var fillStyle = (XSSFCellStyle)workbook.CreateCellStyle();
                fillStyle.Alignment = HorizontalAlignment.Center;
                fillStyle.VerticalAlignment = VerticalAlignment.Center;
                fillStyle.BorderBottom = BorderStyle.Thin;
                fillStyle.BorderLeft = BorderStyle.Thin;
                fillStyle.BorderRight = BorderStyle.Thin;
                fillStyle.BorderTop = BorderStyle.Thin;
                fillStyle.FillPattern = FillPattern.SolidForeground;
                try
                {
                    var hex = hexColor.TrimStart('#');
                    if (hex.Length == 6)
                    {
                        var rb = Convert.ToByte(hex.Substring(0, 2), 16);
                        var g = Convert.ToByte(hex.Substring(2, 2), 16);
                        var b = Convert.ToByte(hex.Substring(4, 2), 16);
                        fillStyle.SetFillForegroundColor(new XSSFColor(new byte[] { rb, g, b }, null));
                    }
                }
                catch { /* ignore invalid hex */ }
                fillStyleCache[hexColor] = fillStyle;
                return fillStyle;
            }

            for (int r = 0; r <= sheet.LastRowNum; r++)
            {
                var row = sheet.GetRow(r);
                if (row == null) continue;

                // 设置行高: Row 0=20, Row 1=30, 数据行=20
                if (r == 0)
                    row.HeightInPoints = 20;
                else if (r == 1)
                    row.HeightInPoints = 30;
                else
                    row.HeightInPoints = 20;

                List<(string Id, string ProductSpecId)> rowDataList = null;
                var isDataRow = r >= 2 && sheetRowData != null && columnFieldNames != null && colorMap != null
                    && sheetRowData.TryGetValue(sheet.SheetName, out rowDataList);
                var dataRowIndex = r - 2;
                var rowId = isDataRow && rowDataList != null && dataRowIndex >= 0 && dataRowIndex < rowDataList.Count
                    ? rowDataList[dataRowIndex].Id
                    : null;

                for (int c = 0; c < row.LastCellNum; c++)
                {
                    var cell = row.GetCell(c);
                    if (cell != null)
                    {
                        if (r <= 1)
                        {
                            cell.CellStyle = headerStyle;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(rowId) && c < columnFieldNames.Count)
                            {
                                var fieldName = columnFieldNames[c];
                                if (!string.IsNullOrEmpty(fieldName) && colorMap != null)
                                {
                                    var key = $"{rowId}::{fieldName}";
                                    if (colorMap.TryGetValue(key, out var colorValue) && !string.IsNullOrEmpty(colorValue))
                                    {
                                        cell.CellStyle = GetOrCreateFillStyle(colorValue);
                                        ConvertTextToNumericIfApplicable(r, cell, c);
                                        continue;
                                    }
                                }
                            }
                            cell.CellStyle = style;
                            ConvertTextToNumericIfApplicable(r, cell, c);
                        }
                    }
                }
            }

            void ConvertTextToNumericIfApplicable(int rowIndex, ICell cell, int columnIndex)
            {
                if (rowIndex < 2 || columnFieldNames == null || columnIndex >= columnFieldNames.Count) return;
                var fieldName = columnFieldNames[columnIndex];
                if (!IsNumericExportColumn(fieldName)) return;
                if (cell.CellType != CellType.String) return;
                var s = cell.StringCellValue?.Trim();
                if (string.IsNullOrEmpty(s)) return;
                if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var num))
                    cell.SetCellValue(num);
            }

            // 强制合并第一列 (检测日期) - Row 0 & Row 1
            try
            {
                sheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(0, 1, 0, 0));
            }
            catch { /* Already merged */ }

            // 2. 表头纵向合并 (Vertical Merge) - Row 0 & Row 1
            for (int c = 0; c < row0.LastCellNum; c++)
            {
                var cell0 = row0.GetCell(c);
                if (cell0 != null)
                {
                    var val = cell0.StringCellValue?.Trim();
                    if (!string.IsNullOrEmpty(val) && verticalMergedColumns.Contains(val))
                    {
                        var range = new NPOI.SS.Util.CellRangeAddress(0, 1, c, c);
                        sheet.AddMergedRegion(range);
                    }
                }
            }

            // 3. 表头横向合并 (Horizontal Merge) - Row 0
            // 自动检测 Row 0 中连续相同的非空值
            int mergeStart = -1;
            string currentVal = null;

            for (int c = 0; c <= row0.LastCellNum; c++)
            {
                string val = null;
                bool isVertical = false;

                if (c < row0.LastCellNum)
                {
                    var cell = row0.GetCell(c);
                    val = cell?.StringCellValue?.Trim();
                    if (val != null && verticalMergedColumns.Contains(val)) isVertical = true;
                }

                // 如果遇到垂直合并列，或者值改变了，或者到了末尾 -> 结束之前的合并
                if (isVertical || val != currentVal || c == row0.LastCellNum)
                {
                    if (mergeStart != -1 && currentVal != null && (c - mergeStart) > 1)
                    {
                        // 检查是否是需要 Block Merge (2行合并) 的大标题
                        // 只有 "带厚" 需要 Block Merge，"叠片系数厚度分布" 保留 1-N
                        if (currentVal == "带厚")
                        {
                            var range = new NPOI.SS.Util.CellRangeAddress(0, 1, mergeStart, c - 1);
                            sheet.AddMergedRegion(range);
                        }
                        else
                        {
                            // 普通横向合并 (Row 0)
                            var range = new NPOI.SS.Util.CellRangeAddress(0, 0, mergeStart, c - 1);
                            sheet.AddMergedRegion(range);
                        }
                    }

                    if (isVertical)
                    {
                        mergeStart = -1;
                        currentVal = null;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(val))
                        {
                            mergeStart = c;
                            currentVal = val;
                        }
                        else
                        {
                            mergeStart = -1;
                            currentVal = null;
                        }
                    }
                }
            }

            // 4. 自动调整列宽 (对所有列执行)
            for (int c = 0; c < row0.LastCellNum; c++)
            {
                sheet.AutoSizeColumn(c);
                // 对某些窄列设置最小宽度
                // NPOI 宽度单位是 1/256 字符宽度
                if (sheet.GetColumnWidth(c) < 2500)
                {
                    sheet.SetColumnWidth(c, 2500); // 约10字符宽
                }
            }
        }

        var outputStream = new MemoryStream();
        workbook.Write(outputStream);
        return new MemoryStream(outputStream.ToArray());
    }

    /// <summary>
    /// 构建 DataTable (从字典列表).
    /// </summary>
    private DataTable BuildDataTableFromDict(
        List<Dictionary<string, object>> dataList,
        int detectionColumns,
        Dictionary<string, string> categoryMap,
        List<KeyValuePair<string, string>> sortedCategories,
        Dictionary<string, (string CategoryId, string LevelName)> featureIdToCategoryAndLevelName)
    {
        var dataTable = new DataTable();

        // 辅助方法：添加列
        void AddCol(string name) => dataTable.Columns.Add(name, typeof(object));

        // 定义所有列
        // 1. 基础信息
        AddCol("C1"); // 检验日期
        AddCol("C2"); // 喷次
        AddCol("C3"); // 贴标
        AddCol("C4"); // 炉号

        // 2. 性能 (1.35T: Ss; 50Hz: Ps; Hc; 刻痕后: Ss, Ps, Hc)
        AddCol("C5"); // 1.35T -> Ss
        AddCol("C6"); // 50Hz -> Ps
        AddCol("C7"); // Hc
        AddCol("C8"); // 刻痕后 -> Ss
        AddCol("C9"); // 刻痕后 -> Ps
        AddCol("C10"); // 刻痕后 -> Hc
        AddCol("C11"); // 性能录入员

        // 3. 物理
        AddCol("C12"); // 一米带重
        AddCol("C13"); // 带宽

        // 4. 带厚 (1..N)
        for (int i = 1; i <= detectionColumns; i++) AddCol($"Thick{i}");

        // 5. 带厚范围 (改为3列: 最小值, ~, 最大值)
        AddCol("C_ThickMin");
        AddCol("C_ThickSep"); // 分隔符 ~
        AddCol("C_ThickMax");

        // 6. 规格与物理特性
        AddCol("C_Diff"); // 极差
        AddCol("C_Density"); // 密度
        AddCol("C_Lam"); // 叠片系数

        // 7. 外观缺陷 (动态列，与 sortedCategories 顺序一致)
        foreach (var kvp in sortedCategories) AddCol($"Appear_{kvp.Key}");

        // 8. 外观固定字段
        AddCol("C_Break"); // 断头数
        AddCol("C_CoilW"); // 单卷重
        AddCol("C_AppearUser"); // 外观员

        // 9. 汇总判定
        AddCol("C_AvgThick"); // 平均厚度
        AddCol("C_MagRes"); // 磁性能判定
        AddCol("C_ThickRes"); // 厚度判定
        AddCol("C_LamRes"); // 叠片判定

        // 10. 花纹 (中Si: L, R; 中B: L, R; 左: W, S; 中: W, S; 右: W, S)
        AddCol("C_MidSiL");
        AddCol("C_MidSiR");
        AddCol("C_MidBL");
        AddCol("C_MidBR");
        AddCol("C_LPW");
        AddCol("C_LPS");
        AddCol("C_MPW");
        AddCol("C_MPS");
        AddCol("C_RPW");
        AddCol("C_RPS");

        // 11. 四米带重
        AddCol("C_FullW");

        // 12. 叠片分布 (1..N)
        for (int i = 1; i <= detectionColumns; i++) AddCol($"LamDist{i}");

        // 13. 其他
        AddCol("C_MaxThick"); // 最大厚度
        AddCol("C_MaxAvg"); // 最大平均
        AddCol("C_Creator"); // 录入人
        AddCol("C_Type"); // 带型
        AddCol("C_FirstInsp"); // 一次交检
        AddCol("C_Class"); // 班次

        // --- 构建表头行 1 ---
        var r1 = dataTable.NewRow();
        int c = 0;

        // 1.
        r1[c++] = "检验日期";
        r1[c++] = "喷次";
        r1[c++] = "贴标";
        r1[c++] = "炉号";

        // 2. 性能 (Groups)
        r1[c++] = "1.35T"; // Ss
        r1[c++] = "50Hz"; // Ps
        r1[c++] = "Hc (A/m)"; // Hc
        r1[c++] = "刻痕后性能"; // Ss
        r1[c++] = "刻痕后性能"; // Ps
        r1[c++] = "刻痕后性能"; // Hc
        r1[c++] = "性能录入员";

        // 3.
        r1[c++] = "一米带重(g)";
        r1[c++] = "带宽 (mm)";

        // 4. 带厚 Group
        for (int i = 1; i <= detectionColumns; i++) r1[c++] = "带厚";

        // 5. 带厚范围 (3列)
        r1[c++] = "带厚范围";
        r1[c++] = "带厚范围";
        r1[c++] = "带厚范围";

        // 6.
        r1[c++] = "带厚极差";
        r1[c++] = "密度 (g/cm³)";
        r1[c++] = "叠片系数";

        // 7. 外观缺陷 (Top level，与 sortedCategories 顺序一致)
        foreach (var kvp in sortedCategories) r1[c++] = kvp.Value;

        // 8.
        r1[c++] = "断头数(个)";
        r1[c++] = "单卷重量(kg)";
        r1[c++] = "外观检验员";

        // 9.
        r1[c++] = "平均厚度";
        r1[c++] = "磁性能判定";
        r1[c++] = "厚度判定";
        r1[c++] = "叠片系数判定";

        // 10. 花纹 Groups
        r1[c++] = "中Si";
        r1[c++] = "中Si";
        r1[c++] = "中B";
        r1[c++] = "中B";
        r1[c++] = "左花纹";
        r1[c++] = "左花纹";
        r1[c++] = "中花纹";
        r1[c++] = "中花纹";
        r1[c++] = "右花纹";
        r1[c++] = "右花纹";

        // 11.
        r1[c++] = "四米带重(g)";

        // 12. 叠片分布 Group
        for (int i = 1; i <= detectionColumns; i++) r1[c++] = "叠片系数厚度分布";

        // 13.
        r1[c++] = "最大厚度";
        r1[c++] = "最大平均厚度";
        r1[c++] = "录入人";
        r1[c++] = "带型";
        r1[c++] = "一次交检";
        r1[c++] = "班次";

        dataTable.Rows.Add(r1);

        // --- 构建表头行 2 ---
        var r2 = dataTable.NewRow();
        c = 0;

        // 1. (Empty for span)
        r2[c++] = "";
        r2[c++] = "";
        r2[c++] = "";
        r2[c++] = "";

        // 2.
        r2[c++] = "Ss激磁功率 (VA/kg)"; // 1.35T child
        r2[c++] = "Ps铁损 (W/kg)"; // 50Hz child
        r2[c++] = ""; // Hc (span)
        r2[c++] = "Ss激磁功率 (VA/kg)"; // After child
        r2[c++] = "Ps铁损 (W/kg)";
        r2[c++] = "Hc (A/m)";
        r2[c++] = ""; // Editor

        // 3.
        r2[c++] = "";
        r2[c++] = "";

        // 4. 带厚 N
        for (int i = 1; i <= detectionColumns; i++) r2[c++] = ""; // 合并成大标题，不需要 1..N

        // 5. 带厚范围子标题 (3列)
        r2[c++] = "最小值";
        r2[c++] = "~";
        r2[c++] = "最大值";

        // 6.
        r2[c++] = "";
        r2[c++] = "";
        r2[c++] = "";

        // 7. 外观 (Empty for span)
        foreach (var kvp in sortedCategories) r2[c++] = "";

        // 8.
        r2[c++] = "";
        r2[c++] = "";
        r2[c++] = "";

        // 9.
        r2[c++] = "";
        r2[c++] = "";
        r2[c++] = "";
        r2[c++] = "";

        // 10. 花纹
        r2[c++] = "左";
        r2[c++] = "右";
        r2[c++] = "左";
        r2[c++] = "右";
        r2[c++] = "纹宽";
        r2[c++] = "纹距";
        r2[c++] = "纹宽";
        r2[c++] = "纹距";
        r2[c++] = "纹宽";
        r2[c++] = "纹距";

        // 11.
        r2[c++] = "";

        // 12. 叠片 N
        for (int i = 1; i <= detectionColumns; i++) r2[c++] = i.ToString();

        // 13.
        r2[c++] = "";
        r2[c++] = "";
        r2[c++] = "";
        r2[c++] = "";
        r2[c++] = "";
        r2[c++] = "";

        dataTable.Rows.Add(r2);

        // --- 填充数据 ---
        foreach (var d in dataList)
        {
            var row = dataTable.NewRow();
            c = 0;
            object Get(string k) => d.ContainsKey(k) ? d[k] : null;

            // 1.
            row[c++] = Get("detectionDateStr") ?? (d.ContainsKey("detectionDate") ? ((DateTime?)d["detectionDate"])?.ToString("yyyy-MM-dd") : null);
            row[c++] = Get("sprayNo");
            row[c++] = Get("labeling");
            row[c++] = Get("furnaceNoFormatted");

            // 2.
            row[c++] = Get("perfSsPower");
            row[c++] = Get("perfPsLoss");
            row[c++] = Get("perfHc");
            row[c++] = Get("perfAfterSsPower");
            row[c++] = Get("perfAfterPsLoss");
            row[c++] = Get("perfAfterHc");
            row[c++] = Get("perfEditorName");

            // 3.
            row[c++] = Get("oneMeterWeight");
            row[c++] = Get("width");

            // 4.
            for (int i = 1; i <= detectionColumns; i++) row[c++] = Get($"thickness{i}");

            // 5. 带厚范围 (3列: 最小值, ~, 最大值)
            row[c++] = Get("thicknessMin");
            row[c++] = "~";
            row[c++] = Get("thicknessMax");

            // 6.
            row[c++] = Get("thicknessDiff");
            row[c++] = Get("density");
            row[c++] = Get("laminationFactor");

            // 7. 外观特性动态列：导出特性等级名称（如 轻、中、重），不再导出 ✓
            var categoryIdToLevelNames = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            if (d.ContainsKey("appearanceFeatureIds") && d["appearanceFeatureIds"] != null)
            {
                try
                {
                    var idsJson = d["appearanceFeatureIds"].ToString();
                    if (!string.IsNullOrEmpty(idsJson))
                    {
                        var featureIds = JsonSerializer.Deserialize<List<string>>(idsJson);
                        if (featureIds != null && featureIdToCategoryAndLevelName != null)
                        {
                            foreach (var fid in featureIds)
                            {
                                if (string.IsNullOrEmpty(fid) || !featureIdToCategoryAndLevelName.TryGetValue(fid, out var catLevel)) continue;
                                if (string.IsNullOrEmpty(catLevel.CategoryId)) continue;
                                if (!categoryIdToLevelNames.TryGetValue(catLevel.CategoryId, out var list))
                                {
                                    list = new List<string>();
                                    categoryIdToLevelNames[catLevel.CategoryId] = list;
                                }
                                if (!string.IsNullOrEmpty(catLevel.LevelName) && !list.Contains(catLevel.LevelName))
                                    list.Add(catLevel.LevelName);
                            }
                        }
                    }
                }
                catch { }
            }
            foreach (var kvp in sortedCategories)
            {
                var levelNames = categoryIdToLevelNames.TryGetValue(kvp.Key, out var names) && names != null && names.Count > 0
                    ? string.Join("/", names)
                    : "";
                row[c++] = levelNames;
            }

            // 8.
            row[c++] = Get("breakCount");
            row[c++] = Get("singleCoilWeight");
            row[c++] = Get("appearEditorName");

            // 9.
            row[c++] = Get("avgThickness");
            row[c++] = Get("magneticResult");
            row[c++] = Get("thicknessResult");
            row[c++] = Get("laminationResult");

            // 10.
            row[c++] = Get("midSiLeft");
            row[c++] = Get("midSiRight");
            row[c++] = Get("midBLeft");
            row[c++] = Get("midBRight");
            row[c++] = Get("leftPatternWidth");
            row[c++] = Get("leftPatternSpacing");
            row[c++] = Get("midPatternWidth");
            row[c++] = Get("midPatternSpacing");
            row[c++] = Get("rightPatternWidth");
            row[c++] = Get("rightPatternSpacing");

            // 11.
            row[c++] = Get("coilWeight");

            // 12.
            for (int i = 1; i <= detectionColumns; i++) row[c++] = Get($"detection{i}");

            // 13.
            row[c++] = Get("maxThicknessRaw");
            row[c++] = Get("maxAvgThickness");
            row[c++] = Get("creatorUserName") ?? Get("creatorUserId");
            row[c++] = Get("stripType");
            row[c++] = Get("firstInspection");
            row[c++] = Get("shiftNo");

            dataTable.Rows.Add(row);
        }

        return dataTable;
    }

    /// <summary>
    /// 获取计算状态文本.
    /// </summary>
    private string GetCalcStatusText(IntermediateDataCalcStatus status)
    {
        return status switch
        {
            IntermediateDataCalcStatus.PENDING => "未计算",
            IntermediateDataCalcStatus.PROCESSING => "计算中",
            IntermediateDataCalcStatus.SUCCESS => "成功",
            IntermediateDataCalcStatus.FAILED => "失败",
            _ => "未知"
        };
    }
}
