using WeatherForecast.Domain.Models;

namespace WeatherForecast.Application.Interfaces;

public interface IWeatherProvider
{
    string Name { get; }
    Task<ProviderForecast?> GetForecastAsync(Location location, DateTime date);
}