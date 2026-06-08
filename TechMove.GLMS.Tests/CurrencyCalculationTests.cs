using TechMove.GLMS.Services;

namespace TechMove.GLMS.Tests;

public class CurrencyCalculationTests
{
    private readonly CurrencyCalculationService _sut = new();

    [Fact]
    public void GivenRate18_5_When100Usd_ThenZarIs1850()
    {
        var result = _sut.ConvertUsdToZar(100m, 18.5m);

        Assert.Equal(1850m, result);
    }

    [Fact]
    public void GivenRate0_WhenAnyUsd_ThenZarIs0()
    {
        var result = _sut.ConvertUsdToZar(500m, 0m);

        Assert.Equal(0m, result);
    }

    [Fact]
    public void GivenRate18_5_When0Usd_ThenZarIs0()
    {
        var result = _sut.ConvertUsdToZar(0m, 18.5m);

        Assert.Equal(0m, result);
    }

    [Fact]
    public void GivenLargeAmount_CalculatesCorrectly()
    {
        var result = _sut.ConvertUsdToZar(1_000_000m, 18.5m);

        Assert.Equal(18_500_000m, result);
    }

    [Fact]
    public void GivenRate_WhenFractionalUsd_CalculatesCorrectly()
    {
        // 0.01 * 18.5 = 0.185 — no rounding applied by the service
        var result = _sut.ConvertUsdToZar(0.01m, 18.5m);

        Assert.Equal(0.185m, result);
    }

    [Fact]
    public void GivenNegativeRate_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _sut.ConvertUsdToZar(100m, -1m));
    }

    [Fact]
    public void GivenNullAmount_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.ConvertUsdToZar(null, 18.5m));
    }

    [Fact]
    public void GivenPositiveRate_WhenNegativeUsd_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _sut.ConvertUsdToZar(-50m, 18.5m));
    }
}
