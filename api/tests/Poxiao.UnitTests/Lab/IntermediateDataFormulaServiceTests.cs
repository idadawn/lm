using Poxiao.Lab.Service;
using Xunit;

namespace Poxiao.UnitTests.Lab;

public class IntermediateDataFormulaServiceTests
{
    [Fact]
    public void ResolveInitializedPrecision_WhenDatabasePrecisionExists_KeepsDatabaseValue()
    {
        var result = IntermediateDataFormulaService.ResolveInitializedPrecision(2, 6);

        Assert.Equal(2, result);
    }

    [Fact]
    public void ResolveInitializedPrecision_WhenDatabasePrecisionMissing_UsesColumnPrecision()
    {
        var result = IntermediateDataFormulaService.ResolveInitializedPrecision(null, 6);

        Assert.Equal(6, result);
    }
}
