using System.Text.Json;

namespace TechMove.GLMS.Core.Services.CurrencyExchange;

// when the external API is unreachable.

public class FileFallbackRateStore : IFallbackRateStore
{
    private readonly string _filePath;
    private static readonly SemaphoreSlim _lock = new(1, 1);

    public FileFallbackRateStore(string contentRootPath)
    {
        if (string.IsNullOrWhiteSpace(contentRootPath))
            throw new ArgumentException("Content root path is required.", nameof(contentRootPath));

        var dir = Path.Combine(contentRootPath, "App_Data");
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "exchange-rate-fallback.json");
    }

    public async Task<decimal?> GetAsync(string currencyCode, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            if (!File.Exists(_filePath)) return null;

            var json = await File.ReadAllTextAsync(_filePath, ct);
            if (string.IsNullOrWhiteSpace(json)) return null;

            var dict = JsonSerializer.Deserialize<Dictionary<string, decimal>>(json);
            if (dict is null) return null;

            return dict.TryGetValue(currencyCode, out var rate) ? rate : null;
        }
        catch
        {
            return null;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SetAsync(string currencyCode, decimal rate, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(currencyCode)) return;
        if (rate <= 0) return;

        await _lock.WaitAsync(ct);
        try
        {
            Dictionary<string, decimal> dict = new();

            if (File.Exists(_filePath))
            {
                var existing = await File.ReadAllTextAsync(_filePath, ct);
                if (!string.IsNullOrWhiteSpace(existing))
                {
                    dict = JsonSerializer.Deserialize<Dictionary<string, decimal>>(existing)
                           ?? new Dictionary<string, decimal>();
                }
            }

            dict[currencyCode] = rate;

            var options = new JsonSerializerOptions { WriteIndented = true };
            var output = JsonSerializer.Serialize(dict, options);
            await File.WriteAllTextAsync(_filePath, output, ct);
        }
        finally
        {
            _lock.Release();
        }
    }
}