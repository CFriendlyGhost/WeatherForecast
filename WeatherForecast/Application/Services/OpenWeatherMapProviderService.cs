using System.Text.Json.Nodes;
using WeatherForecast.Application.Interfaces;
using WeatherForecast.Domain.Models;
using Microsoft.Extensions.Logging;

namespace WeatherForecast.Application.Services;

public class OpenWeatherMapProviderService(HttpClient httpClient, IConfiguration config, ILogger<OpenWeatherMapProviderService> logger) : IWeatherProvider
{
    private readonly string? _apiKey = config["ApiKeys:OpenWeatherMap"] ??
                                       Environment.GetEnvironmentVariable("OPENWEATHER_APIKEY");
    
    private readonly string? _baseUrl = config["ApiUrls:OpenWeatherMap"];
    public string Name => "OpenWeatherMap";

    public async Task<ProviderForecast?> GetForecastAsync(Location location, DateTime date)
    {
        if (string.IsNullOrWhiteSpace(_baseUrl))
        {
            logger.LogError("Base URL is missing for OpenWeatherMapProviderService.");
            return null;
        }

        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            logger.LogError("API Key is missing for OpenWeatherMapProviderService.");
            return null;
        }

        try
        {
            var url = $"{_baseUrl}?lat={location.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                      $"&lon={location.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}&units=metric&appid={_apiKey}";
            
            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("OpenWeatherMap returned status code: {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var node = JsonNode.Parse(json);
            var list = node?["list"]?.AsArray();
            
            return CalculateAverageForecast(Name, list);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while fetching forecast from OpenWeatherMap.");
            return null;
        }
    }

    private static ProviderForecast? CalculateAverageForecast(string providerName, JsonArray? list)
    {
        if (list == null || list.Count == 0)
        {
            return null;
        }

        double sum = 0;
        var count = 0;
        
        foreach (var item in list)
        {
            var temp = item?["main"]?["temp"]?.GetValue<double>();
            if (temp.HasValue)
            {
                sum += temp.Value;
                count++;
            }
        }
        
        return count > 0 
            ? new ProviderForecast(providerName, Math.Round(sum / count, 2)) 
            : null;
    }
}