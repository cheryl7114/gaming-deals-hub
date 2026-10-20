using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using SOA_CA1.Models;

namespace SOA_CA1.ApiClients;

public class CheapSharkApiClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<Deal>> GetDealsForGameAsync(
        string gameName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gameName);

        var searchUrl =
            $"games?title={Uri.EscapeDataString(gameName)}&limit=10";

        var matches = await GetAsync<List<GameSearchResult>>(
            searchUrl, cancellationToken);

        var match = matches.FirstOrDefault(result =>
                        string.Equals(result.External, gameName,
                            StringComparison.OrdinalIgnoreCase))
                    ?? matches.FirstOrDefault();

        if (match is null)
        {
            return [];
        }

        var details = await GetAsync<GameDetails>(
            $"games?id={Uri.EscapeDataString(match.GameId)}",
            cancellationToken);

        if (details.Deals is null)
        {
            throw new InvalidDataException(
                "CheapShark returned game details without a deals list.");
        }
        
        var stores = await GetAsync<List<StoreDetails>>("stores", cancellationToken);
        var storeNames = stores.ToDictionary(store => store.StoreId, store => store.StoreName);

        return details.Deals
            .Select(deal => MapDeal(deal, match.GameId, storeNames))
            .OrderBy(deal => deal.SalePriceUsd)
            .ToArray();
    }

    private async Task<T> GetAsync<T>(
        string path,
        CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync(path, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"CheapShark GET {response.RequestMessage?.RequestUri} returned " +
                $"{(int)response.StatusCode}: {body}",
                null,
                response.StatusCode);
        }

        return JsonSerializer.Deserialize<T>(body)
            ?? throw new InvalidDataException(
                $"CheapShark returned an empty response for {path}.");
    }

    private static Deal MapDeal(CheapSharkDeal deal, string gameId, IReadOnlyDictionary<string, string?> storeNames)
    {
        if (string.IsNullOrWhiteSpace(deal.DealId)
            || string.IsNullOrWhiteSpace(deal.StoreId))
        {
            throw new InvalidDataException(
                "CheapShark returned a deal without an ID or store ID.");
        }

        var storeName = storeNames.GetValueOrDefault(deal.StoreId) ?? deal.StoreId;
        var dealId = Uri.UnescapeDataString(deal.DealId);
        

        return new Deal(
            storeName,
            ParsePrice(deal.Price, "price"),
            ParsePrice(deal.RetailPrice, "retail price"),
            $"https://www.cheapshark.com/redirect?dealID={Uri.EscapeDataString(dealId)}");
    }

    private static decimal ParsePrice(string? value, string fieldName)
    {
        if (decimal.TryParse(
                value,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var price))
        {
            return price;
        }

        throw new InvalidDataException(
            $"CheapShark returned an invalid deal {fieldName}.");
    }

    private class GameSearchResult
    {
        [JsonPropertyName("gameID")]
        public string GameId { get; set; } = string.Empty;

        [JsonPropertyName("external")]
        public string? External { get; set; }
    }

    private class GameDetails
    {
        [JsonPropertyName("deals")]
        public List<CheapSharkDeal>? Deals { get; set; }
    }

    private class CheapSharkDeal
    {
        [JsonPropertyName("dealID")]
        public string? DealId { get; set; }

        [JsonPropertyName("storeID")]
        public string? StoreId { get; set; }

        [JsonPropertyName("price")]
        public string? Price { get; set; }

        [JsonPropertyName("retailPrice")]
        public string? RetailPrice { get; set; }

        [JsonPropertyName("store")]
        public StoreDetails? Store { get; set; }
    }

    private class StoreDetails
    {
        [JsonPropertyName("storeID")]
        public string StoreId { get; set; } = string.Empty;

        [JsonPropertyName("storeName")]
        public string StoreName { get; set; } = string.Empty;
    }
}