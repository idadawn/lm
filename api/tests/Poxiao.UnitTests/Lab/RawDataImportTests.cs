using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MiniExcelLibs;
using Moq;
using Poxiao.Infrastructure.Core.Manager;
using Poxiao.Infrastructure.Core.Manager.Files;
using Poxiao.Infrastructure.Security;
using Poxiao.Lab.Entity;
using Poxiao.Lab.Interfaces;
using Poxiao.Lab.Service;
using Poxiao.Systems.Interfaces.Common;
using SqlSugar;
using Xunit;

namespace Poxiao.UnitTests.Lab
{
    public class RawDataImportTests
    {
        private readonly Mock<ISqlSugarRepository<RawDataImportSessionEntity>> _mockSessionRepo;
        private readonly Mock<ISqlSugarRepository<RawDataEntity>> _mockRawDataRepo;
        private readonly Mock<ISqlSugarRepository<ProductSpecEntity>> _mockProductSpecRepo;
        private readonly Mock<ISqlSugarRepository<RawDataImportLogEntity>> _mockLogRepo;
        private readonly Mock<ISqlSugarRepository<AppearanceFeatureEntity>> _mockFeatureRepo;
        private readonly Mock<
            ISqlSugarRepository<AppearanceFeatureCategoryEntity>
        > _mockCategoryRepo;
        private readonly Mock<ISqlSugarRepository<AppearanceFeatureLevelEntity>> _mockFeatureLevelRepo;
        private readonly Mock<IFileService> _mockFileService;
        private readonly Mock<IUserManager> _mockUserManager;
        private readonly Mock<IIntermediateDataService> _mockIntermediateDataService;
        private readonly Mock<IAppearanceFeatureService> _mockAppearanceFeatureService;
        private readonly Mock<ISqlSugarRepository<ProductSpecAttributeEntity>> _mockSpecAttrRepo;
        private readonly Mock<IFileManager> _mockFileManager;
        private readonly Mock<IRawDataValidationService> _mockValidationService;
        private readonly Mock<ISqlSugarRepository<ExcelImportTemplateEntity>> _mockExcelTemplateRepo;

        private readonly RawDataImportSessionService _service;

        public RawDataImportTests()
        {
            _mockSessionRepo = new Mock<ISqlSugarRepository<RawDataImportSessionEntity>>();
            _mockRawDataRepo = new Mock<ISqlSugarRepository<RawDataEntity>>();
            _mockProductSpecRepo = new Mock<ISqlSugarRepository<ProductSpecEntity>>();
            _mockLogRepo = new Mock<ISqlSugarRepository<RawDataImportLogEntity>>();
            _mockFeatureRepo = new Mock<ISqlSugarRepository<AppearanceFeatureEntity>>();
            _mockCategoryRepo = new Mock<ISqlSugarRepository<AppearanceFeatureCategoryEntity>>();
            _mockFeatureLevelRepo = new Mock<ISqlSugarRepository<AppearanceFeatureLevelEntity>>();
            _mockFileService = new Mock<IFileService>();
            _mockUserManager = new Mock<IUserManager>();
            _mockIntermediateDataService = new Mock<IIntermediateDataService>();
            _mockAppearanceFeatureService = new Mock<IAppearanceFeatureService>();
            _mockSpecAttrRepo = new Mock<ISqlSugarRepository<ProductSpecAttributeEntity>>();
            _mockFileManager = new Mock<IFileManager>();
            _mockValidationService = new Mock<IRawDataValidationService>();
            _mockExcelTemplateRepo = new Mock<ISqlSugarRepository<ExcelImportTemplateEntity>>();

            // Setup Product Spec Mock
            _mockProductSpecRepo.Setup(x => x.GetList()).Returns(new List<ProductSpecEntity>());

        }

        [Fact]
        public void ParseExcel_WithMiniExcel_ShouldParseCorrectly()
        {
            // Arrange
            var testData = new[]
            {
                new
                {
                    日期 = DateTime.Now,
                    炉号 = "1甲20251101-1-4-1",
                    宽度 = 1200.5,
                    带材重量 = 5000.1,
                    检测1 = 10.1,
                    检测2 = 20.2,
                },
                new
                {
                    日期 = DateTime.Now,
                    炉号 = "1甲20251101-1-4-2",
                    宽度 = 1200.5,
                    带材重量 = 5000.2,
                    检测1 = 10.3,
                    检测2 = 20.4,
                },
            };

            byte[] fileBytes;
            using (var stream = new MemoryStream())
            {
                stream.SaveAs(testData);
                fileBytes = stream.ToArray();
            }

            // Act
            // Use reflection to call private method
            var methodInfo = typeof(RawDataImportSessionService).GetMethod(
                "ParseExcel",
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            Assert.NotNull(methodInfo);

            var result =
                methodInfo.Invoke(_service, new object[] { fileBytes, "test.xlsx", 0 })
                as List<RawDataEntity>;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            var row1 = result[0];
            Assert.Equal("1甲20251101-1-4-1", row1.FurnaceNo);
            Assert.Equal(1200.5m, row1.Width);
            Assert.Equal(5000.1m, row1.CoilWeight);
        }

        [Fact]
        public void ParseExcel_ShouldHandleDynamicColumns()
        {
            // Arrange
            var testData = new List<Dictionary<string, object>>();
            var row = new Dictionary<string, object>();
            row["日期"] = DateTime.Now;
            row["炉号"] = "1甲20251101-1-4-1";
            row["宽度"] = 1000;
            // Mixed Headers
            row["1"] = 11.1; // Column 1
            row["检测2"] = 22.2; // Column 2
            row["列3"] = 33.3; // Column 3
            testData.Add(row);

            byte[] fileBytes;
            using (var stream = new MemoryStream())
            {
                stream.SaveAs(testData);
                fileBytes = stream.ToArray();
            }

            // Act
            var methodInfo = typeof(RawDataImportSessionService).GetMethod(
                "ParseExcel",
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            var result =
                methodInfo.Invoke(_service, new object[] { fileBytes, "test.xlsx", 0 })
                as List<RawDataEntity>;

            // Assert
            Assert.NotNull(result);
            var entity = result[0];
        }
    }
}
