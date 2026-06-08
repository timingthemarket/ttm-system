namespace TTM.Shared.Constants;

public enum Indicators
{
    Unknown = 0,
    /// <summary>
    /// Cash flow
    /// </summary>
    FreeCashFlow = 101,
    
    /// <summary>
    /// Cash flow
    /// </summary>
    OperatingActivities = 102,
    
    /// <summary>
    /// Cash flow
    /// </summary>
    InvestingActivities = 103,
    
    CashFlowForTheYear = 104,
    
    FinancingActivities = 105,
    
    /// <summary>
    /// Balance sheet
    /// </summary>
    GrossIncome = 201,
    
    /// <summary>
    /// Balance sheet
    /// </summary>
    NetDebt = 202,
    
    /// <summary>
    /// Balance sheet
    /// </summary>
    IntangibleAssets = 203,
    
    /// <summary>
    /// Balance sheet
    /// </summary>
    TangibleAssets = 204,
    
    CurrentAssets = 205,
    
    NonCurrentAssets = 206,
    
    TotalAssets = 207,
    
    ProfitToEquityHolders = 208,
    
    NonCurrentLiabilities = 209,
    
    CurrentLiabilities = 210,
    
    TotalLiabilitiesAndEquity = 211,
    
    CashAndEquivalents = 212,
    
    TotalEquity = 213,
    
    FinancialAssets = 214,
    
    // Income statement
    Eps = 301,
    
    // (EBIT)
    OperatingIncome = 302,
    
    Revenues = 303,
    
    GrossProfit = 304,
    
    NetSales = 305,

    // Other
    Dividend = 901,
    
    NumberOfShares = 902,
    
    // CalculatedIndicators
    // Beta
    BetaOmx30 = 10_001,
    BetaNordic40 = 10_002,
    
    // Returns
    Return = 10_010,
    
    // PE-value
    Pe = 10_020,
    
    // Volatility
    Volatility = 10_030,
    
    // Momentum
    RsiMomentum = 10_040,
    
    Roc = 10_050,
    
    Roic = 10_060,
    /// <summary>
    /// Piotroski F-Score 9-point scoring system
    /// </summary>
    FScore = 10_070,
}