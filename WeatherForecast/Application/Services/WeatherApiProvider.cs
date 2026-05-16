using System.Text.Json.Nodes;
using WeatherForecast.Application.Interfaces;
using WeatherForecast.Domain.Models;

namespace WeatherForecast.Application.Services;

public class WeatherApiProvider(HttpClient httpClient, IConfiguration config, ILogger<WeatherApiProvider> logger)
    : IWeatherProvider
{
    private readonly string? _apiKey =
        config["ApiKeys:WeatherApi"] ?? Environment.GetEnvironmentVariable("WEATHERAPI_APIKEY");
    
    private readonly string? _baseUrl = config["ApiUrls:WeatherApi"];
    public string Name => "WeatherAPI";

    public async Task<ProviderForecast?> GetForecastAsync(Location location, DateTime date)
    {
        if (string.IsNullOrWhiteSpace(_baseUrl))
        {
            logger.LogError("Base URL is missing for WeatherApiProvider.");
            return null;
        }

        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            logger.LogError("API Key is missing for WeatherApiProvider.");
            return null;
        }

        try
        {
            var url = $"{_baseUrl}?q={location.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)},{location.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}&days=1&key={_apiKey}";
            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("WeatherAPI returned status code: {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var node = JsonNode.Parse(json);
            var weatherDetail = node?["forecast"]?["forecastday"]?[0]?["day"]?["avgtemp_c"]?.GetValue<double>();
             
            return weatherDetail.HasValue ? new ProviderForecast(Name, weatherDetail.Value) : null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while fetching forecast from WeatherAPI.");
            return null;
        }
    }
}