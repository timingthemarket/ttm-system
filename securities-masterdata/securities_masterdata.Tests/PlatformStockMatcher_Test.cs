using securities_masterdata.Domain.Handlers.Sync;
using Xunit;

namespace securities_masterdata.Tests;

public class PlatformStockMatcher_Test
{
    private static PlatformStockMatcher CreateMatcher() =>
        new([
            ("SAAB B", "SAAB AB ser. B"),
            ("VOLV B", "Volvo, AB ser. B"),
            ("AZN", "AstraZeneca PLC"),
            ("ERIC B", "Ericsson, Telefonab. L M ser. B")
        ]);

    [Theory]
    [InlineData("SAAB B")]
    [InlineData("saab b")] // ticker comparison is case insensitive
    [InlineData("AZN")]
    public void Matches_ByTicker(string ticker)
    {
        Assert.True(CreateMatcher().Matches(ticker, "a name that is nowhere near the list"));
    }

    [Fact]
    public void Matches_ByExactName_WhenTickerIsUnknown()
    {
        Assert.True(CreateMatcher().Matches("UNKNOWN", "AstraZeneca PLC"));
    }

    [Fact]
    public void Matches_ByName_IgnoringCompanySuffix()
    {
        // "AstraZeneca PLC" and "AstraZeneca" are the same name once the suffix is stripped
        Assert.True(CreateMatcher().Matches("UNKNOWN", "AstraZeneca"));
    }

    [Fact]
    public void Matches_ByName_WithSmallSpellingDifference()
    {
        Assert.True(CreateMatcher().Matches("UNKNOWN", "Astra Zeneca PLC"));
    }

    [Fact]
    public void DoesNotMatch_WhenNeitherTickerNorNameIsClose()
    {
        Assert.False(CreateMatcher().Matches("NOSUCH", "Totally Made Up Holding AB"));
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("   ", "   ")]
    public void DoesNotMatch_OnMissingTickerAndName(string? ticker, string? name)
    {
        Assert.False(CreateMatcher().Matches(ticker, name));
    }

    [Fact]
    public void SkipsBlankTickersAndNamesFromThePlatformList()
    {
        var matcher = new PlatformStockMatcher([(null, null), ("", ""), ("  ", "  "), ("ABC", "Alpha Beta")]);

        Assert.Equal(1, matcher.TickerCount);
        Assert.True(matcher.Matches("ABC", null));
        Assert.False(matcher.Matches("", ""));
    }
}
