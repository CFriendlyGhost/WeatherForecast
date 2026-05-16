using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using WeatherForecast.Application.Services;
using WeatherForecast.Domain.Models;
using WeatherForecast.Tests.Helpers;

namespace WeatherForecast.Tests.UnitTests;

public class WeatherApiProviderTests
{
    [Fact]
    public async Task GetForecastAsync_ReturnsForecast_WhenApiReturnsValidJson()
    {
        var json = JsonSerializer.Serialize(new
        {
            forecast = new { forecastday = new[] { new { day = new { avgtemp_c = 9.1 } } } }
        });
        var client = HttpClientTestHelper.CreateClient(_ => new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        });
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ApiUrls:WeatherApi"] = "http://weatherapi.test",
                ["ApiKeys:WeatherApi"] = "fake"
            })
            .Build();
        var logger = Mock.Of<ILogger<WeatherApiProvider>>();
        var provider = new WeatherApiProvider(client, config, logger);

        var forecast = await provider.GetForecastAsync(new Location(1,2), DateTime.UtcNow);

        forecast.HasValue.Should().BeTrue();
        forecast!.Value.ProviderName.Should().Be("WeatherAPI");
        forecast.Value.TemperatureC.Should().Be(9.1);
    }
}

