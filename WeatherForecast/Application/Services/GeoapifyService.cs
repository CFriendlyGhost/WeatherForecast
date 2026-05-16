using System.Text.Json.Nodes;
using WeatherForecast.Application.Interfaces;
using WeatherForecast.Domain.Models;
using Microsoft.Extensions.Logging;

namespace WeatherForecast.Application.Services;

public class GeoapifyService(HttpClient httpClient, IConfiguration config, ILogger<GeoapifyService> logger) : IGeocodeService
{
    private readonly string? _apiKey =
        config["ApiKeys:Geoapify"] ?? Environment.GetEnvironmentVariable("GEOAPIFY_APIKEY");
    
    private readonly string? _baseUrl = config["ApiUrls:Geoapify"];

    public async Task<Location?> GetLocationAsync(string city, string country)
    {
        if (string.IsNullOrWhiteSpace(_baseUrl))
        {
            logger.LogError("Base URL is missing for GeoapifyService.");
            return null;
        }

        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            logger.LogError("API Key is missing for GeoapifyService.");
            return null;
        }

        try
        {
            var url = $"{_baseUrl}?text={Uri.EscapeDataString($"{city}, {country}")}&apiKey={_apiKey}";
            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Geoapify returned status code: {StatusCode}", response.StatusCode);
                return null;
            }
            
            var json = await response.Content.ReadAsStringAsync();
            var node = JsonNode.Parse(json);
            var firstFeature = node?["features"]?[0]?["properties"];
            
            if (firstFeature != null)
            {
                return new Location(
                    firstFeature["lat"]?.GetValue<double>() ?? 0,
                    firstFeature["lon"]?.GetValue<double>() ?? 0
                );
            }
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while fetching location from Geoapify.");
            return null;
        }
    }
}