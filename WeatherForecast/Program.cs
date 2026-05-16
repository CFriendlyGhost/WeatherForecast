using WeatherForecast.Application.Interfaces;
using WeatherForecast.Application.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();
builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddTransient<IGeocodeService, GeoapifyService>();
builder.Services.AddTransient<IWeatherProvider, WeatherApiProvider>();
builder.Services.AddTransient<IWeatherProvider, OpenWeatherMapProviderService>();
builder.Services.AddTransient<IWeatherProvider, OpenMeteoProvider>();
builder.Services.AddTransient<WeatherService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WeatherForecast API v1"));
}

app.MapControllers();

app.Run();
