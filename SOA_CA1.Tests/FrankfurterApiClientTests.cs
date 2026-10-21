using SOA_CA1.ApiClients;

namespace SOA_CA1.Tests;

public class FrankfurterApiClientTests
{
    [Fact]
    public async Task GetUsdToEurRateAsync_MapsDateAndRate()
    {
        // Return a fixed response and verify the client requests the expected USD-to-EUR rate.
        var handler = new StubHttpMessageHandler((request, _) =>
        {
            Assert.Equal(
                "https://api.test/v1/latest?base=USD&symbols=EUR",
                request.RequestUri!.ToString());

            return Task.FromResult(StubHttpMessageHandler.JsonResponse(
                """{"date":"2025-04-18","rates":{"EUR":0.88}}"""));
        });
        var client = new FrankfurterApiClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.test/")
        });

        var result = await client.GetUsdToEurRateAsync();

        // Confirm the response is mapped correctly and can be used for conversion.
        Assert.Equal("USD", result.FromCurrency);
        Assert.Equal("EUR", result.ToCurrency);
        Assert.Equal(0.88m, result.Rate);
        Assert.Equal(new DateOnly(2025, 4, 18), result.Date);
        Assert.Equal(8.8m, result.Convert(10));
    }

    [Theory]
    [InlineData("""{"date":"not-a-date","rates":{"EUR":0.9}}""")]
    [InlineData("""{"date":"2025-04-18","rates":{}}""")]
    public async Task GetUsdToEurRateAsync_RejectsInvalidResponse(string json)
    {
        // Invalid dates or missing EUR rates should not be accepted as a valid exchange rate.
        var handler = new StubHttpMessageHandler((_, _) =>
            Task.FromResult(StubHttpMessageHandler.JsonResponse(json)));
        var client = new FrankfurterApiClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.test/")
        });

        await Assert.ThrowsAsync<InvalidDataException>(
            () => client.GetUsdToEurRateAsync());
    }

    [Fact]
    public async Task GetUsdToEurRateAsync_RejectsEmptyResponse()
    {
        // An empty API response should raise an error instead of returning a misleading result.
        var handler = new StubHttpMessageHandler((_, _) =>
            Task.FromResult(StubHttpMessageHandler.JsonResponse("null")));
        var client = new FrankfurterApiClient(new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.test/")
        });

        await Assert.ThrowsAsync<InvalidDataException>(
            () => client.GetUsdToEurRateAsync());
    }
}
