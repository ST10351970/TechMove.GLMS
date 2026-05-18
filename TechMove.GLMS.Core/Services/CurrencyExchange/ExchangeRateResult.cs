namespace TechMove.GLMS.Core.Services.CurrencyExchange;

public class ExchangeRateResult
{
    public bool Success { get; init; }
    public decimal Rate { get; init; }
    public string SourceCurrency { get; init; } = string.Empty;
    public string TargetCurrency { get; init; } = "ZAR";
    public bool FromCache { get; init; }
    public string? ErrorMessage { get; init; }

    public static ExchangeRateResult Ok(string source, decimal rate, bool fromCache = false) =>
        new()
        {
            Success = true,
            SourceCurrency = source,
            Rate = rate,
            FromCache = fromCache
        };

    public static ExchangeRateResult Fail(string source, string error) =>
        new()
        {
            Success = false,
            SourceCurrency = source,
            ErrorMessage = error
        };
}