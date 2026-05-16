using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using WeatherForecast.Application.Exceptions;
using WeatherForecast.Application.Interfaces;
using WeatherForecast.Application.Queries;
using WeatherForecast.Domain.Models;

namespace WeatherForecast.Tests.UnitTests;

public class GetWeatherQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsAggregatedResult_WhenProvidersReturnValues()
    {
        var geocode = new Mock<IGeocodeService>();
        geocode.Setup(x => x.GetLocationAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new Location(10, 20));

        var provider1 = new Mock<IWeatherProvider>();
        provider1.Setup(p => p.GetForecastAsync(It.IsAny<Location>(), It.IsAny<DateTime>()))
            .ReturnsAsync(new ProviderForecast("P1", 5.0));

        var provider2 = new Mock<IWeatherProvider>();
        provider2.Setup(p => p.GetForecastAsync(It.IsAny<Location>(), It.IsAny<DateTime>()))
            .ReturnsAsync((ProviderForecast?)null);

        var cache = new MemoryCache(new MemoryCacheOptions());
        var handler = new GetWeatherQueryHandler(geocode.Object, [provider1.Object, provider2.Object], cache);
        var query = new GetWeatherQuery("City", "Country", DateTime.UtcNow);

        var result = await handler.HandleAsync(query);

        result.HasValue.Should().BeTrue();
        result!.Value.City.Should().Be("City");
        result.Value.Country.Should().Be("Country");
        var forecasts = result.Value.Forecasts.ToList();
        forecasts.Should().HaveCount(1);
        forecasts[0].TemperatureC.Should().Be(5.0);
        var cached = await handler.HandleAsync(query);
        cached.HasValue.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_ThrowsLocationNotFoundException_WhenGeocodeReturnsNull()
    {
        var geocode = new Mock<IGeocodeService>();
        geocode.Setup(x => x.GetLocationAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((Location?)null);
        var provider = new Mock<IWeatherProvider>();
        var cache = new MemoryCache(new MemoryCacheOptions());
        var handler = new GetWeatherQueryHandler(geocode.Object, [provider.Object], cache);
        var query = new GetWeatherQuery("City", "Country", DateTime.UtcNow);

        var act = () => handler.HandleAsync(query);

        await act.Should().ThrowAsync<LocationNotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_ReturnsNull_WhenAllProvidersReturnNull()
    {
        // Arrange
        var geocode = new Mock<IGeocodeService>();
        geocode.Setup(x => x.GetLocationAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new Location(10, 20));

        var provider1 = new Mock<IWeatherProvider>();
        provider1.Setup(p => p.GetForecastAsync(It.IsAny<Location>(), It.IsAny<DateTime>()))
            .ReturnsAsync((ProviderForecast?)null);

        var provider2 = new Mock<IWeatherProvider>();
        provider2.Setup(p => p.GetForecastAsync(It.IsAny<Location>(), It.IsAny<DateTime>()))
            .ReturnsAsync((ProviderForecast?)null);

        var cache = new MemoryCache(new MemoryCacheOptions());
        var handler = new GetWeatherQueryHandler(geocode.Object, [provider1.Object, provider2.Object], cache);
        var query = new GetWeatherQuery("City", "Country", DateTime.UtcNow);

        // Act
        var result = await handler.HandleAsync(query);

        // Assert
        result.HasValue.Should().BeFalse();
    }
}

