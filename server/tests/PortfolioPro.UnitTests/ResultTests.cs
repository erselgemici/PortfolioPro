using PortfolioPro.Application.Common;

namespace PortfolioPro.UnitTests;

public class ResultTests
{
    [Fact]
    public void ImplicitConversion_FromValue_ShouldBeSuccess()
    {
        Result<int> result = 42;

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void ImplicitConversion_FromError_ShouldBeFailure()
    {
        Result<int> result = Error.NotFound("test.not_found", "Bulunamadı");

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }

    [Fact]
    public void Value_ShouldThrow_WhenResultIsFailure()
    {
        Result<int> result = Error.Conflict("test.conflict", "Çakışma");

        Assert.Throws<InvalidOperationException>(() => result.Value);
    }
}
