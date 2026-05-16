using Microsoft.Extensions.Caching.Memory;
using WeatherForecast.Application.Interfaces;
using WeatherForecast.Domain.Models;

namespace WeatherForecast.Application.Services;

public class WeatherService(
    IGeocodeService geocodeService,
    IEnumerable<IWeatherProvider> weatherProviders,
    IMemoryCache cache)
{
    public async Task<WeatherResult?> GetWeatherAsync(string city, string country, DateTime date)
    {
        var cacheKey = $"weather_{city}_{country}_{date:yyyyMMdd}";
        
        if (cache.TryGetValue(cacheKey, out WeatherResult cachedResult))
        {
            return cachedResult;
        }

        var location = await geocodeService.GetLocationAsync(city, country);
        if (location == null) return null;

        var tasks = weatherProviders
            .Select(p => p.GetForecastAsync(location.Value, date));
        
        var results = await Task.WhenAll(tasks);
        
        var validForecasts = results
            .Where(r => r != null)
            .Select(r => r!.Value)
            .ToList();

        if (validForecasts.Count == 0)
        {
            return null;
        }

        var finalResult = new WeatherResult(city, country, date, validForecasts);
        
        cache.Set(cacheKey, finalResult, TimeSpan.FromMinutes(30));

        return finalResult;
    }
}
