using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using WeatherForecast.Application.Services;
using WeatherForecast.Domain.Models;
using WeatherForecast.Tests.Helpers;

namespace WeatherForecast.Tests.UnitTests;

public class OpenWeatherMapProviderTests
{
    [Fact]
    public async Task GetForecastAsync_ReturnsForecast_WhenApiReturnsValidJson()
    {
        var json = JsonSerializer.Serialize(new
        {
            list = new[] { new { main = new { temp = 15.2 } } }
        });
        var client = HttpClientTestHelper.CreateClient(_ => new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(json)
        });
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ApiUrls:OpenWeatherMap"] = "http://openweathermap.test",
                ["ApiKeys:OpenWeatherMap"] = "fake"
            })
            .Build();
        var logger = Mock.Of<ILogger<OpenWeatherMapProviderService>>();
        var provider = new OpenWeatherMapProviderService(client, config, logger);

        var forecast = await provider.GetForecastAsync(new Location(1,2), DateTime.UtcNow);

        forecast.HasValue.Should().BeTrue();
        forecast!.Value.ProviderName.Should().Be("OpenWeatherMap");
        forecast.Value.TemperatureC.Should().Be(15.2);
    }
}

