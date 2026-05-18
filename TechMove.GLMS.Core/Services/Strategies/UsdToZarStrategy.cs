namespace TechMove.GLMS.Core.Services.Strategies;

public class UsdToZarStrategy : ICurrencyStrategy
{
    public string SourceCurrencyCode => "USD";

    public decimal ConvertToZAR(decimal amount, decimal exchangeRate)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));

        if (exchangeRate <= 0)
            throw new ArgumentException("Exchange rate must be positive.", nameof(exchangeRate));

        return Math.Round(amount * exchangeRate, 2, MidpointRounding.AwayFromZero);
    }
}