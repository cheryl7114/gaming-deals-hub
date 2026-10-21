using Microsoft.Extensions.Configuration;
using SOA_CA1.ApiClients;

namespace SOA_CA1.Tests;

public class RawgApiClientTests
{
    [Fact]
    public async Task SearchGamesAsync_EscapesSearchAndMapsResults()
    {
        // Return a fixed game response and verify the key and search text are URL-encoded.
        var handler = new StubHttpMessageHandler((request, _) =>
        {
            Assert.Contains("key=test%20key", request.RequestUri!.Query);
            Assert.Contains("search=zelda%20%26%20link", request.RequestUri.Query);

            return Task.FromResult(StubHttpMessageHandler.JsonResponse(
                """{"results":[{"id":1,"name":"The Legend of Zelda","released":"1986-02-21","rating":4.5,"background_image":"https://img.test/zelda.jpg","description_raw":"Adventure"}]}"""));
        });
        var client = CreateClient(handler, "test key");

        var games = await client.SearchGamesAsync("zelda & link");

        // Confirm the API fields are mapped into the app's Game model.
        var game = Assert.Single(games);
        Assert.Equal(1, game.RawgId);
        Assert.Equal("The Legend of Zelda", game.Name);
        Assert.Equal(new DateOnly(1986, 2, 21), game.ReleaseDate);
        Assert.Equal(4.5m, game.Rating);
        Assert.Equal("https://img.test/zelda.jpg", game.ArtworkUrl);
        Assert.Equal("Adventure", game.Description);
    }

    [Fact]
    public async Task GetFullDetailsAsync_CombinesGameAndScreenshots()
    {
        // Simulate the separate game and screenshot responses used by the details request.
        var handler = new StubHttpMessageHandler((request, _) =>
        {
            return Task.FromResult(request.RequestUri!.AbsolutePath switch
            {
                "/games/7" => StubHttpMessageHandler.JsonResponse(
                    """{"id":7,"name":"Game Seven","released":"2024-06-01","developers":[{"name":"Studio A"},{"name":null}]}"""),
                "/games/7/screenshots" => StubHttpMessageHandler.JsonResponse(
                    """{"results":[{"image":"https://img.test/one.jpg"},{"image":null},{"image":"https://img.test/two.jpg"}]}"""),
                _ => throw new InvalidOperationException(
                    $"Unexpected request: {request.RequestUri}")
            });
        });
        var client = CreateClient(handler, "key");

        var game = await client.GetFullDetailsAsync(7);

        // Verify game details and non-empty developer and screenshot values are combined.
        Assert.Equal(7, game.RawgId);
        Assert.Equal("Game Seven", game.Name);
        Assert.Equal(["Studio A"], game.Developers);
        Assert.Equal(
            ["https://img.test/one.jpg", "https://img.test/two.jpg"],
            game.Screenshots);
    }

    [Fact]
    public async Task GetPopularGamesAsync_RequestsTwelveGamesByPopularity()
    {
        var handler = new StubHttpMessageHandler((request, _) =>
        {
            // Ensure the popular-games request uses the expected page size and ordering.
            Assert.Equal(
                "?key=key&page_size=12&ordering=-added",
                request.RequestUri!.Query);
            return Task.FromResult(StubHttpMessageHandler.JsonResponse(
                """{"results":[{"id":2,"name":"Popular Game"}]}"""));
        });
        var client = CreateClient(handler, "key");

        var games = await client.GetPopularGamesAsync();

        Assert.Equal("Popular Game", Assert.Single(games).Name);
    }

    [Fact]
    public async Task SearchGamesAsync_RequiresConfiguredApiKey()
    {
        // Missing configuration should fail before any HTTP request is attempted.
        var client = CreateClient(new StubHttpMessageHandler((_, _) =>
            throw new InvalidOperationException("No request should be sent.")), null);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => client.SearchGamesAsync("game"));
    }

    private static RawgApiClient CreateClient(
        HttpMessageHandler handler,
        string? apiKey)
    {
        // Give the client test configuration and a local base address instead of real services.
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Rawg:ApiKey"] = apiKey
            })
            .Build();

        return new RawgApiClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.test/")
        }, configuration);
    }
}
