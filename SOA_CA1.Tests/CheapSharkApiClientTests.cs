using System.Net;
using SOA_CA1.ApiClients;

namespace SOA_CA1.Tests;

public class CheapSharkApiClientTests
{
    [Fact]
    public async Task GetDealsForGameAsync_MapsAndSortsDeals()
    {
        var requests = new List<string>();
        // Return sample responses for each API call 
        var handler = new StubHttpMessageHandler((request, _) =>
        {
            var uri = request.RequestUri!;
            requests.Add(uri.PathAndQuery);

            return Task.FromResult(uri.AbsolutePath switch
            {
                // Include a non-matching result to verify program does title matching
                "/games" when uri.Query.StartsWith("?title=Stardew%20Valley", StringComparison.Ordinal) =>
                    StubHttpMessageHandler.JsonResponse(
                        """[{"gameID":"wrong","external":"Other"},{"gameID":"42","external":"stardew valley"}]"""),
                "/games" when uri.Query.Contains("id=42", StringComparison.Ordinal) =>
                    StubHttpMessageHandler.JsonResponse(
                        """{"deals":[{"dealID":"deal%2Fone","storeID":"1","price":"8.50","retailPrice":"20"},{"dealID":"deal-two","storeID":"9","price":"5","retailPrice":"10"}]}"""),
                "/stores" =>
                    StubHttpMessageHandler.JsonResponse(
                        """[{"storeID":"1","storeName":"Example Store"}]"""),
                _ => throw new InvalidOperationException($"Unexpected request: {uri}")
            });
        });
        var client = new CheapSharkApiClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.test/")
        });

        // The client searches for the game, loads its deals and store names, then maps the results.
        var deals = await client.GetDealsForGameAsync("Stardew Valley");

        // Verify ordering, store-name fallback, mapped prices, and redirect URL.
        Assert.Equal(2, deals.Count);
        Assert.Equal(5m, deals[0].SalePriceUsd);
        Assert.Equal("9", deals[0].StoreName);
        Assert.Equal("Example Store", deals[1].StoreName);
        Assert.Equal(20m, deals[1].NormalPriceUsd);
        Assert.Equal(
            "https://www.cheapshark.com/redirect?dealID=deal%2Fone",
            deals[1].DealUrl);
        Assert.Equal(3, requests.Count);
        Assert.Contains("title=Stardew%20Valley", requests[0]);
        Assert.Contains("id=42", requests[1]);
        Assert.Equal("/stores", requests[2]);
    }

    [Fact]
    public async Task GetDealsForGameAsync_ReturnsEmptyWhenGameIsNotFound()
    {
        var calls = 0;
        // An empty search result should return no deals without requesting details or stores.
        var handler = new StubHttpMessageHandler((_, _) =>
        {
            calls++;
            return Task.FromResult(StubHttpMessageHandler.JsonResponse("[]"));
        });
        var client = new CheapSharkApiClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.test/")
        });

        var deals = await client.GetDealsForGameAsync("Unknown Game");

        // Only the search request should have been made.
        Assert.Empty(deals);
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task GetDealsForGameAsync_ThrowsForUnsuccessfulResponse()
    {
        // Simulate an upstream failure and ensure it is reported, not treated as an empty result.
        var handler = new StubHttpMessageHandler((_, _) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadGateway)
            {
                Content = new StringContent("upstream unavailable")
            }));
        var client = new CheapSharkApiClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.test/")
        });

        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => client.GetDealsForGameAsync("Game"));

        Assert.Equal(HttpStatusCode.BadGateway, exception.StatusCode);
    }
}
