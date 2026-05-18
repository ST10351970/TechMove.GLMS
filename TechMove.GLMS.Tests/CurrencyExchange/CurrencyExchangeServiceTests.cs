using System.Net;
using System.Net.Http;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Contrib.HttpClient;
using TechMove.GLMS.Core.Services.CurrencyExchange;
using Xunit;

namespace TechMove.GLMS.Tests.CurrencyExchange;

public class CurrencyExchangeServiceTests
{
    private readonly Mock<IFallbackRateStore> _fallbackStoreMock = new();
    private readonly IMemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());
    private readonly Mock<ILogger<CurrencyExchangeService>> _loggerMock = new();

    private CurrencyExchangeService BuildService(HttpClient httpClient)
    {
        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(CurrencyExchangeService.HttpClientName))
                   .Returns(httpClient);

        return new CurrencyExchangeService(
            factoryMock.Object,
            _memoryCache,
            _fallbackStoreMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetRateToZarAsync_SuccessfulApiResponse_ReturnsRate()
    {
        // Arrange: mock an HTTP response with a clean JSON payload
        var handler = new Mock<HttpMessageHandler>();
        var json = """
            {
                "result": "success",
                "base_code": "USD",
                "rates": { "ZAR": 18.50 }
            }
            """;
        handler.SetupAnyRequest()
               .ReturnsResponse(HttpStatusCode.OK, json, "application/json");

        var httpClient = handler.CreateClient();
        httpClient.BaseAddress = new Uri("https://open.er-api.com/");

        var service = BuildService(httpClient);

        // Act
        var result = await service.GetRateToZarAsync("USD");

        // Assert
        result.Success.Should().BeTrue();
        result.Rate.Should().Be(18.50m);
        result.FromCache.Should().BeFalse();
        _fallbackStoreMock.Verify(s => s.SetAsync(
            "USD", 18.50m, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetRateToZarAsync_ApiFailure_FallsBackToDiskStore()
    {
        // Arrange: API throws, but disk has a stored rate from previous success
        var handler = new Mock<HttpMessageHandler>();
        handler.SetupAnyRequest()
               .ThrowsAsync(new HttpRequestException("Simulated API outage"));

        var httpClient = handler.CreateClient();
        httpClient.BaseAddress = new Uri("https://open.er-api.com/");

        _fallbackStoreMock.Setup(s => s.GetAsync("USD", It.IsAny<CancellationToken>()))
                          .ReturnsAsync(17.95m);

        var service = BuildService(httpClient);

        // Act
        var result = await service.GetRateToZarAsync("USD");

        // Assert: succeeded, but flagged as cached
        result.Success.Should().BeTrue();
        result.Rate.Should().Be(17.95m);
        result.FromCache.Should().BeTrue();
    }

    [Fact]
    public async Task GetRateToZarAsync_ApiFailureAndNoFallback_ReturnsFailure()
    {
        // Arrange: API down, no disk fallback
        var handler = new Mock<HttpMessageHandler>();
        handler.SetupAnyRequest()
               .ThrowsAsync(new HttpRequestException("network unreachable"));

        var httpClient = handler.CreateClient();
        httpClient.BaseAddress = new Uri("https://open.er-api.com/");

        _fallbackStoreMock.Setup(s => s.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((decimal?)null);

        var service = BuildService(httpClient);

        // Act
        var result = await service.GetRateToZarAsync("USD");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetRateToZarAsync_SecondCall_ServesFromMemoryCache()
    {
        // Arrange: count how many times the API is hit across two calls
        var handler = new Mock<HttpMessageHandler>();
        var json = """
            { "result": "success", "base_code": "USD", "rates": { "ZAR": 18.50 } }
            """;
        handler.SetupAnyRequest()
               .ReturnsResponse(HttpStatusCode.OK, json, "application/json");

        var httpClient = handler.CreateClient();
        httpClient.BaseAddress = new Uri("https://open.er-api.com/");
        var service = BuildService(httpClient);

        // Act: two calls in quick succession
        await service.GetRateToZarAsync("USD");
        await service.GetRateToZarAsync("USD");

        // Assert: API was only called once — the second call hit the memory cache
        handler.VerifyAnyRequest(Times.Once());
    }

    [Fact]
    public async Task GetRateToZarAsync_ZarSource_ReturnsRateOfOneWithoutApiCall()
    {
        // ZAR-to-ZAR is trivially 1.0 — no API call should be made
        var handler = new Mock<HttpMessageHandler>();
        var httpClient = handler.CreateClient();
        httpClient.BaseAddress = new Uri("https://open.er-api.com/");
        var service = BuildService(httpClient);

        var result = await service.GetRateToZarAsync("ZAR");

        result.Success.Should().BeTrue();
        result.Rate.Should().Be(1m);
        handler.VerifyAnyRequest(Times.Never());
    }

    [Fact]
    public async Task GetRateToZarAsync_EmptyOrNullCurrencyCode_ReturnsFailure()
    {
        // Edge case: null/empty input
        var handler = new Mock<HttpMessageHandler>();
        var httpClient = handler.CreateClient();
        httpClient.BaseAddress = new Uri("https://open.er-api.com/");
        var service = BuildService(httpClient);

        var resultEmpty = await service.GetRateToZarAsync("");
        var resultNull = await service.GetRateToZarAsync(null!);

        resultEmpty.Success.Should().BeFalse();
        resultNull.Success.Should().BeFalse();
    }
}