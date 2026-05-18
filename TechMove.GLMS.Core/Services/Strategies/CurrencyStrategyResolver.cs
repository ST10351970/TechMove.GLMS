namespace TechMove.GLMS.Core.Services.Strategies;

/// Picks the correct currency strategy at runtime based on the source currency code.

public class CurrencyStrategyResolver
{
    private readonly IEnumerable<ICurrencyStrategy> _strategies;

    public CurrencyStrategyResolver(IEnumerable<ICurrencyStrategy> strategies)
    {
        _strategies = strategies ?? throw new ArgumentNullException(nameof(strategies));
    }

    public ICurrencyStrategy Resolve(string sourceCurrencyCode)
    {
        if (string.IsNullOrWhiteSpace(sourceCurrencyCode))
            throw new ArgumentException("Source currency code is required.", nameof(sourceCurrencyCode));

        var strategy = _strategies.FirstOrDefault(
            s => string.Equals(s.SourceCurrencyCode, sourceCurrencyCode, StringComparison.OrdinalIgnoreCase));

        if (strategy is null)
            throw new NotSupportedException(
                $"No currency strategy registered for code '{sourceCurrencyCode}'.");

        return strategy;
    }
}