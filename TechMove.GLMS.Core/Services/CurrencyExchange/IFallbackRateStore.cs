namespace TechMove.GLMS.Core.Services.CurrencyExchange;

public interface IFallbackRateStore
{
    Task<decimal?> GetAsync(string currencyCode, CancellationToken ct = default);
    Task SetAsync(string currencyCode, decimal rate, CancellationToken ct = default);
}