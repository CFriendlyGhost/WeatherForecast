using System.Text.Json.Nodes;
using WeatherForecast.Application.Interfaces;
using WeatherForecast.Domain.Models;

namespace WeatherForecast.Application.Services;

public class OpenMeteoProvider(HttpClient httpClient, IConfiguration config) : IWeatherProvider
{
    private readonly string _baseUrl = config["ApiUrls:OpenMeteo"] ?? "https://api.open-meteo.com/v1/forecast";
    public string Name => "Open-Meteo";

    public async Task<ProviderForecast?> GetForecastAsync(Location location, DateTime date)
    {
         var url = $"{_baseUrl}?latitude={location.Latitude}&longitude={location.Longitude}&hourly=temperature_2m";
         var response = await httpClient.GetAsync(url);
         if (!response.IsSuccessStatusCode) return null;

         var json = await response.Content.ReadAsStringAsync();
         var node = JsonNode.Parse(json);
         var temps = node?["hourly"]?["temperature_2m"]?.AsArray();
         
         if (temps != null && temps.Count > 0)
         {
             var firstTemp = temps[0]?.GetValue<double>();
             if (firstTemp.HasValue) return new ProviderForecast(Name, firstTemp.Value);
         }
         return null;
    }
}
