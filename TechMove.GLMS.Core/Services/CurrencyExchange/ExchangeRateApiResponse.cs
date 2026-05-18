using System.Text.Json.Serialization;

namespace TechMove.GLMS.Core.Services.CurrencyExchange;

/// <summary>
/// JSON response from https://open.er-api.com/v6/latest/{BASE}.
/// </summary>
public class ExchangeRateApiResponse
{
    [JsonPropertyName("result")]
    public string? Result { get; set; }

    [JsonPropertyName("base_code")]
    public string? BaseCode { get; set; }

    [JsonPropertyName("time_last_update_utc")]
    public string? TimeLastUpdateUtc { get; set; }

    [JsonPropertyName("rates")]
    public Dictionary<string, decimal>? Rates { get; set; }
}