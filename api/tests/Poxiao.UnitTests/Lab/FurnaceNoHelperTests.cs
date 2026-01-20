using System;
using Poxiao.Lab.Entity.Models;
using Poxiao.Lab.Helpers;
using Xunit;

namespace Poxiao.UnitTests.Lab
{
    public class FurnaceNoHelperTests
    {
        [Theory]
        [InlineData("1甲20251101-1-4-1", true, "甲")]
        [InlineData("1乙20251101-1-4-1", true, "乙")]
        [InlineData("1丙20251101-1-4-1", true, "丙")]
        [InlineData("1A20251101-1-4-1", true, "A")]
        [InlineData("1B20251101-1-4-1", true, "B")]
        [InlineData("1Night20251101-1-4-1", true, "Night")]
        [InlineData("1-20251101-1-4-1", true, "-")] // Even just a hyphen if regex allows
        public void ParseFurnaceNo_Should_Succeed_For_Various_Shifts(
            string furnaceNo,
            bool expectedSuccess,
            string expectedShift
        )
        {
            // Act
            var result = FurnaceNoHelper.ParseFurnaceNo(furnaceNo);

            // Assert
            if (expectedSuccess)
            {
                Assert.True(
                    result.Success,
                    $"Expected success for {furnaceNo}, but failed: {result.ErrorMessage}"
                );
                Assert.Contains(expectedShift, result.Shift);
                Assert.NotNull(result.ProdDate);
            }
            else
            {
                Assert.False(result.Success, $"Expected failure for {furnaceNo}, but succeeded.");
            }
        }

        [Fact]
        public void ParseFurnaceNo_Should_Fail_For_Invalid_Date()
        {
            // Act
            var result = FurnaceNoHelper.ParseFurnaceNo("1甲20251301-1-4-1"); // Invalid Month 13

            // Assert
            // Note: Currently ParseFurnaceNo uses Regex which only checks \d{8},
            // but then it uses DateTime.TryParseExact which will fail for 1301.
            Assert.False(result.Success);
            Assert.Contains("日期格式错误", result.ErrorMessage);
        }

        [Fact]
        public void ParseFurnaceNo_Should_Fail_For_Empty_Input()
        {
            var result = FurnaceNoHelper.ParseFurnaceNo("");
            Assert.False(result.Success);
            Assert.Equal("炉号为空", result.ErrorMessage);
        }

        [Theory]
        [InlineData("1乙20251101-1-1-1", true, "乙", 1, 1, 1)] // 用户提到的例子
        [InlineData("1甲20251101-1-1.5-1", true, "甲", 1, 1.5, 1)] // 卷号是小数
        [InlineData("1乙20251101-1-1-1.5", true, "乙", 1, 1, 1.5)] // 分卷号是小数
        [InlineData("1丙20251101-1-1.5-2.5", true, "丙", 1, 1.5, 2.5)] // 卷号和分卷号都是小数
        public void ParseFurnaceNo_Should_Support_Decimal_CoilAndSubcoil(
            string furnaceNo,
            bool expectedSuccess,
            string expectedShift,
            int expectedFurnaceNo,
            decimal expectedCoilNo,
            decimal expectedSubcoilNo
        )
        {
            // Act
            var result = FurnaceNoHelper.ParseFurnaceNo(furnaceNo);

            // Assert
            if (expectedSuccess)
            {
                Assert.True(
                    result.Success,
                    $"Expected success for {furnaceNo}, but failed: {result.ErrorMessage}"
                );
                Assert.Contains(expectedShift, result.Shift);
                Assert.NotNull(result.ProdDate);
                Assert.Equal(expectedFurnaceNo, result.FurnaceNoNumeric);
                Assert.Equal(expectedCoilNo, result.CoilNoNumeric);
                Assert.Equal(expectedSubcoilNo, result.SubcoilNoNumeric);
            }
            else
            {
                Assert.False(result.Success, $"Expected failure for {furnaceNo}, but succeeded.");
            }
        }

        [Fact]
        public void FurnaceNoClass_Should_Parse_Standard_FurnaceNo()
        {
            // Arrange
            var furnaceNoStr = "1甲20251101-1-4-1脆";

            // Act
            var furnaceNo = FurnaceNo.Parse(furnaceNoStr);

            // Assert
            Assert.True(furnaceNo.IsValid);
            Assert.Equal("1", furnaceNo.LineNo);
            Assert.Equal("甲", furnaceNo.Shift);
            Assert.Equal(new DateTime(2025, 11, 1), furnaceNo.ProdDate);
            Assert.Equal("1", furnaceNo.FurnaceBatchNo);
            Assert.Equal("4", furnaceNo.CoilNo);
            Assert.Equal("1", furnaceNo.SubcoilNo);
            Assert.Equal("脆", furnaceNo.FeatureSuffix);
        }

        [Fact]
        public void FurnaceNoClass_Should_Generate_Standard_FurnaceNo()
        {
            // Arrange
            var furnaceNo = FurnaceNo.Parse("1甲20251101-1-4-1脆");

            // Act
            var standardFurnaceNo = furnaceNo.GetStandardFurnaceNo();

            // Assert
            Assert.Equal("1甲20251101-1", standardFurnaceNo);
        }

        [Fact]
        public void FurnaceNoClass_Should_Generate_Base_FurnaceNo()
        {
            // Arrange
            var furnaceNo = FurnaceNo.Parse("1甲20251101-1-4-1脆");

            // Act
            var baseFurnaceNo = furnaceNo.GetFurnaceNo();

            // Assert
            Assert.Equal("1甲20251101-1-4-1", baseFurnaceNo);
        }

        [Fact]
        public void FurnaceNoClass_Should_Generate_Full_FurnaceNo()
        {
            // Arrange
            var furnaceNo = FurnaceNo.Parse("1甲20251101-1-4-1脆");

            // Act
            var fullFurnaceNo = furnaceNo.GetFullFurnaceNo();

            // Assert
            Assert.Equal("1甲20251101-1-4-1脆", fullFurnaceNo);
        }

        [Fact]
        public void FurnaceNoClass_Should_Build_From_Parts()
        {
            // Arrange & Act
            var furnaceNo = FurnaceNo.Build(
                lineNo: "2",
                shift: "乙",
                prodDate: new DateTime(2025, 11, 2),
                furnaceBatchNo: "3",
                coilNo: "5",
                subcoilNo: "2",
                featureSuffix: "硬"
            );

            // Assert
            Assert.True(furnaceNo.IsValid);
            Assert.Equal("2乙20251102-3-5-2硬", furnaceNo.GetFullFurnaceNo());
            Assert.Equal("2乙20251102-3", furnaceNo.GetStandardFurnaceNo());
        }

        [Fact]
        public void FurnaceNoClass_Should_Compare_Equality_By_Standard_FurnaceNo()
        {
            // Arrange
            var furnaceNo1 = FurnaceNo.Parse("1甲20251101-1-4-1脆");
            var furnaceNo2 = FurnaceNo.Parse("1甲20251101-1-5-2硬");
            var furnaceNo3 = FurnaceNo.Parse("2乙20251101-1-4-1脆");

            // Act & Assert
            Assert.Equal(furnaceNo1.GetStandardFurnaceNo(), furnaceNo2.GetStandardFurnaceNo());
            Assert.NotEqual(furnaceNo1.GetStandardFurnaceNo(), furnaceNo3.GetStandardFurnaceNo());
            Assert.True(furnaceNo1 == furnaceNo2);
            Assert.False(furnaceNo1 == furnaceNo3);
        }

        [Fact]
        public void FurnaceNoClass_Should_Remove_FeatureSuffix()
        {
            // Arrange
            var furnaceNo = FurnaceNo.Parse("1甲20251101-1-4-1脆");

            // Act
            var withoutFeature = furnaceNo.WithoutFeatureSuffix();

            // Assert
            Assert.True(withoutFeature.IsValid);
            Assert.Null(withoutFeature.FeatureSuffix);
            Assert.Equal("1甲20251101-1-4-1", withoutFeature.GetFullFurnaceNo());
        }

        [Fact]
        public void FurnaceNoClass_Should_Generate_SprayNo()
        {
            // Arrange
            var furnaceNo = FurnaceNo.Parse("1甲20251101-1-4-1脆");

            // Act
            var sprayNo = furnaceNo.GetSprayNo();

            // Assert
            Assert.Equal("20251101-1", sprayNo);
        }

        [Fact]
        public void FurnaceNoClass_Should_Generate_BatchNo()
        {
            // Arrange
            var furnaceNo = FurnaceNo.Parse("1甲20251101-1-4-1脆");

            // Act
            var batchNo = furnaceNo.GetBatchNo();

            // Assert
            Assert.Equal("1甲20251101-1", batchNo);
        }

        [Fact]
        public void FurnaceNoClass_SprayNo_Should_Be_Same_For_Same_Date_And_Furnace()
        {
            // Arrange
            var furnaceNo1 = FurnaceNo.Parse("1甲20251101-1-4-1脆");
            var furnaceNo2 = FurnaceNo.Parse("2乙20251101-1-5-2硬");

            // Act
            var sprayNo1 = furnaceNo1.GetSprayNo();
            var sprayNo2 = furnaceNo2.GetSprayNo();

            // Assert
            Assert.Equal(sprayNo1, sprayNo2);
            Assert.Equal("20251101-1", sprayNo1);
        }

        [Fact]
        public void FurnaceNoClass_BatchNo_Should_Be_Different_For_Different_Line_Or_Shift()
        {
            // Arrange
            var furnaceNo1 = FurnaceNo.Parse("1甲20251101-1-4-1脆");
            var furnaceNo2 = FurnaceNo.Parse("1乙20251101-1-4-1脆");
            var furnaceNo3 = FurnaceNo.Parse("2甲20251101-1-4-1脆");

            // Act
            var batchNo1 = furnaceNo1.GetBatchNo();
            var batchNo2 = furnaceNo2.GetBatchNo();
            var batchNo3 = furnaceNo3.GetBatchNo();

            // Assert
            Assert.NotEqual(batchNo1, batchNo2); // 不同班次
            Assert.NotEqual(batchNo1, batchNo3); // 不同产线
            Assert.Equal("1甲20251101-1", batchNo1);
            Assert.Equal("1乙20251101-1", batchNo2);
            Assert.Equal("2甲20251101-1", batchNo3);
        }
    }
}
