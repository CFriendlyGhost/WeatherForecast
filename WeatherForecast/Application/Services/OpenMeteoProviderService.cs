using System.Text.Json.Nodes;
using WeatherForecast.Application.Interfaces;
using WeatherForecast.Domain.Models;

namespace WeatherForecast.Application.Services;

public class OpenMeteoProvider(HttpClient httpClient, IConfiguration config, ILogger<OpenMeteoProvider> logger) : IWeatherProvider
{
    private readonly string? _baseUrl = config["ApiUrls:OpenMeteo"];
    public string Name => "Open-Meteo";

    public async Task<ProviderForecast?> GetForecastAsync(Location location, DateTime date)
    {
        if (string.IsNullOrWhiteSpace(_baseUrl))
        {
            logger.LogError("Base URL is missing for OpenMeteoProvider.");
            return null;
        }

        try
        {
             var url = 
                 $"{_baseUrl}?latitude={location.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                 $"&longitude={location.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}" +
                 $"&hourly=temperature_2m";
             
             var response = await httpClient.GetAsync(url);
             if (!response.IsSuccessStatusCode)
             {
                 logger.LogWarning("Open-Meteo returned status code: {StatusCode}", response.StatusCode);
                 return null;
             }

             var json = await response.Content.ReadAsStringAsync();
             var node = JsonNode.Parse(json);
             var weatherDetails = node?["hourly"]?["temperature_2m"]?.AsArray();
             
             if (weatherDetails != null && weatherDetails.Count > 0)
             {
                 var firstTemp = weatherDetails[0]?.GetValue<double>();
                 if (firstTemp.HasValue) return new ProviderForecast(Name, firstTemp.Value);
             }
             return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while fetching forecast from Open-Meteo.");
            return null;
        }
    }
}
