using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using SOA_CA1.Models;

namespace SOA_CA1.ApiClients;

public class FrankfurterApiClient(HttpClient httpClient)
{
    public async Task<ExchangeRate> GetUsdToEurRateAsync(
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetFromJsonAsync<FrankfurterResponse>(
                           "v1/latest?base=USD&symbols=EUR",
                           cancellationToken)
                       ?? throw new InvalidDataException(
                           "Frankfurter returned an empty response.");

        if (!DateOnly.TryParse(
                response.Date,
                CultureInfo.InvariantCulture,
                out var date))
        {
            throw new InvalidDataException(
                "Frankfurter returned an invalid date.");
        }

        if (response.Rates?.TryGetValue("EUR", out var rate) != true || rate <= 0)
        {
            throw new InvalidDataException(
                "Frankfurter did not return a valid EUR exchange rate.");
        }

        return new ExchangeRate("USD", "EUR", rate, date);
    }

    private sealed class FrankfurterResponse
    {
        [JsonPropertyName("date")]
        public string? Date { get; set; }

        [JsonPropertyName("rates")]
        public Dictionary<string, decimal>? Rates { get; set; }
    }
}