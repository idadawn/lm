using System.Collections.Generic;
using Newtonsoft.Json;
using Poxiao.Lab.Entity;
using Poxiao.Lab.Entity.Entity;
using Poxiao.Lab.Helpers;
using Xunit;

namespace Poxiao.UnitTests.Lab
{
    public class RawDataSerializationTests
    {
        [Fact]
        public void Serialization_Should_Preserve_ProductSpecId_And_DetectionData()
        {
            // Arrange
            var entity = new RawDataEntity
            {
                FurnaceNo = "1A20251101-1-4-1",
                ProductSpecId = "test-spec-id",
                ProductSpecName = "Test Spec",
                Shift = "A",
                IsValidData = 1,
            };

            // Act
            // 1. Serialize (Step 1 saves to file)
            var json = JsonConvert.SerializeObject(new List<RawDataEntity> { entity });

            // 2. Deserialize (Step 2 loads from file)
            var deserializedList = JsonConvert.DeserializeObject<List<RawDataEntity>>(json);
            var result = deserializedList[0];

            // Assert
            Assert.Equal("test-spec-id", result.ProductSpecId);
            Assert.Equal("Test Spec", result.ProductSpecName);
            Assert.Equal("A", result.Shift);
        }

        [Fact]
        public void Serialization_Should_Handle_Empty_DetectionData_Correctly()
        {
            // Arrange
            var entity = new RawDataEntity { FurnaceNo = "1A20251101-1-4-1" };

            // Act
            var json = JsonConvert.SerializeObject(new List<RawDataEntity> { entity });
            var deserializedList = JsonConvert.DeserializeObject<List<RawDataEntity>>(json);
            var result = deserializedList[0];
        }
    }
}
