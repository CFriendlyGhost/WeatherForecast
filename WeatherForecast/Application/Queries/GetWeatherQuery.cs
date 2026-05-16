namespace WeatherForecast.Application.Queries;

public sealed record GetWeatherQuery(string City, string Country, DateTime Date);

