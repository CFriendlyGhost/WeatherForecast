using System.Text.Json.Nodes;
using WeatherForecast.Application.Interfaces;
using WeatherForecast.Domain.Models;

namespace WeatherForecastApp.Infrastructure;

public class OpenWeatherMapProviderService(HttpClient httpClient, IConfiguration config) : IWeatherProvider
{
    private readonly string _apiKey = config["ApiKeys:OpenWeatherMap"] ?? Environment.GetEnvironmentVariable("ApiKeys__OpenWeatherMap") ?? "";
    private readonly string _baseUrl = config["ApiUrls:OpenWeatherMap"] ?? "https://api.openweathermap.org/data/2.5/forecast";
    public string Name => "OpenWeatherMap";

    public async Task<ProviderForecast?> GetForecastAsync(Location location, DateTime date)
    {
        var url = $"{_baseUrl}?lat={location.Latitude}&lon={location.Longitude}&units=metric&appid={_apiKey}";
        var response = await httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        var node = JsonNode.Parse(json);
        var temp = node?["list"]?[0]?["main"]?["temp"]?.GetValue<double>();
         
        return temp.HasValue ? new ProviderForecast(Name, temp.Value) : null;
    }
}