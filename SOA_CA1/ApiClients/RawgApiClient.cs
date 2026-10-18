using System.Globalization;
using System.Text.Json.Serialization;
using SOA_CA1.Models;

namespace SOA_CA1.ApiClients;

public class RawgApiClient(HttpClient httpClient, IConfiguration configuration)
{
    // show lightweight game info for search results
    public async Task<IReadOnlyList<Game>> SearchGamesAsync(
        string search,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(search);

        var url = BuildApiUrl("games", $"&search={Uri.EscapeDataString(search)}");
        var response = await httpClient.GetFromJsonAsync<RawgGamesResponse>(
            url, cancellationToken);

        return response?.Results?.Select(MapGame).ToArray() ?? [];
    }

    // get full game details 
    public async Task<Game> GetFullDetailsAsync(
        int rawgId,
        CancellationToken cancellationToken = default)
    {
        if (rawgId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rawgId));
        }

        var gameUrl = BuildApiUrl($"games/{rawgId}");
        var screenshotsUrl = BuildApiUrl($"games/{rawgId}/screenshots");

        // RAWG returns the game record and screenshots from separate endpoints.
        // Request both together, then combine them into the app's Game model.
        var gameTask = httpClient.GetFromJsonAsync<RawgGameDto>(
            gameUrl, cancellationToken);
        var screenshotsTask = httpClient.GetFromJsonAsync<RawgScreenshotsResponse>(
            screenshotsUrl, cancellationToken);

        await Task.WhenAll(gameTask, screenshotsTask);

        var rawgGame = await gameTask
            ?? throw new InvalidDataException("RAWG returned an empty game response.");
        var screenshots = await screenshotsTask
            ?? throw new InvalidDataException("RAWG returned an empty screenshots response.");

        var game = MapGame(rawgGame);
        game.Developers = rawgGame.Developers?
            .Select(developer => developer.Name)
            .OfType<string>()
            .ToList() ?? [];
        game.Screenshots = screenshots.Results?
            .Select(screenshot => screenshot.Image)
            .OfType<string>()
            .ToList() ?? [];

        return game;
    }
    
    // fetch 12 popular games (ordered by number of users who added the game to their library)
    public async Task<IReadOnlyList<Game>> GetPopularGamesAsync(
        CancellationToken cancellationToken = default)
    {
        var url = BuildApiUrl("games", "&page_size=12&ordering=-added");
        var response = await httpClient.GetFromJsonAsync<RawgGamesResponse>(
            url, cancellationToken);

        return response?.Results?.Select(MapGame).ToArray() ?? [];
    }

    private string BuildApiUrl(string path, string query = "")
    {
        var apiKey = configuration["Rawg:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException("RAWG API key is not configured.");
        }

        return $"{path}?key={Uri.EscapeDataString(apiKey)}{query}";
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
            rawgGame.Name ?? throw new InvalidDataException(
                "RAWG returned a game without a name."),
            releaseDate,
            rawgGame.Rating,
            rawgGame.BackgroundImage,
            rawgGame.Description);
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

        [JsonPropertyName("description_raw")]
        public string? Description { get; set; }

        [JsonPropertyName("developers")]
        public List<RawgDeveloperDto>? Developers { get; set; }
    }

    private class RawgDeveloperDto
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    private class RawgScreenshotsResponse
    {
        [JsonPropertyName("results")]
        public List<RawgScreenshotDto>? Results { get; set; }
    }

    private class RawgScreenshotDto
    {
        [JsonPropertyName("image")]
        public string? Image { get; set; }
    }
}
