namespace WeatherForecast.Domain.Models;

public readonly record struct WeatherResult(string City, string Country, DateTime Date, IEnumerable<ProviderForecast> Forecasts);
