using WeatherForecast.Domain.Models;

namespace WeatherForecast.Application.Interfaces;

public interface IGeocodeService
{
    Task<Location?> GetLocationAsync(string city, string country);
}