using Microsoft.Extensions.Caching.Memory;
using WeatherForecast.Application.Exceptions;
using WeatherForecast.Application.Interfaces;
using WeatherForecast.Domain.Models;

namespace WeatherForecast.Application.Queries;

public sealed class GetWeatherQueryHandler(
	IGeocodeService geocodeService,
	IEnumerable<IWeatherProvider> weatherProviders,
	IMemoryCache cache) : IGetWeatherQueryHandler
{
	public async Task<WeatherResult?> HandleAsync(GetWeatherQuery query)
	{
		var cacheKey = $"weather_{query.City}_{query.Country}_{query.Date:yyyyMMdd}";

		if (cache.TryGetValue(cacheKey, out WeatherResult cachedResult))
		{
			return cachedResult;
		}

		var location = await geocodeService.GetLocationAsync(query.City, query.Country);
		if (location == null)
		{
			throw new LocationNotFoundException(query.City, query.Country);
		}

		var tasks = weatherProviders
			.Select(p => p.GetForecastAsync(location.Value, query.Date));

		var results = await Task.WhenAll(tasks);

		var validForecasts = results
			.Where(r => r != null)
			.Select(r => r!.Value)
			.ToList();

		if (validForecasts.Count == 0)
		{
			return null;
		}

		var finalResult = new WeatherResult(query.City, query.Country, query.Date, validForecasts);

		cache.Set(cacheKey, finalResult, TimeSpan.FromMinutes(30));

		return finalResult;
	}
}

