using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using WeatherForecast.Application.Services;
using WeatherForecast.Domain.Models;
using WeatherForecast.Tests.Helpers;

namespace WeatherForecast.Tests.UnitTests;

public class OpenMeteoProviderTests
{
    private static readonly double[] Value = [5.0, 10.0, 15.0];

    [Fact]
    public async Task GetForecastAsync_ReturnsForecast_WhenApiReturnsValues()
    {
        var json = JsonSerializer.Serialize(new
        {
            hourly = new { temperature_2m = Value }
        });
        var client = HttpClientTestHelper.CreateClient(_ => new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        });
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ApiUrls:OpenMeteo"] = "http://open-meteo.test" })
            .Build();
        var logger = Mock.Of<ILogger<OpenMeteoProvider>>();
        var provider = new OpenMeteoProvider(client, config, logger);
        
        var forecast = await provider.GetForecastAsync(new Location(1,2), DateTime.UtcNow);

        forecast.HasValue.Should().BeTrue();
        forecast!.Value.ProviderName.Should().Be("Open-Meteo");
        forecast.Value.TemperatureC.Should().Be(10.0);
    }
}

