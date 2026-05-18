namespace TechMove.GLMS.Core.Services.Strategies;

/// Strategy interface for converting an amount in a source currency to ZAR.

public interface ICurrencyStrategy
{
    /// The ISO 4217 three-letter code this strategy handles (e.g. "USD").

    string SourceCurrencyCode { get; }

    /// Converts an amount from the source currency to ZAR using the given exchange rate.

    /// <param name="amount">Amount in the source currency.</param>
    /// <param name="exchangeRate">Exchange rate: 1 source currency = X ZAR.</param>
    /// <returns>The converted amount in ZAR, rounded to 2 decimal places.</returns>
    decimal ConvertToZAR(decimal amount, decimal exchangeRate);
}