namespace TechMove.GLMS.Core.Services.CurrencyExchange;

public interface ICurrencyExchangeService
{
    /// <summary>
    /// Fetches current ZAR exchange rate.
    /// Falls back to cached value if API is unreachable.
    /// </summary>
    Task<ExchangeRateResult> GetRateToZarAsync(string sourceCurrencyCode, CancellationToken ct = default);
}