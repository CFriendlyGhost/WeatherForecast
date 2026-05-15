namespace WeatherForecast.Domain.Models;

public readonly record struct ProviderForecast(string ProviderName, double TemperatureC);