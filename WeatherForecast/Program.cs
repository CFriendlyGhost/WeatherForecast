using WeatherForecast.Application.Endpoints;
using WeatherForecast.Application.Interfaces;
using WeatherForecast.Application.Services;
using WeatherForecastApp.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();

builder.Services.AddTransient<IGeocodeService, GeoapifyService>();
builder.Services.AddTransient<IWeatherProvider, WeatherApiProvider>();
builder.Services.AddTransient<IWeatherProvider, OpenWeatherMapProviderService>();
builder.Services.AddTransient<IWeatherProvider, OpenMeteoProvider>();
builder.Services.AddTransient<WeatherService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapWeatherEndpoints();

app.Run();
