using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using SOA_CA1.Models;

namespace SOA_CA1.ApiClients;

public class RawgApiClient(HttpClient httpClient, IConfiguration configuration)
{
    public async Task<IReadOnlyList<Game>> SearchGamesAsync(
        string search,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(search);

        var apiKey = configuration["Rawg:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "RAWG API key is not configured.");
        }

        var url = $"games?key={Uri.EscapeDataString(apiKey)}" +
                  $"&search={Uri.EscapeDataString(search)}";

        var result = await httpClient.GetFromJsonAsync<RawgGamesResponse>(
            url, cancellationToken);

        return result?.Results?.Select(MapGame).ToArray() ?? [];
    }

    private static Game MapGame(RawgGameDto rawgGame)
    {
        DateOnly? releaseDate = null;
        if (DateOnly.TryParse(
                rawgGame.Released,
                CultureInfo.InvariantCulture,
                out var parsedDate))
        {
            releaseDate = parsedDate;
        }

        return new Game(
            rawgGame.Id,
            rawgGame.Name ?? throw new InvalidDataException("RAWG returned a game without a name."),
            releaseDate,
            rawgGame.Rating,
            rawgGame.BackgroundImage);
    }

    private class RawgGamesResponse
    {
        [JsonPropertyName("results")]
        public List<RawgGameDto>? Results { get; set; }
    }

    private class RawgGameDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("released")]
        public string? Released { get; set; }

        [JsonPropertyName("rating")]
        public decimal? Rating { get; set; }

        [JsonPropertyName("background_image")]
        public string? BackgroundImage { get; set; }
    }
}