using Microsoft.AspNetCore.Mvc;
using WeatherForecast.Application.Services;

namespace WeatherForecast.Application.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherController(WeatherService weatherService, ILogger<WeatherController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetWeather([FromQuery] string city, [FromQuery] string country, [FromQuery] DateTime date)
    {
        var today = DateTime.UtcNow.Date;
        if (date.Date > today.AddDays(6) || date.Date < today)
        {
            return BadRequest("Weather forecast is available for a maximum of 6 days ahead, and the date cannot be in the past.");
        }

        var result = await weatherService.GetWeatherAsync(city, country, date);
        
        if (result == null)
        {
            logger.LogError(
                "Failed to retrieve weather forecasts for {City}, {Country} on {Date}. All providers returned null or fetching location failed.",
                city, country, date);

            return StatusCode(500);
        }
        
        return Ok(result);
    }
}
