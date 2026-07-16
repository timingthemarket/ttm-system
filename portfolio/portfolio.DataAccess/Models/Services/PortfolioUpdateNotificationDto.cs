using TTM.Shared.Constants;
using TTM.Shared.Models;

namespace portfolio.DataAccess.Models.Services;

public class PortfolioUpdateNotificationDto
{
    public DateOnly SessionDate { get; set; }
    public List<SessionIndicator> NewIndicators { get; set; }
    public List<SessionIndicator> OldIndicators { get; set; }
    public SessionPortfolio SessionPortfolio { get; set; }
}

public class SessionIndicator
{
    public Indicators Indicator { get; set; }
    public Direction Direction { get; set; }
    public int? LookBackPeriod { get; set; }
    public Aggregator? LookBackAggregator { get; set; }
}

public class SessionPortfolio
{
    public Guid Id { get; set; }
    public double RowSimilarity { get; set; }
    public decimal Money { get; set; }
    public double PortfolioPercentageChange { get; set; }
    public DateOnly SecuritiesDate { get; set; }
    public List<SessionSecurity> Securities { get; set; }
}

public class SessionSecurity
{
    public long SecurityId { get; set; }
    public string Ticker { get; set; }
    public string Sector { get; set; }
    public long Rank { get; set; }
    public int Amount { get; set; }
}