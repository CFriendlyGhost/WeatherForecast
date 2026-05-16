using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using WeatherForecast.Application.Services;
using WeatherForecast.Tests.Helpers;

namespace WeatherForecast.Tests.UnitTests;

public class GeoapifyServiceTests
{
    [Fact]
    public async Task GetLocationAsync_ReturnsLocation_WhenApiReturnsValidResponse()
    {
        var json = JsonSerializer.Serialize(new
        {
            features = new[] {
                new { properties = new { lat = 12.34, lon = 56.78 } }
            }
        });
        var client = HttpClientTestHelper.CreateClient(_ => new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        });
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ApiUrls:Geoapify"] = "http://api.geoapify.test",
                ["ApiKeys:Geoapify"] = "fake-key"
            })
            .Build();
        var logger = Mock.Of<ILogger<GeoapifyService>>();
        var svc = new GeoapifyService(client, config, logger);

        var result = await svc.GetLocationAsync("City","Country");

        result.HasValue.Should().BeTrue();
        result!.Value.Latitude.Should().Be(12.34);
        result.Value.Longitude.Should().Be(56.78);
    }

    [Fact]
    public async Task GetLocationAsync_ReturnsNull_WhenBaseUrlMissing()
    {
        var client = HttpClientTestHelper.CreateClient(_ => new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("{}")
        });
        var config = new ConfigurationBuilder().Build();
        var logger = Mock.Of<ILogger<GeoapifyService>>();
        var svc = new GeoapifyService(client, config, logger);

        var result = await svc.GetLocationAsync("City","Country");

        result.Should().BeNull();
    }
}

