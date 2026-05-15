using System.Text.Json.Nodes;
using WeatherForecast.Application.Interfaces;
using WeatherForecast.Domain.Models;

namespace WeatherForecast.Application.Services;

public class WeatherApiProvider(HttpClient httpClient, IConfiguration config) : IWeatherProvider
{
    private readonly string _apiKey = config["ApiKeys:WeatherApi"] ?? Environment.GetEnvironmentVariable("ApiKeys__WeatherApi") ?? "";
    private readonly string _baseUrl = config["ApiUrls:WeatherApi"] ?? "https://api.weatherapi.com/v1/forecast.json";
    public string Name => "WeatherAPI";

    public async Task<ProviderForecast?> GetForecastAsync(Location location, DateTime date)
    {
        var url = $"{_baseUrl}?q={location.Latitude},{location.Longitude}&days=1&key={_apiKey}";
        var response = await httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        var node = JsonNode.Parse(json);
        var temp = node?["forecast"]?["forecastday"]?[0]?["day"]?["avgtemp_c"]?.GetValue<double>();
         
        return temp.HasValue ? new ProviderForecast(Name, temp.Value) : null;
    }
}