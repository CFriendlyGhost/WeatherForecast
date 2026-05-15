using System.Text.Json.Nodes;
using WeatherForecast.Application.Interfaces;
using WeatherForecast.Domain.Models;

namespace WeatherForecast.Application.Services;

public class GeoapifyService(HttpClient httpClient, IConfiguration config) : IGeocodeService
{
    private readonly string _apiKey = config["ApiKeys:Geoapify"] ?? Environment.GetEnvironmentVariable("ApiKeys__Geoapify") ?? "";
    private readonly string _baseUrl = config["ApiUrls:Geoapify"] ?? "https://api.geoapify.com/v1/geocode/search";

    public async Task<Location?> GetLocationAsync(string city, string country)
    {
        var url = $"{_baseUrl}?text={Uri.EscapeDataString($"{city}, {country}")}&apiKey={_apiKey}";
        var response = await httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;
        
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
}