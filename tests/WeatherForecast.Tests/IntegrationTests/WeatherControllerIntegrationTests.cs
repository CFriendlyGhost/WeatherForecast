using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using WeatherForecast.Application.Exceptions;
using WeatherForecast.Application.Interfaces;
using WeatherForecast.Application.Queries;
using WeatherForecast.Domain.Models;

namespace WeatherForecast.Tests.IntegrationTests;

public class WeatherControllerIntegrationTests
{
    [Fact]
    public async Task GetWeather_Controller_ReturnsOkResult_WithData()
    {
        var handler = new Mock<IGetWeatherQueryHandler>();
        var date = DateTime.UtcNow.Date;
        var expected = new WeatherResult("City", "Country", date, 
            [new ProviderForecast("TestProvider", 3.14)]);
        handler.Setup(x => x.HandleAsync(It.IsAny<GetWeatherQuery>())).ReturnsAsync(expected);
        var controller = CreateController(handler.Object);

        var response = await controller.GetWeather("City", "Country", date);

        response.Should().BeOfType<OkObjectResult>();
        var ok = (OkObjectResult)response;
        ok.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetWeather_Controller_ReturnsBadRequest_WhenLocationCannotBeFound()
    {
        var handler = new Mock<IGetWeatherQueryHandler>();
        var date = DateTime.UtcNow.Date;
        handler.Setup(x => x.HandleAsync(It.IsAny<GetWeatherQuery>()))
            .ThrowsAsync(new LocationNotFoundException("City", "Country"));
        var controller = CreateController(handler.Object);

        var response = await controller.GetWeather("City", "Country", date);

        response.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = (BadRequestObjectResult)response;
        badRequest.Value.Should().Be("Unable to find coordinates for the specified location. Please check the city " +
                                     "'City' and country 'Country'.");
    }

    [Fact]
    public async Task GetWeather_Controller_ReturnsServerError_WhenHandlerReturnsNull()
    {
        var handler = new Mock<IGetWeatherQueryHandler>();
        var date = DateTime.UtcNow.Date;
        handler.Setup(x => x.HandleAsync(It.IsAny<GetWeatherQuery>())).ReturnsAsync((WeatherResult?)null);
        var controller = CreateController(handler.Object);

        var response = await controller.GetWeather("City", "Country", date);

        response.Should().BeOfType<StatusCodeResult>();
        ((StatusCodeResult)response).StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetWeather_Controller_ReturnsBadRequest_WhenDateIsOutOfRange()
    {
        var handler = new Mock<IGetWeatherQueryHandler>(MockBehavior.Strict);
        var controller = CreateController(handler.Object);
        var invalidDate = DateTime.UtcNow.Date.AddDays(7);

        var response = await controller.GetWeather("City", "Country", invalidDate);

        response.Should().BeOfType<BadRequestObjectResult>();
        ((BadRequestObjectResult)response).Value.Should().Be("Weather forecast is available for a maximum of 6 days ahead," +
                                                             " and the date cannot be in the past.");
    }

    private static Application.Controllers.WeatherController CreateController(IGetWeatherQueryHandler handler)
    {
        var logger = Mock.Of<ILogger<Application.Controllers.WeatherController>>();
        return new Application.Controllers.WeatherController(handler, logger);
    }
}

