using WeatherForecast.Application.Queries;
using WeatherForecast.Domain.Models;

namespace WeatherForecast.Application.Interfaces;

public interface IGetWeatherQueryHandler
{
    Task<WeatherResult?> HandleAsync(GetWeatherQuery query);
}

