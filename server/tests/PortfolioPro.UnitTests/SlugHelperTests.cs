using PortfolioPro.Application.Common.Helpers;

namespace PortfolioPro.UnitTests;

public class SlugHelperTests
{
    // NOT: [Theory] + [InlineData] aynı testi farklı girdilerle birden çok kez çalıştırıyor
    [Theory]
    [InlineData("101 Çetele", "101-cetele")]
    [InlineData("Güzel Şehir İstanbul", "guzel-sehir-istanbul")]
    [InlineData("  DevLog: .NET & Angular!  ", "devlog-net-angular")]
    [InlineData("a---b", "a-b")]
    public void Generate_ShouldCreateUrlFriendlySlug(string input, string expected)
    {
        var slug = SlugHelper.Generate(input);

        Assert.Equal(expected, slug);
    }

    [Fact]
    public void Generate_ShouldLimitLengthTo80()
    {
        var slug = SlugHelper.Generate(new string('a', 200));

        Assert.True(slug.Length <= 80);
    }

    [Fact]
    public void Generate_ShouldReturnEmpty_WhenInputIsBlank()
    {
        Assert.Equal(string.Empty, SlugHelper.Generate("   "));
    }
}
