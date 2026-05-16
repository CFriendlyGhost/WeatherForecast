namespace WeatherForecast.Application.Exceptions;

public sealed class LocationNotFoundException(string city, string country)
    : Exception($"Unable to find coordinates for the specified location. Please check the city '{city}' and country '{country}'.")
{
    public string City { get; } = city;
    public string Country { get; } = country;
}

