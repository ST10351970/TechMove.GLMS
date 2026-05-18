using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace TechMove.GLMS.Core.Services.CurrencyExchange;

public class CurrencyExchangeService : ICurrencyExchangeService
{
    public const string HttpClientName = "ExchangeRateApi";
    public const string TargetCurrency = "ZAR";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly IFallbackRateStore _fallbackStore;
    private readonly ILogger<CurrencyExchangeService> _logger;

    //API updates daily
    private static readonly TimeSpan CacheLifetime = TimeSpan.FromHours(1);

    public CurrencyExchangeService(
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        IFallbackRateStore fallbackStore,
        ILogger<CurrencyExchangeService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _fallbackStore = fallbackStore;
        _logger = logger;
    }

    public async Task<ExchangeRateResult> GetRateToZarAsync(string sourceCurrencyCode, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(sourceCurrencyCode))
            return ExchangeRateResult.Fail(sourceCurrencyCode ?? "", "Source currency code is required.");

        var normalisedCode = sourceCurrencyCode.Trim().ToUpperInvariant();

        // ZAR to ZAR. 1.0
        if (normalisedCode == TargetCurrency)
            return ExchangeRateResult.Ok(normalisedCode, 1m);

        var memoryCacheKey = $"fx:fresh:{normalisedCode}";

        // Fast path: serve from in-memory fresh cache if present
        if (_cache.TryGetValue<decimal>(memoryCacheKey, out var cachedRate))
        {
            return ExchangeRateResult.Ok(normalisedCode, cachedRate, fromCache: false);
        }

        // External API call
        try
        {
            var client = _httpClientFactory.CreateClient(HttpClientName);
            var endpoint = $"v6/latest/{normalisedCode}";

            var response = await client.GetAsync(endpoint, ct);
            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<ExchangeRateApiResponse>(cancellationToken: ct);

            if (payload is null || !string.Equals(payload.Result, "success", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("API returned an unsuccessful response.");

            if (payload.Rates is null || !payload.Rates.TryGetValue(TargetCurrency, out var rate))
                throw new InvalidOperationException($"Response did not contain a {TargetCurrency} rate.");

            if (rate <= 0)
                throw new InvalidOperationException($"Received non-positive rate: {rate}.");

            // Cache in memory for fast read
            _cache.Set(memoryCacheKey, rate, CacheLifetime);

            // Persist to disk
            await _fallbackStore.SetAsync(normalisedCode, rate, ct);

            return ExchangeRateResult.Ok(normalisedCode, rate);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "External exchange rate lookup failed for {Source}. Attempting disk fallback.", normalisedCode);

            var diskRate = await _fallbackStore.GetAsync(normalisedCode, ct);
            if (diskRate.HasValue)
            {
                _cache.Set(memoryCacheKey, diskRate.Value, CacheLifetime);
                return ExchangeRateResult.Ok(normalisedCode, diskRate.Value, fromCache: true);
            }

            return ExchangeRateResult.Fail(
                normalisedCode,
                $"Unable to retrieve exchange rate from external API and no cached value is available. ({ex.Message})");
        }
    }
}