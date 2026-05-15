using WeatherForecast.Application.Services;

namespace WeatherForecast.Application.Endpoints;

public static class WeatherEndpoints
{
    public static void MapWeatherEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/weather", async (string city, string country, DateTime date, WeatherService weatherService) =>
        {
            var result = await weatherService.GetWeatherAsync(city, country, date);
            return result is not null ? Results.Ok(result) : Results.NotFound("Could not retrieve weather forecast.");
        })
        .WithName("GetAggregatedWeatherForecast");
    }
}
